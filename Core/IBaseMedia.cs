using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace Melody.Core
{
    public interface IBaseMedia
    {
        string Title { get; set; }
        string[] Authors { get; set; }
        string Name { get; }
        MediaID ID { get;}
        BitmapImage Bitmap { get; }
    }
    public class Media : IBaseMedia
    {
        private Media()
        {
        }
        public static Media Empty()
        {
            return new Media();
        }
        public string Title { get; set; }
        public string[] Authors { get; set; }
        public string Name { get; }

        public MediaID ID { get; set; }

        public BitmapImage Bitmap { get; set; }
    }
}
