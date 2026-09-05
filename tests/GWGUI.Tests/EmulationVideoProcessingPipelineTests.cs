using System.Runtime.ExceptionServices;
using System.IO;
using System.Threading;
using GWGUI.App.Factories.Rendering.Emulation;
using GWGUI.App.Functions.Rendering.Emulation;
using GWGUI.App.Interfaces.Rendering.Emulation;
using GWGUI.App.Rendering.Emulation.Surfaces;
using GWGUI.App.Rendering.Emulation.Processing;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Dictionaries;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Functions;
using Veldrid;
using Veldrid.SPIRV;

namespace GWGUI.Tests;

public sealed class EmulationVideoProcessingPipelineTests
{
    [Fact]
    public void RfGpuNoiseIsScaledByTheSelectedIntensity()
    {
        Assert.Contains("float rfNoise=noise*amount", SignalConnectionRf.Shader,
            StringComparison.Ordinal);
        Assert.Contains("rfNoise*.16", SignalConnectionRf.Shader, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("Category", "GpuExhaustive")]
    public void VeldridPortableShadersCompileToSpirv()
    {
        var vertex = SpirvCompilation.CompileGlslToSpirv(
            VeldridVideoProcessingShaders.Vertex, "video.vert", ShaderStages.Vertex,
            GlslCompileOptions.Default);
        Assert.NotEmpty(vertex.SpirvBytes);
        foreach (var sampling in Enum.GetValues<EmulationVideoSampling>())
        {
            var fragment = SpirvCompilation.CompileGlslToSpirv(
                VeldridVideoProcessingShaders.Fragment(sampling,
                    EmulationVideoDisplayTechnology.Normal), $"video-{sampling}.frag",
                ShaderStages.Fragment, GlslCompileOptions.Default);
            Assert.NotEmpty(fragment.SpirvBytes);
        }
    }

    [Fact]
    public void VeldridDisplayTechnologyShadersCompileToSpirv()
    {
        foreach (var displayTechnology in Enum.GetValues<EmulationVideoDisplayTechnology>())
        {
            var fragment = SpirvCompilation.CompileGlslToSpirv(
                VeldridVideoProcessingShaders.Fragment(
                    EmulationVideoSampling.Nearest, displayTechnology),
                $"video-{displayTechnology}.frag", ShaderStages.Fragment,
                GlslCompileOptions.Default);
            Assert.NotEmpty(fragment.SpirvBytes);
        }
    }

    [Fact]
    public void VeldridDirect3D11PresentsFirstFrameWithinTenSeconds()
    {
        RunSta(() =>
        {
            var surface = new VeldridVideoSurface(GraphicsBackend.Direct3D11);
            var window = new System.Windows.Window
            {
                Content = surface,
                Width = 128,
                Height = 128,
                ShowInTaskbar = false,
                ShowActivated = false
            };
            try
            {
                window.Show();
                window.UpdateLayout();
                var frame = new VideoFrame(Enumerable.Repeat((byte)255, 4 * 4 * 4).ToArray(),
                    4, 4, 16, EmulationPixelFormat.Xrgb8888, 1f, 1, TimeSpan.Zero);
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var worker = Task.Factory.StartNew(() => surface.Present(frame),
                    CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                Assert.True(worker.Wait(TimeSpan.FromSeconds(10)),
                    $"The first Direct3D frame took longer than ten seconds ({stopwatch.Elapsed}).");
            }
            finally
            {
                window.Close();
                surface.Dispose();
            }
        });
    }

    [Theory]
    [InlineData(GraphicsBackend.Direct3D11)]
    [InlineData(GraphicsBackend.Vulkan)]
    public void VeldridSpecializedDisplaysBuildPipelinesAndPresentFrames(GraphicsBackend backend)
    {
        RunSta(() =>
        {
            var surface = new VeldridVideoSurface(backend);
            var window = new System.Windows.Window
            {
                Content = surface,
                Width = 128,
                Height = 96,
                ShowInTaskbar = false,
                ShowActivated = false
            };
            try
            {
                window.Show();
                window.UpdateLayout();
                surface.SetVideoProcessing(new EmulationVideoProcessingConfiguration
                {
                    DisplayTechnology = EmulationVideoDisplayTechnology.SegmentDisplay,
                    SegmentDisplay = new EmulationSegmentDisplayVideoConfiguration(
                        Layout: EmulationSegmentDisplayLayout.Sixteen,
                        Color: EmulationSegmentDisplayColor.Green,
                        Thickness: 67, Contrast: 78, Glow: 54,
                        ResponseTimeMilliseconds: 120, CellSize: 56,
                        HorizontalGap: 23, VerticalGap: 31, SegmentGap: 16,
                        EndShape: EmulationSegmentEndShape.Rounded,
                        DecimalPoint: true, Colon: true, Brightness: 91,
                        ActivationThreshold: 38, OffSegmentVisibility: 13,
                        BlackDepth: 96, HaloRadius: 47,
                        PersistenceMilliseconds: 180)
                });
                var bright = new VideoFrame(Enumerable.Repeat((byte)220, 8 * 8 * 4).ToArray(),
                    8, 8, 32, EmulationPixelFormat.Xrgb8888, 1f, 1, TimeSpan.Zero);
                var dark = bright with
                {
                    Pixels = new byte[8 * 8 * 4], Sequence = 2,
                    Timestamp = TimeSpan.FromMilliseconds(16)
                };
                Exception? failure = null;
                var worker = Task.Factory.StartNew(() =>
                {
                    try
                    {
                        surface.Present(bright);
                        surface.Present(dark);
                    }
                    catch (Exception error) { failure = error; }
                }, CancellationToken.None, TaskCreationOptions.LongRunning,
                    TaskScheduler.Default);
                Assert.True(worker.Wait(TimeSpan.FromSeconds(30)),
                    $"{backend} timed out while building the segment-display pipeline.");
                if (failure is not null) ExceptionDispatchInfo.Capture(failure).Throw();

                surface.SetVideoProcessing(new EmulationVideoProcessingConfiguration
                {
                    DisplayTechnology = EmulationVideoDisplayTechnology.EPaper,
                    EPaper = new EmulationEPaperVideoConfiguration(
                        ColorMode: EmulationEPaperColorMode.Color4096,
                        Contrast: 76, Dithering: 41, RefreshTimeMilliseconds: 220,
                        Ghosting: 32, InkDensity: 87, PaperBrightness: 92,
                        PaperWarmth: 39, ColorSaturation: 61,
                        SurfaceTexture: 18, EdgeSoftness: 26)
                });
                failure = null;
                worker = Task.Factory.StartNew(() =>
                {
                    try
                    {
                        surface.Present(bright with { Sequence = 3,
                            Timestamp = TimeSpan.FromMilliseconds(32) });
                        surface.Present(dark with { Sequence = 4,
                            Timestamp = TimeSpan.FromMilliseconds(48) });
                    }
                    catch (Exception error) { failure = error; }
                }, CancellationToken.None, TaskCreationOptions.LongRunning,
                    TaskScheduler.Default);
                Assert.True(worker.Wait(TimeSpan.FromSeconds(30)),
                    $"{backend} timed out while building the electronic-paper pipeline.");
                if (failure is not null) ExceptionDispatchInfo.Capture(failure).Throw();
                surface.SetVideoProcessing(new EmulationVideoProcessingConfiguration
                {
                    DisplayTechnology = EmulationVideoDisplayTechnology.Projection,
                    Projection = new(62, 56, 98, 34, 61, 18, 29)
                });
                worker = Task.Factory.StartNew(() => surface.Present(bright with { Sequence = 5 }),
                    CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                Assert.True(worker.Wait(TimeSpan.FromSeconds(30)),
                    $"{backend} timed out while building the projection pipeline.");
            }
            finally
            {
                window.Close();
                surface.Dispose();
            }
        });
    }

    [Fact]
    public void VeldridDirect3D11PresentsVisiblePixelsFromDedicatedThread()
    {
        RunSta(() =>
        {
            var surface = new VeldridVideoSurface(GraphicsBackend.Direct3D11);
            var window = new System.Windows.Window
            {
                Content = surface,
                Width = 128,
                Height = 128,
                Left = 20,
                Top = 20,
                Topmost = true,
                WindowStyle = System.Windows.WindowStyle.None,
                ShowInTaskbar = false,
                ShowActivated = false
            };
            try
            {
                window.Show();
                window.UpdateLayout();
                var frame = new VideoFrame(Enumerable.Repeat((byte)255, 4 * 4 * 4).ToArray(),
                    4, 4, 16, EmulationPixelFormat.Xrgb8888, 1f, 1, TimeSpan.Zero);
                var worker = Task.Factory.StartNew(() => surface.Present(frame),
                    CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                Assert.True(worker.Wait(TimeSpan.FromSeconds(20)));
                Thread.Sleep(150);
                var center = surface.PointToScreen(new System.Windows.Point(
                    surface.ActualWidth / 2, surface.ActualHeight / 2));
                using var bitmap = new System.Drawing.Bitmap(1, 1);
                using (var graphics = System.Drawing.Graphics.FromImage(bitmap))
                    graphics.CopyFromScreen((int)center.X, (int)center.Y, 0, 0,
                        new System.Drawing.Size(1, 1));
                var pixel = bitmap.GetPixel(0, 0);
                Assert.True(pixel.R > 32 || pixel.G > 32 || pixel.B > 32,
                    $"Direct3D presented a black pixel ({pixel.R}, {pixel.G}, {pixel.B}).");
            }
            finally
            {
                window.Close();
                surface.Dispose();
            }
        });
    }
    [Theory]
    [Trait("Category", "GpuExhaustive")]
    [InlineData(EmulationVideoRenderer.OpenGL)]
    [InlineData(EmulationVideoRenderer.Direct3D11)]
    [InlineData(EmulationVideoRenderer.Vulkan)]
    public void GpuSamplingModesPresentDistinctPixels(EmulationVideoRenderer renderer)
    {
        RunSta(() =>
        {
            using var surface = CreateDeterministicSurface(renderer);
            var window = new System.Windows.Window
            {
                Content = surface.View, Width = 180, Height = 140,
                Left = 20, Top = 20, Topmost = true,
                WindowStyle = System.Windows.WindowStyle.None,
                ShowInTaskbar = false, ShowActivated = false
            };
            using var commands =
                new System.Collections.Concurrent.BlockingCollection<Action>();
            var worker = Task.Factory.StartNew(() =>
            {
                foreach (var command in commands.GetConsumingEnumerable()) command();
            }, CancellationToken.None, TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
            try
            {
                window.Show();
                window.UpdateLayout();
                var frame = AdvancedValidationFrame(16, 16);
                var images = new Dictionary<EmulationVideoSampling, byte[]>();
                foreach (var sampling in EmulationVideoProcessingCatalog.SamplingResourceKeys.Keys)
                {
                    surface.SetVideoProcessing(new EmulationVideoProcessingConfiguration
                    {
                        Sampling = sampling
                    });
                    Exception? presentFailure = null;
                    using var presented = new ManualResetEventSlim();
                    commands.Add(() =>
                    {
                        try { for (var repeat = 0; repeat < 4; repeat++) surface.Present(frame); }
                        catch (Exception error) { presentFailure = error; }
                        finally { presented.Set(); }
                    });
                    Assert.True(presented.Wait(TimeSpan.FromSeconds(60)),
                        renderer + " timed out while presenting " + sampling);
                    if (presentFailure is not null)
                        ExceptionDispatchInfo.Capture(presentFailure).Throw();
                    Thread.Sleep(40);
                    var origin = surface.View.PointToScreen(new System.Windows.Point(0, 0));
                    var width = Math.Max(1, (int)surface.View.ActualWidth);
                    var height = Math.Max(1, (int)surface.View.ActualHeight);
                    using var bitmap = new System.Drawing.Bitmap(width, height);
                    using (var graphics = System.Drawing.Graphics.FromImage(bitmap))
                        graphics.CopyFromScreen((int)origin.X, (int)origin.Y, 0, 0,
                            new System.Drawing.Size(width, height));
                    var bytes = new byte[checked(width * height * 3)];
                    var index = 0;
                    for (var y = 0; y < height; y++)
                    for (var x = 0; x < width; x++)
                    {
                        var pixel = bitmap.GetPixel(x, y);
                        bytes[index++] = pixel.R;
                        bytes[index++] = pixel.G;
                        bytes[index++] = pixel.B;
                    }
                    images[sampling] = bytes;
                }

                var nearest = images[EmulationVideoSampling.Nearest];
                foreach (var (sampling, image) in images)
                {
                    if (sampling == EmulationVideoSampling.Nearest) continue;
                    var changedPixels = 0;
                    long absoluteDifference = 0;
                    for (var index = 0; index < image.Length; index += 3)
                    {
                        var difference = Math.Abs(image[index] - nearest[index])
                            + Math.Abs(image[index + 1] - nearest[index + 1])
                            + Math.Abs(image[index + 2] - nearest[index + 2]);
                        if (difference >= 3) changedPixels++;
                        absoluteDifference += difference;
                    }
                    var pixelCount = image.Length / 3;
                    Assert.True(changedPixels >= Math.Max(8, pixelCount / 1000),
                        $"{renderer} {sampling} changes only {changedPixels}/{pixelCount} pixels from Normal.");
                    Assert.True(absoluteDifference / (double)image.Length >= .02,
                        $"{renderer} {sampling} is visually too close to Normal.");
                }

                var bilinear = images[EmulationVideoSampling.Bilinear];
                var sharp = images[EmulationVideoSampling.SharpBilinear];
                var sharpDifference = bilinear.Zip(sharp,
                    (first, second) => Math.Abs(first - second)).Average();
                Assert.True(sharpDifference >= .1,
                    $"{renderer} Bilinéaire net is too close to Bilinéaire ({sharpDifference:F3}).");

                var pairs = images.SelectMany((first, index) => images.Skip(index + 1)
                    .Select(second => (First: first, Second: second)));
                foreach (var pair in pairs)
                {
                    var changed = pair.First.Value.Zip(pair.Second.Value,
                        (first, second) => Math.Abs(first - second)).Count(value => value >= 2);
                    Assert.True(changed >= 6,
                        $"{renderer} produced equivalent outputs for {pair.First.Key} and {pair.Second.Key}.");
                }
            }
            finally
            {
                commands.CompleteAdding();
                worker.Wait(TimeSpan.FromSeconds(60));
                window.Close();
            }
        });
    }

    [Theory]
    [Trait("Category", "GpuExhaustive")]
    [InlineData(EmulationVideoRenderer.OpenGL)]
    [InlineData(EmulationVideoRenderer.Direct3D11)]
    [InlineData(EmulationVideoRenderer.Vulkan)]
    public void ProjectionOptionsChangeActualGpuPixelsWithoutRebuilding(EmulationVideoRenderer renderer)
    {
        RunSta(() =>
        {
            using var surface = CreateDeterministicSurface(renderer);
            var window = new System.Windows.Window
            {
                Content = surface.View, Width = 180, Height = 140,
                Left = 20, Top = 20, Topmost = true,
                WindowStyle = System.Windows.WindowStyle.None,
                ShowInTaskbar = false, ShowActivated = false
            };
            using var commands = new System.Collections.Concurrent.BlockingCollection<Action>();
            var worker = Task.Factory.StartNew(() =>
            {
                foreach (var command in commands.GetConsumingEnumerable()) command();
            }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            try
            {
                window.Show();
                window.UpdateLayout();
                var neutral = new EmulationProjectionVideoConfiguration(0, 0, 0, 0);
                var variants = new Dictionary<string, EmulationProjectionVideoConfiguration>
                {
                    ["Neutral"] = neutral,
                    ["OpticalBlur"] = neutral with { OpticalBlur = 100 },
                    ["Diffusion"] = neutral with { Diffusion = 100 },
                    ["ScreenTexture"] = neutral with { ScreenTexture = 100 },
                    ["Convergence"] = neutral with { Convergence = 100 },
                    ["LightOutput"] = neutral with { LightOutput = 100 },
                    ["AmbientLight"] = neutral with { AmbientLight = 100 },
                    ["Vignette"] = neutral with { Vignette = 100 }
                };
                var frame = AdvancedValidationFrame(16, 16);
                byte[]? baseline = null;
                foreach (var (name, configuration) in variants)
                {
                    var timer = System.Diagnostics.Stopwatch.StartNew();
                    surface.SetVideoProcessing(new EmulationVideoProcessingConfiguration
                    {
                        DisplayTechnology = EmulationVideoDisplayTechnology.Projection,
                        Projection = configuration
                    });
                    Exception? failure = null;
                    using var presented = new ManualResetEventSlim();
                    commands.Add(() =>
                    {
                        try { for (var repeat = 0; repeat < 4; repeat++) surface.Present(frame); }
                        catch (Exception error) { failure = error; }
                        finally { presented.Set(); }
                    });
                    Assert.True(presented.Wait(TimeSpan.FromSeconds(30)), renderer + " " + name);
                    if (failure is not null) ExceptionDispatchInfo.Capture(failure).Throw();
                    timer.Stop();
                    if (baseline is not null)
                        Assert.True(timer.ElapsedMilliseconds < 1000,
                            $"{renderer} {name} took {timer.ElapsedMilliseconds} ms.");
                    Thread.Sleep(40);
                    var origin = surface.View.PointToScreen(new System.Windows.Point(0, 0));
                    var width = Math.Max(1, (int)surface.View.ActualWidth);
                    var height = Math.Max(1, (int)surface.View.ActualHeight);
                    using var bitmap = new System.Drawing.Bitmap(width, height);
                    using (var graphics = System.Drawing.Graphics.FromImage(bitmap))
                        graphics.CopyFromScreen((int)origin.X, (int)origin.Y, 0, 0,
                            new System.Drawing.Size(width, height));
                    var image = new byte[width * height * 3];
                    var index = 0;
                    for (var y = 0; y < height; y++)
                    for (var x = 0; x < width; x++)
                    {
                        var pixel = bitmap.GetPixel(x, y);
                        image[index++] = pixel.R;
                        image[index++] = pixel.G;
                        image[index++] = pixel.B;
                    }
                    if (baseline is null)
                    {
                        Assert.True(image.Max() - image.Min() > 50, renderer + " has no visible image.");
                        baseline = image;
                    }
                    else
                    {
                        var difference = image.Zip(baseline, (a, b) => Math.Abs(a - b)).Average();
                        Assert.True(difference > .02, $"{renderer} {name} has no visible effect.");
                    }
                }
            }
            finally
            {
                commands.CompleteAdding();
                worker.Wait(TimeSpan.FromSeconds(30));
                window.Close();
            }
        });
    }

    [Fact]
    public void OpenGlProgramBuildsAndPresentsAProcessedFrame()
    {
        RunSta(() =>
        {
            var surface = new OpenGlVideoSurface();
            var window = new System.Windows.Window
            {
                Content = surface,
                Width = 64,
                Height = 64,
                WindowStyle = System.Windows.WindowStyle.None,
                ShowInTaskbar = false,
                ShowActivated = false
            };
            try
            {
                window.Show();
                window.UpdateLayout();
                surface.SetVideoProcessing(new EmulationVideoProcessingConfiguration
                {
                    Sampling = EmulationVideoSampling.Bilinear,
                    Adjustments = new EmulationImageAdjustments(
                        Brightness: 1, Contrast: 2, Gamma: 3, Saturation: 4, Sharpness: 5),
                    DisplayTechnology = EmulationVideoDisplayTechnology.Crt,
                    Crt = new EmulationCrtVideoConfiguration(EmulationCrtColorMode.Amber,
                        BeamWidth: 30, BeamIntensity: 40, BeamDiffusion: 20,
                        HaloIntensity: 25, Mask: EmulationCrtMask.SlotMask,
                        MaskSubpixels: EmulationSubpixelLayout.Bgr, MaskIntensity: 35,
                        HorizontalCurvature: 10, VerticalCurvature: 10, Vignette: 15, ScanlinesEnabled: true,
                        ScanlineIntensity: 40, ScanlineThickness: 50,
                        ScanlineCompensation: 20, PatternEnabled: true,
                        PatternFrequency: 15, PatternPhase: 20, PatternIntensity: 10)
                });
                surface.Present(new VideoFrame(new byte[]
                {
                    0, 0, 0, 0, 60, 20, 100, 0,
                    30, 90, 140, 0, 120, 180, 240, 0
                }, 2, 2, 8, EmulationPixelFormat.Xrgb8888, 1f, 1, TimeSpan.Zero));

                Assert.NotNull(surface.Snapshot);
                Assert.True(surface.Snapshot!.PixelWidth > 0);
                Assert.True(surface.Snapshot.PixelHeight > 0);

                surface.SetVideoProcessing(new EmulationVideoProcessingConfiguration
                {
                    DisplayTechnology = EmulationVideoDisplayTechnology.FixedPixel,
                    FixedPixel = new EmulationFixedPixelVideoConfiguration(
                        Technology: EmulationFixedPixelTechnology.LedBacklitLcd,
                        GridIntensity: 35, PixelGap: 20,
                        ResponseTimeMilliseconds: 100,
                        PersistenceIntensity: 60, BacklightIntensity: 80, BlackDepth: 20)
                });
                surface.Present(new VideoFrame(Enumerable.Repeat((byte)255, 16).ToArray(),
                    2, 2, 8, EmulationPixelFormat.Xrgb8888, 1f, 2, TimeSpan.Zero));
                surface.Present(new VideoFrame(new byte[16], 2, 2, 8,
                    EmulationPixelFormat.Xrgb8888, 1f, 3, TimeSpan.FromMilliseconds(16)));
                Assert.NotNull(surface.Snapshot);
                var temporal = new byte[surface.Snapshot!.PixelWidth
                    * surface.Snapshot.PixelHeight * 4];
                surface.Snapshot.CopyPixels(temporal, surface.Snapshot.PixelWidth * 4, 0);
                Assert.Contains(temporal.Chunk(4), pixel =>
                    pixel[0] > 0 || pixel[1] > 0 || pixel[2] > 0);

                surface.SetVideoProcessing(new EmulationVideoProcessingConfiguration
                {
                    DisplayTechnology = EmulationVideoDisplayTechnology.LedMatrix,
                    LedMatrix = new EmulationLedMatrixVideoConfiguration(
                        EmulationLedMatrixColor.Blue, CellSize: 60, CellGap: 45,
                        Diffusion: 70, Brightness: 90,
                        Shape: EmulationLedMatrixShape.Round,
                        HaloRadius: 55, BlackDepth: 95)
                });
                surface.Present(new VideoFrame(Enumerable.Repeat((byte)180, 16).ToArray(),
                    2, 2, 8, EmulationPixelFormat.Xrgb8888, 1f, 4,
                    TimeSpan.FromMilliseconds(32)));

                surface.SetVideoProcessing(new EmulationVideoProcessingConfiguration
                {
                    DisplayTechnology = EmulationVideoDisplayTechnology.SegmentDisplay,
                    SegmentDisplay = new EmulationSegmentDisplayVideoConfiguration(
                        Layout: EmulationSegmentDisplayLayout.Sixteen,
                        Color: EmulationSegmentDisplayColor.Blue,
                        Thickness: 62, Contrast: 73, Glow: 48,
                        ResponseTimeMilliseconds: 0, CellSize: 58,
                        HorizontalGap: 19, VerticalGap: 27, SegmentGap: 18,
                        EndShape: EmulationSegmentEndShape.Rounded,
                        DecimalPoint: true, Colon: true, Brightness: 92,
                        ActivationThreshold: 37, OffSegmentVisibility: 11,
                        BlackDepth: 94, HaloRadius: 44,
                        PersistenceMilliseconds: 0)
                });
                surface.Present(new VideoFrame(Enumerable.Repeat((byte)180, 16).ToArray(),
                    2, 2, 8, EmulationPixelFormat.Xrgb8888, 1f, 5,
                    TimeSpan.FromMilliseconds(48)));
                Assert.NotNull(surface.Snapshot);

                surface.SetVideoProcessing(new EmulationVideoProcessingConfiguration
                {
                    DisplayTechnology = EmulationVideoDisplayTechnology.EPaper,
                    EPaper = new EmulationEPaperVideoConfiguration(
                        ColorMode: EmulationEPaperColorMode.Color4096,
                        Contrast: 72, Dithering: 34, RefreshTimeMilliseconds: 180,
                        Ghosting: 28, InkDensity: 84, PaperBrightness: 91,
                        PaperWarmth: 42, ColorSaturation: 63,
                        SurfaceTexture: 17, EdgeSoftness: 23)
                });
                surface.Present(new VideoFrame(Enumerable.Repeat((byte)170, 16).ToArray(),
                    2, 2, 8, EmulationPixelFormat.Xrgb8888, 1f, 6,
                    TimeSpan.FromMilliseconds(64)));
                Assert.NotNull(surface.Snapshot);
                surface.SetVideoProcessing(new EmulationVideoProcessingConfiguration
                {
                    DisplayTechnology = EmulationVideoDisplayTechnology.Projection,
                    Projection = new(62, 56, 98, 34, 61, 18, 29)
                });
                surface.Present(new VideoFrame(Enumerable.Repeat((byte)170, 16).ToArray(),
                    2, 2, 8, EmulationPixelFormat.Xrgb8888, 1f, 7,
                    TimeSpan.FromMilliseconds(80)));
            }
            finally
            {
                window.Close();
                surface.Dispose();
            }
        });
    }

    [Fact]
    [Trait("Category", "GpuExhaustive")]
    public void RendererSnapshotsMatchDeterministicCpuImagesAtNeutralAndBounds()
    {
        RunSta(() =>
        {
            var frame = new VideoFrame(new byte[]
            {
                0, 0, 0, 0, 20, 40, 80, 0, 60, 100, 140, 0, 100, 160, 220, 0,
                15, 35, 55, 0, 45, 75, 105, 0, 85, 125, 165, 0, 125, 185, 245, 0,
                30, 60, 90, 0, 70, 110, 150, 0, 110, 170, 210, 0, 150, 210, 250, 0
            }, 4, 3, 16, EmulationPixelFormat.Xrgb8888, 4f / 3f, 12,
                TimeSpan.FromMilliseconds(20));
            var configurations = new[]
            {
                new EmulationVideoProcessingConfiguration(),
                new EmulationVideoProcessingConfiguration
                {
                    Adjustments = new EmulationImageAdjustments(-10, -10, -10, -10, -10)
                },
                new EmulationVideoProcessingConfiguration
                {
                    Adjustments = new EmulationImageAdjustments(10, 10, 10, 10, 10)
                },
                new EmulationVideoProcessingConfiguration
                {
                    Sampling = EmulationVideoSampling.SharpBilinear,
                    DisplayTechnology = EmulationVideoDisplayTechnology.Crt,
                    Crt = new EmulationCrtVideoConfiguration(EmulationCrtColorMode.Green,
                        BeamWidth: 45, BeamIntensity: 35, BeamDiffusion: 20,
                        HaloIntensity: 30, Mask: EmulationCrtMask.ShadowMask,
                        MaskIntensity: 40, HorizontalCurvature: 12, VerticalCurvature: 12, Vignette: 15,
                        ScanlinesEnabled: true, ScanlineIntensity: 35,
                        ScanlineThickness: 50, ScanlinePhase: EmulationScanlinePhase.Quarter,
                        ScanlineCompensation: 20, PatternEnabled: true,
                        PatternFrequency: 18, PatternPhase: 25, PatternIntensity: 12)
                },
                new EmulationVideoProcessingConfiguration
                {
                    DisplayTechnology = EmulationVideoDisplayTechnology.FixedPixel,
                    FixedPixel = new EmulationFixedPixelVideoConfiguration(
                        Technology: EmulationFixedPixelTechnology.LedBacklitLcd,
                        GridIntensity: 35, PixelGap: 20,
                        ResponseTimeMilliseconds: 16, PersistenceIntensity: 10,
                        BacklightIntensity: 80, BlackDepth: 15)
                },
                new EmulationVideoProcessingConfiguration
                {
                    DisplayTechnology = EmulationVideoDisplayTechnology.FixedPixel,
                    FixedPixel = new EmulationFixedPixelVideoConfiguration(
                        Technology: EmulationFixedPixelTechnology.Oled,
                        Subpixels: EmulationSubpixelLayout.Bgr,
                        GridIntensity: 20, PixelGap: 10,
                        ResponseTimeMilliseconds: 1, PersistenceIntensity: 5,
                        BlackDepth: 100)
                },
                new EmulationVideoProcessingConfiguration
                {
                    DisplayTechnology = EmulationVideoDisplayTechnology.Plasma,
                    Sampling = EmulationVideoSampling.Bilinear,
                    Plasma = new EmulationPlasmaVideoConfiguration(
                        CellStructure: 35, Diffusion: 30,
                        TemporalDithering: 20, PersistenceIntensity: 20)
                },
                new EmulationVideoProcessingConfiguration
                {
                    DisplayTechnology = EmulationVideoDisplayTechnology.Plasma,
                    Sampling = EmulationVideoSampling.Bilinear,
                    Plasma = new EmulationPlasmaVideoConfiguration(
                        CellStructure: 60, Diffusion: 50,
                        TemporalDithering: 80, PersistenceIntensity: 70)
                },
                new EmulationVideoProcessingConfiguration
                {
                    DisplayTechnology = EmulationVideoDisplayTechnology.Vector,
                    Sampling = EmulationVideoSampling.Bilinear,
                    Vector = new EmulationVectorVideoConfiguration(
                        LineThreshold: 50, LineIntensity: 75,
                        HaloIntensity: 45, PersistenceIntensity: 30)
                },
                new EmulationVideoProcessingConfiguration
                {
                    DisplayTechnology = EmulationVideoDisplayTechnology.Vector,
                    Sampling = EmulationVideoSampling.Bilinear,
                    Vector = new EmulationVectorVideoConfiguration(
                        LineThreshold: 20, LineIntensity: 90,
                        HaloIntensity: 70, PersistenceIntensity: 80)
                },
                new EmulationVideoProcessingConfiguration
                {
                    DisplayTechnology = EmulationVideoDisplayTechnology.Vfd,
                    Vfd = new EmulationVfdVideoConfiguration(EmulationVfdColor.Blue,
                        PhosphorIntensity: 70, HaloIntensity: 25,
                        PersistenceMilliseconds: 20)
                },
                new EmulationVideoProcessingConfiguration
                {
                    DisplayTechnology = EmulationVideoDisplayTechnology.Vfd,
                    Vfd = new EmulationVfdVideoConfiguration(EmulationVfdColor.Green,
                        PhosphorIntensity: 100, HaloIntensity: 70,
                        PersistenceMilliseconds: 80)
                },
                new EmulationVideoProcessingConfiguration
                {
                    DisplayTechnology = EmulationVideoDisplayTechnology.LedMatrix,
                    LedMatrix = new EmulationLedMatrixVideoConfiguration(
                        EmulationLedMatrixColor.Rgb, CellSize: 35, CellGap: 30,
                        Diffusion: 20, Brightness: 75)
                },
                new EmulationVideoProcessingConfiguration
                {
                    DisplayTechnology = EmulationVideoDisplayTechnology.LedMatrix,
                    LedMatrix = new EmulationLedMatrixVideoConfiguration(
                        EmulationLedMatrixColor.Amber, CellSize: 100, CellGap: 80,
                        Diffusion: 70, Brightness: 100)
                },
                new EmulationVideoProcessingConfiguration
                {
                    DisplayTechnology = EmulationVideoDisplayTechnology.DotMatrix,
                    DotMatrix = new EmulationDotMatrixVideoConfiguration(
                        EmulationDotMatrixPalette.Green, EmulationDotMatrixShape.Round,
                        DotSize: 55, Contrast: 70, ResponseTimeMilliseconds: 0)
                },
                new EmulationVideoProcessingConfiguration
                {
                    DisplayTechnology = EmulationVideoDisplayTechnology.DotMatrix,
                    DotMatrix = new EmulationDotMatrixVideoConfiguration(
                        EmulationDotMatrixPalette.Blue, EmulationDotMatrixShape.Square,
                        DotSize: 100, Contrast: 100, ResponseTimeMilliseconds: 1000)
                },
                new EmulationVideoProcessingConfiguration
                {
                    DisplayTechnology = EmulationVideoDisplayTechnology.SegmentDisplay,
                    SegmentDisplay = new EmulationSegmentDisplayVideoConfiguration(
                        EmulationSegmentDisplayLayout.Seven, EmulationSegmentDisplayColor.Red,
                        Thickness: 55, Contrast: 80, Glow: 20, ResponseTimeMilliseconds: 0)
                },
                new EmulationVideoProcessingConfiguration
                {
                    DisplayTechnology = EmulationVideoDisplayTechnology.SegmentDisplay,
                    SegmentDisplay = new EmulationSegmentDisplayVideoConfiguration(
                        EmulationSegmentDisplayLayout.Sixteen, EmulationSegmentDisplayColor.Blue,
                        Thickness: 100, Contrast: 100, Glow: 80, ResponseTimeMilliseconds: 1000)
                },
                new EmulationVideoProcessingConfiguration
                {
                    DisplayTechnology = EmulationVideoDisplayTechnology.EPaper,
                    EPaper = new EmulationEPaperVideoConfiguration(
                        EmulationEPaperColorMode.Monochrome, Contrast: 70, Dithering: 35,
                        RefreshTimeMilliseconds: 0, Ghosting: 20)
                },
                new EmulationVideoProcessingConfiguration
                {
                    DisplayTechnology = EmulationVideoDisplayTechnology.EPaper,
                    EPaper = new EmulationEPaperVideoConfiguration(
                        EmulationEPaperColorMode.Color4096, Contrast: 100, Dithering: 100,
                        RefreshTimeMilliseconds: 1000, Ghosting: 100)
                },
                new EmulationVideoProcessingConfiguration
                {
                    DisplayTechnology = EmulationVideoDisplayTechnology.Projection,
                    Projection = new EmulationProjectionVideoConfiguration(
                        OpticalBlur: 20, Diffusion: 15, ScreenTexture: 10, Convergence: 5)
                },
                new EmulationVideoProcessingConfiguration
                {
                    DisplayTechnology = EmulationVideoDisplayTechnology.Projection,
                    Projection = new EmulationProjectionVideoConfiguration(
                        OpticalBlur: 100, Diffusion: 100, ScreenTexture: 100, Convergence: 100)
                },
                new EmulationVideoProcessingConfiguration
                {
                    Temporal = new EmulationTemporalVideoConfiguration(GeneralPersistence: 100)
                },
                new EmulationVideoProcessingConfiguration
                {
                    Temporal = new EmulationTemporalVideoConfiguration(MotionBlur: 100)
                },
                new EmulationVideoProcessingConfiguration
                {
                    Temporal = new EmulationTemporalVideoConfiguration(Flicker: 100)
                },
                new EmulationVideoProcessingConfiguration
                {
                    Temporal = new EmulationTemporalVideoConfiguration(Interlacing: 100)
                },
                new EmulationVideoProcessingConfiguration
                {
                    Temporal = new EmulationTemporalVideoConfiguration(BlackFrameInsertion: true)
                },
                new EmulationVideoProcessingConfiguration
                {
                    SignalSimulation = new(EmulationSignalConnection.Composite, 100)
                },
                new EmulationVideoProcessingConfiguration
                {
                    SignalSimulation = new(EmulationSignalConnection.SVideo, 100)
                },
                new EmulationVideoProcessingConfiguration
                {
                    SignalSimulation = new(EmulationSignalConnection.Rf, 100)
                },
                new EmulationVideoProcessingConfiguration
                {
                    SignalSimulation = new(Standard: EmulationSignalStandard.Pal,
                        StandardIntensity: 100)
                },
                new EmulationVideoProcessingConfiguration
                {
                    SignalSimulation = new(Standard: EmulationSignalStandard.Ntsc,
                        StandardIntensity: 100)
                },
                new EmulationVideoProcessingConfiguration
                {
                    Stylistic = new EmulationStylisticVideoConfiguration(Grain: 100)
                },
                new EmulationVideoProcessingConfiguration
                {
                    Stylistic = new EmulationStylisticVideoConfiguration(Vhs: 100)
                },
                new EmulationVideoProcessingConfiguration
                {
                    Stylistic = new EmulationStylisticVideoConfiguration(
                        ChromaticAberration: 100)
                },
                new EmulationVideoProcessingConfiguration
                {
                    Stylistic = new EmulationStylisticVideoConfiguration(Bloom: 100)
                },
                new EmulationVideoProcessingConfiguration
                {
                    Stylistic = new EmulationStylisticVideoConfiguration(Sepia: true)
                }
            }.Concat(Enum.GetValues<EmulationVideoSampling>()
                .Where(sampling => sampling is not EmulationVideoSampling.Xbr
                    and not EmulationVideoSampling.Xbrz
                    and not EmulationVideoSampling.Hqx
                    and not EmulationVideoSampling.ScaleFx
                    and not EmulationVideoSampling.ScaleNx
                    and not EmulationVideoSampling.Sabr).Select(sampling =>
                new EmulationVideoProcessingConfiguration { Sampling = sampling })).ToArray();

            foreach (var renderer in Enum.GetValues<EmulationVideoRenderer>())
            {
                var surface = CreateDeterministicSurface(renderer);
                var window = new System.Windows.Window
                {
                    Content = surface.View,
                    Width = 24,
                    Height = 18,
                    Left = -10000,
                    Top = -10000,
                    WindowStyle = System.Windows.WindowStyle.None,
                    ShowInTaskbar = false,
                    ShowActivated = false
                };
                try
                {
                    window.Show();
                    window.UpdateLayout();
                    using var cpu = new SoftwareEmulationVideoProcessingPipeline();
                    foreach (var configuration in configurations)
                    {
                        surface.SetVideoProcessing(configuration);
                        surface.Present(frame);
                        Assert.NotNull(surface.Snapshot);
                        var snapshot = surface.Snapshot;
                        var actual = new byte[checked(
                            snapshot.PixelWidth * snapshot.PixelHeight * 4)];
                        snapshot.CopyPixels(actual, snapshot.PixelWidth * 4, 0);
                        var expectedFrame = cpu.Process(configuration, frame,
                            new EmulationVideoProcessingSize(frame.Width, frame.Height),
                            new EmulationVideoProcessingSize(
                                snapshot.PixelWidth, snapshot.PixelHeight));
                        var expected = EmulationVideoPixelFunctions.ToBgra32(expectedFrame);

                        Assert.Equal(expected.Length, actual.Length);
                        for (var index = 0; index < expected.Length; index++)
                            Assert.InRange(Math.Abs(expected[index] - actual[index]), 0, 1);
                    }
                }
                finally
                {
                    window.Close();
                    surface.Dispose();
                }
            }
        });
    }

    [Theory]
    [Trait("Category", "GpuExhaustive")]
    [InlineData(EmulationVideoRenderer.Wpf, EmulationVideoSampling.Xbr)]
    [InlineData(EmulationVideoRenderer.OpenGL, EmulationVideoSampling.Xbr)]
    [InlineData(EmulationVideoRenderer.Direct3D11, EmulationVideoSampling.Xbr)]
    [InlineData(EmulationVideoRenderer.Vulkan, EmulationVideoSampling.Xbr)]
    [InlineData(EmulationVideoRenderer.Wpf, EmulationVideoSampling.Xbrz)]
    [InlineData(EmulationVideoRenderer.OpenGL, EmulationVideoSampling.Xbrz)]
    [InlineData(EmulationVideoRenderer.Direct3D11, EmulationVideoSampling.Xbrz)]
    [InlineData(EmulationVideoRenderer.Vulkan, EmulationVideoSampling.Xbrz)]
    [InlineData(EmulationVideoRenderer.Wpf, EmulationVideoSampling.Hqx)]
    [InlineData(EmulationVideoRenderer.OpenGL, EmulationVideoSampling.Hqx)]
    [InlineData(EmulationVideoRenderer.Direct3D11, EmulationVideoSampling.Hqx)]
    [InlineData(EmulationVideoRenderer.Vulkan, EmulationVideoSampling.Hqx)]
    [InlineData(EmulationVideoRenderer.Wpf, EmulationVideoSampling.ScaleFx)]
    [InlineData(EmulationVideoRenderer.OpenGL, EmulationVideoSampling.ScaleFx)]
    [InlineData(EmulationVideoRenderer.Direct3D11, EmulationVideoSampling.ScaleFx)]
    [InlineData(EmulationVideoRenderer.Vulkan, EmulationVideoSampling.ScaleFx)]
    [InlineData(EmulationVideoRenderer.Wpf, EmulationVideoSampling.ScaleNx)]
    [InlineData(EmulationVideoRenderer.OpenGL, EmulationVideoSampling.ScaleNx)]
    [InlineData(EmulationVideoRenderer.Direct3D11, EmulationVideoSampling.ScaleNx)]
    [InlineData(EmulationVideoRenderer.Vulkan, EmulationVideoSampling.ScaleNx)]
    [InlineData(EmulationVideoRenderer.Wpf, EmulationVideoSampling.Sabr)]
    [InlineData(EmulationVideoRenderer.OpenGL, EmulationVideoSampling.Sabr)]
    [InlineData(EmulationVideoRenderer.Direct3D11, EmulationVideoSampling.Sabr)]
    [InlineData(EmulationVideoRenderer.Vulkan, EmulationVideoSampling.Sabr)]
    public void PixelArtScalerMatchesCpuReferenceOnRenderer(
        EmulationVideoRenderer renderer, EmulationVideoSampling sampling)
    {
        RunSta(() =>
        {
            var frame = new VideoFrame(new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 255, 0,
                0, 0, 0, 0, 255, 255, 255, 0, 255, 255, 255, 0,
                255, 255, 255, 0, 255, 255, 255, 0, 255, 255, 255, 0
            }, 3, 3, 12, EmulationPixelFormat.Xrgb8888, 1f, 1, TimeSpan.Zero);
            var configuration = new EmulationVideoProcessingConfiguration
            {
                Sampling = sampling
            };
            using var surface = CreateDeterministicSurface(renderer);
            var window = new System.Windows.Window
            {
                Content = surface.View, Width = 24, Height = 18,
                Left = -10000, Top = -10000,
                WindowStyle = System.Windows.WindowStyle.None,
                ShowInTaskbar = false, ShowActivated = false
            };
            try
            {
                window.Show();
                window.UpdateLayout();
                surface.SetVideoProcessing(configuration);
                surface.Present(frame);
                var snapshot = Assert.IsType<System.Windows.Media.Imaging.WriteableBitmap>(
                    surface.Snapshot);
                var actual = new byte[snapshot.PixelWidth * snapshot.PixelHeight * 4];
                snapshot.CopyPixels(actual, snapshot.PixelWidth * 4, 0);
                using var cpu = new SoftwareEmulationVideoProcessingPipeline();
                var expectedFrame = cpu.Process(configuration, frame, new(3, 3),
                    new(snapshot.PixelWidth, snapshot.PixelHeight));
                var expected = EmulationVideoPixelFunctions.ToBgra32(expectedFrame);
                Assert.Equal(expected.Length, actual.Length);
                for (var index = 0; index < expected.Length; index++)
                    Assert.InRange(Math.Abs(expected[index] - actual[index]), 0, 1);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Theory]
    [Trait("Category", "GpuExhaustive")]
    [InlineData(EmulationVideoRenderer.Wpf)]
    [InlineData(EmulationVideoRenderer.OpenGL)]
    [InlineData(EmulationVideoRenderer.Direct3D11)]
    [InlineData(EmulationVideoRenderer.Vulkan)]
    public void DeditheringMatchesCpuReferenceOnRenderer(EmulationVideoRenderer renderer)
    {
        RunSta(() =>
        {
            var frame = CheckerboardFrame(4, 4, 80, 120);
            var configuration = new EmulationVideoProcessingConfiguration
            {
                Restoration = new EmulationImageRestorationConfiguration(Dedithering: 100)
            };
            using var surface = CreateDeterministicSurface(renderer);
            var window = new System.Windows.Window
            {
                Content = surface.View, Width = 24, Height = 18,
                Left = -10000, Top = -10000,
                WindowStyle = System.Windows.WindowStyle.None,
                ShowInTaskbar = false, ShowActivated = false
            };
            try
            {
                window.Show();
                window.UpdateLayout();
                surface.SetVideoProcessing(configuration);
                surface.Present(frame);
                var snapshot = Assert.IsType<System.Windows.Media.Imaging.WriteableBitmap>(
                    surface.Snapshot);
                var actual = new byte[snapshot.PixelWidth * snapshot.PixelHeight * 4];
                snapshot.CopyPixels(actual, snapshot.PixelWidth * 4, 0);
                using var cpu = new SoftwareEmulationVideoProcessingPipeline();
                var expectedFrame = cpu.Process(configuration, frame, new(4, 4),
                    new(snapshot.PixelWidth, snapshot.PixelHeight));
                var expected = EmulationVideoPixelFunctions.ToBgra32(expectedFrame);
                Assert.Equal(expected.Length, actual.Length);
                for (var index = 0; index < expected.Length; index++)
                    Assert.InRange(Math.Abs(expected[index] - actual[index]), 0, 1);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Theory]
    [Trait("Category", "GpuExhaustive")]
    [InlineData(EmulationVideoRenderer.Wpf)]
    [InlineData(EmulationVideoRenderer.OpenGL)]
    [InlineData(EmulationVideoRenderer.Direct3D11)]
    [InlineData(EmulationVideoRenderer.Vulkan)]
    public void DenoisingMatchesCpuReferenceOnRenderer(EmulationVideoRenderer renderer)
    {
        RunSta(() =>
        {
            var frame = NoisyEdgeFrame(5, 5);
            var configuration = new EmulationVideoProcessingConfiguration
            {
                Restoration = new EmulationImageRestorationConfiguration(Denoising: 100)
            };
            using var surface = CreateDeterministicSurface(renderer);
            var window = new System.Windows.Window
            {
                Content = surface.View, Width = 25, Height = 20,
                Left = -10000, Top = -10000,
                WindowStyle = System.Windows.WindowStyle.None,
                ShowInTaskbar = false, ShowActivated = false
            };
            try
            {
                window.Show();
                window.UpdateLayout();
                surface.SetVideoProcessing(configuration);
                surface.Present(frame);
                var snapshot = Assert.IsType<System.Windows.Media.Imaging.WriteableBitmap>(
                    surface.Snapshot);
                var actual = new byte[snapshot.PixelWidth * snapshot.PixelHeight * 4];
                snapshot.CopyPixels(actual, snapshot.PixelWidth * 4, 0);
                using var cpu = new SoftwareEmulationVideoProcessingPipeline();
                var expectedFrame = cpu.Process(configuration, frame, new(5, 5),
                    new(snapshot.PixelWidth, snapshot.PixelHeight));
                var expected = EmulationVideoPixelFunctions.ToBgra32(expectedFrame);
                Assert.Equal(expected.Length, actual.Length);
                for (var index = 0; index < expected.Length; index++)
                    Assert.InRange(Math.Abs(expected[index] - actual[index]), 0, 1);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Theory]
    [Trait("Category", "GpuExhaustive")]
    [InlineData(EmulationVideoRenderer.Wpf)]
    [InlineData(EmulationVideoRenderer.OpenGL)]
    [InlineData(EmulationVideoRenderer.Direct3D11)]
    [InlineData(EmulationVideoRenderer.Vulkan)]
    public void DebandingMatchesCpuReferenceOnRenderer(EmulationVideoRenderer renderer)
    {
        RunSta(() =>
        {
            var frame = BandedGradientFrame(5, 5);
            var configuration = new EmulationVideoProcessingConfiguration
            {
                Restoration = new EmulationImageRestorationConfiguration(Debanding: 100)
            };
            using var surface = CreateDeterministicSurface(renderer);
            var window = new System.Windows.Window
            {
                Content = surface.View, Width = 25, Height = 20,
                Left = -10000, Top = -10000,
                WindowStyle = System.Windows.WindowStyle.None,
                ShowInTaskbar = false, ShowActivated = false
            };
            try
            {
                window.Show();
                window.UpdateLayout();
                surface.SetVideoProcessing(configuration);
                surface.Present(frame);
                var snapshot = Assert.IsType<System.Windows.Media.Imaging.WriteableBitmap>(
                    surface.Snapshot);
                var actual = new byte[snapshot.PixelWidth * snapshot.PixelHeight * 4];
                snapshot.CopyPixels(actual, snapshot.PixelWidth * 4, 0);
                using var cpu = new SoftwareEmulationVideoProcessingPipeline();
                var expectedFrame = cpu.Process(configuration, frame, new(5, 5),
                    new(snapshot.PixelWidth, snapshot.PixelHeight));
                var expected = EmulationVideoPixelFunctions.ToBgra32(expectedFrame);
                Assert.Equal(expected.Length, actual.Length);
                for (var index = 0; index < expected.Length; index++)
                    Assert.InRange(Math.Abs(expected[index] - actual[index]), 0, 1);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Theory]
    [Trait("Category", "GpuExhaustive")]
    [InlineData(EmulationVideoRenderer.Wpf)]
    [InlineData(EmulationVideoRenderer.OpenGL)]
    [InlineData(EmulationVideoRenderer.Direct3D11)]
    [InlineData(EmulationVideoRenderer.Vulkan)]
    public void DetailRecoveryMatchesCpuReferenceOnRenderer(EmulationVideoRenderer renderer)
    {
        RunSta(() =>
        {
            var frame = DetailFrame(5, 5);
            var configuration = new EmulationVideoProcessingConfiguration
            {
                Restoration = new EmulationImageRestorationConfiguration(DetailRecovery: 100)
            };
            using var surface = CreateDeterministicSurface(renderer);
            var window = new System.Windows.Window
            {
                Content = surface.View, Width = 25, Height = 20,
                Left = -10000, Top = -10000,
                WindowStyle = System.Windows.WindowStyle.None,
                ShowInTaskbar = false, ShowActivated = false
            };
            try
            {
                window.Show();
                window.UpdateLayout();
                surface.SetVideoProcessing(configuration);
                surface.Present(frame);
                var snapshot = Assert.IsType<System.Windows.Media.Imaging.WriteableBitmap>(
                    surface.Snapshot);
                var actual = new byte[snapshot.PixelWidth * snapshot.PixelHeight * 4];
                snapshot.CopyPixels(actual, snapshot.PixelWidth * 4, 0);
                using var cpu = new SoftwareEmulationVideoProcessingPipeline();
                var expectedFrame = cpu.Process(configuration, frame, new(5, 5),
                    new(snapshot.PixelWidth, snapshot.PixelHeight));
                var expected = EmulationVideoPixelFunctions.ToBgra32(expectedFrame);
                Assert.Equal(expected.Length, actual.Length);
                for (var index = 0; index < expected.Length; index++)
                    Assert.InRange(Math.Abs(expected[index] - actual[index]), 0, 1);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Theory]
    [Trait("Category", "GpuExhaustive")]
    [InlineData(EmulationVideoRenderer.Wpf)]
    [InlineData(EmulationVideoRenderer.OpenGL)]
    [InlineData(EmulationVideoRenderer.Direct3D11)]
    [InlineData(EmulationVideoRenderer.Vulkan)]
    public void DeinterlacingMatchesCpuReferenceOnRenderer(EmulationVideoRenderer renderer)
    {
        RunSta(() =>
        {
            var frame = InterlacedFrame(3, 5);
            var configuration = new EmulationVideoProcessingConfiguration
            {
                Restoration = new EmulationImageRestorationConfiguration(
                    Deinterlacing: EmulationDeinterlacingMode.BobEvenLines)
            };
            using var surface = CreateDeterministicSurface(renderer);
            var window = new System.Windows.Window
            {
                Content = surface.View, Width = 24, Height = 20,
                Left = -10000, Top = -10000,
                WindowStyle = System.Windows.WindowStyle.None,
                ShowInTaskbar = false, ShowActivated = false
            };
            try
            {
                window.Show();
                window.UpdateLayout();
                surface.SetVideoProcessing(configuration);
                surface.Present(frame);
                var snapshot = Assert.IsType<System.Windows.Media.Imaging.WriteableBitmap>(
                    surface.Snapshot);
                var actual = new byte[snapshot.PixelWidth * snapshot.PixelHeight * 4];
                snapshot.CopyPixels(actual, snapshot.PixelWidth * 4, 0);
                using var cpu = new SoftwareEmulationVideoProcessingPipeline();
                var expectedFrame = cpu.Process(configuration, frame, new(3, 5),
                    new(snapshot.PixelWidth, snapshot.PixelHeight));
                var expected = EmulationVideoPixelFunctions.ToBgra32(expectedFrame);
                Assert.Equal(expected.Length, actual.Length);
                for (var index = 0; index < expected.Length; index++)
                    Assert.InRange(Math.Abs(expected[index] - actual[index]), 0, 1);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    [Trait("Category", "GpuExhaustive")]
    public void CrtPresetsRenderDistinctAmigaAndAtariValidationBoardsOnAllRenderers()
    {
        RunSta(() =>
        {
            var presets = new[]
            {
                EmulationVideoPreset.CrtArcadeColor,
                EmulationVideoPreset.CrtTelevisionColor,
                EmulationVideoPreset.CrtGreen,
                EmulationVideoPreset.CrtAmber,
                EmulationVideoPreset.CrtWhite
            };
            var machineFrames = new[]
            {
                ValidationFrame(64, 48, amiga: true),
                ValidationFrame(64, 48, amiga: false)
            };
            var outputDirectory = Environment.GetEnvironmentVariable(
                "GWGUI_CRT_VALIDATION_OUTPUT");

            foreach (var renderer in Enum.GetValues<EmulationVideoRenderer>())
            {
                using var surface = CreateDeterministicSurface(renderer);
                var window = new System.Windows.Window
                {
                    Content = surface.View,
                    Width = 160,
                    Height = 120,
                    Left = -10000,
                    Top = -10000,
                    WindowStyle = System.Windows.WindowStyle.None,
                    ShowInTaskbar = false,
                    ShowActivated = false
                };
                var cells = new List<(int Row, int Column, int Width, int Height, byte[] Pixels)>();
                try
                {
                    window.Show();
                    window.UpdateLayout();
                    for (var row = 0; row < machineFrames.Length; row++)
                    {
                        var hashes = new HashSet<string>(StringComparer.Ordinal);
                        for (var column = 0; column < presets.Length; column++)
                        {
                            surface.SetVideoProcessing(
                                EmulationVideoProcessingCatalog.PresetConfigurations[presets[column]]);
                            surface.Present(machineFrames[row]);
                            Assert.NotNull(surface.Snapshot);
                            var snapshot = surface.Snapshot;
                            var pixels = new byte[checked(snapshot.PixelWidth
                                * snapshot.PixelHeight * 4)];
                            snapshot.CopyPixels(pixels, snapshot.PixelWidth * 4, 0);
                            Assert.Contains(pixels.Chunk(4), pixel =>
                                pixel[0] != 0 || pixel[1] != 0 || pixel[2] != 0);
                            hashes.Add(Convert.ToHexString(
                                System.Security.Cryptography.SHA256.HashData(pixels)));
                            cells.Add((row, column, snapshot.PixelWidth,
                                snapshot.PixelHeight, pixels));
                        }
                        Assert.Equal(presets.Length, hashes.Count);
                    }
                }
                finally
                {
                    window.Close();
                }

                if (!string.IsNullOrWhiteSpace(outputDirectory))
                    WriteValidationBoard(outputDirectory, renderer, cells,
                        presets.Length, machineFrames.Length, "crt");
            }
        });
    }

    [Fact]
    public void AdvancedFiltersRenderDistinctValidationBoardsOnAllRenderers()
    {
        RunSta(() =>
        {
            var configurations = new EmulationVideoProcessingConfiguration[]
            {
                new(),
                new() { Sampling = EmulationVideoSampling.Xbr },
                new() { Sampling = EmulationVideoSampling.Xbrz },
                new() { Sampling = EmulationVideoSampling.Hqx },
                new() { Sampling = EmulationVideoSampling.ScaleFx },
                new() { Sampling = EmulationVideoSampling.ScaleNx },
                new() { Sampling = EmulationVideoSampling.Sabr },
                new() { Restoration = new(Dedithering: 100) },
                new() { Restoration = new(Denoising: 100) },
                new() { Restoration = new(Debanding: 100) },
                new() { Restoration = new(DetailRecovery: 100) },
                new() { Restoration = new(Deinterlacing:
                    EmulationDeinterlacingMode.BobEvenLines) }
            };
            var frame = AdvancedValidationFrame(16, 16);
            using var validationPipeline = new SoftwareEmulationVideoProcessingPipeline();
            var validationFrames = configurations.Select(configuration =>
                validationPipeline.Process(configuration, frame, new(16, 16), new(48, 48)))
                .ToArray();
            var outputDirectory = Path.Combine(RepositoryRoot(),
                "build", "validation", "advanced-filter-validation");

            foreach (var renderer in Enum.GetValues<EmulationVideoRenderer>())
            {
                using var surface = CreateDeterministicSurface(renderer);
                var window = new System.Windows.Window
                {
                    Content = surface.View, Width = 96, Height = 72,
                    Left = -10000, Top = -10000,
                    WindowStyle = System.Windows.WindowStyle.None,
                    ShowInTaskbar = false, ShowActivated = false
                };
                var cells = new List<(int Row, int Column, int Width, int Height, byte[] Pixels)>();
                try
                {
                    window.Show();
                    window.UpdateLayout();
                    for (var column = 0; column < configurations.Length; column++)
                    {
                        surface.SetVideoProcessing(new EmulationVideoProcessingConfiguration());
                        surface.Present(validationFrames[column]);
                        var snapshot = Assert.IsType<System.Windows.Media.Imaging.WriteableBitmap>(
                            surface.Snapshot);
                        var pixels = new byte[checked(snapshot.PixelWidth * snapshot.PixelHeight * 4)];
                        snapshot.CopyPixels(pixels, snapshot.PixelWidth * 4, 0);
                        cells.Add((0, column, snapshot.PixelWidth, snapshot.PixelHeight, pixels));
                    }
                }
                finally
                {
                    window.Close();
                }

                var baseline = cells[0].Pixels;
                Assert.All(cells.Skip(1), cell => Assert.False(baseline.SequenceEqual(cell.Pixels)));
                WriteValidationBoard(outputDirectory, renderer, cells,
                    configurations.Length, 1, "advanced");
            }
        });
    }

    [Theory]
    [InlineData(0f)]
    [InlineData(0.003f)]
    [InlineData(0.018f)]
    [InlineData(0.5f)]
    [InlineData(1f)]
    public void CpuSrgbLinearConversionsRoundTrip(float srgb)
    {
        var linear = SoftwareEmulationVideoProcessingPipeline.SrgbToLinear(srgb);
        var roundTrip = SoftwareEmulationVideoProcessingPipeline.LinearToSrgb(linear);

        Assert.InRange(linear, 0f, 1f);
        Assert.Equal(srgb, roundTrip, 5);
    }

    [Fact]
    public void CpuAdjustmentsChangePixelsAndPreserveFrameMetadata()
    {
        var pixels = new byte[]
        {
            15, 30, 45, 0, 60, 75, 90, 0, 105, 120, 135, 0,
            25, 40, 55, 0, 70, 85, 100, 0, 115, 130, 145, 0,
            35, 50, 65, 0, 80, 95, 110, 0, 125, 140, 155, 0
        };
        var frame = new VideoFrame(pixels, 3, 3, 12, EmulationPixelFormat.Xrgb8888,
            16f / 9f, 73, TimeSpan.FromMilliseconds(250));
        using var pipeline = new SoftwareEmulationVideoProcessingPipeline();
        var configuration = new EmulationVideoProcessingConfiguration
        {
            Adjustments = new EmulationImageAdjustments(
                Brightness: 2, Contrast: -2, Gamma: 3, Saturation: 4, Sharpness: 5)
        };

        var output = pipeline.Process(configuration, frame,
            new EmulationVideoProcessingSize(3, 3),
            new EmulationVideoProcessingSize(3, 3));

        Assert.False(frame.Pixels.Span.SequenceEqual(output.Pixels.Span));
        Assert.Equal((frame.Width, frame.Height, frame.AspectRatio, frame.Sequence, frame.Timestamp),
            (output.Width, output.Height, output.AspectRatio, output.Sequence, output.Timestamp));
        Assert.Equal(12, output.Pitch);
        Assert.Equal(EmulationPixelFormat.Xrgb8888, output.PixelFormat);
    }

    [Fact]
    public void CpuCrtColorModesApplyDocumentedLinearLuminanceTints()
    {
        var frame = new VideoFrame(new byte[] { 50, 100, 200, 0 }, 1, 1, 4,
            EmulationPixelFormat.Xrgb8888, 1f, 1, TimeSpan.Zero);
        using var pipeline = new SoftwareEmulationVideoProcessingPipeline();

        byte[] Process(EmulationCrtColorMode mode)
        {
            var output = pipeline.Process(new EmulationVideoProcessingConfiguration
            {
                DisplayTechnology = EmulationVideoDisplayTechnology.Crt,
                Crt = new EmulationCrtVideoConfiguration(mode)
            }, frame, new EmulationVideoProcessingSize(1, 1),
                new EmulationVideoProcessingSize(1, 1));
            return EmulationVideoPixelFunctions.ToBgra32(output);
        }

        var color = Process(EmulationCrtColorMode.Color);
        var green = Process(EmulationCrtColorMode.Green);
        var amber = Process(EmulationCrtColorMode.Amber);
        var white = Process(EmulationCrtColorMode.White);
        var gray = Process(EmulationCrtColorMode.Gray);

        Assert.Equal(new byte[] { 50, 100, 200 }, color[..3]);
        Assert.True(green[1] > green[2] && green[1] > green[0]);
        Assert.True(amber[2] > amber[1] && amber[1] > amber[0]);
        Assert.Equal(white[0], white[1]);
        Assert.Equal(white[1], white[2]);
        Assert.Equal(gray[0], gray[1]);
        Assert.Equal(gray[1], gray[2]);
        Assert.True(gray[0] < white[0]);
    }

    [Fact]
    public void CpuCrtOpticalPassesAreNeutralAtZeroAndComposable()
    {
        var pixels = Enumerable.Range(0, 25).SelectMany(index => new byte[]
        {
            (byte)(10 + index * 3), (byte)(30 + index * 4), (byte)(60 + index * 6), 0
        }).ToArray();
        var frame = new VideoFrame(pixels, 5, 5, 20, EmulationPixelFormat.Xrgb8888,
            1f, 2, TimeSpan.Zero);
        using var pipeline = new SoftwareEmulationVideoProcessingPipeline();

        byte[] Process(EmulationCrtVideoConfiguration crt)
        {
            var output = pipeline.Process(new EmulationVideoProcessingConfiguration
            {
                DisplayTechnology = EmulationVideoDisplayTechnology.Crt,
                Crt = crt
            }, frame, new EmulationVideoProcessingSize(5, 5),
                new EmulationVideoProcessingSize(5, 5));
            return EmulationVideoPixelFunctions.ToBgra32(output);
        }

        var neutral = Process(new EmulationCrtVideoConfiguration());
        for (var index = 0; index < pixels.Length; index += 4)
            Assert.Equal(pixels.AsSpan(index, 3).ToArray(), neutral.AsSpan(index, 3).ToArray());

        var individual = new[]
        {
            new EmulationCrtVideoConfiguration(BeamWidth: 100),
            new EmulationCrtVideoConfiguration(BeamIntensity: 100),
            new EmulationCrtVideoConfiguration(BeamDiffusion: 100),
            new EmulationCrtVideoConfiguration(HaloIntensity: 100),
            new EmulationCrtVideoConfiguration(Mask: EmulationCrtMask.ApertureGrille,
                MaskIntensity: 100),
            new EmulationCrtVideoConfiguration(HorizontalCurvature: 100, VerticalCurvature: 100),
            new EmulationCrtVideoConfiguration(Vignette: 100)
        }.Select(Process).ToArray();
        Assert.All(individual, output => Assert.False(neutral.SequenceEqual(output)));

        var combined = Process(new EmulationCrtVideoConfiguration(BeamWidth: 45,
            BeamIntensity: 50, BeamDiffusion: 30, HaloIntensity: 40,
            Mask: EmulationCrtMask.SlotMask, MaskSubpixels: EmulationSubpixelLayout.Bgr,
            MaskIntensity: 55, HorizontalCurvature: 25, VerticalCurvature: 25, Vignette: 35));
        Assert.All(individual, output => Assert.False(combined.SequenceEqual(output)));
    }

    [Fact]
    public void CpuCrtScanlinesHonorOrientationIntensityThicknessPhaseAndCompensation()
    {
        var pixels = Enumerable.Repeat(new byte[] { 100, 100, 100, 0 }, 16)
            .SelectMany(pixel => pixel).ToArray();
        var frame = new VideoFrame(pixels, 4, 4, 16, EmulationPixelFormat.Xrgb8888,
            1f, 3, TimeSpan.Zero);
        using var pipeline = new SoftwareEmulationVideoProcessingPipeline();

        byte[] Process(EmulationCrtVideoConfiguration crt) => EmulationVideoPixelFunctions.ToBgra32(
            pipeline.Process(new EmulationVideoProcessingConfiguration
            {
                DisplayTechnology = EmulationVideoDisplayTechnology.Crt,
                Crt = crt
            }, frame, new EmulationVideoProcessingSize(4, 4),
                new EmulationVideoProcessingSize(8, 8)));

        var neutral = Process(new EmulationCrtVideoConfiguration(
            ScanlinesEnabled: true, ScanlineIntensity: 0));
        var horizontal = Process(new EmulationCrtVideoConfiguration(
            ScanlinesEnabled: true, ScanlineIntensity: 60, ScanlineThickness: 40));
        var vertical = Process(new EmulationCrtVideoConfiguration(
            ScanlinesEnabled: true, ScanlineOrientation: EmulationPatternOrientation.Vertical,
            ScanlineIntensity: 60, ScanlineThickness: 40));
        var thick = Process(new EmulationCrtVideoConfiguration(
            ScanlinesEnabled: true, ScanlineIntensity: 60, ScanlineThickness: 90));
        var phased = Process(new EmulationCrtVideoConfiguration(
            ScanlinesEnabled: true, ScanlineIntensity: 60, ScanlineThickness: 40,
            ScanlinePhase: EmulationScanlinePhase.Quarter));
        var compensated = Process(new EmulationCrtVideoConfiguration(
            ScanlinesEnabled: true, ScanlineIntensity: 60, ScanlineThickness: 40,
            ScanlineCompensation: 100));

        Assert.Equal(pixels[0], neutral[0]);
        Assert.Equal(horizontal[0], horizontal[4]);
        Assert.NotEqual(horizontal[0], horizontal[64]);
        Assert.Equal(vertical[0], vertical[64]);
        Assert.NotEqual(vertical[0], vertical[8]);
        Assert.False(horizontal.SequenceEqual(thick));
        Assert.False(horizontal.SequenceEqual(phased));
        Assert.True(compensated.Sum(value => (long)value) > horizontal.Sum(value => (long)value));
    }

    [Fact]
    public void CpuCrtVoluntaryPatternHonorsOrientationFrequencyPhaseAndIntensity()
    {
        var pixels = Enumerable.Repeat(new byte[] { 120, 120, 120, 0 }, 64)
            .SelectMany(pixel => pixel).ToArray();
        var frame = new VideoFrame(pixels, 8, 8, 32, EmulationPixelFormat.Xrgb8888,
            1f, 4, TimeSpan.Zero);
        using var pipeline = new SoftwareEmulationVideoProcessingPipeline();

        byte[] Process(EmulationCrtVideoConfiguration crt) => EmulationVideoPixelFunctions.ToBgra32(
            pipeline.Process(new EmulationVideoProcessingConfiguration
            {
                DisplayTechnology = EmulationVideoDisplayTechnology.Crt,
                Crt = crt
            }, frame, new EmulationVideoProcessingSize(8, 8),
                new EmulationVideoProcessingSize(8, 8)));

        var neutral = Process(new EmulationCrtVideoConfiguration(
            PatternEnabled: true, PatternIntensity: 0));
        var horizontal = Process(new EmulationCrtVideoConfiguration(
            PatternEnabled: true, PatternFrequency: 10, PatternIntensity: 70));
        var vertical = Process(new EmulationCrtVideoConfiguration(
            PatternEnabled: true, PatternOrientation: EmulationPatternOrientation.Vertical,
            PatternFrequency: 10, PatternIntensity: 70));
        var frequency = Process(new EmulationCrtVideoConfiguration(
            PatternEnabled: true, PatternFrequency: 45, PatternIntensity: 70));
        var phase = Process(new EmulationCrtVideoConfiguration(
            PatternEnabled: true, PatternFrequency: 10, PatternPhase: 25,
            PatternIntensity: 70));

        Assert.Equal(pixels[0], neutral[0]);
        Assert.Equal(horizontal[0], horizontal[4]);
        Assert.NotEqual(horizontal[0], horizontal[32]);
        Assert.Equal(vertical[0], vertical[32]);
        Assert.NotEqual(vertical[0], vertical[4]);
        Assert.False(horizontal.SequenceEqual(frequency));
        Assert.False(horizontal.SequenceEqual(phase));
        Assert.False(neutral.SequenceEqual(horizontal));
    }

    [Theory]
    [InlineData(EmulationCrtMask.None, EmulationSubpixelLayout.Monochrome)]
    [InlineData(EmulationCrtMask.None, EmulationSubpixelLayout.Rgb)]
    [InlineData(EmulationCrtMask.None, EmulationSubpixelLayout.Bgr)]
    [InlineData(EmulationCrtMask.ApertureGrille, EmulationSubpixelLayout.Monochrome)]
    [InlineData(EmulationCrtMask.ApertureGrille, EmulationSubpixelLayout.Rgb)]
    [InlineData(EmulationCrtMask.ApertureGrille, EmulationSubpixelLayout.Bgr)]
    [InlineData(EmulationCrtMask.ShadowMask, EmulationSubpixelLayout.Monochrome)]
    [InlineData(EmulationCrtMask.ShadowMask, EmulationSubpixelLayout.Rgb)]
    [InlineData(EmulationCrtMask.ShadowMask, EmulationSubpixelLayout.Bgr)]
    [InlineData(EmulationCrtMask.SlotMask, EmulationSubpixelLayout.Monochrome)]
    [InlineData(EmulationCrtMask.SlotMask, EmulationSubpixelLayout.Rgb)]
    [InlineData(EmulationCrtMask.SlotMask, EmulationSubpixelLayout.Bgr)]
    public void CpuCrtMaskAndSubpixelChoicesAreDeterministic(
        EmulationCrtMask mask, EmulationSubpixelLayout layout)
    {
        var frame = DeterministicFrame(6, 5);
        using var pipeline = new SoftwareEmulationVideoProcessingPipeline();
        var configuration = new EmulationVideoProcessingConfiguration
        {
            DisplayTechnology = EmulationVideoDisplayTechnology.Crt,
            Crt = new EmulationCrtVideoConfiguration(Mask: mask,
                MaskSubpixels: layout, MaskIntensity: 70)
        };

        var first = pipeline.Process(configuration, frame,
            new EmulationVideoProcessingSize(6, 5), new EmulationVideoProcessingSize(6, 5));
        var second = pipeline.Process(configuration, frame,
            new EmulationVideoProcessingSize(6, 5), new EmulationVideoProcessingSize(6, 5));
        var actual = EmulationVideoPixelFunctions.ToBgra32(first);
        Assert.Equal(actual, EmulationVideoPixelFunctions.ToBgra32(second));
        Assert.Equal(mask == EmulationCrtMask.None,
            frame.Pixels.Span.ToArray().Where((_, index) => index % 4 != 3)
                .SequenceEqual(actual.Where((_, index) => index % 4 != 3)));
    }

    [Fact]
    public void CpuCrtResizeAndLiveChangesAreDeterministicAndStateless()
    {
        var frame = DeterministicFrame(4, 3);
        using var pipeline = new SoftwareEmulationVideoProcessingPipeline();
        var configuration = new EmulationVideoProcessingConfiguration
        {
            Sampling = EmulationVideoSampling.Bicubic,
            DisplayTechnology = EmulationVideoDisplayTechnology.Crt,
            Crt = new EmulationCrtVideoConfiguration(EmulationCrtColorMode.Green,
                BeamWidth: 35, HaloIntensity: 25, HorizontalCurvature: 20,
                VerticalCurvature: 20, Vignette: 15,
                ScanlinesEnabled: true, ScanlineIntensity: 30, ScanlineThickness: 55)
        };
        var sourceSize = new EmulationVideoProcessingSize(4, 3);
        var first = pipeline.Process(configuration, frame, sourceSize,
            new EmulationVideoProcessingSize(13, 7));

        _ = pipeline.Process(new EmulationVideoProcessingConfiguration
        {
            DisplayTechnology = EmulationVideoDisplayTechnology.Crt,
            Crt = new EmulationCrtVideoConfiguration(EmulationCrtColorMode.Amber,
                PatternEnabled: true, PatternFrequency: 70, PatternIntensity: 80)
        }, frame, sourceSize, new EmulationVideoProcessingSize(11, 8));
        var repeated = pipeline.Process(configuration, frame, sourceSize,
            new EmulationVideoProcessingSize(13, 7));
        var resized = pipeline.Process(configuration, frame, sourceSize,
            new EmulationVideoProcessingSize(11, 8));

        Assert.Equal((13, 7, 13 * 4), (first.Width, first.Height, first.Pitch));
        Assert.Equal(first.Pixels.Span.ToArray(), repeated.Pixels.Span.ToArray());
        Assert.Equal((11, 8, 11 * 4), (resized.Width, resized.Height, resized.Pitch));
        Assert.NotEqual(first.Pixels.Length, resized.Pixels.Length);
    }

    [Fact]
    public void CpuFixedPixelGridSubpixelsMonochromeAndGeneralSharpnessAreDeterministic()
    {
        var frame = DeterministicFrame(4, 3);
        using var pipeline = new SoftwareEmulationVideoProcessingPipeline();

        byte[] Process(EmulationFixedPixelVideoConfiguration fixedPixel,
            int sharpness = 0) => EmulationVideoPixelFunctions.ToBgra32(pipeline.Process(
                new EmulationVideoProcessingConfiguration
                {
                    DisplayTechnology = EmulationVideoDisplayTechnology.FixedPixel,
                    Sampling = EmulationVideoSampling.Nearest,
                    Adjustments = new EmulationImageAdjustments(Sharpness: sharpness),
                    FixedPixel = fixedPixel
                }, frame, new EmulationVideoProcessingSize(4, 3),
                new EmulationVideoProcessingSize(13, 9)));

        var neutral = Process(new EmulationFixedPixelVideoConfiguration());
        var neutralBgr = Process(new EmulationFixedPixelVideoConfiguration(
            Subpixels: EmulationSubpixelLayout.Bgr));
        var rgb = Process(new EmulationFixedPixelVideoConfiguration(
            Subpixels: EmulationSubpixelLayout.Rgb, GridIntensity: 70, PixelGap: 40));
        var rgbRepeated = Process(new EmulationFixedPixelVideoConfiguration(
            Subpixels: EmulationSubpixelLayout.Rgb, GridIntensity: 70, PixelGap: 40));
        var bgr = Process(new EmulationFixedPixelVideoConfiguration(
            Subpixels: EmulationSubpixelLayout.Bgr, GridIntensity: 70, PixelGap: 40));
        var monochrome = Process(new EmulationFixedPixelVideoConfiguration(
            Subpixels: EmulationSubpixelLayout.Monochrome));
        var customBlue = Process(new EmulationFixedPixelVideoConfiguration(
            Subpixels: EmulationSubpixelLayout.Monochrome,
            MonochromePalette: EmulationMonochromePalette.Blue));
        var sharp = Process(new EmulationFixedPixelVideoConfiguration(
            Subpixels: EmulationSubpixelLayout.Rgb, GridIntensity: 70, PixelGap: 40), 10);

        Assert.Equal(rgb, rgbRepeated);
        Assert.Equal(neutral, neutralBgr);
        Assert.False(neutral.SequenceEqual(rgb));
        Assert.False(rgb.SequenceEqual(bgr));
        Assert.False(monochrome.SequenceEqual(customBlue));
        Assert.All(monochrome.Chunk(4), pixel => Assert.True(
            pixel[1] >= pixel[2] && pixel[1] >= pixel[0]));
        Assert.All(customBlue.Chunk(4), pixel => Assert.True(
            pixel[0] >= pixel[1] && pixel[0] >= pixel[2]));
        Assert.False(rgb.SequenceEqual(sharp));
    }

    [Fact]
    public void CpuFixedPixelAppliesOnlyDocumentedConditionalTechnologyDifferences()
    {
        var frame = DeterministicFrame(3, 2);
        using var pipeline = new SoftwareEmulationVideoProcessingPipeline();

        byte[] Process(EmulationFixedPixelTechnology technology, int? backlight,
            int? blackDepth) => EmulationVideoPixelFunctions.ToBgra32(pipeline.Process(
                new EmulationVideoProcessingConfiguration
                {
                    DisplayTechnology = EmulationVideoDisplayTechnology.FixedPixel,
                    FixedPixel = new EmulationFixedPixelVideoConfiguration(
                        Technology: technology, BacklightIntensity: backlight,
                        BlackDepth: blackDepth)
                }, frame, new EmulationVideoProcessingSize(3, 2),
                new EmulationVideoProcessingSize(3, 2)));

        foreach (var technology in new[]
                 { EmulationFixedPixelTechnology.Lcd, EmulationFixedPixelTechnology.LedBacklitLcd })
            Assert.False(Process(technology, 0, null)
                .SequenceEqual(Process(technology, 100, null)));

        Assert.Equal(Process(EmulationFixedPixelTechnology.Oled, 0, null),
            Process(EmulationFixedPixelTechnology.Oled, 100, null));
        Assert.False(Process(EmulationFixedPixelTechnology.Oled, null, 0)
            .SequenceEqual(Process(EmulationFixedPixelTechnology.Oled, null, 100)));
        Assert.False(Process(EmulationFixedPixelTechnology.Lcd, null, 0)
            .SequenceEqual(Process(EmulationFixedPixelTechnology.Lcd, null, 100)));
        Assert.False(Process(EmulationFixedPixelTechnology.Lcd, null, null)
            .SequenceEqual(Process(EmulationFixedPixelTechnology.LedBacklitLcd, null, null)));
    }

    [Fact]
    public void CpuFixedPixelTemporalResponseUsesOneResettableHistoryFrame()
    {
        static VideoFrame Pixel(byte value, int milliseconds) => new(
            new byte[] { value, value, value, 0 }, 1, 1, 4,
            EmulationPixelFormat.Xrgb8888, 1f, milliseconds,
            TimeSpan.FromMilliseconds(milliseconds));
        static EmulationVideoProcessingConfiguration Configuration(int response, int persistence) =>
            new()
            {
                DisplayTechnology = EmulationVideoDisplayTechnology.FixedPixel,
                FixedPixel = new EmulationFixedPixelVideoConfiguration(
                    Technology: EmulationFixedPixelTechnology.Oled,
                    Subpixels: EmulationSubpixelLayout.Monochrome,
                    MonochromePalette: EmulationMonochromePalette.White,
                    ResponseTimeMilliseconds: response, PersistenceIntensity: persistence)
            };
        static byte Channel(SoftwareEmulationVideoProcessingPipeline pipeline,
            EmulationVideoProcessingConfiguration configuration, VideoFrame frame,
            EmulationVideoProcessingSize output) => EmulationVideoPixelFunctions.ToBgra32(
                pipeline.Process(configuration, frame, new EmulationVideoProcessingSize(1, 1),
                    output))[0];

        using var shortPipeline = new SoftwareEmulationVideoProcessingPipeline();
        var response = Configuration(1000, 0);
        Assert.Equal(255, Channel(shortPipeline, response, Pixel(255, 0),
            new EmulationVideoProcessingSize(1, 1)));
        var shortInterval = Channel(shortPipeline, response, Pixel(0, 16),
            new EmulationVideoProcessingSize(1, 1));

        using var longPipeline = new SoftwareEmulationVideoProcessingPipeline();
        _ = Channel(longPipeline, response, Pixel(255, 0),
            new EmulationVideoProcessingSize(1, 1));
        var longInterval = Channel(longPipeline, response, Pixel(0, 1000),
            new EmulationVideoProcessingSize(1, 1));
        Assert.True(shortInterval > longInterval && longInterval > 0);

        using var persistencePipeline = new SoftwareEmulationVideoProcessingPipeline();
        var persistence = Configuration(0, 50);
        _ = Channel(persistencePipeline, persistence, Pixel(255, 0),
            new EmulationVideoProcessingSize(1, 1));
        Assert.InRange(Channel(persistencePipeline, persistence, Pixel(0, 16),
            new EmulationVideoProcessingSize(1, 1)), 187, 189);
        Assert.Equal(0, Channel(persistencePipeline, persistence, Pixel(0, 32),
            new EmulationVideoProcessingSize(2, 2)));

        using var resetPipeline = new SoftwareEmulationVideoProcessingPipeline();
        _ = Channel(resetPipeline, persistence, Pixel(255, 0),
            new EmulationVideoProcessingSize(1, 1));
        _ = Channel(resetPipeline, new EmulationVideoProcessingConfiguration(), Pixel(0, 16),
            new EmulationVideoProcessingSize(1, 1));
        Assert.Equal(0, Channel(resetPipeline, persistence, Pixel(0, 32),
            new EmulationVideoProcessingSize(1, 1)));
    }

    [Fact]
    public void CpuPlasmaPassesAreNeutralDeterministicAndUseResettableDecayingHistory()
    {
        var frame = DeterministicFrame(4, 3);
        byte[] Process(EmulationPlasmaVideoConfiguration plasma, long sequence = 10)
        {
            using var pipeline = new SoftwareEmulationVideoProcessingPipeline();
            var input = frame with { Sequence = sequence };
            return EmulationVideoPixelFunctions.ToBgra32(pipeline.Process(
                new EmulationVideoProcessingConfiguration
                {
                    DisplayTechnology = EmulationVideoDisplayTechnology.Plasma,
                    Plasma = plasma
                }, input, new EmulationVideoProcessingSize(4, 3),
                new EmulationVideoProcessingSize(13, 9)));
        }

        var neutral = Process(new EmulationPlasmaVideoConfiguration());
        var cells = Process(new EmulationPlasmaVideoConfiguration(CellStructure: 100));
        var diffusion = Process(new EmulationPlasmaVideoConfiguration(Diffusion: 100));
        var dither = Process(new EmulationPlasmaVideoConfiguration(TemporalDithering: 100));
        var blacks = Process(new EmulationPlasmaVideoConfiguration(BlackDepth: 100));
        var phosphors = Process(new EmulationPlasmaVideoConfiguration(PhosphorIntensity: 100));
        var gamma = Process(new EmulationPlasmaVideoConfiguration(GammaResponse: 100));
        Assert.False(neutral.SequenceEqual(cells));
        Assert.False(neutral.SequenceEqual(diffusion));
        Assert.False(neutral.SequenceEqual(dither));
        Assert.False(neutral.SequenceEqual(blacks));
        Assert.False(neutral.SequenceEqual(phosphors));
        Assert.False(neutral.SequenceEqual(gamma));
        Assert.Equal(dither, Process(
            new EmulationPlasmaVideoConfiguration(TemporalDithering: 100)));
        Assert.False(dither.SequenceEqual(Process(
            new EmulationPlasmaVideoConfiguration(TemporalDithering: 100), 11)));

        static VideoFrame Pixel(byte value, long sequence) => new(
            new byte[] { value, value, value, 0 }, 1, 1, 4,
            EmulationPixelFormat.Xrgb8888, 1f, sequence, TimeSpan.Zero);
        var configuration = new EmulationVideoProcessingConfiguration
        {
            DisplayTechnology = EmulationVideoDisplayTechnology.Plasma,
            Plasma = new EmulationPlasmaVideoConfiguration(PersistenceIntensity: 50)
        };
        using var history = new SoftwareEmulationVideoProcessingPipeline();
        _ = history.Process(configuration, Pixel(255, 1),
            new EmulationVideoProcessingSize(1, 1), new EmulationVideoProcessingSize(1, 1));
        var retained = history.Process(configuration, Pixel(0, 2),
            new EmulationVideoProcessingSize(1, 1), new EmulationVideoProcessingSize(1, 1));
        Assert.InRange(EmulationVideoPixelFunctions.ToBgra32(retained)[0], 187, 189);
        var resized = history.Process(configuration, Pixel(0, 3),
            new EmulationVideoProcessingSize(1, 1), new EmulationVideoProcessingSize(2, 2));
        Assert.Equal(0, EmulationVideoPixelFunctions.ToBgra32(resized)[0]);
        _ = history.Process(new EmulationVideoProcessingConfiguration(), Pixel(0, 4),
            new EmulationVideoProcessingSize(1, 1), new EmulationVideoProcessingSize(1, 1));
        var reset = history.Process(configuration, Pixel(0, 5),
            new EmulationVideoProcessingSize(1, 1), new EmulationVideoProcessingSize(1, 1));
        Assert.Equal(0, EmulationVideoPixelFunctions.ToBgra32(reset)[0]);
    }

    [Fact]
    public void CpuPlasmaDiffusionSpreadsHighlightsAndMaximumPersistenceDecays()
    {
        var pixels = new byte[5 * 5 * 4];
        var center = (2 * 5 + 2) * 4;
        pixels[center] = pixels[center + 1] = pixels[center + 2] = 255;
        var frame = new VideoFrame(pixels, 5, 5, 20, EmulationPixelFormat.Xrgb8888,
            1f, 1, TimeSpan.Zero);
        using (var diffusion = new SoftwareEmulationVideoProcessingPipeline())
        {
            var output = diffusion.Process(new EmulationVideoProcessingConfiguration
            {
                DisplayTechnology = EmulationVideoDisplayTechnology.Plasma,
                Plasma = new EmulationPlasmaVideoConfiguration(Diffusion: 100)
            }, frame, new EmulationVideoProcessingSize(5, 5),
                new EmulationVideoProcessingSize(5, 5));
            Assert.True(EmulationVideoPixelFunctions.ToBgra32(output)[(2 * 5 + 3) * 4] > 0);
        }

        using (var limiter = new SoftwareEmulationVideoProcessingPipeline())
        {
            var fullWhite = Enumerable.Repeat((byte)255, 4 * 4 * 4).ToArray();
            var brightFrame = new VideoFrame(fullWhite, 4, 4, 16,
                EmulationPixelFormat.Xrgb8888, 1f, 1, TimeSpan.Zero);
            var output = limiter.Process(new EmulationVideoProcessingConfiguration
            {
                DisplayTechnology = EmulationVideoDisplayTechnology.Plasma,
                Plasma = new EmulationPlasmaVideoConfiguration(
                    AutomaticBrightnessLimiter: 100)
            }, brightFrame, new EmulationVideoProcessingSize(4, 4),
                new EmulationVideoProcessingSize(4, 4));
            Assert.True(EmulationVideoPixelFunctions.ToBgra32(output)[0] < 255);
        }

        static VideoFrame Pixel(byte value, long sequence) => new(
            new byte[] { value, value, value, 0 }, 1, 1, 4,
            EmulationPixelFormat.Xrgb8888, 1f, sequence, TimeSpan.Zero);
        var configuration = new EmulationVideoProcessingConfiguration
        {
            DisplayTechnology = EmulationVideoDisplayTechnology.Plasma,
            Plasma = new EmulationPlasmaVideoConfiguration(PersistenceIntensity: 100)
        };
        using var persistence = new SoftwareEmulationVideoProcessingPipeline();
        _ = persistence.Process(configuration, Pixel(255, 1),
            new EmulationVideoProcessingSize(1, 1), new EmulationVideoProcessingSize(1, 1));
        var first = EmulationVideoPixelFunctions.ToBgra32(persistence.Process(configuration,
            Pixel(0, 2), new EmulationVideoProcessingSize(1, 1),
            new EmulationVideoProcessingSize(1, 1)))[0];
        var second = EmulationVideoPixelFunctions.ToBgra32(persistence.Process(configuration,
            Pixel(0, 3), new EmulationVideoProcessingSize(1, 1),
            new EmulationVideoProcessingSize(1, 1)))[0];
        Assert.True(first > second);
        Assert.True(second > 0);
    }

    [Fact]
    public void CpuVectorRasterApproximationDetectsLinesAndUsesOneResettableHistoryFrame()
    {
        var pixels = new byte[5 * 5 * 4];
        for (var y = 0; y < 5; y++)
        for (var x = 0; x < 5; x++)
        {
            var value = x >= 2 || y == 2 ? (byte)180 : (byte)15;
            var index = (y * 5 + x) * 4;
            pixels[index] = pixels[index + 1] = pixels[index + 2] = value;
        }
        var frame = new VideoFrame(pixels, 5, 5, 20, EmulationPixelFormat.Xrgb8888,
            1f, 10, TimeSpan.Zero);

        byte[] Process(EmulationVectorVideoConfiguration vector)
        {
            using var pipeline = new SoftwareEmulationVideoProcessingPipeline();
            return EmulationVideoPixelFunctions.ToBgra32(pipeline.Process(
                new EmulationVideoProcessingConfiguration
                {
                    DisplayTechnology = EmulationVideoDisplayTechnology.Vector,
                    Vector = vector
                }, frame, new EmulationVideoProcessingSize(5, 5),
                new EmulationVideoProcessingSize(5, 5)));
        }

        var neutral = Process(new EmulationVectorVideoConfiguration(
            LineThreshold: 50, HaloIntensity: 100));
        var lowThreshold = Process(new EmulationVectorVideoConfiguration(
            LineThreshold: 10, LineIntensity: 80));
        var highThreshold = Process(new EmulationVectorVideoConfiguration(
            LineThreshold: 90, LineIntensity: 80));
        var halo = Process(new EmulationVectorVideoConfiguration(
            LineThreshold: 10, LineIntensity: 80, HaloIntensity: 100));
        var wideBeam = Process(new EmulationVectorVideoConfiguration(
            LineThreshold: 10, LineIntensity: 80, BeamWidth: 100));
        var unfocusedBeam = Process(new EmulationVectorVideoConfiguration(
            LineThreshold: 10, LineIntensity: 80, BeamFocus: 0));
        var greenPhosphor = Process(new EmulationVectorVideoConfiguration(
            LineThreshold: 10, LineIntensity: 80,
            PhosphorColor: EmulationCrtColorMode.Green));
        var broadHalo = Process(new EmulationVectorVideoConfiguration(
            LineThreshold: 10, LineIntensity: 80, HaloIntensity: 100,
            HaloRadius: 100));
        Assert.Equal(pixels.Where((_, index) => index % 4 != 3),
            neutral.Where((_, index) => index % 4 != 3));
        Assert.False(neutral.SequenceEqual(lowThreshold));
        Assert.False(lowThreshold.SequenceEqual(highThreshold));
        Assert.False(lowThreshold.SequenceEqual(halo));
        Assert.False(lowThreshold.SequenceEqual(wideBeam));
        Assert.False(lowThreshold.SequenceEqual(unfocusedBeam));
        Assert.False(lowThreshold.SequenceEqual(greenPhosphor));
        Assert.False(halo.SequenceEqual(broadHalo));

        static VideoFrame Pixel(byte value, long sequence) => new(
            new byte[] { value, value, value, 0 }, 1, 1, 4,
            EmulationPixelFormat.Xrgb8888, 1f, sequence, TimeSpan.Zero);
        var configuration = new EmulationVideoProcessingConfiguration
        {
            DisplayTechnology = EmulationVideoDisplayTechnology.Vector,
            Vector = new EmulationVectorVideoConfiguration(PersistenceIntensity: 50)
        };
        using var history = new SoftwareEmulationVideoProcessingPipeline();
        _ = history.Process(configuration, Pixel(255, 1),
            new EmulationVideoProcessingSize(1, 1), new EmulationVideoProcessingSize(1, 1));
        var retained = history.Process(configuration, Pixel(0, 2),
            new EmulationVideoProcessingSize(1, 1), new EmulationVideoProcessingSize(1, 1));
        Assert.InRange(EmulationVideoPixelFunctions.ToBgra32(retained)[0], 187, 189);
        var resized = history.Process(configuration, Pixel(0, 3),
            new EmulationVideoProcessingSize(1, 1), new EmulationVideoProcessingSize(2, 2));
        Assert.Equal(0, EmulationVideoPixelFunctions.ToBgra32(resized)[0]);
        _ = history.Process(new EmulationVideoProcessingConfiguration(), Pixel(0, 4),
            new EmulationVideoProcessingSize(1, 1), new EmulationVideoProcessingSize(1, 1));
        var reset = history.Process(configuration, Pixel(0, 5),
            new EmulationVideoProcessingSize(1, 1), new EmulationVideoProcessingSize(1, 1));
        Assert.Equal(0, EmulationVideoPixelFunctions.ToBgra32(reset)[0]);
    }

    [Theory]
    [InlineData(EmulationVideoRenderer.Wpf)]
    [InlineData(EmulationVideoRenderer.OpenGL)]
    [InlineData(EmulationVideoRenderer.Direct3D11)]
    [InlineData(EmulationVideoRenderer.Vulkan)]
    public void NeutralPipelinePreservesMetadataAndRendererAcrossOutputSizes(
        EmulationVideoRenderer renderer)
    {
        var pixels = new byte[]
        {
            1, 2, 3, 0, 4, 5, 6, 0,
            7, 8, 9, 0, 10, 11, 12, 0
        };
        var frame = new VideoFrame(pixels, 2, 2, 8, EmulationPixelFormat.Xrgb8888,
            4f / 3f, 42, TimeSpan.FromMilliseconds(125));
        using var pipeline = EmulationVideoProcessingPipelineFactory.Create(renderer);

        var output = pipeline.Process(new EmulationVideoProcessingConfiguration(), frame,
            new EmulationVideoProcessingSize(2, 2),
            new EmulationVideoProcessingSize(317, 199));

        Assert.Equal(renderer, pipeline.Renderer);
        Assert.Equal(frame.AspectRatio, output.AspectRatio);
        Assert.Equal((frame.Sequence, frame.Timestamp), (output.Sequence, output.Timestamp));
        if (renderer == EmulationVideoRenderer.Wpf)
        {
            Assert.NotSame(frame, output);
            Assert.Equal((317, 199), (output.Width, output.Height));
        }
        else
        {
            Assert.Same(frame, output);
            Assert.True(frame.Pixels.Span.SequenceEqual(output.Pixels.Span));
            Assert.Equal((frame.Width, frame.Height), (output.Width, output.Height));
        }
    }

    [Theory]
    [InlineData(EmulationVideoSampling.Nearest)]
    [InlineData(EmulationVideoSampling.Bilinear)]
    [InlineData(EmulationVideoSampling.SharpBilinear)]
    [InlineData(EmulationVideoSampling.Bicubic)]
    [InlineData(EmulationVideoSampling.Xbr)]
    [InlineData(EmulationVideoSampling.Xbrz)]
    [InlineData(EmulationVideoSampling.Hqx)]
    [InlineData(EmulationVideoSampling.ScaleFx)]
    [InlineData(EmulationVideoSampling.ScaleNx)]
    [InlineData(EmulationVideoSampling.Sabr)]
    [InlineData(EmulationVideoSampling.Hq2x)]
    [InlineData(EmulationVideoSampling.Hq3x)]
    [InlineData(EmulationVideoSampling.Hq4x)]
    [InlineData(EmulationVideoSampling.TwoXSai)]
    [InlineData(EmulationVideoSampling.SuperTwoXSai)]
    [InlineData(EmulationVideoSampling.SuperEagle)]
    [InlineData(EmulationVideoSampling.EpxScale2x)]
    [InlineData(EmulationVideoSampling.Jinc2)]
    [InlineData(EmulationVideoSampling.Lanczos)]
    public void CpuSamplingIsDeterministicAtNonIntegerScale(EmulationVideoSampling sampling)
    {
        var frame = new VideoFrame(new byte[]
        {
            0, 0, 0, 0, 40, 20, 80, 0, 90, 50, 130, 0,
            20, 60, 100, 0, 80, 120, 160, 0, 140, 180, 220, 0
        }, 3, 2, 12, EmulationPixelFormat.Xrgb8888, 3f / 2f, 9, TimeSpan.Zero);
        var configuration = new EmulationVideoProcessingConfiguration { Sampling = sampling };
        using var pipeline = new SoftwareEmulationVideoProcessingPipeline();

        var first = pipeline.Process(configuration, frame,
            new EmulationVideoProcessingSize(3, 2), new EmulationVideoProcessingSize(5, 4));
        var second = pipeline.Process(configuration, frame,
            new EmulationVideoProcessingSize(3, 2), new EmulationVideoProcessingSize(5, 4));

        Assert.Equal((5, 4, 20), (first.Width, first.Height, first.Pitch));
        Assert.True(first.Pixels.Span.SequenceEqual(second.Pixels.Span));
        for (var index = 3; index < first.Pixels.Length; index += 4)
            Assert.Equal(255, first.Pixels.Span[index]);
    }

    [Fact]
    public void XbrLevelOneSmoothsDiagonalCornersAndIsNeutralAtOneToOneScale()
    {
        var pixels = new byte[]
        {
            0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 255, 0,
            0, 0, 0, 0, 255, 255, 255, 0, 255, 255, 255, 0,
            255, 255, 255, 0, 255, 255, 255, 0, 255, 255, 255, 0
        };
        var frame = new VideoFrame(pixels, 3, 3, 12, EmulationPixelFormat.Xrgb8888,
            1f, 1, TimeSpan.Zero);
        using var pipeline = new SoftwareEmulationVideoProcessingPipeline();
        var xbr = new EmulationVideoProcessingConfiguration
        {
            Sampling = EmulationVideoSampling.Xbr
        };
        var nearest = xbr with { Sampling = EmulationVideoSampling.Nearest };

        var scaled = pipeline.Process(xbr, frame, new(3, 3), new(9, 9));
        var repeated = pipeline.Process(xbr, frame, new(3, 3), new(9, 9));
        var blocky = pipeline.Process(nearest, frame, new(3, 3), new(9, 9));
        var unchanged = pipeline.Process(xbr, frame, new(3, 3), new(3, 3));

        Assert.True(scaled.Pixels.Span.SequenceEqual(repeated.Pixels.Span));
        Assert.False(scaled.Pixels.Span.SequenceEqual(blocky.Pixels.Span));
        Assert.Contains(scaled.Pixels.ToArray().Chunk(4), color => color[0] is > 0 and < 255);
        Assert.True(frame.Pixels.Span.SequenceEqual(unchanged.Pixels.Span));
    }

    [Fact]
    public void XbrzClassifiesStrongCornersAndIsNeutralAtOneToOneScale()
    {
        var pixels = new byte[]
        {
            0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 255, 0,
            0, 0, 0, 0, 255, 255, 255, 0, 255, 255, 255, 0,
            255, 255, 255, 0, 255, 255, 255, 0, 255, 255, 255, 0
        };
        var frame = new VideoFrame(pixels, 3, 3, 12, EmulationPixelFormat.Xrgb8888,
            1f, 1, TimeSpan.Zero);
        using var pipeline = new SoftwareEmulationVideoProcessingPipeline();
        var xbrz = new EmulationVideoProcessingConfiguration
        {
            Sampling = EmulationVideoSampling.Xbrz
        };
        var nearest = xbrz with { Sampling = EmulationVideoSampling.Nearest };

        var scaled = pipeline.Process(xbrz, frame, new(3, 3), new(9, 9));
        var repeated = pipeline.Process(xbrz, frame, new(3, 3), new(9, 9));
        var blocky = pipeline.Process(nearest, frame, new(3, 3), new(9, 9));
        var unchanged = pipeline.Process(xbrz, frame, new(3, 3), new(3, 3));

        Assert.True(scaled.Pixels.Span.SequenceEqual(repeated.Pixels.Span));
        Assert.False(scaled.Pixels.Span.SequenceEqual(blocky.Pixels.Span));
        Assert.Contains(scaled.Pixels.ToArray().Chunk(4), color => color[0] is > 0 and < 255);
        Assert.True(frame.Pixels.Span.SequenceEqual(unchanged.Pixels.Span));
    }

    [Fact]
    public void HqxUsesNeighborhoodPatternsAndIsNeutralAtOneToOneScale()
    {
        var pixels = new byte[]
        {
            0,0,0,0, 0,0,0,0, 255,255,255,0,
            0,0,0,0, 255,255,255,0, 255,255,255,0,
            255,255,255,0, 255,255,255,0, 255,255,255,0
        };
        var frame = new VideoFrame(pixels,3,3,12,EmulationPixelFormat.Xrgb8888,1f,1,TimeSpan.Zero);
        using var pipeline = new SoftwareEmulationVideoProcessingPipeline();
        var hqx = new EmulationVideoProcessingConfiguration { Sampling = EmulationVideoSampling.Hqx };
        var nearest = hqx with { Sampling = EmulationVideoSampling.Nearest };
        var scaled = pipeline.Process(hqx,frame,new(3,3),new(9,9));
        var blocky = pipeline.Process(nearest,frame,new(3,3),new(9,9));
        var unchanged = pipeline.Process(hqx,frame,new(3,3),new(3,3));
        Assert.False(scaled.Pixels.Span.SequenceEqual(blocky.Pixels.Span));
        Assert.Contains(scaled.Pixels.ToArray().Chunk(4), color => color[0] is > 0 and < 255);
        Assert.True(frame.Pixels.Span.SequenceEqual(unchanged.Pixels.Span));
    }

    [Fact]
    public void ScaleFxReconstructsDiagonalWithSourcePaletteAndIsNeutralAtOneToOneScale()
    {
        var pixels = new byte[]
        {
            0,0,0,0, 0,0,0,0, 255,255,255,0,
            0,0,0,0, 255,255,255,0, 255,255,255,0,
            255,255,255,0, 255,255,255,0, 255,255,255,0
        };
        var frame = new VideoFrame(pixels, 3, 3, 12, EmulationPixelFormat.Xrgb8888,
            1f, 1, TimeSpan.Zero);
        using var pipeline = new SoftwareEmulationVideoProcessingPipeline();
        var scaleFx = new EmulationVideoProcessingConfiguration
        {
            Sampling = EmulationVideoSampling.ScaleFx
        };
        var nearest = scaleFx with { Sampling = EmulationVideoSampling.Nearest };

        var scaled = pipeline.Process(scaleFx, frame, new(3, 3), new(9, 9));
        var repeated = pipeline.Process(scaleFx, frame, new(3, 3), new(9, 9));
        var blocky = pipeline.Process(nearest, frame, new(3, 3), new(9, 9));
        var unchanged = pipeline.Process(scaleFx, frame, new(3, 3), new(3, 3));

        Assert.True(scaled.Pixels.Span.SequenceEqual(repeated.Pixels.Span));
        Assert.False(scaled.Pixels.Span.SequenceEqual(blocky.Pixels.Span));
        Assert.All(scaled.Pixels.ToArray().Chunk(4), color =>
            Assert.True(color[0] is 0 or 255 && color[1] is 0 or 255 && color[2] is 0 or 255));
        Assert.True(frame.Pixels.Span.SequenceEqual(unchanged.Pixels.Span));
    }

    [Fact]
    public void ScaleNxAppliesTwoAndThreeTimesRulesWithoutChangingThePalette()
    {
        var pixels = new byte[]
        {
            0,0,0,0, 0,0,0,0, 255,255,255,0,
            0,0,0,0, 255,255,255,0, 255,255,255,0,
            255,255,255,0, 255,255,255,0, 255,255,255,0
        };
        var frame = new VideoFrame(pixels, 3, 3, 12, EmulationPixelFormat.Xrgb8888,
            1f, 1, TimeSpan.Zero);
        using var pipeline = new SoftwareEmulationVideoProcessingPipeline();
        var scaleNx = new EmulationVideoProcessingConfiguration
        {
            Sampling = EmulationVideoSampling.ScaleNx
        };
        var nearest = scaleNx with { Sampling = EmulationVideoSampling.Nearest };

        var scaled2x = pipeline.Process(scaleNx, frame, new(3, 3), new(6, 6));
        var scaled3x = pipeline.Process(scaleNx, frame, new(3, 3), new(9, 9));
        var repeated3x = pipeline.Process(scaleNx, frame, new(3, 3), new(9, 9));
        var nearest2x = pipeline.Process(nearest, frame, new(3, 3), new(6, 6));
        var nearest3x = pipeline.Process(nearest, frame, new(3, 3), new(9, 9));
        var unchanged = pipeline.Process(scaleNx, frame, new(3, 3), new(3, 3));

        Assert.False(scaled2x.Pixels.Span.SequenceEqual(nearest2x.Pixels.Span));
        Assert.False(scaled3x.Pixels.Span.SequenceEqual(nearest3x.Pixels.Span));
        Assert.True(scaled3x.Pixels.Span.SequenceEqual(repeated3x.Pixels.Span));
        Assert.All(scaled2x.Pixels.ToArray().Concat(scaled3x.Pixels.ToArray()).Chunk(4), color =>
            Assert.True(color[0] is 0 or 255 && color[1] is 0 or 255 && color[2] is 0 or 255));
        Assert.True(frame.Pixels.Span.SequenceEqual(unchanged.Pixels.Span));
    }

    [Fact]
    public void SabrAntialiasesDiagonalCornersAndIsNeutralAtOneToOneScale()
    {
        var pixels = new byte[]
        {
            0,0,0,0, 0,0,0,0, 255,255,255,0,
            0,0,0,0, 255,255,255,0, 255,255,255,0,
            255,255,255,0, 255,255,255,0, 255,255,255,0
        };
        var frame = new VideoFrame(pixels, 3, 3, 12, EmulationPixelFormat.Xrgb8888,
            1f, 1, TimeSpan.Zero);
        using var pipeline = new SoftwareEmulationVideoProcessingPipeline();
        var sabr = new EmulationVideoProcessingConfiguration
        {
            Sampling = EmulationVideoSampling.Sabr
        };
        var nearest = sabr with { Sampling = EmulationVideoSampling.Nearest };

        var scaled = pipeline.Process(sabr, frame, new(3, 3), new(9, 9));
        var repeated = pipeline.Process(sabr, frame, new(3, 3), new(9, 9));
        var blocky = pipeline.Process(nearest, frame, new(3, 3), new(9, 9));
        var unchanged = pipeline.Process(sabr, frame, new(3, 3), new(3, 3));

        Assert.True(scaled.Pixels.Span.SequenceEqual(repeated.Pixels.Span));
        Assert.False(scaled.Pixels.Span.SequenceEqual(blocky.Pixels.Span));
        Assert.Contains(scaled.Pixels.ToArray().Chunk(4), color => color[0] is > 0 and < 255);
        Assert.True(frame.Pixels.Span.SequenceEqual(unchanged.Pixels.Span));
    }

    [Fact]
    public void DeditheringBlendsOnlyCheckerboardsBeforeScalingAndIsNeutralAtZero()
    {
        var checkerboard = CheckerboardFrame(4, 4, 80, 120);
        var uniform = CheckerboardFrame(4, 4, 100, 100);
        using var pipeline = new SoftwareEmulationVideoProcessingPipeline();
        var active = new EmulationVideoProcessingConfiguration
        {
            Sampling = EmulationVideoSampling.Nearest,
            Restoration = new EmulationImageRestorationConfiguration(Dedithering: 100)
        };
        var neutral = active with
        {
            Restoration = new EmulationImageRestorationConfiguration(Dedithering: 0)
        };

        var restored = pipeline.Process(active, checkerboard, new(4, 4), new(4, 4));
        var scaled = pipeline.Process(active, checkerboard, new(4, 4), new(8, 8));
        var untouched = pipeline.Process(neutral, checkerboard, new(4, 4), new(4, 4));
        var flat = pipeline.Process(active, uniform, new(4, 4), new(4, 4));

        Assert.False(checkerboard.Pixels.Span.SequenceEqual(restored.Pixels.Span));
        Assert.Contains(restored.Pixels.ToArray().Chunk(4), color => color[0] is > 80 and < 120);
        Assert.True(checkerboard.Pixels.Span.SequenceEqual(untouched.Pixels.Span));
        Assert.Equal(uniform.Pixels.ToArray().Where((_, index) => index % 4 != 3),
            flat.Pixels.ToArray().Where((_, index) => index % 4 != 3));
        for (var y = 0; y < 8; y++)
        for (var x = 0; x < 8; x++)
        {
            var sourceOffset = ((y / 2) * 4 + x / 2) * 4;
            var outputOffset = (y * 8 + x) * 4;
            Assert.Equal(restored.Pixels.Span[sourceOffset], scaled.Pixels.Span[outputOffset]);
        }
    }

    [Fact]
    public void DenoisingReducesSmallNoisePreservesHardEdgesAndRunsBeforeScaling()
    {
        var frame = NoisyEdgeFrame(5, 5);
        using var pipeline = new SoftwareEmulationVideoProcessingPipeline();
        var active = new EmulationVideoProcessingConfiguration
        {
            Sampling = EmulationVideoSampling.Nearest,
            Restoration = new EmulationImageRestorationConfiguration(Denoising: 100)
        };
        var neutral = active with
        {
            Restoration = new EmulationImageRestorationConfiguration(Denoising: 0)
        };

        var restored = pipeline.Process(active, frame, new(5, 5), new(5, 5));
        var scaled = pipeline.Process(active, frame, new(5, 5), new(10, 10));
        var untouched = pipeline.Process(neutral, frame, new(5, 5), new(5, 5));
        var restoredPixels = restored.Pixels.Span;

        Assert.InRange(restoredPixels[(2 * 5 + 1) * 4], 61, 79);
        Assert.Equal(200, restoredPixels[(2 * 5 + 2) * 4]);
        Assert.True(frame.Pixels.Span.SequenceEqual(untouched.Pixels.Span));
        for (var y = 0; y < 10; y++)
        for (var x = 0; x < 10; x++)
        {
            var sourceOffset = ((y / 2) * 5 + x / 2) * 4;
            var outputOffset = (y * 10 + x) * 4;
            Assert.Equal(restoredPixels[sourceOffset], scaled.Pixels.Span[outputOffset]);
        }
    }

    [Fact]
    public void DebandingSmoothsSmallGradientStepsPreservesHardEdgesAndRunsBeforeScaling()
    {
        var frame = BandedGradientFrame(5, 5);
        using var pipeline = new SoftwareEmulationVideoProcessingPipeline();
        var active = new EmulationVideoProcessingConfiguration
        {
            Sampling = EmulationVideoSampling.Nearest,
            Restoration = new EmulationImageRestorationConfiguration(Debanding: 100)
        };
        var neutral = active with
        {
            Restoration = new EmulationImageRestorationConfiguration(Debanding: 0)
        };

        var restored = pipeline.Process(active, frame, new(5, 5), new(5, 5));
        var scaled = pipeline.Process(active, frame, new(5, 5), new(10, 10));
        var untouched = pipeline.Process(neutral, frame, new(5, 5), new(5, 5));
        var restoredPixels = restored.Pixels.Span;

        Assert.InRange(restoredPixels[(2 * 5 + 1) * 4], 81, 83);
        Assert.Equal(84, restoredPixels[(2 * 5 + 3) * 4]);
        Assert.Equal(220, restoredPixels[(2 * 5 + 4) * 4]);
        Assert.True(frame.Pixels.Span.SequenceEqual(untouched.Pixels.Span));
        for (var y = 0; y < 10; y++)
        for (var x = 0; x < 10; x++)
        {
            var sourceOffset = ((y / 2) * 5 + x / 2) * 4;
            var outputOffset = (y * 10 + x) * 4;
            Assert.Equal(restoredPixels[sourceOffset], scaled.Pixels.Span[outputOffset]);
        }
    }

    [Fact]
    public void DetailRecoveryBoostsSourceDetailProtectsHardEdgesAndRunsBeforeScaling()
    {
        var frame = DetailFrame(5, 5);
        using var pipeline = new SoftwareEmulationVideoProcessingPipeline();
        var active = new EmulationVideoProcessingConfiguration
        {
            Sampling = EmulationVideoSampling.Nearest,
            Restoration = new EmulationImageRestorationConfiguration(DetailRecovery: 100)
        };
        var neutral = active with
        {
            Restoration = new EmulationImageRestorationConfiguration(DetailRecovery: 0)
        };

        var restored = pipeline.Process(active, frame, new(5, 5), new(5, 5));
        var scaled = pipeline.Process(active, frame, new(5, 5), new(10, 10));
        var untouched = pipeline.Process(neutral, frame, new(5, 5), new(5, 5));
        var restoredPixels = restored.Pixels.Span;

        Assert.InRange(restoredPixels[(2 * 5 + 1) * 4], 111, 120);
        Assert.Equal(220, restoredPixels[(2 * 5 + 4) * 4]);
        Assert.True(frame.Pixels.Span.SequenceEqual(untouched.Pixels.Span));
        for (var y = 0; y < 10; y++)
        for (var x = 0; x < 10; x++)
        {
            var sourceOffset = ((y / 2) * 5 + x / 2) * 4;
            var outputOffset = (y * 10 + x) * 4;
            Assert.Equal(restoredPixels[sourceOffset], scaled.Pixels.Span[outputOffset]);
        }
    }

    [Fact]
    public void DeinterlacingSupportsBothBobFieldsAndBlendBeforeScaling()
    {
        var frame = InterlacedFrame(3, 5);
        using var pipeline = new SoftwareEmulationVideoProcessingPipeline();
        VideoFrame Process(EmulationDeinterlacingMode mode, int width = 3, int height = 5) =>
            pipeline.Process(new EmulationVideoProcessingConfiguration
            {
                Sampling = EmulationVideoSampling.Nearest,
                Restoration = new EmulationImageRestorationConfiguration(Deinterlacing: mode)
            }, frame, new(3, 5), new(width, height));

        var even = Process(EmulationDeinterlacingMode.BobEvenLines);
        var odd = Process(EmulationDeinterlacingMode.BobOddLines);
        var blend = Process(EmulationDeinterlacingMode.Blend);
        var neutral = Process(EmulationDeinterlacingMode.Off);
        var scaled = Process(EmulationDeinterlacingMode.BobEvenLines, 6, 10);

        Assert.InRange(even.Pixels.Span[(1 * 3 + 1) * 4], 29, 32);
        Assert.Equal(200, odd.Pixels.Span[(0 * 3 + 1) * 4]);
        Assert.InRange(blend.Pixels.Span[(2 * 3 + 1) * 4], 41, 179);
        Assert.True(frame.Pixels.Span.SequenceEqual(neutral.Pixels.Span));
        for (var y = 0; y < 10; y++)
        for (var x = 0; x < 6; x++)
        {
            var sourceOffset = ((y / 2) * 3 + x / 2) * 4;
            var outputOffset = (y * 6 + x) * 4;
            Assert.Equal(even.Pixels.Span[sourceOffset], scaled.Pixels.Span[outputOffset]);
        }
    }

    [Fact]
    public void AdvancedIndependentFiltersComposeWithoutClearingAnySetting()
    {
        var frame = DeterministicFrame(8, 8);
        var restoration = new EmulationImageRestorationConfiguration(Dedithering: 45,
            Denoising: 35, Debanding: 40, DetailRecovery: 30,
            Deinterlacing: EmulationDeinterlacingMode.Blend);
        using var pipeline = new SoftwareEmulationVideoProcessingPipeline();

        foreach (var sampling in Enum.GetValues<EmulationVideoSampling>())
        {
            var configuration = new EmulationVideoProcessingConfiguration
            {
                Sampling = sampling,
                Restoration = restoration
            };
            var first = pipeline.Process(configuration, frame, new(8, 8), new(16, 16));
            var second = pipeline.Process(configuration, frame, new(8, 8), new(16, 16));

            Assert.Equal(restoration,
                EmulationVideoProcessingConfigurationFunctions.Normalize(configuration).Restoration);
            Assert.True(first.Pixels.Span.SequenceEqual(second.Pixels.Span));
            Assert.Equal((16, 16), (first.Width, first.Height));
        }
    }

    [Fact]
    public void VfdColorsHaloAndPersistenceAreDistinctAndBounded()
    {
        var lit = new VideoFrame(new byte[]
        {
            0,0,0,0, 0,0,0,0, 0,0,0,0,
            0,0,0,0, 180,180,180,0, 0,0,0,0,
            0,0,0,0, 0,0,0,0, 0,0,0,0
        }, 3, 3, 12, EmulationPixelFormat.Xrgb8888, 1f, 1, TimeSpan.Zero);
        var dark = lit with
        {
            Pixels = new byte[36], Sequence = 2,
            Timestamp = TimeSpan.FromMilliseconds(16)
        };
        var hashes = new HashSet<string>(StringComparer.Ordinal);
        foreach (var color in Enum.GetValues<EmulationVfdColor>())
        {
            using var pipeline = new SoftwareEmulationVideoProcessingPipeline();
            var configuration = new EmulationVideoProcessingConfiguration
            {
                DisplayTechnology = EmulationVideoDisplayTechnology.Vfd,
                Vfd = new EmulationVfdVideoConfiguration(color,
                    PhosphorIntensity: 80, HaloIntensity: 100,
                    PersistenceMilliseconds: 200)
            };
            var first = EmulationVideoPixelFunctions.ToBgra32(pipeline.Process(configuration,
                lit, new(3, 3), new(3, 3)));
            var retained = EmulationVideoPixelFunctions.ToBgra32(pipeline.Process(configuration,
                dark, new(3, 3), new(3, 3)));
            hashes.Add(Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(first)));
            Assert.Contains(first.Chunk(4), pixel => pixel[0] > 0 || pixel[1] > 0 || pixel[2] > 0);
            Assert.Contains(retained.Chunk(4), pixel => pixel[0] > 0 || pixel[1] > 0 || pixel[2] > 0);
        }
        Assert.Equal(Enum.GetValues<EmulationVfdColor>().Length, hashes.Count);

        byte[] Render(EmulationVfdVideoConfiguration vfd,
            EmulationVideoProcessingSize? output = null)
        {
            using var pipeline = new SoftwareEmulationVideoProcessingPipeline();
            return EmulationVideoPixelFunctions.ToBgra32(pipeline.Process(
                new EmulationVideoProcessingConfiguration
                {
                    DisplayTechnology = EmulationVideoDisplayTechnology.Vfd,
                    Vfd = vfd
                }, lit, new(3, 3), output ?? new(3, 3)));
        }

        var baseVfd = new EmulationVfdVideoConfiguration(PhosphorIntensity: 100,
            EmissionThreshold: 10, GlassDarkening: 100, HaloIntensity: 0);
        Assert.False(Render(baseVfd).SequenceEqual(Render(baseVfd with { HaloIntensity = 100 })));
        Assert.False(Render(baseVfd).SequenceEqual(Render(baseVfd with { EmissionThreshold = 90 })));
        Assert.False(Render(baseVfd).SequenceEqual(Render(baseVfd with { GlassDarkening = 0 })));
        Assert.False(Render(baseVfd, new(6, 6)).SequenceEqual(Render(baseVfd with
        {
            Structure = EmulationVfdStructure.DotMatrix,
            CellSize = 35,
            CellGap = 60
        }, new(6, 6))));

        using var withoutPersistence = new SoftwareEmulationVideoProcessingPipeline();
        using var withPersistence = new SoftwareEmulationVideoProcessingPipeline();
        var noPersistence = new EmulationVideoProcessingConfiguration
        {
            DisplayTechnology = EmulationVideoDisplayTechnology.Vfd,
            Vfd = baseVfd with { PersistenceMilliseconds = 0 }
        };
        var persistent = noPersistence with
        {
            Vfd = baseVfd with { PersistenceMilliseconds = 250 }
        };
        _ = withoutPersistence.Process(noPersistence, lit, new(3, 3), new(3, 3));
        _ = withPersistence.Process(persistent, lit, new(3, 3), new(3, 3));
        var darkWithout = EmulationVideoPixelFunctions.ToBgra32(withoutPersistence.Process(
            noPersistence, dark, new(3, 3), new(3, 3)));
        var darkWith = EmulationVideoPixelFunctions.ToBgra32(withPersistence.Process(
            persistent, dark, new(3, 3), new(3, 3)));
        Assert.True(darkWith.Sum(value => (long)value) > darkWithout.Sum(value => (long)value));
    }

    [Fact]
    public void LedMatrixOptionsAreIndependentDistinctAndBounded()
    {
        var pixels = Enumerable.Range(0, 12 * 12).SelectMany(index => new byte[]
        {
            (byte)(index * 17 % 256), (byte)(index * 29 % 256),
            (byte)(index * 43 % 256), 0
        }).ToArray();
        var frame = new VideoFrame(pixels, 12, 12, 48, EmulationPixelFormat.Xrgb8888,
            1f, 1, TimeSpan.Zero);
        var hashes = new HashSet<string>(StringComparer.Ordinal);
        foreach (var color in Enum.GetValues<EmulationLedMatrixColor>())
        {
            using var pipeline = new SoftwareEmulationVideoProcessingPipeline();
            var configuration = new EmulationVideoProcessingConfiguration
            {
                DisplayTechnology = EmulationVideoDisplayTechnology.LedMatrix,
                LedMatrix = new EmulationLedMatrixVideoConfiguration(color,
                    CellSize: 70, CellGap: 55, Diffusion: 35, Brightness: 90)
            };
            var output = EmulationVideoPixelFunctions.ToBgra32(pipeline.Process(configuration,
                frame, new(12, 12), new(12, 12)));
            hashes.Add(Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(output)));
            Assert.Contains(output.Chunk(4), pixel => pixel[0] > 0 || pixel[1] > 0 || pixel[2] > 0);
        }
        Assert.Equal(Enum.GetValues<EmulationLedMatrixColor>().Length, hashes.Count);

        using var comparisonPipeline = new SoftwareEmulationVideoProcessingPipeline();
        byte[] Render(EmulationLedMatrixVideoConfiguration ledMatrix) =>
            EmulationVideoPixelFunctions.ToBgra32(comparisonPipeline.Process(
                new EmulationVideoProcessingConfiguration
                {
                    DisplayTechnology = EmulationVideoDisplayTechnology.LedMatrix,
                    LedMatrix = ledMatrix
                }, frame, new(12, 12), new(12, 12)));
        var compact = Render(new(CellSize: 0, CellGap: 0, Diffusion: 0, Brightness: 25));
        var spaced = Render(new(CellSize: 100, CellGap: 100, Diffusion: 100,
            Brightness: 100, Shape: EmulationLedMatrixShape.Square,
            HaloRadius: 100, BlackDepth: 100));
        Assert.False(compact.SequenceEqual(spaced));

        var baseline = new EmulationLedMatrixVideoConfiguration(CellSize: 45, CellGap: 35,
            Diffusion: 55, Brightness: 80, Shape: EmulationLedMatrixShape.Round,
            HaloRadius: 30, BlackDepth: 90);
        var variants = new[]
        {
            baseline, baseline with { CellSize = 80 }, baseline with { CellGap = 75 },
            baseline with { Diffusion = 90 }, baseline with { Brightness = 30 },
            baseline with { Shape = EmulationLedMatrixShape.Square },
            baseline with { HaloRadius = 80 }, baseline with { BlackDepth = 20 }
        };
        Assert.Equal(variants.Length, variants.Select(value => Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(Render(value)))).Distinct().Count());
    }

    [Fact]
    public void DotMatrixPalettesShapesContrastAndResponseAreDistinctAndBounded()
    {
        var bright = new VideoFrame(Enumerable.Repeat((byte)255, 12 * 12 * 4).ToArray(),
            12, 12, 48, EmulationPixelFormat.Xrgb8888, 1f, 1, TimeSpan.Zero);
        var colored = bright with
        {
            Pixels = Enumerable.Range(0, 12 * 12).SelectMany(index => new byte[]
            {
                (byte)(index * 17 % 256), (byte)(index * 43 % 256),
                (byte)(index * 79 % 256), 0
            }).ToArray()
        };
        var dark = bright with
        {
            Pixels = new byte[12 * 12 * 4], Sequence = 2,
            Timestamp = TimeSpan.FromMilliseconds(16)
        };
        var hashes = new HashSet<string>(StringComparer.Ordinal);
        foreach (var palette in Enum.GetValues<EmulationDotMatrixPalette>())
        {
            using var pipeline = new SoftwareEmulationVideoProcessingPipeline();
            var configuration = new EmulationVideoProcessingConfiguration
            {
                DisplayTechnology = EmulationVideoDisplayTechnology.DotMatrix,
                DotMatrix = new EmulationDotMatrixVideoConfiguration(palette,
                    EmulationDotMatrixShape.Round, DotSize: 55, Contrast: 70,
                    ResponseTimeMilliseconds: 0)
            };
            var output = EmulationVideoPixelFunctions.ToBgra32(pipeline.Process(configuration,
                colored, new(12, 12), new(12, 12)));
            hashes.Add(Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(output)));
        }
        Assert.Equal(Enum.GetValues<EmulationDotMatrixPalette>().Length, hashes.Count);

        byte[] Render(EmulationDotMatrixShape shape, int size, int contrast)
        {
            using var pipeline = new SoftwareEmulationVideoProcessingPipeline();
            return EmulationVideoPixelFunctions.ToBgra32(pipeline.Process(
                new EmulationVideoProcessingConfiguration
                {
                    DisplayTechnology = EmulationVideoDisplayTechnology.DotMatrix,
                    DotMatrix = new(EmulationDotMatrixPalette.Green, shape,
                        size, contrast, ResponseTimeMilliseconds: 0)
                }, bright, new(12, 12), new(12, 12)));
        }
        Assert.False(Render(EmulationDotMatrixShape.Round, 20, 20)
            .SequenceEqual(Render(EmulationDotMatrixShape.Square, 100, 100)));

        using var slowPipeline = new SoftwareEmulationVideoProcessingPipeline();
        using var immediatePipeline = new SoftwareEmulationVideoProcessingPipeline();
        EmulationVideoProcessingConfiguration Configuration(int response) => new()
        {
            DisplayTechnology = EmulationVideoDisplayTechnology.DotMatrix,
            DotMatrix = new(ResponseTimeMilliseconds: response)
        };
        slowPipeline.Process(Configuration(1000), bright, new(12, 12), new(12, 12));
        immediatePipeline.Process(Configuration(0), bright, new(12, 12), new(12, 12));
        var slow = EmulationVideoPixelFunctions.ToBgra32(slowPipeline.Process(
            Configuration(1000), dark, new(12, 12), new(12, 12)));
        var immediate = EmulationVideoPixelFunctions.ToBgra32(immediatePipeline.Process(
            Configuration(0), dark, new(12, 12), new(12, 12)));
        Assert.False(slow.SequenceEqual(immediate));

        using var persistentPipeline = new SoftwareEmulationVideoProcessingPipeline();
        using var noPersistencePipeline = new SoftwareEmulationVideoProcessingPipeline();
        var persistentConfiguration = Configuration(0) with
        {
            DotMatrix = new(ResponseTimeMilliseconds: 0,
                PersistenceMilliseconds: 1000)
        };
        persistentPipeline.Process(persistentConfiguration, bright, new(12, 12), new(12, 12));
        noPersistencePipeline.Process(Configuration(0), bright, new(12, 12), new(12, 12));
        var persistent = EmulationVideoPixelFunctions.ToBgra32(persistentPipeline.Process(
            persistentConfiguration, dark, new(12, 12), new(12, 12)));
        var withoutPersistence = EmulationVideoPixelFunctions.ToBgra32(
            noPersistencePipeline.Process(Configuration(0), dark, new(12, 12), new(12, 12)));
        Assert.False(persistent.SequenceEqual(withoutPersistence));
    }

    [Fact]
    public void EveryDotMatrixSpatialAndLightOptionChangesTheLogicalCellRendering()
    {
        var pixels = Enumerable.Range(0, 24 * 18).SelectMany(index => new byte[]
        {
            (byte)(index * 13 % 256), (byte)(index * 29 % 256),
            (byte)(index * 61 % 256), 0
        }).ToArray();
        var frame = new VideoFrame(pixels, 24, 18, 96, EmulationPixelFormat.Xrgb8888,
            1f, 1, TimeSpan.Zero);
        var baseline = new EmulationDotMatrixVideoConfiguration(
            EmulationDotMatrixPalette.Rgb, EmulationDotMatrixShape.Round,
            DotSize: 55, Contrast: 70, ResponseTimeMilliseconds: 0,
            CellSize: 25, CellGap: 20, Brightness: 80, HaloIntensity: 15);
        byte[] Render(EmulationDotMatrixVideoConfiguration value)
        {
            using var pipeline = new SoftwareEmulationVideoProcessingPipeline();
            return EmulationVideoPixelFunctions.ToBgra32(pipeline.Process(
                new EmulationVideoProcessingConfiguration
                {
                    DisplayTechnology = EmulationVideoDisplayTechnology.DotMatrix,
                    DotMatrix = value
                }, frame, new(24, 18), new(96, 72)));
        }
        var variants = new[]
        {
            baseline, baseline with { CellSize = 80 }, baseline with { DotSize = 90 },
            baseline with { CellGap = 75 }, baseline with { Contrast = 20 },
            baseline with { Brightness = 25 }, baseline with { HaloIntensity = 90 },
            baseline with { Shape = EmulationDotMatrixShape.Rectangle }
        };
        Assert.Equal(variants.Length, variants.Select(value => Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(Render(value)))).Distinct().Count());
    }

    [Fact]
    public void SegmentDisplayLayoutsColorsGeometryGlowAndResponseAreDistinctAndBounded()
    {
        var pixels = Enumerable.Range(0, 24 * 24).SelectMany(index => new byte[]
        {
            (byte)(index * 11 % 256), (byte)(index * 31 % 256),
            (byte)(index * 47 % 256), 0
        }).ToArray();
        var frame = new VideoFrame(pixels, 24, 24, 96, EmulationPixelFormat.Xrgb8888,
            1f, 1, TimeSpan.Zero);
        byte[] Render(EmulationSegmentDisplayVideoConfiguration segmentDisplay)
        {
            using var pipeline = new SoftwareEmulationVideoProcessingPipeline();
            return EmulationVideoPixelFunctions.ToBgra32(pipeline.Process(
                new EmulationVideoProcessingConfiguration
                {
                    DisplayTechnology = EmulationVideoDisplayTechnology.SegmentDisplay,
                    SegmentDisplay = segmentDisplay
                }, frame, new(24, 24), new(96, 72)));
        }

        var colorHashes = Enum.GetValues<EmulationSegmentDisplayColor>()
            .Select(color => Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
                Render(new(Color: color, ResponseTimeMilliseconds: 0)))))
            .ToHashSet(StringComparer.Ordinal);
        Assert.Equal(Enum.GetValues<EmulationSegmentDisplayColor>().Length, colorHashes.Count);

        var layoutHashes = Enum.GetValues<EmulationSegmentDisplayLayout>()
            .Select(layout => Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
                Render(new(Layout: layout, ResponseTimeMilliseconds: 0)))))
            .ToHashSet(StringComparer.Ordinal);
        Assert.Equal(Enum.GetValues<EmulationSegmentDisplayLayout>().Length, layoutHashes.Count);
        Assert.False(Render(new(Thickness: 0, Contrast: 0, Glow: 0,
                ResponseTimeMilliseconds: 0)).SequenceEqual(
            Render(new(Thickness: 100, Contrast: 100, Glow: 100,
                ResponseTimeMilliseconds: 0))));
        Assert.False(Render(new(CellSize: 0, HorizontalGap: 0, VerticalGap: 0,
                SegmentGap: 0, Brightness: 20, ActivationThreshold: 20,
                OffSegmentVisibility: 0, BlackDepth: 100, HaloRadius: 0,
                ResponseTimeMilliseconds: 0, PersistenceMilliseconds: 0)).SequenceEqual(
            Render(new(CellSize: 100, HorizontalGap: 100, VerticalGap: 100,
                SegmentGap: 100, Brightness: 100, ActivationThreshold: 80,
                OffSegmentVisibility: 100, BlackDepth: 0, HaloRadius: 100,
                ResponseTimeMilliseconds: 0, PersistenceMilliseconds: 0))));

        var baseline = new EmulationSegmentDisplayVideoConfiguration(
            Layout: EmulationSegmentDisplayLayout.Sixteen,
            Color: EmulationSegmentDisplayColor.Red,
            Thickness: 55, Contrast: 65, Glow: 45,
            ResponseTimeMilliseconds: 0, CellSize: 45,
            HorizontalGap: 20, VerticalGap: 20, SegmentGap: 15,
            EndShape: EmulationSegmentEndShape.Beveled,
            DecimalPoint: false, Colon: false, Brightness: 75,
            ActivationThreshold: 45, OffSegmentVisibility: 12,
            BlackDepth: 85, HaloRadius: 35, PersistenceMilliseconds: 0);
        var baselinePixels = Render(baseline);
        void AssertOptionChangesImage(string option,
            EmulationSegmentDisplayVideoConfiguration changed)
        {
            Assert.False(baselinePixels.SequenceEqual(Render(changed)),
                $"Segment-display option {option} did not change the rendered image.");
        }
        AssertOptionChangesImage(nameof(baseline.Layout), baseline with
            { Layout = EmulationSegmentDisplayLayout.Seven });
        AssertOptionChangesImage(nameof(baseline.Color), baseline with
            { Color = EmulationSegmentDisplayColor.Green });
        AssertOptionChangesImage(nameof(baseline.Thickness), baseline with { Thickness = 85 });
        AssertOptionChangesImage(nameof(baseline.Contrast), baseline with { Contrast = 95 });
        AssertOptionChangesImage(nameof(baseline.Glow), baseline with { Glow = 5 });
        AssertOptionChangesImage(nameof(baseline.CellSize), baseline with { CellSize = 80 });
        AssertOptionChangesImage(nameof(baseline.HorizontalGap), baseline with
            { HorizontalGap = 75 });
        AssertOptionChangesImage(nameof(baseline.VerticalGap), baseline with
            { VerticalGap = 75 });
        AssertOptionChangesImage(nameof(baseline.SegmentGap), baseline with { SegmentGap = 75 });
        AssertOptionChangesImage(nameof(baseline.EndShape), baseline with
            { EndShape = EmulationSegmentEndShape.Rounded });
        AssertOptionChangesImage(nameof(baseline.DecimalPoint), baseline with
            { DecimalPoint = true });
        AssertOptionChangesImage(nameof(baseline.Colon), baseline with { Colon = true });
        AssertOptionChangesImage(nameof(baseline.Brightness), baseline with { Brightness = 25 });
        AssertOptionChangesImage(nameof(baseline.ActivationThreshold), baseline with
            { ActivationThreshold = 80 });
        AssertOptionChangesImage(nameof(baseline.OffSegmentVisibility), baseline with
            { OffSegmentVisibility = 70 });
        AssertOptionChangesImage(nameof(baseline.BlackDepth), baseline with { BlackDepth = 25 });
        AssertOptionChangesImage(nameof(baseline.HaloRadius), baseline with { HaloRadius = 80 });

        var dark = frame with
        {
            Pixels = new byte[24 * 24 * 4], Sequence = 2,
            Timestamp = TimeSpan.FromMilliseconds(16)
        };
        EmulationVideoProcessingConfiguration Configuration(int response, int persistence) => new()
        {
            DisplayTechnology = EmulationVideoDisplayTechnology.SegmentDisplay,
            SegmentDisplay = new(ResponseTimeMilliseconds: response,
                PersistenceMilliseconds: persistence)
        };

        using var persistentPipeline = new SoftwareEmulationVideoProcessingPipeline();
        using var noPersistencePipeline = new SoftwareEmulationVideoProcessingPipeline();
        persistentPipeline.Process(Configuration(0, 1000), frame, new(24, 24), new(24, 24));
        noPersistencePipeline.Process(Configuration(0, 0), frame, new(24, 24), new(24, 24));
        var persistent = EmulationVideoPixelFunctions.ToBgra32(persistentPipeline.Process(
            Configuration(0, 1000), dark, new(24, 24), new(24, 24)));
        var noPersistence = EmulationVideoPixelFunctions.ToBgra32(noPersistencePipeline.Process(
            Configuration(0, 0), dark, new(24, 24), new(24, 24)));
        Assert.False(persistent.SequenceEqual(noPersistence));

        var initialDark = dark with { Sequence = 1, Timestamp = TimeSpan.Zero };
        var brightAfterDark = frame with
        {
            Sequence = 2, Timestamp = TimeSpan.FromMilliseconds(16)
        };
        using var slowResponsePipeline = new SoftwareEmulationVideoProcessingPipeline();
        using var immediateResponsePipeline = new SoftwareEmulationVideoProcessingPipeline();
        slowResponsePipeline.Process(Configuration(1000, 0), initialDark,
            new(24, 24), new(24, 24));
        immediateResponsePipeline.Process(Configuration(0, 0), initialDark,
            new(24, 24), new(24, 24));
        var slowResponse = EmulationVideoPixelFunctions.ToBgra32(slowResponsePipeline.Process(
            Configuration(1000, 0), brightAfterDark, new(24, 24), new(24, 24)));
        var immediateResponse = EmulationVideoPixelFunctions.ToBgra32(
            immediateResponsePipeline.Process(Configuration(0, 0), brightAfterDark,
                new(24, 24), new(24, 24)));
        Assert.False(slowResponse.SequenceEqual(immediateResponse));
    }

    [Fact]
    public void EPaperOptionsAreIndividuallyDistinctAndBounded()
    {
        var pixels = Enumerable.Range(0, 16 * 16).SelectMany(index => new byte[]
        {
            (byte)(index * 13 % 256), (byte)(index * 37 % 256),
            (byte)(index * 59 % 256), 0
        }).ToArray();
        var frame = new VideoFrame(pixels, 16, 16, 64, EmulationPixelFormat.Xrgb8888,
            1f, 1, TimeSpan.Zero);
        byte[] Render(EmulationEPaperVideoConfiguration ePaper)
        {
            using var pipeline = new SoftwareEmulationVideoProcessingPipeline();
            return EmulationVideoPixelFunctions.ToBgra32(pipeline.Process(
                new EmulationVideoProcessingConfiguration
                {
                    DisplayTechnology = EmulationVideoDisplayTechnology.EPaper,
                    EPaper = ePaper
                }, frame, new(16, 16), new(16, 16)));
        }
        var hashes = Enum.GetValues<EmulationEPaperColorMode>()
            .Select(mode => Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
                Render(new(ColorMode: mode, RefreshTimeMilliseconds: 0)))))
            .ToHashSet(StringComparer.Ordinal);
        Assert.Equal(Enum.GetValues<EmulationEPaperColorMode>().Length, hashes.Count);
        var baseline = new EmulationEPaperVideoConfiguration(
            ColorMode: EmulationEPaperColorMode.Color4096, Contrast: 60,
            Dithering: 40, RefreshTimeMilliseconds: 0, Ghosting: 0,
            InkDensity: 75, PaperBrightness: 80, PaperWarmth: 30,
            ColorSaturation: 55, SurfaceTexture: 20, EdgeSoftness: 20);
        var baselinePixels = Render(baseline);
        void AssertOptionChangesImage(string option, EmulationEPaperVideoConfiguration changed)
        {
            Assert.False(baselinePixels.SequenceEqual(Render(changed)),
                $"Electronic-paper option {option} did not change the rendered image.");
        }
        AssertOptionChangesImage(nameof(baseline.Contrast), baseline with { Contrast = 95 });
        AssertOptionChangesImage(nameof(baseline.Dithering), baseline with { Dithering = 95 });
        AssertOptionChangesImage(nameof(baseline.InkDensity), baseline with { InkDensity = 20 });
        AssertOptionChangesImage(nameof(baseline.PaperBrightness), baseline with
            { PaperBrightness = 25 });
        AssertOptionChangesImage(nameof(baseline.PaperWarmth), baseline with { PaperWarmth = 90 });
        AssertOptionChangesImage(nameof(baseline.ColorSaturation), baseline with
            { ColorSaturation = 5 });
        AssertOptionChangesImage(nameof(baseline.SurfaceTexture), baseline with
            { SurfaceTexture = 90 });
        AssertOptionChangesImage(nameof(baseline.EdgeSoftness), baseline with
            { EdgeSoftness = 90 });

        var dark = frame with
        {
            Pixels = new byte[16 * 16 * 4], Sequence = 2,
            Timestamp = TimeSpan.FromMilliseconds(16)
        };
        EmulationVideoProcessingConfiguration Configuration(int refresh, int ghosting) => new()
        {
            DisplayTechnology = EmulationVideoDisplayTechnology.EPaper,
            EPaper = new(RefreshTimeMilliseconds: refresh, Ghosting: ghosting)
        };
        using var slowRefreshPipeline = new SoftwareEmulationVideoProcessingPipeline();
        using var immediateRefreshPipeline = new SoftwareEmulationVideoProcessingPipeline();
        slowRefreshPipeline.Process(Configuration(1000, 0), frame, new(16, 16), new(16, 16));
        immediateRefreshPipeline.Process(Configuration(0, 0), frame, new(16, 16), new(16, 16));
        var slowRefresh = EmulationVideoPixelFunctions.ToBgra32(slowRefreshPipeline.Process(
            Configuration(1000, 0), dark, new(16, 16), new(16, 16)));
        var immediateRefresh = EmulationVideoPixelFunctions.ToBgra32(
            immediateRefreshPipeline.Process(Configuration(0, 0), dark,
                new(16, 16), new(16, 16)));
        Assert.False(slowRefresh.SequenceEqual(immediateRefresh));

        using var ghostedPipeline = new SoftwareEmulationVideoProcessingPipeline();
        using var cleanPipeline = new SoftwareEmulationVideoProcessingPipeline();
        ghostedPipeline.Process(Configuration(0, 100), frame, new(16, 16), new(16, 16));
        cleanPipeline.Process(Configuration(0, 0), frame, new(16, 16), new(16, 16));
        var ghosted = EmulationVideoPixelFunctions.ToBgra32(ghostedPipeline.Process(
            Configuration(0, 100), dark, new(16, 16), new(16, 16)));
        var clean = EmulationVideoPixelFunctions.ToBgra32(cleanPipeline.Process(
            Configuration(0, 0), dark, new(16, 16), new(16, 16)));
        Assert.False(ghosted.SequenceEqual(clean));
    }

    [Fact]
    public void GeneralPersistenceIsIndependentAndResetsOnSequenceOrSizeChanges()
    {
        static VideoFrame Solid(byte value, int width, int height, long sequence) => new(
            Enumerable.Repeat(new byte[] { value, value, value, 0 }, width * height)
                .SelectMany(pixel => pixel).ToArray(),
            width, height, width * 4, EmulationPixelFormat.Xrgb8888, 1f, sequence,
            TimeSpan.FromMilliseconds(sequence * 16));

        var configuration = new EmulationVideoProcessingConfiguration
        {
            DisplayTechnology = EmulationVideoDisplayTechnology.Normal,
            Temporal = new EmulationTemporalVideoConfiguration(GeneralPersistence: 50)
        };
        using var persistent = new SoftwareEmulationVideoProcessingPipeline();
        using var neutral = new SoftwareEmulationVideoProcessingPipeline();
        persistent.Process(configuration, Solid(255, 2, 2, 1), new(2, 2), new(2, 2));
        neutral.Process(new EmulationVideoProcessingConfiguration(), Solid(255, 2, 2, 1),
            new(2, 2), new(2, 2));

        var trail = EmulationVideoPixelFunctions.ToBgra32(persistent.Process(configuration,
            Solid(0, 2, 2, 2), new(2, 2), new(2, 2)));
        var withoutTrail = EmulationVideoPixelFunctions.ToBgra32(neutral.Process(
            new EmulationVideoProcessingConfiguration(), Solid(0, 2, 2, 2),
            new(2, 2), new(2, 2)));
        Assert.Contains(trail, value => value > 0);
        Assert.All(withoutTrail.Chunk(4), pixel =>
            Assert.Equal(new byte[] { 0, 0, 0, 255 }, pixel));

        persistent.Process(configuration, Solid(255, 2, 2, 5), new(2, 2), new(2, 2));
        var afterRegression = EmulationVideoPixelFunctions.ToBgra32(persistent.Process(
            configuration, Solid(0, 2, 2, 4), new(2, 2), new(2, 2)));
        Assert.All(afterRegression.Chunk(4), pixel =>
            Assert.Equal(new byte[] { 0, 0, 0, 255 }, pixel));

        persistent.Process(configuration, Solid(255, 1, 1, 6), new(1, 1), new(1, 1));
        var afterResize = EmulationVideoPixelFunctions.ToBgra32(persistent.Process(
            configuration, Solid(0, 2, 2, 7), new(2, 2), new(2, 2)));
        Assert.All(afterResize.Chunk(4), pixel =>
            Assert.Equal(new byte[] { 0, 0, 0, 255 }, pixel));
    }

    [Fact]
    public void MotionBlurAccumulatesWeightedHistoryWithoutFreezingAndResetsOnSequenceChanges()
    {
        static VideoFrame Solid(byte value, long sequence) => new(
            Enumerable.Repeat(new byte[] { value, value, value, 0 }, 4)
                .SelectMany(pixel => pixel).ToArray(),
            2, 2, 8, EmulationPixelFormat.Xrgb8888, 1f, sequence,
            TimeSpan.FromMilliseconds(sequence * 16));
        var configuration = new EmulationVideoProcessingConfiguration
        {
            Temporal = new EmulationTemporalVideoConfiguration(MotionBlur: 50)
        };
        using var pipeline = new SoftwareEmulationVideoProcessingPipeline();
        pipeline.Process(configuration, Solid(255, 1), new(2, 2), new(2, 2));
        var blended = EmulationVideoPixelFunctions.ToBgra32(pipeline.Process(configuration,
            Solid(0, 2), new(2, 2), new(2, 2)));
        Assert.All(blended.Chunk(4), pixel =>
        {
            Assert.InRange(pixel[0], (byte)170, (byte)178);
            Assert.Equal(pixel[0], pixel[1]);
            Assert.Equal(pixel[1], pixel[2]);
        });

        var next = EmulationVideoPixelFunctions.ToBgra32(pipeline.Process(configuration,
            Solid(0, 3), new(2, 2), new(2, 2)));
        Assert.All(next.Chunk(4), pixel =>
        {
            Assert.InRange(pixel[0], (byte)110, (byte)125);
            Assert.True(pixel[0] < blended[0]);
        });

        using var maximum = new SoftwareEmulationVideoProcessingPipeline();
        var maximumConfiguration = new EmulationVideoProcessingConfiguration
        {
            Temporal = new EmulationTemporalVideoConfiguration(MotionBlur: 100)
        };
        maximum.Process(maximumConfiguration, Solid(255, 1), new(2, 2), new(2, 2));
        var maximumBlend = EmulationVideoPixelFunctions.ToBgra32(maximum.Process(
            maximumConfiguration, Solid(0, 2), new(2, 2), new(2, 2)));
        Assert.All(maximumBlend.Chunk(4), pixel => Assert.InRange(pixel[0], (byte)230, (byte)245));

        pipeline.Process(configuration, Solid(255, 5), new(2, 2), new(2, 2));
        var afterRegression = EmulationVideoPixelFunctions.ToBgra32(pipeline.Process(configuration,
            Solid(0, 4), new(2, 2), new(2, 2)));
        Assert.All(afterRegression.Chunk(4), pixel =>
            Assert.Equal(new byte[] { 0, 0, 0, 255 }, pixel));
    }

    [Fact]
    public void FlickerDimsOddFramesWithoutReplacingThemWithBlackFrames()
    {
        static VideoFrame Frame(long sequence) => new(
            Enumerable.Repeat(new byte[] { 255, 255, 255, 0 }, 4)
                .SelectMany(pixel => pixel).ToArray(),
            2, 2, 8, EmulationPixelFormat.Xrgb8888, 1f, sequence, TimeSpan.Zero);
        static byte[] Render(int intensity, long sequence)
        {
            using var pipeline = new SoftwareEmulationVideoProcessingPipeline();
            return EmulationVideoPixelFunctions.ToBgra32(pipeline.Process(
                new EmulationVideoProcessingConfiguration
                {
                    Temporal = new EmulationTemporalVideoConfiguration(Flicker: intensity)
                }, Frame(sequence), new(2, 2), new(2, 2)));
        }
        var neutral = Render(0, 1);
        var even = Render(100, 2);
        var odd = Render(100, 1);
        Assert.Equal(neutral, even);
        Assert.All(odd.Chunk(4), pixel =>
        {
            Assert.InRange(pixel[0], (byte)180, (byte)195);
            Assert.NotEqual((byte)0, pixel[0]);
        });
    }

    [Fact]
    public void InterlacingWeavesAlternatingFieldsFromConsecutiveSourceFrames()
    {
        static VideoFrame Frame(byte[] values, long sequence) => new(
            values.SelectMany(value => new byte[] { value, value, value, 0 }).ToArray(),
            1, 4, 4, EmulationPixelFormat.Xrgb8888, 0.25f, sequence, TimeSpan.Zero);
        using var pipeline = new SoftwareEmulationVideoProcessingPipeline();
        byte[] Render(byte[] values, long sequence) =>
            EmulationVideoPixelFunctions.ToBgra32(pipeline.Process(
                new EmulationVideoProcessingConfiguration
                {
                    Temporal = new EmulationTemporalVideoConfiguration(
                        Interlacing: 100, InterlacingVisibility: 100)
                }, Frame(values, sequence), new(1, 4), new(1, 4)));

        Assert.Equal(new byte[] { 10, 20, 30, 40 },
            Render(new byte[] { 10, 20, 30, 40 }, 1).Chunk(4).Select(pixel => pixel[0]));
        Assert.Equal(new byte[] { 6, 110, 23, 130 },
            Render(new byte[] { 100, 110, 120, 130 }, 2).Chunk(4).Select(pixel => pixel[0]));
        Assert.Equal(new byte[] { 200, 90, 220, 106 },
            Render(new byte[] { 200, 210, 220, 230 }, 3).Chunk(4).Select(pixel => pixel[0]));
    }

    [Fact]
    public void InterlacingUsesEmulatedSourceLinesBeforeScaling()
    {
        static VideoFrame Frame(byte value, long sequence) => new(
            Enumerable.Repeat(new byte[] { value, value, value, 0 }, 6)
                .SelectMany(pixel => pixel).ToArray(),
            2, 3, 8, EmulationPixelFormat.Xrgb8888, 2f / 3f,
            sequence, TimeSpan.FromMilliseconds(sequence * 20));
        using var pipeline = new SoftwareEmulationVideoProcessingPipeline();
        var configuration = new EmulationVideoProcessingConfiguration
        {
            Temporal = new EmulationTemporalVideoConfiguration(
                Interlacing: 100, InterlacingVisibility: 100)
        };
        _ = pipeline.Process(configuration, Frame(40, 1), new(2, 3), new(4, 6));
        var result = EmulationVideoPixelFunctions.ToBgra32(pipeline.Process(
            configuration, Frame(200, 2), new(2, 3), new(4, 6)));

        for (var outputY = 0; outputY < 6; outputY++)
        {
            var expected = outputY is 0 or 1 or 4 or 5 ? (byte)31 : (byte)200;
            for (var outputX = 0; outputX < 4; outputX++)
                Assert.Equal(expected, result[(outputY * 4 + outputX) * 4]);
        }
    }
    [Fact]
    public void BlackFrameInsertionBlacksOddFramesAfterOtherEffects()
    {
        var pixels = Enumerable.Repeat(new byte[] { 255, 255, 255, 0 }, 4)
            .SelectMany(pixel => pixel).ToArray();
        byte[] Render(long sequence) {
            using var pipeline = new SoftwareEmulationVideoProcessingPipeline();
            return EmulationVideoPixelFunctions.ToBgra32(pipeline.Process(
                new EmulationVideoProcessingConfiguration
                {
                    Temporal = new EmulationTemporalVideoConfiguration(
                        GeneralPersistence: 100, Flicker: 100, BlackFrameInsertion: true)
                }, new VideoFrame(pixels, 2, 2, 8, EmulationPixelFormat.Xrgb8888, 1f,
                    sequence, TimeSpan.Zero), new(2, 2), new(2, 2)));
        }
        Assert.All(Render(2).Chunk(4), pixel =>
            Assert.Equal(new byte[] { 255, 255, 255, 255 }, pixel));
        Assert.All(Render(1).Chunk(4), pixel =>
            Assert.Equal(new byte[] { 0, 0, 0, 255 }, pixel));
    }

    [Fact]
    public void AdditionalSignalChoicesProduceDistinctBoundedResults()
    {
        var pixels = Enumerable.Range(0, 8 * 6).SelectMany(index => new byte[]
        {
            (byte)(index * 17 % 256), (byte)(index * 43 % 256),
            (byte)(index * 79 % 256), 0
        }).ToArray();
        byte[] Render(EmulationSignalSimulationConfiguration signal)
        {
            using var pipeline = new SoftwareEmulationVideoProcessingPipeline();
            return EmulationVideoPixelFunctions.ToBgra32(pipeline.Process(
                new EmulationVideoProcessingConfiguration { SignalSimulation = signal },
                new VideoFrame(pixels, 8, 6, 32, EmulationPixelFormat.Xrgb8888, 4f / 3f,
                    1, TimeSpan.FromMilliseconds(20)), new(8, 6), new(8, 6)));
        }
        var neutral = Render(new());
        var rgb = Render(new(EmulationSignalConnection.RgbScart, 100));
        var component = Render(new(EmulationSignalConnection.Component, 100));
        var secam = Render(new(Standard: EmulationSignalStandard.Secam,
            StandardIntensity: 100));

        Assert.False(neutral.SequenceEqual(rgb));
        Assert.False(rgb.SequenceEqual(component));
        Assert.False(component.SequenceEqual(secam));
        Assert.All(new[] { rgb, component, secam }.SelectMany(image => image),
            value => Assert.InRange(value, byte.MinValue, byte.MaxValue));
    }

    [Fact]
    public void CompositeSimulationBlursChromaAndUsesSequenceWithoutChangingSignalOptions()
    {
        var pixels = Enumerable.Range(0, 12 * 4).SelectMany(index => new byte[]
        {
            (byte)(index % 2 == 0 ? 0 : 255), (byte)(index % 3 == 0 ? 255 : 0),
            (byte)(index % 2 == 0 ? 255 : 0), 0
        }).ToArray();
        byte[] Render(int intensity, long sequence) {
            using var pipeline = new SoftwareEmulationVideoProcessingPipeline();
            return EmulationVideoPixelFunctions.ToBgra32(pipeline.Process(
                new EmulationVideoProcessingConfiguration
                {
                    SignalSimulation = new(EmulationSignalConnection.Composite, intensity)
                }, new VideoFrame(pixels, 12, 4, 48, EmulationPixelFormat.Xrgb8888, 3f,
                    sequence, TimeSpan.Zero), new(12, 4), new(12, 4)));
        }
        var neutral = Render(0, 1);
        var firstPhase = Render(100, 1);
        var secondPhase = Render(100, 2);
        Assert.Equal(pixels.Select((value, index) => index % 4 == 3 ? (byte)255 : value), neutral);
        Assert.False(neutral.SequenceEqual(firstPhase));
        Assert.False(firstPhase.SequenceEqual(secondPhase));
        Assert.All(firstPhase, value => Assert.InRange(value, byte.MinValue, byte.MaxValue));
    }

    [Fact]
    public void SVideoSimulationPreservesLuminanceAndHasNoSequencePhase()
    {
        var pixels = Enumerable.Range(0, 12 * 4).SelectMany(index => new byte[]
        {
            (byte)(index % 2 == 0 ? 0 : 255), (byte)(index % 3 == 0 ? 255 : 0),
            (byte)(index % 2 == 0 ? 255 : 0), 0
        }).ToArray();
        byte[] Render(int intensity, long sequence) {
            using var pipeline = new SoftwareEmulationVideoProcessingPipeline();
            return EmulationVideoPixelFunctions.ToBgra32(pipeline.Process(
                new EmulationVideoProcessingConfiguration
                {
                    SignalSimulation = new(EmulationSignalConnection.SVideo, intensity)
                }, new VideoFrame(pixels, 12, 4, 48, EmulationPixelFormat.Xrgb8888, 3f,
                    sequence, TimeSpan.Zero), new(12, 4), new(12, 4)));
        }
        var neutral = Render(0, 1);
        var first = Render(100, 1);
        var second = Render(100, 2);
        Assert.False(neutral.SequenceEqual(first));
        Assert.Equal(first, second);
        static double Luminance(byte[] image, int offset)
        {
            var red = SoftwareEmulationVideoProcessingPipeline.SrgbToLinear(image[offset + 2] / 255f);
            var green = SoftwareEmulationVideoProcessingPipeline.SrgbToLinear(image[offset + 1] / 255f);
            var blue = SoftwareEmulationVideoProcessingPipeline.SrgbToLinear(image[offset] / 255f);
            return red * 0.299 + green * 0.587 + blue * 0.114;
        }
        for (var pixel = 0; pixel < 48; pixel++)
        {
            var offset = pixel * 4;
            Assert.InRange(Math.Abs(Luminance(neutral, offset) - Luminance(first, offset)),
                0, 0.03);
        }
    }

    [Fact]
    public void RfSimulationAddsBoundedSequenceDependentNoiseAndBlur()
    {
        var pixels = Enumerable.Range(0, 16 * 8).SelectMany(index => new byte[]
        {
            (byte)(index * 17 % 256), (byte)(index * 31 % 256),
            (byte)(index * 47 % 256), 0
        }).ToArray();
        byte[] Render(int intensity, long sequence) {
            using var pipeline = new SoftwareEmulationVideoProcessingPipeline();
            return EmulationVideoPixelFunctions.ToBgra32(pipeline.Process(
                new EmulationVideoProcessingConfiguration
                {
                    SignalSimulation = new(EmulationSignalConnection.Rf, intensity)
                }, new VideoFrame(pixels, 16, 8, 64, EmulationPixelFormat.Xrgb8888, 2f,
                    sequence, TimeSpan.Zero), new(16, 8), new(16, 8)));
        }
        var neutral = Render(0, 1);
        var first = Render(100, 1);
        var repeat = Render(100, 1);
        var next = Render(100, 2);
        Assert.False(neutral.SequenceEqual(first));
        Assert.Equal(first, repeat);
        Assert.False(first.SequenceEqual(next));
    }

    [Fact]
    public void PalSimulationAlternatesLineChromaWithoutDependingOnFrameSequence()
    {
        var pixels = Enumerable.Range(0, 8 * 8).SelectMany(index => new byte[]
        {
            (byte)(index * 13 % 256), (byte)(index * 37 % 256),
            (byte)(index * 71 % 256), 0
        }).ToArray();
        byte[] Render(int intensity, long sequence) {
            using var pipeline = new SoftwareEmulationVideoProcessingPipeline();
            return EmulationVideoPixelFunctions.ToBgra32(pipeline.Process(
                new EmulationVideoProcessingConfiguration
                {
                    SignalSimulation = new(Standard: EmulationSignalStandard.Pal,
                        StandardIntensity: intensity)
                }, new VideoFrame(pixels, 8, 8, 32, EmulationPixelFormat.Xrgb8888, 1f,
                    sequence, TimeSpan.Zero), new(8, 8), new(8, 8)));
        }
        var neutral = Render(0, 1);
        var first = Render(100, 1);
        var second = Render(100, 2);
        Assert.False(neutral.SequenceEqual(first));
        Assert.Equal(first, second);
        Assert.False(first.AsSpan(0, 32).SequenceEqual(first.AsSpan(32, 32)));
    }

    [Fact]
    public void NtscSimulationDelaysChromaWithoutAddingFrameNoise()
    {
        var pixels = Enumerable.Range(0, 12 * 6).SelectMany(index => new byte[]
        {
            (byte)(index * 19 % 256), (byte)(index * 41 % 256),
            (byte)(index * 73 % 256), 0
        }).ToArray();
        byte[] Render(int intensity, long sequence) {
            using var pipeline = new SoftwareEmulationVideoProcessingPipeline();
            return EmulationVideoPixelFunctions.ToBgra32(pipeline.Process(
                new EmulationVideoProcessingConfiguration
                {
                    SignalSimulation = new(Standard: EmulationSignalStandard.Ntsc,
                        StandardIntensity: intensity)
                }, new VideoFrame(pixels, 12, 6, 48, EmulationPixelFormat.Xrgb8888, 2f,
                    sequence, TimeSpan.Zero), new(12, 6), new(12, 6)));
        }
        var neutral = Render(0, 1);
        var first = Render(100, 1);
        var repeat = Render(100, 1);
        var next = Render(100, 2);
        Assert.False(neutral.SequenceEqual(first));
        Assert.Equal(first, repeat);
        Assert.Equal(first, next);
    }

    [Fact]
    public void GrainIsFineBoundedRepeatableAndChangesWithSequence()
    {
        var pixels = Enumerable.Repeat(new byte[] { 128, 128, 128, 0 }, 16 * 8)
            .SelectMany(pixel => pixel).ToArray();
        byte[] Render(int intensity, long sequence) {
            using var pipeline = new SoftwareEmulationVideoProcessingPipeline();
            return EmulationVideoPixelFunctions.ToBgra32(pipeline.Process(
                new EmulationVideoProcessingConfiguration
                {
                    Stylistic = new EmulationStylisticVideoConfiguration(Grain: intensity)
                }, new VideoFrame(pixels, 16, 8, 64, EmulationPixelFormat.Xrgb8888, 2f,
                    sequence, TimeSpan.Zero), new(16, 8), new(16, 8)));
        }
        var neutral = Render(0, 1);
        var first = Render(100, 1);
        var repeat = Render(100, 1);
        var next = Render(100, 2);
        Assert.False(neutral.SequenceEqual(first));
        Assert.Equal(first, repeat);
        Assert.False(first.SequenceEqual(next));
        Assert.All(first.Chunk(4), pixel => Assert.InRange(pixel[0], (byte)105, (byte)150));
    }

    [Fact]
    public void VhsProducesRepeatableLineJitterChromaBleedAndDropouts()
    {
        var pixels = Enumerable.Range(0, 20 * 24).SelectMany(index => new byte[]
        {
            (byte)(index * 11 % 256), (byte)(index * 29 % 256),
            (byte)(index * 61 % 256), 0
        }).ToArray();
        byte[] Render(int intensity, long sequence) {
            using var pipeline = new SoftwareEmulationVideoProcessingPipeline();
            return EmulationVideoPixelFunctions.ToBgra32(pipeline.Process(
                new EmulationVideoProcessingConfiguration
                {
                    Stylistic = new EmulationStylisticVideoConfiguration(Vhs: intensity)
                }, new VideoFrame(pixels, 20, 24, 80, EmulationPixelFormat.Xrgb8888, 5f / 6f,
                    sequence, TimeSpan.Zero), new(20, 24), new(20, 24)));
        }
        var neutral = Render(0, 1);
        var first = Render(100, 1);
        var repeat = Render(100, 1);
        var next = Render(100, 2);
        Assert.False(neutral.SequenceEqual(first));
        Assert.Equal(first, repeat);
        Assert.False(first.SequenceEqual(next));
        static double LineLuminance(byte[] image, int line) => image.AsSpan(line * 80, 80)
            .ToArray().Where((_, index) => index % 4 != 3).Average(value => value);
        Assert.Contains(Enumerable.Range(0, 24), line =>
            LineLuminance(first, line) < LineLuminance(neutral, line) * 0.85);
    }

    [Fact]
    public void ChromaticAberrationSeparatesRedAndBlueDeterministically()
    {
        var pixels = Enumerable.Range(0, 12 * 5).SelectMany(index => new byte[]
        {
            (byte)(index * 17 % 256), (byte)(index * 31 % 256),
            (byte)(index * 67 % 256), 0
        }).ToArray();
        byte[] Render(int intensity, long sequence)
        {
            using var pipeline = new SoftwareEmulationVideoProcessingPipeline();
            return EmulationVideoPixelFunctions.ToBgra32(pipeline.Process(
                new EmulationVideoProcessingConfiguration
                {
                    Stylistic = new EmulationStylisticVideoConfiguration(
                        ChromaticAberration: intensity)
                }, new VideoFrame(pixels, 12, 5, 48, EmulationPixelFormat.Xrgb8888,
                    12f / 5f, sequence, TimeSpan.Zero), new(12, 5), new(12, 5)));
        }

        var neutral = Render(0, 1);
        var first = Render(100, 1);
        Assert.False(neutral.SequenceEqual(first));
        Assert.Equal(first, Render(100, 1));
        Assert.Equal(first, Render(100, 2));
        Assert.All(first.Chunk(4), pixel => Assert.Equal((byte)255, pixel[3]));
    }

    [Fact]
    public void BloomSpreadsOnlyHighlightsAndIsSequenceIndependent()
    {
        var pixels = new byte[11 * 11 * 4];
        var center = (5 * 11 + 5) * 4;
        pixels[center] = 255;
        pixels[center + 1] = 255;
        pixels[center + 2] = 255;
        byte[] Render(int intensity, long sequence)
        {
            using var pipeline = new SoftwareEmulationVideoProcessingPipeline();
            return EmulationVideoPixelFunctions.ToBgra32(pipeline.Process(
                new EmulationVideoProcessingConfiguration
                {
                    Stylistic = new EmulationStylisticVideoConfiguration(Bloom: intensity)
                }, new VideoFrame(pixels, 11, 11, 44, EmulationPixelFormat.Xrgb8888,
                    1f, sequence, TimeSpan.Zero), new(11, 11), new(11, 11)));
        }

        var neutral = Render(0, 1);
        var first = Render(100, 1);
        Assert.False(neutral.SequenceEqual(first));
        Assert.Equal(first, Render(100, 2));
        var neighbor = (5 * 11 + 4) * 4;
        Assert.Equal((byte)0, neutral[neighbor]);
        Assert.True(first[neighbor] > 0);
        Assert.Equal((byte)0, first[0]);
    }

    [Fact]
    public void SepiaProducesWarmBrownColorAndIsSequenceIndependent()
    {
        var pixels = Enumerable.Repeat(new byte[] { 128, 128, 128, 0 }, 4)
            .SelectMany(pixel => pixel).ToArray();
        byte[] Render(bool enabled, long sequence)
        {
            using var pipeline = new SoftwareEmulationVideoProcessingPipeline();
            return EmulationVideoPixelFunctions.ToBgra32(pipeline.Process(
                new EmulationVideoProcessingConfiguration
                {
                    Stylistic = new EmulationStylisticVideoConfiguration(Sepia: enabled)
                }, new VideoFrame(pixels, 4, 1, 16, EmulationPixelFormat.Xrgb8888,
                    4f, sequence, TimeSpan.Zero), new(4, 1), new(4, 1)));
        }

        var neutral = Render(false, 1);
        var full = Render(true, 1);
        Assert.False(neutral.SequenceEqual(full));
        Assert.Equal(full, Render(true, 2));
        Assert.True(full[2] > full[1]);
        Assert.True(full[1] > full[0]);
    }

    [Fact]
    public void ProjectionBlurDiffusionTextureAndConvergenceAreDistinctAndBounded()
    {
        var pixels = Enumerable.Range(0, 20 * 20).SelectMany(index => new byte[]
        {
            (byte)(index * 17 % 256), (byte)(index * 43 % 256),
            (byte)(index * 71 % 256), 0
        }).ToArray();
        var frame = new VideoFrame(pixels, 20, 20, 80, EmulationPixelFormat.Xrgb8888,
            1f, 1, TimeSpan.Zero);
        byte[] Render(EmulationProjectionVideoConfiguration projection)
        {
            using var pipeline = new SoftwareEmulationVideoProcessingPipeline();
            return EmulationVideoPixelFunctions.ToBgra32(pipeline.Process(
                new EmulationVideoProcessingConfiguration
                {
                    DisplayTechnology = EmulationVideoDisplayTechnology.Projection,
                    Projection = projection
                }, frame, new(20, 20), new(20, 20)));
        }
        var neutral = Render(new(0, 0, 0, 0));
        var variants = new[]
        {
            Render(new(100, 0, 0, 0)), Render(new(0, 100, 0, 0)),
            Render(new(0, 0, 100, 0)), Render(new(0, 0, 0, 100)),
            Render(new(0, 0, 0, 0, LightOutput: 100)),
            Render(new(0, 0, 0, 0, AmbientLight: 100)),
            Render(new(0, 0, 0, 0, Vignette: 100))
        };
        Assert.All(variants, variant => Assert.False(neutral.SequenceEqual(variant)));
        Assert.Equal(EmulationVideoPixelFunctions.ToBgra32(frame), neutral);
        Assert.False(neutral.SequenceEqual(Render(new(0, 0, 0, 1))));
        Assert.Equal(Render(new(0, 0, 100, 0)), Render(new(0, 0, 100, 0)));
        Assert.Equal(variants.Length, variants.Select(variant => Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(variant))).Distinct().Count());
    }

    [Fact]
    public void SnapshotUsesProcessedOutputFromTheCommonSurfacePath()
    {
        RunSta(() =>
        {
            var source = Frame([1, 2, 3, 0], sequence: 1);
            var processed = Frame([11, 22, 33, 0], sequence: 2);
            foreach (var renderer in Enum.GetValues<EmulationVideoRenderer>())
            {
                using var pipeline = new ReplacingPipeline(renderer, processed);
                var surfaceFrame = EmulationVideoSurfaceFrameFunctions.Process(
                    pipeline, new EmulationVideoProcessingConfiguration(), source,
                    new EmulationVideoProcessingSize(127, 93));

                Assert.Same(processed, surfaceFrame.Frame);
                Assert.Equal(new byte[] { 11, 22, 33, 255 }, surfaceFrame.Bgra32Pixels);
            }

            using var surface = new WpfVideoSurface(
                new ReplacingPipeline(EmulationVideoRenderer.Wpf, processed));
            surface.Present(source);

            var snapshot = Assert.IsType<System.Windows.Media.Imaging.WriteableBitmap>(
                surface.Snapshot);
            var snapshotPixels = new byte[4];
            snapshot.CopyPixels(snapshotPixels, 4, 0);
            Assert.Equal(new byte[] { 11, 22, 33, 255 }, snapshotPixels);
            Assert.NotEqual(source.Pixels.ToArray(), snapshotPixels);
        });
    }

    [Fact]
    public void XbrzLargeOutputCompletesWithinInteractiveBudget()
    {
        var configuration = new EmulationVideoProcessingConfiguration
        {
            Sampling = EmulationVideoSampling.Xbrz
        };
        using var pipeline = new SoftwareEmulationVideoProcessingPipeline();
        var warmup = DeterministicFrame(3, 3);
        pipeline.Process(configuration, warmup, new(3, 3), new(6, 6));
        var frame = DeterministicFrame(320, 200);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var output = pipeline.Process(configuration, frame, new(320, 200), new(1280, 800));
        stopwatch.Stop();

        Assert.Equal((1280, 800), (output.Width, output.Height));
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(5),
            $"xBRZ 1280x800 took {stopwatch.Elapsed.TotalMilliseconds:F0} ms.");
    }

    private static VideoFrame DeterministicFrame(int width, int height)
    {
        var pixels = Enumerable.Range(0, width * height).SelectMany(index => new byte[]
        {
            (byte)((index * 17 + 11) % 220),
            (byte)((index * 29 + 23) % 230),
            (byte)((index * 41 + 37) % 240),
            0
        }).ToArray();
        return new VideoFrame(pixels, width, height, width * 4,
            EmulationPixelFormat.Xrgb8888, width / (float)height, 5, TimeSpan.Zero);
    }

    private static VideoFrame CheckerboardFrame(int width, int height, byte first, byte second)
    {
        var pixels = new byte[checked(width * height * 4)];
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var value = ((x + y) & 1) == 0 ? first : second;
            var offset = (y * width + x) * 4;
            pixels[offset] = value;
            pixels[offset + 1] = value;
            pixels[offset + 2] = value;
        }
        return new VideoFrame(pixels, width, height, width * 4,
            EmulationPixelFormat.Xrgb8888, 1f, 1, TimeSpan.Zero);
    }

    private static VideoFrame NoisyEdgeFrame(int width, int height)
    {
        var pixels = new byte[checked(width * height * 4)];
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var value = x < 2 ? (byte)60 : (byte)200;
            if (x == 1 && y == 2) value = 80;
            var offset = (y * width + x) * 4;
            pixels[offset] = value;
            pixels[offset + 1] = value;
            pixels[offset + 2] = value;
        }
        return new VideoFrame(pixels, width, height, width * 4,
            EmulationPixelFormat.Xrgb8888, 1f, 2, TimeSpan.Zero);
    }

    private static VideoFrame BandedGradientFrame(int width, int height)
    {
        var bands = new byte[] { 80, 80, 84, 84, 220 };
        var pixels = new byte[checked(width * height * 4)];
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var value = bands[Math.Min(x, bands.Length - 1)];
            var offset = (y * width + x) * 4;
            pixels[offset] = value;
            pixels[offset + 1] = value;
            pixels[offset + 2] = value;
        }
        return new VideoFrame(pixels, width, height, width * 4,
            EmulationPixelFormat.Xrgb8888, 1f, 3, TimeSpan.Zero);
    }

    private static VideoFrame DetailFrame(int width, int height)
    {
        var pixels = new byte[checked(width * height * 4)];
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var value = x == width - 1 ? (byte)220 : (byte)100;
            if (x == 1 && y == 2) value = 110;
            var offset = (y * width + x) * 4;
            pixels[offset] = value;
            pixels[offset + 1] = value;
            pixels[offset + 2] = value;
        }
        return new VideoFrame(pixels, width, height, width * 4,
            EmulationPixelFormat.Xrgb8888, 1f, 4, TimeSpan.Zero);
    }

    private static VideoFrame InterlacedFrame(int width, int height)
    {
        var rows = new byte[] { 20, 200, 40, 180, 60 };
        var pixels = new byte[checked(width * height * 4)];
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var value = rows[Math.Min(y, rows.Length - 1)];
            var offset = (y * width + x) * 4;
            pixels[offset] = value;
            pixels[offset + 1] = value;
            pixels[offset + 2] = value;
        }
        return new VideoFrame(pixels, width, height, width * 4,
            EmulationPixelFormat.Xrgb8888, width / (float)height, 5, TimeSpan.Zero);
    }

    private static VideoFrame AdvancedValidationFrame(int width, int height)
    {
        var pixels = new byte[checked(width * height * 4)];
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            byte value;
            if (x < 4 && y < 4)
                value = (byte)(((x + y) & 1) == 0 ? 80 : 120);
            else if (x < 8 && y < 8)
                value = (byte)(x < 6 ? 80 : 84);
            else if (x >= 12)
                value = (byte)((y & 1) == 0 ? 20 : 200);
            else if (y >= 8)
                value = (byte)(x <= y - 8 ? 25 : 225);
            else
                value = 100;
            if (x == 6 && y == 6) value = 112;
            var exactPixelArt = x < 3 && y is >= 8 and <= 10;
            var offset = (y * width + x) * 4;
            pixels[offset] = value;
            pixels[offset + 1] = exactPixelArt ? value : (byte)Math.Min(255, value + x % 3);
            pixels[offset + 2] = exactPixelArt ? value : (byte)Math.Max(0, value - y % 3);
        }
        return new VideoFrame(pixels, width, height, width * 4,
            EmulationPixelFormat.Xrgb8888, 1f, 6, TimeSpan.Zero);
    }

    private static VideoFrame ValidationFrame(int width, int height, bool amiga)
    {
        var pixels = new byte[checked(width * height * 4)];
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var index = (y * width + x) * 4;
            if (amiga)
            {
                pixels[index] = (byte)((x / 4 % 2 == y / 4 % 2) ? 210 : 35);
                pixels[index + 1] = (byte)(30 + y * 200 / Math.Max(1, height - 1));
                pixels[index + 2] = (byte)(20 + x * 220 / Math.Max(1, width - 1));
            }
            else
            {
                pixels[index] = (byte)(20 + y * 210 / Math.Max(1, height - 1));
                pixels[index + 1] = (byte)((x / 8 % 2 == 0) ? 190 : 45);
                pixels[index + 2] = (byte)((x + y) % 16 < 8 ? 225 : 30);
            }
        }
        return new VideoFrame(pixels, width, height, width * 4,
            EmulationPixelFormat.Xrgb8888, amiga ? 4f / 3f : 16f / 10f,
            amiga ? 600 : 800, TimeSpan.Zero);
    }

    private static void WriteValidationBoard(string outputDirectory,
        EmulationVideoRenderer renderer,
        IReadOnlyList<(int Row, int Column, int Width, int Height, byte[] Pixels)> cells,
        int columns, int rows, string prefix)
    {
        Directory.CreateDirectory(outputDirectory);
        var cellWidth = cells.Max(cell => cell.Width);
        var rowHeights = Enumerable.Range(0, rows).Select(row =>
            cells.Where(cell => cell.Row == row).Max(cell => cell.Height)).ToArray();
        foreach (var row in Enumerable.Range(0, rows))
            Assert.All(cells.Where(cell => cell.Row == row), cell =>
                Assert.Equal((cellWidth, rowHeights[row]), (cell.Width, cell.Height)));
        const int separator = 2;
        var width = columns * cellWidth + (columns - 1) * separator;
        var height = rowHeights.Sum() + (rows - 1) * separator;
        var pixels = new byte[checked(width * height * 4)];
        for (var index = 3; index < pixels.Length; index += 4) pixels[index] = 255;
        foreach (var cell in cells)
        {
            var targetX = cell.Column * (cellWidth + separator);
            var targetY = rowHeights.Take(cell.Row).Sum() + cell.Row * separator;
            for (var y = 0; y < cell.Height; y++)
                Buffer.BlockCopy(cell.Pixels, y * cellWidth * 4, pixels,
                    ((targetY + y) * width + targetX) * 4, cellWidth * 4);
        }
        var bitmap = System.Windows.Media.Imaging.BitmapSource.Create(width, height, 96, 96,
            System.Windows.Media.PixelFormats.Bgra32, null, pixels, width * 4);
        var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
        encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmap));
        using var stream = File.Create(Path.Combine(outputDirectory,
            $"{prefix}-{renderer.ToString().ToLowerInvariant()}.png"));
        encoder.Save(stream);
    }

    private static VideoFrame Frame(byte[] pixels, long sequence) =>
        new(pixels, 1, 1, 4, EmulationPixelFormat.Xrgb8888, 1f, sequence,
            TimeSpan.FromMilliseconds(sequence));

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "GWGUI.sln")))
            directory = directory.Parent;
        return directory?.FullName
            ?? throw new DirectoryNotFoundException("GWGUI.sln");
    }

    private static IEmulationVideoSurface CreateDeterministicSurface(
        EmulationVideoRenderer renderer) =>
        renderer == EmulationVideoRenderer.Wpf
            ? new WpfVideoSurface(new SoftwareEmulationVideoProcessingPipeline())
            : EmulationVideoSurfaceFactory.Create(renderer);
    private static void RunSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try { action(); }
            catch (Exception error) { failure = error; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure is not null) ExceptionDispatchInfo.Capture(failure).Throw();
    }

    private sealed class ReplacingPipeline(
        EmulationVideoRenderer renderer,
        VideoFrame replacement) : IEmulationVideoProcessingPipeline
    {
        public EmulationVideoRenderer Renderer { get; } = renderer;

        public VideoFrame Process(EmulationVideoProcessingConfiguration configuration,
            VideoFrame frame, EmulationVideoProcessingSize sourceSize,
            EmulationVideoProcessingSize outputSize) => replacement;

        public void Dispose() { }
    }
}
