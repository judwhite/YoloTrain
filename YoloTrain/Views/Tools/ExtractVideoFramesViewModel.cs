using System.Collections.ObjectModel;
using System.Windows.Input;
using YoloTrain.Mvvm;

namespace YoloTrain.Views.Tools
{
    public interface IExtractVideoFramesViewModel : IViewModel
    {
        ObservableCollection<InputFile> InputFiles { get; }
        string FilePrefix { get; set; }
        string OutputDirectory { get; set; }

        ICommand AddInputFilesCommand { get; }
        ICommand RemoveSelectedInputFilesCommand { get; }
        ICommand ClearInputFilesCommand { get; }
        ICommand ConvertCommand { get; }
        ICommand CancelCommand { get; }
    }

    public class InputFile : ViewModel
    {
        public string FullPath
        {
            get => Get<string>(nameof(FullPath));
            set => Set(nameof(FullPath), value);
        }

        public decimal? PercentComplete
        {
            get => Get<decimal?>(nameof(PercentComplete));
            set => Set(nameof(PercentComplete), value);
        }
    }

    public class ExtractVideoFramesViewModel : ViewModel, IExtractVideoFramesViewModel
    {
        public ObservableCollection<InputFile> InputFiles { get; }
        public string FilePrefix { get; set; }
        public string OutputDirectory { get; set; }
        public ICommand AddInputFilesCommand { get; }
        public ICommand RemoveSelectedInputFilesCommand { get; }
        public ICommand ClearInputFilesCommand { get; }
        public ICommand ConvertCommand { get; }
        public ICommand CancelCommand { get; }
    }
}
