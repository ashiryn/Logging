namespace FluffyVoid.Logging;

/// <summary>
///     Data class used to store data to be logged out and the target to log to. Used solely to ensure the LogManager is
///     thread safe
/// </summary>
[Serializable]
public class LogQueueData
{
    /// <summary>
    ///     The log message to be displayed to a log target
    /// </summary>
    public LogInformation? Information { get; private set; }
    /// <summary>
    ///     List of the LogTargets that need to be logged to
    /// </summary>
    public HashSet<Type> Targets { get; }

    /// <summary>
    ///     Constructor used to initialize the data for use
    /// </summary>
    public LogQueueData()
    {
        Targets = new HashSet<Type>();
    }
    /// <summary>
    ///     Recycles the embedded log information for reuse within the LogManager
    /// </summary>
    internal void Recycle()
    {
        Information?.Recycle();
    }

    /// <summary>
    ///     Resets the data to a fresh state to aid in recycling of the queue data
    /// </summary>
    /// <param name="information">The log information that is to be logged out</param>
    internal void Renew(LogInformation information)
    {
        Information = information;
        Targets.Clear();
    }
}
