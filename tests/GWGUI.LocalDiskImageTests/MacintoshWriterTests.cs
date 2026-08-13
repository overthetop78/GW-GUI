using System.Buffers.Binary;
using System.IO;
using GWGUI.MediaEngine.Composition;
using GWGUI.MediaEngine.Containers.Apple;
using GWGUI.MediaEngine.Containers.Apple.DiskCopy;
using GWGUI.MediaEngine.Conversion.Apple;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Hfs;
using GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Mfs;

namespace GWGUI.Tests;

/// <summary>Vérifie les Writers Macintosh bruts et DiskCopy 4.2.</summary>
public sealed class MacintoshWriterTests
{
    [Theory]
    [InlineData("3.5 pouces - MFS - 400 Kio", "Macintosh System Disk 1.1g (3.5-400K).dsk", DiskImageFormatIds.AppleMacMfs)]
    [InlineData("3.5 pouces - HFS - 800 Kio", "System 3.2.dsk", DiskImageFormatIds.AppleMacHfs)]
    [InlineData("3.5 pouces - HFS - 1.44 Mio", "System_6.0.6_System_Startup.dsk", DiskImageFormatIds.Mac1440)]
    public async Task RawWriterPreservesEveryMacintoshSectorAndFileSystem(string directory, string fileName, string targetFormatId)
    {
        var sourcePath = ImagePath(directory, fileName);
        var outputPath = Path.Combine(Path.GetTempPath(), $"gwgui-mac-{Guid.NewGuid():N}.img");
        try
        {
            await MediaEngineFactory.CreateMacintoshConversionService().ConvertAsync(sourcePath, outputPath, targetFormatId);
            Assert.Equal(await File.ReadAllBytesAsync(sourcePath), await File.ReadAllBytesAsync(outputPath));
            var reader = new AppleDiskImageReader();
            var source = await reader.ReadAsync(sourcePath);
            var reopened = await reader.ReadAsync(outputPath);
            AssertImagesEqual(source, reopened);
            var sourceVolume = ReadFileSystem(source);
            var reopenedVolume = ReadFileSystem(reopened);
            Assert.Equal(sourceVolume.Name, reopenedVolume.Name);
            AssertEntriesEqual(sourceVolume.Entries, reopenedVolume.Entries);
        }
        finally
        {
            if (File.Exists(outputPath)) File.Delete(outputPath);
        }
    }

    [Theory]
    [InlineData(".image")]
    [InlineData(".dc42")]
    public async Task DiskCopyWriterPreservesHeaderChecksumsCatalogAndForks(string extension)
    {
        var sourcePath = ImagePath("3.5 pouces - HFS - 800 Kio", "System 3.2.dsk");
        var outputPath = Path.Combine(Path.GetTempPath(), $"System 3.2{extension}");
        try
        {
            Assert.True(MacintoshConversionService.CanCreate(DiskImageFormatIds.AppleMacHfs, extension));
            await MediaEngineFactory.CreateMacintoshConversionService().ConvertAsync(sourcePath, outputPath, DiskImageFormatIds.AppleMacHfs);
            var bytes = await File.ReadAllBytesAsync(outputPath);
            Assert.Equal(DiskCopyFormat.PrivateWord, BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(DiskCopyLayout.PrivateWordOffset)));
            var dataLength = checked((int)BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(DiskCopyLayout.DataLengthOffset)));
            var tagLength = checked((int)BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(DiskCopyLayout.TagLengthOffset)));
            Assert.Equal(819_200, dataLength);
            Assert.Equal(0, tagLength);
            Assert.Equal(DiskCopyReader.CalculateChecksum(bytes.AsSpan(DiskCopyLayout.HeaderSize, dataLength)), BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(DiskCopyLayout.DataChecksumOffset)));
            var detailed = DiskCopyReader.ReadDetailed(bytes);
            Assert.Equal("System 3.2", System.Text.Encoding.ASCII.GetString(detailed.NameBytes.ToArray()));
            Assert.Equal(DiskCopyFormat.DiskFormat800K, detailed.DiskFormat);
            Assert.Equal(DiskCopyFormat.FormatByteMacintoshHfs, detailed.FormatByte);
            var source = await new AppleDiskImageReader().ReadAsync(sourcePath);
            AssertImagesEqual(source, detailed.Image);
            AssertEntriesEqual(ReadFileSystem(source).Entries, ReadFileSystem(detailed.Image).Entries);
        }
        finally
        {
            if (File.Exists(outputPath)) File.Delete(outputPath);
        }
    }

    private static FileSystemVolume ReadFileSystem(GWGUI.MediaEngine.SectorImages.SectorImage image)
    {
        if (new MacMfsFileSystemReader().CanRead(image)) return new MacMfsFileSystemReader().Read(image);
        var hfs = new MacHfsFileSystemReader();
        Assert.True(hfs.CanRead(image));
        return hfs.Read(image);
    }

    private static void AssertImagesEqual(GWGUI.MediaEngine.SectorImages.SectorImage expected, GWGUI.MediaEngine.SectorImages.SectorImage actual)
    {
        Assert.Equal(expected.BlockCount, actual.BlockCount);
        for (var logical = 0; logical < expected.BlockCount; logical++)
        {
            Assert.Equal(expected.GetBlock(logical).ToArray(), actual.GetBlock(logical).ToArray());
            Assert.Equal(expected.AvailableBlocks.Single(block => block.LogicalBlock == logical).Address, actual.AvailableBlocks.Single(block => block.LogicalBlock == logical).Address);
        }
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

    private static string ImagePath(string directory, string fileName) => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "image_test", "validated_images", "Apple", "Macintosh", directory, fileName));
}
