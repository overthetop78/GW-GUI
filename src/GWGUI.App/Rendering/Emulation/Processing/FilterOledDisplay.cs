using GWGUI.Emulation.Contracts;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterOledDisplay
{
    internal const string Shader = """
        vec3 filterOledDisplay(vec3 color,float blackDepth)
        {
            float blackFloor=(1.0-blackDepth)*.025;
            return vec3(blackFloor)+(color*color*(3.0-2.0*color))*1.08*(1.0-blackFloor);
        }
        """;

    internal static void Apply(float[] colors, EmulationFixedPixelVideoConfiguration configuration)
    {
        var blackDepth = (configuration.BlackDepth ?? 100) / 100f;
        var floor = (1f - blackDepth) * 0.025f;
        FilterFixedPixelLight.ApplyEmissive(colors, floor, 1.08f);
    }
}
