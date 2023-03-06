using Melody.Core;
using Newtonsoft.Json;
using Windows.UI.Xaml.Media.Imaging;

namespace Melody.ViewModels
{
    public interface IDownloadItem
    {
        BitmapImage Bitmap { get; }
        string Title { get; }
        string[] Authors { get; }
        string Status { get; }
        int ProgressValue { get; }
        string StatusGlyph { get; }
        string ToString();
        void CancelDownload();
    }

}
