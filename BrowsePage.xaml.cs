using Media_Downloader_App.Classes;
using Media_Downloader_App.Dialogs;
using Media_Downloader_App.Statics;
using Media_Downloader_App.SubPages;
using Microsoft.Toolkit.Uwp;
using MP3DL.Media;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using YoutubeExplode.Search;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Media_Downloader_App
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BrowsePage : BasePage
    {
        public BrowsePage()
        {
            this.InitializeComponent();

            SpotifyResultGrid.Visibility = Visibility.Collapsed;
            YouTubeResultGrid.Visibility = Visibility.Collapsed;

            SpotifyTrackResults = new ObservableCollection<SpotifyTrack>();
            SpotifyPlaylistResults = new ObservableCollection<SpotifyPlaylist>();
            SpotifyAlbumResults = new ObservableCollection<SpotifyAlbum>();
            YouTubeResults = new ObservableCollection<YouTubeVideo>();

            ST_ResultsListView.ItemsSource = SpotifyTrackResults;
            SP_ResultsFlipView.ItemsSource = SpotifyPlaylistResults;
            SA_ResultsFlipView.ItemsSource = SpotifyAlbumResults;
            YT_ResultsListView.ItemsSource = YouTubeResults;

            Settings.ThemeChanged += Settings_ThemeChanged;
        }
        public override string Header => "Browse";
        public override string MinimalHeader => "BROWSE";
        public ObservableCollection<SpotifyTrack> SpotifyTrackResults { get; set; }
        public ObservableCollection<SpotifyPlaylist> SpotifyPlaylistResults { get; set; }
        public ObservableCollection<SpotifyAlbum> SpotifyAlbumResults { get; set; }
        public ObservableCollection<YouTubeVideo> YouTubeResults { get; set; }
        private IMedia PreviouslyPlayed { get; set; } 
        private List<VideoSearchResult> YouTubeSearchResult { get; set; }

        private void Settings_ThemeChanged(object sender, EventArgs e)
        {
            RequestedTheme = Settings.Theme;
        }
        private async void BrowseTextBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            try
            {
                if (Settings.SpotifyClient.Authd && (Settings.OutputFolder != "Downloads" && !string.IsNullOrWhiteSpace(Settings.OutputFolder)))
                {
                    if (!string.IsNullOrWhiteSpace(BrowseTextbox.Text))
                    {
                        await GetResults(BrowseTextbox.Text);
                    }
                }
                else if(!Settings.SpotifyClient.Authd)
                {
                    InfoHelper.ShowInAppNotification("You are not authorized! Please go to the Settings tab and \"Authorize Spotify\"");
                }else if (string.IsNullOrWhiteSpace(Settings.OutputFolder) || Settings.OutputFolder == "Downloads")
                {
                    InfoHelper.ShowInAppNotification("You have not set a Downloads folder! Please go to the Settings tab and choose a folder");
                }
            }
            catch
            {

            }
        }
        private async Task GetResults(string Query)
        {
            var p = new PagingOptions(Settings.SpotifyClient, Settings.YouTubeClient, Query, 25, 0);
            LoadingControl.IsLoading = true;

            ST_ClearPreviouslyPlayed();
            ClearResults();
            if (Utils.IsSpotifyLink(Query))
            {
                GetSpecifiedResult(Query);
            }
            else
            {

                var playlistresults = await p.SpotifyClient.BrowseSpotifyPlaylist(p.Query, p.Results, p.Offset);
                var albumresults = await p.SpotifyClient.BrowseSpotifyAlbum(p.Query, p.Results, p.Offset);
                var trackresults = await p.SpotifyClient.BrowseSpotifyTracks(p.Query, p.Results, p.Offset);
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
                SpotifyResultGrid.Visibility = Visibility.Visible;
            }
            YouTubeSearchResult = (await p.YouTubeClient.GetVideoSearchResult(p.Query, p.Results, p.Offset)).ToList();
            YouTubeResults.Clear();
            LoadingControl.IsLoading = false;
            foreach (var result in YouTubeSearchResult)
            {
                var video = await p.YouTubeClient.GetVideo(result.Url);
                if (video.DurationAsTimeSpan != TimeSpan.Zero)
                {
                    YouTubeResults.Add(video);
                    YouTubeResultGrid.Visibility = Visibility.Visible;
                }
            }
        }
        private void ClearResults()
        {
            YouTubeSearchResult?.Clear();
            SpotifyPlaylistResults.Clear();
            SpotifyAlbumResults.Clear();
            SpotifyTrackResults.Clear();
        }
        private async void GetSpecifiedResult(string Query)
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
            //Show Spotify esults
            SpotifyResultGrid.Visibility = Visibility.Visible;
            GettingSpotifyResultsFinished();
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
            MainPage.Current.AddToDownloads(media);
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
                MainPage.Current.AddToDownloads(video);
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
                MainPage.Current.AddToDownloads(media);
            }
        }
        private async void YT_OpenInWeb_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var mediaURL = "https://www.youtube.com/watch?v=" + (button.DataContext as YouTubeVideo).ID;
            var mediaUri = new Uri(mediaURL, UriKind.Absolute);

            await Launcher.LaunchUriAsync(mediaUri, new LauncherOptions { FallbackUri = new Uri(@"https://www.youtube.com", UriKind.Absolute) });
        }
        private void YT_CopyLink_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var Links = (button.DataContext as YouTubeVideo).Link;
            ClipboardExtensions.CopyToClipboard(Links.Web);
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
            var mediaplayer = VisualTreeHelper.GetChild(DependencyObjectHelper.RecursiveGetParent(button,3),1) as MediaElement;

            if (item.Symbol == Symbol.Play)
            {
                ST_ClearPreviouslyPlayed();
                item.IsPlayingPreview = true;
                mediaplayer.Play();
                item.Symbol = Symbol.Pause;

                PreviouslyPlayed = item;
            }
            else
            {
                item.IsPlayingPreview = false;
                mediaplayer.Stop();
                item.Symbol = Symbol.Play;
            }
        }
        private void ST_ClearPreviouslyPlayed()
        {
            try
            {
                if (PreviouslyPlayed is SpotifyTrack track)
                {
                    var container = ST_ResultsListView.ContainerFromItem(track);
                    var mediaplayer = VisualTreeHelper.GetChild(DependencyObjectHelper.RecursiveGetFirstChild(container, 2), 1) as MediaElement;

                    mediaplayer?.Stop();
                    track.IsPlayingPreview = false;
                    track.Symbol = Symbol.Play;
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
            MainPage.Current.AddToDownloads(media);
        }
        private async void ST_OpenInWeb_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var track = (button.DataContext as SpotifyTrack);

            ContentDialog dialog = new ContentDialog()
            {
                Title = "Opening...",
                Content = "Would you like to open it in the app?",
                PrimaryButtonText = "Open in app",
                SecondaryButtonText = "Open in web",
                CloseButtonText = "Cancel"
            };
            switch(await dialog.ShowAsync())
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
            var button = sender as Button;
            var mediaLinks = (button.DataContext as SpotifyTrack).Link;
            ClipboardExtensions.CopyToClipboard(mediaLinks.Web);
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
        private void SP_ResultsFlipView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var collection = e.ClickedItem as SpotifyPlaylist;

            App.SendToRootFrame(typeof(CollectionDetailsPage), collection);
        }
        private void SA_ResultsFlipView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var collection = e.ClickedItem as SpotifyAlbum;

            App.SendToRootFrame(typeof(CollectionDetailsPage), collection);
        }

        private void SA_ResultsFlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SA_ResultsFlipView.SelectedItem = null;
        }

        private void SP_ResultsFlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SP_ResultsFlipView.SelectedItem = null;
        }

        private void Collections_Download_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuFlyoutItem).DataContext is SpotifyPlaylist playlist)
            {
                InfoHelper.ShowInAppNotification($"Successfully added \"{playlist.Title}\" to Downloads");
                MainPage.Current.AddToDownloads(playlist, false);
            }
            else if ((sender as MenuFlyoutItem).DataContext is SpotifyAlbum album)
            {
                InfoHelper.ShowInAppNotification($"Successfully added \"{album.Title}\" to Downloads");
                MainPage.Current.AddToDownloads(album, false);
            }
        }
        private void Collections_BgDownload_Click(object sender, RoutedEventArgs e)
        {
            if((sender as MenuFlyoutItem).DataContext is SpotifyPlaylist playlist)
            {
                InfoHelper.ShowInAppNotification($"Successfully added \"{playlist.Title}\" to Downloads");
                MainPage.Current.AddToDownloads(playlist, true);
            }else if((sender as MenuFlyoutItem).DataContext is SpotifyAlbum album)
            {
                InfoHelper.ShowInAppNotification($"Successfully added \"{album.Title}\" to Downloads");
                MainPage.Current.AddToDownloads(album, true);
            }
        }

        private void Collections_CopyLink(object sender, RoutedEventArgs e)
        {
            var collection = (sender as MenuFlyoutItem).DataContext as IMediaCollection;
            ClipboardExtensions.CopyToClipboard(collection.Link.Web);
        }
    }
}