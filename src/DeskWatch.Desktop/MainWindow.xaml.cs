using DeskWatch.Desktop.ViewModels;
using System.Windows;

namespace DeskWatch.Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _deviceClosed;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainViewModel();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await ((MainViewModel)DataContext)._deviceService.StartAsync();
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_deviceClosed) return;

            // The window has to stay open until the running synchronization has stopped.
            e.Cancel = true;
            await ((MainViewModel)DataContext)._deviceService.CloseAsync();

            _deviceClosed = true;
            Close();
        }
    }
}