using System.Diagnostics;
using System.Windows.Forms;
using System;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace YTDownloader
{
    public partial class Form1 : Form
    {
        private bool isDownloading = false;
        private string youtubeDlPath = "";
        //private bool enableDebug = false;
        private string downloaderExe = "youtube-dl-external.exe";
        //private int totalBytes = 0;
        //private int downloadedBytes = 0;

        public Form1()
        {
            InitializeComponent();
            youtubeDlPath = System.IO.Path.Combine(Application.StartupPath, downloaderExe);
            if (!System.IO.File.Exists(youtubeDlPath))
            {
                MessageBox.Show(downloaderExe + " not found. Please put it in the same directory as this program and try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }

            txtUrl.TextChanged += TxtUrl_TextChanged;
        }

        private void TxtUrl_TextChanged(object sender, EventArgs e)
        {
            string url = txtUrl.Text;
            string pattern = @"^(https?://)?(www\.)?(youtube\.com/watch\?v=)([a-zA-Z0-9_-]+).*";
            Match match = Regex.Match(url, pattern);
            if (match.Success)
            {
                string newUrl = match.Groups[3].Value + match.Groups[4].Value;
                txtUrl.Text = newUrl;
                txtUrl.SelectionStart = txtUrl.Text.Length;
            }
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            if (isDownloading)
            {
                return;
            }

            isDownloading = true;
            btnDownload.Enabled = false;
            txtUrl.Enabled = false;
            rdoMp3.Enabled = false;
            rdoVideo.Enabled = false;
            btnDownload.Text = "Downloading...";
            //progressBar1.Visible = true;

            string url = txtUrl.Text;
            string youtubeDlCommand = "";

            if (rdoVideo.Checked)
            {
                youtubeDlCommand = $"\"{youtubeDlPath}\" -o \"%USERPROFILE%\\Desktop\\%(title)s.%(ext)s\" \"{url}\"";
            }
            else if (rdoMp3.Checked)
            {
                youtubeDlCommand = $"\"{youtubeDlPath}\" -x --audio-format mp3 -o \"%USERPROFILE%\\Desktop\\%(title)s.%(ext)s\" \"{url}\"";
            }

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += Worker_DoWork;
            worker.ProgressChanged += Worker_ProgressChanged;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            worker.WorkerReportsProgress = true;
            worker.RunWorkerAsync(youtubeDlCommand);
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            string youtubeDlCommand = e.Argument as string;

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = $"/C \"{youtubeDlCommand}\"";
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            //startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            Console.WriteLine(startInfo.FileName + " " + startInfo.Arguments); // log command to console

            Process process = new Process();
            process.StartInfo = startInfo;
            process.OutputDataReceived += Process_OutputDataReceived;
            process.ErrorDataReceived += Process_ErrorDataReceived;

            process.Start();

            process.EnableRaisingEvents = true;
            process.OutputDataReceived += new DataReceivedEventHandler(Process_OutputDataReceived);

            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            process.WaitForExit();
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!String.IsNullOrEmpty(e.Data))
            {
                Console.WriteLine("Error: " + e.Data);
            }
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                if (e.Data.Contains("[download]"))
                {
                    int index = e.Data.IndexOf('%');
                    if (index >= 0)
                    {
                        string percentStr = e.Data.Substring(index - 4, 4);
                        if (double.TryParse(percentStr, out double percent))
                        {
                            if (sender != null) // add null check
                            {
                                BeginInvoke(new Action(() => {
                                    labelProgress.Text = percentStr + " %";
                                }));
                            }
                            Console.WriteLine(percentStr);
                        }
                    }
                }
                else
                {
                    Console.WriteLine(e.Data);
                }
            }
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            UpdateProgressLabel(e.ProgressPercentage);
        }

        private void UpdateProgressLabel(int percent)
        {
            string progressText = percent.ToString() + "%";
            Console.WriteLine(progressText);
            labelProgress.Text = progressText;
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            isDownloading = false;
            btnDownload.Enabled = true;
            txtUrl.Enabled = true;
            rdoMp3.Enabled = true;
            rdoVideo.Enabled = true;
            btnDownload.Text = "Download";
            //progressBar1.Visible = false;
        }
    }
}