using Melody.Core;
using Melody.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLib.Ape;

namespace Melody.Statics
{
    public static class DownloadManager
    {
        public static ObservableCollection<IDownloadItem> Downloads { get; set; } = new ObservableCollection<IDownloadItem>();
        public static event EventHandler<DownloadAddedEventArgs> DownloadAdded;
        public static async void AddToDownloads(IBaseMedia BaseMedia)
        {
            var item = ConvertToDownloadItem(BaseMedia);
            Downloads.Add(item);
            if(item is DownloadItemViewModel itemvm)
            {
                await itemvm.StartDownload();
            }else if(item is DownloadCollectionItemViewModel collectionvm) {
                collectionvm.StartDownload();
            }
            OnAddedDownload(item);
        }

        private static void OnAddedDownload(IDownloadItem DownloadItem)
        {
            DownloadAdded?.Invoke(typeof(DownloadManager), new DownloadAddedEventArgs { DownloadItem = DownloadItem});
        }

        public static IDownloadItem ConvertToDownloadItem(SpotifyTrack Media)
        {
            var item = new DownloadItemViewModel(Media);
            return item;
        }
        public static IDownloadItem ConvertToDownloadItem(YouTubeVideo Media)
        {
            var item = new DownloadItemViewModel(Media);
            return item;
        }
        public static IDownloadItem ConvertToDownloadItem(SpotifyPlaylist Playlist)
        {
            var item = new DownloadCollectionItemViewModel(Playlist);
            return item;
        }
        public static IDownloadItem ConvertToDownloadItem(SpotifyAlbum Album)
        {
            var item = new DownloadCollectionItemViewModel(Album);
            return item;
        }
        public static IDownloadItem ConvertToDownloadItem(IBaseMedia BaseMedia)
        {
            if (BaseMedia is SpotifyTrack track)
            {
                return ConvertToDownloadItem(track);
            }
            else if (BaseMedia is YouTubeVideo video)
            {
                return ConvertToDownloadItem(video);
            }else if(BaseMedia is SpotifyPlaylist playlist)
            {
                return ConvertToDownloadItem(playlist);
            }else if(BaseMedia is SpotifyAlbum album)
            {
                return ConvertToDownloadItem(album);
            }
            return null;
        }
    }
    public class DownloadAddedEventArgs : EventArgs
    {
        public IDownloadItem DownloadItem;
    }
}
