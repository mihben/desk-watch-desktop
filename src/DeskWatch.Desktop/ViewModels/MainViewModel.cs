using DeskWatch.Desktop.Commands;
using DeskWatch.Desktop.Drivers;
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

    public enum SynchronizationStatus
    {
        Idle,
        Synced,
        Failed
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

        private DateTime? _clockTime;
        public DateTime? ClockTime { get { return _clockTime; } set { _clockTime = value; NotifyPropertyChanged(nameof(ClockTime)); } }

        private DeviceStatus _deviceStatus = DeviceStatus.Unknown;
        public DeviceStatus DeviceStatus { get { return _deviceStatus; } set { _deviceStatus = value; NotifyPropertyChanged(nameof(DeviceStatus)); } }

        private SynchronizationStatus _synchronizationStatus = SynchronizationStatus.Idle;
        public SynchronizationStatus SynchronizationStatus { get { return _synchronizationStatus; } set { _synchronizationStatus = value; NotifyPropertyChanged(nameof(SynchronizationStatus)); } }

        public ICommand ConnectCommand { get; }
        public ICommand DisconnectCommand { get; }
        public ICommand SleepCommand { get; }
        public ICommand SynchronizeCommand { get; }

        public ReadOnlyObservableCollection<EventModel> Events { get; } = EventStore.Instance.Events;

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainViewModel()
        {
            _deviceService.ConnectionStatusChanged += OnConnectionStatusChanged;
            _deviceService.DeviceStatusChanged += OnDeviceStatusChanged;
            _deviceService.Synchronized += OnSynchronized;
            _deviceService.SynchronizeFailed += OnSynchronizeFailed;

            ConnectCommand = new RelayCommand(async (_) => await _deviceService.ConnectAsync(), (_) => ConnectionStatus is ConnectionStatus.Offline or ConnectionStatus.Lost);
            DisconnectCommand = new RelayCommand(async (_) => await _deviceService.DisconnectAsync(), (_) => ConnectionStatus == ConnectionStatus.Connected);
            SleepCommand = new RelayCommand(async (_) => await _deviceService.SleepAsync(), (_) => ConnectionStatus == ConnectionStatus.Connected || (ConnectionStatus == ConnectionStatus.Offline && Port is not null && DeviceStatus != DeviceStatus.Sleeping));
            SynchronizeCommand = new RelayCommand(async (_) => await _deviceService.SynchronizeAsync(), (_) => ConnectionStatus == ConnectionStatus.Connected);
        }

        private void OnDeviceStatusChanged(object? sender, DeviceStatus status)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                DeviceStatus = status;
                CommandManager.InvalidateRequerySuggested();
            });
        }

        private void OnSynchronized(object? sender, DateTime time)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                LastUpdated = DateTime.Now;
                ClockTime = time;
                SynchronizationStatus = SynchronizationStatus.Synced;
            });
        }

        private void OnSynchronizeFailed(object? sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() => SynchronizationStatus = SynchronizationStatus.Failed);
        }

        private void OnConnectionStatusChanged(object? sender, ConnectionStatus status)
        {
            Port = _deviceService.Port;
            ConnectionStatus = status;
            LastUpdated = DateTime.Now;

            if (status != ConnectionStatus.Connected) ClockTime = null;
            if (status is ConnectionStatus.Connecting or ConnectionStatus.Lost) DeviceStatus = DeviceStatus.Unknown;
            if (status is ConnectionStatus.Connecting or ConnectionStatus.Offline) SynchronizationStatus = SynchronizationStatus.Idle;

            // The status changes without user input, so the buttons would not refresh on their own.
            CommandManager.InvalidateRequerySuggested();
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
