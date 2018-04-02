using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using YoloTrain.Mvvm;

namespace YoloTrain.Views.Project
{
    public interface INewProjectViewModel : IViewModel
    {
        ICommand SelectProjectDirectoryCommand { get; }
        ICommand FindDarknetExecutableCommand { get; }

        ICommand LoadYoloConfigCommand { get; }
        ICommand DetectAnchorsCommand { get; }
        ICommand LoadObjDataCommand { get; }

        ICommand SaveCommand { get; }
        ICommand CancelCommand { get; }

        bool IsSaveEnabled { get; }

        string ProjectFileName { get; set; }
        string ProjectDirectory { get; set; }

        string DarknetExecutable { get; set; }

        string YoloConfigFile { get; set; }
        int BatchSize { get; set; }
        int Subdivisions { get; set; }
        int HeightWidth { get; set; }
        int Filters { get; set; }
        string Anchors { get; set; }
        int Classes { get; set; }
        bool IsRandomChecked { get; set; }
        bool IsDetectEnabled { get; }

        string ObjDataFile { get; set; }
        string TrainFileName { get; set; }
        string ValidFileName { get; set; }
        string ClassNamesFileName { get; set; }
        string BackupFolder { get; set; }
    }

    public class NewProjectViewModel : ViewModel, INewProjectViewModel
    {
        public NewProjectViewModel()
        {
            PropertyChanged += NewProjectViewModel_PropertyChanged;
            CancelCommand = new DelegateCommand(() => CloseWindow(false));
            SelectProjectDirectoryCommand = new DelegateCommand(() =>
            {
                var result = ShowSelectFolderDialog(out string selectedDirectory);
                if (result == true)
                {
                    if (Directory.Exists(selectedDirectory))
                    {
                        ProjectDirectory = selectedDirectory;
                    }
                }
            });
            FindDarknetExecutableCommand = new DelegateCommand(() =>
            {
                const string filter = "darknet.exe|darknet.exe|Executables (*.exe)|*.exe|All files (*.*)|*.*";
                var result = ShowOpenFileDialog("Find darknet.exe", filter, out string fileName);
                if (result == true)
                {
                    if (File.Exists(fileName))
                    {
                        DarknetExecutable = fileName;
                    }
                }
            });

            SetDefaults();
        }

        private void NewProjectViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Project
            if (e.PropertyName == nameof(ProjectFileName))
            {
                if (!string.IsNullOrWhiteSpace(ProjectFileName))
                {
                    if (!ProjectFileName.EndsWith(".ytcfg"))
                    {
                        ProjectFileName += ".ytcfg";
                    }
                }
            }

            if (e.PropertyName == nameof(Classes))
            {
                if (Classes < 0)
                {
                    return;
                }
                Filters = (Classes + 5) * 5;
            }

            if (e.PropertyName == nameof(HeightWidth))
            {
                HeightWidth = FindClosestInt(HeightWidth, 32);
            }
        }

        private static int FindClosestInt(int orig, int mod)
        {
            if (orig <= mod)
                return mod;

            var n = orig % mod;
            if (n == 0)
                return orig;

            if (mod / 2 > n)
                return orig - n;

            return orig + mod - n;
        }

        private void SetDefaults()
        {
            ProjectDirectory = Environment.CurrentDirectory;

            BatchSize = 64;
            Subdivisions = 16;
            HeightWidth = 416;
            Anchors = "10,13,  16,30,  33,23,  30,61,  62,45,  59,119,  116,90,  156,198,  373,326";
            Classes = 0;
            IsRandomChecked = false;

            TrainFileName = "data/train.txt";
            ValidFileName = "data/valid.txt";
            ClassNamesFileName = "data/obj.names";
            BackupFolder = "backup/";
        }

        public ICommand SelectProjectDirectoryCommand { get; }
        public ICommand FindDarknetExecutableCommand { get; }
        public ICommand LoadYoloConfigCommand { get; }
        public ICommand DetectAnchorsCommand { get; }
        public ICommand LoadObjDataCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public bool IsSaveEnabled
        {
            get => Get<bool>(nameof(IsSaveEnabled));
            private set => Set(nameof(IsSaveEnabled), value);
        }

        public string ProjectFileName
        {
            get => Get<string>(nameof(ProjectFileName));
            set => Set(nameof(ProjectFileName), value);
        }

        public string ProjectDirectory
        {
            get => Get<string>(nameof(ProjectDirectory));
            set => Set(nameof(ProjectDirectory), value);
        }

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

        public int HeightWidth
        {
            get => Get<int>(nameof(HeightWidth));
            set => Set(nameof(HeightWidth), value);
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

        public string DarknetExecutable
        {
            get => Get<string>(nameof(DarknetExecutable));
            set => Set(nameof(DarknetExecutable), value);
        }

        public string YoloConfigFile
        {
            get => Get<string>(nameof(YoloConfigFile));
            set => Set(nameof(YoloConfigFile), value);
        }

        public bool IsDetectEnabled
        {
            get => Get<bool>(nameof(IsDetectEnabled));
            private set => Set(nameof(IsDetectEnabled), value);
        }

        public string ObjDataFile
        {
            get => Get<string>(nameof(ObjDataFile));
            set => Set(nameof(ObjDataFile), value);
        }
    }
}
