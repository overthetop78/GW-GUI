using GWGUI.Emulation.Common;
using GWGUI.MediaEngine.Composition;
using GWGUI.MediaEngine.Containers.Atari.Msa;
using GWGUI.MediaEngine.Containers.Atari.St;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Hfs;
using GWGUI.MediaEngine.Geometries.Atari;
using System.Buffers.Binary;
using System.IO;

namespace GWGUI.Tests;

/// <summary>Vérifie les conversions internes Atari ST et MSA sur toutes les géométries cataloguées.</summary>
public sealed class AtariStWriterTests
{
    /// <summary>Retourne les neuf formats Atari ST pris en charge.</summary>
    public static TheoryData<string> Formats => new()
    {
        DiskImageFormatIds.AtariSt180,
        DiskImageFormatIds.AtariSt360,
        DiskImageFormatIds.AtariSt400,
        DiskImageFormatIds.AtariSt440,
        DiskImageFormatIds.AtariSt720,
        DiskImageFormatIds.AtariSt800,
        DiskImageFormatIds.AtariSt810,
        DiskImageFormatIds.AtariSt880,
        DiskImageFormatIds.AtariSt1440
    };

    /// <summary>Vérifie qu'une source brute complète reste identique après conversion ST.</summary>
    [Theory]
    [MemberData(nameof(Formats))]
    public async Task WritesEveryCataloguedGeometryAsExactSt(string formatId)
    {
        Assert.True(AtariStGeometry.TryFromFormatId(formatId, out var geometry));
        var source = TemporaryPath(".st");
        var output = TemporaryPath(".st");
        var expected = DeterministicBytes(geometry.Capacity);
        try
        {
            await File.WriteAllBytesAsync(source, expected);
            await MediaEngineFactory.CreateAtariStConversionService().ConvertAsync(source, output, formatId);
            Assert.Equal(expected, await File.ReadAllBytesAsync(output));
            var image = await new AtariStReader().ReadAsync(output);
            Assert.Equal(formatId, image.FormatId);
            Assert.Empty(image.MissingBlocks);
        }
        finally
        {
            File.Delete(source);
            File.Delete(output);
        }
    }

    /// <summary>Vérifie la relecture exacte des pistes MSA brutes et compressées.</summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task WritesRawAndCompressedMsaTracks(bool compressible)
    {
        Assert.True(AtariStGeometry.TryFromFormatId(DiskImageFormatIds.AtariSt720, out var geometry));
        var source = TemporaryPath(".st");
        var output = TemporaryPath(".msa");
        var expected = compressible ? new byte[geometry.Capacity] : DeterministicBytes(geometry.Capacity);
        try
        {
            await File.WriteAllBytesAsync(source, expected);
            await MediaEngineFactory.CreateAtariStConversionService().ConvertAsync(source, output, DiskImageFormatIds.AtariSt720);
            var container = await File.ReadAllBytesAsync(output);
            var firstTrackLength = BinaryPrimitives.ReadUInt16BigEndian(container.AsSpan(10, 2));
            Assert.Equal(compressible, firstTrackLength < geometry.SectorsPerTrack * AtariStGeometry.SectorSize);
            var image = await new MsaReader().ReadAsync(output);
            Assert.Equal(expected, image.AvailableBlocks.OrderBy(block => block.LogicalBlock).SelectMany(block => block.Data).ToArray());
        }
        finally
        {
            File.Delete(source);
            File.Delete(output);
        }
    }

    /// <summary>Vérifie le trajet ST vers MSA puis MSA vers ST sur chaque géométrie cataloguée.</summary>
    [Theory]
    [MemberData(nameof(Formats))]
    public async Task RoundTripsEveryCataloguedGeometryBetweenStAndMsa(string formatId)
    {
        Assert.True(AtariStGeometry.TryFromFormatId(formatId, out var geometry));
        var source = TemporaryPath(".st");
        var intermediate = TemporaryPath(".msa");
        var output = TemporaryPath(".st");
        var expected = DeterministicBytes(geometry.Capacity);
        try
        {
            await File.WriteAllBytesAsync(source, expected);
            var service = MediaEngineFactory.CreateAtariStConversionService();
            await service.ConvertAsync(source, intermediate, formatId);
            await service.ConvertAsync(intermediate, output, formatId);
            var msaImage = await new MsaReader().ReadAsync(intermediate);
            var stImage = await new AtariStReader().ReadAsync(output);
            Assert.Equal(formatId, msaImage.FormatId);
            Assert.Equal(formatId, stImage.FormatId);
            Assert.Equal(geometry.Cylinders, msaImage.Cylinders);
            Assert.Equal(geometry.Heads, msaImage.Heads);
            Assert.Equal(geometry.SectorsPerTrack, msaImage.SectorsPerTrack);
            Assert.Equal(msaImage.AvailableBlocks.Count, stImage.AvailableBlocks.Count);
            foreach (var expectedBlock in msaImage.AvailableBlocks.OrderBy(block => block.LogicalBlock))
            {
                var actualBlock = Assert.Single(stImage.AvailableBlocks, block => block.LogicalBlock == expectedBlock.LogicalBlock);
                Assert.Equal(expectedBlock.Data, actualBlock.Data);
            }
            Assert.Equal(expected, await File.ReadAllBytesAsync(output));
        }
        finally
        {
            File.Delete(source);
            File.Delete(intermediate);
            File.Delete(output);
        }
    }

    /// <summary>Vérifie qu'un changement de capacité est refusé sans écraser la destination.</summary>
    [Fact]
    public async Task RejectsLossyGeometryChangeWithoutReplacingDestination()
    {
        Assert.True(AtariStGeometry.TryFromFormatId(DiskImageFormatIds.AtariSt720, out var geometry));
        var source = TemporaryPath(".st");
        var output = TemporaryPath(".st");
        var preserved = new byte[] { 1, 2, 3, 4 };
        try
        {
            await File.WriteAllBytesAsync(source, DeterministicBytes(geometry.Capacity));
            await File.WriteAllBytesAsync(output, preserved);
            await Assert.ThrowsAsync<InvalidDataException>(() => MediaEngineFactory.CreateAtariStConversionService().ConvertAsync(source, output, DiskImageFormatIds.AtariSt800));
            Assert.Equal(preserved, await File.ReadAllBytesAsync(output));
        }
        finally
        {
            File.Delete(source);
            File.Delete(output);
        }
    }

    /// <summary>Produit un contenu déterministe sans BPB valide et peu compressible.</summary>
    private static byte[] DeterministicBytes(int length)
    {
        var data = new byte[length];
        for (var index = 0; index < data.Length; index++) data[index] = (byte)((index * 73 + index / 512 * 29) & 0xFF);
        return data;
    }

    /// <summary>Crée un chemin temporaire portant l'extension demandée.</summary>
    private static string TemporaryPath(string extension) => Path.Combine(Path.GetTempPath(), $"gwgui-{Guid.NewGuid():N}{extension}");
}
