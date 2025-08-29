namespace FluffyVoid.Logging;

/// <summary>
///     Log target designed to write out logging to a master log file as well as a category log file when using the
///     LogManager
/// </summary>
public class LogToFileByCategory : LogToFile
{
    /// <summary>
    ///     Copy Constructor used to initialize the log target
    /// </summary>
    public LogToFileByCategory(LogToFile toCopy)
        : base(toCopy)
    {
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
    public LogToFileByCategory(string filePath, string name, bool logByDefault = true, bool includeCallStack = false, string datetimeFileNamePattern = "yyyy-MMMM-dd-dddd")
        : base(filePath, name, logByDefault, includeCallStack, datetimeFileNamePattern)
    {
    }
    /// <summary>
    ///     Cleans up any log files that are at or older than the passed in number of days since creation/last modification
    /// </summary>
    /// <param name="age">Number of days to compare to log files for deletion</param>
    /// <param name="ignoreModificationTime">
    ///     Whether to ignore the modification time as a criteria for detecting stale log
    ///     files
    /// </param>
    public override int CleanupFiles(int age, bool ignoreModificationTime)
    {
        DirectoryInfo[] directories = new DirectoryInfo(FilePath).GetDirectories();
        Queue<FileInfo> toDelete = new Queue<FileInfo>();

        foreach(DirectoryInfo directory in directories)
        {
            FileInfo[] files = directory.GetFiles();

            foreach(FileInfo file in files)
            {
                if(IsOld(file.LastAccessTime, age) || (ignoreModificationTime && IsOld(file.CreationTime, age)))
                {
                    toDelete.Enqueue(file);
                }
            }
        }

        int result = toDelete.Count;
        DeleteFiles(toDelete);

        result += base.CleanupFiles(age, ignoreModificationTime);

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
            if(!string.IsNullOrEmpty(data.Category))
            {
                string filePath = $"{FilePath}/{data.Category}/{(!string.IsNullOrEmpty(Name) ? $"{Name}_" : "")}{data.Category}_{DateTime.Now.ToString(DatetimeFileNamePattern)}.log";

                string? directoryName = Path.GetDirectoryName(filePath);

                if(!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }

                try
                {
                    using(StreamWriter writer = new StreamWriter(filePath, true))
                    {
                        writer.WriteLine(data.ToRawString(IncludeCallStack, true));
                    }
                }
                catch(Exception)
                {
                    LogManager.LogExcluding<LogToFileByCategory>($"Error when attempting to write log message {data.ToRawString(false, true)}", nameof(LogToFileByCategory));

                    return;
                }
            }
        }

        base.Log(data);
    }
}
