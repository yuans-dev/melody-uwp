using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeExplode.Videos.Streams;

namespace Media_Downloader_App.ViewModels
{
    public class StreamInfoViewModel
    {
        public string StreamInfoDisplay { get; set; }
        public IVideoStreamInfo StreamInfo { get; set; }
    }
}
