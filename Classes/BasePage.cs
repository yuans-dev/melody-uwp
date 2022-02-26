using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Media_Downloader_App.Classes
{
    public class BasePage : Page
    {
        public virtual string Header => "";
        public virtual string MinimalHeader => "";
    }
}
