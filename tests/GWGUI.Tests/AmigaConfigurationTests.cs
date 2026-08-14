using System.IO;
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
    public void FirmwareCatalog_FindsRomBinAndKeyWithHashes()
    {
        var directory = Path.Combine(Path.GetTempPath(), "GWGUI-Amiga-Firmware", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            File.WriteAllBytes(Path.Combine(directory, "kick.rom"), [1, 2, 3]);
            File.WriteAllBytes(Path.Combine(directory, "extended.bin"), [4, 5]);
            File.WriteAllBytes(Path.Combine(directory, "rom.key"), [6]);
            File.WriteAllText(Path.Combine(directory, "ignore.txt"), "ignored");
            var entries = new AmigaFirmwareCatalog(directory).Scan();
            Assert.Equal(3, entries.Count);
            Assert.All(entries, entry => Assert.Equal(64, entry.Sha256.Length));
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void ModelCatalog_ContainsEveryExternallySupportedPreset()
    {
        Assert.Equal(13, AmigaModelCatalog.All.Count);
        Assert.Equal("OCS", AmigaModelCatalog.Get("A500").Chipset);
        Assert.Equal("AGA", AmigaModelCatalog.Get("A1200").Chipset);
        Assert.True(AmigaModelCatalog.Get("CD32").HasCdDrive);
    }

    [Fact]
    public void ExternalCoreInstaller_OnlyAcceptsPinnedLibrary()
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
            using (var stream = new FileStream(installer.LibraryPath, FileMode.Append, FileAccess.Write)) stream.WriteByte(0);
            Assert.False(installer.IsInstalled);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "GWGUI.sln"))) directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException("GWGUI repository root not found.");
    }
}
