using System.IO.Ports;
using System.Text.Json;

namespace DeskWatch.Desktop.Drivers
{
    public enum DeviceStatus
    {
        Offline,
        Connected

    }

    public class DeviceDriver
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private const string CONNECT = "CONNECT";
        private const string DISCONNECT = "DISCONNECT";
        private const string SET = "SET";

        private SerialPort? _serial;

        public event EventHandler<string>? Connected;
        public event EventHandler? ConnectionFailed;
        public event EventHandler? Disconnected;
        public event EventHandler? Synchronized;

        public event EventHandler<DeviceEvent>? EventReceived;

        public async Task ConnectAsync(string port)
        {
            if (_serial is null)
            {
                _serial = new SerialPort(port, 9600);

                _serial.Open();
                _serial.ReadTimeout = 3000;
            }

            _serial.WriteLine(CONNECT);

            var response = JsonSerializer.Deserialize<DeviceEvent>(_serial.ReadLine(), _jsonSerializerOptions);
            EventReceived?.Invoke(this, response);
            Connected?.Invoke(this, port);
        }

        public async Task DisconnectAsync()
        {
            _serial.WriteLine(DISCONNECT);

            var response = JsonSerializer.Deserialize<DeviceEvent>(_serial.ReadLine(), _jsonSerializerOptions);
            EventReceived?.Invoke(this, response);
            Disconnected?.Invoke(this, EventArgs.Empty);
        }

        public async Task SynchronizeAsync(string time)
        {
            _serial.WriteLine($"{SET} {time}");

            var response = JsonSerializer.Deserialize<DeviceEvent>(_serial.ReadLine(), _jsonSerializerOptions);
            EventReceived?.Invoke(this, response);
            Synchronized?.Invoke(this, EventArgs.Empty);
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
