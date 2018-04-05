using System.Windows;
using YoloTrain.Mvvm;

namespace YoloTrain.Views.Controls
{
    /// <summary>
    /// Interaction logic for ImagePreviewButton.xaml
    /// </summary>
    public partial class ImagePreviewButton : UserControlView
    {
        public static readonly DependencyProperty PreviewOffsetIndexProperty =
            DependencyProperty.Register("PreviewOffsetIndex", typeof(int), typeof(ImagePreviewButton), new PropertyMetadata());

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(ImagePreviewButton), new PropertyMetadata());

        public ImagePreviewButton()
        {
            InitializeComponent();
        }

        public int PreviewOffsetIndex
        {
            get => (int)GetValue(PreviewOffsetIndexProperty);
            set => SetValue(PreviewOffsetIndexProperty, value);
        }

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }
    }
}
