using System.Numerics;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal readonly record struct VectorVideoShaderParameters(Vector4 Effect, Vector4 Temporal)
{
    internal static VectorVideoShaderParameters From(
        EmulationVideoProcessingConfiguration configuration,
        bool hasHistory = false)
    {
        var vector = configuration.Vector;
        return new VectorVideoShaderParameters(
            new Vector4(configuration.DisplayTechnology == EmulationVideoDisplayTechnology.Vector
                ? 1f : 0f, vector.LineThreshold / 100f,
                vector.LineIntensity / 100f, vector.HaloIntensity / 100f),
            new Vector4(vector.PersistenceIntensity / 100f,
                hasHistory ? 1f : 0f, 0f, 0f));
    }
}
