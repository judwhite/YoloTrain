using System.Windows;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace YoloTrain.Mvvm.ApplicationServices
{
    public class DialogService : IDialogService
    {
        public bool? ShowWindow<T>(Window owner)
            where T : IWindow
        {
            return ShowWindow(owner, IoC.Resolve<T>());
        }

        public bool? ShowWindow(Window owner, IWindow window)
        {
            MouseHelper.SetWaitCursor();
            try
            {
                window.Owner = owner;
                var viewModel = window.DataContext as IViewModel;
                if (viewModel != null)
                    viewModel.CloseWindow = (result) => { ((Window)window).DialogResult = result; window.Close(); };
            }
            finally
            {
                MouseHelper.ResetCursor();
            }

            return window.ShowDialog();
        }

        public bool? ShowSelectFolderDialog(Window owner, out string directoryName)
        {
            CommonOpenFileDialog cfd;
            MouseHelper.SetWaitCursor();
            try
            {
                cfd = new CommonOpenFileDialog();
                cfd.InitialDirectory = @"C:\";
                cfd.IsFolderPicker = true;
            }
            finally
            {
                MouseHelper.ResetCursor();
            }

            var result = cfd.ShowDialog(owner);
            if (result == CommonFileDialogResult.Ok)
                directoryName = cfd.FileName;
            else
                directoryName = null;

            return !string.IsNullOrWhiteSpace(directoryName);
        }

        public bool? ShowOpenFileDialog(Window owner, string title, string filter, out string fileName)
        {
            OpenFileDialog openFileDialog;
            MouseHelper.SetWaitCursor();
            try
            {
                openFileDialog = new OpenFileDialog();
                openFileDialog.Title = title;
                openFileDialog.Filter = filter;
            }
            finally
            {
                MouseHelper.ResetCursor();
            }

            bool? result = openFileDialog.ShowDialog(owner);

            if (result == true)
                fileName = openFileDialog.FileName;
            else
                fileName = null;

            return result;
        }
    }
}
