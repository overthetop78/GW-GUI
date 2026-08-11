using System.IO;
using GWGUI.MediaEngine.Containers.TeleDisk;
using GWGUI.MediaEngine.FileSystems.Readers;
using GWGUI.MediaEngine.Images;
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

    private async Task VerifyImage(string fileName)
    {
        var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "image_test", "validated_images", "UCSD", "p-System", "5.25 pouces - IBM MFM - 160 Kio", fileName));
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
}
