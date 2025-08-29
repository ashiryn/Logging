namespace FluffyVoid.Logging;

public partial class LogManager
{
    /// <summary>
    ///     Logs out a message to all registered log targets
    /// </summary>
    /// <param name="message">The message to log out</param>
    /// <param name="source">The source value to assign to the log message</param>
    /// <param name="color">The color to assign to the log message</param>
    /// <param name="severity">The desired severity to assign to the log message</param>
    /// <param name="ex">The exception to assign to the log message</param>
    public static void Log(object message, string source, string color = "", Severity severity = Severity.Debug, Exception? ex = null)
    {
        Log(GetLogInformation<LogInformation>(message, source, color, severity, ex));
    }
    /// <summary>
    ///     Logs out a message to all registered log targets except for the specified log target
    /// </summary>
    /// <typeparam name="T">The type of the log target to exclude from logging to</typeparam>
    /// <param name="message">The message to log out</param>
    /// <param name="source">The source value to assign to the log message</param>
    /// <param name="color">The color to assign to the log message</param>
    /// <param name="severity">The desired severity to assign to the log message</param>
    /// <param name="ex">The exception to assign to the log message</param>
    public static void LogExcluding<T>(object message, string source, string color = "", Severity severity = Severity.Debug, Exception? ex = null)
    {
        LogExcluding(GetLogInformation<LogInformation>(message, source, color, severity, ex), typeof(T));
    }
    /// <summary>
    ///     Logs out a message to the specified log target
    /// </summary>
    /// <typeparam name="T">The type of the log target to solely log to</typeparam>
    /// <param name="message">The message to log out</param>
    /// <param name="source">The source value to assign to the log message</param>
    /// <param name="color">The color to assign to the log message</param>
    /// <param name="severity">The desired severity to assign to the log message</param>
    /// <param name="ex">The exception to assign to the log message</param>
    public static void LogTo<T>(object message, string source, string color = "", Severity severity = Severity.Debug, Exception? ex = null)
    {
        LogTo(GetLogInformation<LogInformation>(message, source, color, severity, ex), typeof(T));
    }
}
