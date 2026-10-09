using GWGUI.Emulation;

namespace GWGUI.Emulation.Acorn.Common.Machines.Common.Contracts;

public sealed record InputConfiguration(
    IReadOnlyDictionary<string, EmulationKey>? KeyboardMappings = null,
    IReadOnlyList<ControllerBinding>? Controllers = null,
    string? MouseDeviceId = null,
    bool CaptureMouse = false,
    EmulationKey ReleaseMouseKey = EmulationKey.Escape);

