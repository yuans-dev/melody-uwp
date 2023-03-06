using SpotifyAPI.Web;
using System.Linq;
using Windows.UI.Xaml.Media.Imaging;

namespace Melody.Core
{
    public class SpotifyPlaylist : IMediaCollection
    {
        public SpotifyPlaylist(FullPlaylist Playlist)
        {
            Title = Playlist.Name;
            Authors = new string[1] { Playlist.Owner.DisplayName };

            ID = new MediaID(MediaType.YouTubeVideo, Playlist.Id);
            Link = new MediaLink(Playlist.Uri, "https://open.spotify.com/playlist/" + Playlist.Id);
            MediaCount = (uint)Playlist.Tracks.Total;
            Media = Playlist.Tracks;

            Bitmap = new BitmapImage(new System.Uri(Playlist.Images[0].Url, System.UriKind.Absolute));
        }
        public SpotifyPlaylist(SimplePlaylist Playlist)
        {
            Title = Playlist.Name;
            Authors = new string[1] { Playlist.Owner.DisplayName };

            ID = new MediaID(MediaType.YouTubeVideo, Playlist.Id);
            Link = new MediaLink(Playlist.Uri, "https://open.spotify.com/playlist/" + Playlist.Id);
            MediaCount = (uint)Playlist.Tracks.Total;
            Media = Playlist.Tracks;

            Bitmap = new BitmapImage(new System.Uri(Playlist.Images[0].Url, System.UriKind.Absolute));
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
        public Paging<PlaylistTrack<IPlayableItem>> Media { get; internal set; }
        public BitmapImage Bitmap { get; set; }
        public override string ToString()
        {
            return "Playlist";
        }
    }
}
