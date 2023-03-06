using Melody.Abstractions;
using Melody.Core;
using Melody.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TagLib;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Composition;

namespace Melody.Statics
{
    public static class ExtensionMethods
    {
        public static string MakeSafeForFiles(this string input)
        {
            string tmp = input;
            tmp = tmp.Replace('/', '-');
            tmp = tmp.Replace('|', ' ');
            tmp = tmp.Replace('\"', ' ');
            tmp = tmp.Replace('[', ' ');
            tmp = tmp.Replace(']', ' ');
            tmp = tmp.Replace('{', ' ');
            tmp = tmp.Replace('}', ' ');
            tmp = tmp.Replace('\'', ' ');
            tmp = tmp.Replace(',', ' ');
            tmp = tmp.Replace('.', ' ');
            tmp = tmp.Replace(':', ' ');
            tmp = tmp.Replace('?', ' ');
            tmp = tmp.Replace('*', ' ');
            return tmp;
        }
        public static string[] ToArray(this string str, string separator)
        {
            return str.Split(separator);
        }
        public static string ToString(this string[] array, string separator)
        {
            var str = "";
            foreach (var item in array)
            {
                str = $"{str}{separator}{item}";
            }
            return str.Substring(1, str.Length - 1).Trim();
        }
        public static void CopyToClipboard(this string Text)
        {
            DataPackage dataPackage = new DataPackage();
            dataPackage.RequestedOperation = DataPackageOperation.Copy;
            dataPackage.SetText(Text);
            Clipboard.SetContent(dataPackage);
        }
        public static async Task CopyToAsync(this Stream source, Stream destination, int bufferSize, IProgress<long> progress = null, CancellationToken cancellationToken = default)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (!source.CanRead)
                throw new ArgumentException("Has to be readable", nameof(source));
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));
            if (!destination.CanWrite)
                throw new ArgumentException("Has to be writable", nameof(destination));
            if (bufferSize < 0)
                throw new ArgumentOutOfRangeException(nameof(bufferSize));

            var buffer = new byte[bufferSize];
            long totalBytesRead = 0;
            int bytesRead;
            while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) != 0)
            {
                await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
                totalBytesRead += bytesRead;
                progress?.Report(totalBytesRead);
            }
        }
        public static async Task<Stream> ConvertToMP3Async(this Stream stream)
        {
            MediaEncodingProfile profile =
            MediaEncodingProfile.CreateMp3(AudioEncodingQuality.High);
            MediaTranscoder transcoder = new MediaTranscoder();

            var temp = new MemoryStream();
            System.Diagnostics.Debug.WriteLine(
                $"[TASK](Converting stream to .mp3)\n" +
                $"Stream details:\n" +
                $"Length: {stream.Length}\n" +
                $"Position: {stream.Position}\n" +
                $"Can read: {stream.CanRead}\n" +
                $"Can write: {stream.CanWrite}\n" +
                $"Can seek: {stream.CanSeek}\n" +
                $"Can timeout: {stream.CanTimeout}");

            var destination = temp.AsRandomAccessStream();
            var source = stream.AsRandomAccessStream();

            PrepareTranscodeResult prepareOp = await
                transcoder.PrepareStreamTranscodeAsync(source, destination, profile);

            if (prepareOp.CanTranscode)
            {
                await prepareOp.TranscodeAsync();
            }

            var output = destination.AsStreamForRead();
            output.Seek(0,SeekOrigin.Begin);
            return output;
        }
        public static async Task<Stream> ConvertToMP3Async(this Windows.Media.Core.IMediaSource stream)
        {
            MediaEncodingProfile profile =
            MediaEncodingProfile.CreateMp3(AudioEncodingQuality.High);
            MediaTranscoder transcoder = new MediaTranscoder();

            var temp = new MemoryStream();
            var destination = temp.AsRandomAccessStream();

            PrepareTranscodeResult prepareOp = await
                transcoder.PrepareMediaStreamSourceTranscodeAsync(stream,destination,profile);  

            if (prepareOp.CanTranscode)
            {
                await prepareOp.TranscodeAsync();
            }

            var output = destination.AsStreamForRead();
            output.Seek(0, SeekOrigin.Begin);
            return output;
        }
        public static async Task<StorageFile> ConvertToMP3Async(this StorageFile source, StorageFile destination)
        {
            MediaEncodingProfile profile =
            MediaEncodingProfile.CreateMp3(AudioEncodingQuality.High);
            MediaTranscoder transcoder = new MediaTranscoder();

            var temp = new MemoryStream();

            PrepareTranscodeResult prepareOp = await
                transcoder.PrepareFileTranscodeAsync(source, destination, profile);

            if (prepareOp.CanTranscode)
            {
                await prepareOp.TranscodeAsync();
            }

            return destination;
        }
        public static async Task<bool> TryDeleteAsync(this StorageFile file)
        {
            try
            {
                await file.DeleteAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static bool IsWithinRange(this double x,double maximum, double minimum)
        {
            if(x < maximum && x > minimum)
            {
                return true;
            }
            else { return false; }
        }
        public static async Task<StorageFile> SetMetadataAsync(this StorageFile file, SpotifyTrack track)
        {
            StorageFileAbstraction taglibfile = new StorageFileAbstraction(file);
            using (var tagFile = TagLib.File.Create(taglibfile, ReadStyle.Average))
            {
                //read the raw tags
                tagFile.Tag.Title = track.Title;
                tagFile.Tag.Performers = track.Authors;
                tagFile.Tag.Album = track.Album;
                tagFile.Tag.Track = track.Number;
                tagFile.Tag.Year = (uint)Int32.Parse(track.Year);

                try
                {
                    //Set cover art
                    RandomAccessStreamReference random = RandomAccessStreamReference.CreateFromUri(track.Bitmap.UriSour‌​ce);

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
                }
                catch
                {

                }

                //Save and dispose
                tagFile.Save();
                tagFile.Dispose();
            }
            return file;
        }
        public static async Task<StorageFile> SetMetadataAsync(this StorageFile file, YouTubeVideo video)
        {
            StorageFileAbstraction taglibfile = new StorageFileAbstraction(file);
            using (var tagFile = TagLib.File.Create(taglibfile, ReadStyle.Average))
            {
                //read the raw tags
                tagFile.Tag.Title = video.Title;
                tagFile.Tag.Performers = video.Authors;
                tagFile.Tag.Album = video.Album;
                tagFile.Tag.Track = video.Number;
                tagFile.Tag.Year = (uint)Int32.Parse(video.Year);

                //Set cover art
                if (!video.SpotifyTagged)
                {
                    RandomAccessStreamReference random = RandomAccessStreamReference.CreateFromUri(video.JpgBitmap.UriSource);
                    using (IRandomAccessStream stream = await random.OpenReadAsync())
                    {
                        var dec = await BitmapDecoder.CreateAsync(stream);
                        var square = Math.Min(dec.PixelHeight, dec.PixelWidth) - 100;


                        var p = new Point((int)((dec.PixelWidth / 2) - (square / 2)), (int)((dec.PixelHeight / 2) - (square / 2)));
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
                }
                else
                {
                    RandomAccessStreamReference random = RandomAccessStreamReference.CreateFromUri(video.Bitmap.UriSour‌​ce);

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
                }
                

                //Save and dispose
                tagFile.Save();
                tagFile.Dispose();
            }
            return file;
        }
        public static async Task<YouTubeVideo> SetMetadataAsync(this YouTubeVideo video)
        {
            var track = await Settings.SpotifyClient.GetTrack(await Settings.SpotifyClient.SearchTrack(video.Title, video.DurationAsTimeSpan.TotalMilliseconds, 5));
            Debug.WriteLine($"[METADATA] Video duration: {video.DurationAsTimeSpan.TotalMilliseconds}");
            Debug.WriteLine($"[METADATA] Track duration: {track.Duration} Track link: {track.Link.Web}");
            if (video.DurationAsTimeSpan.TotalMilliseconds.IsWithinRange(track.Duration + 500, track.Duration - 1000))
            {
                Debug.WriteLine("Found corresponding track");
                video.Title = track.Title;
                video.Authors = track.Authors;
                video.Album = track.Album;
                video.Bitmap = track.Bitmap;
                video.SpotifyTagged = true;
                return video;
            }
            else
            {
                return video;
            }
        }
        public static string[] GetComponents(this string str)
        {
            str = str.Replace("FEAT.","");
            str = str.Replace(".", "");
            if (str.Contains("(") && str.Contains(")"))
            {
                int start = str.IndexOf("(");
                int end = str.IndexOf(")");
                var str1 = str.Substring(0, start);
                var str2 = str.Substring(start+1, end - start - 1);
                string[] array = new string[2] { str1, str2 };
                return array;
            }
            str = str.Replace('(', ' ');
            str = str.Replace(')', ' ');
            str = str.Replace('」', ' ');
            str = str.Replace('「', ' ');
            return str.Split(" - ");
        }
        public static bool Contains(this string str, string[] array, bool and)
        {
            bool doesContain = false;
            if (and)
            {
                
                foreach (string item in array)
                {
                    if (str.Contains(item))
                    {
                        Debug.WriteLine($"{str} contains {item}");
                        doesContain = true;
                    }
                    else {
                        Debug.WriteLine($"{str} does not contain {item}");
                        doesContain = false; }
                }
            }
            else
            {
                foreach(var item in array)
                {
                    if (str.Contains(item))
                    {
                        doesContain = true;
                    }
                }
            }
            return doesContain;
        }
    }
}
