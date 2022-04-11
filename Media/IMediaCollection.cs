using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace MP3DL.Media
{
    public interface IMediaCollection
    {
        string Title { get; }
        string Author { get;  }
        string ID { get;  }
        MediaLink Link { get; }
        uint MediaCount { get;}
        BitmapImage Bitmap { get; }
    }
}
