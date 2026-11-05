using DeskWatch.Desktop.Commands;
using DeskWatch.Desktop.Drivers;
using DeskWatch.Desktop.Models;
using DeskWatch.Desktop.Services;
using DeskWatch.Desktop.Stores;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
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

    public enum OverallStatus
    {
        Offline,
        Connecting,
        Connected,
        Sleeping,
        Lost
    }

    public class MainViewModel : INotifyPropertyChanged
    {
        private const string DEVICE_SYNCHRONIZED_EVENT = "time.synchronized";
        private const string APPLICATION_SYNCHRONIZED_EVENT = "application.synchronized";

        private const string DISPLAY_READY = "READY";
        private const string DISPLAY_OFFLINE = "OFFLN";
        private const string DISPLAY_UNKNOWN = "--:--";

        public readonly DeviceService _deviceService = new DeviceService();

        private string? _port;
        private DateTime? _lastUpdated;
        private string? _displayedTime;
        private bool _synchronizationFailed;
        private bool _showSynchronizationEvents;

        public string Version { get; } = $"v{Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3)}";

        public ConnectionStatus ConnectionStatus { get; private set; } = ConnectionStatus.Offline;
        public DeviceStatus DeviceStatus { get; private set; } = DeviceStatus.Unknown;

        public OverallStatus Status => DeviceStatus == DeviceStatus.Sleeping
            ? OverallStatus.Sleeping
            : ConnectionStatus switch
            {
                ConnectionStatus.Connecting => OverallStatus.Connecting,
                ConnectionStatus.Connected => OverallStatus.Connected,
                ConnectionStatus.Lost => OverallStatus.Lost,
                _ => OverallStatus.Offline
            };

        public string StatusDetail => Status switch
        {
            OverallStatus.Connecting => "Looking for the device",
            OverallStatus.Connected when _synchronizationFailed => "Last synchronization failed",
            OverallStatus.Connected => $"{_port} · updated {_lastUpdated:HH:mm:ss}",
            OverallStatus.Sleeping => $"{_port} · display is off",
            OverallStatus.Lost => "Connection lost",
            _ when DeviceStatus == DeviceStatus.Unknown => "Device not found or sleeping",
            _ => $"{_port} · not connected"
        };

        // What the device shows, as far as the application knows.
        public string DisplayText => DeviceStatus switch
        {
            DeviceStatus.Connected => _displayedTime ?? DISPLAY_READY,
            DeviceStatus.Offline => DISPLAY_OFFLINE,
            DeviceStatus.Sleeping => string.Empty,
            _ => DISPLAY_UNKNOWN
        };

        // A device that does not answer may be sleeping, and then only waking it up helps.
        public bool CanWakeUp => ConnectionStatus == ConnectionStatus.Offline && DeviceStatus is DeviceStatus.Sleeping or DeviceStatus.Unknown;

        public bool ShowSynchronizationEvents
        {
            get { return _showSynchronizationEvents; }
            set
            {
                _showSynchronizationEvents = value;
                NotifyPropertyChanged(nameof(ShowSynchronizationEvents));
                Events.Refresh();
            }
        }

        public ICollectionView Events { get; }

        public ICommand ToggleConnectionCommand { get; }
        public ICommand ToggleSleepCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainViewModel()
        {
            _deviceService.ConnectionStatusChanged += OnConnectionStatusChanged;
            _deviceService.DeviceStatusChanged += OnDeviceStatusChanged;
            _deviceService.Synchronized += OnSynchronized;
            _deviceService.SynchronizeFailed += OnSynchronizeFailed;

            Events = CollectionViewSource.GetDefaultView(EventStore.Instance.Events);
            Events.Filter = IsEventVisible;

            ToggleConnectionCommand = new RelayCommand(
                async (_) =>
                {
                    if (ConnectionStatus == ConnectionStatus.Connected) await _deviceService.DisconnectAsync();
                    else await _deviceService.ConnectAsync();
                },
                (_) => ConnectionStatus != ConnectionStatus.Connecting && DeviceStatus != DeviceStatus.Sleeping);

            ToggleSleepCommand = new RelayCommand(
                async (_) =>
                {
                    if (CanWakeUp) await _deviceService.WakeUpAsync();
                    else await _deviceService.SleepAsync();
                },
                (_) => CanWakeUp || ConnectionStatus == ConnectionStatus.Connected || (ConnectionStatus == ConnectionStatus.Offline && _port is not null));
        }

        private bool IsEventVisible(object item)
        {
            var model = (EventModel)item;

            return _showSynchronizationEvents
                || model.Level == "ERR"
                || model.Event is not (DEVICE_SYNCHRONIZED_EVENT or APPLICATION_SYNCHRONIZED_EVENT);
        }

        private void OnDeviceStatusChanged(object? sender, DeviceStatus status)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                DeviceStatus = status;
                Refresh();
            });
        }

        private void OnSynchronized(object? sender, string displayTime)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _lastUpdated = DateTime.Now;
                _displayedTime = displayTime;
                _synchronizationFailed = false;
                Refresh();
            });
        }

        private void OnSynchronizeFailed(object? sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _synchronizationFailed = true;
                Refresh();
            });
        }

        private void OnConnectionStatusChanged(object? sender, ConnectionStatus status)
        {
            _port = _deviceService.Port;
            ConnectionStatus = status;
            _lastUpdated = DateTime.Now;

            if (status != ConnectionStatus.Connected)
            {
                _displayedTime = null;
                _synchronizationFailed = false;
            }

            if (status is ConnectionStatus.Connecting or ConnectionStatus.Lost) DeviceStatus = DeviceStatus.Unknown;

            Refresh();
        }

        private void Refresh()
        {
            NotifyPropertyChanged(nameof(ConnectionStatus));
            NotifyPropertyChanged(nameof(DeviceStatus));
            NotifyPropertyChanged(nameof(Status));
            NotifyPropertyChanged(nameof(StatusDetail));
            NotifyPropertyChanged(nameof(DisplayText));
            NotifyPropertyChanged(nameof(CanWakeUp));

            // The status changes without user input, so the buttons would not refresh on their own.
            CommandManager.InvalidateRequerySuggested();
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
