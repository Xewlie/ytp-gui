namespace YTDownloader
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.label1 = new System.Windows.Forms.Label();
            this.txtUrl = new System.Windows.Forms.TextBox();
            this.rdoVideo = new System.Windows.Forms.RadioButton();
            this.rdoMp3 = new System.Windows.Forms.RadioButton();
            this.btnDownload = new System.Windows.Forms.Button();
            this.labelProgress = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(22, 48);
            this.label1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 25);
            this.label1.TabIndex = 0;
            this.label1.Text = "URL:";
            // 
            // txtUrl
            // 
            this.txtUrl.Location = new System.Drawing.Point(92, 42);
            this.txtUrl.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.txtUrl.Name = "txtUrl";
            this.txtUrl.Size = new System.Drawing.Size(450, 29);
            this.txtUrl.TabIndex = 1;
            this.txtUrl.Text = "";
            // 
            // rdoVideo
            // 
            this.rdoVideo.AutoSize = true;
            this.rdoVideo.Checked = true;
            this.rdoVideo.Location = new System.Drawing.Point(92, 114);
            this.rdoVideo.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.rdoVideo.Name = "rdoVideo";
            this.rdoVideo.Size = new System.Drawing.Size(88, 29);
            this.rdoVideo.TabIndex = 2;
            this.rdoVideo.TabStop = true;
            this.rdoVideo.Text = "Video";
            this.rdoVideo.UseVisualStyleBackColor = true;
            // 
            // rdoMp3
            // 
            this.rdoMp3.AutoSize = true;
            this.rdoMp3.Location = new System.Drawing.Point(198, 114);
            this.rdoMp3.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.rdoMp3.Name = "rdoMp3";
            this.rdoMp3.Size = new System.Drawing.Size(78, 29);
            this.rdoMp3.TabIndex = 3;
            this.rdoMp3.Text = "MP3";
            this.rdoMp3.UseVisualStyleBackColor = true;
            // 
            // btnDownload
            // 
            this.btnDownload.Location = new System.Drawing.Point(28, 183);
            this.btnDownload.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.btnDownload.Name = "btnDownload";
            this.btnDownload.Size = new System.Drawing.Size(517, 42);
            this.btnDownload.TabIndex = 4;
            this.btnDownload.Text = "Download";
            this.btnDownload.UseVisualStyleBackColor = true;
            this.btnDownload.Click += new System.EventHandler(this.btnDownload_Click);
            // 
            // labelProgress
            // 
            this.labelProgress.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.labelProgress.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelProgress.Location = new System.Drawing.Point(22, 247);
            this.labelProgress.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.labelProgress.Name = "labelProgress";
            this.labelProgress.Size = new System.Drawing.Size(528, 74);
            this.labelProgress.TabIndex = 6;
            this.labelProgress.Text = "---";
            this.labelProgress.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(572, 318);
            this.Controls.Add(this.labelProgress);
            this.Controls.Add(this.btnDownload);
            this.Controls.Add(this.rdoMp3);
            this.Controls.Add(this.rdoVideo);
            this.Controls.Add(this.txtUrl);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "YouTube Downloader";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

    }
}
