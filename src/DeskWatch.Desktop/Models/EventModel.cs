namespace DeskWatch.Desktop.Models
{
    public enum Source
    {
        Application,
        Device
    }

    public record EventModel
    {
        public required ulong Timestamp { get; init; }
        public required string Level { get; init; }
        public Source Source { get; set; }
        public required string Event { get; init; }
        public required string Message { get; init; }
    }
}
