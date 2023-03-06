using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.IO;
using System.Threading.Tasks;

namespace Melody.Statics
{
    public class ImageTools
    {
        public static async Task<Stream> CropAsync(Stream stream, System.Drawing.Point point, System.Drawing.Size size)
        {
            using (Image img = Image.Load(stream))
            {
                MemoryStream outstream = new MemoryStream();
                img.Mutate(i => i.Crop(new Rectangle(point.X, point.Y, size.Width, size.Height)));

                await img.SaveAsJpegAsync(outstream);
                return outstream;
            }
        }
    }
}
