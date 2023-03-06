using YoutubeExplode.Videos.Streams;

namespace Melody.ViewModels
{
    public class StreamInfoViewModel
    {
        public string StreamInfoDisplay { get; set; }
        public IVideoStreamInfo StreamInfo { get; set; }
    }
}
