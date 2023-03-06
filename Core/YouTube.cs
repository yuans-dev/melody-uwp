using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Unidecode.NET;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Common;
using YoutubeExplode.Search;
using YoutubeExplode.Videos.Streams;
using System.Diagnostics;
using System.ServiceModel.Channels;
using Melody.Statics;

namespace Melody.Core
{
    public class YouTube
    {
        private YoutubeClient Client;
        public YouTubeVideo CurrentVideo { get; private set; }
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
        public async void InitializeURL(string URL)
        {
            try
            {
                Client = new YoutubeClient();
                var temp = await Client.Videos.GetAsync(URL);
            }
            catch
            {
                throw new ArgumentException("Invalid URL");
            }
        }
        public async Task<IStreamInfo> GetStreamInfo(string URL, bool IsVideo = false, IStreamInfo RequestedVideoQuality = null)
        {
            var manifest = await Client.Videos.Streams.GetManifestAsync(URL);
            if (IsVideo)
            {
                if (RequestedVideoQuality is null)
                {
                    return manifest.GetVideoOnlyStreams().GetWithHighestVideoQuality();
                }
                else
                {
                    return RequestedVideoQuality;
                }
            }
            else
            {
                IStreamInfo output = null;
                foreach(var stream in manifest.GetAudioOnlyStreams())
                {
                    System.Diagnostics.Debug.WriteLine($"[TASK](Getting stream info) {stream.Container} | {stream.Bitrate.KiloBitsPerSecond}kbps");
                    if (stream.Container == Container.WebM && (output == null || stream.Bitrate.KiloBitsPerSecond > output.Bitrate.KiloBitsPerSecond))
                    {
                        output = stream;
                    }
                }
                if(output == null)
                {
                    return manifest.GetAudioOnlyStreams().GetWithHighestBitrate();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"{output.Container} | {output.Bitrate.KiloBitsPerSecond}kbps");
                    return output;
                }
            }
        }

        public async Task<Stream> GetStream(IStreamInfo streaminfo, System.Threading.CancellationToken token)
        {
            var stream = await Client.Videos.Streams.GetAsync(streaminfo, token);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }
        public async Task<string> Search(string SearchQuery, string Keyword, double Duration, double MarginOfError)
        {
            var TempClient = new YoutubeClient();
            int Results = 1;
            var Keywords = Keyword.ToUpper().GetComponents();
            VideoSearchResult Result;
            var Videos = await TempClient.Search.GetVideosAsync(SearchQuery).CollectAsync(Results);
            for (int i = 0; i < Results; i++)
            {
                Result = Videos[i];
                TimeSpan ts = (TimeSpan)Result.Duration;
                var Title = Result.Title.ToUpper();
                Title = Title.Replace('(', ' ');
                Title = Title.Replace(')', ' ');
                Title = Title.Replace('」', ' ');
                Title = Title.Replace('「', ' ');
                Title = Title.Unidecode().ToUpper();
                var ChannelName = Result.Author.Title.ToUpper();

                Debug.WriteLine($"[SEARCHINFO] Media found: {ChannelName} {Title} ({ts.TotalMilliseconds}) \n" +
                            $"comparing to keyword: {Keywords[0]} \n" +
                            $"with range: {Duration - MarginOfError/16} - {Duration + MarginOfError}");

                if (ts.TotalMilliseconds.IsWithinRange(Duration + MarginOfError, Duration - MarginOfError/16))
                {
                    if (Title.Contains(Keywords, true) || ChannelName.Contains(Keywords[0]))
                    {
                        return Result.Url;
                    }
                }
            }

            return string.Empty;
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
