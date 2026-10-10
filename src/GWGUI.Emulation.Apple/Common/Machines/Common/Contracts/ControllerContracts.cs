using GWGUI.Emulation.Apple.Common.Machines.Common.Enums;
using GWGUI.Emulation.Apple.Common.Constants;
namespace GWGUI.Emulation.Apple.Common.Machines.Common.Contracts;

public sealed record ControllerBinding(
    int Port,
    PeripheralCategory Peripheral,
    string? DeviceId = null,
    IReadOnlyDictionary<string, string>? Mappings = null,
    int DeadZonePercent = ControllerConstants.DefaultDeadZonePercent,
    string? VisualId = null);
