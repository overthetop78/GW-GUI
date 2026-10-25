using GWGUI.Emulation.Enums;

namespace GWGUI.Emulation.Contracts;

public sealed record EmulationControllerOutputBinding(string DeviceId, EmulationControllerMotor Motor);
