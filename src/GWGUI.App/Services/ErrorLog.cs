using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace GWGUI.App.Services;

public static class ErrorLog
{
    private static readonly object Gate = new();

    public static string? Write(Exception exception, string context, string? directory = null)
    {
        try
        {
            directory ??= Path.Combine(StoragePaths.DataDirectory, "Logs");
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, $"errors-{DateTime.Now:yyyyMMdd}.log");
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
                .AppendLine(exception.ToString())
                .AppendLine()
                .ToString();
            lock (Gate) File.AppendAllText(path, entry, new UTF8Encoding(false));
            return path;
        }
        catch { return null; }
    }
}
