using System.Diagnostics;
using Microsoft.Win32;
using SevenZip;

namespace YTDownloader;

public class DependanciesDownloadHelper
{
    public async Task DownloadRequiredFilesAsync(HttpClient httpClient, Button downloadButton)
    {
        if (!FileExistsInPath(Constants.youtubeDLExe))
        {
            await DownloadYTDLAsync(httpClient, downloadButton);
        }

        if (!FileExistsInPath(Constants.ffmpegExe))
        {
            await DownloadAndExtractFFMPEGAsync(httpClient, downloadButton);
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
    
    private async Task DownloadAndExtractFFMPEGAsync(HttpClient httpClient, Control btnDownload)
    {
        btnDownload.Enabled = false;
        btnDownload.Text = @"Downloading " + Path.GetFileName(Constants.ffmpegUrl);

        // Check if 7ZIP is installed and set the library up
        // If 7zip isn't installed, it looks for 7zip in current directory
        // If neither is found, it recommends to download ffmpeg manually.
        // This means the user didn't have ffmpeg in PATH, in folder, and also didn't have 7zip installed
        // and they also didn't have 7zip from the folder either which is distributed from the release
        // big fail!
        Set7ZipLibraryPath();

        string archivePath = Path.Combine(Path.GetTempPath(), Path.GetFileName(Constants.ffmpegUrl));

        HttpResponseMessage response = await httpClient.GetAsync(Constants.ffmpegUrl);

        try
        {
            if (response.IsSuccessStatusCode)
            {
                await using Stream contentStream = await response.Content.ReadAsStreamAsync(), fileStream = new FileStream(archivePath, FileMode.Create, FileAccess.Write, FileShare.None);
                await contentStream.CopyToAsync(fileStream);
            }
            else
            {
                throw new HttpRequestException("Couldn't download " + Path.GetFileName(Constants.ffmpegUrl));
            }

            btnDownload.Text = @"Extracting " + Constants.ffmpegExe;

            using SevenZipExtractor extractor = new SevenZipExtractor(archivePath);
            await extractor.ExtractFilesAsync(Application.StartupPath, $@"{Constants.ffmpegInnerFolderName}\bin\" + Constants.ffmpegExe);

            // Move the extracted file to the root directory and delete the unneeded folders
            string extractedFilePath = Path.Combine(Application.StartupPath, Constants.ffmpegInnerFolderName, "bin", Constants.ffmpegExe);
            string destinationPath = Path.Combine(Application.StartupPath, Constants.ffmpegExe);

            File.Move(extractedFilePath, destinationPath, true);

            // Delete the extracted folders
            try
            {
                Directory.Delete(Path.Combine(Application.StartupPath, Constants.ffmpegInnerFolderName), true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }
        catch (Exception ex)
        {
            if (Debugger.IsAttached)
            {
                Console.WriteLine(ex.Message);
            }
            MessageBox.Show(@"Couldn't download or extract " + Constants.ffmpegExe, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }
        finally
        {
            File.Delete(archivePath);
            btnDownload.Enabled = true;
            btnDownload.Text = @"Download";
        }
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
                MessageBox.Show(@"7-Zip was not installed on this computer, and a fall back 7z.dll was not found in the application directory.\n\nPlease install 7-Zip or put a 7z.dll file in the application directory.\n\nAlternatively, download FFMPEG at " + Constants.ffmpegUrl + @"and extract ffmpeg.exe in the bin folder to the same directory as this executable, or put it in your PATH.", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
    
    private async Task DownloadYTDLAsync(HttpClient httpClient, Button btnDownload)
    {
        btnDownload.Enabled = false;
        btnDownload.Text = @"Downloading " + Constants.youtubeDLExe;

        try
        {
            HttpResponseMessage response = await httpClient.GetAsync(Constants.ytp);

            if (response.IsSuccessStatusCode)
            {
                await using Stream contentStream = await response.Content.ReadAsStreamAsync(), fileStream = new FileStream(Constants.YoutubeDLPath(), FileMode.Create, FileAccess.Write, FileShare.None);
                await contentStream.CopyToAsync(fileStream);
            }
            else
            {
                throw new HttpRequestException("Couldn't download " + Constants.youtubeDLExe);
            }
        }
        catch (Exception ex)
        {
            if (Debugger.IsAttached)
            {
                Console.WriteLine(ex.Message);
            }
            MessageBox.Show(@"Couldn't download " + Constants.youtubeDLExe, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }
        finally
        {
            btnDownload.Enabled = true;
            btnDownload.Text = @"Download";
        }
    }
    

    
}