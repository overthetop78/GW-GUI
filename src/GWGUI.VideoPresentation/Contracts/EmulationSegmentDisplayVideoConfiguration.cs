using GWGUI.VideoPresentation.Enums;

namespace GWGUI.VideoPresentation.Contracts;

public sealed record EmulationSegmentDisplayVideoConfiguration(
    EmulationSegmentDisplayLayout Layout = EmulationSegmentDisplayLayout.Seven,
    EmulationSegmentDisplayColor Color = EmulationSegmentDisplayColor.Red,
    int Thickness = 55,
    int Contrast = 80,
    int Glow = 20,
    int ResponseTimeMilliseconds = 30,
    int CellSize = 45,
    int HorizontalGap = 15,
    int VerticalGap = 20,
    int SegmentGap = 12,
    EmulationSegmentEndShape EndShape = EmulationSegmentEndShape.Beveled,
    bool DecimalPoint = false,
    bool Colon = false,
    int Brightness = 85,
    int ActivationThreshold = 45,
    int OffSegmentVisibility = 8,
    int BlackDepth = 100,
    int HaloRadius = 25,
    int PersistenceMilliseconds = 60);
