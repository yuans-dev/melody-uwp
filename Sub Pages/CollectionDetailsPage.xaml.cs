using Media_Downloader_App.Statics;
using Media_Downloader_App.Core;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Media_Downloader_App.Classes;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Media_Downloader_App.SubPages
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
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            IsLoading = true;
            if (e.Parameter is SpotifyPlaylist playlist)
            {
                CollectionLabel.Text = "PLAYLIST";
                Collection = playlist;
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
                CollectionLabel.Text = "ALBUM";
                Collection = album;
                try
                {
                    MediaListView.ItemsSource = await Settings.SpotifyClient.GetAlbumTracks(album);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[CollectionDetailsPage] {ex.Message}");
                }
            }
            IsLoading = false;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            InAppNotif.Dismiss();
            var button = sender as Button;
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.GoBack();
            button.IsEnabled = false;
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button.DataContext as SpotifyTrack;
            var mediaplayer = VisualTreeHelper.GetChild(VisualTreeHelper.GetParent(button), 9) as MediaElement;

            if (!item.IsPlayingPreview)
            {
                try
                {
                    ClearPreviouslyPlayed();
                }
                catch
                {

                }
                mediaplayer?.Play();
                item.IsPlayingPreview = true;

                PreviouslyPlayed = item;
            }
            else
            {
                mediaplayer?.Stop();
                item.IsPlayingPreview = false;
            }
        }
        private void ClearPreviouslyPlayed()
        {
            if (PreviouslyPlayed != null)
            {
                var container = MediaListView.ContainerFromItem(PreviouslyPlayed);
                var mediaplayer = VisualTreeHelper.GetChild(VisualTreeHelperExtensions.RecursiveGetFirstChild(container, 2), 9) as MediaElement;

                mediaplayer?.Stop();
                PreviouslyPlayed.IsPlayingPreview = false;
            }
        }
        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var media = button.DataContext as IMedia;

            MainPage.Current.AddToDownloads(media);
            InfoHelper.ShowInAppNotification($"Successfully added \"{media.Name}\" to Downloads", InAppNotif);
        }

        private void MediaListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MediaListView.SelectedItem = null;
        }
    }
}
