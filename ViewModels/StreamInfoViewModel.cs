using YoutubeExplode.Videos.Streams;

namespace Media_Downloader_App.ViewModels
{
    public class StreamInfoViewModel
    {
        public string StreamInfoDisplay { get; set; }
        public IVideoStreamInfo StreamInfo { get; set; }
    }
}
