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
}
