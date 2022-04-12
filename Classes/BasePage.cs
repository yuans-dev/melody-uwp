using Windows.UI.Xaml.Controls;

namespace Media_Downloader_App.Classes
{
    public class BasePage : Page
    {
        public virtual string Header => "";
        public virtual string MinimalHeader => "";
    }
}
