using Melody.Statics;
using Melody.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
using Windows.UI.Xaml.Navigation;
using Melody.Classes;
using Hqub.Lastfm.Entities;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Melody.Sub_Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PopularInTagPage : BasePage
    {
        public PopularInTagPage()
        {
            this.InitializeComponent();

            RequestedTheme = Settings.Theme;
        }
        private SpotifyTrack PreviouslyPlayed { get; set; }
        public override string Label { get; set; }
        public override string LoadingText => "SEARCHING";
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            IsLoading = true;
            if (e.Parameter is string tag)
            {
                Label = $"Popular in \"{tag}\":";

                var UniqueClient = new Spotify();
                UniqueClient.Details.ID = Settings.SpotifyClient.Details.ID;
                UniqueClient.Details.Secret = Settings.SpotifyClient.Details.Secret;
                UniqueClient.SpotifyTracksFromLastFMProgressChanged += SpotifyClient_SpotifyTracksFromLastFMProgressChanged;
                await UniqueClient.Auth();

                try
                {
                    ResultsListView.ItemsSource = await UniqueClient.GetSpotifyTracksFromLastFM(await Auxiliaries.GetTopTracksInTag(tag, 30));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[PopularInTagPage] {ex.Message}");
                }
            }
            IsLoading = false;
        }
        private void SpotifyClient_SpotifyTracksFromLastFMProgressChanged(object sender, CollectionProgressEventArgs e)
        {
            LoadingProgress = (double)e.Finished / (double)e.Total;
        }
        private void ST_Play_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button.DataContext as SpotifyTrack;

            Player.Player.Play(item);
        }
        private void ST_Download_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var media = button.DataContext as IMedia;

            InfoHelper.ShowInAppNotification($"Successfully added \"{media.Name}\" to Downloads", InAppNotif);
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
        private void ST_Tag_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button).DataContext as string;

            MainPage.Current.Navigate(typeof(PopularInTagPage), item);
        }
    }
}
