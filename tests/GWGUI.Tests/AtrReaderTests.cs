using System.Buffers.Binary;
using System.IO;
using GWGUI.MediaEngine.Containers.Atari.Atr;
using GWGUI.MediaEngine.Conversion.Atari;

namespace GWGUI.Tests;

/// <summary>Vérifie la lecture publique et l'extraction des conteneurs ATR locaux.</summary>
public sealed class AtrReaderTests
{
    [Theory]
    [InlineData("validated_images/Atari/Atari 130XE/5.25 pouces - Chargeur propriétaire - 90 Kio/seeds-of-evil-atari-130xe.atr", 128, 720)]
    [InlineData("Atari 8-bit/os xl-xe.atr", 256, 720)]
    public async Task ReadsHeaderGeometryAddressesCapacityAndSectorContents(
        string relativePath,
        int expectedSectorSize,
        int expectedSectorCount)
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
