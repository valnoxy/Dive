using System.Windows;

namespace Dive.UI.Common.UserInterface
{
    /// <summary>
    /// Interaktionslogik für Skeleton.xaml
    /// </summary>
    public partial class Skeleton
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(Skeleton));
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(string), typeof(Skeleton));
        public static readonly DependencyProperty SizeProperty =
            DependencyProperty.Register(nameof(Size), typeof(string), typeof(Skeleton));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Value
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public string Size
        {
            get => (string)GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }

        public Skeleton()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void Switch()
        {
            SkeletonGrid.Visibility = SkeletonGrid.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
            ContentGrid.Visibility = ContentGrid.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
