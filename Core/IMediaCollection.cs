using Windows.UI.Xaml.Media.Imaging;

namespace Melody.Core
{
    public interface IMediaCollection : IBaseMedia
    {
        MediaLink Link { get; }
        uint MediaCount { get; }
    }
}
