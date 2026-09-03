using GWGUI.Emulation.Enums;

namespace GWGUI.Emulation.Contracts;

public sealed record EmulationLedMatrixVideoConfiguration(
    EmulationLedMatrixColor Color = EmulationLedMatrixColor.Rgb,
    int CellSize = 35,
    int CellGap = 30,
    int Diffusion = 20,
    int Brightness = 75);
