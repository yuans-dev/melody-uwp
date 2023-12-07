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
        public static async void Play (SpotifyTrack item)
        {
            SetInfo(item);
            Show();
            if (DefaultMediaElement.MediaPlayer.PlaybackSession.CanPause)
            {
                DefaultMediaElement.MediaPlayer.Pause();
            }
            try
            {
                DefaultMediaElement.Source = MediaSource.CreateFromUri(new Uri(item.PreviewURL, UriKind.Absolute));
            }
            catch
            {
                DefaultMediaElement.Source = MediaSource.CreateFromUri(new Uri(await TryGetPreviewURLFromYT(item), UriKind.Absolute));
            }
            
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
            PlayerGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }
        public static void Hide()
        {
            PlayerGrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            if (DefaultMediaElement.MediaPlayer.PlaybackSession.CanPause)
            {
                DefaultMediaElement.MediaPlayer.Pause();
            }
        }
        private static async Task<string> TryGetPreviewURLFromYT(SpotifyTrack track)
        {
            var ytlink = await ToYouTubeLink(track);

            var streaminfo = await Settings.YouTubeClient.GetStreamInfo(ytlink);
            return streaminfo.Url;
        }
        private static async Task<string> ToYouTubeLink(SpotifyTrack Track)
        {
            string title = await Track.Title.Romanize();
            string[] keywords = new string[2] { title, Track.Title };
            int[] errormargins = new int[6] { 0, 1000, 2000, 4000, 8000, 12000 };
            var i = 1;
            foreach (var keyword in keywords)
            {
                foreach (var margin in errormargins)
                {
                    var result = await Settings.YouTubeClient.Search($"{title.ToLower()} {Track.Authors[0].ToLower()}", keyword, Track.Duration, margin);
                    if (!string.IsNullOrWhiteSpace(result))
                    {
                        Settings.YouTubeClient.InitializeURL(result);
                        return result;
                    }
                }
            }
            Debug.WriteLine($"{title.ToLower()} {Track.Authors[0].ToLower()} official");
            var fuzzyresult = await Settings.YouTubeClient.FuzzySearch($"{title.ToLower()} {Track.Authors[0].ToLower()} official", Track.Duration);
            Settings.YouTubeClient.InitializeURL(fuzzyresult);
            return fuzzyresult;
        }
    }
}
