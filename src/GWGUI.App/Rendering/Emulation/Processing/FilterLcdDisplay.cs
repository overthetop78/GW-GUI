using GWGUI.Emulation.Contracts;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterLcdDisplay
{
    internal const string Shader = """
        vec3 filterLcdDisplay(vec3 color,float backlight,float blackDepth,float bleed,float light)
        {
            float highlight=clamp((light-.45)/.55,0.0,1.0);
            color+=max(vec3(light)-color,vec3(0))*bleed*.35*highlight;
            float blackFloor=backlight*(.018+(1.0-blackDepth)*.22);
            return vec3(blackFloor)+color*(.06+pow(backlight,.8)*.94)*(1.0-blackFloor);
        }
        """;

    internal static void Apply(float[] colors, int width, int height,
        EmulationFixedPixelVideoConfiguration configuration)
    {
        var backlight = (configuration.BacklightIntensity ?? 65) / 100f;
        var blackDepth = (configuration.BlackDepth ?? 35) / 100f;
        var floor = backlight * (0.018f + (1f - blackDepth) * 0.22f);
        var bleed = configuration.BacklightBleedIntensity / 100f * 0.35f;
        FilterFixedPixelLight.ApplyBacklight(colors, width, height, backlight, floor, bleed, 0.06f, 0.94f);
    }
}
