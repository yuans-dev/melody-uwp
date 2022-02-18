﻿using Media_Downloader_App.Dialogs;
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
    public sealed partial class BrowsePage : Page
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

            TokenSource = new CancellationTokenSource();

            Settings.ThemeChanged += Settings_ThemeChanged;
        }
        public ObservableCollection<SpotifyTrack> SpotifyTrackResults { get; set; }
        public ObservableCollection<SpotifyPlaylist> SpotifyPlaylistResults { get; set; }
        public ObservableCollection<SpotifyAlbum> SpotifyAlbumResults { get; set; }
        public ObservableCollection<YouTubeVideo> YouTubeResults { get; set; }
        private List<VideoSearchResult> YouTubeSearchResult { get; set; }
        private CancellationTokenSource TokenSource { get; set; }

        private void Settings_ThemeChanged(object sender, EventArgs e)
        {
            RequestedTheme = Settings.Theme;
        }
        private async void BrowseTextBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            try
            {
                if (Settings.SpotifyClient.Authd)
                {
                    if (!string.IsNullOrWhiteSpace(BrowseTextbox.Text))
                    {
                        await GetResults(BrowseTextbox.Text);
                    }
                }
                else
                {
                    InfoHelper.ShowInAppNotification("You are not authorized! Please go to the Settings tab and \"Authorize Spotify\"");
                }
            }
            catch
            {

            }
        }
        private async Task GetResults(string Query)
        {
            var p = new PagingOptions(Settings.SpotifyClient, Settings.YouTubeClient, Query, 25, 0);
            BrowseProgressBar.Visibility = Visibility.Visible;

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
                YouTubeSearchResult = (await p.YouTubeClient.GetVideoSearchResult(p.Query, p.Results, p.Offset)).ToList();
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

                YouTubeResults.Clear();
                foreach(var result in YouTubeSearchResult)
                {
                    YouTubeResults.Add(await p.YouTubeClient.GetVideo(result.Url));
                    YouTubeResultGrid.Visibility = Visibility.Visible;
                }
            }
            BrowseProgressBar.Visibility = Visibility.Collapsed;
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

            var IsSuccess = await Launcher.LaunchUriAsync(mediaUri, new LauncherOptions { FallbackUri = new Uri(@"https://www.youtube.com", UriKind.Absolute) });


        }

        private void ST_Play_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var icon = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(button, 0), 0) as SymbolIcon;
            var mediaplayer = VisualTreeHelper.GetChild(DependencyObjectHelper.RecursiveGetParent(button, 3), 1) as MediaElement;
            var textblock = VisualTreeHelper.GetChild(VisualTreeHelper.GetParent(button), 2) as TextBlock;

            if (icon.Symbol == Symbol.Play)
            {
                textblock.Visibility = Visibility.Visible;
                mediaplayer.Play();
                icon.Symbol = Symbol.Pause;
            }
            else
            {
                textblock.Visibility = Visibility.Collapsed;
                mediaplayer.Pause();
                icon.Symbol = Symbol.Play;
            }
        }

        private void ST_Download_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var media = button.DataContext as IMedia;

            InfoHelper.ShowInAppNotification($"Successfully added \"{media.Name}\" to Downloads");
            MainPage.Current.AddToDownloads(media);
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

        private void SP_Download_Click(object sender, RoutedEventArgs e)
        {
            var collection = (sender as MenuFlyoutItem).DataContext as SpotifyPlaylist;

            InfoHelper.ShowInAppNotification($"Successfully added \"{collection.Title}\" to Downloads");
            MainPage.Current.AddToDownloads(collection, false);
        }
        private void SP_BgDownload_Click(object sender, RoutedEventArgs e)
        {
            var collection = (sender as MenuFlyoutItem).DataContext as SpotifyPlaylist;

            InfoHelper.ShowInAppNotification($"Successfully added \"{collection.Title}\" to Downloads");
            MainPage.Current.AddToDownloads(collection,true);
        }
        private void SA_Download_Click(object sender, RoutedEventArgs e)
        {
            var collection = (sender as MenuFlyoutItem).DataContext as SpotifyAlbum;

            InfoHelper.ShowInAppNotification($"Successfully added \"{collection.Title}\" to Downloads");
            MainPage.Current.AddToDownloads(collection, false);
        }
        private void SA_BgDownload_Click(object sender, RoutedEventArgs e)
        {
            var collection = (sender as MenuFlyoutItem).DataContext as SpotifyAlbum;

            InfoHelper.ShowInAppNotification($"Successfully added \"{collection.Title}\" to Downloads");
            MainPage.Current.AddToDownloads(collection, true);
        }
    }
}
