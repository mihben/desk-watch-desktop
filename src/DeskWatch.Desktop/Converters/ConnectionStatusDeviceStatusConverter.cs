using DeskWatch.Desktop.ViewModels;
using System.Globalization;
using System.Windows.Data;

namespace DeskWatch.Desktop.Converters
{
    public class ConnectionStatusDeviceStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((ConnectionStatus)value)
            {
                case ConnectionStatus.Connected:
                    return "ONLINE";
                default: return "OFFLINE";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
