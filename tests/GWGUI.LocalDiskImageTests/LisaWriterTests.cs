using System.Buffers.Binary;
using System.IO;
using GWGUI.MediaEngine.Composition;
using GWGUI.MediaEngine.Containers.Apple.DiskCopy;
using GWGUI.MediaEngine.Conversion.Apple;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.FileSystems.Apple.Lisa;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.Tests;

/// <summary>Vérifie la conversion DiskCopy des images Lisa Office et MacWorks.</summary>
public sealed class LisaWriterTests
{
    [Theory]
    [InlineData("3.5 pouces - Lisa Office - 400 Kio", "Apple Lisa Software - Disk Images_LisaGuide [test] [données seules].scp", DiskImageFormatIds.AppleLisaOffice)]
    [InlineData("3.5 pouces - MacWorks - 400 Kio", "Apple Lisa Software - Disk Images_Lisa MacWorks 3.0 [test] [données seules].scp", DiskImageFormatIds.AppleLisaMacWorks)]
    public async Task ScpConversionCreatesAReadableTaggedDiskCopyImage(string directory, string fileName, string formatId)
    {
        var sourcePath = ImagePath(directory, fileName);
        var outputPath = Path.Combine(Path.GetTempPath(), $"gwgui-lisa-scp-{Guid.NewGuid():N}.dc42");
        try
        {
            await MediaEngineFactory.CreateLisaConversionService().ConvertAsync(sourcePath, outputPath, formatId);
            var outputBytes = await File.ReadAllBytesAsync(outputPath);
            var output = DiskCopyReader.ReadDetailed(outputBytes);
            Assert.Equal(formatId, output.Image.FormatId);
            Assert.All(output.Image.AvailableBlocks, block => Assert.Equal(DiskCopyLayout.TagSizePerBlock, block.Tag?.Count));
            AssertChecksums(outputBytes);
            Assert.All(output.Image.AvailableBlocks, block => Assert.All(block.Tag!, value => Assert.Equal(0, value)));
        }
        finally
        {
            if (File.Exists(outputPath)) File.Delete(outputPath);
        }
    }

    [Theory]
    [InlineData("3.5 pouces - Lisa Office - 400 Kio", "Apple Lisa Software - Disk Images_LisaGuide.image", DiskImageFormatIds.AppleLisaOffice, ".dc42")]
    [InlineData("3.5 pouces - MacWorks - 400 Kio", "Apple Lisa Software - Disk Images_Lisa MacWorks 3.0.image", DiskImageFormatIds.AppleLisaMacWorks, ".image")]
    public async Task DiskCopyConversionPreservesHeaderPagesTagsAndChecksums(string directory, string fileName, string formatId, string extension)
    {
        var sourcePath = ImagePath(directory, fileName);
        var outputPath = Path.Combine(Path.GetTempPath(), $"gwgui-lisa-{Guid.NewGuid():N}{extension}");
        try
        {
            Assert.True(LisaConversionService.CanCreate(formatId, extension));
            await MediaEngineFactory.CreateLisaConversionService().ConvertAsync(sourcePath, outputPath, formatId);
            var sourceBytes = await File.ReadAllBytesAsync(sourcePath);
            var outputBytes = await File.ReadAllBytesAsync(outputPath);
            var source = DiskCopyReader.ReadDetailed(sourceBytes);
            var output = DiskCopyReader.ReadDetailed(outputBytes);
            Assert.Equal(source.NameBytes, output.NameBytes);
            Assert.Equal(source.DiskFormat, output.DiskFormat);
            Assert.Equal(source.FormatByte, output.FormatByte);
            Assert.Equal(formatId, output.Image.FormatId);
            AssertImagesEqual(source.Image, output.Image);
            AssertChecksums(outputBytes);
            if (formatId == DiskImageFormatIds.AppleLisaOffice) AssertVolumesEqual(new LisaFileSystemReader().Read(source.Image), new LisaFileSystemReader().Read(output.Image));
        }
        finally
        {
            if (File.Exists(outputPath)) File.Delete(outputPath);
        }
    }

    private static void AssertChecksums(byte[] container)
    {
        var dataLength = checked((int)BinaryPrimitives.ReadUInt32BigEndian(container.AsSpan(DiskCopyLayout.DataLengthOffset)));
        var tagLength = checked((int)BinaryPrimitives.ReadUInt32BigEndian(container.AsSpan(DiskCopyLayout.TagLengthOffset)));
        Assert.Equal(DiskCopyReader.CalculateChecksum(container.AsSpan(DiskCopyLayout.HeaderSize, dataLength)), BinaryPrimitives.ReadUInt32BigEndian(container.AsSpan(DiskCopyLayout.DataChecksumOffset)));
        Assert.Equal(DiskCopyReader.CalculateChecksum(container.AsSpan(DiskCopyLayout.HeaderSize + dataLength + DiskCopyLayout.TagChecksumExcludedPrefixSize, tagLength - DiskCopyLayout.TagChecksumExcludedPrefixSize)), BinaryPrimitives.ReadUInt32BigEndian(container.AsSpan(DiskCopyLayout.TagChecksumOffset)));
    }

    private static void AssertImagesEqual(SectorImage expected, SectorImage actual)
    {
        Assert.Equal(expected.BlockCount, actual.BlockCount);
        for (var logical = 0; logical < expected.BlockCount; logical++)
        {
            var expectedBlock = expected.AvailableBlocks.Single(block => block.LogicalBlock == logical);
            var actualBlock = actual.AvailableBlocks.Single(block => block.LogicalBlock == logical);
            Assert.Equal(expectedBlock.Address, actualBlock.Address);
            Assert.Equal(expectedBlock.Data, actualBlock.Data);
            Assert.Equal(expectedBlock.Tag, actualBlock.Tag);
        }
    }

    private static void AssertVolumesEqual(FileSystemVolume expected, FileSystemVolume actual)
    {
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.Capacity, actual.Capacity);
        Assert.Equal(expected.FreeBytes, actual.FreeBytes);
        AssertEntriesEqual(expected.Entries, actual.Entries);
    }

    private static void AssertEntriesEqual(IReadOnlyList<FileSystemEntry> expected, IReadOnlyList<FileSystemEntry> actual)
    {
        Assert.Equal(expected.Count, actual.Count);
        for (var index = 0; index < expected.Count; index++)
        {
            Assert.Equal(expected[index].Name, actual[index].Name);
            Assert.Equal(expected[index].Kind, actual[index].Kind);
            Assert.Equal(expected[index].Size, actual[index].Size);
            Assert.Equal(expected[index].Content, actual[index].Content);
            AssertEntriesEqual(expected[index].Children, actual[index].Children);
        }
    }

    private static string ImagePath(string directory, string fileName) => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "image_test", "validated_images", "Apple", "Apple Lisa", directory, fileName));
}
