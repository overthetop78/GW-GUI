using System.Collections.Concurrent;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using GWGUI.App.Interfaces.Rendering.Emulation;
using GWGUI.App.Presenters.Emulation.Machine;
using GWGUI.App.Views.Controls.Emulation.Machine;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Tests.Application.TestInfrastructure;
using GWGUI.Tests.Emulation.Video;
using GWGUI.VideoPresentation.Contracts;
using GWGUI.VideoPresentation.Enums;

namespace GWGUI.Tests.Interface.EmulationViews;

[Collection("WPF")]
public sealed class HiddenMachineRenderingTests(StaExecutionScenarios sta)
{
    [Fact]
    public void TemporalResetDoesNotWeaveAnImageFromBeforeSuspension()
    {
        using var pipeline = new GWGUI.App.Rendering.Emulation.Processing.SoftwareEmulationVideoProcessingPipeline();
        using var fresh = new GWGUI.App.Rendering.Emulation.Processing.SoftwareEmulationVideoProcessingPipeline();
        var settings = new EmulationVideoProcessingConfiguration { Temporal = new(Interlacing: 100) };
        var old = new VideoFrame(new byte[] { 0, 0, 255, 255, 0, 0, 255, 255 },
            1, 2, 4, EmulationPixelFormat.Xrgb8888, 0.5f, 1, TimeSpan.Zero);
        var current = old with { Pixels = new byte[] { 255, 0, 0, 255, 255, 0, 0, 255 },
            Sequence = 100, Timestamp = TimeSpan.FromSeconds(2) };
        pipeline.Process(settings, old, new(1, 2), new(1, 2));
        pipeline.ResetTemporalHistory();
        Assert.Equal(fresh.Process(settings, current, new(1, 2), new(1, 2)).Pixels.ToArray(),
            pipeline.Process(settings, current, new(1, 2), new(1, 2)).Pixels.ToArray());
    }

    private sealed class BlockingPipeline : IEmulationVideoProcessingPipeline
    {
        internal readonly ManualResetEventSlim Entered = new();
        internal readonly ManualResetEventSlim Release = new();
        internal readonly ConcurrentQueue<(long Sequence, int Resets)> Processed = new();
        private int _resets;
        public EmulationVideoRenderer Renderer => EmulationVideoRenderer.Wpf;
        public VideoFrame Process(EmulationVideoProcessingConfiguration configuration, VideoFrame frame,
            EmulationVideoProcessingSize sourceSize, EmulationVideoProcessingSize outputSize)
        {
            if (frame.Sequence == 1) { Entered.Set(); Assert.True(Release.Wait(TimeSpan.FromSeconds(5))); }
            Processed.Enqueue((frame.Sequence, _resets));
            return frame;
        }
        public void ResetTemporalHistory() => _resets++;
        public void Dispose() { }
    }

    [Fact]
    public async Task SoftwareWorkerDropsPendingFrameAndResetsBeforeResuming()
    {
        var pipeline = new BlockingPipeline();
        using var worker = new GWGUI.App.Rendering.Emulation.Processing.SoftwareVideoFrameProcessingWorker(pipeline);
        var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        try
        {
            worker.Submit(new(), Frame(1), new(1, 1), 0, _ => { });
            await Until(() => pipeline.Entered.IsSet);
            worker.Submit(new(), Frame(2), new(1, 1), 0, _ => { });
            worker.Suspend();
            worker.Submit(new(), Frame(3), new(1, 1), 1, result =>
            {
                if (result.Error is not null) completed.TrySetException(result.Error);
                else completed.TrySetResult();
            });
            pipeline.Release.Set();
            await completed.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.Equal(new[] { (1L, 0), (3L, 1) }, pipeline.Processed.ToArray());
        }
        finally { pipeline.Release.Set(); }
    }

    private sealed class Surface(EmulationVideoRenderer renderer) : IEmulationVideoSurface
    {
        public FrameworkElement View { get; } = new Border();
        public System.Windows.Media.Imaging.BitmapSource? Snapshot => null;
        public EmulationVideoRenderer Renderer => renderer;
        public IntPtr InputHandle => IntPtr.Zero;
        public ConcurrentQueue<long> Frames { get; } = new();
        public int Resets;
        public bool Disposed;
        public Action? BeforePresent;
        public void Present(VideoFrame frame) { BeforePresent?.Invoke(); Frames.Enqueue(frame.Sequence); }
        public void SetVideoProcessing(EmulationVideoProcessingConfiguration configuration) { }
        public void ResetHistory() => Interlocked.Increment(ref Resets);
        public void Dispose() => Disposed = true;
    }

    private static VideoFrame Frame(long sequence) => new(new byte[] { 1, 2, 3, 255 },
        1, 1, 4, EmulationPixelFormat.Xrgb8888, 1, sequence, TimeSpan.FromMilliseconds(sequence * 20));
    private static Window Host(object content) => new()
    {
        Content = content, Width = 320, Height = 240, ShowActivated = false,
        ShowInTaskbar = false, Opacity = 0
    };
    private static async Task Drain() => await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
    private static async Task Until(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (!condition() && DateTime.UtcNow < deadline) await Task.Delay(10);
        Assert.True(condition());
    }

    [Theory]
    [InlineData(EmulationVideoRenderer.Wpf)]
    [InlineData(EmulationVideoRenderer.Vulkan)]
    public Task HiddenFramesAreSkippedAndResumeUsesCurrentFrame(EmulationVideoRenderer renderer) => sta.RunAsync(async () =>
    {
        var source = new VideoProcessingScenarios.VideoSource();
        var view = new MachineView();
        var surface = new Surface(renderer);
        using var presenter = new MachineVideoPresenter(view, source.Machine, renderer, createSurface: _ => surface);
        var host = Host(view);
        try
        {
            source.Send(Frame(1));
            await Drain();
            Assert.Empty(surface.Frames);
            host.Show();
            await Until(() => surface.Frames.Contains(1));
            await Drain();
            var initialResets = surface.Resets;
            host.Hide();
            await Drain();
            var count = surface.Frames.Count;
            source.Send(Frame(2));
            source.Send(Frame(3));
            await Drain();
            Assert.Equal(count, surface.Frames.Count);
            Assert.False(surface.Disposed);
            Assert.Equal(1, source.Subscribers);
            host.Show(); // No new frame event: also covers a paused machine.
            await Until(() => surface.Frames.Contains(3));
            Assert.DoesNotContain(2, surface.Frames);
            Assert.Equal(initialResets + 1, surface.Resets);
            Assert.False(presenter.IsShaderLoading);
            presenter.Dispose();
            count = surface.Frames.Count;
            source.Send(Frame(4));
            await Drain();
            Assert.Equal(count, surface.Frames.Count);
            Assert.True(surface.Disposed);
        }
        finally { host.Close(); }
    });

    [Fact]
    public Task FullscreenContainerRemainsActiveAndMinimizingSuspends() => sta.RunAsync(async () =>
    {
        var source = new VideoProcessingScenarios.VideoSource();
        var view = new MachineView();
        var surface = new Surface(EmulationVideoRenderer.Wpf);
        using var presenter = new MachineVideoPresenter(view, source.Machine, surface.Renderer, createSurface: _ => surface);
        var host = Host(view);
        var fullscreenContainer = new Grid();
        var fullscreen = Host(fullscreenContainer);
        try
        {
            host.Show(); await Drain();
            view.DisplayHost.Children.Remove(view.Screen);
            fullscreenContainer.Children.Add(view.Screen);
            presenter.SetDisplayHost(fullscreenContainer);
            host.Hide(); fullscreen.Show(); await Drain();
            source.Send(Frame(1));
            await Until(() => surface.Frames.Contains(1));
            fullscreen.WindowState = WindowState.Minimized; await Drain();
            var count = surface.Frames.Count;
            source.Send(Frame(2)); await Drain();
            Assert.Equal(count, surface.Frames.Count);
            fullscreen.WindowState = WindowState.Normal;
            await Until(() => surface.Frames.Contains(2));
            fullscreenContainer.Children.Remove(view.Screen);
            view.DisplayHost.Children.Add(view.Screen);
            presenter.SetDisplayHost(view.DisplayHost);
            fullscreen.Hide(); host.Show(); await Drain();
            source.Send(Frame(3));
            await Until(() => surface.Frames.Contains(3));
            Assert.False(surface.Disposed);
        }
        finally { fullscreen.Close(); host.Close(); }
    });

    [Fact]
    public Task HidingDoesNotWaitForAnInFlightGpuFrameAndDropsPendingFrame() => sta.RunAsync(async () =>
    {
        var source = new VideoProcessingScenarios.VideoSource();
        var view = new MachineView();
        var surface = new Surface(EmulationVideoRenderer.Vulkan);
        using var entered = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();
        surface.BeforePresent = () => { entered.Set(); Assert.True(release.Wait(TimeSpan.FromSeconds(5))); };
        using var presenter = new MachineVideoPresenter(view, source.Machine, surface.Renderer, createSurface: _ => surface);
        var host = Host(view);
        try
        {
            host.Show(); await Drain();
            source.Send(Frame(1));
            await Until(() => entered.IsSet);
            source.Send(Frame(2));
            host.Hide(); // Must return while the GPU frame is still blocked.
            release.Set();
            await Until(() => surface.Frames.Contains(1));
            await Drain();
            Assert.DoesNotContain(2, surface.Frames);
            surface.BeforePresent = null;
            source.Send(Frame(3));
            host.Show();
            await Until(() => surface.Frames.Contains(3));
            Assert.DoesNotContain(2, surface.Frames);
        }
        finally { release.Set(); host.Close(); }
    });
}
