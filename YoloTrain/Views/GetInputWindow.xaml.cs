using YoloTrain.Mvvm;

namespace YoloTrain.Views
{
    /// <summary>
    /// Interaction logic for GetInputWindow.xaml
    /// </summary>
    public partial class GetInputWindow : WindowView
    {
        public GetInputWindow(IGetInputViewModel viewModel)
            : base(viewModel)
        {
            InitializeComponent();
        }
    }
}
