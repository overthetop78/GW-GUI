using GWGUI.Emulation.Common;
using GWGUI.MediaEngine.Composition;
using GWGUI.MediaEngine.Containers.TeleDisk;
using GWGUI.MediaEngine.Containers.Ucsd.Raw;
using GWGUI.MediaEngine.Conversion.Ucsd;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Exploration;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.FileSystems.Ucsd;
using GWGUI.MediaEngine.Geometries.Ucsd;
using GWGUI.MediaEngine.Reconstruction.Iso;
using GWGUI.MediaEngine.SectorImages;
using System.IO;
using GWGUI.MediaEngine.Decoding;
using Xunit.Abstractions;

namespace GWGUI.Tests;

public sealed class UcsdDiskImageTests(ITestOutputHelper output)
{
    [Fact]
    public Task SuppliedUcsdPascalTeleDiskImageExposesItsDirectory() => VerifyImage("ucsdpasc.td0");

    [Fact]
    public Task SuppliedUcsdStartTeleDiskImageExposesItsDirectory() => VerifyImage("ucsdstrt.td0");

    [Fact]
    public Task SuppliedUcsdSystemOneTeleDiskImageExposesItsDirectory() => VerifyImage("ucsdsys1.td0");

    [Fact]
    public Task SuppliedUcsdSystemTwoTeleDiskImageExposesItsDirectory() => VerifyImage("ucsdsys2.td0");

    [Fact]
    public Task SuppliedUcsdUtilitiesTeleDiskImageExposesItsDirectory() => VerifyImage("ucsdutil.td0");

    [Fact]
    public Task SuppliedUcsdZInterpreterTeleDiskImageExposesItsDirectory() => VerifyImage("ucsdzint.td0");

    [Fact]
    public async Task ExplicitUcsdScpSelectionUsesTheIsoReader()
    {
        var document = await DiskImageExplorer.CreateDefault().ExploreAsync(ScpImagePath(), DiskImageFormatIds.UcsdIbmMfm);
        Assert.Equal(DiskImageFormatIds.UcsdIbmMfm, document.Image.FormatId);
        Assert.True(document.FileSystemRecognized);
    }

    [Fact]
    public async Task AutomaticIsoCandidatesIncludeUcsd()
    {
        var document = await DiskImageExplorer.CreateDefault().ExploreAsync(ScpImagePath());
        Assert.Equal(DiskImageFormatIds.UcsdIbmMfm, document.Image.FormatId);
        Assert.True(document.FileSystemRecognized);
    }

    [Fact]
    public async Task UcsdImgConversionPreservesCatalogSegmentsAndFileContents()
    {
        var sourcePath = ImagePath("ucsdpasc.td0");
        if (!File.Exists(sourcePath)) return;
        var outputPath = Path.Combine(Path.GetTempPath(), $"gwgui-ucsd-{Guid.NewGuid():N}.img");
        try
        {
            Assert.True(UcsdImgConversionService.CanCreate(DiskImageFormatIds.UcsdIbmMfm, ".img"));
            await MediaEngineFactory.CreateUcsdImgConversionService().ConvertAsync(sourcePath, outputPath);
            var sourceImage = (await new Td0Reader().ReadAsync(sourcePath)).WithFormatId(DiskImageFormatIds.UcsdIbmMfm);
            var reopenedImage = await new UcsdRawImageReader().ReadAsync(outputPath);
            Assert.Equal(40, reopenedImage.Cylinders);
            Assert.Equal(UcsdIbmMfmGeometry.HeadCount, reopenedImage.Heads);
            Assert.Equal(UcsdIbmMfmGeometry.LogicalSectorsPerCylinder, reopenedImage.SectorsPerTrack);
            Assert.Equal(sourceImage.BlockCount, reopenedImage.BlockCount);
            for (var block = 0; block < sourceImage.BlockCount; block++) Assert.Equal(sourceImage.GetBlock(block).ToArray(), reopenedImage.GetBlock(block).ToArray());
            var reader = new UcsdFileSystemReader();
            var sourceVolume = reader.Read(sourceImage);
            var reopenedVolume = reader.Read(reopenedImage);
            Assert.Equal(sourceVolume.Name, reopenedVolume.Name);
            Assert.Equal(sourceVolume.Capacity, reopenedVolume.Capacity);
            Assert.Equal(sourceVolume.FreeBytes, reopenedVolume.FreeBytes);
            AssertEntriesEqual(sourceVolume.Entries, reopenedVolume.Entries);
        }
        finally
        {
            if (File.Exists(outputPath)) File.Delete(outputPath);
        }
    }

    [Fact]
    public async Task UcsdConversionCreatesReadableTeleDiskImage()
    {
        var sourcePath = ImagePath("ucsdpasc.td0");
        if (!File.Exists(sourcePath)) return;
        var rawPath = Path.Combine(Path.GetTempPath(), $"gwgui-ucsd-{Guid.NewGuid():N}.img");
        var outputPath = Path.Combine(Path.GetTempPath(), $"gwgui-ucsd-{Guid.NewGuid():N}.td0");
        try
        {
            var service = MediaEngineFactory.CreateUcsdImgConversionService();
            Assert.True(UcsdImgConversionService.CanCreate(DiskImageFormatIds.UcsdIbmMfm, ".td0"));
            await service.ConvertAsync(sourcePath, rawPath);
            await service.ConvertAsync(rawPath, outputPath);
            var source = await new Td0Reader().ReadAsync(sourcePath);
            var actual = await new Td0Reader().ReadAsync(outputPath);
            Assert.Equal(source.AvailableBlocks.Select(block => block.Data), actual.AvailableBlocks.Select(block => block.Data));
        }
        finally
        {
            if (File.Exists(rawPath)) File.Delete(rawPath);
            if (File.Exists(outputPath)) File.Delete(outputPath);
        }
    }

    [Fact]
    public void UcsdPolicyUsesPhysicalCandidatesAndDeclaredGeometry()
    {
        var physical = Enumerable.Range(1, UcsdIbmMfmGeometry.LogicalSectorsPerCylinder).ToDictionary(number => new SectorAddress(0, 0, number), number => new List<IsoSectorCandidate> { new(new(4, 1, number, 2, 512, true, 0, Data: new byte[512]), 1) });
        var addressed = new Dictionary<SectorAddress, List<IsoSectorCandidate>>();
        var image = new UcsdIsoScpSectorImagePolicy().Build(DiskImageFormatIds.UcsdIbmMfm, new(addressed, physical));
        Assert.Equal(UcsdIbmMfmGeometry.HeadCount, image.Heads);
        Assert.Equal(UcsdIbmMfmGeometry.LogicalSectorsPerCylinder, image.SectorsPerTrack);
        Assert.Equal(UcsdIbmMfmGeometry.LogicalSectorsPerCylinder, image.AvailableBlocks.Count);
    }

    private async Task VerifyImage(string fileName)
    {
        var path = ImagePath(fileName);
        if (!File.Exists(path)) return;
        var image = await new Td0Reader().ReadAsync(path);
        var reader = new UcsdFileSystemReader();
        Assert.True(reader.CanRead(image));
        var volume = reader.Read(image);
        output.WriteLine($"Format={image.FormatId}; Geometry={image.Cylinders}x{image.Heads}x{image.SectorsPerTrack}; Blocks={image.BlockCount}; Volume={volume.Name}; Files={volume.Entries.Count}; Free={volume.FreeBytes}");
        foreach (var entry in volume.Entries) output.WriteLine($"{entry.Name} | {entry.Comment} | {entry.Size} | {entry.Modified:yyyy-MM-dd}");
        foreach (var warning in volume.Warnings) output.WriteLine($"WARNING: {warning}");
        Assert.False(string.IsNullOrWhiteSpace(volume.Name));
        Assert.NotEmpty(volume.Entries);
        Assert.All(volume.Entries, entry => Assert.False(string.IsNullOrWhiteSpace(entry.Name)));
        Assert.Empty(volume.Warnings);
    }

    private static string ScpImagePath()
    {
        var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "image_test", "validated_images", "UCSD", "p-System", "5.25 pouces - IBM MFM - 160 Kio", "ucsdpasc [test].scp"));
        Assert.True(File.Exists(path), $"Image SCP UCSD obligatoire absente : {path}");
        return path;
    }

    private static string ImagePath(string fileName) => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "image_test", "validated_images", "UCSD", "p-System", "5.25 pouces - IBM MFM - 160 Kio", fileName));

    private static void AssertEntriesEqual(IReadOnlyList<FileSystemEntry> expected, IReadOnlyList<FileSystemEntry> actual)
    {
        Assert.Equal(expected.Count, actual.Count);
        for (var index = 0; index < expected.Count; index++)
        {
            Assert.Equal(expected[index].Name, actual[index].Name);
            Assert.Equal(expected[index].Kind, actual[index].Kind);
            Assert.Equal(expected[index].Size, actual[index].Size);
            Assert.Equal(expected[index].StorageReference, actual[index].StorageReference);
            Assert.Equal(expected[index].Content, actual[index].Content);
            AssertEntriesEqual(expected[index].Children, actual[index].Children);
        }
    }
}
