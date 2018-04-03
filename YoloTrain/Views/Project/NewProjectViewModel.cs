using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using Newtonsoft.Json;
using YoloTrain.Config;
using YoloTrain.Mvvm;

namespace YoloTrain.Views.Project
{
    public interface INewProjectViewModel : IViewModel
    {
        ICommand SelectProjectDirectoryCommand { get; }
        ICommand FindDarknetExecutableCommand { get; }

        ICommand LoadYoloConfigCommand { get; }
        ICommand LoadObjDataCommand { get; }

        ICommand SaveCommand { get; }
        ICommand CancelCommand { get; }

        bool IsSaveEnabled { get; }

        string ProjectFileName { get; set; }
        string ProjectDirectory { get; set; }

        string DarknetExecutable { get; set; }
        string ImagesDirectory { get; set; }

        string YoloConfigFile { get; set; }
        int? BatchSize { get; set; }
        int? Subdivisions { get; set; }
        int? HeightWidth { get; set; }
        bool IsRandomChecked { get; set; }
        int? MaxObjects { get; set; }

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

            SaveCommand = new DelegateCommand(Save);
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

        private void Save()
        {
            // TODO (judwhite): finish
            // TODO (judwhite): add validation
            MouseHelper.SetWaitCursor();
            try
            {
                const int classes = 0;
                const int filters = (classes + 5) * 3;

                string yoloTemplate = File.ReadAllText(Path.Combine("darknet", "yolov3.cfg.template"));
                string yoloCfg = yoloTemplate
                    .Replace("%batch%", BatchSize.ToString())
                    .Replace("%subdivisions%", Subdivisions.ToString())
                    .Replace("%width%", HeightWidth.ToString())
                    .Replace("%height%", HeightWidth.ToString())
                    .Replace("%filters%", filters.ToString())
                    .Replace("%classes%", classes.ToString())
                    .Replace("%anchors%", "10,13,  16,30,  33,23,  30,61,  62,45,  59,119,  116,90,  156,198,  373,326")
                    .Replace("%random%", IsRandomChecked ? "1" : "0")
                    .Replace("%max%", MaxObjects.ToString());

                Directory.CreateDirectory(Path.GetDirectoryName(YoloConfigFile));
                File.WriteAllText(YoloConfigFile, yoloCfg);

                string objDataTemplate = File.ReadAllText(Path.Combine("darknet", "obj.data.template"));
                string objData = objDataTemplate
                    .Replace("%classes%", classes.ToString())
                    .Replace("%train%", TrainFileName)
                    .Replace("%valid%", ValidFileName)
                    .Replace("%names%", ClassNamesFileName)
                    .Replace("%backup%", BackupFolder);

                Directory.CreateDirectory(Path.GetDirectoryName(ObjDataFile));
                File.WriteAllText(ObjDataFile, objData);

                var project = new YoloProject
                {
                    Version = 1,
                    YoloVersion = "3",
                    DarknetExecutableFilePath = DarknetExecutable,
                    YoloConfigFilePath = YoloConfigFile,
                    ObjectDataFilePath = ObjDataFile,
                    ImagesDirectory = ImagesDirectory
                };

                string projectFullPath = Path.Combine(ProjectDirectory, ProjectFileName);
                string projectJson = JsonConvert.SerializeObject(project);
                File.WriteAllText(projectFullPath, projectJson);

                var yoloConfig = new YoloTrainSettings
                {
                    Version = 1,
                    // TODO (judwhite): maintain history
                    RecentProjects = new List<string> { projectFullPath }
                };

                var yoloConfigJson = JsonConvert.SerializeObject(yoloConfig);
                File.WriteAllText("yolotrain.cfg", yoloConfigJson);

                CloseWindow(true);
            }
            finally
            {
                MouseHelper.ResetCursor();
            }
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

            if (e.PropertyName == nameof(HeightWidth) && HeightWidth != null)
            {
                HeightWidth = FindClosestInt(HeightWidth.Value, 32);
            }

            if ((e.PropertyName == nameof(ProjectFileName) || e.PropertyName == nameof(DarknetExecutable)) &&
                !string.IsNullOrWhiteSpace(ProjectFileName) &&
                !string.IsNullOrWhiteSpace(DarknetExecutable))
            {
                var darknetDirectory = Path.GetDirectoryName(DarknetExecutable);
                var projectName = Path.GetFileNameWithoutExtension(ProjectFileName);
                if (!projectName.EndsWith("-yolov3"))
                    projectName += "-yolov3";

                if (string.IsNullOrWhiteSpace(YoloConfigFile))
                {
                    YoloConfigFile = Path.Combine(darknetDirectory, projectName + ".cfg");
                }

                if (string.IsNullOrWhiteSpace(ObjDataFile))
                {
                    ObjDataFile = Path.Combine(darknetDirectory, projectName + "-data", "obj.data");
                }
            }

            if (e.PropertyName == nameof(YoloConfigFile) && !string.IsNullOrWhiteSpace(YoloConfigFile))
            {
                // TODO (judwhite): load if exists

                BatchSize = 16;
                Subdivisions = 8;
                HeightWidth = 608;
                IsRandomChecked = true;
                MaxObjects = 150;
            }

            if (e.PropertyName == nameof(ObjDataFile) && !string.IsNullOrWhiteSpace(ObjDataFile))
            {
                // TODO (judwhite): load if exists

                var dataDirectory = Path.GetFileName(Path.GetDirectoryName(ObjDataFile));
                var backupDirectory = (dataDirectory + "-backup").Replace("-data-backup", "-backup");

                TrainFileName = dataDirectory + "/train.txt";
                ValidFileName = dataDirectory + "/valid.txt";
                ClassNamesFileName = dataDirectory + "/obj.names";
                BackupFolder = backupDirectory + "/";

                if (string.IsNullOrWhiteSpace(ImagesDirectory))
                {
                    ImagesDirectory = dataDirectory + "/img";
                }
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
        }

        public ICommand SelectProjectDirectoryCommand { get; }
        public ICommand FindDarknetExecutableCommand { get; }
        public ICommand LoadYoloConfigCommand { get; }
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

        public int? BatchSize
        {
            get => Get<int?>(nameof(BatchSize));
            set => Set(nameof(BatchSize), value);
        }

        public int? Subdivisions
        {
            get => Get<int?>(nameof(Subdivisions));
            set => Set(nameof(Subdivisions), value);
        }

        public int? HeightWidth
        {
            get => Get<int?>(nameof(HeightWidth));
            set => Set(nameof(HeightWidth), value);
        }

        public int? MaxObjects
        {
            get => Get<int?>(nameof(MaxObjects));
            set => Set(nameof(MaxObjects), value);
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

        public string ImagesDirectory
        {
            get => Get<string>(nameof(ImagesDirectory));
            set => Set(nameof(ImagesDirectory), value);
        }

        public string YoloConfigFile
        {
            get => Get<string>(nameof(YoloConfigFile));
            set => Set(nameof(YoloConfigFile), value);
        }

        public string ObjDataFile
        {
            get => Get<string>(nameof(ObjDataFile));
            set => Set(nameof(ObjDataFile), value);
        }
    }
}
