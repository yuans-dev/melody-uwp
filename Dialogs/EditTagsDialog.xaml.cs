using Melody.Core;
using Melody.Statics;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using YoutubeExplode.Videos;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Melody.Dialogs
{
    public sealed partial class EditTagsDialog : ContentDialog
    {
        public EditTagsDialog(IMedia Media)
        {
            this.InitializeComponent();

            this.Media = Media;

            MediaBitmapImage.Source = this.Media.Bitmap;
            TitleTextBox.Text = this.Media.Title;
            ArtistTextBox.Text = this.Media.Authors.ToString(", ");
            AlbumTextBox.Text = this.Media.Album;
        }
        private IMedia Media { get; set; }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Media.Title = TitleTextBox.Text;
            Media.Authors = ArtistTextBox.Text.ToArray(", ");
            Media.Album = AlbumTextBox.Text;
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private async void FetchFromSpotifyButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var video = Media as YouTubeVideo;
            var track = await Settings.SpotifyClient.GetTrack(await Settings.SpotifyClient.SearchTrack($"{ArtistTextBox.Text} - {TitleTextBox.Text}", video.DurationAsTimeSpan.TotalMilliseconds, 5));
            if (video.DurationAsTimeSpan.TotalMilliseconds.IsWithinRange(track.Duration + 1000, track.Duration - 500))
            {
                video.Title = track.Title;
                video.Authors = track.Authors;
                video.Album = track.Album;
                video.Bitmap = track.Bitmap;
                TitleTextBox.Text = video.Title;
                ArtistTextBox.Text = video.Authors.ToString(", ");
                AlbumTextBox.Text = video.Album;
            }
            else
            {
                video.Bitmap = track.Bitmap;
            }
            MediaBitmapImage.Source = video.Bitmap;
            video.SpotifyTagged = true;
        }

        private async void RevertButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var video = Media as YouTubeVideo;
            var original = await Settings.YouTubeClient.GetVideo(video.ID.ID);

            video.Title = original.Title;
            video.Authors = original.Authors;
            video.Album = original.Album;
            video.Bitmap = original.Bitmap;
            TitleTextBox.Text = video.Title;
            ArtistTextBox.Text = video.Authors.ToString(", ");
            AlbumTextBox.Text = video.Album;
            MediaBitmapImage.Source = video.Bitmap;
            video.SpotifyTagged = false;
        }
    }
}
