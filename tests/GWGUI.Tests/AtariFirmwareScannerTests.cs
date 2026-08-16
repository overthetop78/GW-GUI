using System.IO;
using GWGUI.App;
using GWGUI.Emulation.Atari;

namespace GWGUI.Tests;

public sealed class AtariFirmwareScannerTests
{
    private const byte FirstTestByte = 0x2A;
    private const byte SecondTestByte = 0x51;

    [Fact]
    public void ApplicationStorageExposesEveryAtariFirmwareFamilyDirectory()
    {
        Assert.Equal(Path.Combine(StoragePaths.AtariFirmwareDirectory, AtariFirmwareConstants.StFamilyDirectoryName),
            StoragePaths.AtariStFirmwareDirectory);
        Assert.Equal(Path.Combine(StoragePaths.AtariFirmwareDirectory, AtariFirmwareConstants.EightBitFamilyDirectoryName),
            StoragePaths.AtariEightBitFirmwareDirectory);
        Assert.Equal(Path.Combine(StoragePaths.AtariFirmwareDirectory, AtariFirmwareConstants.Atari5200FamilyDirectoryName),
            StoragePaths.Atari5200FirmwareDirectory);
        Assert.Equal(Path.Combine(StoragePaths.AtariFirmwareDirectory, AtariFirmwareConstants.Atari2600FamilyDirectoryName),
            StoragePaths.Atari2600FirmwareDirectory);
        Assert.Equal(Path.Combine(StoragePaths.AtariFirmwareDirectory, AtariFirmwareConstants.Atari7800FamilyDirectoryName),
            StoragePaths.Atari7800FirmwareDirectory);
        Assert.Equal(Path.Combine(StoragePaths.AtariFirmwareDirectory, AtariFirmwareConstants.LynxFamilyDirectoryName),
            StoragePaths.AtariLynxFirmwareDirectory);
        Assert.Equal(Path.Combine(StoragePaths.AtariFirmwareDirectory, AtariFirmwareConstants.JaguarFamilyDirectoryName),
            StoragePaths.AtariJaguarFirmwareDirectory);
    }

    [Fact]
    public async Task MissingRootCreatesEveryFamilyDirectoryAndReturnsNoFirmware()
    {
        var root = NewRoot();
        try
        {
            var result = await new AtariFirmwareScanner(root).ScanAsync(AtariMachineModel.St);

            Assert.Empty(result);
            Assert.All(Enum.GetValues<AtariMachineFamily>(), family => Assert.True(Directory.Exists(
                Path.Combine(root, AtariFirmwareScanFunctions.FamilyDirectoryName(family)))));
        }
        finally { DeleteRoot(root); }
    }

    [Fact]
    public async Task RefreshIgnoresIrrelevantFilesAndKeepsUnknownFirmwareExplicit()
    {
        var root = NewRoot();
        try
        {
            var scanner = new AtariFirmwareScanner(root);
            await scanner.ScanAsync(AtariMachineModel.Lynx);
            var directory = Path.Combine(root,
                AtariFirmwareScanFunctions.FamilyDirectoryName(AtariMachineFamily.Lynx));
            await File.WriteAllTextAsync(Path.Combine(directory, "notes.txt"), "ignored");
            var firmwarePath = Path.Combine(directory, AtariFirmwareConstants.LynxBootFileName);
            await File.WriteAllBytesAsync(firmwarePath, [FirstTestByte]);

            var result = await scanner.ScanAsync(AtariMachineModel.Lynx);
            var firmware = Assert.Single(result);
            Assert.Equal(AtariFirmwareDetectionStatus.Unknown, firmware.Detection);
            Assert.Equal(AtariFirmwareCompatibility.PartiallyCompatible, firmware.Compatibility);
            Assert.Equal(AtariFirmwareKind.LynxBootRom, firmware.Definition?.Kind);
            Assert.Equal(Path.GetFullPath(firmwarePath), firmware.Path);
        }
        finally { DeleteRoot(root); }
    }

    [Fact]
    public async Task DuplicateContentsAreReportedWithoutChangingEitherFile()
    {
        var root = NewRoot();
        try
        {
            var scanner = new AtariFirmwareScanner(root);
            await scanner.ScanAsync(AtariMachineModel.Atari7800);
            var directory = Path.Combine(root,
                AtariFirmwareScanFunctions.FamilyDirectoryName(AtariMachineFamily.Atari7800));
            var first = Path.Combine(directory, "first.rom");
            var second = Path.Combine(directory, "second.rom");
            var content = new[] { FirstTestByte, SecondTestByte };
            await File.WriteAllBytesAsync(first, content);
            await File.WriteAllBytesAsync(second, content);

            var result = await scanner.ScanAsync(AtariMachineModel.Atari7800);

            Assert.Equal(AtariFirmwareScannerTestConstants.DuplicateFileCount, result.Count);
            Assert.All(result, firmware => Assert.True(firmware.IsDuplicate));
            Assert.Equal(content, await File.ReadAllBytesAsync(first));
            Assert.Equal(content, await File.ReadAllBytesAsync(second));
        }
        finally { DeleteRoot(root); }
    }

    [Fact]
    public async Task LockedCandidateIsReportedAsUnreadable()
    {
        var root = NewRoot();
        try
        {
            var scanner = new AtariFirmwareScanner(root);
            await scanner.ScanAsync(AtariMachineModel.Lynx);
            var path = Path.Combine(root,
                AtariFirmwareScanFunctions.FamilyDirectoryName(AtariMachineFamily.Lynx),
                AtariFirmwareConstants.LynxBootFileName);
            await File.WriteAllBytesAsync(path, [FirstTestByte]);
            using var locked = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

            var firmware = Assert.Single(await scanner.ScanAsync(AtariMachineModel.Lynx));

            Assert.Equal(AtariFirmwareDetectionStatus.Unreadable, firmware.Detection);
            Assert.Equal(AtariFirmwareCompatibility.Incompatible, firmware.Compatibility);
            Assert.NotNull(firmware.ReadError);
        }
        finally { DeleteRoot(root); }
    }

    [Fact]
    public void KnownTosFingerprintWithWrongRegionIsIncompatible()
    {
        var definition = AtariFirmwareCatalog.Get(
            AtariFirmwareFunctions.TosId(AtariStModelConstants.Tos102));

        Assert.Equal(AtariFirmwareCompatibility.Compatible,
            AtariFirmwareScanFunctions.Classify(definition, null, AtariMachineModel.St,
                AtariStRegion.UnitedStates));
        Assert.Equal(AtariFirmwareCompatibility.Incompatible,
            AtariFirmwareScanFunctions.Classify(definition, null, AtariMachineModel.St,
                AtariStRegion.Germany));
    }

    [Fact]
    public void PublishedFingerprintIsIdentifiedAsTheExactFirmwareDefinition()
    {
        var definition = AtariFirmwareScanFunctions.Identify(AtariFirmwareConstants.LynxBootMd5);

        Assert.NotNull(definition);
        Assert.Equal(AtariFirmwareConstants.LynxBootId, definition.Id);
        Assert.Equal(AtariFirmwareKind.LynxBootRom, definition.Kind);
    }

    [Fact]
    public async Task SelectionReferencesOriginalPathAndRefreshDoesNotCopyIt()
    {
        var root = NewRoot();
        try
        {
            var scanner = new AtariFirmwareScanner(root);
            await scanner.ScanAsync(AtariMachineModel.Lynx);
            var path = Path.Combine(root,
                AtariFirmwareScanFunctions.FamilyDirectoryName(AtariMachineFamily.Lynx),
                AtariFirmwareConstants.LynxBootFileName);
            await File.WriteAllBytesAsync(path, [FirstTestByte]);
            var item = Assert.Single(await scanner.ScanAsync(AtariMachineModel.Lynx));

            var selection = AtariFirmwareScanFunctions.CreateSelection(item);

            Assert.Equal(Path.GetFullPath(path), selection.Path);
            Assert.Equal(AtariFirmwareKind.LynxBootRom, selection.Kind);
            Assert.Single(Directory.EnumerateFiles(Path.GetDirectoryName(path)!));
        }
        finally { DeleteRoot(root); }
    }

    [Fact]
    public void MissingRequiredFirmwareIsRejectedWithItsExactRoleAndModel()
    {
        var configuration = new AtariMachineConfiguration(AtariMachineModel.Lynx);

        var error = Assert.Throws<FileNotFoundException>(() =>
            AtariFirmwareRuntimeFunctions.ValidateRequiredFirmware(configuration));

        Assert.Contains(nameof(AtariFirmwareKind.LynxBootRom), error.Message, StringComparison.Ordinal);
        Assert.Contains(nameof(AtariMachineModel.Lynx), error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SelectedFirmwareIsStagedUnderExactExpectedNameWithoutChangingSource()
    {
        var root = NewRoot();
        try
        {
            Directory.CreateDirectory(root);
            var source = Path.Combine(root, "selected.img");
            var system = Path.Combine(root, "session", "System");
            var content = new[] { FirstTestByte, SecondTestByte };
            await File.WriteAllBytesAsync(source, content);
            var written = File.GetLastWriteTimeUtc(source);
            var configuration = new AtariMachineConfiguration(AtariMachineModel.Lynx,
                [new AtariFirmwareConfiguration(AtariFirmwareKind.LynxBootRom, source, true)]);

            AtariFirmwareRuntimeFunctions.PrepareSystemDirectory(configuration, system);

            Assert.Equal(content, await File.ReadAllBytesAsync(source));
            Assert.Equal(written, File.GetLastWriteTimeUtc(source));
            Assert.Equal(content, await File.ReadAllBytesAsync(
                Path.Combine(system, AtariFirmwareConstants.LynxBootFileName)));
            Assert.Single(Directory.EnumerateFiles(system));
        }
        finally { DeleteRoot(root); }
    }

    [Fact]
    public async Task AmbiguousUnknownFirmwareIsNeverSilentlyRenamedAsAnotherRom()
    {
        var root = NewRoot();
        try
        {
            Directory.CreateDirectory(root);
            var source = Path.Combine(root, "unknown.rom");
            await File.WriteAllBytesAsync(source, [FirstTestByte]);

            var error = Assert.Throws<InvalidDataException>(() =>
                AtariFirmwareRuntimeFunctions.ResolveDefinition(AtariMachineModel.Atari800Xl,
                    AtariFirmwareKind.AtariXlOs, source));

            Assert.Contains(nameof(AtariFirmwareKind.AtariXlOs), error.Message, StringComparison.Ordinal);
            Assert.Contains(source, error.Message, StringComparison.Ordinal);
        }
        finally { DeleteRoot(root); }
    }

    [Fact]
    public async Task ReusedSessionCannotSilentlyRetainAFormerOptionalFirmware()
    {
        var root = NewRoot();
        try
        {
            var system = Path.Combine(root, "System");
            Directory.CreateDirectory(system);
            var stale = Path.Combine(system, AtariFirmwareConstants.Atari7800FileName);
            await File.WriteAllBytesAsync(stale, [FirstTestByte]);
            var configuration = new AtariMachineConfiguration(AtariMachineModel.Atari7800);

            AtariFirmwareRuntimeFunctions.PrepareSystemDirectory(configuration, system);

            Assert.False(File.Exists(stale));
        }
        finally { DeleteRoot(root); }
    }

    private static string NewRoot() => Path.Combine(Path.GetTempPath(),
        nameof(AtariFirmwareScannerTests), Guid.NewGuid().ToString("N"));
    private static void DeleteRoot(string root)
    {
        if (Directory.Exists(root)) Directory.Delete(root, true);
    }
}

internal static class AtariFirmwareScannerTestConstants
{
    internal const int DuplicateFileCount = 2;
}
