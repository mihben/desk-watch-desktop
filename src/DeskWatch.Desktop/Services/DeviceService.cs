using DeskWatch.Desktop.Drivers;
using DeskWatch.Desktop.Stores;
using DeskWatch.Desktop.ViewModels;
using System.IO.Ports;
using System.Windows;

namespace DeskWatch.Desktop.Services
{
    public class DeviceService
    {
        private readonly DeviceDriver _driver = new();
        private readonly PeriodicTimer _timer;
        private Task _timerTask;

        private ConnectionStatus _status = ConnectionStatus.Offline;
        public ConnectionStatus Status { get { return _status; } set { _status = value; ConnectionStatusChanged?.Invoke(this, value); } }

        public string? Port { get; private set; }

        public event EventHandler<ConnectionStatus>? ConnectionStatusChanged;
        public event EventHandler? Synchronized;

        public DeviceService()
        {
            _driver.Connected += OnConnected;
            _driver.Disconnected += OnDisconnected;
            _driver.EventReceived += OnEventReceived;
            _driver.Synchronized += OnSynchronized;
        }

        private void OnSynchronized(object? sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            EventStore.Instance.AddApplicationMessageEvent(0, "INF", "application.synchronized", "Time is synchronized."));
            Synchronized?.Invoke(this, EventArgs.Empty);
        }

        private void OnDisconnected(object? sender, EventArgs e)
        {
            Status = ConnectionStatus.Offline;
            EventStore.Instance.AddApplicationMessageEvent(0, "INF", "application.disconnected", "Device disconnected.");
        }

        private void OnEventReceived(object? sender, DeviceEvent e)
        {
            EventStore.Instance.AddDeviceMessageEvent(e.Timestamp, e.Level, e.Event, e.Message);
        }

        private void OnConnected(object? sender, string port)
        {
            Port = port;
            Status = ConnectionStatus.Connected;
            EventStore.Instance.AddApplicationMessageEvent(0, "INF", "application.connected", $"Device is connected on {port} port.");

            _timerTask = Task.Run(PeriodicSynchronizeAsync);
        }

        public async Task PeriodicSynchronizeAsync()
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(500));
            while (true)
            {
                await timer.WaitForNextTickAsync();
                await SynchronizeAsync();
            }
        }

        public async Task ConnectAsync()
        {
            foreach (var port in SerialPort.GetPortNames())
            {
                Status = ConnectionStatus.Connecting;
                EventStore.Instance.AddApplicationMessageEvent(0, "INF", "application.connecting", $"Connecting to device on {port} port.");

                try
                {
                    await _driver.ConnectAsync(port);
                }
                catch (Exception ex)
                {
                    EventStore.Instance.AddApplicationMessageEvent(0, "ERR", "application.connection.failed", $"Connecting was failed on {port} port. {ex.Message}");
                }
            }
        }

        public async Task DisconnectAsync()
        {
            await _driver.DisconnectAsync();
        }

        public async Task SynchronizeAsync()
        {
            var time = DateTime.Now;
            if (time.Second % 2 == 0) await _driver.SynchronizeAsync($"{time:HH:mm}");
            else await _driver.SynchronizeAsync($"{time:HH mm}");
        }

        public async Task CloseAsync()
        {
            await _driver.DisconnectAsync();
            await _driver.CloseAsync();
        }
    }
}
