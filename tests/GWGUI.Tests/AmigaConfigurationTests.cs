using System.IO;
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
        var second = first with { Id = Guid.NewGuid(), Model = "A1200", InitialDiskPath = null, AudioEnabled = false };
        try
        {
            await store.SaveAsync(first);
            await store.SaveAsync(second);
            var loaded = await store.LoadAllAsync();
            Assert.Equal(2, loaded.Count);
            Assert.Contains(loaded, configuration => configuration.Id == first.Id && configuration.Model == "A500");
            Assert.Contains(loaded, configuration => configuration.Id == second.Id && !configuration.AudioEnabled);
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
}
