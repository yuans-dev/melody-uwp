﻿using Melody.ViewModels;
using Melody.Core;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;
using YoutubeExplode;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Melody.Dialogs
{
    public partial class QualitySelectDialog : ContentDialog
    {
        public QualitySelectDialog(YouTubeVideo Video)
        {
            this.InitializeComponent();

            Client = new YoutubeClient();
            StreamInfosList = new ObservableCollection<StreamInfoViewModel>();

            StreamInfosListView.ItemsSource = StreamInfosList;

            this.Video = Video;
            GetStreamInfos();
        }
        private YouTubeVideo Video { get; set; }
        private YoutubeClient Client { get; set; }
        private ObservableCollection<StreamInfoViewModel> StreamInfosList { get; set; }
        public async void GetStreamInfos()
        {
            try
            {
                var manifest = await Client.Videos.Streams.GetManifestAsync(Video.ID.ID);
                var streaminfos = manifest.GetVideoStreams();

                foreach (var streaminfo in streaminfos)
                {
                    string type;
                    if (streaminfo.ToString().Contains("Muxed"))
                    {
                        type = "Video and Audio";
                    }
                    else
                    {
                        type = "Video-only";
                    }
                    string format = $"{streaminfo.Container}";
                    string quality = $"{streaminfo.VideoQuality.MaxHeight}p@{streaminfo.VideoQuality.Framerate}fps";

                    StreamInfosList.Add(new StreamInfoViewModel { StreamInfo = streaminfo, StreamInfoDisplay = $"{type} ({format}) | {quality}" });
                }
            }
            catch
            {
            }
        }

        private void StreamInfosListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StreamInfosListView.SelectedItem != null)
            {
                CurrentlySelectedTextBlock.Text = (StreamInfosListView.SelectedItem as StreamInfoViewModel).StreamInfoDisplay;
                IsPrimaryButtonEnabled = true;
            }
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Video.RequestedVideoQuality = (StreamInfosListView.SelectedItem as StreamInfoViewModel).StreamInfo;
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }
    }
}
