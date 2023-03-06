using SpotifyAPI.Web;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Media.Imaging;

namespace Melody.Core
{
    public class SpotifyAlbum : IMediaCollection
    {
        public SpotifyAlbum(FullAlbum Album)
        {
            Title = Album.Name;
            Authors = new string[1] { Album.Artists[0].Name };
            ID = new MediaID(MediaType.YouTubeVideo, Album.Id);
            Link = new MediaLink(Album.Uri, "https://open.spotify.com/album/" + Album.Id);
            MediaCount = (uint)Album.Tracks.Total;
            Medias = Album.Tracks.Items;
            Bitmap = new BitmapImage(new System.Uri(Album.Images[0].Url, System.UriKind.Absolute));
        }
        public SpotifyAlbum(SimpleAlbum Album)
        {
            Title = Album.Name;
            Authors = new string[1] { Album.Artists[0].Name };
            ID = new MediaID(MediaType.YouTubeVideo, Album.Id);
            Link = new MediaLink(Album.Uri, "https://open.spotify.com/album/" + Album.Id);
            MediaCount = (uint)Album.TotalTracks;
            Bitmap = new BitmapImage(new System.Uri(Album.Images[0].Url, System.UriKind.Absolute));
        }
        public string Title { get; set; }

        public string[] Authors { get; set; }
        public string Name
        {
            get { return $"{Authors.First()} - {Title}"; }
        }

        public MediaID ID { get; private set; }
        public MediaLink Link { get; private set; }

        public uint MediaCount { get; private set; }

        public List<SimpleTrack> Medias { get; internal set; }
        public BitmapImage Bitmap { get; set; }
        public override string ToString()
        {
            return "Album";
        }
    }
}
