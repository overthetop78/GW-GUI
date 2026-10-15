using GWGUI.Emulation.Sony.Emulators.Common.Interop.Exceptions;

namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Services;

internal sealed class CoreDiagnosticMonitor(IReadOnlyList<string>? startupFailureDiagnostics)
{
    private string? _failure;
    internal string? Failure => Volatile.Read(ref _failure);

    internal void Capture(string message)
    {
        if (Failure is not null || startupFailureDiagnostics is null) return;
        foreach (var diagnostic in startupFailureDiagnostics)
            if (message.Contains(diagnostic, StringComparison.Ordinal))
            {
                Interlocked.CompareExchange(ref _failure, message, null);
                return;
            }
    }

    internal void ThrowIfFailed()
    {
        if (Failure is { } diagnostic)
            throw new InvalidOperationException(CoreExceptions.ContentRefused(), new InvalidOperationException(diagnostic));
    }
}
