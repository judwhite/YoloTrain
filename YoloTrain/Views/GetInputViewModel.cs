using System.Windows.Input;
using YoloTrain.Mvvm;

namespace YoloTrain.Views
{
    public interface IGetInputViewModel : IViewModel
    {
        string WindowTitle { get; set; }
        string LabelText { get; set; }
        string InputValue { get; set; }
        ICommand OKCommand { get; }
    }

    public class GetInputViewModel : ViewModel, IGetInputViewModel
    {
        public GetInputViewModel()
        {
            OKCommand = new DelegateCommand(() => CloseWindow(true), () => !string.IsNullOrWhiteSpace(InputValue), false);
        }

        public string WindowTitle
        {
            get => Get<string>(nameof(WindowTitle));
            set => Set(nameof(WindowTitle), value);
        }

        public string LabelText
        {
            get => Get<string>(nameof(LabelText));
            set => Set(nameof(LabelText), value);
        }

        public string InputValue
        {
            get => Get<string>(nameof(InputValue));
            set => Set(nameof(InputValue), value);
        }

        public ICommand OKCommand
        {
            get => Get<ICommand>(nameof(OKCommand));
            private set => Set(nameof(OKCommand), value);
        }
    }
}
