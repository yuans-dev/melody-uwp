using Melody.Statics;
using Melody.Core;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage;
using System.IO;
using System;

namespace Melody.ViewModels
{
    public class DownloadCollectionItemViewModel : INotifyPropertyChanged, IDownloadItem
    {
        public DownloadCollectionItemViewModel(SpotifyPlaylist Playlist)
        {
            StatusGlyph = Glyphs.WaitingGlyph;
            Title = Playlist.Title;
            Authors = Playlist.Authors;
            Bitmap = Playlist.Bitmap;
            HasNotStarted = true;
            TokenSource = new CancellationTokenSource();
            Collection = Playlist;
        }
        public DownloadCollectionItemViewModel(SpotifyAlbum Album)
        {
            StatusGlyph = Glyphs.WaitingGlyph;
            Title = Album.Title;
            Authors = Album.Authors;
            Bitmap = Album.Bitmap;
            HasNotStarted = true;
            TokenSource = new CancellationTokenSource();
            Collection = Album;
        }
        public IMediaCollection Collection { get; set; }
        public BitmapImage Bitmap { get; set; }
        private StorageFolder CollectionFolder { get; set; }
        public string Title { get; set; }
        public string[] Authors { get; set; }
        private int _ProgressValue { get; set; }
        private CancellationTokenSource TokenSource { get; set; }
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
        public Visibility DeleteOptionVisibility { get; private set; } = Visibility.Collapsed;
        private DownloadItemViewModel CurrentlyDownloading { get; set; }
        public ObservableCollection<DownloadItemViewModel> MediaItems { get; set; }
        public async Task<List<DownloadItemViewModel>> GetMediaItems(SpotifyPlaylist Playlist)
        {
            List<DownloadItemViewModel> temp = new List<DownloadItemViewModel>();
            foreach (var track in await Settings.SpotifyClient.GetPlaylistTracks(Playlist))
            {
                temp.Add(new DownloadItemViewModel(track, false, CollectionFolder.Path));
            }
            return temp;
        }
        public async Task<List<DownloadItemViewModel>> GetMediaItems(SpotifyAlbum Album)
        {
            List<DownloadItemViewModel> temp = new List<DownloadItemViewModel>();
            foreach (var track in await Settings.SpotifyClient.GetAlbumTracks(Album))
            {
                temp.Add(new DownloadItemViewModel(track, false, CollectionFolder.Path));
            }
            return temp;
        }
        public async Task InitDownloader(SpotifyAlbum Album)
        {
            MediaItems = new ObservableCollection<DownloadItemViewModel>();
            foreach (var item in await GetMediaItems(Album))
            {
                MediaItems.Add(item);
            }
        }
        private async Task InitDownloader(SpotifyPlaylist Playlist)
        {
            MediaItems = new ObservableCollection<DownloadItemViewModel>();
            foreach (var item in await GetMediaItems(Playlist))
            {
                MediaItems.Add(item);
            }
        }
        public async void StartDownload()
        {
            var outputfolder = await StorageFolder.GetFolderFromPathAsync(Settings.OutputFolder);
            CollectionFolder = await outputfolder.CreateFolderAsync(Collection.Title,CreationCollisionOption.GenerateUniqueName);
            if (Collection is SpotifyPlaylist playlist)
            {
                await InitDownloader(playlist);
            }
            else if (Collection is SpotifyAlbum album)
            {
                await InitDownloader(album);
            }

            StatusGlyph = Glyphs.CancelGlyph;
            ProgressValue = 0;
            HasNotStarted = false;
            System.Diagnostics.Debug.WriteLine($"{MediaItems.Count}");
            for (int i = 0; i < MediaItems.Count; i++)
            {
                if (TokenSource.IsCancellationRequested)
                {
                    continue;
                }
                System.Diagnostics.Debug.WriteLine($"[DownloadCollectionItemViewModel] Item {i + 1}");
                Status = $"Downloading {MediaItems[i].Media.Title}...";
                CurrentlyDownloading = MediaItems[i];
                await MediaItems[i].StartDownload();
                ProgressValue = 100 * (i + 1) / MediaItems.Count;
            }
            ProgressValue = 100;

            OnCollectionDownloadFinished();
        }
        public void CancelDownload()
        {
            StatusGlyph = Glyphs.RetryGlyph;
            CurrentlyDownloading.CancelDownload();
            TokenSource.Cancel();
        }
        public override string ToString()
        {
            return "COLLECTION";
        }
        protected virtual void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
        protected virtual void OnCollectionDownloadFinished()
        {
            int f = 0;
            int c = 0;
            foreach (var item in MediaItems)
            {
                if (item.Status != "Completed.")
                {
                    f++;
                }
                else
                {
                    c++;
                }
            }
            if (f == 0)
            {
                Status = "Completed.";
                StatusGlyph = Glyphs.CheckGlyph;
                InfoHelper.ShowNotification($"You have successfully finished downloading \"{Collection.Title}\"", "Download completed", Bitmap.UriSource);
            }
            else
            {
                Status = $"{f} item(s) failed to download.";
                StatusGlyph = Glyphs.RetryGlyph;
                InfoHelper.ShowNotification($"{f} item(s) failed to download in \"{Collection.Title}\"", "Download incomplete", Bitmap.UriSource);
            }


            TokenSource = new CancellationTokenSource();
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
