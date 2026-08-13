using GWGUI.MediaEngine.Composition;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.SectorImages;
using System.IO;

namespace GWGUI.Tests;

/// <summary>Valide la création SCP commune aux familles Amiga et ISO FM/MFM.</summary>
public sealed class SectorImageScpConversionServiceTests
{
    public static TheoryData<string, int, int, int, int, string, uint, uint, ScpDiskType> SupportedFormats => new()
    {
        { DiskImageFormatIds.AmigaDos, 80, 2, 11, 0, FluxCodecIds.AmigaMfm, TrackEncodingTimings.HighDensityMfmBitCellTicks, TrackEncodingTimings.Rpm300IndexTimeTicks, ScpDiskType.Amiga },
        { DiskImageFormatIds.AmigaDosHighDensity, 80, 2, 22, 0, FluxCodecIds.AmigaMfm, TrackEncodingTimings.ExtraDensityMfmBitCellTicks, TrackEncodingTimings.Rpm300IndexTimeTicks, ScpDiskType.AmigaHighDensity },
        { DiskImageFormatIds.AtariSt720, 80, 2, 9, 1, FluxCodecIds.IsoMfm, TrackEncodingTimings.DoubleDensityMfmBitCellTicks, TrackEncodingTimings.Rpm300IndexTimeTicks, ScpDiskType.AtariStDoubleSided },
        { DiskImageFormatIds.Atari90, 40, 1, 18, 1, FluxCodecIds.IsoFm, TrackEncodingTimings.SingleDensityFmBitCellTicks, TrackEncodingTimings.Rpm300IndexTimeTicks, ScpDiskType.Atari8BitSingleDensity },
        { DiskImageFormatIds.Ibm720, 80, 2, 9, 1, FluxCodecIds.IsoMfm, TrackEncodingTimings.DoubleDensityMfmBitCellTicks, TrackEncodingTimings.Rpm300IndexTimeTicks, ScpDiskType.IbmPc720 },
        { DiskImageFormatIds.Ibm1200, 80, 2, 15, 1, FluxCodecIds.IsoMfm, TrackEncodingTimings.HighDensityMfmBitCellTicks, TrackEncodingTimings.Rpm360IndexTimeTicks, ScpDiskType.IbmPc1200 },
        { DiskImageFormatIds.Msx2Dd, 80, 2, 9, 1, FluxCodecIds.IsoMfm, TrackEncodingTimings.DoubleDensityMfmBitCellTicks, TrackEncodingTimings.Rpm300IndexTimeTicks, ScpDiskType.Other720 },
        { DiskImageFormatIds.AcornDfsSingleSided80, 80, 1, 10, 0, FluxCodecIds.IsoFm, TrackEncodingTimings.SingleDensityFmBitCellTicks, TrackEncodingTimings.Rpm300IndexTimeTicks, ScpDiskType.Other320 },
        { DiskImageFormatIds.AcornAdfs800, 80, 2, 5, 0, FluxCodecIds.IsoMfm, TrackEncodingTimings.DoubleDensityMfmBitCellTicks, TrackEncodingTimings.Rpm300IndexTimeTicks, ScpDiskType.Other720 },
        { DiskImageFormatIds.AmstradCpc, 40, 1, 9, 0xC1, FluxCodecIds.IsoMfm, TrackEncodingTimings.DoubleDensityMfmBitCellTicks, TrackEncodingTimings.Rpm300IndexTimeTicks, ScpDiskType.AmstradCpc },
        { DiskImageFormatIds.EpsonQx10_400, 80, 1, 10, 1, FluxCodecIds.IsoMfm, TrackEncodingTimings.DoubleDensityMfmBitCellTicks, TrackEncodingTimings.Rpm300IndexTimeTicks, ScpDiskType.Other320 }
    };

    [Theory]
    [MemberData(nameof(SupportedFormats))]
    public async Task WritesAndRedecodesEverySupportedFamily(string formatId, int cylinders, int heads, int sectorsPerTrack, int firstSector, string codecId, uint bitCellTicks, uint indexTimeTicks, ScpDiskType diskType)
    {
        var source = CreateTrackImage(formatId, cylinders, heads, sectorsPerTrack, firstSector);
        var outputPath = Path.Combine(Path.GetTempPath(), $"gwgui-sector-scp-{Guid.NewGuid():N}.scp");
        try
        {
            await MediaEngineFactory.CreateSectorImageScpConversionService().ConvertAsync(source, outputPath);
            var scp = await new ScpReader().ReadAsync(outputPath);
            Assert.Equal((byte)diskType, scp.Header.DiskType);
            Assert.Equal(indexTimeTicks == TrackEncodingTimings.Rpm360IndexTimeTicks, scp.Header.Flags.HasFlag(ScpFlags.Rpm360));
            var track = Assert.Single(scp.Tracks);
            var revolution = Assert.Single(track.Revolutions);
            Assert.Equal(indexTimeTicks, revolution.IndexTimeTicks);
            var decoded = new FluxDecoderRegistry().Decode(codecId, revolution.Flux);
            var sectors = decoded.Sectors.Where(sector => sector.IntegrityValid == true && sector.Data is not null).ToDictionary(sector => sector.Number);
            foreach (var block in source.AvailableBlocks)
            {
                Assert.True(sectors.TryGetValue(block.Address.Number, out var sector), $"Secteur SCP absent : {block.Address}.");
                Assert.Equal(block.Data, sector.Data);
            }
            Assert.Equal(bitCellTicks, SectorImageTrackTimingCatalog.BitCellTicks(formatId));
        }
        finally
        {
            if (File.Exists(outputPath)) File.Delete(outputPath);
        }
    }

    [Theory]
    [InlineData(DiskImageFormatIds.Ibm160, ScpDiskType.IbmPc360)]
    [InlineData(DiskImageFormatIds.Ibm180, ScpDiskType.IbmPc360)]
    [InlineData(DiskImageFormatIds.Ibm320, ScpDiskType.IbmPc360)]
    [InlineData(DiskImageFormatIds.Ibm360, ScpDiskType.IbmPc360)]
    [InlineData(DiskImageFormatIds.Ibm800, ScpDiskType.IbmPc720)]
    [InlineData(DiskImageFormatIds.Ibm1440, ScpDiskType.IbmPc1440)]
    [InlineData(DiskImageFormatIds.Ibm1680, ScpDiskType.IbmPc1440)]
    [InlineData(DiskImageFormatIds.IbmDmf, ScpDiskType.IbmPc1440)]
    [InlineData(DiskImageFormatIds.Ibm2880, ScpDiskType.IbmPc1440)]
    public void UsesTheClosestDocumentedScpDiskTypeForEveryIbmGeometry(string formatId, ScpDiskType diskType)
    {
        var image = CreateTrackImage(formatId, 80, 2, 1, 1);
        var scp = MediaEngineFactory.CreateSectorImageScpConversionService().Create(image);
        Assert.Equal((byte)diskType, scp.Header.DiskType);
    }

    private static SectorImage CreateTrackImage(string formatId, int cylinders, int heads, int sectorsPerTrack, int firstSector)
    {
        var sectorSize = formatId.StartsWith(DiskImageFormatIds.AcornDfsPrefix, StringComparison.OrdinalIgnoreCase) || formatId.Equals(DiskImageFormatIds.Atari90, StringComparison.OrdinalIgnoreCase) ? 256 : formatId.Equals(DiskImageFormatIds.AcornAdfs800, StringComparison.OrdinalIgnoreCase) ? 1024 : 512;
        var blocks = Enumerable.Range(0, sectorsPerTrack).Select(index => new SectorBlock(index, new(0, 0, firstSector + index), Enumerable.Repeat(checked((byte)(index + 1)), sectorSize).ToArray())).ToArray();
        return new(formatId, sectorSize, cylinders, heads, sectorsPerTrack, blocks);
    }
}
