using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats;

namespace Media_Downloader_App.Statics
{
    public class ImageTools
    {
        public static async Task<Stream> CropAsync(Stream stream, System.Drawing.Point point, System.Drawing.Size size)
        {
            using (Image img = Image.Load(stream))
            {
                MemoryStream outstream = new MemoryStream();
                img.Mutate(i => i.Crop(new Rectangle(point.X,point.Y, size.Width,size.Height)));

                await img.SaveAsJpegAsync(outstream);
                return outstream;
            }
        }
    }
}
