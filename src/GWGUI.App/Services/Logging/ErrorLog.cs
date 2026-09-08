using GWGUI.App.Services.Storage;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace GWGUI.App.Services.Logging;

public static class ErrorLog
{
    private static readonly object Gate = new();

    public static string? Write(Exception exception, string context, string? directory = null) =>
        WriteEntry(exception.ToString(), context, "errors", directory);

    public static string? WriteInformation(string message, string context, string? directory = null) =>
        WriteEntry(message, context, "information", directory);

    private static string? WriteEntry(string detail, string context, string prefix, string? directory)
    {
        try
        {
            directory ??= StoragePaths.LogsDirectory;
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, $"{prefix}-{DateTime.Now:yyyyMMdd}.log");
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
            lock (Gate) File.AppendAllText(path, entry, new UTF8Encoding(false));
            return path;
        }
        catch { return null; }
    }
}
