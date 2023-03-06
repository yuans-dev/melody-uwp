using Melody.Statics;
using Melody.Core;
using System;
using Windows.Media.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Melody.SubPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class YouTubePreviewPage : Page
    {
        public YouTubePreviewPage()
        {
            this.InitializeComponent();
            RequestedTheme = Settings.Theme;
        }
        public IMedia PreviewSource { get; set; }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            PreviewSource = (IMedia)e.Parameter;

            PreviewAsync();
        }
        private async void PreviewAsync()
        {
            var youtube = new YoutubeClient();
            try
            {
                var streamManifest = await youtube.Videos.Streams.GetManifestAsync(PreviewSource.ID.ID);
                var streamInfo = streamManifest.GetMuxedStreams().GetWithHighestVideoQuality();

                Player.Source = MediaSource.CreateFromUri(new Uri(streamInfo.Url));
                Player.AutoPlay = true;
                Player.MediaPlayer.Volume = 0.25;
            }
            catch
            {
                OnError();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Player.MediaPlayer.Pause();

            var button = sender as Button;
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.GoBack();
            button.IsEnabled = false;
        }
        private void OnError()
        {
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.GoBack();

            InfoHelper.ShowInAppNotification("Cannot play video!");
        }
    }
}
