﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Melody.Converters
{
    internal class StringToUpperCaseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if(value is string str)
            {
                return str.ToUpper();
            }
            else
            {
                try
                {
                    return value.ToString().ToUpper();
                }
                catch
                {
                    return "";
                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is string str)
            {
                return str.ToLower();
            }
            else
            {
                return value.ToString().ToLower();
            }
        }
    }
}
