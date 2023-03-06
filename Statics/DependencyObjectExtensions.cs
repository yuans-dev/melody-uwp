using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Melody.Statics
{
    public static class DependencyObjectExtensions
    {
        public static DependencyObject RecursiveGetParent(this DependencyObject child, int iterations)
        {
            for (int i = 0; i < iterations; i++)
            {
                child = VisualTreeHelper.GetParent(child);
            }
            return child;
        }
        public static DependencyObject RecursiveGetFirstChild(this DependencyObject parent, int iterations)
        {
            for (int i = 0; i < iterations; i++)
            {
                parent = VisualTreeHelper.GetChild(parent, 0);
            }
            return parent;
        }
    }
}
