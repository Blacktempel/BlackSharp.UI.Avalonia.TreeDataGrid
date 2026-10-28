namespace BlackSharp.UI.Avalonia.TreeDataGrid
{
    /// <summary>
    /// Represents the gestures that can cause a cell to enter edit mode.
    /// </summary>
    [Flags]
    public enum BeginEditGestures
    {
        /// <summary>
        /// A cell will only enter edit mode programmatically.
        /// </summary>
        None = 0x00,

        /// <summary>
        /// A cell will enter edit mode when the user presses F2.
        /// </summary>
        F2 = 0x01,

        /// <summary>
        /// A cell will enter edit mode when the user taps it.
        /// </summary>
        Tap = 0x02,

        /// <summary>
        /// A cell will enter edit mode when the user double-taps it.
        /// </summary>
        DoubleTap = 0x4,

        /// <summary>
        /// A cell will enter edit mode in conjuction with a gesture only when the cell or row
        /// is currently selected.
        /// </summary>
        WhenSelected = 0x1000,

        /// <summary>
        /// A cell will enter edit mode when the user presses F2 or double-taps it.
        /// </summary>
        TextInput = 0x08,

        Default = F2 | DoubleTap,
    }
}
