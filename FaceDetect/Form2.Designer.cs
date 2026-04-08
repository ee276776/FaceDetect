namespace FaceDetect
{
    partial class Form2
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            // 釋放所有 Feature Mat
            foreach (var f in _allFaces) f.Feature?.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.btnUploadMulti = new System.Windows.Forms.Button();
            this.btnGroupAndSave = new System.Windows.Forms.Button();
            this.flpPreview = new System.Windows.Forms.FlowLayoutPanel();
            this.lblStatus = new System.Windows.Forms.Label();
            this.trackBarThreshold = new System.Windows.Forms.TrackBar();
            this.lblThreshold = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarThreshold)).BeginInit();
            this.SuspendLayout();

            // btnUploadMulti
            this.btnUploadMulti.Location = new System.Drawing.Point(12, 12);
            this.btnUploadMulti.Size = new System.Drawing.Size(150, 36);
            this.btnUploadMulti.Text = "📁 上傳多張圖片";
            this.btnUploadMulti.Click += new System.EventHandler(this.btnUploadMulti_Click);

            // trackBarThreshold  （0.50 ~ 0.99，預設 0.7）
            this.trackBarThreshold.Location = new System.Drawing.Point(175, 10);
            this.trackBarThreshold.Size = new System.Drawing.Size(220, 45);
            this.trackBarThreshold.Minimum = 50;
            this.trackBarThreshold.Maximum = 99;
            this.trackBarThreshold.Value = 70;
            this.trackBarThreshold.TickFrequency = 5;
            this.trackBarThreshold.Scroll += new System.EventHandler(this.trackBarThreshold_Scroll);

            // lblThreshold
            this.lblThreshold.Location = new System.Drawing.Point(400, 18);
            this.lblThreshold.Size = new System.Drawing.Size(260, 20);
            this.lblThreshold.Text = "相似度閾值：0.70（餘弦距離 ≤ 0.30）";

            // btnGroupAndSave
            this.btnGroupAndSave.Location = new System.Drawing.Point(670, 12);
            this.btnGroupAndSave.Size = new System.Drawing.Size(150, 36);
            this.btnGroupAndSave.Text = "🗂 分群並儲存";
            this.btnGroupAndSave.Click += new System.EventHandler(this.btnGroupAndSave_Click);

            // lblStatus
            this.lblStatus.Location = new System.Drawing.Point(12, 56);
            this.lblStatus.Size = new System.Drawing.Size(820, 20);
            this.lblStatus.Text = "請先上傳多張圖片...";

            // flpPreview
            this.flpPreview.Location = new System.Drawing.Point(12, 82);
            this.flpPreview.Size = new System.Drawing.Size(960, 580);
            this.flpPreview.AutoScroll = true;
            this.flpPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flpPreview.BackColor = System.Drawing.Color.WhiteSmoke;

            // Form
            this.ClientSize = new System.Drawing.Size(990, 680);
            this.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.btnUploadMulti, this.trackBarThreshold, this.lblThreshold,
                this.btnGroupAndSave, this.lblStatus, this.flpPreview
            });
            this.Text = "多圖人臉分群（FaceRecognizerSF）";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;

            ((System.ComponentModel.ISupportInitialize)(this.trackBarThreshold)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Button btnUploadMulti;
        private System.Windows.Forms.Button btnGroupAndSave;
        private System.Windows.Forms.FlowLayoutPanel flpPreview;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.TrackBar trackBarThreshold;
        private System.Windows.Forms.Label lblThreshold;
    }
}