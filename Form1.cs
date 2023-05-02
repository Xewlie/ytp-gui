using System.Diagnostics;
using System.ComponentModel;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using SevenZip;

namespace YTDownloader;

public partial class Form1 : Form
{
    private bool isDownloading;
    private readonly HttpClient httpClient;

    private const string youtubeDLExe = "yt-dlp.exe";
    private const string ffmpegExe = "ffmpeg.exe";

    private const string ffmpegUrl = "https://files.hyperxewl.xyz/ffmpeg-6.0-full_build.7z";
    private const string ytp = "https://files.hyperxewl.xyz/yt-dlp.exe";

    public Form1()
    {
        InitializeComponent(); // init form objects
        txtUrl.TextChanged += TxtUrl_TextChanged; // set event for download button text changing
        httpClient = new HttpClient();

        DownloadRequiredFilesAsync().ConfigureAwait(false); // check if you need ytp/ffmpeg
    }

    private string YoutubeDLPath()
    {
        return Path.Combine(Application.StartupPath, youtubeDLExe);
    }

    private async Task DownloadRequiredFilesAsync()
    {
        if (!FileExistsInPath(youtubeDLExe))
        {
            await DownloadYTDLAsync();
        }

        if (!FileExistsInPath(ffmpegExe))
        {
            await DownloadAndExtractFFMPEGAsync();
        }
    }

    private bool FileExistsInPath(string filename)
    {
        if (File.Exists(Path.Combine(Application.StartupPath, filename)))
        {
            return true;
        }

        var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        var folders = path.Split(Path.PathSeparator);

        return folders.Any(folder => File.Exists(Path.Combine(folder, filename)));
    }

    private async Task DownloadAndExtractFFMPEGAsync()
    {
        btnDownload.Enabled = false;
        btnDownload.Text = @"Downloading " + Path.GetFileName(ffmpegUrl);

        // Check if 7ZIP is installed and set the library up
        // If 7zip isn't installed, it looks for 7zip in current directory
        // If neither is found, it recommends to download ffmpeg manually.
        // This means the user didn't have ffmpeg in PATH, in folder, and also didn't have 7zip installed
        // and they also didn't have 7zip from the folder either which is distributed from the release
        // big fail!
        Set7ZipLibraryPath();

        string archivePath = Path.Combine(Path.GetTempPath(), Path.GetFileName(ffmpegUrl));

        HttpResponseMessage response = await httpClient.GetAsync(ffmpegUrl);

        try
        {
            if (response.IsSuccessStatusCode)
            {
                await using Stream contentStream = await response.Content.ReadAsStreamAsync(), fileStream = new FileStream(archivePath, FileMode.Create, FileAccess.Write, FileShare.None);
                await contentStream.CopyToAsync(fileStream);
            }
            else
            {
                throw new HttpRequestException("Couldn't download " + Path.GetFileName(ffmpegUrl));
            }

            btnDownload.Text = @"Extracting " + ffmpegExe;

            using SevenZipExtractor extractor = new SevenZipExtractor(archivePath);
            await extractor.ExtractFilesAsync(Application.StartupPath, $@"{extractor.ArchiveFileNames[0]}\bin\" + ffmpegExe);

            // Move the extracted file to the root directory and delete the unneeded folders
            string extractedFilePath = Path.Combine(Application.StartupPath, extractor.ArchiveFileNames[0], "bin", ffmpegExe);
            string destinationPath = Path.Combine(Application.StartupPath, ffmpegExe);

            File.Move(extractedFilePath, destinationPath, true);

            // Delete the extracted folders
            Directory.Delete(Path.Combine(Application.StartupPath, extractor.ArchiveFileNames[0]), true);

        }
        catch (Exception ex)
        {
            if (Debugger.IsAttached)
            {
                Console.WriteLine(ex.Message);
            }
            MessageBox.Show(@"Couldn't download or extract " + ffmpegExe, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }
        finally
        {
            File.Delete(archivePath);
            btnDownload.Enabled = true;
            btnDownload.Text = @"Download";
        }
    }

    private async Task DownloadYTDLAsync()
    {
        btnDownload.Enabled = false;
        btnDownload.Text = @"Downloading " + youtubeDLExe;

        try
        {
            HttpResponseMessage response = await httpClient.GetAsync(ytp);

            if (response.IsSuccessStatusCode)
            {
                await using Stream contentStream = await response.Content.ReadAsStreamAsync(), fileStream = new FileStream(YoutubeDLPath(), FileMode.Create, FileAccess.Write, FileShare.None);
                await contentStream.CopyToAsync(fileStream);
            }
            else
            {
                throw new HttpRequestException("Couldn't download " + youtubeDLExe);
            }
        }
        catch (Exception ex)
        {
            if (Debugger.IsAttached)
            {
                Console.WriteLine(ex.Message);
            }
            MessageBox.Show(@"Couldn't download " + youtubeDLExe, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }
        finally
        {
            btnDownload.Enabled = true;
            btnDownload.Text = @"Download";
        }
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
        if (isDownloading)
        {
            return;
        }

        SaveFileDialog saveFileDialog = new SaveFileDialog();
        saveFileDialog.FileName = "output";
        saveFileDialog.Filter = rdoMp3.Checked
            ? "MP3 Audio File (*.mp3)|*.mp3"
            : "MP4 Video File (*.mp4)|*.mp4";

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
            youtubeDlCommand = $"\"{YoutubeDLPath()}\" -o \"{outputPath}\" \"{url}\"";
        }
        else if (rdoMp3.Checked)
        {
            youtubeDlCommand = $"\"{YoutubeDLPath()}\" -x --audio-format mp3 -o \"{outputPath}\" \"{url}\"";
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
                BeginInvoke(() => {
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

    private void Set7ZipLibraryPath()
    {
        string? sevenZipPath = Get7ZipPath();

        if (!string.IsNullOrEmpty(sevenZipPath))
        {
            SevenZipBase.SetLibraryPath(Path.Combine(sevenZipPath, "7z.dll"));
        }
        else
        {
            string localSevenZipDllPath = Path.Combine(Application.StartupPath, "7z.dll");
            if (File.Exists(localSevenZipDllPath))
            {
                SevenZipBase.SetLibraryPath(localSevenZipDllPath);
            }
            else
            {
                MessageBox.Show(@"7-Zip was not installed on this computer, and a fall back 7z.dll was not found in the application directory.\n\nPlease install 7-Zip or put a 7z.dll file in the application directory.\n\nAlternatively, download FFMPEG at " + ffmpegUrl + @"and extract ffmpeg.exe in the bin folder to the same directory as this executable, or put it in your PATH.", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }
    }

    private string? Get7ZipPath()
    {
        string programFilesPath = Environment.GetFolderPath(Environment.Is64BitOperatingSystem ? Environment.SpecialFolder.ProgramFiles : Environment.SpecialFolder.ProgramFilesX86);

        string expectedPath = Path.Combine(programFilesPath, "7-Zip");
        if (Directory.Exists(expectedPath))
        {
            return expectedPath;
        }

        using RegistryKey? key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\7-Zip");
        if (key != null)
        {
            string? path = key.GetValue("Path") as string;
            if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
            {
                return path;
            }
        }
        return null;
    }

    [GeneratedRegex("^(https?://)?(www\\.)?(youtube\\.com/watch\\?v=)([a-zA-Z0-9_-]+).*")]
    private static partial Regex MyRegex();
}