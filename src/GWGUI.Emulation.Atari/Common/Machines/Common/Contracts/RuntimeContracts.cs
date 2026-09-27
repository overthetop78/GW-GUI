namespace GWGUI.Emulation.Atari.Common.Machines.Common.Contracts;

public sealed record ShortcutExecutionContext(
    string QuickStatePath,
    Func<CancellationToken, ValueTask> TogglePowerAsync,
    Func<CancellationToken, ValueTask> ToggleFullscreenAsync,
    Func<CancellationToken, ValueTask> ReleaseMouseAsync,
    Func<CancellationToken, ValueTask> CaptureScreenshotAsync,
    Func<CancellationToken, ValueTask> ToggleFastForwardAsync,
    Func<CancellationToken, ValueTask> InsertMediaAsync,
    Func<CancellationToken, ValueTask> EjectMediaAsync,
    Func<CancellationToken, ValueTask> SelectNextMediaAsync);

public sealed record ShortcutRule(string Action, ShortcutAvailability Availability);
