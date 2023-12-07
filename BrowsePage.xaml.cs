using Melody.Classes;
using Melody.Dialogs;
using Melody.Statics;
using Melody.SubPages;
using Melody.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using YoutubeExplode.Search;
using System.Collections;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Toolkit.Uwp.UI.Animations;
using System.ComponentModel;
using MetaBrainz.MusicBrainz;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Melody
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BrowsePage : BasePage, INotifyPropertyChanged
    {
        public BrowsePage()
        {
            this.InitializeComponent();

            SpotifyResultGrid.Visibility = Visibility.Collapsed;
            YouTubeResultGrid.Visibility = Visibility.Collapsed;

            SpotifyTrackResults = new ObservableCollection<SpotifyTrack>();
            SpotifyPlaylistResults = new ObservableCollection<SpotifyPlaylist>();
            SpotifyAlbumResults = new ObservableCollection<SpotifyAlbum>();
            YouTubeVideoResults = new ObservableCollection<YouTubeVideo>();
            YouTubePlaylistResults = new ObservableCollection<YouTubePlaylist>();

            Settings.ThemeChanged += Settings_ThemeChanged;
        }
        public override string Header => "Browse";
        public override string MinimalHeader => "BROWSE";
        public override bool IsLoading => base.IsLoading;
        public ObservableCollection<SpotifyTrack> SpotifyTrackResults { get; set; }
        public ObservableCollection<SpotifyPlaylist> SpotifyPlaylistResults { get; set; }
        public ObservableCollection<SpotifyAlbum> SpotifyAlbumResults { get; set; }
        public ObservableCollection<YouTubeVideo> YouTubeVideoResults { get; set; }
        public ObservableCollection<YouTubePlaylist> YouTubePlaylistResults { get; set; }
        private IMedia PreviouslyPlayed { get; set; }
        private List<VideoSearchResult> YouTubeVideoSearchResults { get; set; }
        private List<PlaylistSearchResult> YouTubePlaylistSearchResults { get; set; }
        private bool _SpotifyIsLoading { get; set; } = false;
        private bool _YouTubeIsLoading { get; set; } = false;
        private readonly BrowsingParameters BrowsingParameters = new BrowsingParameters(Settings.SpotifyClient, Settings.YouTubeClient, 25, 0);

        public bool SpotifyIsLoading
        {
            get { return _SpotifyIsLoading; }
            set
            {
                _SpotifyIsLoading = value;
                OnPropertyChanged("SpotifyIsLoading");
            }
        }
        public bool YouTubeIsLoading
        {
            get { return _YouTubeIsLoading; }
            set
            {
                _YouTubeIsLoading = value;
                OnPropertyChanged("YouTubeIsLoading");
            }
        }

        private void Settings_ThemeChanged(object sender, EventArgs e)
        {
            RequestedTheme = Settings.Theme;
        }
        private async void SpotifyBrowseTextBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Settings.OutputFolder) || Settings.OutputFolder == "Downloads")
                {
                    InfoHelper.ShowInAppNotification("You have not set a Downloads folder! Please go to the Settings tab and choose a folder");
                    return;
                }
                if (!Settings.SpotifyClient.Authorized)
                {
                    InfoHelper.ShowInAppNotification("You are not authorized! Please go to the Settings tab and \"Authorize Spotify\"");
                    return;
                }
                if (string.IsNullOrWhiteSpace(SpotifyBrowseTextbox.Text))
                {
                    return;
                }
                await GetSpotifyResults(SpotifyBrowseTextbox.Text);
            }
            catch
            {

            }
        }
        private async void YouTubeBrowseTextbox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (string.IsNullOrWhiteSpace(Settings.OutputFolder) || Settings.OutputFolder == "Downloads")
            {
                InfoHelper.ShowInAppNotification("You have not set a Downloads folder! Please go to the Settings tab and choose a folder");
                return;
            }
            if (string.IsNullOrWhiteSpace(YouTubeBrowseTextbox.Text))
            {
                return;
            }
            await GetYouTubeResults(YouTubeBrowseTextbox.Text);
        }
        private async Task GetSpotifyResults(string Query)
        {
            SpotifyIsLoading = true;

            ST_ClearPreviouslyPlayed();
            ClearSpotifyResults();

            if (Utils.IsSpotifyLink(Query))
            {
                GetSpecifiedSpotifyResult(Query);
            }
            else
            {

                var playlistresults = await BrowsingParameters.SpotifyClient.BrowseSpotifyPlaylist(Query, BrowsingParameters.Results, BrowsingParameters.Offset);
                var albumresults = await BrowsingParameters.SpotifyClient.BrowseSpotifyAlbum(Query, BrowsingParameters.Results, BrowsingParameters.Offset);
                var trackresults = await BrowsingParameters.SpotifyClient.BrowseSpotifyTracks(Query, BrowsingParameters.Results, BrowsingParameters.Offset);
                foreach (var playlist in playlistresults)
                {
                    SpotifyPlaylistResults.Add(playlist);
                }
                foreach (var album in albumresults)
                {
                    SpotifyAlbumResults.Add(album);
                }
                foreach (var track in trackresults)
                {
                    SpotifyTrackResults.Add(track);
                }
                GettingSpotifyResultsFinished();
            }
            SpotifyIsLoading = false;
            SpotifyResultGrid.Visibility = Visibility.Visible;
        }
        private async Task GetYouTubeResults(string Query)
        {
            YouTubeIsLoading = true;
            YouTubeVideoResults.Clear();
            YouTubePlaylistResults.Clear();
            ClearYouTubeResults();

            YouTubePlaylistSearchResults = (await BrowsingParameters.YouTubeClient.GetPlaylistSearchResult(Query, BrowsingParameters.Results, BrowsingParameters.Offset)).ToList();
            YouTubeVideoSearchResults = (await BrowsingParameters.YouTubeClient.GetVideoSearchResult(Query, BrowsingParameters.Results, BrowsingParameters.Offset)).ToList();

            DisplayPlaylistResults();
            DisplayVideoResults();
            YouTubeIsLoading = false;
            YouTubeResultGrid.Visibility = Visibility.Visible;
        }
        private async void DisplayPlaylistResults()
        {
            try
            {
                foreach (var result in YouTubePlaylistSearchResults)
                {
                    var playlist = await BrowsingParameters.YouTubeClient.GetPlaylist(result.Url);
                    YouTubePlaylistResults.Add(playlist);
                }
            }
            catch
            {

            }
        }
        private async void DisplayVideoResults()
        {
            try
            {
                foreach (var result in YouTubeVideoSearchResults)
                {
                    var video = await BrowsingParameters.YouTubeClient.GetVideo(result.Url);
                    if (video.DurationAsTimeSpan != TimeSpan.Zero)
                    {
                        YouTubeVideoResults.Add(video);
                    }
                }
            }
            catch
            {

            }
        }
        private async void GetSpecifiedSpotifyResult(string Query)
        {
            var Link = Utils.GetSpotifyLink(Query);

            Debug.WriteLine($"[BrowsePage] Spotify link identified: {Link.Type} | {Link.ID} | {Link.Link}");

            switch (Link.Type)
            {
                case 0://track
                    SpotifyTrackResults.Add(await Settings.SpotifyClient.GetTrack(Link.ID));
                    break;
                case 1://playlist
                    SpotifyPlaylistResults.Add(await Settings.SpotifyClient.GetPlaylist(Link.ID));
                    break;
                case 2://album
                    SpotifyAlbumResults.Add(await Settings.SpotifyClient.GetAlbum(Link.ID));
                    break;
                default:
                    break;
            }
            //Show Spotify results
            SpotifyResultGrid.Visibility = Visibility.Visible;
            GettingSpotifyResultsFinished();
        }
        private void ClearSpotifyResults()
        {
            SpotifyPlaylistResults.Clear();
            SpotifyAlbumResults.Clear();
            SpotifyTrackResults.Clear();
        }
        private void ClearYouTubeResults()
        {
            YouTubeVideoSearchResults?.Clear();
            YouTubePlaylistSearchResults?.Clear();
        }
        private void ST_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ST_ResultsListView.SelectedItem = null;
        }
        private void YT_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            YT_ResultsListView.SelectedItem = null;
        }
        private void YT_Play_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var media = button.DataContext as IMedia;

            App.SendToRootFrame(typeof(YouTubePreviewPage), media, new DrillInNavigationTransitionInfo());
        }
        private void YT_DownloadAudio_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as MenuFlyoutItem;
            var media = button.DataContext as YouTubeVideo;

            media.IsVideo = false;

            InfoHelper.ShowInAppNotification($"Successfully added \"{media.Name} (Audio)\" to Downloads");
            DownloadManager.AddToDownloads(media);
        }
        private async void YT_DownloadVideo_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as MenuFlyoutItem;
            var video = button.DataContext as YouTubeVideo;

            var dialog = new QualitySelectDialog(video)
            {
                RequestedTheme = Settings.Theme
            };
            var dialogresult = await dialog.ShowAsync();

            if (dialogresult == ContentDialogResult.Primary)
            {
                video.IsVideo = true;
                InfoHelper.ShowInAppNotification($"Successfully added \"{video.Name} (Video)\" to Downloads");
                DownloadManager.AddToDownloads(video);
            }
        }
        private async void YT_EditDownload_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as MenuFlyoutItem;
            var media = button.DataContext as IMedia;

            EditTagsDialog dialog = new EditTagsDialog(media)
            {
                RequestedTheme = Settings.Theme
            };

            var dialogresult = await dialog.ShowAsync();
            if (dialogresult == ContentDialogResult.Primary)
            {
                InfoHelper.ShowInAppNotification($"Successfully added \"{media.Name}\" to Downloads");
                DownloadManager.AddToDownloads(media);
            }
        }
        private async void YT_OpenInWeb_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuFlyoutItem;
            var mediaURL = (item.DataContext as YouTubeVideo).Link.Web;
            var mediaUri = new Uri(mediaURL, UriKind.Absolute);

            await Launcher.LaunchUriAsync(mediaUri, new LauncherOptions { FallbackUri = new Uri(@"https://www.youtube.com", UriKind.Absolute) });
        }
        private void YT_CopyLink_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuFlyoutItem;
            var Links = (item.DataContext as YouTubeVideo).Link;
            Links.Web.CopyToClipboard();

            InfoHelper.ShowInAppNotification("Copied to clipboard!");
        }
        private void SpotifyPivotItem_Unloaded(object sender, RoutedEventArgs e)
        {
            ST_ClearPreviouslyPlayed();
        }
        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ST_ClearPreviouslyPlayed();
        }
        private void ST_Play_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button.DataContext as SpotifyTrack;

            Player.Player.Play(item);
        }
        private void ST_ClearPreviouslyPlayed()
        {
            try
            {
                if (PreviouslyPlayed is SpotifyTrack track)
                {
                    var container = ST_ResultsListView.ContainerFromItem(track);
                    var mediaplayer = VisualTreeHelper.GetChild(container.RecursiveGetFirstChild(2), 1) as MediaElement;

                    mediaplayer?.Stop();
                    track.IsPlayingPreview = false;
                }
            }
            catch
            {

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

            MainPage.Current.Navigate(typeof(Sub_Pages.MoreLikeThisPage), item);
        }
        private void ST_Tag_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button).DataContext as string;

            MainPage.Current.Navigate(typeof(Sub_Pages.PopularInTagPage), item);
        }
        private void GettingSpotifyResultsFinished()
        {
            if (SpotifyPlaylistResults.Count == 0)
            {
                SP_Nothing_TextBlock.Visibility = Visibility.Visible;
            }
            else
            {
                SP_Nothing_TextBlock.Visibility = Visibility.Collapsed;
            }

            if (SpotifyAlbumResults.Count == 0)
            {
                SA_Nothing_TextBlock.Visibility = Visibility.Visible;
            }
            else
            {
                SA_Nothing_TextBlock.Visibility = Visibility.Collapsed;
            }

            if (SpotifyTrackResults.Count == 0)
            {
                ST_Nothing_TextBlock.Visibility = Visibility.Visible;
            }
            else
            {
                ST_Nothing_TextBlock.Visibility = Visibility.Collapsed;
            }
        }
        private void SP_CollectionsView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var collection = e.ClickedItem as SpotifyPlaylist;

            MainPage.Current.Navigate(typeof(CollectionDetailsPage), collection);
        }
        private void SA_CollectionsView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var collection = e.ClickedItem as SpotifyAlbum;

            MainPage.Current.Navigate(typeof(CollectionDetailsPage), collection);
        }
        private void SA_CollectionsView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SA_CollectionsView.SelectedItem = null;
        }
        private void SP_CollectionsView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SP_CollectionsView.SelectedItem = null;
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
        private void TagsDataChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            var repeater = sender as ItemsRepeater;
            var storyboard = repeater.Resources["StoryboardAnimation"] as Storyboard;
            storyboard.Seek(TimeSpan.Zero);
            storyboard.Begin();
        }

        private void YT_CollectionResultsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(YT_CollectionResultsListView.SelectedItem != null)
            {
                var element = sender as ListView;
                var collection = element.SelectedItem as YouTubePlaylist;

                YT_CollectionResultsListView.SelectedItem = null;
                MainPage.Current.Navigate(typeof(CollectionDetailsPage), collection);
            }
            else
            {
                return;
            }  
        }

        private void SpotifyBrowseTextbox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                //sender.ItemsSource = Settings.SearchHistory;
            }
        }
    }
}