using System.Text.Json.Serialization;

namespace DeskWatch.Desktop.Drivers
{
    public class DeviceEvent
    {
        public required ulong Timestamp { get; init; }
        public required string Level { get; init; }
        [JsonPropertyName("event")]
        public required string Event { get; init; }
        public required string Message { get; init; }
    }
}
