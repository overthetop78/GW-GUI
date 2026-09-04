using System.Numerics;
using GWGUI.Emulation.Contracts;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal readonly record struct LedMatrixVideoShaderParameters(
    Vector4 Emission, Vector4 Structure)
{
    internal static LedMatrixVideoShaderParameters From(
        EmulationVideoProcessingConfiguration configuration)
    {
        var value = configuration.LedMatrix;
        return new(
            new((float)value.Color, value.Brightness / 100f,
                value.BlackDepth / 100f, (float)value.Shape),
            new(value.CellSize / 100f, value.CellGap / 100f,
                value.Diffusion / 100f, value.HaloRadius / 100f));
    }
}
