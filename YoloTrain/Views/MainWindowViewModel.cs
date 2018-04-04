using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Newtonsoft.Json;
using YoloTrain.Config;
using YoloTrain.Mvvm;
using YoloTrain.Views.Project;

namespace YoloTrain.Views
{
    public interface IMainWindowViewModel : IViewModel
    {
        ICommand ExitCommand { get; }
        string CurrentImage { get; set; }
        Bitmap CurrentBitmap { get; set; }
    }

    public class MainWindowViewModel : ViewModel, IMainWindowViewModel
    {
        private YoloProject _yoloProject;

        public MainWindowViewModel()
        {
            NewProjectCommand = new DelegateCommand(() =>
            {
                var result = ShowWindow<NewProjectWindow>();
                if (result == true)
                    LoadProject();
            });
            ExitCommand = new DelegateCommand(() => Application.Current.MainWindow.Close());

            PropertyChanged += MainWindowViewModel_PropertyChanged;

            LoadProject();
        }

        private void MainWindowViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CurrentImage))
            {
                if (string.IsNullOrWhiteSpace(CurrentImage))
                {
                    CurrentBitmap = null;
                    return;
                }
                CurrentBitmap = new Bitmap(Image.FromFile(CurrentImage));
            }
        }

        private void LoadProject()
        {
            const string yoloTrainConfigFileName = "yolotrain.cfg";
            if (!File.Exists(yoloTrainConfigFileName))
                return;

            var yoloConfigJson = File.ReadAllText(yoloTrainConfigFileName);
            var yoloConfig = JsonConvert.DeserializeObject<YoloTrainSettings>(yoloConfigJson);

            var projectFileName = yoloConfig.RecentProjects?.FirstOrDefault();
            if (projectFileName == null || !File.Exists(projectFileName))
                return;

            var projectJson = File.ReadAllText(projectFileName);
            _yoloProject = JsonConvert.DeserializeObject<YoloProject>(projectJson);

            string basePath = Path.GetDirectoryName(_yoloProject.DarknetExecutableFilePath);
            string imagesDirectory = Path.Combine(basePath, _yoloProject.ImagesDirectory.Replace("/", @"\"));
            if (!Directory.Exists(imagesDirectory))
            {
                Directory.CreateDirectory(imagesDirectory);
                return;
            }

            string[] imageFiles = Directory.GetFiles(imagesDirectory, "*.jpg", SearchOption.AllDirectories);
            if (imageFiles.Length > 0)
            {

                CurrentImage = imageFiles[0];
            }
        }

        public ICommand NewProjectCommand
        {
            get => Get<ICommand>(nameof(NewProjectCommand));
            private set => Set(nameof(NewProjectCommand), value);
        }

        public ICommand ExitCommand
        {
            get => Get<ICommand>(nameof(ExitCommand));
            private set => Set(nameof(ExitCommand), value);
        }

        public string CurrentImage
        {
            get => Get<string>(nameof(CurrentImage));
            set => Set(nameof(CurrentImage), value);
        }

        public Bitmap CurrentBitmap
        {
            get => Get<Bitmap>(nameof(CurrentBitmap));
            set => Set(nameof(CurrentBitmap), value);
        }
    }
}
