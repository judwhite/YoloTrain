using System.Windows.Controls;

namespace YoloTrain.Mvvm
{
    public class UserControlView : UserControl
    {
        protected UserControlView(IViewModel viewModel)
        {
            DataContext = viewModel;
        }

        public UserControlView()
        {
            // Note: only here to support XAML. Do not throw NotImplementedException() here, XAML complains
        }
    }
}
