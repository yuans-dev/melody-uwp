using Melody.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Melody.Converters
{
    internal class IsValidConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string str)
            {
                if(str =="9999")
                return Visibility.Collapsed; else return Visibility.Visible;
            }
            else
            {
                return value.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is string str)
            {
                return str.ToArray();
            }
            else
            {
                return value;
            }
        }
    }
}
