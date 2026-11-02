using DeskWatch.Desktop.Controls;
using DeskWatch.Desktop.ViewModels;
using System.Globalization;
using System.Windows.Data;

namespace DeskWatch.Desktop.Converters
{
    public class ConnectionStatusBadgeVariantConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = (ConnectionStatus)value;

            switch (status)
            {
                case ConnectionStatus.Offline:
                    return BadgeVariant.Neutral;
                case ConnectionStatus.Connecting:
                    return BadgeVariant.Info;
                case ConnectionStatus.Connected:
                    return BadgeVariant.Success;
                case ConnectionStatus.Lost:
                    return BadgeVariant.Danger;
                default: return BadgeVariant.Neutral;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
