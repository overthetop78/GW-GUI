namespace GWGUI.Emulation.Atari.Emulators.Common.Contracts;

internal sealed record EmulatorPreparedContent(
    MediaConfiguration Configuration,
    string RuntimePath,
    bool NeedsFullPath,
    IReadOnlyDictionary<string, string> RuntimeOptions,
    SessionMedia? SessionMedia = null,
    SessionMedia? BootMedia = null,
    IReadOnlyCollection<string>? ActivityPaths = null,
    bool RequiresDiskControl = false,
    object? State = null);
