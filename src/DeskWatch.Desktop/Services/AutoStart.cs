using Microsoft.Win32;

namespace DeskWatch.Desktop.Services
{
    public static class AutoStart
    {
        public const string HIDDEN_ARGUMENT = "--hidden";

        private const string RUN_KEY = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string VALUE_NAME = "DeskWatch";

        public static bool IsEnabled
        {
            get
            {
                using var key = Registry.CurrentUser.OpenSubKey(RUN_KEY);
                return key?.GetValue(VALUE_NAME) is not null;
            }
            set
            {
                using var key = Registry.CurrentUser.CreateSubKey(RUN_KEY);

                if (value) key.SetValue(VALUE_NAME, $"\"{Environment.ProcessPath}\" {HIDDEN_ARGUMENT}");
                else key.DeleteValue(VALUE_NAME, false);
            }
        }
    }
}
