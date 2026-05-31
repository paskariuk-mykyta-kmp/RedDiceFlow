using Avalonia.Media;
using Avalonia.Data.Converters;
using System;
using System.Globalization;
using RedDiceFlow.ViewModels;

namespace RedDiceFlow.Views
{
    public class StatusToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var status = value as string;
            return status switch
            {
                "sold" => new SolidColorBrush(Color.Parse("#4A90D9")),
                "shipped" => new SolidColorBrush(Color.Parse("#E8A838")),
                "delivered" => new SolidColorBrush(Color.Parse("#3DB86B")),
                "cancelled" => new SolidColorBrush(Color.Parse("#9E9E9E")),
                _ => new SolidColorBrush(Color.Parse("#4A90D9"))
            };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class StatusToDisplayConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var status = value as string;
            if (status == null) return "";

            var isUa = MainWindowViewModel.IsUkrainianStatic;
            return status switch
            {
                "sold" => isUa ? "Продано" : "Sold",
                "shipped" => isUa ? "Відправлено" : "Shipped",
                "delivered" => isUa ? "Доставлено" : "Delivered",
                "cancelled" => isUa ? "Скасовано" : "Cancelled",
                _ => status
            };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class StatusToShipVisibleConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value as string == "sold";

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class StatusToDeliverVisibleConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value as string == "shipped";

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class StatusToCancelVisibleConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var status = value as string;
            return status == "sold" || status == "shipped";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
