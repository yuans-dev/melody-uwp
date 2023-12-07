using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;

namespace Melody.Core
{
    public class YouTubePlaylist : IMediaCollection
    {
        public YouTubePlaylist(Playlist playlist, string thumbnailurl, int count)
        {
            Title = playlist.Title;
            Authors = new string[1] { playlist.Author.Title };
            MediaCount = (uint)count;
            Link = new MediaLink(playlist.Url);
            Bitmap = new BitmapImage(new System.Uri(thumbnailurl, System.UriKind.Absolute));
            ID = new MediaID(MediaType.YouTubePlaylist, playlist.Id);
        }
        public MediaLink Link { get; private set; }

        public uint MediaCount { get; private set; }

        public string Title { get; set; }
        public string[] Authors { get; set; }

        public string Name
        {
            get { return $"{Authors.First()} - {Title}"; }
        }

        public MediaID ID { get; private set; }

        public BitmapImage Bitmap { get; private set; }
        public override string ToString()
        {
            return "Playlist";
        }
        public void UpdateMediaCount(int count)
        {
            MediaCount = (uint)count;
        }
    }
}
