using System;
using System.Collections.Generic;
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
    public abstract class ViewModel : IViewModel
    {
        /// <summary>Dispatcher service.</summary>
        protected static readonly IDispatcher _dispatcher;

        /// <summary>Dialog service.</summary>
        protected static readonly IDialogService _dialogService;

        /// <summary>Occurs when a property value changes.</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>Occurs when MessageBox has been called.</summary>
        public event EventHandler<ShowMessageBoxEventArgs> ShowMessageBox;

        /// <summary>Occurs when ShowWindow has been called.</summary>
        public event EventHandler<ShowDialogWindowEventArgs> ShowDialogWindow;

        /// <summary>Occurs when ShowOpenFileDialog has been called.</summary>
        public event EventHandler<ShowOpenFileDialogEventArgs> ShowOpenFile;

        private readonly Dictionary<string, object> _propertyValues = new Dictionary<string, object>();

        static ViewModel()
        {
            _dispatcher = IoC.Resolve<IDispatcher>();
            _dialogService = IoC.Resolve<IDialogService>();
        }

        /// <summary>
        /// Gets or sets the close window action.
        /// </summary>
        /// <value>The close window action.</value>
        public Action<bool?> CloseWindow { get; set; }

        /// <summary>Raises the <see cref="PropertyChanged"/> event.</summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void RaisePropertyChanged(string propertyName, object oldValue, object newValue)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>Gets the specified property value.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>The property value.</returns>
        protected T Get<T>(string propertyName)
        {
            if (_propertyValues.TryGetValue(propertyName, out object value))
                return (T)value;
            else
                return default(T);
        }

        /// <summary>Sets the specified property value.</summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The value.</param>
        protected void Set<T>(string propertyName, T value)
        {
            bool keyExists;
            T oldValue;
            object oldValueObject;
            if (_propertyValues.TryGetValue(propertyName, out oldValueObject))
            {
                keyExists = true;
                oldValue = (T)oldValueObject;
            }
            else
            {
                keyExists = false;
                oldValue = default(T);
            }

            bool hasChanged = false;
            if (value != null)
            {
                if (!value.Equals(oldValue))
                    hasChanged = true;
            }
            else if (oldValue != null)
            {
                hasChanged = true;
            }

            if (hasChanged)
            {
                if (keyExists)
                    _propertyValues[propertyName] = value;
                else
                    _propertyValues.Add(propertyName, value);

                RaisePropertyChanged(propertyName, oldValue, value);
            }
        }

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
            _dispatcher.BeginInvoke(action);
        }

        /// <summary>Determines whether the calling thread is the thread associated with this <see cref="System.Windows.Threading.Dispatcher" />.</summary>
        /// <returns><c>true</c> if the calling thread is the thread associated with this <see cref="System.Windows.Threading.Dispatcher" />; otherwise, <c>false</c>.</returns>
        protected bool CheckAccess()
        {
            return _dispatcher.CheckAccess();
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
        /// <param name="filter">The file filter.</param>
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
