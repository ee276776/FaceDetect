namespace FaceDetect
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnUpload = new Button();
            flpFaces = new FlowLayoutPanel();
            pbPreview = new PictureBox();
            btnSaveAll = new Button();
            btnExtractFeatures = new Button();
            btnOpenGroupForm = new Button();
            ((System.ComponentModel.ISupportInitialize)pbPreview).BeginInit();
            SuspendLayout();
            // 
            // btnUpload
            // 
            btnUpload.Location = new Point(57, 103);
            btnUpload.Name = "btnUpload";
            btnUpload.Size = new Size(134, 23);
            btnUpload.TabIndex = 0;
            btnUpload.Text = "檔案上傳";
            btnUpload.UseVisualStyleBackColor = true;
            btnUpload.Click += btnUpload_Click;
            // 
            // flpFaces
            // 
            flpFaces.Location = new Point(383, 59);
            flpFaces.Name = "flpFaces";
            flpFaces.Size = new Size(373, 322);
            flpFaces.TabIndex = 1;
            // 
            // pbPreview
            // 
            pbPreview.Location = new Point(57, 132);
            pbPreview.Name = "pbPreview";
            pbPreview.Size = new Size(203, 160);
            pbPreview.TabIndex = 2;
            pbPreview.TabStop = false;
            // 
            // btnSaveAll
            // 
            btnSaveAll.Location = new Point(383, 415);
            btnSaveAll.Name = "btnSaveAll";
            btnSaveAll.Size = new Size(75, 23);
            btnSaveAll.TabIndex = 3;
            btnSaveAll.Text = "儲存全部";
            btnSaveAll.UseVisualStyleBackColor = true;
            btnSaveAll.Click += btnSaveAll_Click;
            // 
            // btnExtractFeatures
            // 
            btnExtractFeatures.Location = new Point(485, 415);
            btnExtractFeatures.Name = "btnExtractFeatures";
            btnExtractFeatures.Size = new Size(75, 23);
            btnExtractFeatures.TabIndex = 4;
            btnExtractFeatures.Text = "五官";
            btnExtractFeatures.UseVisualStyleBackColor = true;
            btnExtractFeatures.Click += btnExtractFeatures_Click;
            // 
            // btnOpenGroupForm
            // 
            btnOpenGroupForm.Location = new Point(98, 325);
            btnOpenGroupForm.Name = "btnOpenGroupForm";
            btnOpenGroupForm.Size = new Size(106, 23);
            btnOpenGroupForm.TabIndex = 5;
            btnOpenGroupForm.Text = "多檔上傳檢測";
            btnOpenGroupForm.UseVisualStyleBackColor = true;
            btnOpenGroupForm.Click += btnOpenGroupForm_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnOpenGroupForm);
            Controls.Add(btnExtractFeatures);
            Controls.Add(btnSaveAll);
            Controls.Add(pbPreview);
            Controls.Add(flpFaces);
            Controls.Add(btnUpload);
            Name = "Form1";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)pbPreview).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Button btnUpload;
        private FlowLayoutPanel flpFaces;
        private PictureBox pbPreview;
        private Button btnSaveAll;
        private Button btnExtractFeatures;
        private Button btnOpenGroupForm;
    }
}
