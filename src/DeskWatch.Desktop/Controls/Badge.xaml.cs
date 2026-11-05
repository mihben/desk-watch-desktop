using System.Windows;
using System.Windows.Controls;

namespace DeskWatch.Desktop.Controls
{
    public enum BadgeVariant
    {
        Neutral,
        Info,
        Success,
        Danger
    }

    /// <summary>
    /// Interaction logic for Badge.xaml
    /// </summary>
    public partial class Badge : UserControl
    {
        public static readonly DependencyProperty VariantProperty = DependencyProperty.Register(nameof(Variant), typeof(BadgeVariant), typeof(Badge), new PropertyMetadata(BadgeVariant.Neutral));

        public BadgeVariant Variant
        {
            get => (BadgeVariant)GetValue(VariantProperty);
            set => SetValue(VariantProperty, value);
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(string), typeof(Badge), new PropertyMetadata(string.Empty));

        public string Value
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public Badge()
        {
            InitializeComponent();
        }
    }
}
