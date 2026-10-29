using Material.Icons;
using System.Windows;
using System.Windows.Controls;

namespace DeskWatch.Desktop.Controls
{
    /// <summary>
    /// Interaction logic for NavigationItem.xaml
    /// </summary>
    public partial class NavigationItem : UserControl
    {
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title), typeof(string), typeof(NavigationItem), new PropertyMetadata(string.Empty));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(nameof(Icon), typeof(MaterialIconKind), typeof(NavigationItem), new PropertyMetadata(MaterialIconKind.QuestionMark));

        public MaterialIconKind Icon
        {
            get => (MaterialIconKind)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }
        public static readonly DependencyProperty SelectedProperty = DependencyProperty.Register(nameof(Selected), typeof(bool), typeof(NavigationItem), new PropertyMetadata(false));

        public bool Selected
        {
            get => (bool)GetValue(SelectedProperty);
            set => SetValue(SelectedProperty, value);
        }

        public NavigationItem()
        {
            InitializeComponent();
        }
    }
}
