using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Media_Downloader_App.Statics
{
    public class VisualTreeHelperExtensions
    {
        public static DependencyObject RecursiveGetParent(DependencyObject child, int iterations)
        {
            for (int i = 0; i < iterations; i++)
            {
                child = VisualTreeHelper.GetParent(child);
            }
            return child;
        }
        public static DependencyObject RecursiveGetFirstChild(DependencyObject parent, int iterations)
        {
            for (int i = 0; i < iterations; i++)
            {
                parent = VisualTreeHelper.GetChild(parent, 0);
            }
            return parent;
        }
    }
}
