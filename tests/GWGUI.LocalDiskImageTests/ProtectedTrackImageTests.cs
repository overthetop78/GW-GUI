using System.IO;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.TrackImages;

namespace GWGUI.Tests;

/// <summary>Vérifie que le contrat de piste conserve les informations impossibles à porter dans SectorImage.</summary>
public sealed class ProtectedTrackImageTests
{
    [Fact]
    public void TrackPreservesMarksGapsErrorsTimingWeakRegionsAndFlux()
    {
        var bits = Enumerable.Range(0, 64).Select(index => index % 3 == 0).ToArray();
        var features = new[]
        {
            new TrackFeature(TrackFeatureKind.IndexMark, 0, 1),
            new TrackFeature(TrackFeatureKind.Gap, 20, 12),
            new TrackFeature(TrackFeatureKind.IntentionalChecksumError, 32, 8),
            new TrackFeature(TrackFeatureKind.WeakRegion, 40, 16)
        };
        var structures = new[] { new FluxStructure(FluxStructureKind.IdAddressMark, 4, 8, "IDAM"), new FluxStructure(FluxStructureKind.DataAddressMark, 12, 8, "DAM") };
        var timing = new[] { new TrackTimingSegment(0, 32, 2_000), new TrackTimingSegment(32, 32, 2_100) };
        var flux = new FluxRevolution(128, [8, 16, 24, 32, 48]);
        var track = new ProtectedTrack(4, 1, bits, timing, structures, features, [new(25, flux)]);
        bits[0] = !bits[0];
        var image = new ProtectedTrackImage([track], true);
        Assert.True(track.Bits![0]);
        Assert.Equal(Enum.GetValues<TrackFeatureKind>(), track.Features.Select(feature => feature.Kind));
        Assert.Equal([FluxStructureKind.IdAddressMark, FluxStructureKind.DataAddressMark], track.Structures.Select(structure => structure.Kind));
        Assert.Equal([2_000d, 2_100d], track.Timing.Select(segment => segment.BitCellNanoseconds));
        Assert.Equal([8u, 16u, 24u, 32u, 48u], track.Revolutions[0].Flux.FluxIntervals);
        Assert.True(image.WriteProtected);
    }

    [Fact]
    public void ContractRejectsMissingRepresentationsInvalidRangesAndDuplicateTracks()
    {
        Assert.Throws<ArgumentException>(() => new ProtectedTrack(0, 0, null, [], [], [], []));
        Assert.Throws<ArgumentException>(() => new ProtectedTrack(0, 0, null, [new(0, 1, 2_000)], [], [], [new(25, new FluxRevolution(1, [1]))]));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ProtectedTrack(0, 0, [true], [], [], [new(TrackFeatureKind.WeakRegion, 1, 1)], []));
        var track = new ProtectedTrack(0, 0, [true], [new(0, 1, 2_000)], [], [], []);
        Assert.Throws<InvalidDataException>(() => new ProtectedTrackImage([track, track], false));
    }

    [Fact]
    public void ScpAdapterPreservesEveryRawRevolutionAndItsResolution()
    {
        var first = new ScpRevolution(100, 3, [10, 20, 30]);
        var second = new ScpRevolution(120, 2, [40, 80]);
        var header = new ScpHeader(0x24, 0, 2, 0, 0, ScpFlags.IndexAligned, ScpBitCellEncoding.Default16Bit, ScpHeadSelection.Side0, 1, 0);
        var source = new ScpImage(header, [new ScpTrack(0, 0, 0, [first, second])], true, 100);
        var image = ScpProtectedTrackImageAdapter.Create(source);
        var track = Assert.Single(image.Tracks);
        Assert.Null(track.Bits);
        Assert.Equal(2, track.Revolutions.Count);
        Assert.All(track.Revolutions, revolution => Assert.Equal(50, revolution.ResolutionNanoseconds));
        Assert.Same(first.Flux, track.Revolutions[0].Flux);
        Assert.Same(second.Flux, track.Revolutions[1].Flux);
        Assert.True(image.WriteProtected);
    }
}
