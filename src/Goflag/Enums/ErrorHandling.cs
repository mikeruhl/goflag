namespace GoFlag.Enums
{
    /// <summary>
    /// Passed into FlagSet, will dictate behavior of parsing if an error is experienced.
    /// </summary>
    public enum ErrorHandling
    {
        /// <summary>
        /// On experiencing an error in parsing, will exit Parse method with error.
        /// </summary>
        ContinueOnError = 0,
        /// <summary>
        /// Will exit program if error is experienced.  If -help/-h is passed, it will exit with code 0 and display help.  Otherwise, it will exit with code 2 and error message.
        /// </summary>
        ExitOnError,
        /// <summary>
        /// A panic in Go is like throwing an exception.  Using this will throw an exception of type InvalidOperationException if an error in parsing is experienced.s
        /// </summary>
        PanicOnError
    }
}
