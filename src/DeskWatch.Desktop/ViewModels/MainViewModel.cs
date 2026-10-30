using DeskWatch.Desktop.Commands;
using System.ComponentModel;
using System.Windows.Input;

namespace DeskWatch.Desktop.ViewModels
{
    public enum Screen
    {
        Dashboard,
        Events
    }

    public class MainViewModel : INotifyPropertyChanged
    {
        private bool _dashboardIsSelected = true;
        public bool DashboardIsSelected { get { return _dashboardIsSelected; } set { _dashboardIsSelected = value; NotifyPropertyChanged(nameof(DashboardIsSelected)); } }

        private bool _eventsIsSelected = false;
        public bool EventsIsSelected { get { return _eventsIsSelected; } set { _eventsIsSelected = value; NotifyPropertyChanged(nameof(EventsIsSelected)); } }

        public ICommand NavigateCommand { get; }

        public IEnumerable<LogEntry> Events { get; }
         = new List<LogEntry> {
            new LogEntry { Timestamp = "15:12:03.965", Event = "connection.synchorinzed", Message = "Time is synchronized." },
            new LogEntry { Timestamp = "15:12:02.358", Event = "connection.established", Message = "Device is connected." },
            new LogEntry { Timestamp = "15:12:02.124", Event = "device.started", Message = "Device has been started. Waiting for connection..." }
        };

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? CanExecuteChanged;

        public MainViewModel()
        {
            NavigateCommand = new RelayCommand(parameter =>
            {
                var screen = (Screen)parameter!;
                DashboardIsSelected = screen == Screen.Dashboard;
                EventsIsSelected = screen == Screen.Events;
            });
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public record LogEntry
    {
        public required string Timestamp { get; init; }
        public required string Event { get; init; }
        public required string Message { get; init; }
    }
}
