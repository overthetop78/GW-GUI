using GWGUI.App.Services.Storage;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace GWGUI.App.Services.Logging;

public static class ErrorLog
{
    private static readonly object Gate = new();
    internal static event Action<string>? EntryWritten;

    public static string? Write(Exception exception, string context, string? directory = null) =>
        WriteEntry(exception.ToString(), context, "errors", directory);

    public static string? WriteInformation(string message, string context, string? directory = null) =>
        WriteEntry(message, context, "information", directory);

    private static string? WriteEntry(string detail, string context, string prefix, string? directory)
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
        Publish(entry);
        return path;
    }

    private static void Publish(string entry)
    {
        if (EntryWritten is not { } written) return;
        foreach (Action<string> subscriber in written.GetInvocationList())
        {
            try { subscriber(entry); }
            catch { }
        }
    }
}
