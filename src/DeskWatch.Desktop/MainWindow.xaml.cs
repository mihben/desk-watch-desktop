using DeskWatch.Desktop.ViewModels;
using System.Windows;

namespace DeskWatch.Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainViewModel();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await ((MainViewModel)DataContext)._deviceService.ConnectAsync();
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            await ((MainViewModel)DataContext)._deviceService.CloseAsync();
        }
    }
}