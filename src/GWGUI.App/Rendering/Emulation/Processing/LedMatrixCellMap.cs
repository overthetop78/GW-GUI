namespace GWGUI.App.Rendering.Emulation.Processing;

internal sealed record LedMatrixCellMap(float[] Emission, float[] CoreMask,
    float[] HaloMask);
