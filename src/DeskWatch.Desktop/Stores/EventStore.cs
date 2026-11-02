using DeskWatch.Desktop.Models;
using System.Collections.ObjectModel;

namespace DeskWatch.Desktop.Stores
{
    public class EventStore
    {
        public static EventStore Instance { get; } = new EventStore();

        private readonly ObservableCollection<EventModel> _events = [];
        public ReadOnlyObservableCollection<EventModel> Events => new(_events);

        public void AddEvent(ulong timestamp, string level, Source source, string @event, string message)
        {
            _events.Insert(0, new EventModel { Timestamp = timestamp, Level = level, Source = source, Event = @event, Message = message });
        }

        public void AddApplicationMessageEvent(ulong timestamp, string level, string @event, string message)
        {
            AddEvent(timestamp, level, Source.Application, @event, message);
        }

        public void AddDeviceMessageEvent(ulong timestamp, string level, string @event, string message)
        {
            AddEvent(timestamp, level, Source.Device, @event, message);
        }
    }
}
