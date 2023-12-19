using Melody.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Melody.Statics;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics;

namespace Melody.Player
{
    public static class Player
    {
        private static MediaPlayerElement DefaultMediaElement = MainPage.Current.DefaultMediaElement;
        private static Grid PlayerGrid = MainPage.Current.PlayerGrid;
        private static RowDefinition PlayerGridRow = MainPage.Current.PlayerGridRow;
        public static async void Play (SpotifyTrack item)
        {
            Show();
            SetInfo(item);
            DefaultMediaElement.IsEnabled = false;
            if (DefaultMediaElement.MediaPlayer.PlaybackSession.CanPause)
            {
                DefaultMediaElement.MediaPlayer.Pause();
            }
            DefaultMediaElement.Source = MediaSource.CreateFromUri(new Uri(await TryGetPreviewURLFromYT(item), UriKind.Absolute));
            DefaultMediaElement.IsEnabled = true;
            DefaultMediaElement.MediaPlayer.Volume = 0.25;
        }
        public static void SetInfo(SpotifyTrack item)
        {
            MainPage.Current.InfoImage.Source = item.Bitmap;
            MainPage.Current.InfoArtist.Text = item.Authors.ToString(", ");
            MainPage.Current.InfoTitle.Text = item.Title;
            MainPage.Current.PlayerDownloadButton.DataContext = item;
        }
        public static void Show()
        {
            PlayerGridRow.Height = new Windows.UI.Xaml.GridLength(170,Windows.UI.Xaml.GridUnitType.Pixel);
            PlayerGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }
        public static void Hide()
        {
            PlayerGridRow.Height = new Windows.UI.Xaml.GridLength(0, Windows.UI.Xaml.GridUnitType.Pixel);
            PlayerGrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            if (DefaultMediaElement.MediaPlayer.PlaybackSession.CanPause)
            {
                DefaultMediaElement.MediaPlayer.Pause();
            }
        }
        private static async Task<string> TryGetPreviewURLFromYT(SpotifyTrack track)
        {
            var ytlink = await Settings.YouTubeClient.ToYouTubeLink(track);

            var streaminfo = await Settings.YouTubeClient.GetStreamInfo(ytlink);
            return streaminfo.Url;
        }
    }
}
