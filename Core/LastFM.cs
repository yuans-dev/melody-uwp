using Hqub.Lastfm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace Media_Downloader_App.Core
{
    public static class LastFM
    {
        private static LastfmClient Client = new LastfmClient("426c76e1e708befbffef3ff521b7f875", "7fdafab8e578755d150712bc2ee82148");
        public static async Task<List<string>> GetTrackTags(string Title, string Artist)
        {
            var list = new List<string>();
            try
            {
                var response = await Client.Track.GetInfoAsync(Title, Artist);
                foreach (var tag in response.Tags)
                {
                    list.Add(tag.Name);
                }
            }
            catch (NullReferenceException)
            {
                System.Diagnostics.Debug.WriteLine($"[LastFM] No tags found for \"{Artist} - {Title}\"");
            }
            return list;
        }
        public static async Task<List<Hqub.Lastfm.Entities.Track>> GetSimilarTracks(string Title,string Artist, int Results)
        {
            var response = await Client.Track.GetSimilarAsync(Title, Artist,Results,false);
            return response;
        }
        public static async Task<List<Hqub.Lastfm.Entities.Track>> GetTopTracksInTag(string Tag, int Results)
        {
            var response = await Client.Tag.GetTopTracksAsync(Tag, page: 1, Results);
            return response.ToList();
        }
    }
}
