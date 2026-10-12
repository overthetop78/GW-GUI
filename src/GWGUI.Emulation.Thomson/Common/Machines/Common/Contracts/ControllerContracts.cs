using GWGUI.Emulation.Thomson.Common.Constants;
using GWGUI.Emulation.Thomson.Common.Machines.Common.Enums;

namespace GWGUI.Emulation.Thomson.Common.Machines.Common.Contracts;

public sealed record ControllerBinding(int Port, PeripheralCategory Peripheral, string? DeviceId = null, IReadOnlyDictionary<string, string>? Mappings = null, int DeadZonePercent = ControllerConstants.DefaultDeadZonePercent, string? VisualId = null);
