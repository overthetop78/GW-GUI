using GWGUI.Emulation.Contracts;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterLedBacklitLcdDisplay
{
    internal const string Shader = """
        vec3 filterLedBacklitLcdDisplay(vec3 color,float backlight,float blackDepth,float bleed,float light)
        {
            float highlight=clamp((light-.45)/.55,0.0,1.0);
            color+=max(vec3(light)-color,vec3(0))*bleed*.58*highlight;
            float blackFloor=backlight*(.008+(1.0-blackDepth)*.12);
            return vec3(blackFloor)+color*(.04+pow(backlight,.8)*.96)*(1.0-blackFloor);
        }
        """;

    internal static void Apply(float[] colors, int width, int height,
        EmulationFixedPixelVideoConfiguration configuration)
    {
        var backlight = (configuration.BacklightIntensity ?? 80) / 100f;
        var blackDepth = (configuration.BlackDepth ?? 55) / 100f;
        var floor = backlight * (0.008f + (1f - blackDepth) * 0.12f);
        var bleed = configuration.BacklightBleedIntensity / 100f * 0.58f;
        FilterFixedPixelLight.ApplyBacklight(colors, width, height, backlight, floor, bleed, 0.04f, 0.96f);
    }
}
