using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari;

public sealed record AtariInputConfiguration(
    IReadOnlyDictionary<string, EmulationKey>? KeyboardMappings = null,
    IReadOnlyList<AtariControllerBinding>? Controllers = null,
    string? MouseDeviceId = null,
    bool CaptureMouse = true,
    EmulationKey ReleaseMouseKey = EmulationKey.Escape);
