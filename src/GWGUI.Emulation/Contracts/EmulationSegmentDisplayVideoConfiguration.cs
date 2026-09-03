using GWGUI.Emulation.Enums;

namespace GWGUI.Emulation.Contracts;

public sealed record EmulationSegmentDisplayVideoConfiguration(
    EmulationSegmentDisplayLayout Layout = EmulationSegmentDisplayLayout.Seven,
    EmulationSegmentDisplayColor Color = EmulationSegmentDisplayColor.Red,
    int Thickness = 55,
    int Contrast = 80,
    int Glow = 20,
    int ResponseTimeMilliseconds = 30);
