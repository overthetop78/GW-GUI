using GWGUI.Emulation.Common;
using GWGUI.MediaEngine.Containers.Adf;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Conversion.Amiga;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Geometries.Amiga;
using GWGUI.MediaEngine.Reconstruction;
using GWGUI.MediaEngine.Reconstruction.Amiga;
using GWGUI.MediaEngine.SectorImages;
using System.IO;

namespace GWGUI.Tests;

public sealed class AmigaAdfWriterTests
{
    [Theory]
    [InlineData(DiskImageFormatIds.AmigaDos)]
    [InlineData(DiskImageFormatIds.AmigaDosHighDensity)]
    public async Task WriterRoundTripsEveryLogicalSector(string formatId)
    {
        var geometry = Geometry(formatId);
        var source = CreateImage(geometry);
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-{Guid.NewGuid():N}.adf");
        try
        {
            await new AmigaAdfWriter().WriteAsync(source, path);
            var reopened = await new AdfReader().ReadAsync(path);

            Assert.Equal(source.FormatId, reopened.FormatId);
            Assert.Equal(source.BlockCount, reopened.BlockCount);
            for (var logicalBlock = 0; logicalBlock < source.BlockCount; logicalBlock++) Assert.Equal(source.GetBlock(logicalBlock).ToArray(), reopened.GetBlock(logicalBlock).ToArray());
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task ConversionServiceWritesAnExistingSectorImageAndRejectsAMismatchedTarget()
    {
        var source = CreateImage(AmigaAdfGeometry.DoubleDensity);
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-{Guid.NewGuid():N}.adf");
        var service = new AmigaAdfConversionService(null!, new AdfReader(), new AmigaAdfWriter());
        try
        {
            await service.ConvertAsync(source, path, DiskImageFormatIds.AmigaDos);
            Assert.Equal(AmigaAdfGeometry.DoubleDensityCapacity, new FileInfo(path).Length);
            await Assert.ThrowsAsync<InvalidDataException>(() => service.ConvertAsync(source, path, DiskImageFormatIds.AmigaDosHighDensity));
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task ConversionServiceReconstructsAnScpSourceBeforeWritingAdf()
    {
        var source = CreateImage(AmigaAdfGeometry.DoubleDensity);
        var scp = CreateScp(source);
        var service = new AmigaAdfConversionService(new AmigaScpSectorImageReader(new MemoryScpReader(scp), new FluxDecoderRegistry()), new AdfReader(), new AmigaAdfWriter());
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-{Guid.NewGuid():N}.adf");
        try
        {
            await service.ConvertAsync("memory.scp", path, DiskImageFormatIds.AmigaDos);
            var reopened = await new AdfReader().ReadAsync(path);
            for (var logicalBlock = 0; logicalBlock < source.BlockCount; logicalBlock++) Assert.Equal(source.GetBlock(logicalBlock).ToArray(), reopened.GetBlock(logicalBlock).ToArray());
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task WriterRefusesAMissingBlockWithoutReplacingTheDestination()
    {
        var complete = CreateImage(AmigaAdfGeometry.DoubleDensity);
        var incomplete = new SectorImage(complete.FormatId, complete.BlockSize, complete.Cylinders, complete.Heads, complete.SectorsPerTrack, complete.AvailableBlocks.Where(block => block.LogicalBlock != 17));
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-{Guid.NewGuid():N}.adf");
        var original = "unchanged"u8.ToArray();
        try
        {
            await File.WriteAllBytesAsync(path, original);
            await Assert.ThrowsAsync<InvalidDataException>(() => new AmigaAdfWriter().WriteAsync(incomplete, path));
            Assert.Equal(original, await File.ReadAllBytesAsync(path));
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Theory]
    [InlineData(DiskImageFormatIds.AmigaDos, ".adf", true)]
    [InlineData(DiskImageFormatIds.AmigaDosHighDensity, ".ADF", true)]
    [InlineData(DiskImageFormatIds.AmigaDos, ".img", false)]
    [InlineData(DiskImageFormatIds.AtariSt720, ".adf", false)]
    public void InternalRoutingRequiresAnAmigaAdfTarget(string formatId, string extension, bool expected) => Assert.Equal(expected, AmigaAdfConversionService.CanCreate(formatId, extension));

    private static RegularSectorGeometry Geometry(string formatId) => formatId == DiskImageFormatIds.AmigaDos ? AmigaAdfGeometry.DoubleDensity : AmigaAdfGeometry.HighDensity;

    private static SectorImage CreateImage(RegularSectorGeometry geometry)
    {
        var blocks = Enumerable.Range(0, geometry.BlockCount).Select(logicalBlock =>
        {
            var track = logicalBlock / geometry.SectorsPerTrack;
            var data = Enumerable.Range(0, geometry.BlockSize).Select(index => (byte)(logicalBlock * 17 + index * 31)).ToArray();
            return new SectorBlock(logicalBlock, new(track / geometry.Heads, track % geometry.Heads, logicalBlock % geometry.SectorsPerTrack), data);
        });
        return new(geometry.FormatId, geometry.BlockSize, geometry.Cylinders, geometry.Heads, geometry.SectorsPerTrack, blocks);
    }

    private static ScpImage CreateScp(SectorImage image)
    {
        var encoder = new AmigaMfmTrackEncoder();
        var tracks = new List<ScpTrack>();
        for (var cylinder = 0; cylinder < image.Cylinders; cylinder++)
        {
            for (var head = 0; head < image.Heads; head++)
            {
                var track = cylinder * image.Heads + head;
                var sectors = Enumerable.Range(0, image.SectorsPerTrack).Select(number => new TrackSector(number, image.GetBlock(track * image.SectorsPerTrack + number).ToArray())).ToArray();
                var encoded = encoder.Encode(new(cylinder, head, sectors));
                var trackNumber = checked((byte)track);
                tracks.Add(new(trackNumber, cylinder, head, [new ScpRevolution(encoded.Revolution, (uint)encoded.Revolution.FluxIntervals.Count)]));
            }
        }
        return new(new(0, 0, 1, 0, checked((byte)(tracks.Count - 1)), ScpFlags.IndexAligned, ScpBitCellEncoding.Explicit16Bit, ScpHeadSelection.Both, 0, 0), tracks, true, 0);
    }

    private sealed class MemoryScpReader(ScpImage image) : IScpReader
    {
        public Task<ScpImage> ReadAsync(string path, CancellationToken cancellationToken = default) => Task.FromResult(image);
    }
}
