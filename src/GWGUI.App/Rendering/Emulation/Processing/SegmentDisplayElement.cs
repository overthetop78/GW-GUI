namespace GWGUI.App.Rendering.Emulation.Processing;

internal readonly record struct SegmentDisplayElement(float StartX, float StartY,
    float EndX, float EndY, bool IsPoint = false);
