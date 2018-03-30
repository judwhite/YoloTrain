using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using YoloTrain.Mvvm.ApplicationServices;
using YoloTrain.Mvvm.Events;

namespace YoloTrain.Mvvm
{
    /// <summary>
    /// WindowView. Handles generic window settings.
    /// </summary>
    public class WindowView : Window, IWindow
    {
        public static readonly DependencyProperty CurrentVisualStateProperty =
            DependencyProperty.Register("CurrentVisualState", typeof(string), typeof(WindowView), new PropertyMetadata(default(string), CurrentVisualStateChanged));

        public static readonly DependencyProperty HandleEscapeProperty =
            DependencyProperty.Register("HandleEscape", typeof(bool), typeof(WindowView), new PropertyMetadata(default(bool)));

        private static readonly IDialogService _dialogService;

        static WindowView()
        {
            _dialogService = IoC.Resolve<IDialogService>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowView"/> class.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        protected WindowView(IViewModel viewModel)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            DataContext = viewModel;

            PreviewKeyDown += WindowView_PreviewKeyDown;
            HandleEscape = true;
            ShowInTaskbar = false;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            FontFamily = new FontFamily("Verdana");
            FontSize = 11.0d;
            Background = (Brush)Application.Current.Resources["WindowBackground"];

            viewModel.ShowMessageBox += ViewModel_ShowMessageBox;
            viewModel.ShowDialogWindow += ViewModel_ShowDialogWindow;
            viewModel.ShowOpenFile += ViewModel_ShowOpenFile;
        }

        private void ViewModel_ShowOpenFile(object sender, ShowOpenFileDialogEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            string fileName;
            bool? result = _dialogService.ShowOpenFileDialog(this, e.Title, e.Filter, out fileName);
            e.FileName = fileName;
            e.Result = result;
        }

        private void ViewModel_ShowDialogWindow(object sender, ShowDialogWindowEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            bool? result = _dialogService.ShowWindow(this, e.IWindow);
            e.Result = result;
        }

        private void ViewModel_ShowMessageBox(object sender, ShowMessageBoxEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            var result = MessageBox.Show
            (
                owner: this,
                messageBoxText: e.MessageBoxText,
                caption: e.Caption,
                button: e.MessageBoxButton,
                icon: e.MessageBoxImage
            );

            e.Result = result;
        }

        /// <summary>Property changed handler for <see cref="CurrentVisualState" /> dependency property.</summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="dependencyPropertyChangedEventArgs">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void CurrentVisualStateChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var window = (WindowView)dependencyObject;
            if (window.CurrentVisualState != null)
            {
                VisualStateManager.GoToElementState(window, window.CurrentVisualState, useTransitions: true);
            }
        }

        /// <summary>Gets or sets the current visual state.</summary>
        /// <value>The current visual state.</value>
        public string CurrentVisualState
        {
            get { return (string)GetValue(CurrentVisualStateProperty); }
            set { SetValue(CurrentVisualStateProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the escape key should be used to close the window.
        /// </summary>
        /// <value><c>true</c> if the escape key should be used to close the window; otherwise, <c>false</c>.</value>
        public bool HandleEscape
        {
            get { return (bool)GetValue(HandleEscapeProperty); }
            set { SetValue(HandleEscapeProperty, value); }
        }

        private void WindowView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (HandleEscape)
            {
                if (e.Key == Key.Escape)
                {
                    Close();
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Only here to support XAML. Use <see cref="WindowView(IViewModel)" /> instead.
        /// </summary>
        public WindowView()
        {
            // Note: only here to support XAML
            throw new NotSupportedException("Only here to support XAML. Use WindowView(IViewModel) instead.");
        }
    }
}
