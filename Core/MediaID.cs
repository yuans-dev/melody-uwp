using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Melody.Core
{
    public struct MediaID
    {
        public MediaID(MediaType type, string id)
        {
            ID = id;
            MediaType = type;
        }
        public string ID { get; set; }
        public MediaType MediaType { get; set; }
        public async Task<IBaseMedia> GetMedia()
        {
            switch (MediaType)
            {
                case MediaType.SpotifyTrack:
                    return await Settings.SpotifyClient.GetTrack(ID);
                case MediaType.SpotifyAlbum:
                    return await Settings.SpotifyClient.GetAlbum(ID);
                case MediaType.SpotifyPlaylist:
                    return await Settings.SpotifyClient.GetPlaylist(ID);
                case MediaType.YouTubeVideo:
                    return await Settings.YouTubeClient.GetVideo(ID);
                case MediaType.YouTubePlaylist:
                    return await Settings.YouTubeClient.GetPlaylist(ID);
                default:
                    return Media.Empty();

            }
        }
        public override string ToString()
        {
            return $"{MediaType}#-#{ID}";
        }
        public static MediaID Parse(string str)
        {
            var res = str.Split("#-#");
            return new MediaID((MediaType)Enum.Parse(typeof(MediaType),res[0]), res[1]);
        }
    }
    public enum MediaType
    {
        SpotifyTrack,
        SpotifyAlbum,
        SpotifyPlaylist,
        YouTubeVideo,
        YouTubePlaylist
    }
}
