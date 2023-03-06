using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace Melody.Core
{
    public class MediaItem
    {
        public MediaItem()
        {

        }
        public MediaItem(IBaseMedia item)
        {
            Title = item.Title;
            Bitmap = item.Bitmap;
            ID = item.ID;
        }
        public string Title { get; set; }
        public MediaID ID { get; set; }
        public BitmapImage Bitmap { get; set; }
    }
}
