using DeskWatch.Desktop.Commands;
using DeskWatch.Desktop.Models;
using DeskWatch.Desktop.Services;
using DeskWatch.Desktop.Stores;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace DeskWatch.Desktop.ViewModels
{
    public enum ConnectionStatus
    {
        Offline,
        Connecting,
        Connected,
        Lost
    }

    public class MainViewModel : INotifyPropertyChanged
    {
        public readonly DeviceService _deviceService = new DeviceService();

        private bool _dashboardIsSelected = true;
        public bool DashboardIsSelected { get { return _dashboardIsSelected; } set { _dashboardIsSelected = value; NotifyPropertyChanged(nameof(DashboardIsSelected)); } }

        private bool _eventsIsSelected = false;
        public bool EventsIsSelected { get { return _eventsIsSelected; } set { _eventsIsSelected = value; NotifyPropertyChanged(nameof(EventsIsSelected)); } }

        private ConnectionStatus _connectionStatus = ConnectionStatus.Offline;
        public ConnectionStatus ConnectionStatus { get { return _connectionStatus; } set { _connectionStatus = value; NotifyPropertyChanged(nameof(ConnectionStatus)); } }

        public string? _port;
        public string? Port { get { return _port; } set { _port = value; NotifyPropertyChanged(nameof(Port)); } }

        public DateTime? _lastUpdate;
        public DateTime? LastUpdated { get { return _lastUpdate; } set { _lastUpdate = value; NotifyPropertyChanged(nameof(LastUpdated)); } }

        public ICommand ConnectCommand { get; }
        public ICommand DisconnectCommand { get; }
        public ICommand SynchronizeCommand { get; }

        public ReadOnlyObservableCollection<EventModel> Events { get; } = EventStore.Instance.Events;

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainViewModel()
        {
            _deviceService.ConnectionStatusChanged += OnConnectionStatusChanged;
            _deviceService.Synchronized += OnSynchronized;

            ConnectCommand = new RelayCommand(async (_) => await _deviceService.ConnectAsync(), (_) => ConnectionStatus == ConnectionStatus.Offline);
            DisconnectCommand = new RelayCommand(async (_) => await _deviceService.DisconnectAsync(), (_) => ConnectionStatus == ConnectionStatus.Connected);
            SynchronizeCommand = new RelayCommand(async (_) => await _deviceService.SynchronizeAsync(), (_) => ConnectionStatus == ConnectionStatus.Connected);
        }

        private void OnSynchronized(object? sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() => LastUpdated = DateTime.Now);
        }

        private void OnConnectionStatusChanged(object? sender, ConnectionStatus status)
        {
            Port = _deviceService.Port;
            ConnectionStatus = status;
            LastUpdated = DateTime.Now;
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
