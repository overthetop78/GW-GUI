using System.IO;
using GWGUI.App.Services;
using GWGUI.Domain.Commands;
using GWGUI.Domain.Conversion;
using GWGUI.Domain.Formats;
using GWGUI.MediaEngine.Containers.Hfe;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Conversion.Flux;

namespace GWGUI.Tests;

public sealed class FluxContainerConversionTests
{
    [Fact]
    public async Task ScpCopyPreservesEveryTrackRevolutionIndexAndInterval()
    {
        var sourcePath = TemporaryPath(".scp");
        var targetPath = TemporaryPath(".scp");
        try
        {
            var source = CreateScp(revolutions: 3);
            await new ScpWriter().WriteAsync(sourcePath, source);

            await CreateService().ConvertAsync(sourcePath, targetPath);

            var actual = await new ScpReader().ReadAsync(targetPath);
            Assert.True(actual.ChecksumValid);
            AssertScpFluxEqual(source, actual);
            Assert.Equal(
                await File.ReadAllBytesAsync(sourcePath),
                await File.ReadAllBytesAsync(targetPath));
        }
        finally
        {
            Delete(sourcePath, targetPath);
        }
    }

    [Fact]
    public async Task HfeCopyPreservesEveryTrackBitAndTiming()
    {
        var sourcePath = TemporaryPath(".hfe");
        var targetPath = TemporaryPath(".hfe");
        try
        {
            var source = CreateHfe();
            await new HfeWriter().WriteAsync(source, sourcePath);
            var normalized = await new HfeReader().ReadAsync(sourcePath);

            await CreateService().ConvertAsync(sourcePath, targetPath);

            var actual = await new HfeReader().ReadAsync(targetPath);
            AssertHfeFluxEqual(normalized, actual);
            Assert.Equal(
                await File.ReadAllBytesAsync(sourcePath),
                await File.ReadAllBytesAsync(targetPath));
        }
        finally
        {
            Delete(sourcePath, targetPath);
        }
    }

    [Fact]
    public async Task HfeToScpPreservesTheAvailableRevolutionWithoutSectorImage()
    {
        var sourcePath = TemporaryPath(".hfe");
        var targetPath = TemporaryPath(".scp");
        try
        {
            var source = CreateHfe();
            await new HfeWriter().WriteAsync(source, sourcePath);
            var normalized = await new HfeReader().ReadAsync(sourcePath);

            await CreateService().ConvertAsync(sourcePath, targetPath);

            var actual = await new ScpReader().ReadAsync(targetPath);
            Assert.True(actual.ChecksumValid);
            Assert.Equal(normalized.Tracks.Count, actual.Tracks.Count);
            foreach (var expectedTrack in normalized.Tracks)
            {
                var trackNumber = ScpFormatConstants.ToTrackNumber(
                    expectedTrack.Cylinder,
                    expectedTrack.Head);
                var actualTrack = Assert.Single(actual.Tracks, track => track.TrackNumber == trackNumber);
                var revolution = Assert.Single(actualTrack.Revolutions);
                Assert.Equal(
                    expectedTrack.Bits.Count * (long)expectedTrack.BitCellTicks,
                    (long)revolution.IndexTimeTicks);
                Assert.Equal(
                    ToIntervals(expectedTrack.Bits, expectedTrack.BitCellTicks),
                    revolution.FluxIntervals);
            }
        }
        finally
        {
            Delete(sourcePath, targetPath);
        }
    }

    [Fact]
    public async Task UniformSingleRevolutionScpCanBecomeHfeWithoutSectorImage()
    {
        var sourcePath = TemporaryPath(".scp");
        var targetPath = TemporaryPath(".hfe");
        try
        {
            var source = CreateScp(revolutions: 1);
            await new ScpWriter().WriteAsync(sourcePath, source);

            await CreateService().ConvertAsync(sourcePath, targetPath);

            var actual = await new HfeReader().ReadAsync(targetPath);
            Assert.Equal(HfeFormat.IsoMfmEncoding, actual.Encoding);
            Assert.Equal(source.Tracks.Count, actual.Tracks.Count);
            foreach (var sourceTrack in source.Tracks)
            {
                var expected = sourceTrack.Revolutions[0];
                var actualTrack = Assert.Single(actual.Tracks, track =>
                    track.Cylinder == sourceTrack.Cylinder && track.Head == sourceTrack.Head);
                Assert.Equal(
                    checked((int)(expected.IndexTimeTicks / actualTrack.BitCellTicks)),
                    actualTrack.Bits.Count);
                Assert.Equal(
                    expected.FluxIntervals,
                    ToIntervals(actualTrack.Bits, actualTrack.BitCellTicks));
            }
        }
        finally
        {
            Delete(sourcePath, targetPath);
        }
    }

    [Fact]
    public async Task MultiRevolutionScpIsNotMisrepresentedAsPreservedHfe()
    {
        var sourcePath = TemporaryPath(".scp");
        var targetPath = TemporaryPath(".hfe");
        try
        {
            await new ScpWriter().WriteAsync(sourcePath, CreateScp(revolutions: 3));

            await Assert.ThrowsAsync<NotSupportedException>(() =>
                CreateService().ConvertAsync(sourcePath, targetPath));
        }
        finally
        {
            Delete(sourcePath, targetPath);
        }
    }

    [Fact]
    public async Task BatchExecutorUsesTheInternalFluxRouteBeforeGw()
    {
        var sourcePath = TemporaryPath(".hfe");
        var targetPath = TemporaryPath(".scp");
        try
        {
            await new HfeWriter().WriteAsync(CreateHfe(), sourcePath);
            var runner = new RecordingRunner();
            var output = new ConversionOutput(
                "raw.scp",
                ".scp",
                targetPath,
                false,
                ConversionFidelityLevel.PreservedFlux);
            var command = new GwCommand("gw.exe", "convert", [sourcePath, targetPath]);

            var result = await new ConversionBatchExecutor(runner)
                .RunAsync(sourcePath, [(output, command)]);

            Assert.False(result.WasCancelled);
            Assert.Equal(0, Assert.Single(result.Items).Result.ExitCode);
            Assert.Equal(0, runner.CallCount);
            Assert.True(File.Exists(targetPath));
        }
        finally
        {
            Delete(sourcePath, targetPath);
        }
    }

    [Fact]
    public void PlannerDeclaresOnlyLosslessFluxRoutesAsPreserved()
    {
        var planner = new ConversionPlanner(new BuiltInImageFormatCatalog());
        var scpCopy = Assert.Single(planner.Plan(
            "source.scp",
            ".",
            "copy",
            [new ConversionSelection("raw.scp", new HashSet<string>())],
            false));
        var hfeToScp = Assert.Single(planner.Plan(
            "source.hfe",
            ".",
            "copy",
            [new ConversionSelection("raw.scp", new HashSet<string>())],
            false));
        var scpToHfe = Assert.Single(planner.Plan(
            "source.scp",
            ".",
            "copy",
            [new ConversionSelection("raw.hfe", new HashSet<string>())],
            false));

        Assert.Equal(ConversionFidelityLevel.PreservedFlux, scpCopy.Fidelity);
        Assert.Equal(ConversionFidelityLevel.PreservedFlux, hfeToScp.Fidelity);
        Assert.Equal(ConversionFidelityLevel.ReconstructedTracks, scpToHfe.Fidelity);
    }

    private static FluxContainerConversionService CreateService() => new(
        new ScpReader(),
        new ScpWriter(),
        new HfeReader(),
        new HfeWriter());

    private static ScpImage CreateScp(int revolutions)
    {
        const uint bitCellTicks = 80;
        var tracks = new List<ScpTrack>();
        for (var cylinder = 0; cylinder < 2; cylinder++)
        {
            for (var head = 0; head < 2; head++)
            {
                var captured = new List<ScpRevolution>();
                for (var revolution = 0; revolution < revolutions; revolution++)
                {
                    var bits = CreateBits(cylinder, head, revolution);
                    var intervals = ToIntervals(bits, bitCellTicks);
                    captured.Add(new ScpRevolution(
                        checked((uint)(bits.Count * bitCellTicks)),
                        checked((uint)intervals.Count),
                        intervals));
                }
                tracks.Add(new ScpTrack(
                    ScpFormatConstants.ToTrackNumber(cylinder, head),
                    cylinder,
                    head,
                    captured));
            }
        }
        var header = new ScpHeader(
            0x19,
            (byte)ScpDiskType.IbmPc720,
            checked((byte)revolutions),
            0,
            3,
            ScpFlags.IndexAligned | ScpFlags.Writable,
            ScpBitCellEncoding.Default16Bit,
            ScpHeadSelection.Both,
            0,
            0);
        return new ScpImage(header, tracks, true, 0);
    }

    private static HfeImage CreateHfe()
    {
        const uint bitCellTicks = 80;
        var tracks = Enumerable.Range(0, 2)
            .SelectMany(cylinder => Enumerable.Range(0, 2)
                .Select(head => new HfeTrack(
                    cylinder,
                    head,
                    CreateBits(cylinder, head, 0),
                    bitCellTicks)))
            .ToArray();
        return new HfeImage(
            HfeFormat.Revision,
            2,
            2,
            HfeFormat.IsoMfmEncoding,
            250,
            tracks);
    }

    private static IReadOnlyList<bool> CreateBits(int cylinder, int head, int revolution)
    {
        var bits = new bool[2048];
        for (var index = 3 + cylinder + head + revolution; index < bits.Length; index += 7)
            bits[index] = true;
        bits[^1] = true;
        return bits;
    }

    private static IReadOnlyList<uint> ToIntervals(
        IReadOnlyList<bool> bits,
        uint bitCellTicks)
    {
        var intervals = new List<uint>();
        uint cells = 0;
        foreach (var bit in bits)
        {
            cells++;
            if (!bit)
                continue;
            intervals.Add(cells * bitCellTicks);
            cells = 0;
        }
        return intervals;
    }

    private static void AssertScpFluxEqual(ScpImage expected, ScpImage actual)
    {
        Assert.Equal(expected.Header with { Checksum = 0 }, actual.Header with { Checksum = 0 });
        Assert.Equal(expected.Tracks.Count, actual.Tracks.Count);
        for (var trackIndex = 0; trackIndex < expected.Tracks.Count; trackIndex++)
        {
            var expectedTrack = expected.Tracks[trackIndex];
            var actualTrack = actual.Tracks[trackIndex];
            Assert.Equal(expectedTrack.TrackNumber, actualTrack.TrackNumber);
            Assert.Equal(expectedTrack.Revolutions.Count, actualTrack.Revolutions.Count);
            for (var revolution = 0; revolution < expectedTrack.Revolutions.Count; revolution++)
            {
                Assert.Equal(
                    expectedTrack.Revolutions[revolution].IndexTimeTicks,
                    actualTrack.Revolutions[revolution].IndexTimeTicks);
                Assert.Equal(
                    expectedTrack.Revolutions[revolution].FluxIntervals,
                    actualTrack.Revolutions[revolution].FluxIntervals);
            }
        }
    }

    private static void AssertHfeFluxEqual(HfeImage expected, HfeImage actual)
    {
        Assert.Equal(expected.Revision, actual.Revision);
        Assert.Equal(expected.Cylinders, actual.Cylinders);
        Assert.Equal(expected.Heads, actual.Heads);
        Assert.Equal(expected.Encoding, actual.Encoding);
        Assert.Equal(expected.BitRate, actual.BitRate);
        Assert.Equal(expected.Tracks.Count, actual.Tracks.Count);
        foreach (var expectedTrack in expected.Tracks)
        {
            var actualTrack = Assert.Single(actual.Tracks, track =>
                track.Cylinder == expectedTrack.Cylinder && track.Head == expectedTrack.Head);
            Assert.Equal(expectedTrack.BitCellTicks, actualTrack.BitCellTicks);
            Assert.Equal(expectedTrack.Bits, actualTrack.Bits);
        }
    }

    private static string TemporaryPath(string extension) => Path.Combine(
        Path.GetTempPath(),
        $"gwgui-flux-{Guid.NewGuid():N}{extension}");

    private static void Delete(params string[] paths)
    {
        foreach (var path in paths)
            if (File.Exists(path))
                File.Delete(path);
    }

    private sealed class RecordingRunner : IGreaseweazleRunner
    {
        public int CallCount { get; private set; }
        public bool IsRunning => false;

        public Task<GwExecutionResult> RunAsync(
            GwCommand command,
            IProgress<GwOutputLine>? progress = null,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(new GwExecutionResult(0, false, TimeSpan.Zero, []));
        }
    }
}
