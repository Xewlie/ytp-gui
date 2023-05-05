using System.Diagnostics;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace YTDownloader;

public partial class MainWindow : Form
{
    private bool isDownloading;

    public MainWindow(DependanciesDownloadHelper dependanciesDownloadHelper, HttpClient httpClient)
    {
        InitializeComponent(); // init form objects
        txtUrl.TextChanged += TxtUrl_TextChanged; // set event for download button text changing
        dependanciesDownloadHelper.DownloadRequiredFilesAsync(httpClient, btnDownload).ConfigureAwait(false); // check if you need ytp/ffmpeg
    }

    private void TxtUrl_TextChanged(object? sender, EventArgs e)
    {
        var url = txtUrl.Text;
        var match = MyRegex().Match(url);
        if (!match.Success) return;
        var newUrl = match.Groups[3].Value + match.Groups[4].Value;
        txtUrl.Text = newUrl;
        txtUrl.SelectionStart = txtUrl.Text.Length;
    }

    private void btnDownload_Click(object sender, EventArgs e)
    {
        if (isDownloading || txtUrl.Text.Length < 1)
        {
            return;
        }

        SaveFileDialog saveFileDialog = new SaveFileDialog();
        saveFileDialog.FileName = "output";
        saveFileDialog.Filter = rdoMp3.Checked ? "MP3 Audio File (*.mp3)|*.mp3" : "WebM Video File (*.webm)|*.webm";

        if (saveFileDialog.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        string outputPath = saveFileDialog.FileName;

        isDownloading = true;
        btnDownload.Enabled = false;
        txtUrl.Enabled = false;
        rdoMp3.Enabled = false;
        rdoVideo.Enabled = false;
        btnDownload.Text = $@"Downloading...";

        var url = txtUrl.Text;
        var youtubeDlCommand = "";

        if (rdoVideo.Checked)
        {
            youtubeDlCommand = $"\"{Constants.YoutubeDLPath()}\" -o \"{outputPath}\" \"{url}\"";
        }
        else if (rdoMp3.Checked)
        {
            youtubeDlCommand = $"\"{Constants.YoutubeDLPath()}\" -x --audio-format mp3 -o \"{outputPath}\" \"{url}\"";
        }

        var worker = new BackgroundWorker();
        worker.DoWork += Worker_DoWork;
        worker.ProgressChanged += Worker_ProgressChanged;
        worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
        worker.WorkerReportsProgress = true;
        worker.RunWorkerAsync(youtubeDlCommand);
    }

    private void Worker_DoWork(object? sender, DoWorkEventArgs e)
    {
        var youtubeDlCommand = e.Argument as string;

        var startInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/C \"{youtubeDlCommand}\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        Console.WriteLine(startInfo.FileName + @" " + startInfo.Arguments); // log command to console

        var process = new Process();
        process.StartInfo = startInfo;
        process.OutputDataReceived += Process_OutputDataReceived;
        process.ErrorDataReceived += Process_ErrorDataReceived;

        process.Start();

        process.EnableRaisingEvents = true;
        process.OutputDataReceived += Process_OutputDataReceived;

        process.BeginErrorReadLine();
        process.BeginOutputReadLine();

        process.WaitForExit();
    }

    private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.Data))
        {
            Console.WriteLine(@"Error: " + e.Data);
        }
    }

    private void Process_OutputDataReceived(object? sender, DataReceivedEventArgs e)
    {
        if (e.Data == null) return;
        if (e.Data.Contains("[download]"))
        {
            var index = e.Data.IndexOf('%');
            if (index < 0) return;
            var percentStr = e.Data.Substring(index - 4, 4);
            if (!double.TryParse(percentStr, out _)) return;
            if (sender != null) // add null check
            {
                BeginInvoke(() =>
                {
                    labelProgress.Text = percentStr + @" %";
                });
            }
            Console.WriteLine(percentStr);
        }
        else
        {
            Console.WriteLine(e.Data);
        }
    }

    private void Worker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
    {
        UpdateProgressLabel(e.ProgressPercentage);
    }

    private void UpdateProgressLabel(int percent)
    {
        var progressText = percent + "%";
        Console.WriteLine(progressText);
        labelProgress.Text = progressText;
    }

    private void Worker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
    {
        isDownloading = false;
        btnDownload.Enabled = true;
        txtUrl.Enabled = true;
        rdoMp3.Enabled = true;
        rdoVideo.Enabled = true;
        btnDownload.Text = @"Download";
    }

    [GeneratedRegex("^(https?://)?(www\\.)?(youtube\\.com/watch\\?v=)([a-zA-Z0-9_-]+).*")]
    private static partial Regex MyRegex();
}