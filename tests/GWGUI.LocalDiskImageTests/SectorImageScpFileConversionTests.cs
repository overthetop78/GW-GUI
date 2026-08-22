using GWGUI.App.Services.Conversion;
using GWGUI.Domain.Conversion;
using GWGUI.Emulation.Common;
using GWGUI.MediaEngine.Composition;
using GWGUI.MediaEngine.Containers.Adf;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Conversion.Scp;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Hfs;
using GWGUI.MediaEngine.Geometries.Amiga;
using GWGUI.MediaEngine.Reconstruction;
using GWGUI.MediaEngine.SectorImages;
using System.IO;

namespace GWGUI.Tests;

/// <summary>Valide la conversion permanente d'un fichier sectoriel en SCP sans exécutable externe.</summary>
public sealed class SectorImageScpFileConversionTests
{
    [Fact]
    public async Task AdfFileIsRecognizedAndReconstructedAsScp()
    {
        var sourcePath = Path.Combine(Path.GetTempPath(), $"gwgui-source-{Guid.NewGuid():N}.adf");
        var outputPath = Path.Combine(Path.GetTempPath(), $"gwgui-output-{Guid.NewGuid():N}.scp");
        try
        {
            var image = CreateImage(AmigaAdfGeometry.DoubleDensity);
            await new AmigaAdfWriter().WriteAsync(image, sourcePath);
            await MediaEngineFactory.CreateSectorImageScpFileConversionService().ConvertAsync(sourcePath, outputPath);

            var scp = await new ScpReader().ReadAsync(outputPath);
            Assert.Equal((byte)ScpDiskType.Amiga, scp.Header.DiskType);
            Assert.Equal(image.Cylinders * image.Heads, scp.Tracks.Count);
        }
        finally
        {
            if (File.Exists(sourcePath)) File.Delete(sourcePath);
            if (File.Exists(outputPath)) File.Delete(outputPath);
        }
    }

    [Theory]
    [InlineData(DiskImageFormatIds.RawScp, DiskImageFileExtensions.Scp, true)]
    [InlineData(DiskImageFormatIds.RawScp, DiskImageFileExtensions.Adf, false)]
    [InlineData(DiskImageFormatIds.AmigaDos, DiskImageFileExtensions.Scp, false)]
    public void InternalRoutingRequiresTheScpTarget(string formatId, string extension, bool expected) =>
        Assert.Equal(expected, SectorImageScpFileConversionService.CanCreate(formatId, extension));

    [Fact]
    public void ScpDestinationIsRoutedInternally() =>
        Assert.True(ConversionBatchExecutor.IsInternal(new ConversionOutput(DiskImageFormatIds.RawScp, DiskImageFileExtensions.Scp, "output.scp", false)));

    private static SectorImage CreateImage(RegularSectorGeometry geometry)
    {
        var blocks = Enumerable.Range(0, geometry.BlockCount).Select(logicalBlock =>
        {
            var track = logicalBlock / geometry.SectorsPerTrack;
            var data = Enumerable.Range(0, geometry.BlockSize).Select(index => unchecked((byte)(logicalBlock * 17 + index * 31))).ToArray();
            return new SectorBlock(logicalBlock, new(track / geometry.Heads, track % geometry.Heads, logicalBlock % geometry.SectorsPerTrack), data);
        });
        return new(geometry.FormatId, geometry.BlockSize, geometry.Cylinders, geometry.Heads, geometry.SectorsPerTrack, blocks);
    }
}
