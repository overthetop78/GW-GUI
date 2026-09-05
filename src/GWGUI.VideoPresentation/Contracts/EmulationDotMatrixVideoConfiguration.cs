using GWGUI.VideoPresentation.Enums;

namespace GWGUI.VideoPresentation.Contracts;

public sealed record EmulationDotMatrixVideoConfiguration(
    EmulationDotMatrixPalette Palette = EmulationDotMatrixPalette.Green,
    EmulationDotMatrixShape Shape = EmulationDotMatrixShape.Round,
    int DotSize = 55,
    int Contrast = 70,
    int ResponseTimeMilliseconds = 120,
    int CellSize = 25,
    int CellGap = 20,
    int Brightness = 80,
    int HaloIntensity = 15,
    int PersistenceMilliseconds = 0);
