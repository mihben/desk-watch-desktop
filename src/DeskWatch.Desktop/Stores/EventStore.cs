using DeskWatch.Desktop.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace DeskWatch.Desktop.Stores
{
    public class EventStore
    {
        private const int MaxEventCount = 1000;

        public static EventStore Instance { get; } = new EventStore();

        private readonly ObservableCollection<EventModel> _events = [];
        public ReadOnlyObservableCollection<EventModel> Events => new(_events);

        public void AddEvent(ulong timestamp, string level, Source source, string @event, string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _events.Insert(0, new EventModel { Timestamp = timestamp, Level = level, Source = source, Event = @event, Message = message });

                if (_events.Count > MaxEventCount) _events.RemoveAt(_events.Count - 1);
            });
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
