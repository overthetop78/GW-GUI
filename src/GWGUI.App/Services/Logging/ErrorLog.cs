using GWGUI.App.Services.Storage;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace GWGUI.App.Services.Logging;

public static class ErrorLog
{
    private const string WarningFilePrefix = "warnings";
    private static readonly object Gate = new();
    internal static event Action<string>? EntryWritten;
    internal static event Action<string>? WarningWritten;

    public static string? Write(Exception exception, string context, string? directory = null) =>
        WriteEntry(exception.ToString(), context, "errors", directory, true);

    internal static string? WriteWithoutPublishing(Exception exception, string context, string? directory = null) =>
        WriteEntry(exception.ToString(), context, "errors", directory, false);

    public static string? WriteInformation(string message, string context, string? directory = null) =>
        WriteEntry(message, context, "information", directory, true);

    public static string? WriteWarning(string message, string context, string? directory = null) =>
        WriteEntry(message, context, WarningFilePrefix, directory, true, warning: true);

    private static string? WriteEntry(string detail, string context, string prefix, string? directory,
        bool publish, bool warning = false)
    {
        var assembly = Assembly.GetEntryAssembly();
        var entry = new StringBuilder()
            .AppendLine("================================================================================")
            .AppendLine($"Time: {DateTimeOffset.Now:O}")
            .AppendLine($"Context: {context}")
            .AppendLine($"Application: {assembly?.GetName().Name} {assembly?.GetName().Version}")
            .AppendLine($"Runtime: {Environment.Version}")
            .AppendLine($"OS: {Environment.OSVersion}")
            .AppendLine($"Culture: {CultureInfo.CurrentUICulture.Name}")
            .AppendLine($"Process: {Environment.ProcessPath}")
            .AppendLine(detail)
            .AppendLine()
            .ToString();
        string? path = null;
        try
        {
            directory ??= StoragePaths.LogsDirectory;
            Directory.CreateDirectory(directory);
            path = Path.Combine(directory, $"{prefix}-{DateTime.Now:yyyyMMdd}.log");
            lock (Gate) File.AppendAllText(path, entry, new UTF8Encoding(false));
        }
        catch { path = null; }
        if (publish) Publish(entry, warning ? WarningWritten : EntryWritten);
        return path;
    }

    private static void Publish(string entry, Action<string>? written)
    {
        if (written is null) return;
        foreach (Action<string> subscriber in written.GetInvocationList())
        {
            try { subscriber(entry); }
            catch { }
        }
    }
}
