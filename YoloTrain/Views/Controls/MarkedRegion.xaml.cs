using System.Windows;
using YoloTrain.Mvvm;

namespace YoloTrain.Views.Controls
{
    /// <summary>
    /// Interaction logic for MarkedRegion.xaml
    /// </summary>
    public partial class MarkedRegion : UserControlView
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(MarkedRegion), new PropertyMetadata());

        public static readonly DependencyProperty RegionIndexProperty =
            DependencyProperty.Register("RegionIndex", typeof(int), typeof(MarkedRegion), new PropertyMetadata());

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(MarkedRegion), new PropertyMetadata());

        public MarkedRegion()
        {
            InitializeComponent();
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public int RegionIndex
        {
            get => (int)GetValue(RegionIndexProperty);
            set => SetValue(RegionIndexProperty, value);
        }

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

    }
}
