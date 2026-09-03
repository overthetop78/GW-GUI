using System.Numerics;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal readonly record struct PlasmaVideoShaderParameters(Vector4 Effect, Vector4 Temporal)
{
    internal static PlasmaVideoShaderParameters From(
        EmulationVideoProcessingConfiguration configuration,
        bool hasHistory = false,
        long sequence = 0)
    {
        var plasma = configuration.Plasma;
        return new PlasmaVideoShaderParameters(
            new Vector4(configuration.DisplayTechnology == EmulationVideoDisplayTechnology.Plasma
                ? 1f : 0f, plasma.CellStructure / 100f, plasma.Diffusion / 100f,
                plasma.TemporalDithering / 100f),
            new Vector4(plasma.PersistenceIntensity / 100f,
                hasHistory ? 1f : 0f, Math.Abs(sequence % 4), 0f));
    }
}
