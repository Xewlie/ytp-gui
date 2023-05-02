# YouTube YT-DLP Express GUI Downloader

A simplistic WinForms GUI for YT-DLP written in C# .NET 7.0. This tool enables users to download high-quality videos and MP3 files from YouTube.

The program's intuitive design allows a user to download the highest quality file by default, or convert it to an MP3. The application is intentionally limited to a few options to keep it simple for quick use.

Upon launch, the tool will automatically download YT-DLP from https://github.com/yt-dlp/yt-dlp and FFMPEG from https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-full.7z if it is not detected in the path or in the current folder, saving both files in the same directory as the program.

The intended audience for this tool is developers who want to utilize the YT-DLP tool through a simple GUI.