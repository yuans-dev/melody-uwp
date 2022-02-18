using Media_Downloader_App;
using Media_Downloader_App.Abstractions;
using Media_Downloader_App.Statics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TagLib;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace MP3DL.Media
{
    public class YouTubeVideo : IMedia
    {
        public YouTubeVideo(Video Video, bool IsVideo)
        {
            this.Video = Video;
            Title = Video.Title;
            Authors = new string[1] { Video.Author.Title };
            PrintedAuthors = PrintAuthors();

            Number = 1;
            Year = Video.UploadDate.Year.ToString();

            Duration = Video.Duration.Value.TotalMilliseconds;
            if(Video.Duration != null)
            {
                DurationAsTimeSpan = (TimeSpan)Video.Duration;
            }
            else
            {
                DurationAsTimeSpan = TimeSpan.Zero;
            }
            
            ID = Video.Id;
            this.IsVideo = IsVideo;

            Bitmap = new BitmapImage(new Uri(Video.Thumbnails[0].Url, UriKind.Absolute));
            JpgBitmap = new BitmapImage(new Uri(Utils.IsolateJPG(Video.Thumbnails[0].Url), UriKind.Absolute));
        }
        public YouTubeVideo(Video Video, bool IsVideo, string ThumnbailUrl)
        {
            this.Video = Video;
            Title = Video.Title;
            Authors = new string[1] { Video.Author.Title };
            PrintedAuthors = PrintAuthors();

            Number = 1;
            Year = Video.UploadDate.Year.ToString();

            Duration = Video.Duration.Value.TotalMilliseconds;
            if (Video.Duration != null)
            {
                DurationAsTimeSpan = (TimeSpan)Video.Duration;
            }
            else
            {
                DurationAsTimeSpan = TimeSpan.Zero;
            }

            ID = Video.Id;
            this.IsVideo = IsVideo;

            Bitmap = new BitmapImage(new Uri(ThumnbailUrl, UriKind.Absolute));
            JpgBitmap = new BitmapImage(new Uri(Utils.IsolateJPG(Video.Thumbnails[0].Url), UriKind.Absolute));
        }
        public YouTubeVideo(YouTubeVideo Video)
        {
            this.Video = Video.Video;
            Title = Video.Title;
            Authors = Video.Authors;
            PrintedAuthors = Video.PrintedAuthors;

            Number = Video.Number;
            Year = Video.Year;

            Duration = Video.Duration;
            DurationAsTimeSpan = Video.DurationAsTimeSpan;
            ID = Video.ID;
            IsVideo = Video.IsVideo;

            Bitmap = Video.Bitmap;
            JpgBitmap = Video.JpgBitmap;
        }
        public string Name
        {
            get
            {
                return $"{FirstAuthor} - {Title}";
            }
        }
        private Video Video { get; set; }
        public string Title { get; set; }

        public string[] Authors { get; private set; }
        public string Album { get; set; }

        public Image Art { get; set; }
        public BitmapImage Bitmap { get; set; }
        private BitmapImage JpgBitmap { get; set; }

        public uint Number { get; private set; }

        public double Duration { get; private set; }
        public TimeSpan DurationAsTimeSpan { get; private set; }

        public string ID { get; private set; }

        public string Year { get; private set; }

        public string PrintedAuthors { get; set; }
        public bool IsPreviewAvailable { get; private set; } = true;

        public string FirstAuthor
        {
            get { return FirstFromPrinted(); }
        }
        public bool IsVideo { get; set; }

        public bool Equals(IMedia other)
        {
            if (other == null)
                return false;

            if (this.Name == other.Name)
                return true;
            else
                return false;
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

                RandomAccessStreamReference random = RandomAccessStreamReference.CreateFromUri(JpgBitmap.UriSource);
                using (IRandomAccessStream stream = await random.OpenReadAsync())
                {
                    var dec = await BitmapDecoder.CreateAsync(stream);
                    var square = Math.Min(dec.PixelHeight, dec.PixelWidth) - 100;

                    
                    var p = new Point((int)((dec.PixelWidth / 2)-(square / 2) ), (int)((dec.PixelHeight / 2)-(square/2)));
                    var size = new Size((int)square, (int)square);

                    System.Diagnostics.Debug.WriteLine($"Dimensions: Width = {dec.PixelWidth} Height = {dec.PixelHeight} | Point: x = {p.X},y = {p.Y}, Square = {square}");

                    using (var stream1 = stream.AsStreamForWrite())
                    {
                        stream1.Position = 0;
                        using (var newstream = await ImageTools.CropAsync(stream1, p, size))
                        {
                            newstream.Position = 0;
                            TagLib.Picture pic = new TagLib.Picture();
                            pic.Data = ByteVector.FromStream(newstream);
                            pic.Type = PictureType.FrontCover;

                            tagFile.Tag.Pictures = new IPicture[] { pic };
                        }
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
            return print.Substring(2, print.Length - 2);
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

            if (temp.StartsWith(" "))
            {
                temp = temp.Substring(1,temp.Length-1);
            }
            return temp;
        }
        public override string ToString()
        {
            return Name;
        }
        public IVideoStreamInfo RequestedVideoQuality { get; set; }
    }
}
