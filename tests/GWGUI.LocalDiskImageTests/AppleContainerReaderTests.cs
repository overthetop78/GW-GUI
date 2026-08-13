using System.Buffers.Binary;
using System.IO;
using System.Text;
using GWGUI.MediaEngine.Containers.Apple.DiskCopy;
using GWGUI.MediaEngine.Containers.Apple;
using GWGUI.MediaEngine.Containers.Apple.TwoImg;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Geometries.Apple;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.Tests;

public sealed class AppleContainerReaderTests
{
    private static readonly Lazy<TestImages> Images = new(CreateTestImages);

    /// <summary>VÃ©rifie les valeurs binaires exposÃ©es par les dÃ©finitions DiskCopy.</summary>
    [Fact]
    public void ExposesExactDiskCopyFormatDefinitions()
    {
        Assert.Equal(0x0100, DiskCopyFormat.PrivateWord);
        Assert.Equal(0u, DiskCopyFormat.MissingChecksum);
        Assert.Equal(2, DiskCopyFormat.ChecksumWordSize);
        Assert.Equal(1, DiskCopyFormat.ChecksumRotation);
        Assert.Equal(32, DiskCopyFormat.ChecksumBitCount);
        Assert.Equal("PREBOOT", Encoding.ASCII.GetString(DiskCopyFormat.PrebootMarker));
    }

    [Fact]
    public async Task ReadsSectorTwoImgHeaderAndExtractsItsPayload()
    {
        var bytes = await File.ReadAllBytesAsync(Images.Value.SectorTwoImg);
        var image = await new AppleDiskImageReader().ReadAsync(Images.Value.SectorTwoImg);

        Assert.True(bytes.AsSpan(TwoImgLayout.SignatureOffset, TwoImgLayout.SignatureLength)
            .SequenceEqual(TwoImgFormat.SignatureBytes));
        Assert.Equal(TwoImgFormat.SupportedVersion,
            BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(TwoImgLayout.VersionOffset)));
        Assert.Equal(TwoImgImageFormat.ProDos, (TwoImgImageFormat)BinaryPrimitives.ReadUInt32LittleEndian(
            bytes.AsSpan(TwoImgLayout.ImageFormatOffset)));
        var dataOffset = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(
            bytes.AsSpan(TwoImgLayout.DataOffsetOffset)));
        var dataLength = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(
            bytes.AsSpan(TwoImgLayout.DataLengthOffset)));

        Assert.Equal(bytes.AsSpan(dataOffset, dataLength).ToArray(), FlattenData(image));
    }

    [Fact]
    public async Task RoutesDosAndProDosTwoImgPayloadsAccordingToTheirImageFormat()
    {
        var payload = await File.ReadAllBytesAsync(Images.Value.RawDos);
        var dos = await new AppleDiskImageReader().ReadAsync(Images.Value.DosTwoImg);
        var proDos = await new AppleDiskImageReader().ReadAsync(Images.Value.ProDosTwoImg);

        Assert.Equal(256, dos.BlockSize);
        Assert.Equal(512, proDos.BlockSize);
        Assert.Equal(payload, FlattenData(dos));
        Assert.Equal(payload, FlattenData(proDos));
    }

    [Fact]
    public async Task RoutesNibTwoImgAndExtractsTheSameSectorsAsItsNibPayload()
    {
        var wrapped = await new AppleDiskImageReader().ReadAsync(Images.Value.NibTwoImg);
        var nib = await new AppleDiskImageReader().ReadAsync(Images.Value.RawNib);

        Assert.Equal(nib.FormatId, wrapped.FormatId);
        Assert.Equal(nib.BlockSize, wrapped.BlockSize);
        Assert.Equal(nib.AvailableBlocks.Count, wrapped.AvailableBlocks.Count);
        Assert.Equal(FlattenData(nib), FlattenData(wrapped));
    }

    [Fact]
    public async Task ReadsTaggedDiskCopyHeaderPayloadTagsAndChecksums()
    {
        var bytes = await File.ReadAllBytesAsync(Images.Value.TaggedDiskCopy);
        var dataLength = checked((int)BinaryPrimitives.ReadUInt32BigEndian(
            bytes.AsSpan(DiskCopyLayout.DataLengthOffset)));
        var tagLength = checked((int)BinaryPrimitives.ReadUInt32BigEndian(
            bytes.AsSpan(DiskCopyLayout.TagLengthOffset)));
        var data = bytes.AsSpan(DiskCopyLayout.HeaderSize, dataLength).ToArray();
        var tags = bytes.AsSpan(DiskCopyLayout.HeaderSize + dataLength, tagLength).ToArray();
        var storedDataChecksum = BinaryPrimitives.ReadUInt32BigEndian(
            bytes.AsSpan(DiskCopyLayout.DataChecksumOffset));
        var storedTagChecksum = BinaryPrimitives.ReadUInt32BigEndian(
            bytes.AsSpan(DiskCopyLayout.TagChecksumOffset));

        Assert.Equal(DiskCopyFormat.PrivateWord,
            BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(DiskCopyLayout.PrivateWordOffset)));

        var image = await new AppleDiskImageReader().ReadAsync(Images.Value.TaggedDiskCopy);
        var orderedBlocks = image.AvailableBlocks.OrderBy(block => block.LogicalBlock).ToArray();

        Assert.Equal(dataLength, image.Capacity);
        Assert.Equal(dataLength / DiskCopyLayout.DataBlockSize, orderedBlocks.Length);
        Assert.Equal(data, orderedBlocks.SelectMany(block => block.Data).ToArray());
        Assert.Equal(tags, orderedBlocks.SelectMany(block => block.Tag ?? []).ToArray());
        Assert.Equal(storedDataChecksum, CalculateDiskCopyChecksum(data));
        Assert.Equal(storedTagChecksum,
            CalculateDiskCopyChecksum(tags.AsSpan(DiskCopyLayout.TagChecksumExcludedPrefixSize)));
        Assert.Equal(DiskImageFormatIds.AppleLisaOffice, image.FormatId);
    }

    [Fact]
    public async Task DiskCopyWriterPreservesSourceNameFormatTagsAndChecksums()
    {
        var sourceBytes = await File.ReadAllBytesAsync(Images.Value.TaggedDiskCopy);
        var source = DiskCopyReader.ReadDetailed(sourceBytes);
        var outputPath = Path.Combine(Path.GetTempPath(), $"gwgui-diskcopy-{Guid.NewGuid():N}.dc42");
        try
        {
            await new DiskCopyWriter().WriteAsync(source.Image, outputPath, source);
            var outputBytes = await File.ReadAllBytesAsync(outputPath);
            var reopened = DiskCopyReader.ReadDetailed(outputBytes);
            Assert.Equal(source.NameBytes, reopened.NameBytes);
            Assert.Equal(source.DiskFormat, reopened.DiskFormat);
            Assert.Equal(source.FormatByte, reopened.FormatByte);
            Assert.Equal(source.Image.AvailableBlocks.SelectMany(block => block.Data), reopened.Image.AvailableBlocks.SelectMany(block => block.Data));
            Assert.Equal(source.Image.AvailableBlocks.SelectMany(block => block.Tag ?? []), reopened.Image.AvailableBlocks.SelectMany(block => block.Tag ?? []));
            Assert.Equal(BinaryPrimitives.ReadUInt32BigEndian(sourceBytes.AsSpan(DiskCopyLayout.DataChecksumOffset)), BinaryPrimitives.ReadUInt32BigEndian(outputBytes.AsSpan(DiskCopyLayout.DataChecksumOffset)));
            Assert.Equal(BinaryPrimitives.ReadUInt32BigEndian(sourceBytes.AsSpan(DiskCopyLayout.TagChecksumOffset)), BinaryPrimitives.ReadUInt32BigEndian(outputBytes.AsSpan(DiskCopyLayout.TagChecksumOffset)));
        }
        finally
        {
            if (File.Exists(outputPath)) File.Delete(outputPath);
        }
    }

    /// <summary>VÃ©rifie qu'un conteneur DiskCopy sans tags dÃ©lÃ¨gue sa charge utile Macintosh au lecteur brut.</summary>
    [Fact]
    public async Task ReadsUntaggedMacintoshDiskCopyPayload()
    {
        var source = Path.Combine(FindImageTestRoot(), "validated_images", "Apple", "Macintosh", "3.5 pouces - HFS - 1.44 Mio", "System_6.0.6_System_Startup.dsk");
        var output = Path.Combine(FindImageTestRoot(), "_generated", "apple-containers");
        Directory.CreateDirectory(output);
        var path = Write(output, "untagged-macintosh.image", BuildUntaggedDiskCopy(File.ReadAllBytes(source)));

        var image = await new AppleDiskImageReader().ReadAsync(path);

        Assert.Equal(DiskImageFormatIds.Mac1440, image.FormatId);
        Assert.All(image.AvailableBlocks, block => Assert.Null(block.Tag));
    }

    /// <summary>VÃ©rifie que le marqueur PREBOOT sÃ©lectionne l'interprÃ©tation Lisa MacWorks.</summary>
    [Fact]
    public async Task RecognizesPrebootAsLisaMacWorks()
    {
        var bytes = BuildTaggedDiskCopy(LisaFileWareGeometry.BlockCount);
        var markerOffset = DiskCopyLayout.HeaderSize + DiskCopyLayout.PrebootSearchBlockIndex * DiskCopyLayout.DataBlockSize;
        DiskCopyFormat.PrebootMarker.CopyTo(bytes.AsSpan(markerOffset));
        var output = Path.Combine(FindImageTestRoot(), "_generated", "apple-containers");
        Directory.CreateDirectory(output);
        var path = Write(output, "lisa-macworks.image", bytes);

        var image = await new AppleDiskImageReader().ReadAsync(path);

        Assert.Equal(DiskImageFormatIds.AppleLisaMacWorks, image.FormatId);
    }

    [Theory]
    [InlineData(1702, 46, 2, 22)]
    [InlineData(800, 80, 1, 12)]
    [InlineData(20, 2, 1, 10)]
    public async Task PreservesMacintoshAndGenericTaggedDiskCopyGeometries(int blockCount, int cylinders, int heads, int sectorsPerTrack)
    {
        var output = Path.Combine(FindImageTestRoot(), "_generated", "apple-containers");
        Directory.CreateDirectory(output);
        var path = Write(output, $"tagged-{blockCount}.image", BuildTaggedDiskCopy(blockCount));

        var image = await new AppleDiskImageReader().ReadAsync(path);

        Assert.Equal(blockCount, image.BlockCount);
        Assert.Equal((long)blockCount * DiskCopyLayout.DataBlockSize, image.Capacity);
        Assert.Equal(cylinders, image.Cylinders);
        Assert.Equal(heads, image.Heads);
        Assert.Equal(sectorsPerTrack, image.SectorsPerTrack);
        Assert.Equal(DiskImageFormatIds.AppleLisaOffice, image.FormatId);
        Assert.All(image.AvailableBlocks, block => Assert.Equal(DiskCopyLayout.TagSizePerBlock, block.Tag?.Count));
    }

    [Theory]
    [InlineData("invalid-signature")]
    [InlineData("invalid-offset")]
    [InlineData("invalid-length")]
    [InlineData("invalid-header-size")]
    [InlineData("truncated-header")]
    public async Task RejectsInvalidTwoImgStructures(string variant)
    {
        await Assert.ThrowsAsync<InvalidDataException>(() =>
            new AppleDiskImageReader().ReadAsync(Images.Value.InvalidTwoImg[variant]));
    }

    [Theory]
    [InlineData("unsupported-version")]
    [InlineData("unsupported-image-format")]
    public async Task RejectsUnsupportedTwoImgFields(string variant)
    {
        await Assert.ThrowsAsync<NotSupportedException>(() =>
            new AppleDiskImageReader().ReadAsync(Images.Value.InvalidTwoImg[variant]));
    }

    [Theory]
    [InlineData("invalid-data-length")]
    [InlineData("invalid-data-checksum")]
    [InlineData("invalid-tag-checksum")]
    [InlineData("invalid-private-word")]
    [InlineData("unrecognized-tags")]
    [InlineData("truncated-header")]
    public async Task RejectsInvalidDiskCopyStructures(string variant)
    {
        await Assert.ThrowsAsync<InvalidDataException>(() =>
            new AppleDiskImageReader().ReadAsync(Images.Value.InvalidDiskCopy[variant]));
    }

    private static TestImages CreateTestImages()
    {
        var root = FindImageTestRoot();
        var appleTwoRoot = Path.Combine(root, "Apple II");
        var sectorTwoImg = Path.Combine(appleTwoRoot, "AMR Hard Drive Utility Disk 3.5.2mg");
        var rawDos = Path.Combine(appleTwoRoot, "DOS 3.3 System Master - 680-0051-00.dsk");
        var rawNib = Path.Combine(appleTwoRoot, "Merlin (1983)(Southwestern Data Systems)(US)(Side A).nib");
        var output = Path.Combine(root, "_generated", "apple", "containers");
        var nibTwoImg = Path.Combine(output, "nibble-wrapped.2mg");
        var taggedDiskCopy = Directory.EnumerateFiles(root, "*LisaGuide.image", SearchOption.AllDirectories)
            .Single();
        RequireFile(sectorTwoImg);
        RequireFile(rawDos);
        RequireFile(rawNib);
        RequireFile(nibTwoImg);
        RequireFile(taggedDiskCopy);
        Directory.CreateDirectory(output);

        var rawDosBytes = File.ReadAllBytes(rawDos);
        var dosTwoImg = Write(output, "dos-order.2mg", BuildTwoImg(TwoImgImageFormat.Dos, rawDosBytes));
        var proDosTwoImg = Write(output, "prodos-order.2mg", BuildTwoImg(TwoImgImageFormat.ProDos, rawDosBytes));

        var validTwoImg = File.ReadAllBytes(dosTwoImg);
        var invalidTwoImg = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["invalid-signature"] = WriteVariant(output, "invalid-signature.2mg", validTwoImg,
                bytes => bytes[TwoImgLayout.SignatureOffset] ^= byte.MaxValue),
            ["unsupported-version"] = WriteVariant(output, "unsupported-version.2mg", validTwoImg,
                bytes => BinaryPrimitives.WriteUInt16LittleEndian(
                    bytes.AsSpan(TwoImgLayout.VersionOffset), TwoImgFormat.SupportedVersion + 1)),
            ["unsupported-image-format"] = WriteVariant(output, "unsupported-image-format.2mg", validTwoImg,
                bytes => BinaryPrimitives.WriteUInt32LittleEndian(
                    bytes.AsSpan(TwoImgLayout.ImageFormatOffset), uint.MaxValue)),
            ["invalid-offset"] = WriteVariant(output, "invalid-offset.2mg", validTwoImg,
                bytes => BinaryPrimitives.WriteUInt32LittleEndian(
                    bytes.AsSpan(TwoImgLayout.DataOffsetOffset), checked((uint)(bytes.Length + 1)))),
            ["invalid-length"] = WriteVariant(output, "invalid-length.2mg", validTwoImg,
                bytes => BinaryPrimitives.WriteUInt32LittleEndian(
                    bytes.AsSpan(TwoImgLayout.DataLengthOffset), checked((uint)rawDosBytes.Length + 1))),
            ["invalid-header-size"] = WriteVariant(output, "invalid-header-size.2mg", validTwoImg,
                bytes => BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(TwoImgLayout.HeaderSizeOffset), TwoImgLayout.MinimumHeaderSize - 1)),
            ["truncated-header"] = Write(output, "truncated-header.2mg",
                validTwoImg[..(TwoImgLayout.MinimumHeaderSize - 1)])
        };

        var validDiskCopy = File.ReadAllBytes(taggedDiskCopy);
        var diskCopyDataLength = checked((int)BinaryPrimitives.ReadUInt32BigEndian(
            validDiskCopy.AsSpan(DiskCopyLayout.DataLengthOffset)));
        var invalidDiskCopy = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["invalid-data-length"] = WriteVariant(output, "invalid-data-length.image", validDiskCopy,
                bytes => BinaryPrimitives.WriteUInt32BigEndian(
                    bytes.AsSpan(DiskCopyLayout.DataLengthOffset), checked((uint)diskCopyDataLength + 1))),
            ["invalid-data-checksum"] = WriteVariant(output, "invalid-data-checksum.image", validDiskCopy,
                bytes => bytes[DiskCopyLayout.HeaderSize] ^= byte.MaxValue),
            ["invalid-tag-checksum"] = WriteVariant(output, "invalid-tag-checksum.image", validDiskCopy,
                bytes => bytes[DiskCopyLayout.HeaderSize + diskCopyDataLength +
                               DiskCopyLayout.TagChecksumExcludedPrefixSize] ^= byte.MaxValue),
            ["invalid-private-word"] = WriteVariant(output, "invalid-private-word.image", validDiskCopy,
                bytes => BinaryPrimitives.WriteUInt16BigEndian(
                    bytes.AsSpan(DiskCopyLayout.PrivateWordOffset), ushort.MaxValue)),
            ["unrecognized-tags"] = WriteVariant(output, "unrecognized-tags.image", validDiskCopy, bytes =>
            {
                BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(DiskCopyLayout.TagLengthOffset), checked((uint)(diskCopyDataLength / DiskCopyLayout.DataBlockSize * DiskCopyLayout.TagSizePerBlock - 1)));
                BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(DiskCopyLayout.TagChecksumOffset), DiskCopyFormat.MissingChecksum);
            }),
            ["truncated-header"] = Write(output, "truncated-header.image",
                validDiskCopy[..(DiskCopyLayout.HeaderSize - 1)])
        };

        return new(sectorTwoImg, rawDos, rawNib, nibTwoImg, dosTwoImg, proDosTwoImg, taggedDiskCopy,
            invalidTwoImg, invalidDiskCopy);
    }

    private static byte[] BuildTwoImg(TwoImgImageFormat imageFormat, byte[] payload)
    {
        var container = new byte[TwoImgLayout.MinimumHeaderSize + payload.Length];
        TwoImgFormat.SignatureBytes.CopyTo(container.AsSpan(TwoImgLayout.SignatureOffset));
        BinaryPrimitives.WriteUInt16LittleEndian(container.AsSpan(TwoImgLayout.HeaderSizeOffset),
            TwoImgLayout.MinimumHeaderSize);
        BinaryPrimitives.WriteUInt16LittleEndian(container.AsSpan(TwoImgLayout.VersionOffset),
            TwoImgFormat.SupportedVersion);
        BinaryPrimitives.WriteUInt32LittleEndian(container.AsSpan(TwoImgLayout.ImageFormatOffset),
            (uint)imageFormat);
        BinaryPrimitives.WriteUInt32LittleEndian(container.AsSpan(TwoImgLayout.DataOffsetOffset),
            TwoImgLayout.MinimumHeaderSize);
        BinaryPrimitives.WriteUInt32LittleEndian(container.AsSpan(TwoImgLayout.DataLengthOffset),
            checked((uint)payload.Length));
        payload.CopyTo(container, TwoImgLayout.MinimumHeaderSize);
        return container;
    }

    private static byte[] BuildTaggedDiskCopy(int blockCount)
    {
        var dataLength = checked(blockCount * DiskCopyLayout.DataBlockSize);
        var tagLength = checked(blockCount * DiskCopyLayout.TagSizePerBlock);
        var container = new byte[DiskCopyLayout.HeaderSize + dataLength + tagLength];
        BinaryPrimitives.WriteUInt32BigEndian(container.AsSpan(DiskCopyLayout.DataLengthOffset), checked((uint)dataLength));
        BinaryPrimitives.WriteUInt32BigEndian(container.AsSpan(DiskCopyLayout.TagLengthOffset), checked((uint)tagLength));
        BinaryPrimitives.WriteUInt16BigEndian(container.AsSpan(DiskCopyLayout.PrivateWordOffset), DiskCopyFormat.PrivateWord);
        return container;
    }

    private static byte[] BuildUntaggedDiskCopy(byte[] payload)
    {
        var container = new byte[DiskCopyLayout.HeaderSize + payload.Length];
        BinaryPrimitives.WriteUInt32BigEndian(container.AsSpan(DiskCopyLayout.DataLengthOffset), checked((uint)payload.Length));
        BinaryPrimitives.WriteUInt16BigEndian(container.AsSpan(DiskCopyLayout.PrivateWordOffset), DiskCopyFormat.PrivateWord);
        payload.CopyTo(container, DiskCopyLayout.HeaderSize);
        return container;
    }

    private static uint CalculateDiskCopyChecksum(ReadOnlySpan<byte> data)
    {
        const int wordSize = sizeof(ushort);
        const int rotation = 1;
        Assert.Equal(0, data.Length % wordSize);
        uint checksum = 0;
        for (var offset = 0; offset < data.Length; offset += wordSize)
        {
            checksum = unchecked(checksum + BinaryPrimitives.ReadUInt16BigEndian(data[offset..]));
            checksum = checksum >> rotation | checksum << (sizeof(uint) * 8 - rotation);
        }
        return checksum;
    }

    private static byte[] FlattenData(SectorImage image) => image.AvailableBlocks
        .OrderBy(block => block.LogicalBlock)
        .SelectMany(block => block.Data)
        .ToArray();

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

    private static void RequireFile(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("Lâ€™image de test Apple locale est absente.", path);
    }

    private static string FindImageTestRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null;
             directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, "image_test");
            if (Directory.Exists(candidate)) return candidate;
        }
        throw new DirectoryNotFoundException("Le dossier local image_test est introuvable.");
    }

    private sealed record TestImages(
        string SectorTwoImg,
        string RawDos,
        string RawNib,
        string NibTwoImg,
        string DosTwoImg,
        string ProDosTwoImg,
        string TaggedDiskCopy,
        IReadOnlyDictionary<string, string> InvalidTwoImg,
        IReadOnlyDictionary<string, string> InvalidDiskCopy);
}
