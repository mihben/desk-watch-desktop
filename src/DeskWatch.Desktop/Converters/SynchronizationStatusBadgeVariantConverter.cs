using DeskWatch.Desktop.Controls;
using DeskWatch.Desktop.ViewModels;
using System.Globalization;
using System.Windows.Data;

namespace DeskWatch.Desktop.Converters
{
    public class SynchronizationStatusBadgeVariantConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((SynchronizationStatus)value)
            {
                case SynchronizationStatus.Synced:
                    return BadgeVariant.Success;
                case SynchronizationStatus.Failed:
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
