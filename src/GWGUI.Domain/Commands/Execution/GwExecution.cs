namespace GWGUI.Domain.Commands.Execution;

public enum GwOutputStream { Standard, Error }

public sealed record GwOutputLine(DateTimeOffset Timestamp, GwOutputStream Stream, string Text);

public sealed record GwExecutionResult(int ExitCode, bool WasCancelled, TimeSpan Duration, IReadOnlyList<GwOutputLine> Output)
{
    public bool IsSuccess => ExitCode == 0 && !WasCancelled;
}
