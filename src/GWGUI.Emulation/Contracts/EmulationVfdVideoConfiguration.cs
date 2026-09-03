using GWGUI.Emulation.Enums;

namespace GWGUI.Emulation.Contracts;

public sealed record EmulationVfdVideoConfiguration(
    EmulationVfdColor Color = EmulationVfdColor.Blue,
    int PhosphorIntensity = 70,
    int HaloIntensity = 25,
    int PersistenceIntensity = 20);
