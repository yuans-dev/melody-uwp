using Melody.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Melody.Dialogs;
using Melody.Statics;
using Melody.SubPages;
using Melody.Core;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml.Media.Animation;
using YoutubeExplode.Search;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Melody
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TopTrendingPage : BasePage
    {
        public TopTrendingPage()
        {
            this.InitializeComponent();

            TopTracksResults = new ObservableCollection<SpotifyTrack>();
            TrendingTracksResults = new ObservableCollection<SpotifyTrack>();

            SetContent();

            Settings.ThemeChanged += Settings_ThemeChanged;
        }
        public override string Header => "Top & Trending";
        public override string MinimalHeader => "TOP & TRENDING";
        public override bool IsLoading => base.IsLoading;
        public ObservableCollection<SpotifyTrack> TopTracksResults { get; set; }
        public ObservableCollection<SpotifyTrack> TrendingTracksResults { get; set; }
        private IMedia PreviouslyPlayed { get; set; }
        private async void SetContent()
        {
            try
            {
                TopTracksResults.Clear();
            }
            catch
            {

            }

            var topresults = await Settings.SpotifyClient.GetSpotifyTracksFromLastFM(await Auxiliaries.GetTopTracks());
            foreach(var result in topresults)
            {
                TopTracksResults.Add(result);
            }
        }
        private void Settings_ThemeChanged(object sender, EventArgs e)
        {
            RequestedTheme = Settings.Theme;
        }
        private void ST_ClearPreviouslyPlayed()
        {
            try
            {
                if (PreviouslyPlayed is SpotifyTrack track)
                {
                    var container = Top_ResultsListView.ContainerFromItem(track);
                    var mediaplayer = VisualTreeHelper.GetChild(container.RecursiveGetFirstChild(2), 1) as MediaElement;

                    mediaplayer?.Stop();
                    track.IsPlayingPreview = false;
                }
            }
            catch
            {

            }
        }
        private void ST_Play_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button.DataContext as SpotifyTrack;
            var mediaplayer = VisualTreeHelper.GetChild(button.RecursiveGetParent(3), 1) as MediaElement;

            if (!item.IsPlayingPreview)
            {
                ST_ClearPreviouslyPlayed();
                item.IsPlayingPreview = true;
                mediaplayer.Play();

                PreviouslyPlayed = item;
            }
            else
            {
                item.IsPlayingPreview = false;
                mediaplayer.Stop();
            }
        }
        private void ST_Download_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var media = button.DataContext as IMedia;

            InfoHelper.ShowInAppNotification($"Successfully added \"{media.Name}\" to Downloads");
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

            InfoHelper.ShowInAppNotification("Copied to clipboard!");
        }
        private void ST_MoreLikeThis_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as MenuFlyoutItem).DataContext as SpotifyTrack;

            App.SendToRootFrame(typeof(Sub_Pages.MoreLikeThisPage), item);
        }
        private void ST_Tag_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button).DataContext as string;

            App.SendToRootFrame(typeof(Sub_Pages.PopularInTagPage), item);
        }
        private void SA_CollectionsView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var collection = e.ClickedItem as SpotifyAlbum;

            App.SendToRootFrame(typeof(CollectionDetailsPage), collection);
        }
        private void Collections_Download_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuFlyoutItem).DataContext is SpotifyPlaylist playlist)
            {
                InfoHelper.ShowInAppNotification($"Successfully added \"{playlist.Title}\" to Downloads");
                DownloadManager.AddToDownloads(playlist);
            }
            else if ((sender as MenuFlyoutItem).DataContext is SpotifyAlbum album)
            {
                InfoHelper.ShowInAppNotification($"Successfully added \"{album.Title}\" to Downloads");
                DownloadManager.AddToDownloads(album);
            }
        }
        private void Collections_CopyLink(object sender, RoutedEventArgs e)
        {
            var collection = (sender as MenuFlyoutItem).DataContext as IMediaCollection;
            collection.Link.Web.CopyToClipboard();
        }
        private void Top_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Top_ResultsListView.SelectedItem = null;
        }
        private void TopTracksSeeMore_Button_Click(object sender, RoutedEventArgs e)
        {
            if (Top_ResultsListView.Height == 320)
            {
                Top_ResultsListView.Height = Double.NaN;
                //TopTracksSeeMore_TextBlock.Text = "COLLAPSE";
            }
            else
            {
                Top_ResultsListView.Height = 320;
                //TopTracksSeeMore_TextBlock.Text = "SEE MORE";
            }
        }
    }
}
