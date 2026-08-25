namespace GWGUI.Emulation.Atari.Contracts;

internal sealed record AtariStateInputFingerprint(
    IReadOnlyList<KeyValuePair<string, string>> KeyboardMappings,
    IReadOnlyList<AtariStateControllerFingerprint> Controllers,
    string? MouseDeviceId,
    bool CaptureMouse,
    string ReleaseMouseKey);
