using GWGUI.Emulation.Contracts;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterLcdDisplay
{
    internal const string Shader = """
        vec3 filterLcdDisplay(vec3 color,float backlight,float blackDepth,float bleed,float light)
        {
            color+=max(vec3(light)-color,vec3(0))*bleed*.10;
            float blackFloor=.035+(1.0-blackDepth)*.13;
            return vec3(blackFloor)+color*(.70+backlight*.35)*(1.0-blackFloor);
        }
        """;

    internal static void Apply(float[] colors, int width, int height,
        EmulationFixedPixelVideoConfiguration configuration)
    {
        var backlight = (configuration.BacklightIntensity ?? 65) / 100f;
        var blackDepth = (configuration.BlackDepth ?? 35) / 100f;
        var floor = 0.035f + (1f - blackDepth) * 0.13f;
        var bleed = configuration.BacklightBleedIntensity / 100f * 0.10f;
        FilterFixedPixelLight.ApplyBacklight(colors, width, height, backlight, floor, bleed, 0.70f, 0.35f);
    }
}
