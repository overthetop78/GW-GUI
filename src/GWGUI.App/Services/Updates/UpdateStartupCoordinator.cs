using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using GWGUI.Updates.Contracts;

namespace GWGUI.App.Services.Updates;

internal sealed class UpdateStartupCoordinator
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();
    private readonly string? _signalPath;
    private readonly string _resultPath;
    private readonly string _workDirectory;
    private int _completionStarted;

    private UpdateStartupCoordinator(string? signalPath, string resultPath, string workDirectory)
    {
        _signalPath = signalPath;
        _resultPath = resultPath;
        _workDirectory = workDirectory;
    }

    internal static UpdateStartupCoordinator? FromArguments(string[] args)
    {
        string? signal = null;
        string? result = null;
        string? work = null;
        for (var index = 0; index < args.Length; index++)
        {
            if (args[index] == "--update-startup-signal" && index + 1 < args.Length) signal = args[++index];
            else if (args[index] == "--update-result" && index + 1 < args.Length) result = args[++index];
            else if (args[index] == "--update-work" && index + 1 < args.Length) work = args[++index];
        }
        if (result is null || work is null) return null;
        var expectedRoot = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "GW GUI", "Updates"));
        var workDirectory = Path.GetFullPath(work).TrimEnd(Path.DirectorySeparatorChar);
        if (!workDirectory.StartsWith(expectedRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) return null;
        return new(signal is null ? null : EnsureInside(workDirectory, signal),
            EnsureInside(workDirectory, result), workDirectory);
    }

    internal async Task<UpdateTransactionResult?> CompleteStartupAsync(CancellationToken cancellationToken = default)
    {
        if (Interlocked.Exchange(ref _completionStarted, 1) != 0)
            throw new InvalidOperationException("Update startup can only be completed once.");
        if (_signalPath is not null)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_signalPath)!);
            await using var signal = new FileStream(_signalPath, FileMode.CreateNew, FileAccess.Write, FileShare.Read,
                4096, FileOptions.Asynchronous);
            await using var writer = new StreamWriter(signal);
            await writer.WriteAsync("ready".AsMemory(), cancellationToken);
        }
        var deadline = DateTime.UtcNow.AddSeconds(35);
        while (!File.Exists(_resultPath) && DateTime.UtcNow < deadline)
            await Task.Delay(100, cancellationToken);
        if (!File.Exists(_resultPath)) return null;
        var result = JsonSerializer.Deserialize<UpdateTransactionResult>(
            await File.ReadAllTextAsync(_resultPath, cancellationToken), JsonOptions);
        _ = CleanupAsync();
        return result;
    }

    private async Task CleanupAsync()
    {
        for (var attempt = 0; attempt < 20; attempt++)
        {
            try
            {
                if (Directory.Exists(_workDirectory)) Directory.Delete(_workDirectory, recursive: true);
                return;
            }
            catch (IOException) { await Task.Delay(250); }
            catch (UnauthorizedAccessException) { await Task.Delay(250); }
        }
    }

    private static string EnsureInside(string root, string value)
    {
        var full = Path.GetFullPath(value);
        if (!full.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("The update startup path is outside its work directory.");
        return full;
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        return options;
    }
}
