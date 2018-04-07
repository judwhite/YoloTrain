using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        string CurrentImage { get; }
        Bitmap CurrentBitmap { get; }
        int CurrentImagePosition { get; set; }
        ObservableCollection<string> Classes { get; }
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
            NextImageCommand = new DelegateCommand(NextImage);
            PreviousImageCommand = new DelegateCommand(PreviousImage);
            ChangeImageCommand = new DelegateCommand<int>(ChangeImage);

            ExitCommand = new DelegateCommand(() => Application.Current.MainWindow.Close());

            PropertyChanged += MainWindowViewModel_PropertyChanged;

            LoadProject();
        }

        private void MainWindowViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CurrentImage))
            {
                OnCurrentImageChanged();
            }

            if (e.PropertyName == nameof(ImagePaths))
            {
                OnImagePathsChanged();
            }

            if (e.PropertyName == nameof(CurrentImagePosition))
            {
                OnCurrentImagePositionChanged();
            }
        }

        public ICommand NewProjectCommand
        {
            get => Get<ICommand>(nameof(NewProjectCommand));
            private set => Set(nameof(NewProjectCommand), value);
        }

        public ICommand ChangeImageCommand
        {
            get => Get<ICommand>(nameof(ChangeImageCommand));
            private set => Set(nameof(ChangeImageCommand), value);
        }

        public ICommand PreviousImageCommand
        {
            get => Get<ICommand>(nameof(PreviousImageCommand));
            private set => Set(nameof(PreviousImageCommand), value);
        }

        public ICommand NextImageCommand
        {
            get => Get<ICommand>(nameof(NextImageCommand));
            private set => Set(nameof(NextImageCommand), value);
        }

        public ICommand ExitCommand
        {
            get => Get<ICommand>(nameof(ExitCommand));
            private set => Set(nameof(ExitCommand), value);
        }

        public int PreviewSelectedOffset
        {
            get => Get<int>(nameof(PreviewSelectedOffset));
            private set => Set(nameof(PreviewSelectedOffset), value);
        }

        public int PreviewStartOffset
        {
            get => Get<int>(nameof(PreviewStartOffset));
            private set => Set(nameof(PreviewStartOffset), value);
        }

        public int ImageCount
        {
            get => Get<int>(nameof(ImageCount));
            private set => Set(nameof(ImageCount), value);
        }

        public string[] PreviewImages
        {
            get => Get<string[]>(nameof(PreviewImages));
            private set => Set(nameof(PreviewImages), value);
        }

        public ObservableCollection<string> ImagePaths
        {
            get => Get<ObservableCollection<string>>(nameof(ImagePaths));
            private set => Set(nameof(ImagePaths), value);
        }

        public string CurrentImage
        {
            get => Get<string>(nameof(CurrentImage));
            private set => Set(nameof(CurrentImage), value);
        }

        public string CurrentImageRelativeFileName
        {
            get => Get<string>(nameof(CurrentImageRelativeFileName));
            private set => Set(nameof(CurrentImageRelativeFileName), value);
        }

        public int CurrentImagePosition
        {
            get => Get<int>(nameof(CurrentImagePosition));
            set => Set(nameof(CurrentImagePosition), value);
        }

        public ObservableCollection<string> Classes
        {
            get => Get<ObservableCollection<string>>(nameof(Classes));
            private set => Set(nameof(Classes), value);
        }

        public Bitmap CurrentBitmap
        {
            get => Get<Bitmap>(nameof(CurrentBitmap));
            private set => Set(nameof(CurrentBitmap), value);
        }

        private void LoadProject()
        {
            CurrentImagePosition = 0;

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

            string objDataFileName = Path.Combine(basePath, _yoloProject.ObjectDataFilePath);
            var objDataLines = File.ReadAllLines(objDataFileName);
            foreach (var objDataLine in objDataLines)
            {
                var parts = objDataLine.Split('=');
                if (parts.Length != 2)
                    continue;

                if (parts[0].Trim() == "names")
                {
                    var namesFileName = Path.Combine(basePath, parts[1].Trim());
                    if (File.Exists(namesFileName))
                    {
                        var classes = File.ReadAllLines(namesFileName).Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
                        Classes = new ObservableCollection<string>(classes);
                    }
                }
            }

            string imagesDirectory = Path.Combine(basePath, _yoloProject.ImagesDirectory.Replace("/", @"\"));
            if (!Directory.Exists(imagesDirectory))
            {
                Directory.CreateDirectory(imagesDirectory);
                return;
            }

            string[] imageFiles = Directory.GetFiles(imagesDirectory, "*.jpg", SearchOption.AllDirectories);
            ImagePaths = new ObservableCollection<string>(imageFiles);
            if (imageFiles.Length > 0)
            {
                CurrentImagePosition = 1;
            }
        }

        private void ChangeImage(int n)
        {
            int newPosition = PreviewStartOffset + n + 1;
            if (newPosition < 1 || newPosition > ImagePaths.Count)
                return;
            CurrentImagePosition = newPosition;
        }

        private void NextImage()
        {
            if (ImagePaths == null || CurrentImagePosition >= ImagePaths.Count)
                return;

            CurrentImagePosition++;
        }

        private void PreviousImage()
        {
            if (ImagePaths == null || CurrentImagePosition <= 1)
                return;

            CurrentImagePosition--;
        }

        private void OnCurrentImagePositionChanged()
        {
            if (CurrentImagePosition <= 0)
            {
                PreviewImages = new string[0];
                CurrentImage = null;
                return;
            }

            CurrentImage = ImagePaths[CurrentImagePosition - 1];
            var previewList = new List<string>();
            int start = Math.Max(1, CurrentImagePosition - 2);

            // TODO (judwhite): determine how many preview images are visible on screen
            if (start + 8 > ImagePaths.Count)
            {
                start = Math.Max(1, ImagePaths.Count - 8);
            }
            for (int i = start; i < start + 10 && i <= ImagePaths.Count; i++)
            {
                previewList.Add(ImagePaths[i - 1]);
                if (i == CurrentImagePosition)
                {
                    PreviewSelectedOffset = i - start;
                }
            }
            PreviewImages = previewList.ToArray();
            PreviewStartOffset = start - 1;
        }

        private void OnImagePathsChanged()
        {
            if (ImagePaths == null)
            {
                ImageCount = 0;
                return;
            }

            ImageCount = ImagePaths.Count;
        }

        private void OnCurrentImageChanged()
        {
            if (string.IsNullOrWhiteSpace(CurrentImage))
            {
                CurrentBitmap = null;
                CurrentImageRelativeFileName = null;
                return;
            }

            CurrentBitmap = new Bitmap(Image.FromFile(CurrentImage));

            if (!string.IsNullOrWhiteSpace(_yoloProject.DarknetExecutableFilePath))
            {
                var lowerDarknetExecutablePath = Path.GetDirectoryName(_yoloProject.DarknetExecutableFilePath).ToLowerInvariant();
                if (CurrentImage.ToLowerInvariant().StartsWith(lowerDarknetExecutablePath))
                    CurrentImageRelativeFileName = CurrentImage.Substring(lowerDarknetExecutablePath.Length + 1);
                else
                    CurrentImageRelativeFileName = CurrentImage;
            }
            else
            {
                CurrentImageRelativeFileName = CurrentImage;
            }
        }
    }
}
