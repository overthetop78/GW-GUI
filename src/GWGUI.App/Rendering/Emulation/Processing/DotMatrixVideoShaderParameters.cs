using System.Numerics;
using GWGUI.Emulation.Contracts;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal readonly record struct DotMatrixVideoShaderParameters(
    Vector4 Geometry, Vector4 Emission, Vector4 Temporal)
{
    internal static DotMatrixVideoShaderParameters From(
        EmulationVideoProcessingConfiguration configuration, bool hasHistory,
        double elapsedMilliseconds)
    {
        var value = configuration.DotMatrix;
        var response = hasHistory
            ? FilterDotMatrixResponse.BlendFactor(value.ResponseTimeMilliseconds,
                elapsedMilliseconds) : 1f;
        var persistence = hasHistory && value.PersistenceMilliseconds > 0
            ? MathF.Exp(-(float)elapsedMilliseconds / value.PersistenceMilliseconds) : 0f;
        return new(
            new((float)value.Palette, (float)value.Shape, value.CellSize / 100f,
                value.DotSize / 100f),
            new(value.CellGap / 100f, value.Contrast / 100f,
                value.Brightness / 100f, value.HaloIntensity / 100f),
            new(response, persistence, hasHistory ? 1f : 0f, 0f));
    }
}
