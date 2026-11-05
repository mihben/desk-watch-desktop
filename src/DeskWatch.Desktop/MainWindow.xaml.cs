using DeskWatch.Desktop.Controls;
using DeskWatch.Desktop.ViewModels;
using System.ComponentModel;
using System.Windows;

namespace DeskWatch.Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel = new();
        private readonly TrayIcon _trayIcon;

        private bool _exiting;
        private bool _deviceClosed;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = _viewModel;

            _trayIcon = new TrayIcon(Open, Exit);
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;

            // Windows is shutting down, so the window must not just hide.
            Application.Current.SessionEnding += (_, _) => _exiting = true;
            Application.Current.Exit += (_, _) => _trayIcon.Dispose();
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.Status)) _trayIcon.Update(_viewModel.Status);
        }

        private void Open()
        {
            Show();
            if (WindowState == WindowState.Minimized) WindowState = WindowState.Normal;
            Activate();
        }

        private void Exit()
        {
            _exiting = true;
            Close();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel._deviceService.StartAsync();
        }

        private async void Window_Closing(object sender, CancelEventArgs e)
        {
            if (_deviceClosed) return;

            e.Cancel = true;

            if (!_exiting)
            {
                Hide();
                return;
            }

            // The window has to stay open until the running synchronization has stopped.
            await _viewModel._deviceService.CloseAsync();

            _deviceClosed = true;
            Close();
        }
    }
}
