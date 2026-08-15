using System.IO;
using System.Net;
using System.Net.Http;
using GWGUI.Emulation.Amiga;

namespace GWGUI.Tests;

public sealed class AmigaConfigurationTests
{
    [Fact]
    public async Task ConfigurationStore_RoundTripsMultipleMachines()
    {
        var directory = Path.Combine(Path.GetTempPath(), "GWGUI-Amiga-Config", Guid.NewGuid().ToString("N"));
        var store = new AmigaConfigurationStore(directory);
        var first = AmigaMachineConfiguration.A500(@"C:\ROMs\Kickstart 1.3.rom", @"F:\Diskettes\Workbench.adf");
        var second = first with
        {
            Id = Guid.NewGuid(), Model = "A1200", InitialDiskPath = null, AudioEnabled = false,
            Input = new AmigaInputConfiguration(MouseDeviceId: "mouse-1",
                ControllerBindings: [new AmigaControllerBinding(0, AmigaControllerType.Cd32Pad, "gamepad-uuid")])
        };
        try
        {
            await store.SaveAsync(first);
            await store.SaveAsync(second);
            var broken = Path.Combine(directory, "broken");
            Directory.CreateDirectory(broken);
            await File.WriteAllTextAsync(Path.Combine(broken, "machine.json"), "{broken");
            var loaded = await store.LoadAllAsync();
            Assert.Equal(2, loaded.Count);
            Assert.Contains(loaded, configuration => configuration.Id == first.Id && configuration.Model == "A500");
            Assert.Contains(loaded, configuration => configuration.Id == second.Id && !configuration.AudioEnabled);
            Assert.Equal("gamepad-uuid", loaded.Single(configuration => configuration.Id == second.Id)
                .Input!.ControllerBindings![0].DeviceId);
            store.Delete(first.Id);
            Assert.Single(await store.LoadAllAsync());
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }
    }

    [Fact]
    public async Task ConfigurationStore_UsesRelativePathsOnlyInsideDataDirectory()
    {
        var root = Path.Combine(Path.GetTempPath(), "GWGUI-Amiga-Portable", Guid.NewGuid().ToString("N"));
        var data = Path.Combine(root, "Data");
        var configurations = Path.Combine(data, "Emulation", "Machines", "Amiga", "Configurations");
        var firmware = Path.Combine(data, "Emulation", "Machines", "Amiga", "Firmware", "kick.rom");
        var externalDisk = Path.Combine(root, "External", "game.adf");
        var hardDisk = Path.Combine(data, "Emulation", "Machines", "Amiga", "Media", "workbench.hdf");
        Directory.CreateDirectory(Path.GetDirectoryName(firmware)!);
        Directory.CreateDirectory(Path.GetDirectoryName(externalDisk)!);
        await File.WriteAllBytesAsync(firmware, [1]);
        await File.WriteAllBytesAsync(externalDisk, [2]);
        Directory.CreateDirectory(Path.GetDirectoryName(hardDisk)!);
        await File.WriteAllBytesAsync(hardDisk, [3]);
        try
        {
            var configuration = AmigaMachineConfiguration.A500(firmware, externalDisk) with
            {
                Media = [new AmigaMediaConfiguration(hardDisk, AmigaMediaKind.HardDrive)]
            };
            var store = new AmigaConfigurationStore(configurations, data);
            await store.SaveAsync(configuration);
            var json = await File.ReadAllTextAsync(Path.Combine(configurations, configuration.Id.ToString("N"), "machine.json"));
            Assert.Contains("Emulation/Machines/Amiga/Firmware/kick.rom", json, StringComparison.Ordinal);
            Assert.Contains(externalDisk.Replace("\\", "\\\\"), json, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Emulation/Machines/Amiga/Media/workbench.hdf", json, StringComparison.Ordinal);
            var loaded = Assert.Single(await store.LoadAllAsync());
            Assert.Equal(Path.GetFullPath(firmware), loaded.KickstartPath);
            Assert.Equal(Path.GetFullPath(externalDisk), loaded.InitialDiskPath);
            Assert.Equal(Path.GetFullPath(hardDisk), Assert.Single(loaded.Media!).Path);
            Assert.Equal(3, loaded.SchemaVersion);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void FirmwareCatalog_FindsRomBinAndKeyWithHashes()
    {
        var directory = Path.Combine(Path.GetTempPath(), "GWGUI-Amiga-Firmware", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            File.WriteAllBytes(Path.Combine(directory, "kick.rom"), [1, 2, 3]);
            File.WriteAllBytes(Path.Combine(directory, "extended.bin"), [4, 5]);
            File.WriteAllBytes(Path.Combine(directory, "rom.key"), [6]);
            var customKickstart = new byte[524288];
            customKickstart[12] = 0;
            customKickstart[13] = 34;
            customKickstart[14] = 0;
            customKickstart[15] = 5;
            File.WriteAllBytes(Path.Combine(directory, "custom.rom"), customKickstart);
            File.WriteAllText(Path.Combine(directory, "ignore.txt"), "ignored");
            var entries = new AmigaFirmwareCatalog(directory).Scan();
            Assert.Equal(4, entries.Count);
            Assert.All(entries, entry => Assert.Equal(64, entry.Sha256.Length));
            var detected = Assert.Single(entries, entry => entry.Path.EndsWith("custom.rom", StringComparison.Ordinal));
            Assert.Equal(AmigaFirmwareType.Kickstart, detected.Type);
            Assert.Equal("rev 34.005", detected.Version);
            Assert.Contains("A500", detected.CompatibleModels);
            Assert.False(detected.IsKnown);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void ModelCatalog_ContainsEveryExternallySupportedPreset()
    {
        Assert.Equal(10, AmigaModelCatalog.All.Count);
        Assert.Equal(
            ["A500", "A500PLUS", "A600", "A1000", "A1200", "A2000", "A3000", "A4000", "CDTV", "CD32"],
            AmigaModelCatalog.All.Select(model => model.Id));
        Assert.Equal("OCS", AmigaModelCatalog.Get("A1000").Chipset);
        Assert.Equal("OCS", AmigaModelCatalog.Get("A500").Chipset);
        Assert.Equal("AGA", AmigaModelCatalog.Get("A1200").Chipset);
        Assert.Equal("ECS", AmigaModelCatalog.Get("A3000").Chipset);
        Assert.Equal("A2000", AmigaModelCatalog.Get("A3000").BackendModel);
        Assert.True(AmigaModelCatalog.Get("CD32").HasCdDrive);
        Assert.Equal(1024, AmigaModelCatalog.Get("A600").ChipMemoryKib);
        Assert.Equal(4, AmigaModelCatalog.Get("A1000").MaximumFloppyDrives);
        Assert.All(AmigaModelCatalog.All, model =>
        {
            Assert.NotEmpty(model.CpuModels);
            Assert.Contains(model.Chipset, new[] { "OCS", "ECS", "AGA" });
            Assert.Equal(model.SupportsHardDrives, model.MaximumHardDrives > 0);
            Assert.InRange(model.MaximumFloppyDrives, 0, 4);
        });
    }

    [Fact]
    public void ExternalCoreInstaller_AcceptsTheInstalledWindowsX64Library()
    {
        var repository = FindRepositoryRoot();
        var source = Path.Combine(repository, "artifacts", "ppua", "puae_libretro.dll");
        var directory = Path.Combine(Path.GetTempPath(), "GWGUI-Amiga-Core", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            File.Copy(source, Path.Combine(directory, "puae_libretro.dll"));
            using var client = new HttpClient();
            var installer = new AmigaExternalCoreInstaller(client, directory);
            Assert.True(installer.IsInstalled);
            using (var stream = new FileStream(installer.LibraryPath, FileMode.Open, FileAccess.ReadWrite))
            {
                using var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, leaveOpen: true);
                stream.Position = 0x3c;
                var peOffset = reader.ReadInt32();
                stream.Position = peOffset + 4;
                stream.WriteByte(0x4c);
                stream.WriteByte(0x01);
            }
            Assert.False(installer.IsInstalled);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public async Task ExternalCoreInstaller_ExtractsAndValidatesOfficialPinnedArchive()
    {
        var repository = FindRepositoryRoot();
        var archive = await File.ReadAllBytesAsync(Path.Combine(repository, "artifacts", "ppua", "puae_libretro.dll.zip"));
        using var client = new HttpClient(new StaticDownloadHandler(archive));
        var directory = Path.Combine(Path.GetTempPath(), "GWGUI-Amiga-Core", Guid.NewGuid().ToString("N"));
        try
        {
            var installer = new AmigaExternalCoreInstaller(client, directory);
            var installed = await installer.InstallAsync();
            Assert.True(installer.IsInstalled);
            Assert.Equal(AmigaExternalCoreInstaller.LibrarySize, new FileInfo(installed).Length);
            Assert.Contains(AmigaExternalCoreInstaller.DownloadUrl,
                await File.ReadAllTextAsync(Path.Combine(directory, "core.json")), StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "GWGUI.sln"))) directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException("GWGUI repository root not found.");
    }

    private sealed class StaticDownloadHandler(byte[] content) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(content) });
    }
}
