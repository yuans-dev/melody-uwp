using SpotifyAPI.Web;
using System.Collections.Generic;
using Windows.UI.Xaml.Media.Imaging;

namespace MP3DL.Media
{
    public class SpotifyAlbum : IMediaCollection
    {
        public SpotifyAlbum(FullAlbum Album)
        {
            Title = Album.Name;
            Author = Album.Artists[0].Name;
            ID = Album.Id;
            Link = new MediaLink(Album.Uri, "https://open.spotify.com/album/" + Album.Id);
            MediaCount = (uint)Album.Tracks.Total;
            Medias = Album.Tracks.Items;
            Bitmap = new BitmapImage(new System.Uri(Album.Images[0].Url, System.UriKind.Absolute));
        }
        public SpotifyAlbum(SimpleAlbum Album)
        {
            Title = Album.Name;
            Author = Album.Artists[0].Name;
            ID = Album.Id;
            Link = new MediaLink(Album.Uri, "https://open.spotify.com/album/" + Album.Id);
            MediaCount = (uint)Album.TotalTracks;
            Bitmap = new BitmapImage(new System.Uri(Album.Images[0].Url, System.UriKind.Absolute));
        }
        public string Title { get; private set; }

        public string Author { get; private set; }

        public string ID { get; private set; }
        public MediaLink Link { get; private set; }

        public uint MediaCount { get; private set; }

        public List<SimpleTrack> Medias { get; internal set; }
        public BitmapImage Bitmap { get; set; }
    }
}
