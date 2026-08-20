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
    public void Atari400AcceptsItsOsRevisionsButNeverAnAtariStTos()
    {
        var osA = AtariFirmwareCatalog.Get(AtariFirmwareConstants.AtariOsAId);
        var osANtsc = AtariFirmwareCatalog.Get(AtariFirmwareConstants.AtariOsANtscId);
        var osB = AtariFirmwareCatalog.Get(AtariFirmwareConstants.AtariOsBId);
        var tos = AtariFirmwareCatalog.Get(AtariFirmwareFunctions.TosId(AtariStModelConstants.Tos206));

        Assert.Equal(AtariFirmwareCompatibility.Compatible,
            AtariFirmwareScanFunctions.Classify(osA, null, AtariMachineModel.Atari400, null));
        Assert.Equal("Rev. A PAL", osA.Version);
        Assert.Equal(AtariFirmwareCompatibility.Compatible,
            AtariFirmwareScanFunctions.Classify(osANtsc, null, AtariMachineModel.Atari400, null));
        Assert.Equal("Rev. A NTSC", osANtsc.Version);
        Assert.Equal(AtariFirmwareCompatibility.Compatible,
            AtariFirmwareScanFunctions.Classify(osB, null, AtariMachineModel.Atari400, null));
        Assert.Equal("Rev. B NTSC", osB.Version);
        Assert.Equal(AtariFirmwareCompatibility.Incompatible,
            AtariFirmwareScanFunctions.Classify(tos, null, AtariMachineModel.Atari400, null));
    }

    [Fact]
    public void PublishedFingerprintIsIdentifiedAsTheExactFirmwareDefinition()
    {
        var definition = AtariFirmwareScanFunctions.Identify(AtariFirmwareConstants.LynxBootMd5);

        Assert.NotNull(definition);
        Assert.Equal(AtariFirmwareConstants.LynxBootId, definition.Id);
        Assert.Equal(AtariFirmwareKind.LynxBootRom, definition.Kind);
    }

    [Theory]
    [InlineData(AtariFirmwareConstants.Atari5200RevisionAMd5, AtariMachineModel.Atari5200,
        AtariFirmwareKind.Atari5200Bios, "Revision A")]
    [InlineData(AtariFirmwareConstants.Atari7800EuropeMd5, AtariMachineModel.Atari7800,
        AtariFirmwareKind.Atari7800Bios, "Europe")]
    [InlineData(AtariFirmwareConstants.AtariXlXeOsV3Md5, AtariMachineModel.Atari800Xl,
        AtariFirmwareKind.AtariXlOs, "BB01R3")]
    [InlineData(AtariFirmwareConstants.AtariXlXeOsR59Md5, AtariMachineModel.Atari130Xe,
        AtariFirmwareKind.AtariXlOs, "BB01R59")]
    [InlineData(AtariFirmwareConstants.AtariXlXeOsR59AMd5, AtariMachineModel.Xegs,
        AtariFirmwareKind.AtariXlOs, "BB01R59A")]
    public void VerifiedAlternateRevisionsAreKnownAndCompatible(string md5, AtariMachineModel model,
        AtariFirmwareKind kind, string version)
    {
        var definition = AtariFirmwareScanFunctions.Identify(md5);

        Assert.NotNull(definition);
        Assert.Equal(kind, definition.Kind);
        Assert.Equal(version, definition.Version);
        Assert.Equal(AtariFirmwareCompatibility.Compatible,
            AtariFirmwareScanFunctions.Classify(definition, null, model, null));
    }

    [Fact]
    public void ExternalJaguarBiosIsNamedButCannotBeSelectedByEmbeddedBiosCore()
    {
        var definition = AtariFirmwareScanFunctions.Identify(AtariFirmwareConstants.JaguarBootMd5);

        Assert.NotNull(definition);
        Assert.Equal("World", definition.Version);
        Assert.Equal(AtariFirmwareCompatibility.Incompatible,
            AtariFirmwareScanFunctions.Classify(definition, null, AtariMachineModel.Jaguar, null));
        var scanned = new AtariScannedFirmware("jaguar.j64", 131072, AtariFirmwareConstants.JaguarBootMd5,
            AtariFirmwareDetectionStatus.Known, definition, AtariFirmwareCompatibility.Incompatible, false, null);
        Assert.Throws<InvalidOperationException>(() => AtariFirmwareScanFunctions.CreateSelection(scanned));
    }

    [Theory]
    [InlineData(0x01, 0x62, 0x00, 0x05, AtariMachineModel.Ste, AtariStRegion.France, "1.62")]
    [InlineData(0x02, 0x06, 0x00, 0x05, AtariMachineModel.MegaSte, AtariStRegion.France, "2.06")]
    [InlineData(0x03, 0x06, 0x00, 0x05, AtariMachineModel.Tt, AtariStRegion.France, "3.06")]
    [InlineData(0x04, 0x04, 0x00, 0xFF, AtariMachineModel.Falcon, AtariStRegion.Multilingual, "4.04")]
    public async Task TosHeaderIdentifiesVersionRegionAndCompatibleModel(byte major, byte minor,
        byte configurationHigh, byte configurationLow, AtariMachineModel model, AtariStRegion expectedRegion,
        string expectedVersion)
    {
        var root = NewRoot();
        try
        {
            var directory = Path.Combine(root,
                AtariFirmwareScanFunctions.FamilyDirectoryName(AtariMachineFamily.St));
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, $"tos-{expectedVersion}.img");
            var data = new byte[262_144];
            data[0] = 0x60; data[1] = 0x2E;
            data[2] = major; data[3] = minor;
            data[28] = configurationHigh; data[29] = configurationLow;
            await File.WriteAllBytesAsync(path, data);

            var identity = await AtariFirmwareScanFunctions.IdentifyTosAsync(path, CancellationToken.None);
            var scanned = Assert.Single(await new AtariFirmwareScanner(root).ScanAsync(model, expectedRegion));

            Assert.NotNull(identity);
            Assert.Equal(expectedVersion, identity.Value.Definition.Version);
            Assert.Equal(expectedRegion, identity.Value.Region);
            Assert.Equal(AtariFirmwareDetectionStatus.Known, scanned.Detection);
            Assert.Equal(AtariFirmwareCompatibility.Compatible, scanned.Compatibility);
        }
        finally { DeleteRoot(root); }
    }

    [Fact]
    public async Task TosHeaderRegionMismatchIsIncompatible()
    {
        var root = NewRoot();
        try
        {
            var directory = Path.Combine(root,
                AtariFirmwareScanFunctions.FamilyDirectoryName(AtariMachineFamily.St));
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, "tos-1.62.img");
            var data = new byte[262_144];
            data[0] = 0x60; data[1] = 0x2E; data[2] = 0x01; data[3] = 0x62;
            data[28] = 0x00; data[29] = 0x05;
            await File.WriteAllBytesAsync(path, data);

            var scanned = Assert.Single(await new AtariFirmwareScanner(root).ScanAsync(
                AtariMachineModel.Ste, AtariStRegion.Germany));

            Assert.Equal(AtariFirmwareCompatibility.Incompatible, scanned.Compatibility);
        }
        finally { DeleteRoot(root); }
    }

    [Fact]
    public async Task OriginalTosBranchHeaderIsRecognized()
    {
        var root = NewRoot();
        try
        {
            var directory = Path.Combine(root,
                AtariFirmwareScanFunctions.FamilyDirectoryName(AtariMachineFamily.St));
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, "tos-1.00.img");
            var data = new byte[196_608];
            data[0] = 0x60; data[1] = 0x1E; data[2] = 0x01; data[3] = 0x00;
            data[28] = 0x00; data[29] = 0x07;
            await File.WriteAllBytesAsync(path, data);

            var scanned = Assert.Single(await new AtariFirmwareScanner(root).ScanAsync(
                AtariMachineModel.St, AtariStRegion.UnitedKingdom));

            Assert.Equal(AtariFirmwareDetectionStatus.Known, scanned.Detection);
            Assert.Equal("1.00", scanned.Definition?.Version);
            Assert.Equal(AtariFirmwareCompatibility.Compatible, scanned.Compatibility);
        }
        finally { DeleteRoot(root); }
    }

    [Theory]
    [InlineData("EmuTOS 0.9.9.1", 262_144, AtariMachineModel.Ste, "EmuTOS 0.9.9.1",
        AtariFirmwareDistribution.BuiltInOpenReplacement)]
    [InlineData("KAOS - TOS 1.4.3", 196_608, AtariMachineModel.St, "KAOS TOS 1.4.3",
        AtariFirmwareDistribution.UserSuppliedCopyrighted)]
    public async Task AlternativeTosIsIdentifiedByEmbeddedProductAndVersion(string marker, int imageSize,
        AtariMachineModel model, string expectedVersion, AtariFirmwareDistribution expectedDistribution)
    {
        var root = NewRoot();
        try
        {
            var directory = Path.Combine(root,
                AtariFirmwareScanFunctions.FamilyDirectoryName(AtariMachineFamily.St));
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, "alternative.img");
            var data = new byte[imageSize];
            data[0] = 0x60; data[1] = 0x2E; data[2] = 0x01; data[3] = 0x04;
            data[28] = 0x00; data[29] = 0x05;
            System.Text.Encoding.ASCII.GetBytes(marker).CopyTo(data, 128);
            await File.WriteAllBytesAsync(path, data);

            var scanned = Assert.Single(await new AtariFirmwareScanner(root).ScanAsync(model, AtariStRegion.France));

            Assert.Equal(AtariFirmwareDetectionStatus.Known, scanned.Detection);
            Assert.Equal(expectedVersion, scanned.Definition?.Version);
            Assert.Equal(expectedDistribution, scanned.Definition?.Distribution);
            Assert.Equal(AtariFirmwareCompatibility.Compatible, scanned.Compatibility);
        }
        finally { DeleteRoot(root); }
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
    public async Task HeaderIdentifiedTosIsStagedUnderTheSharedHatariFileName()
    {
        var root = NewRoot();
        try
        {
            Directory.CreateDirectory(root);
            var source = Path.Combine(root, "TOS v2.06 (1991)(Atari)(Mega-STE)(FR).img");
            var system = Path.Combine(root, "session", "System");
            var content = new byte[262_144];
            content[0] = 0x60; content[1] = 0x2E; content[2] = 0x02; content[3] = 0x06;
            content[28] = 0x00; content[29] = 0x05;
            await File.WriteAllBytesAsync(source, content);
            var configuration = new AtariMachineConfiguration(AtariMachineModel.MegaSte,
                [new AtariFirmwareConfiguration(AtariFirmwareKind.Tos, source, true)]);

            AtariFirmwareRuntimeFunctions.PrepareSystemDirectory(configuration, system);

            Assert.Equal(content, await File.ReadAllBytesAsync(
                Path.Combine(system, AtariFirmwareConstants.TosFileName)));
            Assert.True(File.Exists(source));
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
