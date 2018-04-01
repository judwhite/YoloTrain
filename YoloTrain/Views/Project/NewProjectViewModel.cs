using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using YoloTrain.Mvvm;

namespace YoloTrain.Views.Project
{
    public interface INewProjectViewModel : IViewModel
    {
    }

    public class NewProjectViewModel : ViewModel, INewProjectViewModel
    {
        public NewProjectViewModel()
        {
            PropertyChanged += NewProjectViewModel_PropertyChanged;
            CancelCommand = new DelegateCommand(() =>
            {
                var result = MessageBox("Cancel changes?", "New profile", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result != MessageBoxResult.Yes)
                    return;
                CloseWindow(false);
            });

            SetDefaults();
        }

        private void NewProjectViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Classes))
            {
                Filters = (Classes + 5) * 5;
            }
        }

        private void SetDefaults()
        {
            BatchSize = 64;
            Subdivisions = 8;
            Height = 416;
            Width = 416;
            Anchors = "1.08,1.19,  3.42,4.41,  6.63,11.38,  9.42,5.11,  16.62,10.52";
            Classes = 0;
            IsRandomChecked = false;

            TrainFileName = "data/train.txt";
            ValidFileName = "data/valid.txt";
            ClassNamesFileName = "data/obj.names";
            BackupFolder = "backup/";
        }

        public ICommand NewProjectFileCommand { get; }
        public ICommand LoadProjectFileCommand { get; }
        public ICommand FindDarknetExecutableCommand { get; }
        public ICommand NewYoloConfigCommand { get; }
        public ICommand LoadYoloConfigCommand { get; }
        public ICommand DetectAnchorsCommand { get; }
        public ICommand NewObjDataCommand { get; }
        public ICommand LoadObjDataCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public int BatchSize
        {
            get => Get<int>(nameof(BatchSize));
            set => Set(nameof(BatchSize), value);
        }

        public int Subdivisions
        {
            get => Get<int>(nameof(Subdivisions));
            set => Set(nameof(Subdivisions), value);
        }

        public int Height
        {
            get => Get<int>(nameof(Height));
            set => Set(nameof(Height), value);
        }

        public int Width
        {
            get => Get<int>(nameof(Width));
            set => Set(nameof(Width), value);
        }

        public int Filters
        {
            get => Get<int>(nameof(Filters));
            set => Set(nameof(Filters), value);
        }

        public string Anchors
        {
            get => Get<string>(nameof(Anchors));
            set => Set(nameof(Anchors), value);
        }

        public int Classes
        {
            get => Get<int>(nameof(Classes));
            set => Set(nameof(Classes), value);
        }

        public bool IsRandomChecked
        {
            get => Get<bool>(nameof(IsRandomChecked));
            set => Set(nameof(IsRandomChecked), value);
        }

        public string TrainFileName
        {
            get => Get<string>(nameof(TrainFileName));
            set => Set(nameof(TrainFileName), value);
        }

        public string ValidFileName
        {
            get => Get<string>(nameof(ValidFileName));
            set => Set(nameof(ValidFileName), value);
        }

        public string ClassNamesFileName
        {
            get => Get<string>(nameof(ClassNamesFileName));
            set => Set(nameof(ClassNamesFileName), value);
        }

        public string BackupFolder
        {
            get => Get<string>(nameof(BackupFolder));
            set => Set(nameof(BackupFolder), value);
        }
    }
}
