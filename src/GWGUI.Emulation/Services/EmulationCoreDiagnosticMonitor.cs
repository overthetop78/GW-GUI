using System.Threading;

namespace GWGUI.Emulation.Services;

/// <summary>Implémentation interne du premier diagnostic fatal capturé pour une session de cœur.</summary>
internal sealed class EmulationCoreDiagnosticMonitor(IReadOnlyList<string>? startupFailureDiagnostics, Func<string, Exception> createFailure) : IEmulationCoreDiagnosticMonitor
{
    private string? _failure;
    /// <inheritdoc/>
    public string? Failure => Volatile.Read(ref _failure);

    /// <inheritdoc/>
    public void Capture(string message)
    {
        if (Failure is not null || startupFailureDiagnostics is null) return;
        foreach (var diagnostic in startupFailureDiagnostics)
            if (message.Contains(diagnostic, StringComparison.Ordinal))
            {
                Interlocked.CompareExchange(ref _failure, message, null);
                return;
            }
    }

    /// <inheritdoc/>
    public void ThrowIfFailed()
    {
        if (Failure is { } diagnostic) throw createFailure(diagnostic);
    }
}
