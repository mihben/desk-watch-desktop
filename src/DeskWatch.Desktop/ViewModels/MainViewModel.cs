using DeskWatch.Desktop.Models;
using DeskWatch.Desktop.Services;
using DeskWatch.Desktop.Stores;
using System.Collections.ObjectModel;
using System.ComponentModel;

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

        public ReadOnlyObservableCollection<EventModel> Events { get; } = EventStore.Instance.Events;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? CanExecuteChanged;

        public MainViewModel()
        {
            _deviceService.Connecting += OnConnecting;
            _deviceService.ConnectingFailed += OnConnectingFailed;
            _deviceService.DeviceResponsed += OnDeviceResponded;
            _deviceService.Connected += OnConnected;
        }

        private void OnConnected(object? sender, string e)
        {
            ConnectionStatus = ConnectionStatus.Connected;
            EventStore.Instance.AddApplicationMessageEvent(0, "INF", "application.connected", "Device connected.");
        }

        private void OnDeviceResponded(object? sender, EventModel response)
        {
            EventStore.Instance.AddDeviceMessageEvent(response.Timestamp, response.Level, response.Event, response.Message);
        }

        private void OnConnectingFailed(object? sender, EventArgs e)
        {
            ConnectionStatus = ConnectionStatus.Offline;
            EventStore.Instance.AddApplicationMessageEvent(0, "ERR", "application.connecting.failed", "Connecting failed.");
        }

        private void OnConnecting(object? sender, string port)
        {
            ConnectionStatus = ConnectionStatus.Connecting;
            EventStore.Instance.AddApplicationMessageEvent(0, "INF", "application.connecting", $"Connecting to device on {port} port");
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
