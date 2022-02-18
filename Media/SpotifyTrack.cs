using Media_Downloader_App.Abstractions;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using TagLib;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace MP3DL.Media
{
    public class SpotifyTrack : IMedia
    {
        public SpotifyTrack(FullTrack Track, FullAlbum Album)
        {
            Title = Track.Name;
            Authors = ((IEnumerable<SimpleArtist>)Track.Artists).Select(p => p.Name).ToArray();
            PrintedAuthors = PrintAuthors();
            _Album = Album;

            this.Album = Track.Album.Name;
            Number = (uint)Track.TrackNumber;
            Year = GetReleaseYear(Track.Album.ReleaseDate);

            PreviewURL = Track.PreviewUrl;
            Duration = Track.DurationMs;
            ID = Track.Id;

            Bitmap = new BitmapImage(new Uri(Album.Images[0].Url, UriKind.Absolute));
        }
        public SpotifyTrack(SimpleTrack Track, FullAlbum Album)
        {
            Title = Track.Name;
            Authors = ((IEnumerable<SimpleArtist>)Track.Artists).Select(p => p.Name).ToArray();
            PrintedAuthors = PrintAuthors();
            _Album = Album;

            this.Album = Album.Name;
            Number = (uint)Track.TrackNumber;
            Year = GetReleaseYear(Album.ReleaseDate);

            PreviewURL = Track.PreviewUrl;
            Duration = Track.DurationMs;
            ID = Track.Id;

            Bitmap = new BitmapImage(new Uri(Album.Images[0].Url, UriKind.Absolute));
        }
        public SpotifyTrack(SimpleTrack Track, SimpleAlbum Album)
        {
            Title = Track.Name;
            Authors = ((IEnumerable<SimpleArtist>)Track.Artists).Select(p => p.Name).ToArray();
            PrintedAuthors = PrintAuthors();
            _SimpleAlbum = Album;

            this.Album = Album.Name;
            Number = (uint)Track.TrackNumber;
            Year = GetReleaseYear(Album.ReleaseDate);

            PreviewURL = Track.PreviewUrl;
            Duration = Track.DurationMs;
            ID = Track.Id;

            Bitmap = new BitmapImage(new Uri(Album.Images[0].Url, UriKind.Absolute));
        }
        public SpotifyTrack(FullTrack Track, SimpleAlbum Album)
        {
            Title = Track.Name;
            Authors = ((IEnumerable<SimpleArtist>)Track.Artists).Select(p => p.Name).ToArray();
            PrintedAuthors = PrintAuthors();
            _SimpleAlbum = Album;

            this.Album = Track.Album.Name;
            Number = (uint)Track.TrackNumber;
            Year = GetReleaseYear(Track.Album.ReleaseDate);

            PreviewURL = Track.PreviewUrl;
            Duration = Track.DurationMs;
            ID = Track.Id;

            Bitmap = new BitmapImage(new Uri(Album.Images[0].Url, UriKind.Absolute));
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

            Bitmap = Track.Bitmap;
        }
        public System.Drawing.Image Art { get; set; }
        public BitmapImage Bitmap { get; set; }
        public string Name
        {
            get { return $"{FirstAuthor} - {Title}"; }
        }
        public string Title { get; set; }
        public string[] Authors { get; private set; }
        public string PrintedAuthors { get; set; }
        public string FirstAuthor
        {
            get
            {
                return FirstFromPrinted();
            }
        }
        private FullAlbum _Album { get; set; }
        private SimpleAlbum _SimpleAlbum { get; set; }
        public string Album { get; set; }
        public uint Number { get; private set; }
        public string Year { get; private set; }
        public string ID { get; private set; }
        public string PreviewURL { get; private set; }
        public double Duration { get; private set; }
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
        public double Opacity
        {
            get
            {
                if (String.IsNullOrWhiteSpace(PreviewURL))
                {
                    return 0.14;
                }
                else
                {
                    return 1;
                }
            }
        }
        public async void SetTagsAsync(StorageFile file)
        {
            StorageFileAbstraction taglibfile = new StorageFileAbstraction(file);
            using (var tagFile = TagLib.File.Create(taglibfile, ReadStyle.Average))
            {
                //read the raw tags
                tagFile.Tag.Title = Title;
                tagFile.Tag.Performers = PrintedAuthorsToArray();
                tagFile.Tag.Album = Album;
                tagFile.Tag.Track = Number;
                tagFile.Tag.Year = (uint)Int32.Parse(Year);

                //Set cover art
                RandomAccessStreamReference random = RandomAccessStreamReference.CreateFromUri(Bitmap.UriSour‌​ce);

                using (IRandomAccessStream stream = await random.OpenReadAsync())
                {
                    using (var TempStream = stream.AsStream())
                    {
                        TempStream.Position = 0;
                        TagLib.Picture pic = new TagLib.Picture();
                        pic.Data = ByteVector.FromStream(TempStream);
                        pic.Type = PictureType.FrontCover;

                        tagFile.Tag.Pictures = new IPicture[] { pic };
                    }
                }
                //Save and dispose
                tagFile.Save();
                tagFile.Dispose();
            }
        }
        private string PrintAuthors()
        {
            string print = "";
            foreach (string author in Authors)
            {
                print = $"{print}, {author}";
            }
            return print.Substring(2, print.Trim().Length - 2);
        }
        private string[] PrintedAuthorsToArray()
        {
            List<string> templist = new List<string>();
            string tempstring;
            string tempprintedauthors = PrintedAuthors;
            int i = 0;

            if (!tempprintedauthors.EndsWith(","))
            {
                tempprintedauthors = tempprintedauthors + ",";
            }
            while (tempprintedauthors.Contains(","))
            {
                int x = tempprintedauthors.IndexOf(",");
                tempstring = tempprintedauthors.Substring(0, x);
                tempprintedauthors = tempprintedauthors.Substring(x + 1, tempprintedauthors.Length - x - 1);
                if (tempstring.StartsWith(" "))
                {
                    tempstring = tempstring.Substring(1, tempstring.Length - 1);
                }
                templist.Add(tempstring);
                i++;
            }
            return templist.ToArray();
        }
        private string FirstFromPrinted()
        {
            string printedauthors = PrintedAuthors;
            if (!printedauthors.EndsWith(","))
            {
                printedauthors += ",";
            }
            int x = printedauthors.IndexOf(",");
            string temp = printedauthors.Substring(0,x);

            return temp.Trim();
        }
        private string GetReleaseYear(string fulldate)
        {
            if (fulldate.Contains('-'))
            {
                int x = fulldate.IndexOf('-');
                return fulldate.Substring(0,x);
            }
            else
            {
                return fulldate;
            }
        }
        public override string ToString()
        {
            return Name;
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
