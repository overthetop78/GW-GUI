using GWGUI.Emulation.Enums;

namespace GWGUI.Emulation.Contracts;

public sealed record EmulationVfdVideoConfiguration(
    EmulationVfdColor Color = EmulationVfdColor.Blue,
    int PhosphorIntensity = 70,
    int EmissionThreshold = 28,
    int GlassDarkening = 75,
    EmulationVfdStructure Structure = EmulationVfdStructure.Graphic,
    int CellSize = 70,
    int CellGap = 20,
    int HaloIntensity = 25,
    int HaloRadius = 25,
    int PersistenceMilliseconds = 20);
