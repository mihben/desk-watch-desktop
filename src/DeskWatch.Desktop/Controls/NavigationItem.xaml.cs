using DeskWatch.Desktop.ViewModels;
using Material.Icons;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(NavigationItem), new PropertyMetadata(null));
        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(nameof(CommandParameter), typeof(Screen), typeof(NavigationItem), new PropertyMetadata(Screen.Dashboard));
        public Screen? CommandParameter
        {
            get => (Screen?)GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        public NavigationItem()
        {
            InitializeComponent();
        }

        private void Border_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Command?.Execute(CommandParameter);
        }
    }
}
