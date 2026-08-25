using GWGUI.Emulation;
using GWGUI.Emulation.Atari;
using GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Hfs;
using System.IO;

namespace GWGUI.Tests;

public sealed class AtariHatariStorageTests
{
    private static readonly IReadOnlySet<string> SupportedExtensions =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "vhd", "ide", "gem", "st" };

    [Theory]
    [InlineData("disk.vhd", AtariStorageBus.Acsi)]
    [InlineData("disk.ide", AtariStorageBus.Ide)]
    public void HardDiskImages_AreResolvedByVerifiedExtension(string fileName, AtariStorageBus expectedBus)
    {
        var root = CreateRoot();
        var path = Path.Combine(root, fileName);
        File.WriteAllBytes(path, [1]);
        try
        {
            var media = CreateHardDisk(path);
            var storage = AtariHatariStorageFunctions.Prepare(AtariMachineModel.St, media, SupportedExtensions);

            Assert.Equal(expectedBus, storage.Bus);
            Assert.Equal(Path.GetFullPath(path), storage.RuntimePath);
            Assert.Equal(Path.GetFullPath(path), Assert.Single(storage.Volumes).Path);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void GemdosDirectory_CreatesTemporaryMarkerAndKeepsExternalPath()
    {
        var root = CreateRoot();
        var directory = Path.Combine(root, "My GEMDOS Drive");
        Directory.CreateDirectory(directory);
        var media = new AtariMediaConfiguration(directory, AtariMediaCategory.Directory,
            EmulationMediaSlot.HardDisk0, MountPoint: "D:");
        try
        {
            var storage = AtariHatariStorageFunctions.Prepare(AtariMachineModel.Ste, media, SupportedExtensions);

            Assert.Equal(AtariStorageBus.Gemdos, storage.Bus);
            Assert.Equal(directory + ".GEM", storage.RuntimePath);
            Assert.True(File.Exists(storage.RuntimePath));
            var volume = Assert.Single(storage.Volumes);
            Assert.Equal("D", volume.MountPoint);
            Assert.Equal(directory, volume.Path);

            AtariHatariStorageFunctions.Cleanup(storage);
            Assert.False(File.Exists(storage.RuntimePath));
            Assert.True(Directory.Exists(directory));
        }
        finally
        {
            AtariHatariStorageFunctions.Cleanup(null);
            if (File.Exists(directory + ".GEM")) File.Delete(directory + ".GEM");
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void GemdosPartitions_PreserveLettersAndSortedMountOrder()
    {
        var root = CreateRoot();
        var directory = Path.Combine(root, "Volumes");
        Directory.CreateDirectory(Path.Combine(directory, "D"));
        Directory.CreateDirectory(Path.Combine(directory, "C"));
        var media = new AtariMediaConfiguration(directory, AtariMediaCategory.Directory,
            EmulationMediaSlot.HardDisk0, MountOrder: 7);
        AtariHatariStorage? storage = null;
        try
        {
            storage = AtariHatariStorageFunctions.Prepare(AtariMachineModel.Tt, media, SupportedExtensions);

            Assert.Equal(["C", "D"], storage.Volumes.Select(volume => volume.MountPoint));
            Assert.Equal([7, 8], storage.Volumes.Select(volume => volume.Order));
        }
        finally
        {
            AtariHatariStorageFunctions.Cleanup(storage);
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void GemdosMarkerFile_IsRejectedInsteadOfBeingTreatedAsHostDirectory()
    {
        var root = CreateRoot();
        var marker = Path.Combine(root, "drive.GEM");
        File.WriteAllBytes(marker, []);
        try
        {
            var media = new AtariMediaConfiguration(marker, AtariMediaCategory.Directory,
                EmulationMediaSlot.HardDisk0);
            var error = Assert.Throws<InvalidDataException>(() => AtariHatariStorageFunctions.Prepare(
                AtariMachineModel.St, media, SupportedExtensions));
            Assert.Equal(AtariHatariStorageErrors.GemdosRequiresDirectory, error.Message);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void MissingDirectory_IsRejectedPrecisely()
    {
        var root = CreateRoot();
        try
        {
            var media = new AtariMediaConfiguration(Path.Combine(root, "missing"), AtariMediaCategory.Directory,
                EmulationMediaSlot.HardDisk0);
            Assert.Throws<DirectoryNotFoundException>(() => AtariHatariStorageFunctions.Prepare(
                AtariMachineModel.St, media, SupportedExtensions));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void LockedWritableImage_IsRejectedAndReleasedAfterLockEnds()
    {
        var root = CreateRoot();
        var path = Path.Combine(root, "locked.vhd");
        File.WriteAllBytes(path, [1]);
        try
        {
            var media = CreateHardDisk(path);
            using (new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                Assert.Throws<IOException>(() => AtariHatariStorageFunctions.Prepare(
                    AtariMachineModel.St, media, SupportedExtensions));

            var storage = AtariHatariStorageFunctions.Prepare(AtariMachineModel.St, media, SupportedExtensions);
            Assert.Equal(path, storage.RuntimePath);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void ModelCapabilities_RejectAcsiOnFalconButAllowIde()
    {
        var root = CreateRoot();
        var acsi = Path.Combine(root, "disk.vhd");
        var ide = Path.Combine(root, "disk.ide");
        File.WriteAllBytes(acsi, [1]);
        File.WriteAllBytes(ide, [1]);
        try
        {
            Assert.Throws<InvalidOperationException>(() => AtariHatariStorageFunctions.Prepare(
                AtariMachineModel.Falcon, CreateHardDisk(acsi), SupportedExtensions));
            Assert.Equal(AtariStorageBus.Ide, AtariHatariStorageFunctions.Prepare(
                AtariMachineModel.Falcon, CreateHardDisk(ide), SupportedExtensions).Bus);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void ReadOnlySelection_OverridesHardDriveWriteProtectionOption()
    {
        var root = CreateRoot();
        var path = Path.Combine(root, "disk.vhd");
        File.WriteAllBytes(path, [1]);
        try
        {
            var media = CreateHardDisk(path) with { IsReadOnly = true };
            var storage = AtariHatariStorageFunctions.Prepare(AtariMachineModel.St, media, SupportedExtensions);
            var options = AtariHatariStorageFunctions.ApplyWriteProtection(
                new Dictionary<string, string> { ["hatari_writeprotect_hd"] = "off" }, storage);

            Assert.Equal("on", options["hatari_writeprotect_hd"]);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void MultipleStartupMedia_AreRejectedInsteadOfSilentlyDroppingMounts()
    {
        var root = CreateRoot();
        var floppy = Path.Combine(root, "boot.st");
        var hardDisk = Path.Combine(root, "disk.vhd");
        File.WriteAllBytes(floppy, [1]);
        File.WriteAllBytes(hardDisk, [1]);
        try
        {
            var configuration = new AtariMachineConfiguration(AtariMachineModel.St, media:
            [
                new AtariMediaConfiguration(floppy, AtariMediaCategory.Floppy, EmulationMediaSlot.Floppy0),
                CreateHardDisk(hardDisk) with { MountOrder = 1 }
            ]);
            Assert.Throws<InvalidOperationException>(() => AtariHatariContentFunctions.Prepare(
                configuration, Path.Combine(root, "session"), SupportedExtensions));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void ConfigurationMount_IsRestoredAfterCleanPowerCycle()
    {
        var root = CreateRoot();
        var directory = Path.Combine(root, "Persistent Drive");
        Directory.CreateDirectory(directory);
        var configuration = new AtariMachineConfiguration(AtariMachineModel.St, media:
        [
            new AtariMediaConfiguration(directory, AtariMediaCategory.Directory, EmulationMediaSlot.HardDisk0,
                MountPoint: "E")
        ]);
        try
        {
            var first = AtariHatariContentFunctions.Prepare(configuration,
                Path.Combine(root, "first-session"), SupportedExtensions)!;
            Assert.Equal("E", Assert.Single(first.Storage!.Volumes).MountPoint);
            AtariHatariContentFunctions.Cleanup(first);

            var second = AtariHatariContentFunctions.Prepare(configuration,
                Path.Combine(root, "second-session"), SupportedExtensions)!;
            Assert.Equal("E", Assert.Single(second.Storage!.Volumes).MountPoint);
            AtariHatariContentFunctions.Cleanup(second);
        }
        finally
        {
            if (File.Exists(directory + ".GEM")) File.Delete(directory + ".GEM");
            Directory.Delete(root, true);
        }
    }

    private static AtariMediaConfiguration CreateHardDisk(string path) =>
        new(path, AtariMediaCategory.HardDisk, EmulationMediaSlot.HardDisk0);

    private static string CreateRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), "GWGUI-Atari-HatariStorage", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }
}
