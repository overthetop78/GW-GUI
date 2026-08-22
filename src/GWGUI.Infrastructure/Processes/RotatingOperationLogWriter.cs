using GWGUI.Domain.Commands;
using GWGUI.Domain.Commands.Execution;
using System.Text;

namespace GWGUI.Infrastructure.Processes;

public interface IOperationLogWriter
{
    Task WriteAsync(GwCommand command, GwExecutionResult result, CancellationToken cancellationToken = default);
}

public sealed class RotatingOperationLogWriter(string directory, long maximumBytes = 5 * 1024 * 1024, int maximumFiles = 10) : IOperationLogWriter
{
    private readonly SemaphoreSlim gate = new(1, 1);

    public async Task WriteAsync(GwCommand command, GwExecutionResult result, CancellationToken cancellationToken = default)
    {
        var entry = Format(command, result);
        var bytes = Encoding.UTF8.GetByteCount(entry);
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            Directory.CreateDirectory(directory);
            var active = Path.Combine(directory, "operations.log");
            if (File.Exists(active) && new FileInfo(active).Length + bytes > maximumBytes) Rotate(active);
            await File.AppendAllTextAsync(active, entry, new UTF8Encoding(false), cancellationToken).ConfigureAwait(false);
        }
        finally { gate.Release(); }
    }

    private void Rotate(string active)
    {
        if (maximumFiles < 2) { File.Delete(active); return; }
        var oldest = Numbered(maximumFiles - 1);
        if (File.Exists(oldest)) File.Delete(oldest);
        for (var index = maximumFiles - 2; index >= 1; index--)
        {
            var source = Numbered(index);
            if (File.Exists(source)) File.Move(source, Numbered(index + 1));
        }
        File.Move(active, Numbered(1));
    }

    private string Numbered(int index) => Path.Combine(directory, $"operations.{index}.log");

    private static string Format(GwCommand command, GwExecutionResult result)
    {
        var builder = new StringBuilder()
            .AppendLine("================================================================================")
            .AppendLine($"{DateTimeOffset.Now:O} | exit={result.ExitCode} | cancelled={result.WasCancelled} | duration={result.Duration:c}")
            .AppendLine(command.ToDisplayString());
        foreach (var line in result.Output) builder.Append('[').Append(line.Timestamp.ToString("O")).Append("] [").Append(line.Stream).Append("] ").AppendLine(line.Text);
        return builder.AppendLine().ToString();
    }
}
