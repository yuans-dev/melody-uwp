using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace Melody.Converters
{
    internal class BoolToPlaySymbolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if(value is bool boolean)
            {
                if (boolean)
                {
                    return Symbol.Pause;
                }
                else
                {
                    return Symbol.Play;
                }
            }
            return Symbol.Play;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is Symbol symbol)
            {
                switch (symbol)
                {
                    case Symbol.Pause:
                        return true;
                    case Symbol.Play:
                        return false;
                }
            }
            return false;
        }
    }
}
