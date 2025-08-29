namespace FluffyVoid.Logging;

public partial class LogManager
{
    /// <summary>
    ///     Logs out a verbose message to all registered log targets
    /// </summary>
    /// <param name="message">The message to log out</param>
    /// <param name="source">The source value to assign to the log message</param>
    /// <param name="color">The color to assign to the log message</param>
    /// <param name="ex">The exception to assign to the log message</param>
    public static void LogVerbose(object message, string source, string color = "", Exception? ex = null)
    {
        Log(GetLogInformation<LogInformation>(message, source, color, Severity.Verbose, ex));
    }
    /// <summary>
    ///     Logs out a verbose message to all registered log targets except for the specified log target
    /// </summary>
    /// <typeparam name="T">The type of the log target to exclude from logging to</typeparam>
    /// <param name="message">The message to log out</param>
    /// <param name="source">The source value to assign to the log message</param>
    /// <param name="color">The color to assign to the log message</param>
    /// <param name="ex">The exception to assign to the log message</param>
    public static void LogVerboseExcluding<T>(object message, string source, string color = "", Exception? ex = null)
    {
        LogExcluding(GetLogInformation<LogInformation>(message, source, color, Severity.Verbose, ex), typeof(T));
    }
    /// <summary>
    ///     Logs out a verbose message to the specified log target
    /// </summary>
    /// <typeparam name="T">The type of the log target to solely log to</typeparam>
    /// <param name="message">The message to log out</param>
    /// <param name="source">The source value to assign to the log message</param>
    /// <param name="color">The color to assign to the log message</param>
    /// <param name="ex">The exception to assign to the log message</param>
    public static void LogVerboseTo<T>(object message, string source, string color = "", Exception? ex = null)
    {
        LogTo(GetLogInformation<LogInformation>(message, source, color, Severity.Verbose, ex), typeof(T));
    }
}
