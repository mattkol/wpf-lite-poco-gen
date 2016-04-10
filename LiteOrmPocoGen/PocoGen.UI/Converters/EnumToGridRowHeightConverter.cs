// -----------------------------------------------------------------------
// <copyright file="EnumToGridRowHeightConverter.cs" company="Poco Generator for Lite ORMs">
// Copyright (c) Lite Poco Generator. All rights reserved. 
// </copyright>
// -----------------------------------------------------------------------

namespace PocoGen.UI.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using Models;

    /// <summary>
    /// This class represents EnumToGridRowHeightConverter class.
    /// </summary>
    public class EnumToGridRowHeightConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var gridNameSource = (DbInfoOption)values[1];
            if (values[0] == DependencyProperty.UnsetValue)
            {
                return new GridLength(0);
            }

            var gridNameValue = (DbInfoOption)values[0];
            return (gridNameValue == gridNameSource) ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return (value as string).Split(' ');
        }
    }
}