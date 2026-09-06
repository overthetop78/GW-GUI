using GWGUI.VideoPresentation.Functions;
using GWGUI.VideoPresentation.Enums;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.App.Functions.Rendering.Emulation;
namespace GWGUI.Tests.Emulation.Video;
internal static class VideoProcessingScenarios
{
    internal sealed class VideoSource : GWGUI.Emulation.Interfaces.IEmulationVideo
    {
        public VideoFrame? LatestFrame { get; private set; }
        public double FramesPerSecond => 50;
        public event EventHandler<VideoFrame>? FrameReady;
        public int Subscribers => FrameReady?.GetInvocationList().Length ?? 0;
        public void Send(VideoFrame frame) { LatestFrame=frame; FrameReady?.Invoke(this,frame); }
        public GWGUI.Emulation.Interfaces.IEmulatedMachine Machine => GWGUI.Tests.Application.TestInfrastructure.ControlledDependencies.Simulate<GWGUI.Emulation.Interfaces.IEmulatedMachine>((method,_) => method.Name=="get_Video" ? this : throw new InvalidOperationException(method.Name));
    }
    private sealed class Surface(EmulationVideoRenderer renderer) : GWGUI.App.Interfaces.Rendering.Emulation.IEmulationVideoSurface
    {
        public System.Windows.FrameworkElement View { get; } = new System.Windows.Controls.Border();
        public System.Windows.Media.Imaging.BitmapSource? Snapshot => null;
        public EmulationVideoRenderer Renderer => renderer;
        public IntPtr InputHandle => IntPtr.Zero;
        public List<GWGUI.VideoPresentation.Contracts.EmulationVideoProcessingConfiguration> Settings = [];
        public List<VideoFrame> Frames = [];
        public bool Fail, Disposed;
        public void SetVideoProcessing(GWGUI.VideoPresentation.Contracts.EmulationVideoProcessingConfiguration configuration) => Settings.Add(configuration);
        public void Present(VideoFrame frame) { if(Fail) throw new IOException("synthetic renderer failure"); Frames.Add(frame); }
        public void Dispose() => Disposed=true;
    }
    public static async Task Presentation(int failure)
    {
        var source=new VideoSource(); var replacement=new VideoSource(); var created=new List<Surface>();
        var view=new GWGUI.App.Views.Controls.Emulation.Machine.MachineView();
        var processing=new GWGUI.VideoPresentation.Contracts.EmulationVideoProcessingConfiguration { Adjustments=new(Brightness:2) };
        GWGUI.App.Interfaces.Rendering.Emulation.IEmulationVideoSurface Create(EmulationVideoRenderer renderer)
        {
            if(failure==1 && renderer!=EmulationVideoRenderer.Wpf) throw new IOException("synthetic factory failure");
            var surface=new Surface(renderer) { Fail=failure==2 && renderer!=EmulationVideoRenderer.Wpf }; created.Add(surface); return surface;
        }
        using var presenter=new GWGUI.App.Presenters.Emulation.Machine.MachineVideoPresenter(view,source.Machine,EmulationVideoRenderer.Direct3D11,processing,Create);
        Assert.Equal(1,source.Subscribers); Assert.Equal(processing,Assert.Single(created[0].Settings));
        var completed=new TaskCompletionSource<VideoFrame>(TaskCreationOptions.RunContinuationsAsynchronously);
        presenter.FramePresented+=(_,frame)=>completed.TrySetResult(frame);
        var frame=new VideoFrame(new byte[]{1,2,3,255},1,1,4,EmulationPixelFormat.Xrgb8888,1,7,TimeSpan.Zero);
        source.Send(frame); Assert.Same(frame,await completed.Task);
        await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.ContextIdle);
        Assert.False(presenter.IsShaderLoading); Assert.Equal(failure==0?EmulationVideoRenderer.Direct3D11:EmulationVideoRenderer.Wpf,presenter.Renderer);
        Assert.Same(frame,Assert.Single(created[^1].Frames));
        if(failure==2) Assert.True(created[0].Disposed);
        var changed=processing with { Adjustments=new(Contrast:3) }; presenter.SetVideoProcessing(changed);
        Assert.Equal(changed,created[^1].Settings[^1]); Assert.Null(await presenter.CaptureSnapshotAsync());
        presenter.SetMachine(replacement.Machine); Assert.Equal(0,source.Subscribers); Assert.Equal(1,replacement.Subscribers);
        source.Send(frame with { Sequence=8 }); Assert.Single(created[^1].Frames);
        presenter.Dispose(); Assert.Equal(0,replacement.Subscribers); Assert.True(created[^1].Disposed);
        replacement.Send(frame); Assert.Single(created[^1].Frames);
    }
    public static void PipelineBrightness(int setting,byte dark,byte light)
    {
        using var pipeline = new GWGUI.App.Rendering.Emulation.Processing.SoftwareEmulationVideoProcessingPipeline();
        byte[] pixels = [0,0,0,255,255,255,255,255];
        var frame = new VideoFrame(pixels,2,1,8,EmulationPixelFormat.Xrgb8888,2,17,TimeSpan.FromSeconds(1));
        var configuration = new GWGUI.VideoPresentation.Contracts.EmulationVideoProcessingConfiguration { Adjustments = new(Brightness:setting) };
        var result = pipeline.Process(configuration,frame,new(2,1),new(2,1));
        Assert.Equal(new byte[]{dark,dark,dark,255,light,light,light,255},result.Pixels.ToArray());
        Assert.Equal(new byte[]{0,0,0,255,255,255,255,255},pixels);
        Assert.Equal(frame.Sequence,result.Sequence); Assert.Equal(frame.Timestamp,result.Timestamp); Assert.Equal(frame.AspectRatio,result.AspectRatio);
        Assert.Same(frame,pipeline.Process(new(),frame,new(2,1),new(2,1)));
        Assert.Throws<ArgumentException>(()=>pipeline.Process(new(),frame,new(1,1),new(2,1)));
        Assert.Throws<ArgumentOutOfRangeException>(()=>pipeline.Process(new(),frame,new(2,1),new(0,1)));
    }
    public static void Resize()
    {
        using var pipeline = new GWGUI.App.Rendering.Emulation.Processing.SoftwareEmulationVideoProcessingPipeline();
        byte[] pixels=[0,0,255,255,0,255,0,255,255,0,0,255,255,255,255,255];
        var frame=new VideoFrame(pixels,2,2,8,EmulationPixelFormat.Xrgb8888,1,7,TimeSpan.Zero);
        var result=pipeline.Process(new(),frame,new(2,2),new(4,4));
        Assert.Equal(4,result.Width); Assert.Equal(4,result.Height); Assert.Equal(16,result.Pitch);
        var red=new byte[]{0,0,255,255};var green=new byte[]{0,255,0,255};var blue=new byte[]{255,0,0,255};var white=new byte[]{255,255,255,255};
        var expected=new[]{red,red,green,green,red,red,green,green,blue,blue,white,white,blue,blue,white,white}.SelectMany(x=>x);
        Assert.Equal(expected,result.Pixels.ToArray()); Assert.Equal(7,result.Sequence);
    }
    public static void RendererRouting(EmulationVideoRenderer renderer)
    {
        using var pipeline=GWGUI.App.Factories.Rendering.Emulation.EmulationVideoProcessingPipelineFactory.Create(renderer);
        Assert.Equal(renderer,pipeline.Renderer);
        Assert.IsType<GWGUI.App.Rendering.Emulation.Processing.PassthroughEmulationVideoProcessingPipeline>(pipeline);
        var frame=new VideoFrame(new byte[]{0,0,255,255},1,1,4,EmulationPixelFormat.Xrgb8888,1,1,TimeSpan.Zero);
        Assert.Same(frame,pipeline.Process(new(),frame,new(1,1),new(2,2)));
        Assert.Throws<ArgumentException>(()=>pipeline.Process(new(),frame,new(2,2),new(2,2)));
        Assert.Throws<ArgumentOutOfRangeException>(()=>pipeline.Process(new(),frame,new(1,1),new(0,0)));
    }
    public static void Pixels(EmulationPixelFormat format)
    {
        byte[] source = format switch {
            EmulationPixelFormat.Xrgb8888 => [0,0,255,0, 0,255,0,0, 91,92,93,94, 255,0,0,0, 255,255,255,0, 81,82,83,84],
            EmulationPixelFormat.Rgb565 => [0,248,224,7,91,92, 31,0,255,255,81,82],
            _ => [0,124,224,3,91,92, 31,0,255,127,81,82]
        };
        var before = source.ToArray();
        var frame = new VideoFrame(source, 2, 2, format == EmulationPixelFormat.Xrgb8888 ? 12 : 6, format, 1, 7, TimeSpan.Zero);
        Assert.Equal(new byte[] { 0,0,255,255, 0,255,0,255, 255,0,0,255, 255,255,255,255 }, EmulationVideoPixelFunctions.ToBgra32(frame));
        Assert.Equal(before, source); Assert.Equal(7, frame.Sequence);
        Assert.Throws<ArgumentOutOfRangeException>(() => EmulationVideoPixelFunctions.ToBgra32(frame with { Pixels = source.AsMemory(0, 1) }));
    }
    public static void Palettes()
    {
        Assert.Equal(EmulationMonochromePalette.White,EmulationMonochromePaletteFunctions.FromArgb(0xffffffff));
        Assert.Equal(EmulationMonochromePalette.Amber,EmulationMonochromePaletteFunctions.FromArgb(0xffff7509));
        Assert.Equal(EmulationMonochromePalette.Blue,EmulationMonochromePaletteFunctions.FromArgb(0x006bbdff));
    }
}
