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
    internal class ZeroToUnknownCoverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is uint count)
            {
                if(count == 0)
                return "Unknown number of"; else return count.ToString();
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
