using SpotifyAPI.Web;
using Windows.UI.Xaml.Media.Imaging;

namespace MP3DL.Media
{
    public class SpotifyPlaylist : IMediaCollection
    {
        public SpotifyPlaylist(FullPlaylist Playlist)
        {
            Title = Playlist.Name;
            Author = Playlist.Owner.DisplayName;

            ID = Playlist.Id;
            Link = new MediaLink(Playlist.Uri, "https://open.spotify.com/playlist/" + Playlist.Id);
            MediaCount = (uint)Playlist.Tracks.Total;
            Media = Playlist.Tracks;

            Bitmap = new BitmapImage(new System.Uri(Playlist.Images[0].Url, System.UriKind.Absolute));
        }
        public SpotifyPlaylist(SimplePlaylist Playlist)
        {
            Title = Playlist.Name;
            Author = Playlist.Owner.DisplayName;

            ID = Playlist.Id;
            Link = new MediaLink(Playlist.Uri, "https://open.spotify.com/playlist/" + Playlist.Id);
            MediaCount = (uint)Playlist.Tracks.Total;
            Media = Playlist.Tracks;

            Bitmap = new BitmapImage(new System.Uri(Playlist.Images[0].Url, System.UriKind.Absolute));
        }
        public string Title { get; private set; }
        public string Author { get; private set; }
        public string ID { get; private set; }
        public MediaLink Link { get; private set; }
        public uint MediaCount { get; private set; }
        public Paging<PlaylistTrack<IPlayableItem>> Media { get; internal set; }
        public BitmapImage Bitmap { get; set; }
    }
}
