using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace FluffyVoid.Logging;

/// <summary>
///     Global Log Manager that allows for a single log line to log out to multiple targets, I.E write to file, to screen,
///     to database, etc...
/// </summary>
public partial class LogManager
{
    /// <summary>
    ///     The maximum number of log update per dispatch
    /// </summary>
    private const int MaximumLogsPerUpdate = 5;
    /// <summary>
    ///     Sudo-Instance variable used to ensure that only 1 LogManager ever exists no matter how many are attempted to be
    ///     used in the project
    /// </summary>
    private static LogManager? s_instance;

    /// <summary>
    ///     Log target list that stores the different log targets that this LogManager will use to log out information
    /// </summary>
    private readonly Dictionary<Type, LogTarget> _logTargets = new Dictionary<Type, LogTarget>();
    /// <summary>
    ///     Pool manager used to pool the LogInformation and LogQueueData classes
    /// </summary>
    private readonly LogPoolManager _poolManager;

    /// <summary>
    ///     The number of registered log targets currently in use
    /// </summary>
    public static int Count => Instance._logTargets.Count;

    /// <summary>
    ///     Whether the LogManager should operate in a thread safe way by Dispatching messages, or immediately log messages
    ///     when they are received
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public static bool UseDispatcher { get; set; }

    /// <summary>
    ///     Locking object used to keep the LogManager thread safe
    /// </summary>
    protected object LogLock { get; } = new object();
    /// <summary>
    ///     Queue used to register data for logging in a thread safe way
    /// </summary>
    protected Queue<LogQueueData> LogQueue { get; } = new Queue<LogQueueData>();
    /// <summary>
    ///     Maximum lines to include in the callstack
    /// </summary>
    protected int MaxStackTrace { get; private set; } = 10;
    /// <summary>
    ///     The set severity log level assigned to the log manager, discarding any messages below the set level
    /// </summary>
    protected Severity SeverityFilter { get; private set; }

    /// <summary>
    ///     Property that creates the singleton/instance capability of this class
    /// </summary>
    // ReSharper disable once ConvertToNullCoalescingCompoundAssignment
    private static LogManager Instance => s_instance ?? (s_instance = new LogManager());

    /// <summary>
    ///     Default constructor
    /// </summary>
    internal LogManager()
    {
        _poolManager = new LogPoolManager(20);
    }

    /// <summary>
    ///     Function used to add a new log target to the LogManager if it does not already exist
    /// </summary>
    /// <param name="target">The log target to add to the manager</param>
    public static void AddLogTarget(LogTarget target)
    {
        lock(Instance.LogLock)
        {
            Type key = target.GetType();

            if(!Instance._logTargets.ContainsKey(key))
            {
                Instance._logTargets.Add(key, target);
            }
        }
    }

    /// <summary>
    ///     Retrieves a pooled LogInformation object for use in creating a log message
    /// </summary>
    /// <param name="message">The message to store within the information</param>
    /// <param name="source">The source value to assign to the information</param>
    /// <param name="color">The desired color to use for the general log message</param>
    /// <param name="severity">The desired severity for the log information to be set to</param>
    /// <param name="ex">The exception to assign to the information</param>
    /// <typeparam name="TType">The type of LogInformation to retrieve</typeparam>
    /// <returns>A LogInformation object from the object pool</returns>
    public static LogInformation GetLogInformation<TType>(object message, string source, string color = "", Severity severity = Severity.Debug, Exception? ex = null)
        where TType : LogInformation
    {
        lock(Instance.LogLock)
        {
            LogInformation result = Instance._poolManager.GetLogInformation<TType>();
            result.Recycled += RecycleLogInformation;
            result.Initialize(message, source, color, severity, ex);

            return result;
        }
    }

    /// <summary>
    ///     Queues the LogInformation to be logged out to the log targets
    /// </summary>
    /// <param name="data">The data to be logged out</param>
    public static void Log(LogInformation data)
    {
        if(data.LogSeverity < Instance.SeverityFilter)
        {
            return;
        }

        data.CallStack = GetFormattedCallStack();

        lock(Instance.LogLock)
        {
            if(UseDispatcher)
            {
                LogQueueData entry = Instance._poolManager.GetQueueData();
                entry.Renew(data);

                foreach(Type target in Instance._logTargets.Keys)
                {
                    if(Instance._logTargets[target] != null && Instance._logTargets[target].LogByDefault)
                    {
                        entry.Targets.Add(target);
                    }
                }

                Instance.LogQueue.Enqueue(entry);
            }
            else
            {
                foreach(Type target in Instance._logTargets.Keys)
                {
                    LogInfo(target, data);
                }
            }
        }
    }
    /// <summary>
    ///     Queues the LogInformation to be logged out to the log targets except for the excluded log target
    /// </summary>
    /// <param name="data">The data to be logged out</param>
    /// <param name="toExclude">The log target to exclude from logging out to</param>
    public static void LogExcluding(LogInformation data, Type toExclude)
    {
        if(data.LogSeverity < Instance.SeverityFilter)
        {
            return;
        }

        data.CallStack = GetFormattedCallStack();

        lock(Instance.LogLock)
        {
            if(UseDispatcher)
            {
                LogQueueData entry = Instance._poolManager.GetQueueData();
                entry.Renew(data);

                foreach(Type target in Instance._logTargets.Keys)
                {
                    if(target != toExclude && Instance._logTargets[target] != null && Instance._logTargets[target].LogByDefault)
                    {
                        entry.Targets.Add(target);
                    }
                }

                Instance.LogQueue.Enqueue(entry);
            }
            else
            {
                foreach(Type target in Instance._logTargets.Keys)
                {
                    if(target != toExclude && Instance._logTargets[target] != null && Instance._logTargets[target].LogByDefault)
                    {
                        LogInfo(target, data);
                    }
                }
            }
        }
    }
    /// <summary>
    ///     Queues the LogInformation to be logged out to the log targets except for the excluded log target
    /// </summary>
    /// <param name="data">The data to be logged out</param>
    /// <typeparam name="TTarget">The log target to exclude from logging out to</typeparam>
    public static void LogExcluding<TTarget>(LogInformation data)
        where TTarget : LogTarget
    {
        LogExcluding(data, typeof(TTarget));
    }
    /// <summary>
    ///     Queues the LogInformation to be logged out to the specified log target
    /// </summary>
    /// <param name="data">The data to be logged out</param>
    /// <param name="logTo">The log target to log out to</param>
    public static void LogTo(LogInformation data, Type logTo)
    {
        if(data.LogSeverity < Instance.SeverityFilter)
        {
            return;
        }

        data.CallStack = GetFormattedCallStack();

        lock(Instance.LogLock)
        {
            if(Instance._logTargets.ContainsKey(logTo))
            {
                if(UseDispatcher)
                {
                    LogQueueData entry = Instance._poolManager.GetQueueData();
                    entry.Renew(data);
                    entry.Targets.Add(logTo);

                    Instance.LogQueue.Enqueue(entry);
                }
                else
                {
                    LogInfo(logTo, data);
                }
            }
        }
    }
    /// <summary>
    ///     Queues the LogInformation to be logged out to the specified log target
    /// </summary>
    /// <param name="data">The data to be logged out</param>
    /// <typeparam name="TTarget">The log target to log out to</typeparam>
    public static void LogTo<TTarget>(LogInformation data)
        where TTarget : LogTarget
    {
        LogTo(data, typeof(TTarget));
    }
    /// <summary>
    ///     Function used to remove a log target from the LogManager
    /// </summary>
    /// <param name="target">The log target type to remove</param>
    public static void RemoveLogTarget(Type target)
    {
        lock(Instance.LogLock)
        {
            if(Instance._logTargets.ContainsKey(target))
            {
                Instance._logTargets[target].Destroy();
                Instance._logTargets.Remove(target);
            }
        }
    }
    /// <summary>
    ///     Function used to set the maximum lines to include in the callstack
    /// </summary>
    /// <param name="max">The number of lines to cap the callstack to</param>
    public static void SetMaxStackFrames(int max)
    {
        Instance.MaxStackTrace = max;
    }

    /// <summary>
    ///     Sets the log severity level for the log manager, allowing incoming messages that are below the set level to be
    ///     discarded
    /// </summary>
    /// <param name="level">The log severity level to set</param>
    public static void SetSeverityLevel(Severity level)
    {
        Instance.SeverityFilter = level < 0 ? 0 : level > Severity.Always ? Severity.Always : level;
    }
    /// <summary>
    ///     Attempts to retrieve a supplied log target by its type if one has been registered to the log manager
    /// </summary>
    /// <param name="logTarget">Reference to the found Log Target if on has been registered, otherwise false</param>
    /// <typeparam name="TLogTarget">The type of Log Target to get</typeparam>
    /// <returns>True if the Log Target we found, otherwise false</returns>
    public static bool TryGetLogTarget<TLogTarget>(out TLogTarget? logTarget)
        where TLogTarget : LogTarget
    {
        Type logTargetType = typeof(TLogTarget);

        if(Instance._logTargets.ContainsKey(logTargetType) && Instance._logTargets[logTargetType] is TLogTarget result)
        {
            logTarget = result;

            return true;
        }

        logTarget = null;

        return false;
    }
    /// <summary>
    ///     Update function used to check for queued information to log, and log to the stored log targets if information is
    ///     queued
    /// </summary>
    public static void Update()
    {
        lock(Instance.LogLock)
        {
            if(!UseDispatcher || !Instance.LogQueue.Any())
            {
                return;
            }

            int currentLogCount = 0;

            do
            {
                LogInfo(Instance.LogQueue.Dequeue());
                ++currentLogCount;
            } while(currentLogCount < MaximumLogsPerUpdate && Instance.LogQueue.Any());
        }
    }

    /// <summary>
    ///     Helper function that retrieves the callstack and formats it into a readable string
    /// </summary>
    /// <returns>The callstack string to be displayed</returns>
    protected static string GetFormattedCallStack()
    {
        StackFrame[]? stackFrames = new StackTrace(true).GetFrames();
        StringBuilder result = new StringBuilder();
        int maxCount = Instance.MaxStackTrace;

        if(stackFrames == null)
        {
            return string.Empty;
        }

        for(int i = 0; i < maxCount; ++i)
        {
            if(i >= stackFrames.Length)
            {
                break;
            }

            MethodBase currentMethod = stackFrames[i].GetMethod();

            if(currentMethod != null && currentMethod.DeclaringType != null && currentMethod.DeclaringType.FullName != null)
            {
                if(currentMethod.DeclaringType.FullName.StartsWith($"{Instance.GetType().Namespace}.{Instance.GetType().Name}"))
                {
                    ++maxCount;

                    continue;
                }

                result.Append($"{currentMethod.DeclaringType.Namespace}.");

                if(currentMethod.DeclaringType.IsGenericType)
                {
                    Regex genericPattern = new Regex(@"`\d+");
                    result.Append(genericPattern.Replace(currentMethod.DeclaringType.Name, "<"));
                    Type[] arguments = currentMethod.DeclaringType.GetGenericArguments();

                    for(int argIndex = 0; argIndex < arguments.Length; ++argIndex)
                    {
                        if(argIndex == arguments.Length - 1)
                        {
                            result.Append($"{arguments[argIndex].Name}>.");
                        }
                        else
                        {
                            result.Append($"{arguments[argIndex].Name}, ");
                        }
                    }
                }
                else
                {
                    result.Append($"{currentMethod.DeclaringType.Name}.");
                }

                result.AppendLine($"{currentMethod.Name} at line {stackFrames[i].GetFileLineNumber()}");
            }
        }

        if(stackFrames.Length >= Instance.MaxStackTrace)
        {
            result.AppendLine($"...(Capped at {Instance.MaxStackTrace} stack frames)");
        }

        return result.ToString();
    }
    /// <summary>
    ///     Helper function used to log the LogInformation out to the assigned log target
    /// </summary>
    /// <param name="target">The log target to log to</param>
    /// <param name="information">The data to be logged out</param>
    internal static void LogInfo(Type target, LogInformation information)
    {
        if(!Instance._logTargets.ContainsKey(target))
        {
            return;
        }

        try
        {
            Instance._logTargets[target].Log(information);
        }
        catch(Exception ex)
        {
            Instance._logTargets.Remove(target);
            LogException($"{target} threw an exception while trying to Log, removing Log Target from the Log Manager", nameof(LogManager), ex: ex);
        }
    }
    /// <summary>
    ///     Helper function used to log the LogInformation out to the assigned log target
    /// </summary>
    /// <param name="data">The queued data to be logged</param>
    internal static void LogInfo(LogQueueData data)
    {
        foreach(Type target in data.Targets)
        {
            if(data.Information == null)
            {
                continue;
            }

            LogInfo(target, data.Information);
        }
    }

    /// <summary>
    ///     Recycled callback used to return a LogInformation back into the pool
    /// </summary>
    /// <param name="information">The LogInformation to return to the pool</param>
    private static void RecycleLogInformation(LogInformation information)
    {
        information.Recycled -= RecycleLogInformation;

        lock(Instance.LogLock)
        {
            Instance._poolManager.ReturnLogInformation(information);
        }
    }
}
