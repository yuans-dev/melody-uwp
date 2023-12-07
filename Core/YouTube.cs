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
using System.ServiceModel.Channels;
using Melody.Statics;
using Melody.Classes;
using System.Diagnostics;
using YoutubeExplode.Playlists;

namespace Melody.Core
{
    public class YouTube
    {
        public new string Name { get; set; } = "YOUTUBE";
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
        public async Task<YouTubePlaylist> GetPlaylist(string URL)
        {
            Client = new YoutubeClient();
            var temp = await Client.Playlists.GetAsync(URL);
            var thumbnail = temp.Thumbnails.GetWithHighestResolution().Url;
            return new YouTubePlaylist(temp, thumbnail, 0);
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
                throw new ArgumentException($"Invalid URL {URL}");
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
            int Results = 3;
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
                Title = Title.Replace('.', ' ');
                Title = (await Title.Romanize()).ToUpper();
                var ChannelName = Result.Author.Title.ToUpper();

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
        public async Task<string> FuzzySearch(string SearchQuery, double Duration)
        {
            var TempClient = new YoutubeClient();
            int Results = 3;
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
                Title = (await Title.Romanize()).ToUpper();
                var ChannelName = Result.Author.Title.ToUpper();

                if (ts.TotalMilliseconds.IsWithinRange(Duration + 15000, Duration - 15000 / 4))
                {
                    return Result.Url;
                }
            }

            return Videos[0].Url;
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
        public async Task<List<YouTubePlaylist>> BrowseYouTubePlaylists(string BrowseQuery, int Results, int Offset)
        {
            List<YouTubePlaylist> temp = new List<YouTubePlaylist>();
            var TempClient = new YoutubeClient();
            var Playlists = await TempClient.Search.GetPlaylistsAsync(BrowseQuery).CollectAsync(Results + Offset);

            if (Playlists.Count >= Results)
            {
                for (int i = 0; i < Results; i++)
                {
                    temp.Add(await GetPlaylist(Playlists[i + Offset].Url));
                }
            }
            else
            {
                for (int i = 0; i < Playlists.Count; i++)
                {
                    temp.Add(await GetPlaylist(Playlists[i + Offset].Url));
                }
            }
            return temp;
        }
        public async Task<IReadOnlyList<VideoSearchResult>> GetVideoSearchResult(string BrowseQuery, int Results, int Offset)
        {
            var TempClient = new YoutubeClient();
            var Videos = await TempClient.Search.GetVideosAsync(BrowseQuery).CollectAsync(Results + Offset);

            return Videos;
        }
        public async Task<IReadOnlyList<PlaylistSearchResult>> GetPlaylistSearchResult(string BrowseQuery, int Results, int Offset)
        {
            var TempClient = new YoutubeClient();
            var Playlists = await TempClient.Search.GetPlaylistsAsync(BrowseQuery).CollectAsync(Results + Offset);

            return Playlists;
        }
        public async Task<List<YouTubeVideo>> GetPlaylistVideos(YouTubePlaylist playlist)
        {
            int i = 0;
            List<YouTubeVideo> ytvideos = new List<YouTubeVideo>();
            var TempClient = new YoutubeClient();
            var videos = await TempClient.Playlists.GetVideosAsync(playlist.ID.ID);
            foreach(var v in videos)
            {
                ytvideos.Add(new YouTubeVideo(v, true));
                i++;
            }
            playlist.UpdateMediaCount(i);
            return ytvideos;
        }
        public async Task<int> GetPlaylistVideoCount(Playlist playlist)
        {
            int i = 0;
            var TempClient = new YoutubeClient();
            var videos = await TempClient.Playlists.GetVideosAsync(playlist.Id);
            foreach (var v in videos)
            {
                i++;
            }
            return i;
        }
    }
}
