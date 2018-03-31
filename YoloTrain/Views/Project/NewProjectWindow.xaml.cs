using YoloTrain.Mvvm;

namespace YoloTrain.Views.Project
{
    /// <summary>
    /// Interaction logic for NewProjectWindow.xaml
    /// </summary>
    public partial class NewProjectWindow : WindowView
    {
        public NewProjectWindow(INewProjectViewModel viewModel)
            : base(viewModel)
        {
            InitializeComponent();
        }
    }
}
