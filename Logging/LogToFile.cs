namespace FluffyVoid.Logging;

/// <summary>
///     Log target designed to write out logging to a log file when using the LogManager
/// </summary>
[Serializable]
public class LogToFile : LogTarget
{
    /// <summary>
    ///     File naming pattern to name the file on disk
    /// </summary>
    protected string DatetimeFileNamePattern { get; set; } = "yyyy-MMMM-dd-dddd";
    /// <summary>
    ///     File path that the logging will be written to
    /// </summary>
    protected string FilePath { get; set; } = "Logging/";
    /// <summary>
    ///     Optional name to prepend to the file
    /// </summary>
    protected string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Constructor used to initialize the log target
    /// </summary>
    public LogToFile()
        : base(true, true)
    {
    }
    /// <summary>
    ///     Copy constructor for the log target that can allow for the developer to setup the log target via the editor
    ///     inspector
    /// </summary>
    /// <param name="toCopy">The copy created by Unity within the editor inspector</param>
    public LogToFile(LogToFile toCopy)
        : base(toCopy.LogByDefault, toCopy.IncludeCallStack)
    {
        FilePath = !string.IsNullOrEmpty(toCopy.FilePath)
            ? $"{toCopy.FilePath.TrimEnd('/').TrimEnd('\\')}/"
            : "Logging/";

        DatetimeFileNamePattern = !string.IsNullOrEmpty(DatetimeFileNamePattern)
            ? toCopy.DatetimeFileNamePattern
            : "yyyy-MMMM-dd-dddd";

        if(!Directory.Exists(FilePath))
        {
            Directory.CreateDirectory(FilePath);
        }
    }
    /// <summary>
    ///     Constructor used to setup the file path for this log target
    /// </summary>
    /// <param name="filePath">The file path to write the logging out to</param>
    /// <param name="name">Optional name to prepend to the file</param>
    /// <param name="logByDefault">Determines whether the log target will log by default or only when logged to explicitly</param>
    /// <param name="includeCallStack">
    ///     Whether the log target should log should include the call stack as part of its logging
    ///     duties
    /// </param>
    /// <param name="datetimeFileNamePattern">The DateTime pattern to use for naming the log file</param>
    public LogToFile(string filePath, string name, bool logByDefault = true, bool includeCallStack = false, string datetimeFileNamePattern = "yyyy-MMMM-dd-dddd")
        : base(logByDefault, includeCallStack)
    {
        FilePath = !string.IsNullOrEmpty(filePath)
            ? $"{filePath.TrimEnd('/').TrimEnd('\\')}/"
            : "Logging/";

        Name = name;

        DatetimeFileNamePattern = !string.IsNullOrEmpty(datetimeFileNamePattern)
            ? datetimeFileNamePattern
            : "yyyy-MMMM-dd-dddd";

        if(!Directory.Exists(FilePath))
        {
            Directory.CreateDirectory(FilePath);
        }
    }
    /// <summary>
    ///     Cleans up any log files that are at or older than the passed in number of days since creation/last modification
    /// </summary>
    /// <param name="age">Number of days to compare to log files for deletion</param>
    /// <param name="ignoreModificationTime">
    ///     Whether to ignore the modification time as a criteria for detecting stale log
    ///     files
    /// </param>
    public virtual int CleanupFiles(int age, bool ignoreModificationTime)
    {
        DirectoryInfo directory = new DirectoryInfo(FilePath);

        FileInfo[] files = directory.GetFiles();
        Queue<FileInfo> toDelete = new Queue<FileInfo>();

        foreach(FileInfo file in files)
        {
            if(IsOld(file.LastAccessTime, age) || (ignoreModificationTime && IsOld(file.CreationTime, age)))
            {
                toDelete.Enqueue(file);
            }
        }

        int result = toDelete.Count;
        DeleteFiles(toDelete);

        return result;
    }
    /// <summary>
    ///     Function used by the LogManager to log a message out, writes the file out to a .log file
    /// </summary>
    /// <param name="data">The data to be displayed to the log target</param>
    public override void Log(LogInformation data)
    {
        lock(LogLock)
        {
            string filePath = $"{FilePath}/{(!string.IsNullOrEmpty(Name) ? $"{Name}_" : "")}{DateTime.Now.ToString(DatetimeFileNamePattern)}.log";

            try
            {
                using(StreamWriter writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine(data.ToRawString(IncludeCallStack, true));
                }
            }
            catch(Exception)
            {
                LogManager.LogExcluding<LogToFile>($"Error when attempting to write log message {data.ToRawString(false, true)}", nameof(LogToFile));
            }
        }
    }
    /// <summary>
    ///     Helper function used to delete all the files queued for deletion
    /// </summary>
    /// <param name="toDelete">Collection of file info objects to delete</param>
    protected void DeleteFiles(Queue<FileInfo> toDelete)
    {
        while(toDelete.Any())
        {
            toDelete.Dequeue()?.Delete();
        }
    }

    /// <summary>
    ///     Helper function used to check the age of a file
    /// </summary>
    /// <param name="dateToCheck">The date time to check against</param>
    /// <param name="age">The maximum age of a file to keep</param>
    /// <returns>True if the file is older than the set age, otherwise false</returns>
    protected bool IsOld(DateTime dateToCheck, int age)
    {
        return (DateTime.Now - dateToCheck).Days >= age;
    }
}
