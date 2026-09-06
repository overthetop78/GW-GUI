using GWGUI.Domain.Settings.Logging;
using System.Text;

namespace GWGUI.Infrastructure.Processes;

public sealed class ConsoleLogSession(string directory, Func<OperationLogSettings> settingsProvider,
    ILogFileSystem? fileSystem = null, Func<DateTimeOffset>? clock = null)
{
    private readonly ILogFileSystem files = fileSystem ?? new LogFileSystem();
    private readonly Func<DateTimeOffset> now = clock ?? (() => DateTimeOffset.Now);
    private readonly SemaphoreSlim gate = new(1, 1);
    private string? activePath;
    private string? activeAction;

    public async Task BeginAsync(string action, string command)
    {
        activeAction = SafeName(action);
        activePath = Path.Combine(directory, activeAction + ".log");
        await AppendAsync("", false).ConfigureAwait(false);
        await AppendAsync(new string('=', 80), false).ConfigureAwait(false);
        await AppendAsync($"{now():O}", false).ConfigureAwait(false);
        await AppendAsync("> " + command, false).ConfigureAwait(false);
    }

    public Task AppendAsync(string line) => AppendCoreAsync(line + Environment.NewLine, true);
    public Task AppendTextAsync(string text) => AppendCoreAsync(text, true);

    private Task AppendAsync(string line, bool requireActive) => AppendCoreAsync(line + Environment.NewLine, requireActive);

    private async Task AppendCoreAsync(string entry, bool requireActive)
    {
        var settings = settingsProvider().ForAction(activeAction ?? "operation");
        if (!settings.Enabled || activePath is null && requireActive) return;
        if (activePath is null) return;
        await gate.WaitAsync().ConfigureAwait(false);
        try
        {
            files.CreateDirectory(directory);
            var maximumBytes = Math.Max(0L, settings.MaximumKilobytes) * 1024L;
            if (maximumBytes > 0 && files.Exists(activePath) && files.Length(activePath) + Encoding.UTF8.GetByteCount(entry) > maximumBytes)
            {
                if (settings.KeepArchives) Archive(activePath);
                else
                {
                    await TrimOldestLinesAsync(activePath, entry, maximumBytes).ConfigureAwait(false);
                    return;
                }
            }
            if (!files.Exists(activePath) || maximumBytes == 0 || files.Length(activePath) + Encoding.UTF8.GetByteCount(entry) <= maximumBytes)
                await files.AppendAsync(activePath, entry).ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException) { }
        finally { gate.Release(); }
    }

    private async Task TrimOldestLinesAsync(string path, string incoming, long maximumBytes)
    {
        var lines = (await files.ReadLinesAsync(path).ConfigureAwait(false)).Append(incoming.TrimEnd('\r', '\n')).ToArray();
        var retained = new Stack<string>();
        long size = 0;
        for (var index = lines.Length - 1; index >= 0; index--)
        {
            var lineSize = Encoding.UTF8.GetByteCount(lines[index] + Environment.NewLine);
            if (retained.Count > 0 && size + lineSize > maximumBytes) break;
            if (lineSize > maximumBytes && retained.Count == 0) continue;
            retained.Push(lines[index]);
            size += lineSize;
        }
        await files.WriteLinesAsync(path, retained).ConfigureAwait(false);
    }

    private void Archive(string path)
    {
        var folder = Path.GetDirectoryName(path)!;
        var name = Path.GetFileNameWithoutExtension(path);
        var stamp = now().ToString("yyyy-MM-dd_HH-mm-ss");
        var archive = Path.Combine(folder, $"{name}-{stamp}.log");
        for (var suffix = 2; files.Exists(archive); suffix++) archive = Path.Combine(folder, $"{name}-{stamp}-{suffix}.log");
        files.Move(path, archive);
    }

    private static string SafeName(string action)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var value = new string(action.ToLowerInvariant().Select(character => invalid.Contains(character) ? '-' : character).ToArray());
        return string.IsNullOrWhiteSpace(value) ? "operation" : value;
    }
}
