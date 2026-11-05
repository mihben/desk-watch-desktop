using System.IO.Ports;
using System.Text.Json;

namespace DeskWatch.Desktop.Drivers
{
    public enum DeviceStatus
    {
        Unknown,
        Offline,
        Connected
    }

    public class DeviceDriver
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private static class Commands
        {
            public const string CONNECT = "CONNECT";
            public const string DISCONNECT = "DISCONNECT";
            public const string SET = "SET";
        }

        private static class Events
        {
            public const string DEVICE_STARTED = "device.started";
            public const string CONNECTION_ESTABLISHED = "connection.established";
            public const string CONNECTION_REJECTED = "connection.command.rejected";
            public const string CONNECTION_CLOSED = "connection.closed";
            public const string CONNECTION_LOST = "connection.lost";
            public const string DISCONNECTION_REJECTED = "disconnection.command.rejected";
            public const string TIME_SYNCHRONIZED = "time.synchronized";
            public const string SET_REJECTED = "set.command.rejected";
        }

        private const int MAX_RESPONSE_LINES = 10;

        private SerialPort? _serial;

        public event EventHandler<string>? Connected;
        public event EventHandler? ConnectionFailed;
        public event EventHandler? Disconnected;
        public event EventHandler<DateTime>? Synchronized;
        public event EventHandler<DeviceStatus>? DeviceStatusChanged;

        public event EventHandler<DeviceEvent>? EventReceived;

        public async Task ConnectAsync(string port)
        {
            if (_serial is not null)
            {
                _serial.Close();
                _serial = null;
            }

            _serial = new SerialPort(port, 9600);

            _serial.Open();
            _serial.ReadTimeout = 3000;

            SendCommand(Commands.CONNECT, Events.CONNECTION_ESTABLISHED, Events.CONNECTION_REJECTED);
            Connected?.Invoke(this, port);
        }

        public async Task DisconnectAsync()
        {
            SendCommand(Commands.DISCONNECT, Events.CONNECTION_CLOSED, Events.DISCONNECTION_REJECTED);
            Disconnected?.Invoke(this, EventArgs.Empty);
        }

        public async Task SynchronizeAsync(DateTime time)
        {
            var displayTime = time.Second % 2 == 0 ? $"{time:HH:mm}" : $"{time:HH mm}";

            SendCommand($"{Commands.SET} {displayTime}", Events.TIME_SYNCHRONIZED, Events.SET_REJECTED);
            Synchronized?.Invoke(this, time);
        }

        // The device also logs on its own, so a response is recognized by its event and not by its position.
        private void SendCommand(string command, string successEvent, string rejectedEvent)
        {
            var serial = _serial ?? throw new InvalidOperationException("The serial port is not open.");

            serial.WriteLine(command);

            for (var i = 0; i < MAX_RESPONSE_LINES; i++)
            {
                var response = JsonSerializer.Deserialize<DeviceEvent>(serial.ReadLine(), _jsonSerializerOptions)
                    ?? throw new JsonException("The device sent an empty response.");

                EventReceived?.Invoke(this, response);
                UpdateDeviceStatus(response.Event);

                if (response.Event == successEvent) return;
                if (response.Event == rejectedEvent) throw new DeviceCommandRejectedException(response.Message);

                // A command sent while the device was restarting is lost.
                if (response.Event == Events.DEVICE_STARTED) serial.WriteLine(command);
            }

            throw new TimeoutException($"No response to the {command} command.");
        }

        private void UpdateDeviceStatus(string deviceEvent)
        {
            switch (deviceEvent)
            {
                case Events.DEVICE_STARTED:
                case Events.CONNECTION_CLOSED:
                case Events.CONNECTION_LOST:
                    DeviceStatusChanged?.Invoke(this, DeviceStatus.Offline);
                    break;
                case Events.CONNECTION_ESTABLISHED:
                    DeviceStatusChanged?.Invoke(this, DeviceStatus.Connected);
                    break;
            }
        }

        public Task CloseAsync()
        {
            if (_serial is null) return Task.CompletedTask;

            _serial.Close();
            _serial.Dispose();

            return Task.CompletedTask;
        }
    }
}
