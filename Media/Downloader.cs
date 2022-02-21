using Media_Downloader_App.Statics;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Converter;
using YoutubeExplode.Search;
using YoutubeExplode.Videos.Streams;

namespace MP3DL.Media
{
    public enum Result
    {
        Success,
        DuplicateFile,
        NoMediaFound,
        Cancelled,
        FailedRequest,
        FFMPEGNotFound,
        NotDetermined
    }
    public class Downloader
    {
        public Downloader()
        {
            this.OutputPath = "Downloads";
            CancelToken = new CancellationTokenSource().Token;
            Client = new YoutubeClient();
        }
        public event EventHandler<DownloadCompleteEventArgs> DownloadCompleted;
        public event EventHandler<DownloadProgressEventArgs> ProgressChanged;
        private YoutubeClient Client;
        public CancellationToken CancelToken;
        private double _Progress { get; set; }
        private double Progress
        {
            get { return _Progress; }
            set
            {
                _Progress = value;
                OnProgressChanged();
            }
        }
        private string _Status { get; set; }
        private string Status
        {
            get { return _Status; }
            set
            {
                _Status = value;
                OnProgressChanged();
            }
        }
        private IMedia CurrentlyDownloading { get; set; }
        private StorageFolder OutputFolder { get; set; }
        public string OutputPath { get; set; }
        public async Task DownloadMedia(IMedia Media)
        {
            if (Media is SpotifyTrack Track)
            {
                await DownloadMedia(Track);
            }
            else if (Media is YouTubeVideo Video)
            {
                await DownloadMedia(Video);
            }
        }
        public async Task DownloadMedia(SpotifyTrack Track)
        {
            CurrentlyDownloading = Track;
            Client = new YoutubeClient();
            string CleanFilename = Utils.ClearChars(Track.Name);

            Status = "Searching";
            var SearchResult = await SpotifyToYouTube(Track);
            if (string.IsNullOrWhiteSpace(SearchResult))
            {
                OnDownloadCompleted(Result.NoMediaFound);
                return;
            }

            OutputFolder = await StorageFolder.GetFolderFromPathAsync(OutputPath);
            var manifest = await Client.Videos.Streams.GetManifestAsync(SearchResult);
            var streaminfo = manifest.GetAudioOnlyStreams().GetWithHighestBitrate();

            var OutputFile = await OutputFolder.CreateFileAsync($"{CleanFilename}.{streaminfo.Container}", CreationCollisionOption.ReplaceExisting);

            try
            {
                Status = "Downloading";
                Debug.WriteLine($"[DOWNLOADER] Downloading {streaminfo.Url}");

                var stream = await Client.Videos.Streams.GetAsync(streaminfo, CancelToken);

                var Progress = new Progress<long>(p => this.Progress = (double)p / (double) stream.Length);
                var filestream = await OutputFile.OpenStreamForWriteAsync();
                filestream.Seek(0, SeekOrigin.Begin);
                await stream.CopyToAsync(filestream, 81920, Progress,CancelToken);


                stream.Dispose();
                filestream.Dispose();
            }
            catch (System.Net.Http.HttpRequestException)
            {
                OnDownloadCompleted(Result.FailedRequest);
                return;
            }
            catch (TaskCanceledException)
            {
                if (File.Exists(OutputFile.Path))
                {
                    await OutputFile.DeleteAsync();
                }
                OnDownloadCompleted(Result.Cancelled);
                return;
            }
            catch (OperationCanceledException)
            {
                if (File.Exists(OutputFile.Path))
                {
                    await OutputFile.DeleteAsync();
                }
                OnDownloadCompleted(Result.Cancelled);
                return;
            }
            catch (Exception ex)
            {
                OnDownloadCompleted(Result.NotDetermined, ex.Message);
                return;
            }
            var newfile = await ConvertToMP3(OutputFile);
            Track.SetTagsAsync(newfile);

            Debug.WriteLine("[DOWNLOADER] Finished");

            OnDownloadCompleted(Result.Success, newfile);
            return;
        }
        public async Task DownloadMedia(YouTubeVideo Video)
        {
            CurrentlyDownloading = Video;
            Client = new YoutubeClient();
            string CleanFilename = Utils.ClearChars(Video.Name);

            OutputFolder = await StorageFolder.GetFolderFromPathAsync(OutputPath);

            var manifest = await Client.Videos.Streams.GetManifestAsync(Video.ID);

            IStreamInfo streaminfo;

            if (Video.IsVideo)
            {
                if (Video.RequestedVideoQuality is null)
                {
                    streaminfo = manifest.GetVideoOnlyStreams().GetWithHighestVideoQuality();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[DOWNLOADER] Requested quality is {Video.RequestedVideoQuality}");
                    streaminfo = Video.RequestedVideoQuality;
                }
                System.Diagnostics.Debug.WriteLine($"[DOWNLOADER] Downloading at {streaminfo}");
            }
            else
            {
                streaminfo = manifest.GetAudioOnlyStreams().GetWithHighestBitrate();
            }

            var OutputFile = await OutputFolder.CreateFileAsync($"{CleanFilename}.{streaminfo.Container}", CreationCollisionOption.OpenIfExists);

            try
            {
                Status = "Downloading";

                Debug.WriteLine($"[DOWNLOADER] Downloading {streaminfo.Url}");

                var stream = await Client.Videos.Streams.GetAsync(streaminfo, CancelToken);

                var Progress = new Progress<long>(p => this.Progress = (double)p / (double)stream.Length);
                var filestream = await OutputFile.OpenStreamForWriteAsync();
                filestream.Seek(0, SeekOrigin.Begin);
                await stream.CopyToAsync(filestream, 81920, Progress, CancelToken);

                stream.Dispose();
                filestream.Dispose();
            }
            catch (System.Net.Http.HttpRequestException)
            {
                OnDownloadCompleted(Result.FailedRequest);
                return;
            }
            catch (TaskCanceledException)
            {
                if (File.Exists(OutputFile.Path))
                {
                    await OutputFile.DeleteAsync();
                }
                OnDownloadCompleted(Result.Cancelled);
                return;
            }
            catch (OperationCanceledException)
            {
                if (File.Exists(OutputFile.Path))
                {
                    await OutputFile.DeleteAsync();
                }
                OnDownloadCompleted(Result.Cancelled);
                return;
            }
            catch (Exception ex)
            {
                OnDownloadCompleted(Result.NotDetermined, ex.Message);
                return;
            }

            if (!Video.IsVideo)
            {
                var newfile = await ConvertToMP3(OutputFile);
                Video.SetTagsAsync(newfile);
                OnDownloadCompleted(Result.Success, newfile);
            }
            else
            {
                OnDownloadCompleted(Result.Success, OutputFile);
            }

            Debug.WriteLine("[DOWNLOADER] Finished");
            return;
        }
        public async Task BackgroundDownloadMedia(SpotifyTrack Track)
        {
            CurrentlyDownloading = Track;
            Client = new YoutubeClient();
            string CleanFilename = Utils.ClearChars(Track.Name);
            var Progress = new Progress<DownloadOperation>(p =>
            {
                this.Progress = (double)p.Progress.BytesReceived / (double)p.Progress.TotalBytesToReceive;
            });

            Status = "Searching";
            var SearchResult = await SpotifyToYouTube(Track);
            if (string.IsNullOrWhiteSpace(SearchResult))
            {
                OnDownloadCompleted(Result.NoMediaFound);
                return;
            }

            OutputFolder = await StorageFolder.GetFolderFromPathAsync(OutputPath);
            var manifest = await Client.Videos.Streams.GetManifestAsync(SearchResult);
            var streaminfo = manifest.GetAudioOnlyStreams().GetWithHighestBitrate();

            var OutputFile = await OutputFolder.CreateFileAsync($"{CleanFilename}.{streaminfo.Container}", CreationCollisionOption.ReplaceExisting);

            try
            {
                Status = "Downloading";
                Debug.WriteLine($"[DOWNLOADER] Downloading {streaminfo.Url}");

                BackgroundDownloader downloader = new BackgroundDownloader();
                DownloadOperation operation = downloader.CreateDownload(new Uri(streaminfo.Url,UriKind.Absolute), OutputFile);

                await operation.StartAsync().AsTask(CancelToken, Progress);
            }
            catch (System.Net.Http.HttpRequestException)
            {
                OnDownloadCompleted(Result.FailedRequest);
                return;
            }
            catch (TaskCanceledException)
            {
                if (File.Exists(OutputFile.Path))
                {
                    await OutputFile.DeleteAsync();
                }
                OnDownloadCompleted(Result.Cancelled);
                return;
            }
            catch (OperationCanceledException)
            {
                if (File.Exists(OutputFile.Path))
                {
                    await OutputFile.DeleteAsync();
                }
                OnDownloadCompleted(Result.Cancelled);
                return;
            }
            catch (Exception ex)
            {
                OnDownloadCompleted(Result.NotDetermined, ex.Message);
                return;
            }
            var newfile = await ConvertToMP3(OutputFile);
            Track.SetTagsAsync(newfile);

            OnDownloadCompleted(Result.Success, newfile);
            Debug.WriteLine("[DOWNLOADER] Finished");
            return;
        }
        private async Task<string> SpotifyToYouTube(SpotifyTrack Track)
        {
            var SearchResult = await Task.Run(() => Search(Track.Name, Track.FirstAuthor, Track.Duration, 1500));
            Debug.WriteLine("[Downloader] Searching | First attempt");
            Status = "Searching attempt #1";
            if (string.IsNullOrWhiteSpace(SearchResult))
            {
                SearchResult = await Task.Run(() => Search(Track.Name + " Audio", Track.FirstAuthor, Track.Duration, 3000));
                Debug.WriteLine("[Downloader] Searching | Second attempt");
                Status = "Searching attempt #2";
                if (string.IsNullOrWhiteSpace(SearchResult))
                {
                    SearchResult = await Task.Run(() => Search(Track.Title + " Audio", Track.FirstAuthor, Track.Duration, 4000));
                    Debug.WriteLine("[Downloader] Searching | Third attempt");
                    Status = "Searching attempt #3";
                    if (string.IsNullOrWhiteSpace(SearchResult))
                    {
                        SearchResult = await Task.Run(() => Search(Track.Name, Track.FirstAuthor, Track.Duration, 30000));
                        Debug.WriteLine("[Downloader] Searching | Last attempt");
                        Status = "Searching attempt #4";
                        if (string.IsNullOrWhiteSpace(SearchResult))
                        {
                            return "";
                        }
                    }
                }
            }
            return SearchResult;
        }
        private async Task<string> Search(string SearchQuery, string Keyword, double Duration, double MarginOfError)
        {
            int Results = 8;
            VideoSearchResult Result;
            var TempClient = new YoutubeClient();
            string URL = "";
            var Videos = await TempClient.Search.GetVideosAsync(SearchQuery).CollectAsync(Results);
            for (int i = 0; i < Results; i++)
            {
                Result = Videos[i];
                TimeSpan ts = (TimeSpan)Result.Duration;
                URL = Result.Url;
                if (ts.TotalMilliseconds < Duration + MarginOfError && ts.TotalMilliseconds > Duration - MarginOfError)
                {
                    var Title = Result.Title.ToUpper();
                    var ChannelName = Result.Author.Title.ToUpper();
                    if (Title.Contains(Keyword.ToUpper()) || ChannelName.Contains(Keyword.ToUpper()))
                    {
                        break;
                    }
                    else
                    {
                        URL = "";
                    }
                }
                else
                {
                    URL = "";
                }
            }

            return URL;
        }
        private async Task<StorageFile> ConvertToMP3(StorageFile OutputFile)
        {
            Status = "Converting";
            Progress = 0;
            //Creates new file with .mp3
            StorageFile NewFile = await (await OutputFile.GetParentAsync()).CreateFileAsync($"{OutputFile.DisplayName}.mp3",CreationCollisionOption.ReplaceExisting);

            Progress = 0.1;
            System.Diagnostics.Debug.WriteLine($"[DOWNLOADER] Created {NewFile.Path}");

            //Media transcoding garbage
            MediaEncodingProfile profile =
                MediaEncodingProfile.CreateMp3(AudioEncodingQuality.High);
            MediaTranscoder transcoder = new MediaTranscoder();

            PrepareTranscodeResult prepareOp = await
                transcoder.PrepareFileTranscodeAsync(OutputFile, NewFile, profile);
            Progress = 0.3;

            if (prepareOp.CanTranscode)
            {
                Progress = 0.6;
                System.Diagnostics.Debug.WriteLine($"[DOWNLOADER] Transcoding {NewFile.Path}");
                await prepareOp.TranscodeAsync();
            }
            
            //Deletes raw file\
            Progress = 0.8;
            System.Diagnostics.Debug.WriteLine($"[DOWNLOADER] Deleting {OutputFile.Path}");
            await OutputFile.DeleteAsync();
            Progress = 0.9;
            System.Diagnostics.Debug.WriteLine($"[DOWNLOADER] Deleted {OutputFile.Path}");

            return NewFile;
        }
        protected virtual void OnProgressChanged()
        {
            Debug.WriteLine($"[DOWNLOADER] {Status} | {Progress * 100}%");
            ProgressChanged?.Invoke(this,
                    new DownloadProgressEventArgs()
                    {
                        Progress = Progress,
                        Status = Status,
                        IsVideo = CurrentlyDownloading.IsVideo
                    });
        }
        protected virtual void OnDownloadCompleted(Result DownloadResult, StorageFile OutputFile, string ExceptionMessage)
        {
            Progress = 1;
            DownloadCompleted?.Invoke(this,
                    new DownloadCompleteEventArgs()
                    {
                        Result = DownloadResult,
                        ExceptionMessage = ExceptionMessage,
                        OutputFile = OutputFile
                    });
        }
        protected virtual void OnDownloadCompleted(Result DownloadResult, string ExceptionMessage)
        {
            Progress = 1;
            DownloadCompleted?.Invoke(this,
                    new DownloadCompleteEventArgs()
                    {
                        Result = DownloadResult,
                        ExceptionMessage = ExceptionMessage
                    });
        }
        protected virtual void OnDownloadCompleted(Result DownloadResult, StorageFile OutputFile)
        {
            Progress = 1;
            DownloadCompleted?.Invoke(this,
                    new DownloadCompleteEventArgs()
                    {
                        Result = DownloadResult,
                        OutputFile = OutputFile
                    });
        }
        protected virtual void OnDownloadCompleted(Result DownloadResult)
        {
            Progress = 1;
            DownloadCompleted?.Invoke(this,
                    new DownloadCompleteEventArgs()
                    {
                        Result = DownloadResult
                    });
        }
    }
    public class DownloadProgressEventArgs : EventArgs
    {
        public double Progress { get; set; }
        public string Status { get; set; }
        public bool IsVideo { get; set; }
    }
    public class DownloadCompleteEventArgs : EventArgs
    {
        public Result Result { get; set; }
        public string ExceptionMessage { get; set; }
        public StorageFile OutputFile { get; set; }
    }
}
