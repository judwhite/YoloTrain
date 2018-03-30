namespace YoloTrain.Mvvm.Events
{
    /// <summary>
    /// ShowWindowEventArgs
    /// </summary>
    public class ShowDialogWindowEventArgs
    {
        /// <summary>Gets or sets the type of the window. Must inherit <see cref="IWindow" />.</summary>
        /// <value>The type of the window.</value>
        public IWindow IWindow { get; set; }

        /// <summary>Gets or sets the ShowDialog result.</summary>
        /// <value>The ShowDialog result.</value>
        public bool? Result { get; set; }
    }
}
