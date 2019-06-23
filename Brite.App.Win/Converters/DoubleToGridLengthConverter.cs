﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Brite.App.Win.Converters
{
    // From: https://hg01.codeplex.com/sambapos
    /// <summary>
    /// GridLength to double value converter
    /// </summary>
    [ValueConversion(typeof(GridLength), typeof(double))]
    public class DoubleToGridLengthConverter : IValueConverter
    {
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType == typeof(GridLength))
            {
                if (value == null)
                    return GridLength.Auto;
                if (value is double)
                    return new GridLength((double)value);
                return GridLength.Auto;
            }
            if (targetType == typeof(double))
            {
                if (value is GridLength)
                    return ((GridLength)value).Value;
                return double.NaN;
            }
            return null;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ConvertBack(value, targetType, parameter, culture);
        }
    }
}
