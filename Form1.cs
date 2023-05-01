using System.Diagnostics;
using System.Text.RegularExpressions;
using System.ComponentModel;
using Microsoft.Win32;
using SevenZip;

namespace YTDownloader
{
    public enum DownloadButtonState
    {
        Ready,
        DownloadingFFMPEG,
        DownloadingYTDL,
        DownloadingVideo,
        DownloadingMp3,
        ExtractingFFMPEG
    }

    public partial class Form1 : Form
    {
        private bool isDownloading;
        private readonly string youtubeDlPath;
        private const string downloaderExe = "yt-dlp.exe";
        private readonly HttpClient httpClient;
        private DownloadButtonState downloadButtonState;
        private readonly string[] downloadStatusTexts = {
            @"Download",
            @"Downloading ffmpeg-release-full.7z...",
            @"Downloading YT-DLP.exe...",
            @"Downloading video...",
            @"Downloading MP3...",
            @"Extracting FFMPEG..."
        };

        public Form1()
        {
            InitializeComponent();
            youtubeDlPath = Path.Combine(Application.StartupPath, downloaderExe);
            Path.Combine(Application.StartupPath, "ffmpeg.exe");
            httpClient = new HttpClient();
            DownloadRequiredFilesAsync().ConfigureAwait(false);
            txtUrl.TextChanged += TxtUrl_TextChanged;
            downloadButtonState = DownloadButtonState.Ready;
            UpdateDownloadButton();
        }

        private void UpdateDownloadButton()
        {
            btnDownload.Text = downloadStatusTexts[(int)downloadButtonState];
            btnDownload.Enabled = downloadButtonState == DownloadButtonState.Ready;
        }

        private async Task DownloadRequiredFilesAsync()
        {
            if (!FileExistsInPath("yt-dlp.exe"))
            {
                await DownloadYTDLAsync();
            }

            if (!FileExistsInPath("ffmpeg.exe"))
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
            downloadButtonState = DownloadButtonState.DownloadingFFMPEG;
            UpdateDownloadButton();

            // Check if 7ZIP is installed and set the library up
            // If 7zip isn't installed, it looks for 7zip in current directory
            // If neither is found, it recommends to download ffmpeg manually.
            // This means the user didn't have ffmpeg in PATH, in folder, and also didn't have 7zip installed
            // and they also didn't have 7zip from the folder either which is distributed from the release
            // big fail!
            Set7ZipLibraryPath();

            string archivePath = Path.Combine(Path.GetTempPath(), "ffmpeg-release-full.7z");
            const string url = "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-full.7z";
            HttpResponseMessage response = await httpClient.GetAsync(url);

            try
            {
                if (response.IsSuccessStatusCode)
                {
                    await using Stream contentStream = await response.Content.ReadAsStreamAsync(), fileStream = new FileStream(archivePath, FileMode.Create, FileAccess.Write, FileShare.None);
                    await contentStream.CopyToAsync(fileStream);
                }
                else
                {
                    throw new HttpRequestException("Couldn't download ffmpeg-release-full.7z");
                }

                downloadButtonState = DownloadButtonState.ExtractingFFMPEG;
                UpdateDownloadButton();

                using SevenZipExtractor extractor = new SevenZipExtractor(archivePath);
                extractor.ExtractFiles(Application.StartupPath, $@"{extractor.ArchiveFileNames[0]}\bin\ffmpeg.exe");

                // Move the extracted file to the root directory and delete the unneeded folders
                string extractedFilePath = Path.Combine(Application.StartupPath, extractor.ArchiveFileNames[0], "bin", "ffmpeg.exe");
                string destinationPath = Path.Combine(Application.StartupPath, "ffmpeg.exe");

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
                MessageBox.Show(@"Couldn't download or extract ffmpeg.exe", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
            finally
            {
                File.Delete(archivePath);
                downloadButtonState = DownloadButtonState.Ready;
                UpdateDownloadButton();
            }
        }

        private async Task DownloadYTDLAsync()
        {
            downloadButtonState = DownloadButtonState.DownloadingYTDL;
            UpdateDownloadButton();

            try
            {
                HttpResponseMessage response = await httpClient.GetAsync("https://github.com/yt-dlp/yt-dlp/releases/download/2023.03.04/yt-dlp.exe");

                if (response.IsSuccessStatusCode)
                {
                    await using Stream contentStream = await response.Content.ReadAsStreamAsync(), fileStream = new FileStream(youtubeDlPath, FileMode.Create, FileAccess.Write, FileShare.None);
                    await contentStream.CopyToAsync(fileStream);
                }
                else
                {
                    throw new HttpRequestException("Couldn't download yt-dlp.exe");
                }
            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached)
                {
                    Console.WriteLine(ex.Message);
                }
                MessageBox.Show(@"Couldn't download yt-dlp.exe", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
            finally
            {
                downloadButtonState = DownloadButtonState.Ready;
                UpdateDownloadButton();
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

            isDownloading = true;
            txtUrl.Enabled = false;
            rdoMp3.Enabled = false;
            rdoVideo.Enabled = false;

            var url = txtUrl.Text;
            var youtubeDlCommand = "";

            if (rdoVideo.Checked)
            {
                downloadButtonState = DownloadButtonState.DownloadingVideo;
                youtubeDlCommand = $"\"{youtubeDlPath}\" -o \"%USERPROFILE%\\Desktop\\%(title)s.%(ext)s\" \"{url}\"";
            }
            else if (rdoMp3.Checked)
            {
                downloadButtonState = DownloadButtonState.DownloadingMp3;
                youtubeDlCommand = $"\"{youtubeDlPath}\" -x --audio-format mp3 -o \"%USERPROFILE%\\Desktop\\%(title)s.%(ext)s\" \"{url}\"";
            }

            UpdateDownloadButton();

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
            downloadButtonState = DownloadButtonState.Ready;
            UpdateDownloadButton();
            txtUrl.Enabled = true;
            rdoMp3.Enabled = true;
            rdoVideo.Enabled = true;
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
                    MessageBox.Show(@"7-Zip was not installed on this computer, and a fall back 7z.dll was not found in the application directory.\n\n
                    Please install 7-Zip or put the 7z.dll file in the application directory.\n\n
                    Alternatively, download FFMPEG at https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-full.7z and extract ffmpeg.exe in the bin folder
                    to the same directory as this executable, or put it in your PATH.", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

}