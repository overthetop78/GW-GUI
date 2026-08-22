using GWGUI.MediaEngine.Containers.Atari.Atr;
using GWGUI.MediaEngine.Conversion.Atari;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Hfs;
using System.Buffers.Binary;
using System.IO;

namespace GWGUI.Tests;

/// <summary>Vérifie la lecture publique et l'extraction des conteneurs ATR locaux.</summary>
public sealed class AtrReaderTests
{
    /// <summary>Vérifie la longueur de zone d'amorçage associée à chaque taille sectorielle ATR.</summary>
    [Theory]
    [InlineData(AtrLayout.SingleDensitySectorSize, 0)]
    [InlineData(AtrLayout.DoubleDensitySectorSize, AtrLayout.BootSectorCount * AtrLayout.BootSectorSize)]
    [InlineData(AtrLayout.ExtendedSectorSize, AtrLayout.BootSectorCount * AtrLayout.BootSectorSize)]
    public void ComputesBootAreaLength(int sectorSize, int expectedLength) => Assert.Equal(expectedLength, AtrLayout.GetBootAreaLength(sectorSize));

    /// <summary>Vérifie les identifiants des trois géométries ATR reconnues explicitement.</summary>
    [Theory]
    [InlineData(AtrLayout.SingleDensitySectorSize, AtrLayout.StandardSectorCount, DiskImageFormatIds.Atari90)]
    [InlineData(AtrLayout.SingleDensitySectorSize, AtrLayout.EnhancedDensitySectorCount, DiskImageFormatIds.Atari130)]
    [InlineData(AtrLayout.DoubleDensitySectorSize, AtrLayout.StandardSectorCount, DiskImageFormatIds.Atari180)]
    public void PreservesKnownFormatIdentifiers(int sectorSize, int sectorCount, string expectedFormatId) => Assert.Equal(expectedFormatId, AtrFormat.GetFormatId(sectorSize, sectorCount));

    [Theory]
    [InlineData("validated_images/Atari/Atari 130XE/5.25 pouces - Chargeur propriétaire - 90 Kio/seeds-of-evil-atari-130xe.atr", 128, 720, DiskImageFormatIds.Atari90)]
    [InlineData("Atari 8-bit/os xl-xe.atr", 256, 720, DiskImageFormatIds.Atari180)]
    [InlineData("_generated/atari-512-720.atr", 512, 720, "atari.atr.512.720")]
    public async Task ReadsHeaderGeometryAddressesCapacityAndSectorContents(
        string relativePath,
        int expectedSectorSize,
        int expectedSectorCount,
        string expectedFormatId)
    {
        var path = ImagePath(relativePath);
        var bytes = await File.ReadAllBytesAsync(path);
        var paragraphCount = ((long)BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(6)) << 16)
            | BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(2));

        Assert.Equal(0x0296, BinaryPrimitives.ReadUInt16LittleEndian(bytes));
        Assert.Equal(bytes.Length - 16, paragraphCount * 16);
        Assert.Equal(expectedSectorSize, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));

        var image = await new AtrReader().ReadAsync(path);

        Assert.Equal(expectedSectorSize, image.BlockSize);
        Assert.Equal(expectedSectorCount, image.BlockCount);
        Assert.Equal(expectedFormatId, image.FormatId);
        Assert.Equal((expectedSectorCount, AtrLayout.LogicalHeadCount, AtrLayout.LogicalSectorsPerCylinder), (image.Cylinders, image.Heads, image.SectorsPerTrack));
        Assert.Equal(bytes.Length - 16, image.Capacity);
        Assert.Equal(Enumerable.Range(0, expectedSectorCount), image.AvailableBlocks.Select(block => block.LogicalBlock));
        Assert.Equal(Enumerable.Range(1, expectedSectorCount), image.AvailableBlocks.Select(block => block.Address.Number));

        var sourceOffset = 16;
        foreach (var block in image.AvailableBlocks)
        {
            var expectedLength = block.Address.Number <= 3 ? 128 : expectedSectorSize;
            Assert.Equal(expectedLength, block.Data.Count);
            Assert.Equal(bytes.AsSpan(sourceOffset, expectedLength).ToArray(), block.Data.ToArray());
            sourceOffset += expectedLength;
        }
        Assert.Equal(bytes.Length, sourceOffset);
    }

    [Theory]
    [InlineData("validated_images/Atari/Atari 130XE/5.25 pouces - Chargeur propriétaire - 90 Kio/seeds-of-evil-atari-130xe.atr")]
    [InlineData("Atari 8-bit/os xl-xe.atr")]
    [InlineData("_generated/atari-512-720.atr")]
    public async Task ExtractsPayloadAndAllowsSectorBySectorRereading(string relativePath)
    {
        var sourcePath = ImagePath(relativePath);
        var destinationPath = Path.Combine(Path.GetTempPath(), $"gwgui-atr-{Guid.NewGuid():N}.img");
        try
        {
            await AtrPayloadWriter.WriteRawPayloadAsync(sourcePath, destinationPath);

            var container = await File.ReadAllBytesAsync(sourcePath);
            var payload = await File.ReadAllBytesAsync(destinationPath);
            Assert.Equal(container.AsSpan(16).ToArray(), payload);

            var image = await new AtrReader().ReadAsync(sourcePath);
            var offset = 0;
            foreach (var block in image.AvailableBlocks)
            {
                Assert.Equal(block.Data.ToArray(), payload.AsSpan(offset, block.Data.Count).ToArray());
                offset += block.Data.Count;
            }
            Assert.Equal(payload.Length, offset);
        }
        finally
        {
            if (File.Exists(destinationPath)) File.Delete(destinationPath);
        }
    }

    /// <summary>Vérifie que le mot haut du nombre de paragraphes participe à la longueur déclarée.</summary>
    [Fact]
    public async Task ReadsParagraphCountHighWord()
    {
        const int paragraphCount = ushort.MaxValue + 1;
        var payloadLength = paragraphCount * AtrLayout.ParagraphSize;
        var data = new byte[AtrLayout.HeaderSize + payloadLength];
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(AtrLayout.SignatureOffset), AtrFormat.Signature);
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(AtrLayout.ParagraphCountLowOffset), unchecked((ushort)paragraphCount));
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(AtrLayout.ParagraphCountHighOffset), checked((ushort)(paragraphCount >> AtrLayout.ParagraphCountHighWordShift)));
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(AtrLayout.SectorSizeOffset), AtrLayout.SingleDensitySectorSize);
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-atr-high-paragraph-{Guid.NewGuid():N}.atr");
        try
        {
            await File.WriteAllBytesAsync(path, data);
            var image = await new AtrReader().ReadAsync(path);
            Assert.Equal(payloadLength / AtrLayout.SingleDensitySectorSize, image.BlockCount);
            Assert.Equal(payloadLength, image.Capacity);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task RejectsInvalidSignatureSectorSizeDeclaredLengthAndTruncatedPayload()
    {
        var valid = await File.ReadAllBytesAsync(ImagePath("Atari 8-bit/os xl-xe.atr"));

        var invalidSignature = valid.ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(invalidSignature, 0xffff);
        await AssertRejectedAsync(invalidSignature);

        var invalidSectorSize = valid.ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(invalidSectorSize.AsSpan(4), 192);
        await AssertRejectedAsync(invalidSectorSize);

        var invalidDeclaredLength = valid.ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(
            invalidDeclaredLength.AsSpan(2),
            checked((ushort)(BinaryPrimitives.ReadUInt16LittleEndian(invalidDeclaredLength.AsSpan(2)) - 1)));
        await AssertRejectedAsync(invalidDeclaredLength);

        var truncatedPayload = valid[..^16];
        var paragraphCount = (truncatedPayload.Length - 16) / 16;
        BinaryPrimitives.WriteUInt16LittleEndian(truncatedPayload.AsSpan(2), checked((ushort)paragraphCount));
        BinaryPrimitives.WriteUInt16LittleEndian(truncatedPayload.AsSpan(6), checked((ushort)(paragraphCount >> 16)));
        await AssertRejectedAsync(truncatedPayload);
    }

    private static async Task AssertRejectedAsync(byte[] bytes)
    {
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-atr-invalid-{Guid.NewGuid():N}.atr");
        try
        {
            await File.WriteAllBytesAsync(path, bytes);
            await Assert.ThrowsAsync<InvalidDataException>(() => new AtrReader().ReadAsync(path));
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    private static string ImagePath(string relativePath) =>
        Path.Combine(FindImageTestRoot(), relativePath.Replace('/', Path.DirectorySeparatorChar));

    private static string FindImageTestRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, "image_test");
            if (Directory.Exists(candidate)) return candidate;
        }
        throw new DirectoryNotFoundException("Le dossier local image_test est introuvable.");
    }
}
