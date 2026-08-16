namespace GWGUI.Emulation.Atari;

public enum AtariShortcutAvailability
{
    Available,
    Unavailable
}

public sealed record AtariShortcutRule(string Action, AtariShortcutAvailability Availability);

public sealed record AtariShortcutExecutionContext(
    string QuickStatePath,
    Func<CancellationToken, ValueTask> TogglePowerAsync,
    Func<CancellationToken, ValueTask> ToggleFullscreenAsync,
    Func<CancellationToken, ValueTask> ReleaseMouseAsync,
    Func<CancellationToken, ValueTask> CaptureScreenshotAsync,
    Func<CancellationToken, ValueTask> ToggleFastForwardAsync,
    Func<CancellationToken, ValueTask> InsertMediaAsync,
    Func<CancellationToken, ValueTask> EjectMediaAsync,
    Func<CancellationToken, ValueTask> SelectNextMediaAsync);
