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
        foreach (var entry in paths)
        {
            var image = await reader.ReadAsync(entry.Path);
            Assert.True(image.ChecksumValid, $"SCP checksum is invalid: {entry.Path}");
            Assert.True(image.Tracks.Count >= entry.MinimumTrackCount, $"Too few populated tracks in {entry.Path}: {image.Tracks.Count} (expected at least {entry.MinimumTrackCount})");
            Assert.All(entry.ExpectedHeads, head => Assert.Contains(image.Tracks, track => track.Head == head));
            Assert.All(image.Tracks, track => Assert.Contains(track.Head, entry.ExpectedHeads));
            Assert.All(image.Tracks, track => Assert.Equal(image.Header.Revolutions, track.Revolutions.Count));
            Assert.All(image.Tracks.SelectMany(track => track.Revolutions), revolution => Assert.True(revolution.FluxIntervals.Count > 1000));

            var sampleStep = Math.Max(1, image.Tracks.Count / 4);
            foreach (var track in image.Tracks.Where((_, index) => index % sampleStep == 0).Take(4))
            {
                var decoded = decoders.DecodeBest(track.Revolutions);
                Assert.NotNull(decoded);
                Assert.StartsWith(entry.DecoderPrefix, decoded.Value.Result.DecoderId);
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
                var image = new ScpReader().ReadAsync(paths[0].Path).GetAwaiter().GetResult();
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

    private static CorpusEntry[] CorpusPaths()
    {
        var specification = Environment.GetEnvironmentVariable("GWGUI_REAL_SCP_CORPUS");
        if (string.IsNullOrWhiteSpace(specification)) return [];
        var entries = specification.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(value => value.Split('|', 4, StringSplitOptions.TrimEntries))
            .Select(parts => parts.Length switch
            {
                2 => new CorpusEntry(parts[0], Path.GetFullPath(parts[1]), 80, [0, 1]),
                3 when int.TryParse(parts[1], out var minimumTracks) && minimumTracks > 0 => new CorpusEntry(parts[0], Path.GetFullPath(parts[2]), minimumTracks, [0, 1]),
                4 when int.TryParse(parts[1], out var minimumTracks) && minimumTracks > 0 => new CorpusEntry(parts[0], Path.GetFullPath(parts[3]), minimumTracks, ParseHeads(parts[2])),
                _ => throw new InvalidDataException("Invalid SCP corpus specification.")
            })
            .ToArray();
        Assert.True(entries.Length >= 4, "At least four public physical SCP captures are required.");
        Assert.All(entries, entry => Assert.True(File.Exists(entry.Path), $"SCP corpus file is missing: {entry.Path}"));
        return entries;
    }

    private static int[] ParseHeads(string value)
    {
        var heads = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(int.Parse).Distinct().ToArray();
        if (heads.Length == 0 || heads.Any(head => head is < 0 or > 1)) throw new InvalidDataException("Invalid SCP corpus head specification.");
        return heads;
    }

    private sealed record CorpusEntry(string DecoderPrefix, string Path, int MinimumTrackCount, int[] ExpectedHeads);
}
