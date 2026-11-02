using DeskWatch.Desktop.Models;
using Newtonsoft.Json;
using System.IO.Ports;

namespace DeskWatch.Desktop.Services
{
    public class DeviceService
    {
        private SerialPort? _serial;

        public event EventHandler<string>? Connecting;
        public event EventHandler<EventArgs>? ConnectingFailed;
        public event EventHandler<string>? Connected;

        public event EventHandler<EventModel>? DeviceResponsed;

        public async Task ConnectAsync()
        {
            foreach (var port in SerialPort.GetPortNames())
            {
                Connecting?.Invoke(this, port);

                var serial = new SerialPort(port, 9600);
                serial.Open();
                await Task.Delay(1000);

                serial.WriteLine("CONNECT");

                var response = JsonConvert.DeserializeObject<EventModel>(serial.ReadLine());
                Console.WriteLine(response);
                DeviceResponsed?.Invoke(this, response);

                Connected?.Invoke(this, port);
                _serial = serial;
            }
        }

        public void Close()
        {
            _serial?.Close();
        }
    }
}
