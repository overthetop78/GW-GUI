namespace GWGUI.Tests.Emulation.Video;
[Collection("WPF")]
public class VideoTests(GWGUI.Tests.Application.TestInfrastructure.StaExecutionScenarios sta)
{
    [Fact] public void VideoProcessingNormalizesEnumsIntensitiesAndLegacyPalette() => VideoGeometryScenarios.ProcessingLimits();
    [Fact] public void VideoProcessingAppliesBrightnessBeforeContrast() => VideoGeometryScenarios.ProcessingOrder();
    [Theory] [InlineData(0)] [InlineData(1)] [InlineData(2)]
    public Task PresentationRoutesFramesSettingsAndRendererFailures(int failure) => sta.RunAsync(() => VideoProcessingScenarios.Presentation(failure));
    [Theory] [InlineData(-10,(byte)0,(byte)188)] [InlineData(0,(byte)0,(byte)255)] [InlineData(10,(byte)188,(byte)255)]
    public void SoftwarePipelineAdjustsLinearBrightnessAndPreservesMetadata(int setting,byte dark,byte light) => VideoProcessingScenarios.PipelineBrightness(setting,dark,light);
    [Fact] public void SoftwarePipelineNearestSamplingDuplicatesExpectedPixels() => VideoProcessingScenarios.Resize();
    [Theory] [InlineData(GWGUI.VideoPresentation.Enums.EmulationVideoRenderer.Direct3D11)] [InlineData(GWGUI.VideoPresentation.Enums.EmulationVideoRenderer.OpenGL)] [InlineData(GWGUI.VideoPresentation.Enums.EmulationVideoRenderer.Vulkan)]
    public void GpuRendererRoutingPreservesSourceFrame(GWGUI.VideoPresentation.Enums.EmulationVideoRenderer renderer) => VideoProcessingScenarios.RendererRouting(renderer);
    [Theory] [InlineData(GWGUI.Emulation.Enums.EmulationPixelFormat.Xrgb8888)] [InlineData(GWGUI.Emulation.Enums.EmulationPixelFormat.Rgb565)] [InlineData(GWGUI.Emulation.Enums.EmulationPixelFormat.Rgb1555)]
    public void PackedPixelsAndRowPadding(GWGUI.Emulation.Enums.EmulationPixelFormat format) => VideoProcessingScenarios.Pixels(format);
    [Theory] [InlineData(800, 600, 2, 800, 400)] [InlineData(800, 600, 1, 600, 600)] [InlineData(320, 480, 4d/3, 320, 240)]
    public void AspectRatioFitsAvailableArea(double width, double height, double aspect, double expectedWidth, double expectedHeight) => VideoGeometryScenarios.Fit(width,height,aspect,expectedWidth,expectedHeight);
    [Theory] [InlineData(0, 100, 1)] [InlineData(100, -1, 1)] [InlineData(100, 100, double.NaN)] [InlineData(100, 100, double.PositiveInfinity)] [InlineData(100, 100, 0)]
    public void InvalidGeometryProducesEmptyArea(double width, double height, double aspect) => VideoGeometryScenarios.InvalidFit(width,height,aspect);
    [Fact] public void ImageAdjustmentsClampAndPreserveInput()=>VideoGeometryScenarios.Adjustments();
    [Theory]
    [InlineData(-10,2d)]
    [InlineData(0,1d)]
    [InlineData(10,0.5d)]
    [InlineData(100,0.5d)]
    public void GammaExponentUsesBoundedSetting(int gamma,double expected)=>VideoGeometryScenarios.Gamma(gamma,expected);
    [Fact] public void PaletteSelectionIgnoresAlpha()=>VideoProcessingScenarios.Palettes();
}
