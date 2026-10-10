using GWGUI.Emulation.Apple.Common.Machines.Common.Enums;
using GWGUI.Emulation.Apple.Common.Constants;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Apple.Common.Machines.Common.Contracts;

public sealed record InputConfiguration(
    IReadOnlyDictionary<string, EmulationKey>? KeyboardMappings = null,
    IReadOnlyList<ControllerBinding>? Controllers = null,
    string? MouseDeviceId = null,
    bool CaptureMouse = false,
    EmulationKey ReleaseMouseKey = EmulationKey.Escape,
    IReadOnlyDictionary<string, string>? MouseMappings = null);
