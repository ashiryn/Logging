namespace FluffyVoid.Logging;

/// <summary>
///     Base class for Log Targets that defines how the LogManager will log information out
/// </summary>
[Serializable]
public abstract class LogTarget
{
    /// <summary>
    ///     Determines whether the log target will log by default with every Log call, or only when called to Log explicitly
    /// </summary>
    public bool LogByDefault { get; protected set; }
    /// <summary>
    ///     Determines whether the log target will include a detailed callstack as part of its logging duties or not
    /// </summary>
    protected bool IncludeCallStack { get; set; } = true;
    /// <summary>
    ///     Locking object to ensure that the LogManager maintains thread safety
    /// </summary>
    protected object LogLock { get; set; }

    /// <summary>
    ///     Constructor used to initialize the log target
    /// </summary>
    /// <param name="logByDefault">Determines whether the log target will log by default or only when logged to explicitly</param>
    /// <param name="includeCallStack">Whether the log target should display the full call stack</param>
    protected LogTarget(bool logByDefault, bool includeCallStack)
    {
        LogLock = new object();
        LogByDefault = logByDefault;
        IncludeCallStack = includeCallStack;
    }
    /// <summary>
    ///     Constructor used to initialize the log target
    /// </summary>
    protected LogTarget()
    {
        LogLock = new object();
    }

    /// <summary>
    ///     Function used to clean up any resources used by the target
    /// </summary>
    public virtual void Destroy()
    {
    }
    /// <summary>
    ///     Function used by the LogManager to log a message out, contains the logic which determines how the message gets
    ///     logged out
    /// </summary>
    /// <param name="data">The data to be displayed to the log target</param>
    public abstract void Log(LogInformation data);
}
