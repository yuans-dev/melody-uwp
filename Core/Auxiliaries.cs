using Hqub.Lastfm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
using System.Diagnostics;
using MetaBrainz.MusicBrainz;

namespace Melody.Core
{
    public static class Auxiliaries
    {
        private static readonly Query mbq = new Query("Melody", "1.1.9");
        private static readonly LastfmClient Client = new LastfmClient("426c76e1e708befbffef3ff521b7f875", "7fdafab8e578755d150712bc2ee82148");
        public static async Task<List<string>> GetTrackTags(string Title, string Artist)
        {
            var release = await mbq.FindRecordingsAsync($"{Title.ToLower()} AND artist:\"{Artist.ToLower()}\"", 10, 0);
            var tags = new List<MetaBrainz.MusicBrainz.Interfaces.Entities.ITag>();
            if(release.Results.Count > 0)
            {
                for (int i = 0; i < release.Results.Count; i++)
                {
                    if (release.Results[i].Item.Tags == null)
                    {
                        if (release.Results[i].Item.UserTags == null)
                        {

                        }
                        else
                        {
                            tags = release.Results[i].Item.UserTags.ToList();
                            break;
                        }
                    }
                    else
                    {
                        tags = release.Results[i].Item.Tags.ToList();
                        break;
                    }
                }
            }
            var taglist = new List<string>();
            foreach(var tag in tags)
            {
                //var str = tag.ToString().Substring(tag.ToString().IndexOf("]") + 2, tag.ToString().Length - tag.ToString().IndexOf("]") - 2);
                var str = tag.Name;
                taglist.Add(str);
            }
            return taglist;
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
        public static async Task<List<Hqub.Lastfm.Entities.Track>> GetTopTracks()
        {
            var response = await Client.Chart.GetTopTracksAsync(1, 50);
            return response.ToList();
        }
    }
}
