using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Containers.Scp;

namespace GWGUI.Tests;

public sealed class FluxDecodeModelTests
{
    [Fact]
    public void DecodeEnumsExposeEveryExpectedValue()
    {
        Assert.Equal([FluxStructureKind.Sync, FluxStructureKind.IdAddressMark, FluxStructureKind.DataAddressMark, FluxStructureKind.DeletedDataAddressMark, FluxStructureKind.AmigaSync, FluxStructureKind.AppleAddress, FluxStructureKind.AppleData, FluxStructureKind.CommodoreSync, FluxStructureKind.CommodoreHeader, FluxStructureKind.FormatHeader, FluxStructureKind.FormatData, FluxStructureKind.TimingAnomaly], Enum.GetValues<FluxStructureKind>());
        Assert.Equal([SectorIntegrityKind.Crc, SectorIntegrityKind.Checksum], Enum.GetValues<SectorIntegrityKind>());
    }

    [Fact]
    public void DecodedSectorCopiesDataAndTagSources()
    {
        List<byte> data = [1, 2];
        List<byte> tag = [3, 4];
        var sector = new DecodedSector(0, 0, 1, 0, data.Count, true, 0, Data: data, Tag: tag);

        data[0] = 9;
        tag[0] = 8;

        Assert.Equal([1, 2], sector.Data);
        Assert.Equal([3, 4], sector.Tag);
    }

    [Fact]
    public void FluxDecodeResultCopiesEveryCollectionSource()
    {
        List<FluxStructure> structures = [new(FluxStructureKind.Sync, 0, 1, "sync")];
        List<byte> bytes = [1, 2];
        List<DecodedSector> sectors = [new(0, 0, 1, 0, 2, true, 0)];
        var result = new FluxDecodeResult("decoder", "Decoder", 1, 1, structures, bytes, sectors);

        structures.Clear();
        bytes[0] = 9;
        sectors.Clear();

        Assert.Single(result.Structures);
        Assert.Equal([1, 2], result.DecodedBytes);
        Assert.Single(result.Sectors);
    }

    [Fact]
    public void ExposedCollectionsCannotBeModifiedThroughTheirConcreteContracts()
    {
        var structure = new FluxStructure(FluxStructureKind.Sync, 0, 1, "sync");
        var sector = new DecodedSector(0, 0, 1, 0, 1, true, 0, Data: [1], Tag: [2]);
        var result = new FluxDecodeResult("decoder", "Decoder", 1, 1, [structure], [1], [sector]);

        AssertReadOnly(sector.Data!, (byte)3);
        AssertReadOnly(sector.Tag!, (byte)3);
        AssertReadOnly(result.Structures, structure);
        AssertReadOnly(result.DecodedBytes, (byte)3);
        AssertReadOnly(result.Sectors, sector);
    }

    [Fact]
    public void ResultWithoutSectorsExposesEmptyCollection()
    {
        var result = new FluxDecodeResult("decoder", "Decoder", 0, 0, [], []);

        Assert.NotNull(result.Sectors);
        Assert.Empty(result.Sectors);
        AssertReadOnly(result.Sectors, new DecodedSector(0, 0, 1, 0, 0, null, 0));
    }

    [Fact]
    public void FileSeparationPreservesRawDecoderResultData()
    {
        var intervals = Enumerable.Repeat(80u, 30).ToArray();
        intervals[8] = 5;
        intervals[20] = 900;

        var result = new RawFluxDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Length, intervals));

        Assert.Equal("raw", result.DecoderId);
        Assert.Equal(0.05, result.Confidence);
        Assert.Equal(40, result.EstimatedBitCellTicks);
        Assert.Equal([(FluxStructureKind.TimingAnomaly, 16, 1), (FluxStructureKind.TimingAnomaly, 39, 22)], result.Structures.Select(structure => (structure.Kind, structure.BitOffset, structure.BitLength)));
        Assert.Empty(result.DecodedBytes);
        Assert.Empty(result.Sectors);
    }

    private static void AssertReadOnly<T>(IReadOnlyList<T> values, T value)
    {
        var list = Assert.IsAssignableFrom<IList<T>>(values);
        Assert.True(list.IsReadOnly);
        Assert.Throws<NotSupportedException>(() => list.Add(value));
    }
}
