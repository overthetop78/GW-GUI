using System.Buffers.Binary;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using GWGUI.MediaEngine.Containers.Amstrad.CpcDsk;

namespace GWGUI.Tests;

public sealed class CpcDskReaderTests
{
    private static readonly Lazy<TestImages> Images = new(CreateTestImages);

    [Theory]
    [InlineData("standard", "158738173A8F0AE7ABBBA4A05DD0575D194EFD4B6DA9DB510A134F8743753122")]
    [InlineData("extended", "E289A13850E2BC62672CDBA4916D3EFD36B75FB77D238688B25A1E40DABE4852")]
    public async Task ReadsRealStandardAndExtendedImages(string kind, string firstSectorSha256)
    {
        var path = kind == "standard" ? Images.Value.Standard : Images.Value.Extended;

        var image = await new CpcDskReader().ReadAsync(path);

        Assert.Equal(CpcDskFormat.FormatId, image.FormatId);
        Assert.Equal(512, image.BlockSize);
        Assert.Equal(40, image.Cylinders);
        Assert.Equal(1, image.Heads);
        Assert.Equal(9, image.SectorsPerTrack);
        Assert.Equal(360, image.AvailableBlocks.Count);
        Assert.Equal(184_320, image.Capacity);
        var first = image.AvailableBlocks.OrderBy(block => block.LogicalBlock).First();
        Assert.Equal(0, first.Address.Cylinder);
        Assert.Equal(0, first.Address.Head);
        Assert.Equal(0xc1, first.Address.Number);
        Assert.True(first.IntegrityValid);
        Assert.Equal(firstSectorSha256, Convert.ToHexString(SHA256.HashData(first.Data.ToArray())));
    }

    [Fact]
    public async Task ReadsVariableSectorSizesAndIntegrityFromGeneratedEdsk()
    {
        var image = await new CpcDskReader().ReadAsync(Images.Value.VariableExtended);

        Assert.Equal(CpcDskFormat.FormatId, image.FormatId);
        Assert.Equal(256, image.BlockSize);
        Assert.Equal(1, image.Cylinders);
        Assert.Equal(1, image.Heads);
        Assert.Equal(2, image.SectorsPerTrack);
        Assert.Equal(768, image.Capacity);
        var sectors = image.AvailableBlocks.OrderBy(block => block.LogicalBlock).ToArray();
        Assert.Equal(new byte[] { 1, 2 }, sectors.Select(block => checked((byte)block.Address.Number)));
        Assert.Equal(256, sectors[0].Data.Count);
        Assert.Equal(512, sectors[1].Data.Count);
        Assert.True(sectors[0].IntegrityValid);
        Assert.False(sectors[1].IntegrityValid);
        Assert.Equal(Enumerable.Range(0, 256).Select(value => (byte)value), sectors[0].Data);
        Assert.All(sectors[1].Data, value => Assert.Equal(0xa5, value));
        Assert.Equal(256, image.GetBlock(0).Length);
        Assert.Equal(512, image.GetBlock(1).Length);
    }

    [Theory]
    [InlineData("invalid-signature")]
    [InlineData("truncated-header")]
    [InlineData("invalid-geometry")]
    [InlineData("invalid-track-table")]
    [InlineData("invalid-track-header")]
    [InlineData("invalid-sector-table")]
    [InlineData("truncated-sector-data")]
    public async Task RejectsInvalidCpcDskStructures(string variant)
    {
        await Assert.ThrowsAsync<InvalidDataException>(() =>
            new CpcDskReader().ReadAsync(Images.Value.Invalid[variant]));
    }

    [Fact]
    public async Task TrackErrorContainsRejectedTrackIndex()
    {
        var exception = await Assert.ThrowsAsync<InvalidDataException>(() =>
            new CpcDskReader().ReadAsync(Images.Value.Invalid["invalid-track-header"]));

        Assert.Contains("track 0", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SectorErrorContainsRejectedPhysicalAddress()
    {
        var exception = await Assert.ThrowsAsync<InvalidDataException>(() =>
            new CpcDskReader().ReadAsync(Images.Value.Invalid["truncated-sector-data"]));

        Assert.Contains("0:0:2", exception.Message, StringComparison.Ordinal);
    }

    private static TestImages CreateTestImages()
    {
        var imageTestRoot = FindImageTestRoot();
        var cpcDirectory = Path.Combine(imageTestRoot, "validated_images", "Amstrad", "CPC", "3 pouces simple face - 180 Kio");
        var standard = Path.Combine(cpcDirectory, "007 - A View to a Kill (1985)(Domark).dsk");
        var extended = Path.Combine(cpcDirectory, "sean_2024.dsk");
        if (!File.Exists(standard)) throw new FileNotFoundException("L'image CPCEMU Standard locale est absente.", standard);
        if (!File.Exists(extended)) throw new FileNotFoundException("L'image CPCEMU Extended locale est absente.", extended);

        var outputDirectory = Path.Combine(imageTestRoot, "_generated", "cpcdsk");
        Directory.CreateDirectory(outputDirectory);
        var validBytes = BuildVariableExtendedImage();
        var variableExtended = Write(outputDirectory, "variable-sectors-and-integrity.edsk", validBytes);
        var invalid = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["invalid-signature"] = WriteVariant(outputDirectory, "invalid-signature.edsk", validBytes, bytes => bytes[0] = (byte)'X'),
            ["truncated-header"] = Write(outputDirectory, "truncated-header.edsk", validBytes[..100]),
            ["invalid-geometry"] = WriteVariant(outputDirectory, "invalid-geometry.edsk", validBytes,
                bytes => bytes[CpcDskLayout.HeadCountOffset] = CpcDskLayout.MaximumHeadCount + 1),
            ["invalid-track-table"] = WriteVariant(outputDirectory, "invalid-track-table.edsk", validBytes, bytes =>
            {
                bytes[CpcDskLayout.CylinderCountOffset] = CpcDskLayout.MaximumCylinderCount;
                bytes[CpcDskLayout.HeadCountOffset] = CpcDskLayout.MaximumHeadCount;
            }),
            ["invalid-track-header"] = WriteVariant(outputDirectory, "invalid-track-header.edsk", validBytes,
                bytes => bytes[CpcDskLayout.DiskInformationBlockSize] = (byte)'X'),
            ["invalid-sector-table"] = WriteVariant(outputDirectory, "invalid-sector-table.edsk", validBytes,
                bytes => bytes[CpcDskLayout.DiskInformationBlockSize + CpcDskLayout.TrackSectorCountOffset] = 30),
            ["truncated-sector-data"] = WriteVariant(outputDirectory, "truncated-sector-data.edsk", validBytes, bytes =>
            {
                var secondDescriptor = CpcDskLayout.DiskInformationBlockSize +
                                       CpcDskLayout.SectorDescriptorTableOffset +
                                       CpcDskLayout.SectorDescriptorSize;
                var storedSize = bytes.AsSpan(
                    secondDescriptor + CpcDskLayout.SectorStoredSizeOffset,
                    CpcDskLayout.StoredSizeFieldLength);
                var oversized = checked((ushort)(BinaryPrimitives.ReadUInt16LittleEndian(storedSize) + 1));
                BinaryPrimitives.WriteUInt16LittleEndian(storedSize, oversized);
            })
        };

        return new(standard, extended, variableExtended, invalid);
    }

    private static byte[] BuildVariableExtendedImage()
    {
        const int firstSectorSize = 256;
        const int secondSectorSize = 512;
        const byte sectorCount = 2;
        const byte defaultSizeCode = 2;
        const byte gap3Length = 0x4e;
        const byte fillerByte = 0xe5;
        const byte secondSectorSizeCode = 2;
        const byte secondSectorStatus1 = CpcDskLayout.DataErrorMask;
        const byte secondSectorFill = 0xa5;
        var trackSize = CpcDskLayout.TrackInformationBlockSize + firstSectorSize + secondSectorSize;
        var data = new byte[CpcDskLayout.DiskInformationBlockSize + trackSize];
        Encoding.ASCII.GetBytes(CpcDskFormat.ExtendedSignature).CopyTo(data, 0);
        Encoding.ASCII.GetBytes("GWGUI test".PadRight(CpcDskLayout.CreatorLength))
            .CopyTo(data, CpcDskLayout.CreatorOffset);
        data[CpcDskLayout.CylinderCountOffset] = 1;
        data[CpcDskLayout.HeadCountOffset] = 1;
        data[CpcDskLayout.ExtendedTrackSizeTableOffset] =
            checked((byte)(trackSize / CpcDskLayout.ExtendedTrackSizeUnit));

        var trackOffset = CpcDskLayout.DiskInformationBlockSize;
        Encoding.ASCII.GetBytes(CpcDskFormat.TrackSignature).CopyTo(data, trackOffset);
        data[trackOffset + CpcDskLayout.TrackCylinderOffset] = 0;
        data[trackOffset + CpcDskLayout.TrackHeadOffset] = 0;
        data[trackOffset + CpcDskLayout.TrackSectorSizeCodeOffset] = defaultSizeCode;
        data[trackOffset + CpcDskLayout.TrackSectorCountOffset] = sectorCount;
        data[trackOffset + CpcDskLayout.TrackGap3LengthOffset] = gap3Length;
        data[trackOffset + CpcDskLayout.TrackFillerByteOffset] = fillerByte;

        var descriptorsOffset = trackOffset + CpcDskLayout.SectorDescriptorTableOffset;
        WriteSectorDescriptor(data, descriptorsOffset, 0, 0, 1, 1, 0, firstSectorSize);
        WriteSectorDescriptor(data, descriptorsOffset + CpcDskLayout.SectorDescriptorSize,
            0, 0, 2, secondSectorSizeCode, secondSectorStatus1, secondSectorSize);
        var dataOffset = trackOffset + CpcDskLayout.TrackInformationBlockSize;
        for (var index = 0; index < firstSectorSize; index++) data[dataOffset + index] = checked((byte)index);
        data.AsSpan(dataOffset + firstSectorSize, secondSectorSize).Fill(secondSectorFill);
        return data;
    }

    private static void WriteSectorDescriptor(byte[] data, int offset, byte cylinder, byte head, byte number,
        byte sizeCode, byte status1, ushort storedSize)
    {
        data[offset + CpcDskLayout.SectorCylinderOffset] = cylinder;
        data[offset + CpcDskLayout.SectorHeadOffset] = head;
        data[offset + CpcDskLayout.SectorIdOffset] = number;
        data[offset + CpcDskLayout.SectorSizeCodeOffset] = sizeCode;
        data[offset + CpcDskLayout.SectorStatus1Offset] = status1;
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(
            offset + CpcDskLayout.SectorStoredSizeOffset, CpcDskLayout.StoredSizeFieldLength), storedSize);
    }

    private static string WriteVariant(string directory, string fileName, byte[] source, Action<byte[]> change)
    {
        var bytes = (byte[])source.Clone();
        change(bytes);
        return Write(directory, fileName, bytes);
    }

    private static string Write(string directory, string fileName, byte[] bytes)
    {
        var path = Path.Combine(directory, fileName);
        File.WriteAllBytes(path, bytes);
        return path;
    }

    private static string FindImageTestRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, "image_test");
            if (Directory.Exists(candidate)) return candidate;
        }
        throw new DirectoryNotFoundException("Le dossier local image_test est introuvable.");
    }

    private sealed record TestImages(
        string Standard,
        string Extended,
        string VariableExtended,
        IReadOnlyDictionary<string, string> Invalid);
}
