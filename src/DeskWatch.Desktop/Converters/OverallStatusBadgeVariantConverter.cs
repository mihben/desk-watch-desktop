using DeskWatch.Desktop.Controls;
using DeskWatch.Desktop.ViewModels;
using System.Globalization;
using System.Windows.Data;

namespace DeskWatch.Desktop.Converters
{
    public class OverallStatusBadgeVariantConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((OverallStatus)value)
            {
                case OverallStatus.Connecting:
                    return BadgeVariant.Info;
                case OverallStatus.Connected:
                    return BadgeVariant.Success;
                case OverallStatus.Lost:
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
