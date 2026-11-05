using DeskWatch.Desktop.Drivers;
using DeskWatch.Desktop.Stores;
using DeskWatch.Desktop.ViewModels;
using System.IO.Ports;
using System.Text.Json;
using System.Windows;

namespace DeskWatch.Desktop.Services
{
    public class DeviceService
    {
        private readonly DeviceDriver _driver = new();
        private readonly SemaphoreSlim _driverLock = new(1, 1);
        private readonly SemaphoreSlim _operationLock = new(1, 1);
        private CancellationTokenSource? _autoConnectCancellation;
        private Task? _autoConnectTask;
        private bool _autoConnect = true;
        private CancellationTokenSource? _synchronizationCancellation;
        private Task? _synchronizationTask;

        private ConnectionStatus _status = ConnectionStatus.Offline;
        public ConnectionStatus Status { get { return _status; } set { _status = value; ConnectionStatusChanged?.Invoke(this, value); } }

        public string? Port { get; private set; }

        public event EventHandler<ConnectionStatus>? ConnectionStatusChanged;
        public event EventHandler<DeviceStatus>? DeviceStatusChanged;
        public event EventHandler<string>? Synchronized;
        public event EventHandler? SynchronizeFailed;

        public DeviceService()
        {
            _driver.Connected += OnConnected;
            _driver.Disconnected += OnDisconnected;
            _driver.EventReceived += OnEventReceived;
            _driver.Synchronized += OnSynchronized;
            _driver.DeviceStatusChanged += OnDeviceStatusChanged;
        }

        private void OnDeviceStatusChanged(object? sender, DeviceStatus status)
        {
            DeviceStatusChanged?.Invoke(this, status);
        }

        private void OnSynchronized(object? sender, string displayTime)
        {
            Application.Current.Dispatcher.Invoke(() =>
            EventStore.Instance.AddApplicationMessageEvent(0, "INF", "application.synchronized", "Time is synchronized."));
            Synchronized?.Invoke(this, displayTime);
        }

        private void OnDisconnected(object? sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() => Status = ConnectionStatus.Offline);
            EventStore.Instance.AddApplicationMessageEvent(0, "INF", "application.disconnected", "Device disconnected.");
        }

        private void OnEventReceived(object? sender, DeviceEvent e)
        {
            EventStore.Instance.AddDeviceMessageEvent(e.Timestamp, e.Level, e.Event, e.Message);
        }

        private void OnConnected(object? sender, string port)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Port = port;
                Status = ConnectionStatus.Connected;
                EventStore.Instance.AddApplicationMessageEvent(0, "INF", "application.connected", $"Device is connected on {port} port.");

                StartPeriodicSynchronization();
            });
        }

        // The driver blocks while it waits for the device and handles one request at a time.
        private async Task RunOnDriverAsync(Func<Task> action)
        {
            await _driverLock.WaitAsync();
            try
            {
                await Task.Run(action);
            }
            finally
            {
                _driverLock.Release();
            }
        }

        private void StartPeriodicSynchronization()
        {
            if (_synchronizationCancellation is not null) return;

            _synchronizationCancellation = new CancellationTokenSource();
            var cancellationToken = _synchronizationCancellation.Token;
            _synchronizationTask = Task.Run(() => PeriodicSynchronizeAsync(cancellationToken));
        }

        private async Task StopPeriodicSynchronizationAsync()
        {
            if (_synchronizationCancellation is null) return;

            _synchronizationCancellation.Cancel();
            await _synchronizationTask!;

            _synchronizationCancellation.Dispose();
            _synchronizationCancellation = null;
            _synchronizationTask = null;
        }

        private async Task PeriodicSynchronizeAsync(CancellationToken cancellationToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(500));
            try
            {
                while (await timer.WaitForNextTickAsync(cancellationToken) && Status == ConnectionStatus.Connected)
                {
                    await SynchronizeAsync();
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        public async Task StartAsync()
        {
            _autoConnectCancellation = new CancellationTokenSource();
            var cancellationToken = _autoConnectCancellation.Token;
            _autoConnectTask = Task.Run(() => AutoConnectAsync(cancellationToken));

            await ConnectAsync();
        }

        private async Task AutoConnectAsync(CancellationToken cancellationToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
            try
            {
                while (await timer.WaitForNextTickAsync(cancellationToken))
                {
                    await RunOperationAsync(async () =>
                    {
                        if (_autoConnect && Status is ConnectionStatus.Offline or ConnectionStatus.Lost) await ConnectToDeviceAsync(true);
                    });
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        // The user's requests and the automatic connecting must not interleave.
        private async Task RunOperationAsync(Func<Task> operation)
        {
            await _operationLock.WaitAsync();
            try
            {
                await operation();
            }
            finally
            {
                _operationLock.Release();
            }
        }

        public Task ConnectAsync()
        {
            return RunOperationAsync(() =>
            {
                _autoConnect = true;
                return ConnectToDeviceAsync(false);
            });
        }

        // An automatic attempt stays quiet, otherwise a missing device would flood the log and make the status flicker.
        private async Task ConnectToDeviceAsync(bool automatic)
        {
            await StopPeriodicSynchronizationAsync();

            foreach (var port in GetPorts(automatic))
            {
                if (!automatic)
                {
                    Status = ConnectionStatus.Connecting;
                    EventStore.Instance.AddApplicationMessageEvent(0, "INF", "application.connecting", $"Connecting to device on {port} port.");
                }

                try
                {
                    await RunOnDriverAsync(() => _driver.ConnectAsync(port));
                    _autoConnect = true;
                    return;
                }
                catch (DeviceCommandRejectedException ex)
                {
                    // The device is there but refuses to connect, for example because it is sleeping, so retrying is pointless.
                    _autoConnect = false;
                    EventStore.Instance.AddApplicationMessageEvent(0, "ERR", "application.connection.failed", $"Connecting was failed on {port} port. {ex.Message}");
                }
                catch (Exception ex)
                {
                    if (!automatic) EventStore.Instance.AddApplicationMessageEvent(0, "ERR", "application.connection.failed", $"Connecting was failed on {port} port. {ex.Message}");
                }
            }

            if (!automatic) Status = ConnectionStatus.Offline;
        }

        // An automatic attempt leaves the other ports alone while the last working port exists.
        private string[] GetPorts(bool automatic)
        {
            var ports = SerialPort.GetPortNames();

            if (Port is null || !ports.Contains(Port)) return ports;
            if (automatic) return [Port];

            return [Port, .. ports.Where(port => port != Port)];
        }

        public Task DisconnectAsync()
        {
            return RunOperationAsync(() =>
            {
                _autoConnect = false;
                return DisconnectFromDeviceAsync();
            });
        }

        private async Task DisconnectFromDeviceAsync()
        {
            await StopPeriodicSynchronizationAsync();

            try
            {
                await RunOnDriverAsync(_driver.DisconnectAsync);
            }
            catch (DeviceCommandRejectedException ex)
            {
                EventStore.Instance.AddApplicationMessageEvent(0, "ERR", "application.disconnection.rejected", $"The device rejected disconnecting. {ex.Message}");
                Status = ConnectionStatus.Offline;
            }
            catch (Exception ex)
            {
                EventStore.Instance.AddApplicationMessageEvent(0, "ERR", "application.disconnection.failed", $"Disconnecting failed. {ex.Message}");
                Status = ConnectionStatus.Lost;
            }
        }

        public Task SleepAsync()
        {
            return RunOperationAsync(() =>
            {
                _autoConnect = false;
                return PutDeviceToSleepAsync();
            });
        }

        private async Task PutDeviceToSleepAsync()
        {
            // The device refuses to sleep while it is connected.
            if (Status == ConnectionStatus.Connected) await DisconnectFromDeviceAsync();
            if (Status != ConnectionStatus.Offline) return;

            try
            {
                await RunOnDriverAsync(_driver.SleepAsync);
                EventStore.Instance.AddApplicationMessageEvent(0, "INF", "application.asleep", "Device is sleeping.");
            }
            catch (DeviceCommandRejectedException ex)
            {
                EventStore.Instance.AddApplicationMessageEvent(0, "ERR", "application.sleep.rejected", $"The device rejected sleeping. {ex.Message}");
            }
            catch (Exception ex)
            {
                EventStore.Instance.AddApplicationMessageEvent(0, "ERR", "application.sleep.failed", $"Sleeping failed. {ex.Message}");
                Status = ConnectionStatus.Lost;
            }
        }

        public Task WakeUpAsync()
        {
            return RunOperationAsync(WakeUpDeviceAsync);
        }

        // A device that was already sleeping when the application started is on an unknown port.
        private async Task WakeUpDeviceAsync()
        {
            foreach (var port in GetPorts(false))
            {
                try
                {
                    await RunOnDriverAsync(() => _driver.WakeUpAsync(port));

                    Port = port;
                    EventStore.Instance.AddApplicationMessageEvent(0, "INF", "application.wokeup", $"Device woke up on {port} port.");
                    Status = ConnectionStatus.Offline;

                    _autoConnect = true;
                    await ConnectToDeviceAsync(false);
                    return;
                }
                catch (Exception ex)
                {
                    EventStore.Instance.AddApplicationMessageEvent(0, "ERR", "application.wakeup.failed", $"Waking up failed on {port} port. {ex.Message}");
                }
            }
        }

        public async Task SynchronizeAsync()
        {
            try
            {
                await RunOnDriverAsync(() => _driver.SynchronizeAsync(DateTime.Now));
            }
            catch (Exception ex) when (ex is TimeoutException or JsonException)
            {
                EventStore.Instance.AddApplicationMessageEvent(0, "ERR", "application.synchronization.failed", $"Synchronization failed. {ex.Message}");
                SynchronizeFailed?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                // A rejected SET means the device is not connected anymore, for example after a timeout or a restart.
                EventStore.Instance.AddApplicationMessageEvent(0, "ERR", "application.connection.lost", $"Connection is lost. {ex.Message}");
                SynchronizeFailed?.Invoke(this, EventArgs.Empty);
                Application.Current.Dispatcher.Invoke(() => Status = ConnectionStatus.Lost);
            }
        }

        public async Task CloseAsync()
        {
            if (_autoConnectCancellation is not null)
            {
                _autoConnectCancellation.Cancel();
                await _autoConnectTask!;
            }

            if (Status == ConnectionStatus.Connected) await DisconnectAsync();
            else await StopPeriodicSynchronizationAsync();

            await RunOnDriverAsync(_driver.CloseAsync);
        }
    }
}
