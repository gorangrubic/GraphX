﻿using System.Windows.Data;
using System;

namespace GraphX.Converters
{
    public sealed class DoubleToLog10Converter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var val = Math.Log10((double)value);
            return double.IsNegativeInfinity(val) ? 0 : val;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var val = Math.Pow(10, (double)value);
            return double.IsNegativeInfinity(val) ? 0 : val;
        }

        #endregion
    }
}
