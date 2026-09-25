namespace GWGUI.Emulation.Atari.Common.Contracts;

internal sealed record EnvironmentExtendedMessage(string Text, uint DurationMilliseconds, uint Priority,
    uint Level, uint Target, uint Type, sbyte Progress);

internal sealed record EnvironmentMessage(string Text, uint Frames);

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
