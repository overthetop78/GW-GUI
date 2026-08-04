namespace GWGUI.Domain.Commands;

public enum GwOutputStream { Standard, Error }

public sealed record GwOutputLine(DateTimeOffset Timestamp, GwOutputStream Stream, string Text);

public sealed record GwExecutionResult(int ExitCode, bool WasCancelled, TimeSpan Duration, IReadOnlyList<GwOutputLine> Output)
{
    public bool IsSuccess => ExitCode == 0 && !WasCancelled;
}

public interface IGreaseweazleRunner
{
    bool IsRunning { get; }
    Task<GwExecutionResult> RunAsync(GwCommand command, IProgress<GwOutputLine>? output = null, CancellationToken cancellationToken = default);
}
