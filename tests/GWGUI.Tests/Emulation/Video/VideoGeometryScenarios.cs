using GWGUI.VideoPresentation.Contracts;
using GWGUI.VideoPresentation.Functions;
using GWGUI.App.Functions.Rendering.Emulation;
namespace GWGUI.Tests.Emulation.Video;
internal static class VideoGeometryScenarios
{
    public static void ProcessingLimits()
    {
        var input=new EmulationVideoProcessingConfiguration
        {
            DisplayTechnology=(GWGUI.VideoPresentation.Enums.EmulationVideoDisplayTechnology)999,
            Sampling=(GWGUI.VideoPresentation.Enums.EmulationVideoSampling)999,
            Crt=new(HorizontalCurvature:-500,VerticalCurvature:500,BeamWidth:-10,MaskIntensity:200),
            FixedPixel=new(ResponseTimeMilliseconds:5000,GridIntensity:-1,PersistenceIntensity:101,MonochromeColorArgb:0xffffffff,BacklightIntensity:101,BlackDepth:-1),
            Restoration=new(Dedithering:-1,Denoising:101),Temporal=new(GeneralPersistence:101,MotionBlur:-1)
        };
        var output=EmulationVideoProcessingConfigurationFunctions.Normalize(input);
        Assert.Equal(GWGUI.VideoPresentation.Enums.EmulationVideoDisplayTechnology.Normal,output.DisplayTechnology);
        Assert.Equal(GWGUI.VideoPresentation.Enums.EmulationVideoSampling.Nearest,output.Sampling);
        Assert.Equal(-100,output.Crt.HorizontalCurvature); Assert.Equal(100,output.Crt.VerticalCurvature); Assert.Equal(0,output.Crt.BeamWidth); Assert.Equal(100,output.Crt.MaskIntensity);
        Assert.Equal(1000,output.FixedPixel.ResponseTimeMilliseconds); Assert.Equal(0,output.FixedPixel.GridIntensity); Assert.Equal(100,output.FixedPixel.PersistenceIntensity);
        Assert.Null(output.FixedPixel.MonochromeColorArgb); Assert.Equal(GWGUI.VideoPresentation.Enums.EmulationMonochromePalette.White,output.FixedPixel.MonochromePalette);
        Assert.Equal(100,output.FixedPixel.BacklightIntensity); Assert.Equal(0,output.FixedPixel.BlackDepth); Assert.Equal(0,output.Restoration.Dedithering); Assert.Equal(100,output.Restoration.Denoising);
        Assert.Equal(100,output.Temporal.GeneralPersistence); Assert.Equal(0,output.Temporal.MotionBlur);
        Assert.Equal(5000,input.FixedPixel.ResponseTimeMilliseconds); Assert.Equal(output,EmulationVideoProcessingConfigurationFunctions.Normalize(output));
    }
    public static void ProcessingOrder()
    {
        using var pipeline=new GWGUI.App.Rendering.Emulation.Processing.SoftwareEmulationVideoProcessingPipeline();
        var frame=new GWGUI.Emulation.Contracts.VideoFrame(new byte[]{0,0,0,255,255,255,255,255},2,1,8,GWGUI.Emulation.Enums.EmulationPixelFormat.Xrgb8888,2,1,TimeSpan.Zero);
        var processed=pipeline.Process(new(){Adjustments=new(Brightness:5,Contrast:10)},frame,new(2,1),new(2,1));
        // Black becomes .25 linear, then .46 after contrast, which rounds to 181 in sRGB.
        Assert.Equal(new byte[]{181,181,181,255,255,255,255,255},processed.Pixels.ToArray());
    }
    public static void Fit(double width, double height, double aspect, double expectedWidth, double expectedHeight)
    {
        var result = EmulationVideoLayoutFunctions.Fit(width, height, aspect);
        Assert.Equal(expectedWidth, result.Width, 8); Assert.Equal(expectedHeight, result.Height, 8);
        Assert.True(result.Width <= width && result.Height <= height);
    }
    public static void InvalidFit(double width, double height, double aspect) => Assert.True(EmulationVideoLayoutFunctions.Fit(width, height, aspect).IsEmpty);
    public static void Adjustments()
    {
        var original=new EmulationImageAdjustments(-100,100,3,-2,20);
        var result=EmulationImageAdjustmentFunctions.Normalize(original);
        Assert.Equal(new EmulationImageAdjustments(-10,10,3,-2,10),result);
        Assert.Equal(-100,original.Brightness);
        Assert.Equal(new EmulationImageAdjustments(),EmulationImageAdjustmentFunctions.Normalize(null));
    }
    public static void Gamma(int gamma,double expected)=>Assert.Equal(expected,EmulationImageAdjustmentFunctions.GammaExponent(gamma),10);
}
