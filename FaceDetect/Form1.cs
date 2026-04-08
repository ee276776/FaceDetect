using OpenCvSharp;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Windows;
using Mediapipe.Net.Core;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Rect = OpenCvSharp.Rect;
namespace FaceDetect
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            flpFaces.AutoScroll = true;
        }
        private List<Bitmap> _extractedFaces = new List<Bitmap>();
        private string _modelPath = "face_detection_yunet_2023mar.onnx";


        private void btnUpload_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog { Filter = "Images|*.jpg;*.png;*.jpeg" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    _extractedFaces.Clear();
                    flpFaces.Controls.Clear();

                    using var img = Cv2.ImRead(ofd.FileName);

                    // 1. 初始化 YuNet 偵測器
                    var detector = FaceDetectorYN.Create(_modelPath, "", img.Size());

                    // 2. 執行偵測
                    Mat faces = new Mat();
                    detector.Detect(img, faces);

                    // 3. 處理偵測結果
                    for (int i = 0; i < faces.Rows; i++)
                    {
                        // 取得座標 (x, y, width, height)
                        int x = (int)faces.At<float>(i, 0);
                        int y = (int)faces.At<float>(i, 1);
                        int w = (int)faces.At<float>(i, 2);
                        int h = (int)faces.At<float>(i, 3);

                        // 確保裁切範圍不超出圖片
                        Rect rect = new Rect(Math.Max(0, x), Math.Max(0, y),
                                             Math.Min(w, img.Cols - x), Math.Min(h, img.Rows - y));

                        using var faceMat = new Mat(img, rect);
                        Bitmap faceBmp = faceMat.ToBitmap();
                        _extractedFaces.Add(faceBmp);

                        // 顯示在介面上
                        var pb = new PictureBox { Image = faceBmp, SizeMode = PictureBoxSizeMode.Zoom, Width = 120, Height = 120 };
                        flpFaces.Controls.Add(pb);
                    }

                    pbPreview.Image = img.ToBitmap();
                    System.Windows.Forms.MessageBox.Show($"成功偵測到 {faces.Rows} 張人臉！");
                }
            }
        }


        private void btnSaveAll_Click(object sender, EventArgs e)
        {
            if (pbPreview.Image == null) return;

            // 1. 建立當前時間的子資料夾路徑
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string baseDir = Path.Combine(System.Windows.Forms.Application.StartupPath, "ExtractedFaces", timestamp);
            Directory.CreateDirectory(baseDir);

            using var img = OpenCvSharp.Extensions.BitmapConverter.ToMat((Bitmap)pbPreview.Image);
            var detector = FaceDetectorYN.Create(_modelPath, "", img.Size());
            Mat faces = new Mat();
            detector.Detect(img, faces);

            // 2. 遍歷偵測到的每一張臉
            for (int i = 0; i < faces.Rows; i++)
            {
                // 定義人頭編號 (A, B, C...)
                string personId = ((char)('A' + i)).ToString();

                // --- A. 儲存整張人臉 ---
                Rect faceRect = GetSafeRect(img, (int)faces.At<float>(i, 0), (int)faces.At<float>(i, 1), (int)faces.At<float>(i, 2), (int)faces.At<float>(i, 3));
                SavePart(img, faceRect, baseDir, $"{personId}_face.jpg");

                // --- B. 儲存五官 ---
                // 右眼 (人臉視角)
                SavePartByCenter(img, faces.At<float>(i, 4), faces.At<float>(i, 5), 50, baseDir, $"{personId}_reye.jpg");
                // 左眼
                SavePartByCenter(img, faces.At<float>(i, 6), faces.At<float>(i, 7), 50, baseDir, $"{personId}_leye.jpg");
                // 鼻子
                SavePartByCenter(img, faces.At<float>(i, 8), faces.At<float>(i, 9), 60, baseDir, $"{personId}_nose.jpg");
                // 嘴巴 (取兩嘴角中點)
                float mx = (faces.At<float>(i, 10) + faces.At<float>(i, 12)) / 2;
                float my = (faces.At<float>(i, 11) + faces.At<float>(i, 13)) / 2;



                //SavePartByCenter(img, mx, my, 80, baseDir, $"{personId}_mouth.jpg");

                // 這裡加上一個偏移值，例如 + (臉部高度 * 0.05)
                float faceHeight = faces.At<float>(i, 3);
                float mouthOffset = faceHeight * 0.05f;

                // 縮小 size 到 60 或 70，並套用位移
                SavePartByCenter(img, mx, my + mouthOffset, 70, baseDir, $"{personId}_mouth.jpg");
            }

            System.Windows.Forms.MessageBox.Show($"所有照片已分類存儲至：\n{baseDir}");
        }

        // 輔助：根據中心點存檔
        private void SavePartByCenter(Mat src, float cx, float cy, int size, string dir, string fileName)
        {
            int half = size / 2;
            Rect rect = GetSafeRect(src, (int)cx - half, (int)cy - half, size, size);
            SavePart(src, rect, dir, fileName);
        }

        // 輔助：安全擷取並存檔
        private void SavePart(Mat src, Rect rect, string dir, string fileName)
        {
            if (rect.Width <= 0 || rect.Height <= 0) return;
            using var part = new Mat(src, rect);
            part.SaveImage(Path.Combine(dir, fileName));
        }

        // 輔助：防止座標越界
        private Rect GetSafeRect(Mat src, int x, int y, int w, int h)
        {
            int resX = Math.Clamp(x, 0, src.Cols - 1);
            int resY = Math.Clamp(y, 0, src.Rows - 1);
            int resW = Math.Min(w, src.Cols - resX);
            int resH = Math.Min(h, src.Rows - resY);
            return new Rect(resX, resY, resW, resH);
        }
        private void btnOpenGroupForm_Click(object sender, EventArgs e)
        {
            var form2 = new Form2();
            form2.Show();
        }

        private void btnExtractFeatures_Click(object sender, EventArgs e)
        {
            if (pbPreview.Image == null) return;

            // 清空上次的臉部與五官紀錄
            _extractedFaces.Clear();
            flpFaces.Controls.Clear();

            using var img = BitmapConverter.ToMat((Bitmap)pbPreview.Image);

            //using var img = OpenCvSharp.Extensions.BitmapConverter.ToMat((Bitmap)pbPreview.Image); // 將當前預覽圖轉回 Mat
            var detector = FaceDetectorYN.Create(_modelPath, "", img.Size());
            Mat faces = new Mat();
            detector.Detect(img, faces);

            flpFaces.Controls.Clear(); // 清空預覽區

            for (int i = 0; i < faces.Rows; i++)
            {
                // 定義要擷取的部位與其對應的特徵點索引
                // 索引：眼睛(4,5 & 6,7), 鼻子(8,9), 嘴巴(10,11 & 12,13)

                // 1. 擷取鼻子 (以鼻尖點為中心，切一個固定大小)
                ExtractPart(img, faces.At<float>(i, 8), faces.At<float>(i, 9), 60, "Nose");

                // 2. 擷取眼睛 (分別切左右眼)
                ExtractPart(img, faces.At<float>(i, 4), faces.At<float>(i, 5), 50, "RightEye");
                ExtractPart(img, faces.At<float>(i, 6), faces.At<float>(i, 7), 50, "LeftEye");

                // 3. 擷取嘴巴 (取左右嘴角的中點來切)
                float mouthX = (faces.At<float>(i, 10) + faces.At<float>(i, 12)) / 2;
                float mouthY = (faces.At<float>(i, 11) + faces.At<float>(i, 13)) / 2;
                ExtractPart(img, mouthX, mouthY, 80, "Mouth");
            }
        }

        // 輔助方法：根據中心點座標裁切指定大小
        private void ExtractPart(Mat src, float centerX, float centerY, int size, string label)
        {
            int half = size / 2;
            Rect rect = new Rect((int)centerX - half, (int)centerY - half, size, size);

            // 檢查邊界
            rect.X = Math.Clamp(rect.X, 0, src.Cols - 1);
            rect.Y = Math.Clamp(rect.Y, 0, src.Rows - 1);
            rect.Width = Math.Min(rect.Width, src.Cols - rect.X);
            rect.Height = Math.Min(rect.Height, src.Rows - rect.Y);

            if (rect.Width > 0 && rect.Height > 0)
            {
                using var partMat = new Mat(src, rect);
                Bitmap bmp = partMat.ToBitmap();

                // 【關鍵：同步存入清單】這樣儲存按鈕才抓得到
                _extractedFaces.Add(bmp);

                // 顯示到介面
                var pb = new PictureBox
                {
                    Image = bmp,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Width = 80,
                    Height = 80
                };
                flpFaces.Controls.Add(pb);
            }
        }

       
    }
}
