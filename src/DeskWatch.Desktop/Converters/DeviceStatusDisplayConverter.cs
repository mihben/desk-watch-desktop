using DeskWatch.Desktop.Drivers;
using System.Globalization;
using System.Windows.Data;

namespace DeskWatch.Desktop.Converters
{
    public class DeviceStatusDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((DeviceStatus)value)
            {
                case DeviceStatus.Offline:
                case DeviceStatus.Connected:
                    return "Active";
                case DeviceStatus.Sleeping:
                    return "Off";
                default: return "Unknown";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
