using System;

namespace YoloTrain.Mvvm.Events
{
    /// <summary>
    /// ShowSelectFolderDialogEventArgs
    /// </summary>
    public class ShowSelectFolderDialogEventArgs : EventArgs
    {
        /// <summary>Gets or sets the dialog title.</summary>
        /// <value>The dialog title.</value>
        public string Title { get; set; }

        /// <summary>Gets or sets initial directory.</summary>
        /// <value>The initial directory.</value>
        public string InitialDirectory { get; set; }

        /// <summary>Gets or sets the selected directory.</summary>
        /// <value>The selected directory.</value>
        public string SelectedDirectory { get; set; }

        /// <summary>Gets or sets the result.</summary>
        /// <value>The result.</value>
        public bool? Result { get; set; }
    }
}
