using GWGUI.MediaEngine.Composition;
using GWGUI.MediaEngine.Containers.Atari.Atr;
using GWGUI.MediaEngine.Conversion.Atari;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Hfs;
using System.Buffers.Binary;
using System.IO;

namespace GWGUI.Tests;

/// <summary>Vérifie l'écriture et le raccordement des trois profils ATR Atari 8-bit.</summary>
public sealed class AtrWriterTests
{
    /// <summary>Vérifie l'en-tête, les tailles sectorielles et la relecture exacte des profils connus.</summary>
    [Theory]
    [InlineData(DiskImageFormatIds.Atari90, AtrLayout.SingleDensitySectorSize, AtrLayout.StandardSectorCount)]
    [InlineData(DiskImageFormatIds.Atari130, AtrLayout.SingleDensitySectorSize, AtrLayout.EnhancedDensitySectorCount)]
    [InlineData(DiskImageFormatIds.Atari180, AtrLayout.DoubleDensitySectorSize, AtrLayout.StandardSectorCount)]
    public async Task WritesCompleteContainerAndPreservesEverySector(string formatId, int sectorSize, int sectorCount)
    {
        var source = TemporaryPath();
        var output = TemporaryPath();
        var expected = CreateContainer(sectorSize, sectorCount);
        try
        {
            await File.WriteAllBytesAsync(source, expected);
            await MediaEngineFactory.CreateAtrConversionService().ConvertAsync(source, output, formatId);
            Assert.Equal(expected, await File.ReadAllBytesAsync(output));
            var image = await new AtrReader().ReadAsync(output);
            Assert.Equal(formatId, image.FormatId);
            Assert.Equal(sectorCount, image.BlockCount);
            Assert.Empty(image.MissingBlocks);
            Assert.Equal(AtrLayout.BootSectorSize, image.GetBlock(0).Length);
            Assert.Equal(sectorSize, image.GetBlock(sectorCount - 1).Length);
        }
        finally
        {
            File.Delete(source);
            File.Delete(output);
        }
    }

    /// <summary>Vérifie le routage strict de la façade interne ATR.</summary>
    [Fact]
    public void RoutesOnlyCataloguedAtrTargets()
    {
        Assert.True(AtrConversionService.CanCreate(DiskImageFormatIds.Atari90, DiskImageFileExtensions.Atr));
        Assert.True(AtrConversionService.CanCreate(DiskImageFormatIds.Atari130, DiskImageFileExtensions.Atr));
        Assert.True(AtrConversionService.CanCreate(DiskImageFormatIds.Atari180, DiskImageFileExtensions.Atr));
        Assert.False(AtrConversionService.CanCreate(DiskImageFormatIds.AtariSt180, DiskImageFileExtensions.Atr));
        Assert.False(AtrConversionService.CanCreate(DiskImageFormatIds.Atari90, DiskImageFileExtensions.St));
    }

    /// <summary>Construit un conteneur ATR déterministe conforme au profil demandé.</summary>
    private static byte[] CreateContainer(int sectorSize, int sectorCount)
    {
        var payloadLength = AtrLayout.GetBootAreaLength(sectorSize) + (sectorCount - (sectorSize == AtrLayout.SingleDensitySectorSize ? 0 : AtrLayout.BootSectorCount)) * sectorSize;
        var data = new byte[AtrLayout.HeaderSize + payloadLength];
        var paragraphs = payloadLength / AtrLayout.ParagraphSize;
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(AtrLayout.SignatureOffset), AtrFormat.Signature);
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(AtrLayout.ParagraphCountLowOffset), unchecked((ushort)paragraphs));
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(AtrLayout.SectorSizeOffset), checked((ushort)sectorSize));
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(AtrLayout.ParagraphCountHighOffset), checked((ushort)(paragraphs >> AtrLayout.ParagraphCountHighWordShift)));
        for (var index = AtrLayout.HeaderSize; index < data.Length; index++) data[index] = (byte)((index * 37 + index / 128 * 11) & 0xFF);
        return data;
    }

    /// <summary>Crée un chemin ATR temporaire.</summary>
    private static string TemporaryPath() => Path.Combine(Path.GetTempPath(), $"gwgui-{Guid.NewGuid():N}.atr");
}
