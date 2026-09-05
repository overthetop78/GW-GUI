using System.Numerics;
using GWGUI.Emulation.Contracts;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal readonly record struct EPaperVideoShaderParameters(
    Vector4 InkAndColor, Vector4 PaperSurface, Vector4 Temporal)
{
    internal static EPaperVideoShaderParameters From(
        EmulationVideoProcessingConfiguration configuration, bool hasHistory,
        double elapsedMilliseconds)
    {
        var value = configuration.EPaper;
        return new(
            new Vector4((float)value.ColorMode, value.Contrast / 100f,
                value.Dithering / 100f, value.ColorSaturation / 100f),
            new Vector4(value.InkDensity / 100f, value.PaperBrightness / 100f,
                value.PaperWarmth / 100f, value.SurfaceTexture / 100f),
            new Vector4(value.EdgeSoftness / 100f,
                FilterEPaperRefreshTime.BlendFactor(elapsedMilliseconds,
                    value.RefreshTimeMilliseconds),
                FilterEPaperGhosting.BlendFactor(value.Ghosting), hasHistory ? 1f : 0f));
    }
}
