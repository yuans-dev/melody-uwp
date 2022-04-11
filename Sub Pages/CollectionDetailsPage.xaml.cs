using Media_Downloader_App.Statics;
using MP3DL.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Media_Downloader_App.SubPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CollectionDetailsPage : Page
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
            LoadingControl.IsLoading = true;
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
            LoadingControl.IsLoading = false;
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
            var mediaplayer = VisualTreeHelper.GetChild(VisualTreeHelper.GetParent(button), 8) as MediaElement;

            if (item.Symbol == Symbol.Play)
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
                item.Symbol = Symbol.Pause;

                PreviouslyPlayed = item;
            }
            else
            {
                mediaplayer?.Stop();
                item.IsPlayingPreview = false;
                item.Symbol = Symbol.Play;
            }
        }
        private void ClearAllPlaying()
        {
            foreach (SpotifyTrack item in MediaListView.Items)
            {
                if (item.IsPlayingPreview)
                {
                    var container = MediaListView.ContainerFromItem(item);
                    var mediaplayer = VisualTreeHelper.GetChild(DependencyObjectHelper.RecursiveGetFirstChild(container, 2), 8) as MediaElement;

                    mediaplayer?.Stop();
                    item.IsPlayingPreview = false;
                    item.Symbol = Symbol.Play;
                }
            }
        }
        private void ClearPreviouslyPlayed()
        {
            if(PreviouslyPlayed != null)
            {
                var container = MediaListView.ContainerFromItem(PreviouslyPlayed);
                var mediaplayer = VisualTreeHelper.GetChild(DependencyObjectHelper.RecursiveGetFirstChild(container, 2), 8) as MediaElement;

                mediaplayer?.Stop();
                PreviouslyPlayed.IsPlayingPreview = false;
                PreviouslyPlayed.Symbol = Symbol.Play;
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
