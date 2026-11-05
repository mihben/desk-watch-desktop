using System.Windows;

namespace DeskWatch.Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const string INSTANCE_MUTEX_NAME = @"Local\DeskWatch.Desktop.Instance";
        private const string OPEN_EVENT_NAME = @"Local\DeskWatch.Desktop.Open";

        private Mutex? _instanceMutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var openEvent = new EventWaitHandle(false, EventResetMode.AutoReset, OPEN_EVENT_NAME);

            _instanceMutex = new Mutex(true, INSTANCE_MUTEX_NAME, out var isFirstInstance);
            if (!isFirstInstance)
            {
                // Only one instance can use the serial port, so the running one takes over.
                openEvent.Set();
                Shutdown();
                return;
            }

            var window = new MainWindow();
            window.Show();

            ThreadPool.RegisterWaitForSingleObject(openEvent, (_, _) => Dispatcher.BeginInvoke(window.Open), null, Timeout.Infinite, false);
        }
    }
}
