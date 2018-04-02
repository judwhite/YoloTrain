using System;
using System.ComponentModel;
using System.Windows;
using YoloTrain.Mvvm.ApplicationServices;
using YoloTrain.Mvvm.Events;

namespace YoloTrain.Mvvm
{
    /// <summary>
    /// IViewModelBase
    /// </summary>
    public interface IViewModel : INotifyPropertyChanged
    {
        /// <summary>Occurs when MessageBox /> has been called.</summary>
        event EventHandler<ShowMessageBoxEventArgs> ShowMessageBox;

        /// <summary>Occurs when ShowWindow has been called.</summary>
        event EventHandler<ShowDialogWindowEventArgs> ShowDialogWindow;

        /// <summary>Occurs when ShowOpenFileDialog has been called.</summary>
        event EventHandler<ShowOpenFileDialogEventArgs> ShowOpenFile;

        /// <summary>Gets or sets the close window action.</summary>
        /// <value>The close window action.</value>
        Action<bool?> CloseWindow { get; set; }

        /// <summary>Gets or sets the current visual state.</summary>
        /// <value>The current visual state.</value>
        string CurrentVisualState { get; set; }
    }

    /// <summary>
    /// ViewModelBase
    /// </summary>
    public abstract class ViewModel : Model, IViewModel
    {
        /// <summary>Occurs when MessageBox has been called.</summary>
        public event EventHandler<ShowMessageBoxEventArgs> ShowMessageBox;

        /// <summary>Occurs when ShowWindow has been called.</summary>
        public event EventHandler<ShowDialogWindowEventArgs> ShowDialogWindow;

        /// <summary>Occurs when ShowOpenFileDialog has been called.</summary>
        public event EventHandler<ShowOpenFileDialogEventArgs> ShowOpenFile;

        /// <summary>
        /// Gets or sets the close window action.
        /// </summary>
        /// <value>The close window action.</value>
        public Action<bool?> CloseWindow { get; set; }

        /// <summary>Gets or sets the current visual state.</summary>
        /// <value>The current visual state.</value>
        public string CurrentVisualState
        {
            get { return Get<string>("CurrentVisualState"); }
            set
            {
                BeginInvoke(() => Set("CurrentVisualState", value));
            }
        }

        /// <summary>Invokes the specified action on the UI thread.</summary>
        /// <param name="action">The action to invoke.</param>
        protected void BeginInvoke(Action action)
        {
            IoC.Resolve<IDispatcher>().BeginInvoke(action);
        }

        /// <summary>Determines whether the calling thread is the thread associated with this <see cref="System.Windows.Threading.Dispatcher" />.</summary>
        /// <returns><c>true</c> if the calling thread is the thread associated with this <see cref="System.Windows.Threading.Dispatcher" />; otherwise, <c>false</c>.</returns>
        protected bool CheckAccess()
        {
            return IoC.Resolve<IDispatcher>().CheckAccess();
        }

        /// <summary>Messages the box.</summary>
        /// <param name="messageBoxEvent">The message box event.</param>
        /// <returns>The message box result.</returns>
        protected MessageBoxResult MessageBox(ShowMessageBoxEventArgs messageBoxEvent)
        {
            if (messageBoxEvent == null)
                throw new ArgumentNullException(nameof(messageBoxEvent));

            var handler = ShowMessageBox;
            if (handler != null)
                handler(this, messageBoxEvent);
            else
                throw new Exception("'ShowMessageBox' event is not subscribed to.");

            return messageBoxEvent.Result;
        }

        /// <summary>Shows a message box.</summary>
        /// <param name="messageBoxText">The message box text.</param>
        /// <param name="caption">The caption.</param>
        /// <param name="buttons">The buttons.</param>
        /// <param name="image">The image.</param>
        /// <returns>The message box result.</returns>
        protected MessageBoxResult MessageBox(string messageBoxText, string caption, MessageBoxButton buttons, MessageBoxImage image)
        {
            var messageBox = new ShowMessageBoxEventArgs
            {
                MessageBoxText = messageBoxText,
                Caption = caption,
                MessageBoxButton = buttons,
                MessageBoxImage = image
            };

            return MessageBox(messageBox);
        }

        /// <summary>Shows a dialog window.</summary>
        /// <typeparam name="T">The window type.</typeparam>
        /// <returns>The result of <see cref="IWindow.ShowDialog()" />.</returns>
        protected bool? ShowWindow<T>()
            where T : IWindow
        {
            var showWindowEvent = new ShowDialogWindowEventArgs
            {
                IWindow = IoC.Resolve<T>(),
            };

            var handler = ShowDialogWindow;
            if (handler != null)
                handler(this, showWindowEvent);
            else
                throw new Exception("'ShowDialogWindow' event is not subscribed to.");

            return showWindowEvent.Result;
        }

        /// <summary>Shows the open file dialog.</summary>
        /// <param name="title">The dialog title.</param>
        /// <param name="filter">The file filter. For example, Text files (*.txt)|*.txt|All files (*.*)|*.*</param>
        /// <param name="fileName">Name of the file opened.</param>
        /// <returns><c>true</c> if a file is selected.</returns>
        protected bool? ShowOpenFileDialog(string title, string filter, out string fileName)
        {
            var showOpenFileDialogEvent = new ShowOpenFileDialogEventArgs
            {
                Title = title,
                Filter = filter
            };

            var handler = ShowOpenFile;
            if (handler != null)
                handler(this, showOpenFileDialogEvent);
            else
                throw new Exception("'ShowOpenFile' event is not subscribed to.");

            if (showOpenFileDialogEvent.Result == true)
                fileName = showOpenFileDialogEvent.FileName;
            else
                fileName = null;

            return showOpenFileDialogEvent.Result;
        }
    }
}
