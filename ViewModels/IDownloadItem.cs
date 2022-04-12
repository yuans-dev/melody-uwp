using Windows.UI.Xaml.Media.Imaging;

namespace Media_Downloader_App.ViewModels
{
    public interface IDownloadItem
    {
        BitmapImage Bitmap { get; }
        string Title { get; }
        string Author { get; }
        string Status { get; }
        int ProgressValue { get; }
        string StatusGlyph { get; }
        string ToString();
        void CancelDownload();
    }
}
