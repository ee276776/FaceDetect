using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace FaceDetect
{
    public partial class Form2 : Form
    {
        private readonly string _detectorModel = "face_detection_yunet_2023mar.onnx";
        private readonly string _recognizerModel = "face_recognition_sface_2021dec_int8.onnx";

        private class FaceEntry
        {
            public Bitmap Bmp { get; set; }
            public Mat Feature { get; set; }  // 1×512 float32
            public string SourceFile { get; set; }
            public int GroupId { get; set; } = -1;
        }

        private readonly List<FaceEntry> _allFaces = new();
        private FaceRecognizerSF _recognizer = null;

        public Form2()
        {
            InitializeComponent();
            lblThreshold.Text = $"相似度閾值：{trackBarThreshold.Value / 100.0:F2}";
        }

        // ══════════════════════════════════���═════════════════════════
        //  步驟 1：上傳多張圖 → 偵測 → AlignCrop → Feature 擷取
        // ════════════════════════════════════════════════════════════
        private void btnUploadMulti_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Filter = "Images|*.jpg;*.png;*.jpeg",
                Multiselect = true,
                Title = "選取多張圖片（可複選）"
            };
            if (ofd.ShowDialog() != DialogResult.OK) return;

            // 初始化 FaceRecognizerSF（只建一次）
            if (_recognizer == null)
            {
                if (!File.Exists(_recognizerModel))
                {
                    MessageBox.Show(
                        $"找不到辨識模型：{_recognizerModel}\n" +
                        "請從下列連結下載後放入執行目錄：\n" +
                        "https://github.com/opencv/opencv_zoo/tree/main/models/face_recognition_sface");
                    return;
                }
                _recognizer = new FaceRecognizerSF(
                    _recognizerModel, "",
                    Emgu.CV.Dnn.Backend.Default,
                    Emgu.CV.Dnn.Target.Cpu);
            }

            _allFaces.Clear();
            flpPreview.Controls.Clear();
            lblStatus.Text = "偵測中，請稍候...";
            Application.DoEvents();

            foreach (string filePath in ofd.FileNames)
            {
                using var img = CvInvoke.Imread(filePath, ImreadModes.ColorBgr);
                if (img.IsEmpty) continue;

                // ── 建立偵測器（每張圖 size 可能不同）──
                using var detector = new FaceDetectorYN(
                    _detectorModel, "",
                    img.Size,
                    scoreThreshold: 0.9f,
                    nmsThreshold: 0.3f,
                    topK: 5000);

                using var facesMat = new Mat();
                detector.Detect(img, facesMat);

                for (int i = 0; i < facesMat.Rows; i++)
                {
                    // ── A. 取出該行偵測結果（1×15）供 AlignCrop 使用 ──
                    using var faceRow = facesMat.Row(i);
                    using var alignedFace = new Mat();
                    _recognizer.AlignCrop(img, faceRow, alignedFace);

                    // ★ 檢查 1：alignedFace 的尺寸和型別
                    System.Diagnostics.Debug.WriteLine(
                        $"alignedFace: {alignedFace.Width}x{alignedFace.Height} " +
                        $"channels={alignedFace.NumberOfChannels} " +
                        $"depth={alignedFace.Depth} empty={alignedFace.IsEmpty}");

                    if (alignedFace.IsEmpty) { Debug.WriteLine("AlignCrop 失敗！"); continue; }

                    var feature = new Mat();
                    _recognizer.Feature(alignedFace, feature);

                    System.Diagnostics.Debug.WriteLine(
                        $"feature: {feature.Width}x{feature.Height} " +
                        $"Depth={feature.Depth} " +        // 確認是 Cv32F 還是其他
                        $"Channels={feature.NumberOfChannels} " +
                        $"Total={feature.Total}");










                    // ★ 檢查 2：feature 的狀態
                    System.Diagnostics.Debug.WriteLine(
                        $"feature: {feature.Width}x{feature.Height} empty={feature.IsEmpty} total={feature.Total}");

                    if (feature.IsEmpty || feature.Total == 0)
                    {
                        System.Diagnostics.Debug.WriteLine("Feature 擷取失敗！");
                        continue;
                    }
                    CvInvoke.Normalize(feature, feature, 1.0, 0.0, NormType.L2);
                    // ── C. 轉縮圖供 UI 顯示 ──
                    Bitmap bmp = alignedFace.ToBitmap();

                    _allFaces.Add(new FaceEntry
                    {
                        Bmp = bmp,
                        Feature = feature.Clone(),  // ★ 加 Clone()，確保每張臉有獨立的特徵向量
                        SourceFile = Path.GetFileName(filePath)
                    });

                    feature.Dispose(); // 原本的可以釋放

                    AddPreviewBox(bmp, Path.GetFileName(filePath));
                }
            }

            lblStatus.Text = $"共偵測到 {_allFaces.Count} 張人臉，請調整閾值後按「分群並儲存」。";
        }

        private float[] MatToFloatArray(Mat m)
        {
            int len = (int)m.Total.ToInt32();
            float[] arr = new float[len];
            System.Runtime.InteropServices.Marshal.Copy(m.DataPointer, arr, 0, len);

            System.Diagnostics.Debug.WriteLine(
                $"前5值: {string.Join(", ", arr.Take(5).Select(v => v.ToString("F6")))}");
            System.Diagnostics.Debug.WriteLine(
                $"全零: {arr.All(v => v == 0f)}, 全一: {arr.All(v => v == 1f)}");

            return arr;
        }

        private double ComputeCosineDist(Mat feat1, Mat feat2)
        {
            float[] a = MatToFloatArray(feat1);
            float[] b = MatToFloatArray(feat2);

            System.Diagnostics.Debug.WriteLine(
                $"a前5: {string.Join(", ", a.Take(5).Select(v => v.ToString("F6")))}");
            System.Diagnostics.Debug.WriteLine(
                $"b前5: {string.Join(", ", b.Take(5).Select(v => v.ToString("F6")))}");

            double dot = 0, normA = 0, normB = 0;
            for (int k = 0; k < a.Length; k++)
            {
                dot += a[k] * b[k];
                normA += a[k] * a[k];
                normB += b[k] * b[k];
            }

            System.Diagnostics.Debug.WriteLine($"dot={dot:F6} normA={normA:F6} normB={normB:F6}");

            if (normA == 0 || normB == 0) return 1.0;
            return 1.0 - dot / (Math.Sqrt(normA) * Math.Sqrt(normB));
        }
      


        // ════════════════════════════════════════════════════════════
        //  步驟 2：分群（Greedy，FaceRecognizerSF.Match 餘弦距離）
        // ════════════════════════════════════════════════════════════
        private void btnGroupAndSave_Click(object sender, EventArgs e)
        {
            if (_allFaces.Count == 0) { MessageBox.Show("請先上傳圖片！"); return; }

            double cosDistThresh = 1.0 - trackBarThreshold.Value / 100.0;
            int n = _allFaces.Count;

            // ── 步驟1：建立完整距離矩陣 ──
            double[,] distMatrix = new double[n, n];
            for (int i = 0; i < n; i++)
                for (int j = i + 1; j < n; j++)
                {
                    //double d = _recognizer.Match(
                    //    _allFaces[i].Feature,
                    //    _allFaces[j].Feature,
                    //    FaceRecognizerSF.DisType.Cosine);
                    double d = ComputeCosineDist(_allFaces[i].Feature, _allFaces[j].Feature);
                    distMatrix[i, j] = d;
                    distMatrix[j, i] = d;
                }

            // 在 distMatrix 建立完之後，加這段
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < n; i++)
                for (int j = i + 1; j < n; j++)
                    sb.AppendLine($"臉{i} ↔ 臉{j} = {distMatrix[i, j]:F4}");

            MessageBox.Show(sb.ToString(), "距離矩陣 Debug");


            // ── 步驟2：Union-Find 分群（任意兩張臉距離 ≤ 閾值就合併）──
            int[] parent = Enumerable.Range(0, n).ToArray();

            int Find(int x) => parent[x] == x ? x : parent[x] = Find(parent[x]);
            void Union(int a, int b) => parent[Find(a)] = Find(b);

            for (int i = 0; i < n; i++)
                for (int j = i + 1; j < n; j++)
                    if (distMatrix[i, j] <= cosDistThresh)
                        Union(i, j);

            // ── 步驟3：把 root ID 轉成連續群組編號 ──
            var rootToGroup = new Dictionary<int, int>();
            int groupCount = 0;
            for (int i = 0; i < n; i++)
            {
                int root = Find(i);
                if (!rootToGroup.ContainsKey(root))
                    rootToGroup[root] = groupCount++;
                _allFaces[i].GroupId = rootToGroup[root];
            }

            // ── 儲存 ──
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string baseDir = Path.Combine(Application.StartupPath, "GroupedFaces", timestamp);
            Directory.CreateDirectory(baseDir);

            var groupCounter = new Dictionary<int, int>();
            foreach (var face in _allFaces.OrderBy(f => f.GroupId))
            {
                int gid = face.GroupId;
                groupCounter.TryAdd(gid, 0);
                groupCounter[gid]++;

                string groupName = GroupIdToName(gid);
                string subDir = Path.Combine(baseDir, groupName);
                Directory.CreateDirectory(subDir);

                face.Bmp.Save(
                    Path.Combine(subDir, $"{groupName}_{groupCounter[gid]}.jpg"),
                    System.Drawing.Imaging.ImageFormat.Jpeg);
            }

            ShowGroupResult(groupCount);
            MessageBox.Show(
                $"✅ 分群完成！共 {groupCount} 個人，{n} 張臉\n" +
                $"餘弦距離閾值：≤ {cosDistThresh:F2}\n存儲路徑：{baseDir}");
        }

        // ════════════════════════════════════════════════════════════
        //  UI 輔助
        // ════════════════════════════════════════════════════════════
        private void AddPreviewBox(Bitmap bmp, string tooltip)
        {
            var pb = new PictureBox
            {
                Image = bmp,
                SizeMode = PictureBoxSizeMode.Zoom,
                Width = 90,
                Height = 90,
                Margin = new Padding(3)
            };
            new ToolTip().SetToolTip(pb, tooltip);
            flpPreview.Controls.Add(pb);
        }

        private void ShowGroupResult(int groupCount)
        {
            flpPreview.Controls.Clear();
            Color[] palette = {
                Color.Crimson,   Color.RoyalBlue, Color.SeaGreen,  Color.DarkOrange,
                Color.Purple,    Color.Teal,      Color.Magenta,   Color.SaddleBrown,
                Color.DeepPink,  Color.DarkCyan
            };

            foreach (var face in _allFaces.OrderBy(f => f.GroupId))
            {
                Color border = palette[face.GroupId % palette.Length];
                var panel = new Panel
                {
                    Width = 96,
                    Height = 108,
                    BackColor = border,
                    Margin = new Padding(3)
                };
                var pb = new PictureBox
                {
                    Image = face.Bmp,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Bounds = new Rectangle(3, 3, 90, 90)
                };
                var lbl = new Label
                {
                    Text = GroupIdToName(face.GroupId),
                    Bounds = new Rectangle(3, 93, 90, 14),
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = Color.White,
                    BackColor = border,
                    Font = new Font("Arial", 8, FontStyle.Bold)
                };
                panel.Controls.Add(pb);
                panel.Controls.Add(lbl);
                flpPreview.Controls.Add(panel);
            }

            lblStatus.Text =
                $"分群完成：{_allFaces.Count} 張臉 → {groupCount} 個人（" +
                $"餘弦距離 ≤ {1.0 - trackBarThreshold.Value / 100.0:F2}）";
        }

        private static string GroupIdToName(int id)
        {
            string name = "";
            do { name = (char)('A' + id % 26) + name; id = id / 26 - 1; } while (id >= 0);
            return name;
        }

        private void trackBarThreshold_Scroll(object sender, EventArgs e)
        {
            double dist = 1.0 - trackBarThreshold.Value / 100.0;
            lblThreshold.Text = $"相似度閾值：{trackBarThreshold.Value / 100.0:F2}（餘弦距離 ≤ {dist:F2}）";
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _recognizer?.Dispose();
            foreach (var f in _allFaces) f.Feature?.Dispose();
            base.OnFormClosed(e);
        }
    }
}