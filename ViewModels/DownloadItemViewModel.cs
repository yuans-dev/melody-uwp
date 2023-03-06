using Melody.Statics;
using Melody.Core;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using System.IO;
using Windows.ApplicationModel.ExtendedExecution;

namespace Melody.ViewModels
{
    public class DownloadItemViewModel : INotifyPropertyChanged, IDownloadItem
    {
        public DownloadItemViewModel(IMedia Media, bool WillSendNotifs = true, string OutputFolderExtension = "")
        {
            StatusGlyph = Glyphs.WaitingGlyph;
            this.Media = Media;
            Bitmap = Media.Bitmap;
            Title = Media.Title;
            Authors = Media.Authors;
            this.WillSendNotifs = WillSendNotifs;
            OutputPath = Path.Combine(Settings.OutputFolder, OutputFolderExtension);

            CancellationTokenSource = new CancellationTokenSource();
            HasNotStarted = true;
        }
        public StorageFile OutputFile { get; private set; }
        private string OutputPath { get; set; }
        private Downloader Downloader { get; set; }
        private CancellationTokenSource CancellationTokenSource { get; set; }
        public IMedia Media { get; set; }
        private bool WillSendNotifs { get; set; }
        private int _ProgressValue { get; set; }
        public int ProgressValue
        {
            get { return _ProgressValue; }
            set
            {
                _ProgressValue = value;
                OnPropertyChanged("ProgressValue");
            }
        }
        private string _Status { get; set; }
        public string Status
        {
            get { return _Status; }
            set
            {
                _Status = value;
                OnPropertyChanged("Status");
            }
        }
        private string _StatusGlyph { get; set; }
        public string StatusGlyph
        {
            get { return _StatusGlyph; }
            set
            {
                _StatusGlyph = value;
                OnPropertyChanged("StatusGlyph");
            }
        }
        public Visibility ListVisibility { get; set; } = Visibility.Collapsed;
        private bool _HasNotStarted { get; set; }
        public bool HasNotStarted
        {
            get { return _HasNotStarted; }
            set
            {
                _HasNotStarted = value;
                OnPropertyChanged("HasNotStarted");
            }
        }

        public Windows.UI.Xaml.Media.Imaging.BitmapImage Bitmap { get; set; }

        public string Title { get; set; }

        public string[] Authors { get; set; }

        public void InitDownloader()
        {
            Downloader = new Downloader() { OutputPath = OutputPath, CancelToken = CancellationTokenSource.Token };
            Downloader.ProgressChanged += Downloader_ProgressChanged;
            Downloader.DownloadCompleted += Downloader_DownloadCompleted;
        }
        public async Task StartDownload()
        {
            var downloadsession = new ExtendedExecutionSession();
            downloadsession.Reason = ExtendedExecutionReason.Unspecified;
            downloadsession.Revoked += SessionRevoked;
            ExtendedExecutionResult result = await downloadsession.RequestExtensionAsync();
            switch (result)
            {
                case ExtendedExecutionResult.Allowed:
                    InitDownloader();
                    StatusGlyph = Glyphs.CancelGlyph;
                    try
                    {
                        await Downloader.DownloadMedia(Media);
                    }
                    catch (Exception ex)
                    {
                        StatusGlyph = Glyphs.RetryGlyph;
                        System.Diagnostics.Debug.WriteLine($"An exception occurred : {ex.Message}");
                        //throw ex; //For debug
                    }
                    break;

                default:
                case ExtendedExecutionResult.Denied:
                    CancelDownload();
                    break;
            }
            downloadsession.Revoked -= SessionRevoked;
            downloadsession.Dispose();
            downloadsession = null;
        }

        private void SessionRevoked(object sender, ExtendedExecutionRevokedEventArgs args)
        {
            CancelDownload();
        }

        public void CancelDownload()
        {
            StatusGlyph = Glyphs.RetryGlyph;
            CancellationTokenSource.Cancel();
            CancellationTokenSource = new CancellationTokenSource();
        }
        private void Downloader_ProgressChanged(object sender, DownloadProgressEventArgs e)
        {
            HasNotStarted = false;
            var percentage = (int)(e.Progress * 100);
            Status = $"{e.Status}... {percentage}%";
            ProgressValue = percentage;
        }
        private void Downloader_DownloadCompleted(object sender, DownloadCompleteEventArgs e)
        {
            switch (e.Result)
            {
                case Result.Success:
                    Status = "Completed.";
                    StatusGlyph = Glyphs.CheckGlyph;
                    OutputFile = e.OutputFile;
                    break;
                case Result.FailedRequest:
                    Status = "No response from the server! Please try again.";
                    StatusGlyph = Glyphs.RetryGlyph;
                    break;
                case Result.NoMediaFound:
                    Status = "We didn't find any matching YouTube video for this song. Please try downloading from YouTube.";
                    StatusGlyph = Glyphs.RetryGlyph;
                    break;
                case Result.NotDetermined:
                    Status = $"An unknown error occured: {e.ExceptionMessage}.";
                    StatusGlyph = Glyphs.RetryGlyph;
                    break;
                case Result.Cancelled:
                    Status = "Cancelled.";
                    StatusGlyph = Glyphs.RetryGlyph;
                    break;
                case Result.Other:
                    Status = $"{e.ExceptionMessage}";
                    StatusGlyph = Glyphs.RetryGlyph;
                    break;
                default:
                    Status = $"{e.Result}";
                    StatusGlyph = Glyphs.RetryGlyph;
                    break;
            }

            if (WillSendNotifs && e.Result == Result.Success)
            {
                InfoHelper.ShowNotification($"You have successfully finished downloading \"{Media.Name}\"", "Download completed", Bitmap.UriSource);
            }
            else if (WillSendNotifs && e.Result != Result.Success)
            {
                InfoHelper.ShowNotification($"An error occured while downloading \"{Media.Name}\"", "Download error", Bitmap.UriSource);
            }
        }
        public override string ToString()
        {
            if (Media is YouTubeVideo)
            {
                if (Media.IsVideo)
                {
                    return "VIDEO";
                }
                else
                {
                    return "AUDIO ONLY";
                }
            }
            else if (Media is SpotifyTrack)
            {
                return "TRACK";
            }
            else
            {
                return "OTHER";
            }
        }
        protected virtual void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
