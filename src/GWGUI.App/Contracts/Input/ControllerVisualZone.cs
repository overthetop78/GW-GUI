using GWGUI.App.Enums.Input;
using GWGUI.Emulation.Enums;

namespace GWGUI.App.Contracts.Input;

internal sealed record ControllerVisualZone(
    EmulationControllerVisualControl Control,
    ControllerVisualZoneShape Shape,
    double XPercent,
    double YPercent,
    double WidthPercent,
    double HeightPercent);
