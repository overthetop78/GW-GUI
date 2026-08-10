using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Runtime.ExceptionServices;
using GWGUI.App;
using GWGUI.App.Rendering;
using GWGUI.MediaEngine;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Images;
using GWGUI.MediaEngine.SectorImages;
using SkiaSharp;

namespace GWGUI.Tests;

public sealed class RealScpCorpusTests
{
    [Fact]
    public async Task RealAmigaScpPreparesAndRendersBothFacesWhenRequested()
    {
        var scpPath = Environment.GetEnvironmentVariable("GWGUI_REAL_AMIGA_SCP");
        if (string.IsNullOrWhiteSpace(scpPath)) return;

        var image = await new ScpReader().ReadAsync(scpPath);
        Assert.True(image.ChecksumValid);
        foreach (var head in new[] { 0, 1 })
        {
            var tracks = image.Tracks.Where(track => track.Head == head).OrderBy(track => track.Cylinder).ToArray();
            Assert.NotEmpty(tracks);
            IScpRenderer renderer = new SkiaScpRenderer { DecoderId = "amiga.mfm" };
            var preparations = new List<ScpTrackPreparation>();
            await renderer.PrepareAsync(image, head, new ImmediateProgress<ScpTrackPreparation>(preparations.Add));
            Assert.Equal(tracks.Length, preparations.Count);
            Assert.All(preparations, preparation => Assert.True(preparation.HasFlux));

            using var bitmap = new SKBitmap(512, 512);
            using var canvas = new SKCanvas(bitmap);
            renderer.Render(canvas, new ScpRenderRequest(image, head, tracks[0], 512, 512, new SKPoint(256, 256), 1, "No data", $"Side {head}"));
            Assert.NotEqual(new SKColor(7, 10, 14), bitmap.GetPixel(256, 30));
        }
    }

    [Fact]
    public async Task RealAmigaAdfAndScpDecodeToIdenticalSectorImagesWhenRequested()
    {
        var adfPath = Environment.GetEnvironmentVariable("GWGUI_REAL_AMIGA_ADF");
        var scpPath = Environment.GetEnvironmentVariable("GWGUI_REAL_AMIGA_SCP");
        if (string.IsNullOrWhiteSpace(adfPath) || string.IsNullOrWhiteSpace(scpPath)) return;

        var expected = await new AdfImageReader().ReadAsync(adfPath);
        var actual = await new AmigaScpSectorImageReader(new ScpReader(), new FluxDecoderRegistry()).ReadAsync(scpPath);

        Assert.Equal(expected.SectorsPerTrack, actual.SectorsPerTrack);
        Assert.Equal(expected.FormatId, actual.FormatId);
        Assert.Equal(expected.BlockCount, actual.BlockCount);
        Assert.True(actual.MissingBlocks.Count == 0,
            $"Decoded {actual.AvailableBlocks.Count}/{actual.BlockCount} blocks; missing: {string.Join(", ", actual.MissingBlocks.Take(20))}");
        for (var logical = 0; logical < expected.BlockCount; logical++)
            Assert.Equal(expected.GetBlock(logical).ToArray(), actual.GetBlock(logical).ToArray());
    }

    [Fact]
    public async Task RealAmigaAdfAndScpExposeTheSameFileSystemWhenRequested()
    {
        var adfPath = Environment.GetEnvironmentVariable("GWGUI_REAL_AMIGA_ADF");
        var scpPath = Environment.GetEnvironmentVariable("GWGUI_REAL_AMIGA_SCP");
        if (string.IsNullOrWhiteSpace(adfPath) || string.IsNullOrWhiteSpace(scpPath)) return;

        var format = new FileInfo(adfPath).Length == AdfImageReader.HighDensityBytes ? "amiga.amigados_hd" : "amiga.amigados";
        var explorer = DiskImageExplorer.CreateDefault();
        var expected = await explorer.ExploreAsync(adfPath, format);
        var actual = await explorer.ExploreAsync(scpPath, format);

        Assert.Equal(expected.Volume.Name, actual.Volume.Name);
        Assert.Equal(expected.Volume.FileSystem, actual.Volume.FileSystem);
        Assert.Equal(expected.Volume.Capacity, actual.Volume.Capacity);
        Assert.Equal(expected.Volume.FreeBytes, actual.Volume.FreeBytes);
        Assert.Equal(Flatten(expected.Volume.Entries), Flatten(actual.Volume.Entries));
        Assert.Equal(expected.Volume.Warnings, actual.Volume.Warnings);
        Assert.Empty(expected.Volume.Warnings);
    }

    [Fact]
    public async Task RealAmigaAdfRoundTripsThroughTheInternalEncoderWhenRequested()
    {
        var adfPath = Environment.GetEnvironmentVariable("GWGUI_REAL_AMIGA_ADF");
        if (string.IsNullOrWhiteSpace(adfPath)) return;

        var expected = await new AdfImageReader().ReadAsync(adfPath);
        var encoder = new GWGUI.MediaEngine.Encoding.FluxEncoderRegistry();
        var tracks = new List<ScpTrack>();
        for (var cylinder = 0; cylinder < expected.Cylinders; cylinder++)
        for (var head = 0; head < expected.Heads; head++)
        {
            var logicalStart = (cylinder * expected.Heads + head) * expected.SectorsPerTrack;
            var sectors = Enumerable.Range(0, expected.SectorsPerTrack)
                .Select(number => new GWGUI.MediaEngine.Encoding.TrackSector(number, expected.GetBlock(logicalStart + number).ToArray()))
                .ToArray();
            var cellTicks = expected.SectorsPerTrack == 22 ? 20u : 40u;
            var encoded = encoder.Encode("amiga.mfm", new GWGUI.MediaEngine.Encoding.TrackEncodeRequest(cylinder, head, sectors, BitCellTicks: cellTicks));
            tracks.Add(new((byte)(cylinder * 2 + head), cylinder, head, [encoded.Revolution]));
        }
        var scp = new ScpImage(new(0, 0, 1, 0, 159, ScpFlags.IndexAligned, 16, 0, 0, 0), tracks, true, 0);
        var actual = await new AmigaScpSectorImageReader(new MemoryScpReader(scp), new FluxDecoderRegistry()).ReadAsync("memory.scp");

        Assert.Equal(expected.SectorsPerTrack, actual.SectorsPerTrack);
        Assert.Empty(actual.MissingBlocks);
        for (var logical = 0; logical < expected.BlockCount; logical++)
            Assert.Equal(expected.GetBlock(logical).ToArray(), actual.GetBlock(logical).ToArray());
        var volume = new GWGUI.MediaEngine.FileSystems.Readers.AmigaDosFileSystemReader().Read(actual);
        Assert.False(string.IsNullOrWhiteSpace(volume.Name));
        Assert.Empty(volume.Warnings);
    }

    [Fact]
    public async Task RealAmigaDosCaptureReconstructsRootBlockWhenRequested()
    {
        var path = Environment.GetEnvironmentVariable("GWGUI_REAL_AMIGA_SCP");
        if (string.IsNullOrWhiteSpace(path)) return;

        var image = await new AmigaScpSectorImageReader(new ScpReader(), new FluxDecoderRegistry()).ReadAsync(path);
        Assert.True(image.TryGetBlock(image.BlockCount / 2, out _),
            $"AmigaDOS root block {image.BlockCount / 2} is missing; decoded {image.AvailableBlocks.Count}/{image.BlockCount} blocks.");
        var volume = new GWGUI.MediaEngine.FileSystems.Readers.AmigaDosFileSystemReader().Read(image);
        Assert.False(string.IsNullOrWhiteSpace(volume.Name));
    }

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
                view.PrepareAsync().GetAwaiter().GetResult();
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

    private static string[] Flatten(IEnumerable<GWGUI.MediaEngine.FileSystems.FileSystemEntry> entries, string prefix = "") => entries
        .SelectMany(entry => new[]
        {
            $"{prefix}/{entry.Name}|{entry.Kind}|{entry.Size}|{entry.Comment}|{entry.Protection}|{entry.MetadataValid}|{Convert.ToBase64String(entry.Content?.ToArray() ?? [])}"
        }.Concat(Flatten(entry.Children, $"{prefix}/{entry.Name}")))
        .ToArray();

    private sealed record CorpusEntry(string DecoderPrefix, string Path, int MinimumTrackCount, int[] ExpectedHeads);

    private sealed class ImmediateProgress<T>(Action<T> report) : IProgress<T>
    {
        public void Report(T value) => report(value);
    }

    private sealed class MemoryScpReader(ScpImage image) : IScpReader
    {
        public Task<ScpImage> ReadAsync(string path, CancellationToken cancellationToken = default) => Task.FromResult(image);
    }
}
