namespace FluffyVoid.Logging;

/// <summary>
///     Enum used to distinguish the different severities that the LogManager cares about
/// </summary>
public enum Severity
{
    /// <summary>
    ///     Lowest severity level used to log verbose information
    /// </summary>
    Verbose,
    /// <summary>
    ///     Severity level used to log developer debug information
    /// </summary>
    Debug,
    /// <summary>
    ///     Severity level used to log Guard clauses
    /// </summary>
    Guard,
    /// <summary>
    ///     Severity level used to log warning information
    /// </summary>
    Warning,
    /// <summary>
    ///     Severity level used to log command information
    /// </summary>
    Command,
    /// <summary>
    ///     Severity level used to log information
    /// </summary>
    Info,
    /// <summary>
    ///     Severity level used to log error information
    /// </summary>
    Error,
    /// <summary>
    ///     Severity level used to log exception information
    /// </summary>
    Exception,
    /// <summary>
    ///     Severity level used to log critical information
    /// </summary>
    Critical,
    /// <summary>
    ///     Severity level used to log fatal information
    /// </summary>
    Fatal,
    /// <summary>
    ///     Severity level used to always log information
    /// </summary>
    Always
}
