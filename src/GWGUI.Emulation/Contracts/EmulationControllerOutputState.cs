using GWGUI.Emulation.Enums;

namespace GWGUI.Emulation.Contracts;

public sealed record EmulationControllerOutputState(string DeviceId, EmulationControllerMotor Motor, float Strength);
