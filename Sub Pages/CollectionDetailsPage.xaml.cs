using Melody.Statics;
using Melody.Core;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Melody.Classes;
using Windows.System;
using System.Threading.Tasks;
using Melody.Sub_Pages;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Melody.SubPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CollectionDetailsPage : BasePage
    {
        public CollectionDetailsPage()
        {
            this.InitializeComponent();

            RequestedTheme = Settings.Theme;
        }
        public IMediaCollection Collection { get; set; }
        private SpotifyTrack PreviouslyPlayed { get; set; }
        private MediaType mediatype { get; set; }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            IsLoading = true;
            if (e.Parameter is SpotifyPlaylist playlist)
            {
                Collection = playlist;
                mediatype = MediaType.SpotifyPlaylist;
                try
                {
                    MediaListView.ItemsSource = await Settings.SpotifyClient.GetPlaylistTracks(playlist);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[CollectionDetailsPage] {ex.Message}");
                }
            }
            else if (e.Parameter is SpotifyAlbum album)
            {
                Collection = album;
                mediatype = MediaType.SpotifyAlbum;
                try
                {
                    MediaListView.ItemsSource = await Settings.SpotifyClient.GetAlbumTracks(album);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[CollectionDetailsPage] {ex.Message}");
                }
            }else if(e.Parameter is YouTubePlaylist yplaylist)
            {
                Collection = yplaylist;
                mediatype = MediaType.YouTubePlaylist;
                try
                {
                    MediaListView.ItemsSource = await Settings.YouTubeClient.GetPlaylistVideos(yplaylist);
                }catch(Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[CollectionDetailsPage] {ex.Message}");
                }
            }

            IsLoading = false;
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if(mediatype == MediaType.YouTubePlaylist)
            {
                var vidbutton = sender as Button;
                var vid = vidbutton.DataContext as YouTubeVideo;

                App.SendToRootFrame(typeof(YouTubePreviewPage), vid);
            }
            else
            {
                var button = sender as Button;
                var item = button.DataContext as SpotifyTrack;

                Player.Player.Play(item);
            }
        }
        private void ST_Download_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuFlyoutItem;
            var media = item.DataContext as IMedia;

            InfoHelper.ShowInAppNotification($"Successfully added \"{media.Name}\" to Downloads",InAppNotif);
            DownloadManager.AddToDownloads(media);
        }
        private async void ST_OpenInWeb_Click(object sender, RoutedEventArgs e)
        {
            var flyoutitem = sender as MenuFlyoutItem;
            var track = (flyoutitem.DataContext as SpotifyTrack);

            ContentDialog dialog = new ContentDialog()
            {
                Title = "Opening...",
                Content = "Would you like to open it in the app?",
                PrimaryButtonText = "Open in app",
                SecondaryButtonText = "Open in web",
                SecondaryButtonStyle = ButtonTransparentBackground,
                CloseButtonStyle = ButtonTransparentBackground,
                CloseButtonText = "Cancel",
                RequestedTheme = Settings.Theme
            };
            switch (await dialog.ShowAsync())
            {
                case ContentDialogResult.Primary:
                    await Launcher.LaunchUriAsync(new Uri(track.Link.App, UriKind.Absolute), new LauncherOptions { FallbackUri = new Uri(@"https://www.spotify.com/us/download/", UriKind.Absolute) });
                    break;
                case ContentDialogResult.Secondary:
                    await Launcher.LaunchUriAsync(new Uri(track.Link.Web, UriKind.Absolute), new LauncherOptions { FallbackUri = new Uri(@"https://www.spotify.com/us/", UriKind.Absolute) });
                    break;
                default:
                    break;
            }
        }
        private void ST_CopyLink_Click(object sender, RoutedEventArgs e)
        {
            var flyoutitem = sender as MenuFlyoutItem;
            var mediaLinks = (flyoutitem.DataContext as SpotifyTrack).Link;
            mediaLinks.Web.CopyToClipboard();

            InfoHelper.ShowInAppNotification("Copied to clipboard!", InAppNotif);
        }
        private void ST_MoreLikeThis_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as MenuFlyoutItem).DataContext as SpotifyTrack;

            MainPage.Current.Navigate(typeof(MoreLikeThisPage), item);
        }

        private void MediaListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MediaListView.SelectedItem = null;
        }
    }
}
