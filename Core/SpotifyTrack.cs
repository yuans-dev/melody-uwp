using Melody;
using Melody.Abstractions;
using Melody.Classes;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TagLib;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace Melody.Core
{
    public class SpotifyTrack : IMedia, INotifyPropertyChanged
    {
        public SpotifyTrack(FullTrack Track, FullAlbum Album)
        {
            Title = Track.Name;
            Authors = ((IEnumerable<SimpleArtist>)Track.Artists).Select(p => p.Name).ToArray();
            _Album = Album;
            this.Album = Track.Album.Name;
            Number = (uint)Track.TrackNumber;
            Year = GetReleaseYear(Track.Album.ReleaseDate);
            PreviewURL = Track.PreviewUrl;
            Duration = Track.DurationMs;
            ID = new MediaID(MediaType.SpotifyTrack, Track.Id);
            Popularity = Track.Popularity;
            Link = new MediaLink(Track.Uri, "https://open.spotify.com/track/" + Track.Id + "?");
            Bitmap = new BitmapImage(new Uri(Album.Images[0].Url, UriKind.Absolute));
            SetTagsAsync();
        }
        public SpotifyTrack(SimpleTrack Track, FullAlbum Album)
        {
            Title = Track.Name;
            Authors = ((IEnumerable<SimpleArtist>)Track.Artists).Select(p => p.Name).ToArray();
            _Album = Album;
            this.Album = Album.Name;
            Number = (uint)Track.TrackNumber;
            Year = GetReleaseYear(Album.ReleaseDate);
            PreviewURL = Track.PreviewUrl;
            Duration = Track.DurationMs;
            ID = new MediaID(MediaType.SpotifyTrack, Track.Id);
            Popularity = 0;
            Link = new MediaLink(Track.Uri, "https://open.spotify.com/track/" + Track.Id + "?");
            Bitmap = new BitmapImage(new Uri(Album.Images[0].Url, UriKind.Absolute));
            SetTagsAsync();
        }
        public SpotifyTrack(SimpleTrack Track, SimpleAlbum Album)
        {
            Title = Track.Name;
            Authors = ((IEnumerable<SimpleArtist>)Track.Artists).Select(p => p.Name).ToArray();
            _SimpleAlbum = Album;
            this.Album = Album.Name;
            Number = (uint)Track.TrackNumber;
            Year = GetReleaseYear(Album.ReleaseDate);
            PreviewURL = Track.PreviewUrl;
            Duration = Track.DurationMs;
            ID = new MediaID(MediaType.SpotifyTrack, Track.Id);
            Popularity = 0;
            Link = new MediaLink(Track.Uri, "https://open.spotify.com/track/" + Track.Id + "?");
            Bitmap = new BitmapImage(new Uri(Album.Images[0].Url, UriKind.Absolute));
            SetTagsAsync();
        }
        public SpotifyTrack(FullTrack Track, SimpleAlbum Album)
        {
            Title = Track.Name;
            Authors = ((IEnumerable<SimpleArtist>)Track.Artists).Select(p => p.Name).ToArray();
            _SimpleAlbum = Album;
            this.Album = Track.Album.Name;
            Number = (uint)Track.TrackNumber;
            Year = GetReleaseYear(Track.Album.ReleaseDate);
            PreviewURL = Track.PreviewUrl;
            Duration = Track.DurationMs;
            ID = new MediaID(MediaType.SpotifyTrack, Track.Id);
            Popularity = Track.Popularity;
            Link = new MediaLink(Track.Uri, "https://open.spotify.com/track/" + Track.Id + "?");
            Bitmap = new BitmapImage(new Uri(Album.Images[0].Url, UriKind.Absolute));
            SetTagsAsync();
        }
        public SpotifyTrack(SpotifyTrack Track)
        {
            Title = Track.Title;
            Authors = Track.Authors;
            PrintedAuthors = Track.PrintedAuthors;
            _Album = Track._Album;
            this.Album = Track.Album;
            Number = Track.Number;
            Year = Track.Year;
            PreviewURL = Track.PreviewURL;
            Duration = Track.Duration;
            ID = Track.ID;
            Popularity = Track.Popularity;
            Link = Track.Link;
            Bitmap = Track.Bitmap;
            SetTagsAsync();
        }
        public BitmapImage Bitmap { get; set; }
        public string Name
        {
            get { return $"{Authors.First()} - {Title}"; }
        }
        public string Title { get; set; }
        public string[] Authors { get; set; }
        public string PrintedAuthors { get; set; }
        private FullAlbum _Album { get; set; }
        private SimpleAlbum _SimpleAlbum { get; set; }
        public string Album { get; set; }
        public uint Number { get; private set; }
        public string Year { get; private set; }
        public List<string> _Tags = new List<string>();
        public List<string> Tags
        {
            get { return _Tags; }
            set
            {
                _Tags = value;
                OnPropertyChanged("Tags");
            }
        }
        public int Popularity { get; private set; }
        public MediaID ID { get; private set; }
        public MediaLink Link { get; private set; }
        public string PreviewURL { get; private set; }
        public double Duration { get; private set; }
        public string DurationAsTimeSpan
        {
            get
            {
                var timespan = TimeSpan.FromMilliseconds(Duration);
                return timespan.ToString(@"m\:ss");
            }
        }
        public bool IsVideo { get; private set; } = false;
        public bool IsPreviewAvailable
        {
            get
            {
                if (String.IsNullOrWhiteSpace(PreviewURL))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        private bool _IsPlayingPreview { get; set; } = false;
        public bool IsPlayingPreview
        {
            get { return _IsPlayingPreview; }
            set
            {
                _IsPlayingPreview = value;
                OnPropertyChanged("IsPlayingPreview");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
        public async void SetTagsAsync()
        {
            try
            {
                Tags = await LastFM.GetTrackTags(Title, Authors.First().ToString());
            }
            catch
            {

            }
        }
        private string GetReleaseYear(string fulldate)
        {
            try
            {
                if (fulldate.Contains('-'))
                {
                    int x = fulldate.IndexOf('-');
                    return fulldate.Substring(0, x);
                }
                else
                {
                    return fulldate;
                }
            }
            catch
            {
                return "0000";
            }
        }
        public override string ToString()
        {
            return "Track";
        }
        public bool Equals(IMedia other)
        {
            if (other == null)
                return false;

            if (this.Name == other.Name)
                return true;
            else
                return false;
        }
    }
}
