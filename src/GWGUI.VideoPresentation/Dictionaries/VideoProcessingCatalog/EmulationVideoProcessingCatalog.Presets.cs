using GWGUI.VideoPresentation.Contracts;
using GWGUI.VideoPresentation.Enums;

namespace GWGUI.VideoPresentation.Dictionaries;

public static partial class EmulationVideoProcessingCatalog
{
    private static IReadOnlyDictionary<EmulationVideoPreset, EmulationVideoProcessingConfiguration> CreatePresets() =>
        new Dictionary<EmulationVideoPreset, EmulationVideoProcessingConfiguration>
        {
            [EmulationVideoPreset.Normal] = new(),
            [EmulationVideoPreset.CrtArcadeColor] = CrtPreset(EmulationVideoSampling.SharpBilinear,
                EmulationCrtColorMode.Color, 35, 20, EmulationCrtMask.ApertureGrille, 45, 8, 8, 40, 50),
            [EmulationVideoPreset.CrtTelevisionColor] = CrtPreset(EmulationVideoSampling.Bilinear,
                EmulationCrtColorMode.Color, 55, 35, EmulationCrtMask.ShadowMask, 35, 18, 15, 25, 60),
            [EmulationVideoPreset.CrtGreen] = CrtPreset(EmulationVideoSampling.SharpBilinear,
                EmulationCrtColorMode.Green, 42, 35, EmulationCrtMask.None, 0, 12, 10, 35, 50),
            [EmulationVideoPreset.CrtAmber] = CrtPreset(EmulationVideoSampling.SharpBilinear,
                EmulationCrtColorMode.Amber, 42, 35, EmulationCrtMask.None, 0, 12, 10, 35, 50),
            [EmulationVideoPreset.CrtWhite] = CrtPreset(EmulationVideoSampling.SharpBilinear,
                EmulationCrtColorMode.White, 38, 25, EmulationCrtMask.None, 0, 10, 8, 30, 50),
            [EmulationVideoPreset.LcdColor] = FixedPixelPreset(EmulationFixedPixelTechnology.Lcd,
                EmulationSubpixelLayout.Rgb, 35, 20, 16, 10, 70, 8),
            [EmulationVideoPreset.LcdMonochrome] = FixedPixelPreset(EmulationFixedPixelTechnology.Lcd,
                EmulationSubpixelLayout.Monochrome, 45, 25, 35, 25, 60, 15),
            [EmulationVideoPreset.LedBacklitLcd] = FixedPixelPreset(
                EmulationFixedPixelTechnology.LedBacklitLcd, EmulationSubpixelLayout.Rgb,
                25, 15, 8, 5, 85, 12),
            [EmulationVideoPreset.Oled] = FixedPixelPreset(EmulationFixedPixelTechnology.Oled,
                EmulationSubpixelLayout.Rgb, 15, 10, 1, 0, null, 100),
            [EmulationVideoPreset.Plasma] = new()
            {
                DisplayTechnology = EmulationVideoDisplayTechnology.Plasma,
                Sampling = EmulationVideoSampling.Bilinear,
                Plasma = new(35, 30, 20, 20, 55, 25, 20, 35)
            },
            [EmulationVideoPreset.Vector] = new()
            {
                DisplayTechnology = EmulationVideoDisplayTechnology.Vector,
                Sampling = EmulationVideoSampling.Bilinear,
                Vector = new(50, 75, 45, 30)
            }
        };

    private static EmulationVideoProcessingConfiguration CrtPreset(EmulationVideoSampling sampling,
        EmulationCrtColorMode color, int beam, int halo, EmulationCrtMask mask, int maskIntensity,
        int curvature, int vignette, int scanlineIntensity, int scanlineThickness) => new()
        {
            DisplayTechnology = EmulationVideoDisplayTechnology.Crt,
            Sampling = sampling,
            Crt = new(ColorMode: color, BeamIntensity: beam, HaloIntensity: halo, Mask: mask,
                MaskIntensity: maskIntensity, HorizontalCurvature: curvature,
                VerticalCurvature: curvature, Vignette: vignette,
                ScanlinesEnabled: true, ScanlineIntensity: scanlineIntensity,
                ScanlineThickness: scanlineThickness)
        };

    private static EmulationVideoProcessingConfiguration FixedPixelPreset(
        EmulationFixedPixelTechnology technology, EmulationSubpixelLayout subpixels, int grid,
        int gap, int responseMilliseconds, int persistence, int? backlight, int? blackDepth) => new()
        {
            DisplayTechnology = EmulationVideoDisplayTechnology.FixedPixel,
            Sampling = EmulationVideoSampling.Nearest,
            FixedPixel = new(technology, subpixels, GridIntensity: grid, PixelGap: gap,
                ResponseTimeMilliseconds: responseMilliseconds, PersistenceIntensity: persistence,
                BacklightIntensity: backlight, BlackDepth: blackDepth)
        };
}
