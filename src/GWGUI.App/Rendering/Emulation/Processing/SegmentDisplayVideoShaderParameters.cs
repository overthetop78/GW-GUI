using System.Numerics;
using GWGUI.Emulation.Contracts;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal readonly record struct SegmentDisplayVideoShaderParameters(
    Vector4 Geometry, Vector4 Shape, Vector4 Emission, Vector4 Optical, Vector4 Temporal)
{
    internal static SegmentDisplayVideoShaderParameters From(
        EmulationVideoProcessingConfiguration configuration, bool hasHistory,
        double elapsedMilliseconds)
    {
        var value = configuration.SegmentDisplay;
        var flags = (value.DecimalPoint ? 1 : 0) | (value.Colon ? 2 : 0);
        return new(
            new((float)value.Layout, value.CellSize / 100f,
                value.HorizontalGap / 100f, value.VerticalGap / 100f),
            new(value.Thickness / 100f, value.SegmentGap / 100f,
                (float)value.EndShape, flags),
            new((float)value.Color, value.Brightness / 100f,
                value.ActivationThreshold / 100f, value.Contrast / 100f),
            new(value.OffSegmentVisibility / 100f, value.BlackDepth / 100f,
                value.Glow / 100f, value.HaloRadius / 100f),
            new(hasHistory ? FilterSegmentDisplayResponse.BlendFactor(
                    value.ResponseTimeMilliseconds, elapsedMilliseconds) : 1f,
                hasHistory ? FilterSegmentDisplayPersistence.Decay(
                    value.PersistenceMilliseconds, elapsedMilliseconds) : 0f,
                hasHistory ? 1f : 0f, 0f));
    }
}
