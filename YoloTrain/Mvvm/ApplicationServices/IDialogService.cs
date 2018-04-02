using System.Windows;

namespace YoloTrain.Mvvm.ApplicationServices
{
    /// <summary>
    /// IDialogService
    /// </summary>
    public interface IDialogService
    {
        /// <summary>Shows a window.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="window">The window.</param>
        /// <returns>The result of <see cref="IWindow.ShowDialog()"/>.</returns>
        bool? ShowWindow(Window owner, IWindow window);

        /// <summary>Shows a window.</summary>
        /// <typeparam name="T">The window type.</typeparam>
        /// <param name="owner">The owner.</param>
        /// <returns>The result of <see cref="IWindow.ShowDialog()"/>.</returns>
        bool? ShowWindow<T>(Window owner)
            where T : IWindow;

        /// <summary>Shows the open file dialog.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="title">The dialog title.</param>
        /// <param name="filter">The file filter. For example, Text files (*.txt)|*.txt|All files (*.*)|*.*.</param>
        /// <param name="fileName">Name of the file opened.</param>
        /// <returns><c>true</c> if a file is selected.</returns>
        bool? ShowOpenFileDialog(Window owner, string title, string filter, out string fileName);
    }
}
