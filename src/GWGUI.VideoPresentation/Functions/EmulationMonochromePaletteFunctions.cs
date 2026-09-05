using GWGUI.VideoPresentation.Enums;

namespace GWGUI.VideoPresentation.Functions;

public static class EmulationMonochromePaletteFunctions
{
    public static EmulationMonochromePalette FromArgb(uint argb)
    {
        var red = (int)((argb >> 16) & 0xff);
        var green = (int)((argb >> 8) & 0xff);
        var blue = (int)(argb & 0xff);
        return Palettes.MinBy(palette => Distance(red, green, blue,
            palette.Red, palette.Green, palette.Blue)).Palette;
    }

    private static int Distance(int red, int green, int blue, int targetRed,
        int targetGreen, int targetBlue)
    {
        var deltaRed = red - targetRed;
        var deltaGreen = green - targetGreen;
        var deltaBlue = blue - targetBlue;
        return deltaRed * deltaRed + deltaGreen * deltaGreen + deltaBlue * deltaBlue;
    }

    private static readonly (EmulationMonochromePalette Palette, int Red, int Green, int Blue)[]
        Palettes =
        [
            (EmulationMonochromePalette.Green, 143, 170, 106),
            (EmulationMonochromePalette.Gray, 199, 204, 196),
            (EmulationMonochromePalette.Amber, 255, 117, 9),
            (EmulationMonochromePalette.Blue, 107, 189, 255),
            (EmulationMonochromePalette.White, 255, 255, 255)
        ];
}
