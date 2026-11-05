using DeskWatch.Desktop.ViewModels;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace DeskWatch.Desktop.Controls
{
    public sealed class TrayIcon : IDisposable
    {
        private const int ICON_SIZE = 32;

        private readonly Icon _neutralIcon = CreateIcon(ColorTranslator.FromHtml("#B8C7D9"));
        private readonly Icon _infoIcon = CreateIcon(ColorTranslator.FromHtml("#70B5FF"));
        private readonly Icon _successIcon = CreateIcon(ColorTranslator.FromHtml("#20D6A0"));
        private readonly Icon _dangerIcon = CreateIcon(ColorTranslator.FromHtml("#EF4444"));

        private readonly NotifyIcon _notifyIcon = new();

        public TrayIcon(Action open, Action exit)
        {
            var menu = new ContextMenuStrip();
            menu.Items.Add("Open", null, (_, _) => open());
            menu.Items.Add("Exit", null, (_, _) => exit());

            _notifyIcon.ContextMenuStrip = menu;
            _notifyIcon.MouseClick += (_, e) =>
            {
                if (e.Button == MouseButtons.Left) open();
            };

            Update(OverallStatus.Offline);
            _notifyIcon.Visible = true;
        }

        public void Update(OverallStatus status)
        {
            _notifyIcon.Text = $"DeskWatch: {status}";
            _notifyIcon.Icon = status switch
            {
                OverallStatus.Connecting => _infoIcon,
                OverallStatus.Connected => _successIcon,
                OverallStatus.Lost => _dangerIcon,
                _ => _neutralIcon
            };
        }

        private static Icon CreateIcon(Color color)
        {
            using var bitmap = new Bitmap(ICON_SIZE, ICON_SIZE);
            using var graphics = Graphics.FromImage(bitmap);
            using var brush = new SolidBrush(color);

            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.FillEllipse(brush, 4, 4, ICON_SIZE - 8, ICON_SIZE - 8);

            return Icon.FromHandle(bitmap.GetHicon());
        }

        public void Dispose()
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
        }
    }
}
