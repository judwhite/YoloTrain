using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using YoloTrain.Config;
using YoloTrain.Models;
using YoloTrain.Mvvm;
using YoloTrain.Mvvm.ApplicationServices;
using YoloTrain.Utils;
using YoloTrain.Views.Project;

namespace YoloTrain.Views
{
    public interface IMainWindowViewModel : IViewModel
    {
        void SaveProject();

        ICommand ExitCommand { get; }
        ICommand SelectRegionCommand { get; }
        string CurrentImage { get; }
        Bitmap CurrentBitmap { get; }
        int CurrentImagePosition { get; set; }
        ObservableCollection<string> Classes { get; }
        ObservableCollection<string> SortedClasses { get; }
        ObservableCollection<YoloCoords> ImageRegions { get; }
        ObservableCollection<string> ImagePaths { get; }
        int? SelectedRegionIndex { get; }

        void SelectRegion(int? n);

        ICommand MoveLeftCommand { get; }
        ICommand MoveRightCommand { get; }
        ICommand MoveUpCommand { get; }
        ICommand MoveDownCommand { get; }
        ICommand GrowVerticalCommand { get; }
        ICommand ShrinkVerticalCommand { get; }
        ICommand GrowHorizontalCommand { get; }
        ICommand ShrinkHorizontalCommand { get; }
        ICommand DeleteRegionCommand { get; }

        ICommand DuplicatePreviousRegionsCommand { get; }
        ICommand PropagateRegionCommand { get; }

        ICommand AddClassCommand { get; }

        ICommand MoveAllUpCommand { get; }
        ICommand MoveAllDownCommand { get; }
        ICommand MoveAllLeftCommand { get; }
        ICommand MoveAllRightCommand { get; }
        ICommand ExpandAllCommand { get; }
        ICommand ShrinkAllCommand { get; }
        ICommand ClearAllRegionsCommand { get; }

        ICommand UpdateConfigurationFilesCommand { get; }
        ICommand ValidateBoundingBoxesCommand { get; }
    }

    public class MainWindowViewModel : ViewModel, IMainWindowViewModel
    {
        private string _yoloProjectFileName;
        private YoloProject _yoloProject;
        private string _yoloClassNamesFileName;

        public MainWindowViewModel()
        {
            NewProjectCommand = new DelegateCommand(NewProject);
            NextImageCommand = new DelegateCommand(() => NextImage());
            PreviousImageCommand = new DelegateCommand(PreviousImage);
            ChangeImageCommand = new DelegateCommand<int>(ChangeImage);
            SelectRegionCommand = new DelegateCommand<int?>(SelectRegion);
            DeleteRegionCommand = new DelegateCommand(DeleteRegion);

            MoveLeftCommand = new DelegateCommand(() => ChangeImageBounds(2, 0, 0, 0));
            MoveRightCommand = new DelegateCommand(() => ChangeImageBounds(-2, 0, 0, 0));
            MoveUpCommand = new DelegateCommand(() => ChangeImageBounds(0, 2, 0, 0));
            MoveDownCommand = new DelegateCommand(() => ChangeImageBounds(0, -2, 0, 0));

            GrowVerticalCommand = new DelegateCommand(() => ChangeImageBounds(0, 0, 0, 2));
            ShrinkVerticalCommand = new DelegateCommand(() => ChangeImageBounds(0, 0, 0, -2));
            GrowHorizontalCommand = new DelegateCommand(() => ChangeImageBounds(0, 0, 2, 0));
            ShrinkHorizontalCommand = new DelegateCommand(() => ChangeImageBounds(0, 0, -2, 0));

            DuplicatePreviousRegionsCommand = new DelegateCommand(DuplicatePreviousRegions);
            PropagateRegionCommand = new DelegateCommand(PropagateRegion);

            AddClassCommand = new DelegateCommand(AddClass);

            MoveAllUpCommand = new DelegateCommand(() => MoveAll(0, -2));
            MoveAllDownCommand = new DelegateCommand(() => MoveAll(0, 2));
            MoveAllLeftCommand = new DelegateCommand(() => MoveAll(-2, 0));
            MoveAllRightCommand = new DelegateCommand(() => MoveAll(2, 0));

            ExpandAllCommand = new DelegateCommand(() => DilateAll(1.005));
            ShrinkAllCommand = new DelegateCommand(() => DilateAll(0.995));

            ClearAllRegionsCommand = new DelegateCommand(ClearAllRegions);
            RefreshSelectedImageClassImagesCommand = new DelegateCommand(RefreshSelectedImageClassImages);

            BlackoutRegionCommand = new DelegateCommand(BlackoutRegion);

            UpdateConfigurationFilesCommand = new DelegateCommand(() => UpdateConfigurationFiles(true));
            ValidateBoundingBoxesCommand = new DelegateCommand(ValidateBoundingBoxes);
            ExitCommand = new DelegateCommand(() => Application.Current.MainWindow.Close());

            PropertyChanged += MainWindowViewModel_PropertyChanged;

            LoadProject();
        }

        private void MainWindowViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CurrentImage))
                OnCurrentImageChanged();
            if (e.PropertyName == nameof(ImagePaths))
                OnImagePathsChanged();
            if (e.PropertyName == nameof(CurrentImagePosition))
                OnCurrentImagePositionChanged();
            if (e.PropertyName == nameof(Classes))
                OnClassesChanged();
            if (e.PropertyName == nameof(SelectedRegionIndex))
                OnSelectedRegionIndexChanged();
            if (e.PropertyName == nameof(SelectedRegionClass))
                OnSelectedRegionClassChanged();
            if (e.PropertyName == nameof(MassSelectedClass))
                OnMassSelectedClassChanged();
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

        public ICommand SelectRegionCommand
        {
            get => Get<ICommand>(nameof(SelectRegionCommand));
            private set => Set(nameof(SelectRegionCommand), value);
        }

        public ICommand ExitCommand
        {
            get => Get<ICommand>(nameof(ExitCommand));
            private set => Set(nameof(ExitCommand), value);
        }

        public ICommand MoveLeftCommand
        {
            get => Get<ICommand>(nameof(MoveLeftCommand));
            private set => Set(nameof(MoveLeftCommand), value);
        }

        public ICommand MoveRightCommand
        {
            get => Get<ICommand>(nameof(MoveRightCommand));
            private set => Set(nameof(MoveRightCommand), value);
        }

        public ICommand MoveUpCommand
        {
            get => Get<ICommand>(nameof(MoveUpCommand));
            private set => Set(nameof(MoveUpCommand), value);
        }

        public ICommand MoveDownCommand
        {
            get => Get<ICommand>(nameof(MoveDownCommand));
            private set => Set(nameof(MoveDownCommand), value);
        }

        public ICommand GrowVerticalCommand
        {
            get => Get<ICommand>(nameof(GrowVerticalCommand));
            private set => Set(nameof(GrowVerticalCommand), value);
        }

        public ICommand ShrinkVerticalCommand
        {
            get => Get<ICommand>(nameof(ShrinkVerticalCommand));
            private set => Set(nameof(ShrinkVerticalCommand), value);
        }

        public ICommand GrowHorizontalCommand
        {
            get => Get<ICommand>(nameof(GrowHorizontalCommand));
            private set => Set(nameof(GrowHorizontalCommand), value);
        }

        public ICommand ShrinkHorizontalCommand
        {
            get => Get<ICommand>(nameof(ShrinkHorizontalCommand));
            private set => Set(nameof(ShrinkHorizontalCommand), value);
        }

        public ICommand ClearRegionsCommand
        {
            get => Get<ICommand>(nameof(ClearRegionsCommand));
            private set => Set(nameof(ClearRegionsCommand), value);
        }

        public ICommand DeleteRegionCommand
        {
            get => Get<ICommand>(nameof(DeleteRegionCommand));
            private set => Set(nameof(DeleteRegionCommand), value);
        }

        public ICommand DuplicatePreviousRegionsCommand
        {
            get => Get<ICommand>(nameof(DuplicatePreviousRegionsCommand));
            set => Set(nameof(DuplicatePreviousRegionsCommand), value);
        }

        public ICommand PropagateRegionCommand
        {
            get => Get<ICommand>(nameof(PropagateRegionCommand));
            set => Set(nameof(PropagateRegionCommand), value);
        }

        public ICommand AddClassCommand
        {
            get => Get<ICommand>(nameof(AddClassCommand));
            private set => Set(nameof(AddClassCommand), value);
        }

        public ICommand MoveAllUpCommand
        {
            get => Get<ICommand>(nameof(MoveAllUpCommand));
            set => Set(nameof(MoveAllUpCommand), value);
        }

        public ICommand MoveAllDownCommand
        {
            get => Get<ICommand>(nameof(MoveAllDownCommand));
            set => Set(nameof(MoveAllDownCommand), value);
        }

        public ICommand MoveAllLeftCommand
        {
            get => Get<ICommand>(nameof(MoveAllLeftCommand));
            set => Set(nameof(MoveAllLeftCommand), value);
        }

        public ICommand MoveAllRightCommand
        {
            get => Get<ICommand>(nameof(MoveAllRightCommand));
            set => Set(nameof(MoveAllRightCommand), value);
        }

        public ICommand ExpandAllCommand
        {
            get => Get<ICommand>(nameof(ExpandAllCommand));
            set => Set(nameof(ExpandAllCommand), value);
        }

        public ICommand ShrinkAllCommand
        {
            get => Get<ICommand>(nameof(ShrinkAllCommand));
            set => Set(nameof(ShrinkAllCommand), value);
        }

        public ICommand ClearAllRegionsCommand
        {
            get => Get<ICommand>(nameof(ClearAllRegionsCommand));
            set => Set(nameof(ClearAllRegionsCommand), value);
        }

        public ICommand UpdateConfigurationFilesCommand
        {
            get => Get<ICommand>(nameof(UpdateConfigurationFilesCommand));
            set => Set(nameof(UpdateConfigurationFilesCommand), value);
        }

        public ICommand ValidateBoundingBoxesCommand
        {
            get => Get<ICommand>(nameof(ValidateBoundingBoxesCommand));
            set => Set(nameof(ValidateBoundingBoxesCommand), value);
        }

        public ICommand RefreshSelectedImageClassImagesCommand
        {
            get => Get<ICommand>(nameof(RefreshSelectedImageClassImagesCommand));
            set => Set(nameof(RefreshSelectedImageClassImagesCommand), value);
        }

        public ICommand BlackoutRegionCommand
        {
            get => Get<ICommand>(nameof(BlackoutRegionCommand));
            set => Set(nameof(BlackoutRegionCommand), value);
        }

        private void BlackoutRegion()
        {
            var result = MessageBox("Blackout pixels and remove region?",
                                    "Blackout Region",
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            var fileName = CurrentImage;
            string backupFile = fileName + ".orig";
            if (!File.Exists(backupFile))
                File.Copy(CurrentImage, backupFile);

            var regionIndex = SelectedRegionIndex.Value;

            var region = ImageRegions[regionIndex];

            SelectedRegionIndex = null;
            var previewImages = PreviewImages;

            ImageRegions.RemoveAt(regionIndex);
            SaveImageRegions();

            Blackout(fileName, region);
        }

        private void Blackout(string fileName, YoloCoords region)
        {
            try
            {
                byte[] bytes;

                using (MemoryStream memory = new MemoryStream())
                using (var img = Image.FromFile(fileName))
                using (var g = Graphics.FromImage(img))
                {
                    double width = region.Width * img.Width;
                    double height = region.Height * img.Height;
                    double x = region.X * img.Width - width / 2.0;
                    double y = region.Y * img.Height - height / 2.0;

                    g.FillRectangle(System.Drawing.Brushes.Black, (float)x, (float)y, (float)width, (float)height);
                    g.DrawImage(img, new System.Drawing.Point(0, 0));

                    img.Save(memory, System.Drawing.Imaging.ImageFormat.Jpeg);
                    bytes = memory.ToArray();
                }

                File.WriteAllBytes(fileName, bytes);
            }
            catch (Exception ex)
            {
                MessageBox(ex.Message, "Blackout Region", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            finally
            {
                CurrentImage = null;
                CurrentBitmap = null;

                CurrentImage = fileName;
                UpdateImageRegions();
                UpdatePreviewImages();
            }
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

        public ImageSource[] PreviewImages
        {
            get => Get<ImageSource[]>(nameof(PreviewImages));
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

        public ObservableCollection<string> SortedClasses
        {
            get => Get<ObservableCollection<string>>(nameof(SortedClasses));
            private set => Set(nameof(SortedClasses), value);
        }

        public Bitmap CurrentBitmap
        {
            get => Get<Bitmap>(nameof(CurrentBitmap));
            private set
            {
                var bmp = CurrentBitmap;
                if (bmp != null)
                    bmp.Dispose();
                Set(nameof(CurrentBitmap), value);
            }
        }

        public ObservableCollection<YoloCoords> ImageRegions
        {
            get => Get<ObservableCollection<YoloCoords>>(nameof(ImageRegions));
            private set => Set(nameof(ImageRegions), value);
        }

        public void SaveProject()
        {
            if (_yoloProject == null)
                return;

            MouseHelper.SetWaitCursor();
            try
            {
                _yoloProject.LastImageFilePath = CurrentImage;
                var json = JsonConvert.SerializeObject(_yoloProject);
                File.WriteAllText(_yoloProjectFileName, json);
            }
            finally
            {
                MouseHelper.ResetCursor();
            }
        }

        private void LoadProject()
        {
            CurrentImagePosition = 0;
            _yoloProjectFileName = null;
            _yoloProject = null;
            _yoloClassNamesFileName = null;

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
            _yoloProjectFileName = projectFileName;

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
                    _yoloClassNamesFileName = Path.Combine(basePath, parts[1].Trim());
                    LoadClassNames();
                }
            }

            string imagesDirectory = Path.Combine(basePath, _yoloProject.ImagesDirectory);
            if (!Directory.Exists(imagesDirectory))
            {
                Directory.CreateDirectory(imagesDirectory);
                return;
            }

            var imageFiles = Directory.GetFiles(imagesDirectory, "*.jpg", SearchOption.AllDirectories)
                .Select(p => p.Replace('/', '\\'))
                .ToList();

            imageFiles.Sort(new NumericStringComparer());
            ImagePaths = new ObservableCollection<string>(imageFiles);
            if (imageFiles.Count > 0)
            {
                var imageIndex = imageFiles.IndexOf(_yoloProject.LastImageFilePath);
                if (imageIndex == -1)
                    CurrentImagePosition = 1;
                else
                    CurrentImagePosition = imageIndex + 1;
            }
        }

        private void LoadClassNames()
        {
            if (string.IsNullOrWhiteSpace(_yoloClassNamesFileName))
                return;

            if (!File.Exists(_yoloClassNamesFileName))
                return;

            var classes = File.ReadAllLines(_yoloClassNamesFileName)
                              .Where(p => !string.IsNullOrWhiteSpace(p))
                              .ToList();
            Classes = new ObservableCollection<string>(classes);
        }

        public class NumericStringComparer : IComparer<string>
        {
            // TODO (judwhite): this works but it's incredibly slow (2.5s for ~ 20k images). Fix perf.

            private static readonly char[] _splitCharacters = { ' ', '-', '_', '.', '\\', '/', '(', ')', '[', ']', '{', '}', '@' };

            public int Compare(string x, string y)
            {
                var x2 = GetModifiedString(x);
                var y2 = GetModifiedString(y);

                return string.CompareOrdinal(x2, y2);
            }

            private static string GetModifiedString(string v)
            {
                if (v == null)
                    return null;

                var sb = new StringBuilder();
                var parts = v.ToLowerInvariant().Split(_splitCharacters, StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in parts)
                {
                    var modPart = part;

                    // prefix
                    if (char.IsDigit(modPart[0]))
                    {
                        int i = 1;
                        for (; i < modPart.Length; i++)
                        {
                            if (!char.IsDigit(modPart[i]))
                                break;
                        }
                        var intPart = modPart.Substring(0, i);
                        if (intPart.Length < 9)
                        {
                            var intValue = int.Parse(intPart);
                            var prefix = $"{intValue:000000000}";
                            if (i == modPart.Length)
                            {
                                sb.Append(prefix);
                                sb.Append('_');
                                continue;
                            }
                            else
                            {
                                modPart = prefix + modPart.Substring(i);
                            }
                        }
                    }

                    // suffix
                    if (char.IsDigit(modPart[modPart.Length - 1]))
                    {
                        int i = modPart.Length - 1;
                        for (; i >= 0; i--)
                        {
                            if (!char.IsDigit(modPart[i]))
                                break;
                        }
                        var intPart = modPart.Substring(i + 1, modPart.Length - i - 1);
                        if (intPart.Length < 9)
                        {
                            var intValue = int.Parse(intPart);
                            var suffix = $"{intValue:000000000}";
                            modPart = modPart.Substring(0, i + 1) + suffix;
                        }
                    }

                    sb.Append(modPart);
                    sb.Append('_');
                }

                return sb.ToString();
            }
        }

        private void ClearAllRegions()
        {
            var result = MessageBox("Clear all regions in image?", "Clear regions", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
                return;

            SelectedRegionIndex = null;

            ImageRegions.Clear();
            SaveImageRegions();

            RaisePropertyChanged(nameof(ImageRegions), null, null);
        }

        public void SelectRegion(int? n)
        {
            SelectedRegionIndex = n;
        }

        public string SelectedRegionClass
        {
            get => Get<string>(nameof(SelectedRegionClass));
            set => Set(nameof(SelectedRegionClass), value);
        }

        public int? SelectedRegionIndex
        {
            get => Get<int?>(nameof(SelectedRegionIndex));
            private set => Set(nameof(SelectedRegionIndex), value);
        }

        public string MassSelectedClass
        {
            get => Get<string>(nameof(MassSelectedClass));
            set => Set(nameof(MassSelectedClass), value);
        }

        public ObservableCollection<FileRegionModel> MassClassImages
        {
            get => Get<ObservableCollection<FileRegionModel>>(nameof(MassClassImages));
            set => Set(nameof(MassClassImages), value);
        }

        public ObservableCollection<FileRegionModel> SelectedImageClassImages
        {
            get => Get<ObservableCollection<FileRegionModel>>(nameof(SelectedImageClassImages));
            set => Set(nameof(SelectedImageClassImages), value);
        }

        private void MoveAll(int x, int y)
        {
            TransformAll(x, y, 0);
        }

        private void DilateAll(double scale)
        {
            TransformAll(0, 0, scale);
        }

        private void TransformAll(int x, int y, double scale)
        {
            var img = CurrentBitmap;

            var dx = 1.0 / img.Width;
            var dy = 1.0 / img.Height;

            double midX = 0.5;
            double midY = 0.5;

            var selectedRegionIndex = SelectedRegionIndex;
            if (selectedRegionIndex != null)
            {
                midX = ImageRegions[selectedRegionIndex.Value].X;
                midY = ImageRegions[selectedRegionIndex.Value].Y;
            }

            if (scale != 0)
            {
                var dscale = scale - 1.0;

                foreach (var region in ImageRegions)
                {
                    double width = region.Width * img.Width;
                    double height = region.Height * img.Height;
                    double newWidth = width * scale;
                    double newHeight = height * scale;

                    // figure out smallest scale to increase all regions by at least 2px
                    // if image is going to go out of viewport don't include its scale (could be very small
                    // if it's getting smashed against a boundary)
                    var localScale = scale;
                    while (Math.Abs(newWidth - width) < 2 || Math.Abs(newHeight - height) < 2)
                    {
                        localScale += dscale;
                        newWidth = width * localScale;
                        newHeight = height * localScale;
                    }

                    var translateScale = (localScale - 1.0);

                    var newX = region.X + (region.X - midX) * translateScale;
                    var newY = region.Y + (region.Y - midY) * translateScale;

                    if (newX > 0 && newY > 0 && newX < 1 && newY < 1)
                    {
                        scale = localScale;
                    }
                }
            }

            for (int i = 0; i < ImageRegions.Count; i++)
            {
                var region = ImageRegions[i];

                if (scale == 0)
                {
                    // translate
                    region.X += dx * x;
                    region.Y += dy * y;
                }
                else
                {
                    // scale/dilate
                    double width = region.Width * img.Width;
                    double height = region.Height * img.Height;
                    double newWidth = width * scale;
                    double newHeight = height * scale;

                    // translate X/Y
                    var translateScale = (scale - 1.0);

                    region.X += (region.X - midX) * translateScale;
                    region.Y += (region.Y - midY) * translateScale;

                    region.Width = newWidth / img.Width;
                    region.Height = newHeight / img.Height;
                }

                if (region.X < 0)
                    region.X = 0;
                if (region.Y < 0)
                    region.Y = 0;
                if (region.X > 1)
                    region.X = 1;
                if (region.Y > 1)
                    region.Y = 1;

                while (region.Y + region.Height / 2.0 > 1 ||
                       region.Y - region.Height / 2.0 < 0)
                {
                    region.Height -= dy;
                }

                while (region.X + region.Width / 2.0 > 1 ||
                       region.X - region.Width / 2.0 < 0)
                {
                    region.Width -= dx;
                }

                if (region.Height < dy || region.Width < dx)
                {
                    ImageRegions.RemoveAt(i);
                    i--;
                    continue;
                }

                ImageRegions[i] = region;
            }

            SaveImageRegions();
            UpdateImageRegions();

            for (int i = 0; i < ImageRegions.Count; i++)
            {
                var region = ImageRegions[i];
                if (Math.Abs(region.X - midX) <= dx * 4 && Math.Abs(region.Y - midY) <= dy * 4)
                {
                    SelectedRegionIndex = i;
                }
            }
        }

        private void ChangeImage(int n)
        {
            int newPosition = PreviewStartOffset + n + 1;
            if (newPosition < 1 || newPosition > ImagePaths.Count)
                return;
            CurrentImagePosition = newPosition;
        }

        private void ChangeImageBounds(int x, int y, int w, int h)
        {
            var idx = SelectedRegionIndex;
            if (idx == null)
                return;

            var region = ImageRegions[idx.Value];
            var img = CurrentBitmap;

            var dx = 1.0 / img.Width;
            var dy = 1.0 / img.Height;

            region.X += dx * x + dx * w / 2.0;
            region.Y += dy * y + dy * h / 2.0;
            region.Width += dx * w;
            region.Height += dy * h;

            if (region.X < 0 ||
                region.X > 1 ||
                region.Y < 0 ||
                region.Y > 1)
            {
                return;
            }

            if (region.Y + region.Height / 2.0 > 1 ||
                region.Y - region.Height / 2.0 < 0 ||
                region.X + region.Width / 2.0 > 1 ||
                region.X + region.Width / 2.0 < 0)
            {
                return;
            }

            ImageRegions[idx.Value] = region;

            RaisePropertyChanged(nameof(ImageRegions), null, null);
            SelectedRegionIndex = idx.Value;
            RaisePropertyChanged(nameof(SelectedRegionIndex), null, null);

            SaveImageRegions();
        }

        private void SaveImageRegions()
        {
            var txtFileName = GetImageBoundsFileName(CurrentImage);
            var sb = new StringBuilder();
            foreach (var r in ImageRegions)
            {
                if (r.Class == null)
                    continue;
                sb.AppendLine($"{r.Class.Value} {r.X:0.000000} {r.Y:0.000000} {r.Width:0.000000} {r.Height:0.000000}");
            }
            File.WriteAllText(txtFileName, sb.ToString());
        }

        private void NewProject()
        {
            var result = ShowWindow<NewProjectWindow>();
            if (result == true)
                LoadProject();
        }

        private bool NextImage()
        {
            if (ImagePaths == null || CurrentImagePosition >= ImagePaths.Count)
                return false;

            CurrentImagePosition++;
            return true;
        }

        private void PreviousImage()
        {
            if (ImagePaths == null || CurrentImagePosition <= 1)
                return;

            CurrentImagePosition--;
        }

        private void PropagateRegion()
        {
            if (ImagePaths == null || CurrentImagePosition < 1 || SelectedRegionIndex == null)
                return;

            var region = ImageRegions[SelectedRegionIndex.Value];

            var inputViewModel = new GetInputViewModel
            {
                WindowTitle = "Propagate region to next frames",
                LabelText = "Number of frames",
                InputValue = "1"
            };

            var result = ShowWindow<GetInputWindow>(inputViewModel);
            if (result != true)
                return;

            int n;
            if (!int.TryParse(inputViewModel.InputValue, out n) || n <= 0)
                return;

            var dispatcher = IoC.Resolve<IDispatcher>();
            var wait = new AutoResetEvent(false);
            for (int i = 0; i < n; i++)
            {
                dispatcher.BeginInvoke(() =>
                {
                    try
                    {
                        if (!NextImage())
                            return;

                        ImageRegions.Add(region);
                        SaveImageRegions();
                    }
                    finally
                    {
                        wait.Set();
                    }
                });
                wait.WaitOne();
            }

            UpdateImageRegions();
        }

        private void DuplicatePreviousRegions()
        {
            if (ImagePaths == null || CurrentImagePosition <= 1)
                return;

            string previousImagePath = ImagePaths[CurrentImagePosition - 2];
            string currentImagePath = CurrentImage;

            string previousBoundsPath = GetImageBoundsFileName(previousImagePath);
            if (!File.Exists(previousBoundsPath))
            {
                // TODO (judwhite): message user?
                return;
            }

            string currentBoundsPath = GetImageBoundsFileName(currentImagePath);
            File.Copy(previousBoundsPath, currentBoundsPath, overwrite: true);
            OnCurrentImageChanged();
        }

        private void AddClass()
        {
            var viewModel = new GetInputViewModel
            {
                WindowTitle = "Add Class",
                LabelText = "Class name"
            };

            var result = ShowWindow<GetInputWindow>(viewModel);
            if (result != true)
                return;

            string className = viewModel.InputValue.Trim();
            if (Classes.Contains(className))
            {
                SelectedRegionClass = className;
                MessageBox($"Class '{className}' already exists.", "Add Class", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!string.IsNullOrWhiteSpace(_yoloClassNamesFileName))
            {
                var list = File.ReadAllLines(_yoloClassNamesFileName).Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
                list.Add(className);
                File.WriteAllLines(_yoloClassNamesFileName, list);

                LoadClassNames();

                SelectedRegionClass = className;
            }
            else
            {
                // TODO (judwhite): what if it doesn't exist?
            }
        }

        private void OnCurrentImagePositionChanged()
        {
            if (CurrentImagePosition <= 0)
            {
                PreviewImages = new ImageSource[0];
                CurrentImage = null;
                return;
            }

            CurrentImage = ImagePaths[CurrentImagePosition - 1];
            UpdatePreviewImages();
        }

        private int _oldStart;
        private ImageSource[] _oldPreviews = new ImageSource[0];

        private void UpdatePreviewImages()
        {
            var previewList = new List<ImageSource>();
            int start = Math.Max(1, CurrentImagePosition - 2);

            // TODO (judwhite): determine how many preview images are visible on screen
            if (start + 8 > ImagePaths.Count)
            {
                start = Math.Max(1, ImagePaths.Count - 8);
            }
            for (int i = start; i < start + 10 && i <= ImagePaths.Count; i++)
            {
                if (i >= _oldStart && i < _oldStart + _oldPreviews.Length)
                {
                    previewList.Add(_oldPreviews[i - _oldStart]);
                    continue;
                }

                using (var img = Image.FromFile(ImagePaths[i - 1]))
                using (var bmp = new Bitmap(img))
                {
                    previewList.Add(BitmapCloner.Clone(bmp));
                    if (i == CurrentImagePosition)
                    {
                        PreviewSelectedOffset = i - start;
                    }
                }
            }
            PreviewImages = previewList.ToArray();
            PreviewStartOffset = start - 1;

            _oldStart = start;
            _oldPreviews = PreviewImages;
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

        public ImageSource CurrentImageSource
        {
            get => Get<ImageSource>(nameof(CurrentImageSource));
            set => Set(nameof(CurrentImageSource), value);
        }

        private void OnCurrentImageChanged()
        {
            UpdateImageRegions();
            SelectedImageClassImages = null;

            if (string.IsNullOrWhiteSpace(CurrentImage))
            {
                CurrentBitmap = null;
                CurrentImageRelativeFileName = null;
                return;
            }

            using (var img = Image.FromFile(CurrentImage))
            {
                CurrentBitmap = new Bitmap((Image)img.Clone());
            }

            CurrentImageSource = BitmapCloner.Clone(CurrentBitmap);

            if (!string.IsNullOrWhiteSpace(_yoloProject.DarknetExecutableFilePath))
            {
                var lowerDarknetExecutablePath = Path.GetDirectoryName(_yoloProject.DarknetExecutableFilePath).ToLowerInvariant();

                if (CurrentImage.ToLowerInvariant().StartsWith(lowerDarknetExecutablePath))
                    CurrentImageRelativeFileName = CurrentImage.Substring(lowerDarknetExecutablePath.Length + 1).Replace('/', '\\');
                else
                    CurrentImageRelativeFileName = CurrentImage;
            }
            else
            {
                CurrentImageRelativeFileName = CurrentImage;
            }
        }

        private static string GetImageBoundsFileName(string imageFileName)
        {
            string imageDirectory = Path.GetDirectoryName(imageFileName);
            return Path.Combine(imageDirectory, Path.GetFileNameWithoutExtension(imageFileName) + ".txt");
        }

        private void UpdateImageRegions()
        {
            SelectedRegionIndex = null;

            if (string.IsNullOrWhiteSpace(CurrentImage))
                return;

            string imageBoundsFileName = GetImageBoundsFileName(CurrentImage);

            var list = new ObservableCollection<YoloCoords>();
            if (File.Exists(imageBoundsFileName))
            {
                var lines = File.ReadAllLines(imageBoundsFileName);
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];

                    var parts = line.Split(' ');
                    if (parts.Length != 5)
                        continue;

                    var yolo = new YoloCoords
                    {
                        Class = int.Parse(parts[0]),
                        X = double.Parse(parts[1]),
                        Y = double.Parse(parts[2]),
                        Width = double.Parse(parts[3]),
                        Height = double.Parse(parts[4])
                    };

                    list.Add(yolo);
                }
            }
            ImageRegions = list;
        }

        private void DeleteRegion()
        {
            var idx = SelectedRegionIndex;

            if (idx == null)
                return;

            var region = ImageRegions[idx.Value];
            var result = MessageBox($"Delete '{Classes[region.Class.Value]}' region?", "Delete region", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
                return;

            ImageRegions.RemoveAt(idx.Value);

            RaisePropertyChanged(nameof(ImageRegions), null, null);
            SelectedRegionIndex = null;

            SaveImageRegions();
        }

        private void UpdateConfigurationFiles(bool showMessageBox)
        {
            MouseHelper.SetWaitCursor();
            try
            {
                ObjectDataConfig.SaveFromTemplate(_yoloProject);
                YoloTrainConfig.SaveFromTemplate(_yoloProject, null);
            }
            finally
            {
                MouseHelper.ResetCursor();
            }

            if (showMessageBox)
            {
                MessageBox("Configuration files updated successfully.",
                           "Update configuration files",
                           MessageBoxButton.OK,
                           MessageBoxImage.Information);
            }
        }

        private void ValidateBoundingBoxes()
        {
            // validate train/validate files
            MouseHelper.SetWaitCursor();
            try
            {
                var imagesWithBoundsFile = new List<string>();
                foreach (var imageFileName in ImagePaths)
                {
                    var boundsFileName = GetImageBoundsFileName(imageFileName);
                    if (!File.Exists(boundsFileName))
                        continue;

                    imagesWithBoundsFile.Add(imageFileName);
                }

                if (imagesWithBoundsFile.Count == 0)
                {
                    throw new Exception("No bounds files found");
                }

                int lineCount = 0;
                foreach (var imageFileName in imagesWithBoundsFile)
                {
                    var boundsFileName = GetImageBoundsFileName(imageFileName);

                    using (var img = new Bitmap(Image.FromFile(imageFileName)))
                    {
                        var dx = 1.0 / img.Width;
                        var dy = 1.0 / img.Height;

                        var lines = File.ReadAllLines(boundsFileName);
                        lineCount += lines.Length;
                        for (int i = 0; i < lines.Length; i++)
                        {
                            var line = lines[i];
                            var parts = line.Split(' ');
                            if (parts.Length != 5)
                                throw new Exception(string.Format("File '{0}' contains {1} parts on line {2}; expected 5", boundsFileName, parts.Length, i + 1));
                            int classIndex = int.Parse(parts[0]);
                            if (classIndex >= Classes.Count || classIndex < 0)
                                throw new Exception(string.Format("File '{0}' contains an error on line {1} (class index {2} out of bounds)", boundsFileName, i + 1, classIndex));
                            double x = double.Parse(parts[1]);
                            double y = double.Parse(parts[2]);
                            double w = double.Parse(parts[3]);
                            double h = double.Parse(parts[4]);
                            if (x < 0 || y < 0 || x >= 1 || y >= 1)
                                throw new Exception(string.Format("File '{0}' contains an error on line {1} (x,y) out of bounds", boundsFileName, i + 1));
                            if (w < dx * 2)
                                throw new Exception(string.Format("File '{0}' contains an error on line {1} w less than 2dx", boundsFileName, i + 1));
                            if (h < dy * 2)
                                throw new Exception(string.Format("File '{0}' contains an error on line {1} h less than 2dy", boundsFileName, i + 1));
                            if (w < 0.01)
                                throw new Exception(string.Format("File '{0}' contains an error on line {1} w less than 1% of total image width", boundsFileName, i + 1));
                            if (h < 0.01)
                                throw new Exception(string.Format("File '{0}' contains an error on line {1} h less than 1% of total image height", boundsFileName, i + 1));
                            if (x - (w / 2.0) < 0 || y - (h / 2.0) < 0 || x + (w / 2.0) > 1 || y + (h / 2.0) > 1)
                                throw new Exception(string.Format("File '{0}' contains an error on line {1} (x(+/-)w/2,y(+/-h)/2) out of bounds", boundsFileName, i + 1));
                        }
                    }
                }

                MouseHelper.ResetCursor();
                MessageBox(
                    $"Validation successful.\n\nFiles: {imagesWithBoundsFile.Count:#,0}\nBoxes: {lineCount:#,0}",
                    "Validate bounding boxes",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            catch (Exception e)
            {
                MouseHelper.ResetCursor();
                MessageBox(e.Message, "Validate bounding boxes", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OnClassesChanged()
        {
            if (Classes == null)
            {
                SortedClasses = null;
                return;
            }

            SortedClasses = new ObservableCollection<string>(Classes.OrderBy(p => p.ToLowerInvariant()));
        }

        private void OnSelectedRegionIndexChanged()
        {
            var idx = SelectedRegionIndex;
            if (idx == null)
                return;
            if (Classes == null || ImageRegions == null)
                return;

            var region = ImageRegions[idx.Value];
            var classIndex = region.Class;
            if (classIndex == null || classIndex < 0 || classIndex >= Classes.Count)
            {
                classIndex = 0;
                region.Class = classIndex.Value;
                ImageRegions[idx.Value] = region;
            }

            SelectedRegionClass = Classes[classIndex.Value];
        }

        private void OnSelectedRegionClassChanged()
        {
            var idx = SelectedRegionIndex;
            if (idx == null)
                return;
            if (string.IsNullOrWhiteSpace(SelectedRegionClass))
                return;

            int classIndex = Classes.IndexOf(SelectedRegionClass);
            if (classIndex == -1)
            {
                SelectedRegionClass = null;
                return;
            }

            var region = ImageRegions[idx.Value];
            region.Class = classIndex;

            ImageRegions[idx.Value] = region;

            RaisePropertyChanged(nameof(ImageRegions), null, null);
            SelectedRegionIndex = idx.Value;
            RaisePropertyChanged(nameof(SelectedRegionIndex), null, null);

            SaveImageRegions();
        }

        private void OnMassSelectedClassChanged()
        {
            if (string.IsNullOrWhiteSpace(MassSelectedClass))
                return;

            int classIndex = Classes.IndexOf(MassSelectedClass);
            if (classIndex == -1)
                return;

            var list = GetFileRegionModels(ImagePaths, Classes, classIndex);

            MassClassImages = list;
        }

        private void RefreshSelectedImageClassImages()
        {
            if (string.IsNullOrWhiteSpace(CurrentImage))
                return;

            var list = GetFileRegionModels(new[] { CurrentImage }, Classes, -1);

            SelectedImageClassImages = list;
        }

        private static ObservableCollection<FileRegionModel> GetFileRegionModels(ICollection<string> imagePaths, IList<string> classes, int classIndex)
        {
            MouseHelper.SetWaitCursor();
            var list = new ObservableCollection<FileRegionModel>();
            try
            {
                if (imagePaths == null || imagePaths.Count == 0)
                    return list;

                foreach (var imageFileName in imagePaths)
                {
                    Bitmap img = null;

                    if (!File.Exists(imageFileName))
                        continue;

                    var fileName = GetImageBoundsFileName(imageFileName);
                    if (!File.Exists(fileName))
                        continue;

                    var lines = File.ReadAllLines(fileName);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        var line = lines[i];
                        var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length != 5)
                            continue;

                        if (!int.TryParse(parts[0], out var idx))
                            continue;

                        if (classIndex != -1 && idx != classIndex)
                            continue;

                        if (img == null)
                            img = new Bitmap(Image.FromFile(imageFileName));

                        var yoloCoords = new YoloCoords
                        {
                            Class = idx,
                            X = double.Parse(parts[1]),
                            Y = double.Parse(parts[2]),
                            Width = double.Parse(parts[3]),
                            Height = double.Parse(parts[4])
                        };

                        var rwidth = (int)(yoloCoords.Width * img.Width);
                        var rheight = (int)(yoloCoords.Height * img.Height);
                        var imgx = (int)(yoloCoords.X * img.Width - rwidth / 2.0);
                        var imgy = (int)(yoloCoords.Y * img.Height - rheight / 2.0);

                        var rect = new Rectangle(imgx, imgy, rwidth, rheight);
                        var newImg = img.Clone(rect, img.PixelFormat);

                        var bmpsource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                            newImg.GetHbitmap(),
                            IntPtr.Zero,
                            Int32Rect.Empty,
                            BitmapSizeOptions.FromWidthAndHeight(rwidth, rheight)
                        );

                        var item = new FileRegionModel
                        {
                            FileName = fileName,
                            YoloCoords = yoloCoords,
                            FileLineIndex = i,
                            BitmapImage = bmpsource,
                            ClassName = classes[idx]
                        };

                        list.Add(item);
                    }

                    if (img != null)
                        img.Dispose();
                }

                list = new ObservableCollection<FileRegionModel>(list.OrderBy(p => p.ClassName.ToLowerInvariant()));
            }
            finally
            {
                MouseHelper.ResetCursor();
            }

            return list;
        }
    }
}
