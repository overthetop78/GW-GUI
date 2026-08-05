using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Runtime.ExceptionServices;
using GWGUI.App;
using GWGUI.Scp;
using GWGUI.Scp.Decoding;

namespace GWGUI.Tests;

public sealed class RealScpCorpusTests
{
    [Fact]
    public async Task PublicPhysicalCapturesLoadAndDecodeWhenRequested()
    {
        var paths = CorpusPaths();
        if (paths.Length == 0) return;

        var reader = new ScpReader();
        var decoders = new FluxDecoderRegistry();
        foreach (var path in paths)
        {
            var image = await reader.ReadAsync(path);
            Assert.True(image.ChecksumValid, $"SCP checksum is invalid: {path}");
            Assert.True(image.Tracks.Count >= 80, $"Too few populated tracks in {path}: {image.Tracks.Count}");
            Assert.Contains(image.Tracks, track => track.Head == 0);
            Assert.Contains(image.Tracks, track => track.Head == 1);
            Assert.All(image.Tracks, track => Assert.Equal(image.Header.Revolutions, track.Revolutions.Count));
            Assert.All(image.Tracks.SelectMany(track => track.Revolutions), revolution => Assert.True(revolution.FluxIntervals.Count > 1000));

            var sampleStep = Math.Max(1, image.Tracks.Count / 4);
            foreach (var track in image.Tracks.Where((_, index) => index % sampleStep == 0).Take(4))
            {
                var decoded = decoders.DecodeBest(track.Revolutions);
                Assert.NotNull(decoded);
                Assert.StartsWith("iso.", decoded.Value.Result.DecoderId);
                Assert.True(decoded.Value.Result.EstimatedBitCellTicks > 0);
                Assert.NotEmpty(decoded.Value.Result.Structures);
            }
        }
    }

    [Fact]
    public void PublicPhysicalCaptureRendersThroughTheRealWpfSkiaControlWhenRequested()
    {
        var paths = CorpusPaths();
        if (paths.Length == 0) return;

        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                var image = new ScpReader().ReadAsync(paths[0]).GetAwaiter().GetResult();
                var tracks = image.Tracks.Where(track => track.Head == 0).Take(10).ToArray();
                Assert.NotEmpty(tracks);
                var sample = new ScpImage(image.Header, tracks, image.ChecksumValid, image.FileSize);
                var view = new ScpDiskView { Width = 640, Height = 640 };
                view.SetDecoder("raw");
                view.SetImage(sample, 0);
                view.Measure(new Size(640, 640));
                view.Arrange(new Rect(0, 0, 640, 640));
                view.UpdateLayout();

                var bitmap = new RenderTargetBitmap(640, 640, 96, 96, PixelFormats.Pbgra32);
                bitmap.Render(view);
                var pixels = new byte[640 * 640 * 4];
                bitmap.CopyPixels(pixels, 640 * 4, 0);
                Assert.Contains(pixels, value => value != 0);
            }
            catch (Exception exception) { failure = exception; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure is not null) ExceptionDispatchInfo.Capture(failure).Throw();
    }

    private static string[] CorpusPaths()
    {
        var specification = Environment.GetEnvironmentVariable("GWGUI_REAL_SCP_CORPUS");
        if (string.IsNullOrWhiteSpace(specification)) return [];
        var paths = specification.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(Path.GetFullPath).ToArray();
        Assert.True(paths.Length >= 2, "At least two public physical SCP captures are required.");
        Assert.All(paths, path => Assert.True(File.Exists(path), $"SCP corpus file is missing: {path}"));
        return paths;
    }
}
