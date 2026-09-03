using GWGUI.Emulation.Enums;

namespace GWGUI.Emulation.Contracts;

public sealed record EmulationDotMatrixVideoConfiguration(
    EmulationDotMatrixPalette Palette = EmulationDotMatrixPalette.Green,
    EmulationDotMatrixShape Shape = EmulationDotMatrixShape.Round,
    int DotSize = 55,
    int Contrast = 70,
    int ResponseTimeMilliseconds = 120);
