using System.Numerics;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal readonly record struct CrtVideoShaderParameters(
    Vector4 Display,
    Vector4 Beam,
    Vector4 Optical,
    Vector4 Geometry,
    Vector4 Scanlines,
    Vector4 Pattern,
    Vector4 PatternIntensity)
{
    internal static CrtVideoShaderParameters From(
        EmulationVideoProcessingConfiguration configuration)
    {
        var crt = configuration.Crt;
        var tint = LinearTint(crt);
        return new CrtVideoShaderParameters(
            new Vector4(configuration.DisplayTechnology == EmulationVideoDisplayTechnology.Crt
                ? 1f : 0f, (float)crt.ColorMode, tint.X, tint.Y),
            new Vector4(tint.Z, Ratio(crt.BeamWidth), Ratio(crt.BeamIntensity),
                Ratio(crt.BeamDiffusion)),
            new Vector4(Ratio(crt.HaloIntensity), (float)crt.Mask,
                (float)crt.MaskSubpixels, Ratio(crt.MaskIntensity)),
            new Vector4(Ratio(crt.Curvature), Ratio(crt.Vignette),
                crt.ScanlinesEnabled ? 1f : 0f, (float)crt.ScanlineOrientation),
            new Vector4(Ratio(crt.ScanlineIntensity), Ratio(crt.ScanlineThickness),
                Ratio(crt.ScanlinePhase), Ratio(crt.ScanlineCompensation)),
            new Vector4(crt.PatternEnabled ? 1f : 0f, (float)crt.PatternOrientation,
                Ratio(crt.PatternFrequency), Ratio(crt.PatternPhase)),
            new Vector4(Ratio(crt.PatternIntensity), 0f, 0f, 0f));
    }

    private static Vector3 LinearTint(EmulationCrtVideoConfiguration configuration)
    {
        var argb = configuration.ColorMode switch
        {
            EmulationCrtColorMode.Green => 0xFF66FF66u,
            EmulationCrtColorMode.Amber => 0xFFFFB000u,
            EmulationCrtColorMode.White => 0xFFFFFFFFu,
            EmulationCrtColorMode.Gray => 0xFFB0B0B0u,
            EmulationCrtColorMode.Custom => configuration.CustomColorArgb ?? 0xFFFFFFFFu,
            _ => 0xFFFFFFFFu
        };
        return new Vector3(
            SoftwareEmulationVideoProcessingPipeline.SrgbToLinear(((argb >> 16) & 0xff) / 255f),
            SoftwareEmulationVideoProcessingPipeline.SrgbToLinear(((argb >> 8) & 0xff) / 255f),
            SoftwareEmulationVideoProcessingPipeline.SrgbToLinear((argb & 0xff) / 255f));
    }

    private static float Ratio(int value) => value / 100f;
}
