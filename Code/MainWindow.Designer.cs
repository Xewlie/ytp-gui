namespace YTDownloader
{
    partial class MainWindow
    {
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtUrl;
        private System.Windows.Forms.RadioButton rdoVideo;
        private System.Windows.Forms.RadioButton rdoMp3;
        private System.Windows.Forms.Button btnDownload;
        private System.Windows.Forms.Label labelProgress;

        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }


        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            label1 = new Label();
            txtUrl = new TextBox();
            rdoVideo = new RadioButton();
            rdoMp3 = new RadioButton();
            btnDownload = new Button();
            labelProgress = new Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(26, 64);
            label1.Margin = new Padding(7, 0, 7, 0);
            label1.Name = "label1";
            label1.Size = new Size(60, 32);
            label1.TabIndex = 0;
            label1.Text = "URL:";
            // 
            // txtUrl
            // 
            txtUrl.Location = new Point(110, 55);
            txtUrl.Margin = new Padding(7, 9, 7, 9);
            txtUrl.Name = "txtUrl";
            txtUrl.Size = new Size(531, 39);
            txtUrl.TabIndex = 1;
            // 
            // rdoVideo
            // 
            rdoVideo.AutoSize = true;
            rdoVideo.Checked = true;
            rdoVideo.Location = new Point(33, 151);
            rdoVideo.Margin = new Padding(7, 9, 7, 9);
            rdoVideo.Name = "rdoVideo";
            rdoVideo.Size = new Size(107, 36);
            rdoVideo.TabIndex = 2;
            rdoVideo.TabStop = true;
            rdoVideo.Text = "Video";
            rdoVideo.UseVisualStyleBackColor = true;
            // 
            // rdoMp3
            // 
            rdoMp3.AutoSize = true;
            rdoMp3.Location = new Point(154, 151);
            rdoMp3.Margin = new Padding(7, 9, 7, 9);
            rdoMp3.Name = "rdoMp3";
            rdoMp3.Size = new Size(93, 36);
            rdoMp3.TabIndex = 3;
            rdoMp3.Text = "MP3";
            rdoMp3.UseVisualStyleBackColor = true;
            // 
            // btnDownload
            // 
            btnDownload.Location = new Point(33, 243);
            btnDownload.Margin = new Padding(7, 9, 7, 9);
            btnDownload.Name = "btnDownload";
            btnDownload.Size = new Size(611, 55);
            btnDownload.TabIndex = 4;
            btnDownload.Text = "Download";
            btnDownload.UseVisualStyleBackColor = true;
            btnDownload.Click += btnDownload_Click;
            // 
            // labelProgress
            // 
            labelProgress.Anchor = AnchorStyles.None;
            labelProgress.Font = new Font("Microsoft Sans Serif", 14F, FontStyle.Regular, GraphicsUnit.Point);
            labelProgress.Location = new Point(26, 329);
            labelProgress.Margin = new Padding(7, 0, 7, 0);
            labelProgress.Name = "labelProgress";
            labelProgress.Size = new Size(624, 98);
            labelProgress.TabIndex = 6;
            labelProgress.Text = "---";
            labelProgress.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // MainWindow
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(676, 425);
            Controls.Add(labelProgress);
            Controls.Add(btnDownload);
            Controls.Add(rdoMp3);
            Controls.Add(rdoVideo);
            Controls.Add(txtUrl);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(7, 9, 7, 9);
            MaximizeBox = false;
            Name = "MainWindow";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "YouTube Downloader";
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
