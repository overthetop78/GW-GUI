using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterFixedPixel
{
    internal static void Apply(float[] colors, int sourceWidth, int sourceHeight,
        int outputWidth, int outputHeight, EmulationFixedPixelVideoConfiguration configuration)
    {
        FilterFixedPixelSubpixels.Apply(colors, sourceWidth, outputWidth, outputHeight,
            configuration.Subpixels, configuration.MonochromePalette);
        FilterFixedPixelGrid.Apply(colors, sourceWidth, sourceHeight, outputWidth, outputHeight,
            configuration.GridIntensity, configuration.PixelGap);

        switch (configuration.Technology)
        {
            case EmulationFixedPixelTechnology.Lcd:
                FilterLcdDisplay.Apply(colors, outputWidth, outputHeight, configuration);
                break;
            case EmulationFixedPixelTechnology.LedBacklitLcd:
                FilterLedBacklitLcdDisplay.Apply(colors, outputWidth, outputHeight, configuration);
                break;
            case EmulationFixedPixelTechnology.Oled:
                FilterOledDisplay.Apply(colors, configuration);
                break;
        }
    }
}
