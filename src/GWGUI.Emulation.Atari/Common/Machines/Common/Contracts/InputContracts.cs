using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Common.Machines.Common.Contracts;

public sealed record InputConfiguration(
    IReadOnlyDictionary<string, EmulationKey>? KeyboardMappings = null,
    IReadOnlyList<ControllerBinding>? Controllers = null,
    string? MouseDeviceId = null,
    bool CaptureMouse = true,
    EmulationKey ReleaseMouseKey = EmulationKey.Escape);
