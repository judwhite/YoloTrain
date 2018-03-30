using YoloTrain.Mvvm;

namespace YoloTrain.Views
{
    /// <summary>
    /// Interaction logic for MainMenu.xaml
    /// </summary>
    public partial class MainMenu : UserControlView
    {
        public MainMenu()
            : this(IoC.Resolve<IMainWindowViewModel>())
        {
        }

        public MainMenu(IMainWindowViewModel viewModel)
            : base(viewModel)
        {
            InitializeComponent();
        }
    }
}
