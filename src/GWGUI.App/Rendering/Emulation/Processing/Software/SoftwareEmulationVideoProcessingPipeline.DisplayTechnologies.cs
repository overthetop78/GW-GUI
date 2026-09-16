using GWGUI.App.Constants.Rendering.Emulation;
using GWGUI.App.Functions.Rendering.Emulation;
using GWGUI.App.Interfaces.Rendering.Emulation;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal sealed partial class SoftwareEmulationVideoProcessingPipeline : IEmulationVideoProcessingPipeline
{
    private static void ApplyDisplayTechnology(float[] colors, int sourceWidth, int sourceHeight,
        int outputWidth, int outputHeight, long sequence,
        EmulationVideoProcessingConfiguration configuration)
    {
        if (configuration.DisplayTechnology == EmulationVideoDisplayTechnology.FixedPixel)
        {
            FilterFixedPixel.Apply(colors, sourceWidth, sourceHeight,
                outputWidth, outputHeight, configuration.FixedPixel);
            return;
        }
        if (configuration.DisplayTechnology == EmulationVideoDisplayTechnology.Plasma)
        {
            var plasma = configuration.Plasma;
            FilterPlasmaBlackDepth.Apply(colors, plasma.BlackDepth);
            FilterPlasmaGammaResponse.Apply(colors, plasma.GammaResponse);
            FilterPlasmaPhosphorIntensity.Apply(colors, plasma.PhosphorIntensity);
            FilterPlasmaAutomaticBrightnessLimiter.Apply(colors,
                plasma.AutomaticBrightnessLimiter);
            FilterPlasmaCellStructure.Apply(colors, sourceWidth, sourceHeight,
                outputWidth, outputHeight, plasma.CellStructure);
            FilterPlasmaTemporalDithering.Apply(colors, outputWidth, outputHeight,
                sequence, plasma.TemporalDithering);
            FilterPlasmaLightDiffusion.Apply(colors, sourceWidth, sourceHeight,
                outputWidth, outputHeight, plasma.Diffusion);
            return;
        }
        if (configuration.DisplayTechnology == EmulationVideoDisplayTechnology.Vector)
        {
            var vector = configuration.Vector;
            if (vector.LineIntensity > 0)
            {
                var emission = FilterVectorLineDetection.Detect(colors, outputWidth,
                    outputHeight, vector.LineThreshold);
                emission = FilterVectorBeamWidth.Apply(emission, outputWidth, outputHeight,
                    vector.BeamWidth);
                emission = FilterVectorBeamFocus.Apply(emission, outputWidth, outputHeight,
                    vector.BeamFocus);
                FilterVectorLineIntensity.Apply(colors, emission, vector.LineIntensity);
                FilterVectorHalo.Apply(colors, emission, outputWidth, outputHeight,
                    vector.LineIntensity, vector.HaloIntensity, vector.HaloRadius);
                FilterVectorPhosphorColor.Apply(colors, vector.PhosphorColor);
            }
            return;
        }
        if (configuration.DisplayTechnology == EmulationVideoDisplayTechnology.Vfd)
        {
            var vfd = configuration.Vfd;
            var emission = FilterVfdEmissionThreshold.Extract(colors, vfd.EmissionThreshold);
            FilterVfdCellStructure.Apply(emission, outputWidth, outputHeight,
                sourceWidth, sourceHeight, vfd.Structure, vfd.CellSize, vfd.CellGap);
            FilterVfdPhosphorIntensity.Apply(emission, vfd.PhosphorIntensity);
            var halo = FilterVfdHalo.Create(emission, outputWidth, outputHeight,
                sourceWidth, sourceHeight, vfd.HaloRadius, vfd.HaloIntensity);
            FilterVfdGlass.Apply(colors, vfd.GlassDarkening);
            FilterVfdPhosphorColor.Apply(colors, emission, halo, vfd.Color);
            return;
        }
        if (configuration.DisplayTechnology == EmulationVideoDisplayTechnology.LedMatrix)
        {
            var ledMatrix = configuration.LedMatrix;
            var cells = FilterLedMatrixCellStructure.Create(colors, sourceWidth,
                sourceHeight, outputWidth, outputHeight, ledMatrix.CellSize,
                ledMatrix.CellGap, ledMatrix.Shape, ledMatrix.HaloRadius);
            FilterLedMatrixColor.Apply(cells.Emission, ledMatrix.Color);
            FilterLedMatrixBrightness.Apply(cells.Emission, ledMatrix.Brightness);
            FilterLedMatrixBlackDepth.Compose(colors, cells, ledMatrix.Diffusion,
                ledMatrix.BlackDepth);
            return;
        }
        if (configuration.DisplayTechnology == EmulationVideoDisplayTechnology.DotMatrix)
        {
            FilterDotMatrix.Apply(colors, sourceWidth, sourceHeight, outputWidth, outputHeight,
                configuration.DotMatrix);
            return;
        }
        if (configuration.DisplayTechnology == EmulationVideoDisplayTechnology.SegmentDisplay)
        {
            FilterSegmentDisplay.Apply(colors, sourceWidth, sourceHeight,
                outputWidth, outputHeight,
                configuration.SegmentDisplay);
            return;
        }
        if (configuration.DisplayTechnology == EmulationVideoDisplayTechnology.EPaper)
        {
            FilterEPaper.Apply(colors, outputWidth, outputHeight,
                configuration.EPaper);
            return;
        }
        if (configuration.DisplayTechnology == EmulationVideoDisplayTechnology.Projection)
        {
            FilterProjection.Apply(colors, outputWidth, outputHeight,
                configuration.Projection);
            return;
        }
        if (configuration.DisplayTechnology != EmulationVideoDisplayTechnology.Crt) return;

        if (configuration.Crt.ColorMode != EmulationCrtColorMode.Color)
        {
            var tint = CrtTint(configuration.Crt);
            for (var index = 0; index < colors.Length; index += 3)
            {
            var luminance = colors[index] * SoftwareVideoProcessingConstants.RedLuminance
                + colors[index + 1] * SoftwareVideoProcessingConstants.GreenLuminance
                + colors[index + 2] * SoftwareVideoProcessingConstants.BlueLuminance;
                colors[index] = luminance * tint.Red;
                colors[index + 1] = luminance * tint.Green;
                colors[index + 2] = luminance * tint.Blue;
            }
        }
        FilterCrt.Apply(colors, outputWidth, outputHeight, sourceWidth, sourceHeight,
            configuration.Crt);
    }

    private static (float Red, float Green, float Blue) CrtTint(
        EmulationCrtVideoConfiguration configuration)
    {
        var argb = configuration.ColorMode switch
        {
            EmulationCrtColorMode.Green => 0xFF66FF66u,
            EmulationCrtColorMode.Amber => 0xFFFFB000u,
            EmulationCrtColorMode.White => 0xFFFFFFFFu,
            EmulationCrtColorMode.Gray => 0xFFB0B0B0u,
            _ => 0xFFFFFFFFu
        };
        return (
            SrgbToLinear(((argb >> 16) & 0xff)
                / SoftwareVideoProcessingConstants.MaximumColorComponent),
            SrgbToLinear(((argb >> 8) & 0xff)
                / SoftwareVideoProcessingConstants.MaximumColorComponent),
            SrgbToLinear((argb & 0xff)
                / SoftwareVideoProcessingConstants.MaximumColorComponent));
    }

}
