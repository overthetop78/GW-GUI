namespace GWGUI.App.Rendering.Emulation.Processing;

internal static partial class VeldridVideoProcessingShaders
{
    internal static string Fragment(GWGUI.VideoPresentation.Enums.EmulationVideoSampling sampling,
        GWGUI.VideoPresentation.Enums.EmulationVideoDisplayTechnology displayTechnology =
            GWGUI.VideoPresentation.Enums.EmulationVideoDisplayTechnology.Normal)
    {
        var (dependencies, function) = sampling switch
        {
            GWGUI.VideoPresentation.Enums.EmulationVideoSampling.Nearest =>
                (FilterNormal.VeldridShader, "nearestSample"),
            GWGUI.VideoPresentation.Enums.EmulationVideoSampling.Bilinear =>
                (FilterBilinear.VeldridShader, "linearSampleCompact"),
            GWGUI.VideoPresentation.Enums.EmulationVideoSampling.SharpBilinear =>
                (FilterBilinear.VeldridShader + FilterSharpBilinear.VeldridShader,
                    "sharpBilinearSampleCompact"),
            GWGUI.VideoPresentation.Enums.EmulationVideoSampling.Bicubic =>
                (FilterNormal.VeldridShader + FilterBicubic.VeldridShader,
                    "bicubicSampleCompact"),
            GWGUI.VideoPresentation.Enums.EmulationVideoSampling.Xbr =>
                (FilterNormal.VeldridShader + FilterXbr.VeldridShader, "xbrCompactSample"),
            GWGUI.VideoPresentation.Enums.EmulationVideoSampling.Xbrz =>
                (FilterNormal.VeldridShader + FilterXbr.VeldridShader
                    + FilterXbrz.VeldridShader, "xbrzCompactSample"),
            GWGUI.VideoPresentation.Enums.EmulationVideoSampling.Hqx =>
                (FilterNormal.VeldridShader + FilterBilinear.VeldridShader
                    + FilterXbr.VeldridShader + FilterHqx.VeldridShader, "hqxCompactSample"),
            GWGUI.VideoPresentation.Enums.EmulationVideoSampling.Hq2x =>
                (FilterNormal.VeldridShader + FilterBilinear.VeldridShader
                    + FilterXbr.VeldridShader + FilterHqx.VeldridShader
                    + FilterHq2x.VeldridShader, "hq2xSampleCompact"),
            GWGUI.VideoPresentation.Enums.EmulationVideoSampling.Hq3x =>
                (FilterNormal.VeldridShader + FilterBilinear.VeldridShader
                    + FilterXbr.VeldridShader + FilterHqx.VeldridShader
                    + FilterHq3x.VeldridShader, "hq3xSampleCompact"),
            GWGUI.VideoPresentation.Enums.EmulationVideoSampling.Hq4x =>
                (FilterNormal.VeldridShader + FilterBilinear.VeldridShader
                    + FilterXbr.VeldridShader + FilterHqx.VeldridShader
                    + FilterHq4x.VeldridShader, "hq4xSampleCompact"),
            GWGUI.VideoPresentation.Enums.EmulationVideoSampling.TwoXSai =>
                (FilterNormal.VeldridShader + FilterXbr.VeldridShader
                    + FilterTwoXSai.VeldridShader, "twoXSaiSampleCompact"),
            GWGUI.VideoPresentation.Enums.EmulationVideoSampling.SuperTwoXSai =>
                (FilterNormal.VeldridShader + FilterBilinear.VeldridShader
                    + FilterXbr.VeldridShader + FilterTwoXSai.VeldridShader
                    + FilterSuperTwoXSai.VeldridShader, "superTwoXSaiSampleCompact"),
            GWGUI.VideoPresentation.Enums.EmulationVideoSampling.SuperEagle =>
                (FilterNormal.VeldridShader + FilterBilinear.VeldridShader
                    + FilterXbr.VeldridShader + FilterTwoXSai.VeldridShader
                    + FilterSuperEagle.VeldridShader, "superEagleSampleCompact"),
            GWGUI.VideoPresentation.Enums.EmulationVideoSampling.EpxScale2x =>
                (FilterNormal.VeldridShader + FilterXbr.VeldridShader
                    + FilterEpxScale2x.VeldridShader, "epxScale2xSampleCompact"),
            GWGUI.VideoPresentation.Enums.EmulationVideoSampling.Jinc2 =>
                (FilterNormal.VeldridShader + FilterJinc2.VeldridShader,
                    "jinc2SampleCompact"),
            GWGUI.VideoPresentation.Enums.EmulationVideoSampling.Lanczos =>
                (FilterNormal.VeldridShader + FilterLanczos.VeldridShader,
                    "lanczosSampleCompact"),
            GWGUI.VideoPresentation.Enums.EmulationVideoSampling.ScaleFx =>
                (FilterNormal.VeldridShader + FilterXbr.VeldridShader
                    + FilterScaleFx.VeldridShader, "scaleFxCompactSample"),
            GWGUI.VideoPresentation.Enums.EmulationVideoSampling.ScaleNx =>
                (FilterNormal.VeldridShader + FilterXbr.VeldridShader
                    + FilterScaleNx.VeldridShader, "scaleNxCompactSample"),
            GWGUI.VideoPresentation.Enums.EmulationVideoSampling.Sabr =>
                (FilterNormal.VeldridShader + FilterBilinear.VeldridShader
                    + FilterXbr.VeldridShader + FilterSabr.VeldridShader, "sabrCompactSample"),
            _ => throw new ArgumentOutOfRangeException(nameof(sampling), sampling, null)
        };
        return FragmentHeader + $"#define DISPLAY_TECHNOLOGY {(int)displayTechnology}\n" + dependencies
            + $"vec3 sourceColor(vec2 uv){{return {function}(uv);}}\n"
            + VideoBrightnessParameterFunctions.Shader + VideoContrastParameterFunctions.Shader
            + VideoGammaParameterFunctions.Shader + VideoSaturationParameterFunctions.Shader
            + VideoSharpnessParameterFunctions.Shader + FragmentBody;
    }
}
