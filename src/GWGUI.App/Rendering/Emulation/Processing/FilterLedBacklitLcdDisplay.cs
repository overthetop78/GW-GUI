using GWGUI.Emulation.Contracts;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterLedBacklitLcdDisplay
{
    internal const string Shader = """
        vec3 filterLedBacklitLcdDisplay(vec3 color,float backlight,float blackDepth,float bleed,float light)
        {
            color+=max(vec3(light)-color,vec3(0))*bleed*.24;
            float blackFloor=.012+(1.0-blackDepth)*.075;
            return vec3(blackFloor)+color*(.74+backlight*.42)*(1.0-blackFloor);
        }
        """;

    internal static void Apply(float[] colors, int width, int height,
        EmulationFixedPixelVideoConfiguration configuration)
    {
        var backlight = (configuration.BacklightIntensity ?? 80) / 100f;
        var blackDepth = (configuration.BlackDepth ?? 55) / 100f;
        var floor = 0.012f + (1f - blackDepth) * 0.075f;
        var bleed = configuration.BacklightBleedIntensity / 100f * 0.24f;
        FilterFixedPixelLight.ApplyBacklight(colors, width, height, backlight, floor, bleed, 0.74f, 0.42f);
    }
}
