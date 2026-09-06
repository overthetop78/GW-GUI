using GWGUI.Domain.Commands;
using GWGUI.Domain.Commands.Execution;
using System.Text;

namespace GWGUI.Infrastructure.Processes;

public interface IOperationLogWriter
{
    Task WriteAsync(GwCommand command, GwExecutionResult result, CancellationToken cancellationToken = default);
}

public sealed class RotatingOperationLogWriter(string directory, long maximumBytes = 5 * 1024 * 1024, int maximumFiles = 10,
    ILogFileSystem? fileSystem = null, Func<DateTimeOffset>? clock = null) : IOperationLogWriter
{
    private readonly ILogFileSystem files = fileSystem ?? new LogFileSystem();
    private readonly Func<DateTimeOffset> now = clock ?? (() => DateTimeOffset.Now);
    private readonly SemaphoreSlim gate = new(1, 1);

    public async Task WriteAsync(GwCommand command, GwExecutionResult result, CancellationToken cancellationToken = default)
    {
        var entry = Format(command, result);
        var bytes = Encoding.UTF8.GetByteCount(entry);
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            files.CreateDirectory(directory);
            var active = Path.Combine(directory, "operations.log");
            if (files.Exists(active) && files.Length(active) + bytes > maximumBytes) Rotate(active);
            await files.AppendAsync(active, entry, cancellationToken).ConfigureAwait(false);
        }
        finally { gate.Release(); }
    }

    private void Rotate(string active)
    {
        if (maximumFiles < 2) { files.Delete(active); return; }
        var oldest = Numbered(maximumFiles - 1);
        if (files.Exists(oldest)) files.Delete(oldest);
        for (var index = maximumFiles - 2; index >= 1; index--)
        {
            var source = Numbered(index);
            if (files.Exists(source)) files.Move(source, Numbered(index + 1));
        }
        files.Move(active, Numbered(1));
    }

    private string Numbered(int index) => Path.Combine(directory, $"operations.{index}.log");

    private string Format(GwCommand command, GwExecutionResult result)
    {
        var builder = new StringBuilder()
            .AppendLine("================================================================================")
            .AppendLine($"{now():O} | exit={result.ExitCode} | cancelled={result.WasCancelled} | duration={result.Duration:c}")
            .AppendLine(command.ToDisplayString());
        foreach (var line in result.Output) builder.Append('[').Append(line.Timestamp.ToString("O")).Append("] [").Append(line.Stream).Append("] ").AppendLine(line.Text);
        return builder.AppendLine().ToString();
    }
}
