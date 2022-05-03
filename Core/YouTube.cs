using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Search;

namespace Media_Downloader_App.Core
{
    public class YouTube
    {
        private YoutubeClient Client;
        public YouTubeVideo CurrentVideo { get; private set; }
        public async Task SetCurrentVid(string URL)
        {
            try
            {
                Client = new YoutubeClient();
                var temp = await Client.Videos.GetAsync(URL);
                CurrentVideo = new YouTubeVideo(temp, false);
            }
            catch
            {
                throw new ArgumentException("Invalid URL");
            }
        }
        public async Task<YouTubeVideo> GetVideo(string URL)
        {
            try
            {
                Client = new YoutubeClient();
                var temp = await Client.Videos.GetAsync(URL);
                var thumbnail = temp.Thumbnails.GetWithHighestResolution().Url;
                return new YouTubeVideo(temp, false, thumbnail);
            }
            catch
            {
                throw new ArgumentException("Invalid URL");
            }
        }
        public async Task<List<YouTubeVideo>> BrowseYouTubeVideo(string BrowseQuery, int Results, int Offset)
        {
            List<YouTubeVideo> temp = new List<YouTubeVideo>();
            var TempClient = new YoutubeClient();
            var Videos = await TempClient.Search.GetVideosAsync(BrowseQuery).CollectAsync(Results + Offset);

            if (Videos.Count >= Results)
            {
                for (int i = 0; i < Results; i++)
                {
                    temp.Add(await GetVideo(Videos[i + Offset].Url));
                }
            }
            else
            {
                for (int i = 0; i < Videos.Count; i++)
                {
                    temp.Add(await GetVideo(Videos[i + Offset].Url));
                }
            }
            return temp;
        }
        public async Task<IReadOnlyList<VideoSearchResult>> GetVideoSearchResult(string BrowseQuery, int Results, int Offset)
        {
            List<string> temp = new List<string>();
            var TempClient = new YoutubeClient();
            var Videos = await TempClient.Search.GetVideosAsync(BrowseQuery).CollectAsync(Results + Offset);

            return Videos;
        }
    }
}
