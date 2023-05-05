namespace YTDownloader;

public static class Constants
{
    public const string youtubeDLExe = "yt-dlp.exe";
    public const string ffmpegExe = "ffmpeg.exe";

    public const string ffmpegUrl = "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-full.7z";
    public const string ytp = "https://github.com/yt-dlp/yt-dlp/releases/download/2023.03.04/yt-dlp.exe";

    public static string YoutubeDLPath()
    {
        return Path.Combine(Application.StartupPath, youtubeDLExe);
    }
}