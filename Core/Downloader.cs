using Melody.Classes;
using Melody.Statics;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Unidecode.NET;
using Windows.Storage;
using YoutubeExplode.Common;

namespace Melody.Core
{
    public enum Result
    {
        Success,
        DuplicateFile,
        NoMediaFound,
        Cancelled,
        FailedRequest,
        FFMPEGNotFound,
        NotDetermined,
        Other
    }
    public class Downloader
    {
        public Downloader()
        {
            this.OutputPath = "Downloads";
            CancelToken = new CancellationTokenSource().Token;
        }
        public event EventHandler<DownloadCompleteEventArgs> DownloadCompleted;
        public event EventHandler<DownloadProgressEventArgs> ProgressChanged;
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
        public string OutputPath { get; set; }
        public async Task<string> GetStream(SpotifyTrack track)
        {
            var ytlink = await ToYouTubeLink(track);
            var streaminfo = await Settings.YouTubeClient.GetStreamInfo(ytlink);
            return streaminfo.Url;
        }
        public async Task DownloadMedia(IMedia media)
        {
            try
            {
                if (media is SpotifyTrack track)
                {
                    await DownloadMedia(track);
                }
                else if (media is YouTubeVideo video)
                {
                    await DownloadMedia(video);
                }
            }
            catch (YoutubeExplode.Exceptions.VideoUnplayableException)
            {
                OnDownloadCompleted(Result.Other, "Video is unavailable due to age restrictions");
            }catch(UnauthorizedAccessException) 
            {
                OnDownloadCompleted(Result.Other, "Unauthorized access! Please set your downloads folder again.");
            }
        }
        public async Task DownloadMedia(SpotifyTrack track)
        {
            CurrentlyDownloading = track;
            var ytlink = await ToYouTubeLink(track);
            if (string.IsNullOrWhiteSpace(ytlink))
            {
                OnDownloadCompleted(Result.NoMediaFound);
                return;
            }
            Status = "Downloading";

            var streaminfo = await Settings.YouTubeClient.GetStreamInfo(ytlink);
            var outputfile = await (await StorageFolder.GetFolderFromPathAsync(OutputPath))
                .CreateFileAsync($"{track.Name.MakeSafeForFiles()}.mp3", CreationCollisionOption.ReplaceExisting);
            var tempfile = await Settings.TemporaryFolder
                .CreateFileAsync($"{track.Name.MakeSafeForFiles()}.mp3temp", CreationCollisionOption.ReplaceExisting);

            try
            {
                var stream = await Settings.YouTubeClient.GetStream(streaminfo, CancelToken);

                var filestream = await tempfile.OpenStreamForWriteAsync();
                filestream.Seek(0, SeekOrigin.Begin);
                
                var progress = new Progress<long>(p => this.Progress = (double)p / (double)stream.Length);

                await stream.CopyToAsync(filestream, 81920, progress, CancelToken);

                stream.Dispose();
                await filestream.FlushAsync();
                filestream.Dispose();

                await tempfile.ConvertToMP3Async(outputfile);
                await tempfile.TryDeleteAsync();
            }
            catch (System.Net.Http.HttpRequestException)
            {
                OnDownloadCompleted(Result.FailedRequest);
                return;
            }
            catch (TaskCanceledException)
            {
                await outputfile.TryDeleteAsync();
                await tempfile.TryDeleteAsync();
                OnDownloadCompleted(Result.Cancelled);
                return;
            }
            catch (OperationCanceledException)
            {
                await outputfile.TryDeleteAsync();
                await tempfile.TryDeleteAsync();
                OnDownloadCompleted(Result.Cancelled);
                return;
            }
            catch (Exception ex)
            {
                OnDownloadCompleted(Result.NotDetermined, $"{ex.Source} | {ex.Message}");
                //throw ex; //for debug
                return;
            }

            OnDownloadCompleted(Result.Success,"", await outputfile.SetMetadataAsync(track));
            return;
        }
        public async Task DownloadMedia(YouTubeVideo video)
        {
            CurrentlyDownloading = video;

            Status = "Downloading";
            var streaminfo = await Settings.YouTubeClient.GetStreamInfo(video.ID.ID, video.IsVideo, video.RequestedVideoQuality);

            string extension = "";
            switch (video.IsVideo)
            {
                case true:
                    extension = streaminfo.Container.Name;
                    break;
                case false:
                    extension = "mp3";
                    break;
            }

            var outputfile = await 
                (await StorageFolder.GetFolderFromPathAsync(OutputPath))
                .CreateFileAsync($"{video.Name.MakeSafeForFiles()}.{extension}", CreationCollisionOption.ReplaceExisting);
            var tempfile = await
                        Settings.TemporaryFolder
                        .CreateFileAsync($"{video.Name.MakeSafeForFiles()}.{extension}temp", CreationCollisionOption.ReplaceExisting);

            try
            {
                if (!video.IsVideo)
                {
                    var stream = await Settings.YouTubeClient.GetStream(streaminfo, CancelToken);
                    var filestream = await tempfile.OpenStreamForWriteAsync();
                    filestream.Seek(0, SeekOrigin.Begin);
                    var progress = new Progress<long>(p => this.Progress = (double)p / (double)stream.Length);
                    await stream.CopyToAsync(filestream, 81920, progress, CancelToken);
                    stream.Dispose();
                    await filestream.FlushAsync();
                    filestream.Dispose();
                    await tempfile.ConvertToMP3Async(outputfile);
                    await tempfile.TryDeleteAsync();
                }
                else
                {
                    await tempfile.TryDeleteAsync();
                    var stream = await Settings.YouTubeClient.GetStream(streaminfo, CancelToken);
                    var filestream = await outputfile.OpenStreamForWriteAsync();
                    filestream.Seek(0, SeekOrigin.Begin);
                    var progress = new Progress<long>(p => this.Progress = (double)p / (double)stream.Length);
                    await stream.CopyToAsync(filestream, 81920, progress, CancelToken);
                    stream.Dispose();
                    await filestream.FlushAsync();
                    filestream.Dispose();
                }
            }
            catch (System.Net.Http.HttpRequestException)
            {
                OnDownloadCompleted(Result.FailedRequest);
                return;
            }
            catch (TaskCanceledException)
            {
                await outputfile.TryDeleteAsync();
                await tempfile.TryDeleteAsync();
                OnDownloadCompleted(Result.Cancelled);
                return;
            }
            catch (OperationCanceledException)
            {
                await outputfile.TryDeleteAsync();
                await tempfile.TryDeleteAsync();
                OnDownloadCompleted(Result.Cancelled);
                return;
            }
            catch (Exception ex)
            {
                OnDownloadCompleted(Result.NotDetermined, ex.Message);
                return;
            }

            if (!video.IsVideo)
            {
                OnDownloadCompleted(Result.Success,"",await outputfile.SetMetadataAsync(video));
                return;
            }
            else
            {
                OnDownloadCompleted(Result.Success,"",outputfile);
                return;
            }
        }
        private async Task<string> ToYouTubeLink(SpotifyTrack Track)
        {
            string title = await Track.Title.Romanize();
            string[] keywords = new string[2] { title ,Track.Title };
            int[] errormargins = new int[6] {  0, 1000, 2000, 4000, 8000, 12000 };
            var i = 1;
            foreach (var keyword in keywords)
            {
                foreach (var margin in errormargins)
                {
                    Status = $"Searching";
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
        protected virtual void OnProgressChanged()
        {
            ProgressChanged?.Invoke(this,
                    new DownloadProgressEventArgs()
                    {
                        Progress = Progress,
                        Status = Status,
                        IsVideo = CurrentlyDownloading.IsVideo
                    });
        }
        protected virtual void OnDownloadCompleted(Result DownloadResult, string ExceptionMessage = "", StorageFile OutputFile = null )
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
