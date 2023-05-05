namespace YTDownloader;

internal static class Program
{
    
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainWindow(new DependanciesDownloadHelper(), new HttpClient()));
    }    
}