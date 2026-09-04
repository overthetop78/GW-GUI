using System.Numerics;
using GWGUI.Emulation.Contracts;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal readonly record struct VfdVideoShaderParameters(
    Vector4 Display, Vector4 Structure, Vector4 Optical)
{
    internal static VfdVideoShaderParameters From(
        EmulationVideoProcessingConfiguration configuration, bool hasHistory,
        double elapsedMilliseconds)
    {
        var vfd = configuration.Vfd;
        return new(
            new Vector4((float)vfd.Color, vfd.PhosphorIntensity / 100f,
                vfd.EmissionThreshold / 100f, vfd.GlassDarkening / 100f),
            new Vector4((float)vfd.Structure, vfd.CellSize / 100f,
                vfd.CellGap / 100f, vfd.HaloIntensity / 100f),
            new Vector4(vfd.HaloRadius / 100f, vfd.PersistenceMilliseconds,
                (float)elapsedMilliseconds, hasHistory ? 1f : 0f));
    }
}
