using System.Text;
using System.Text.RegularExpressions;

namespace FluffyVoid.Logging;

/// <summary>
///     Data class that builds and stores information used by the LogManager to log information
/// </summary>
[Serializable]
public class LogInformation
{
    /// <summary>
    ///     Event used to inform listeners that this class has been recycled for reuse
    /// </summary>
    public event Action<LogInformation>? Recycled;

    /// <summary>
    ///     Stringbuilder used to efficiently build a message for logging
    /// </summary>
    private StringBuilder _message;
    /// <summary>
    ///     The time that the log message was created
    /// </summary>
    private DateTime _timeStamp;

    /// <summary>
    ///     The callstack information gathered during creation
    /// </summary>
    public string CallStack { get; internal set; }
    /// <summary>
    ///     Custom source declaration for added detail to the log
    /// </summary>
    public string Category { get; internal set; }
    /// <summary>
    ///     Stored exception to associate with the log information
    /// </summary>
    public Exception? Ex { get; internal set; }
    /// <summary>
    ///     The severity level of the log message
    /// </summary>
    public Severity LogSeverity { get; private set; }
    /// <summary>
    ///     Color for the log message to be displayed if the log target supports rich text
    /// </summary>
    public string MessageColor { get; internal set; }

    /// <summary>
    ///     Constructor used to create an empty log information with a severity level of Debug
    /// </summary>
    public LogInformation()
    {
        LogSeverity = Severity.Debug;
        _message = new StringBuilder();
        MessageColor = FindSeverityDefaultColor();
        _timeStamp = DateTime.Now;
        Category = string.Empty;
        CallStack = string.Empty;
        Ex = null;
    }

    /// <summary>
    ///     Appends a string value to the stored message
    /// </summary>
    /// <param name="value">The string to be appended to the message</param>
    public void Append(string value)
    {
        _message.Append(value);
    }
    /// <summary>
    ///     Appends an object to the stored message by calling the default ToString() method of the object
    /// </summary>
    /// <param name="value">The object to be appended to the message</param>
    public void Append(object? value)
    {
        _message.Append(value != null
                            ? value.ToString()
                            : string.Empty);
    }
    /// <summary>
    ///     Appends a string value to the stored message with a desired color for the text passed in
    /// </summary>
    /// <param name="value">The string to be appended to the message</param>
    /// <param name="hexColor">The hex color value to color the passed in string with</param>
    public void AppendColored(string value, string hexColor)
    {
        _message.Append(AddColorTags(value, hexColor));
    }
    /// <summary>
    ///     Appends an object to the stored message by calling the default ToString() method of the object
    /// </summary>
    /// <param name="value">The object to be appended to the message</param>
    /// <param name="hexColor">The hex color value to color the passed in object with</param>
    public void AppendColored(object? value, string hexColor)
    {
        _message.Append(AddColorTags(value != null
                                         ? value.ToString()
                                         : string.Empty,
                                     hexColor));
    }
    /// <summary>
    ///     Appends a newline value to the end of the stored message
    /// </summary>
    public void AppendLine()
    {
        _message.AppendLine();
    }
    /// <summary>
    ///     Appends a string value with a newline value to the stored message
    /// </summary>
    /// <param name="value">The string to be appended to the message</param>
    public void AppendLine(string value)
    {
        _message.AppendLine(value);
    }
    /// <summary>
    ///     Appends an object to the stored message with a newline value by calling the default ToString() method of the object
    /// </summary>
    /// <param name="value">The object to be appended to the message</param>
    public void AppendLine(object? value)
    {
        _message.AppendLine(value != null
                                ? value.ToString()
                                : string.Empty);
    }
    /// <summary>
    ///     Appends a string value with a newline value to the stored message with a desired color for the text passed in
    /// </summary>
    /// <param name="value">The string to be appended to the message</param>
    /// <param name="hexColor">The hex color value to color the passed in object with</param>
    public void AppendLineColored(string value, string hexColor)
    {
        _message.AppendLine(AddColorTags(value, hexColor));
    }
    /// <summary>
    ///     Appends an object with a newline value to the stored message by calling the default ToString() method of the object
    ///     with the desired color for the text
    /// </summary>
    /// <param name="value">The object to be appended to the message</param>
    /// <param name="hexColor">The hex color value to color the passed in object with</param>
    public void AppendLineColored(object? value, string hexColor)
    {
        _message.AppendLine(AddColorTags(value != null
                                             ? value.ToString()
                                             : string.Empty,
                                         hexColor));
    }
    /// <summary>
    ///     Copy constructor used to deep copy from one LogInformation to this one, creates an empty LogInformation is no valid
    ///     LogInformation passed in
    /// </summary>
    /// <param name="toCopy">The log information to copy values from</param>
    public void CopyFrom(LogInformation toCopy)
    {
        LogSeverity = toCopy.LogSeverity;
        MessageColor = toCopy.MessageColor;
        _message = new StringBuilder(toCopy._message.ToString());
        _timeStamp = toCopy._timeStamp;
        CallStack = toCopy.CallStack;
        Category = toCopy.Category;
        Ex = toCopy.Ex;
    }
    /// <summary>
    ///     Constructor used to create an empty log information with a desired information
    /// </summary>
    /// <param name="message">The message to store within the information</param>
    /// <param name="source">The source value to assign to the information</param>
    /// <param name="color">The desired color to use for the general log message</param>
    /// <param name="severity">The desired severity for the log information to be set to</param>
    /// <param name="ex">The exception to assign to the information</param>
    public void Initialize(object? message, string source, string color = "", Severity severity = Severity.Debug, Exception? ex = null)
    {
        LogSeverity = severity;

        _message = new StringBuilder(message != null
                                         ? message.ToString()
                                         : string.Empty);

        MessageColor = !string.IsNullOrEmpty(color)
            ? color
            : FindSeverityDefaultColor();

        _timeStamp = DateTime.Now;
        Category = source;
        Ex = ex;
    }
    /// <summary>
    ///     Recycles the log information for reuse within the LogManager
    /// </summary>
    public virtual void Recycle()
    {
        _message.Clear();
        MessageColor = string.Empty;
        LogSeverity = Severity.Debug;
        CallStack = string.Empty;
        Category = string.Empty;
        Ex = null;

        OnRecycled(this);
    }
    /// <summary>
    ///     Custom ToString that gets the colored version of the text for display to the log target including or excluding the
    ///     callstack
    /// </summary>
    /// <param name="includeCallstack">Whether the string should contain the callstack information or not</param>
    /// <param name="includeFullException">Whether the string should contain the full exception stack information or not</param>
    /// <returns>The colored message string with or without the callstack</returns>
    public string ToColorFormattedString(bool includeCallstack, bool includeFullException)
    {
        StringBuilder result = new StringBuilder();
        result.Append($"<color=#{MessageColor.Replace("#", "")}>");
        result.Append($"[{_timeStamp.ToShortTimeString(),-8} {LogSeverity.ToString(),-9}]{$" ({Category})"}</color> {_message}");

        if(Ex != null)
        {
            result.AppendLine();
            result.Append($"Exception: {Ex.Message}");
        }

        if(includeFullException && Ex != null)
        {
            result.AppendLine();
            result.Append($"Exception Information: {Ex}");
        }

        if(!includeCallstack)
        {
            return result.ToString();
        }

        result.AppendLine();
        result.Append($"Callstack: {CallStack}");

        return result.ToString();
    }
    /// <summary>
    ///     Custom ToString that allows for the option of including or excluding the callstack from a text string without rich
    ///     formatting (I.E. for use in log targets that do not utilize rich text features)
    /// </summary>
    /// <param name="includeCallstack">Whether the string should contain the callstack information or not</param>
    /// <param name="includeFullException">Whether the string should contain the full exception stack information or not</param>
    /// <returns>The raw message string with or without the callstack</returns>
    public string ToRawString(bool includeCallstack, bool includeFullException)
    {
        StringBuilder result = new StringBuilder();
        Regex richTextTagPattern = new Regex("<[^>]*>");
        result.Append($"[{_timeStamp.ToShortTimeString(),-8} {LogSeverity.ToString(),-9}]{$" ({Category})"} {richTextTagPattern.Replace(_message.ToString(), "")}");

        if(Ex != null)
        {
            result.AppendLine();
            result.Append($"Exception: {Ex.Message}");
        }

        if(includeFullException && Ex != null)
        {
            result.AppendLine();
            result.Append($"Exception Information: {Ex}");
        }

        if(!includeCallstack)
        {
            return result.ToString();
        }

        result.AppendLine();
        result.Append($"Callstack: {CallStack}");

        return result.ToString();
    }
    /// <summary>
    ///     Overridden ToString that defaults the LogInformation to return the rich text string with the callstack included
    /// </summary>
    /// <returns>The color formatted string with the callstack included</returns>
    public override string ToString()
    {
        return ToColorFormattedString(true, true);
    }
    /// <summary>
    ///     Overridden ToString that defaults the LogInformation to return the rich text string with the callstack included
    /// </summary>
    /// <param name="includeCallstack">Whether to include the callstack or not</param>
    /// <param name="includeException">Whether to include the exception or not</param>
    /// <returns>The color formatted string with the optional callstack and/or exception data included</returns>
    public string ToString(bool includeCallstack, bool includeException)
    {
        return ToColorFormattedString(includeCallstack, includeException);
    }

    /// <summary>
    ///     Event wrapper for use in derived classes to fire the Recycled event
    /// </summary>
    /// <param name="information">Reference to the LogInformation that was recycled</param>
    protected void OnRecycled(LogInformation information)
    {
        if(Recycled == null)
        {
            LogManager.LogWarning("Attempting to recycle a LogInformation that was not part of the LogManager object pool. Please use LogManager.GetLogInformation to get pre-pooled objects for use.", nameof(LogManager));

            return;
        }

        Recycled.Invoke(information);
    }

    /// <summary>
    ///     Helper function used to surround a section of text with the rich text color markup
    /// </summary>
    /// <param name="value">The text to surround with rich text color markup</param>
    /// <param name="hexColor">The hex color value to set within the markup tags</param>
    /// <returns>The newly built string with color tags applied</returns>
    private string AddColorTags(string value, string hexColor)
    {
        if(string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return !string.IsNullOrWhiteSpace(hexColor)
            ? $"<color=#{hexColor.Replace("#", "")}>{value}</color>"
            : value;
    }
    /// <summary>
    ///     Helper function that finds a default color for messages based on their severity level
    /// </summary>
    /// <returns>The color to use for the message</returns>
    private string FindSeverityDefaultColor()
    {
        string result;

        switch(LogSeverity)
        {
            case Severity.Info:
                result = "008080";

                break;
            case Severity.Verbose:
                result = "008000";

                break;
            case Severity.Warning:
                result = "808000";

                break;
            case Severity.Error:
            case Severity.Critical:
            case Severity.Fatal:
                result = "800000";

                break;
            case Severity.Command:
                result = "000080";

                break;
            case Severity.Exception:
                result = "800080";

                break;
            case Severity.Guard:
                result = "805300";

                break;
            default:
                result = "bfbfbf";

                break;
        }

        return result;
    }
}
