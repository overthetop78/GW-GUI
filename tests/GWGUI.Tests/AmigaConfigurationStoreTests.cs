using GWGUI.Emulation.Amiga;
using GWGUI.Emulation.Amiga.Cores;
using System.IO;

namespace GWGUI.Tests;

public sealed class AmigaConfigurationStoreTests
{
    [Fact]
    public void EmptyOptionalFirmwarePathsDoNotBlockCoreInitialization()
    {
        var root = Path.Combine(Path.GetTempPath(), "GWGUI-Amiga-Core", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var kickstart = Path.Combine(root, "kick.rom");
        File.WriteAllBytes(kickstart, [0]);
        try
        {
            using var core = new AmigaExternalCore(Path.Combine(root, "missing-core.dll"));
            var configuration = AmigaMachineConfiguration.A500(kickstart) with
            {
                ExtendedRomPath = string.Empty,
                RomKeyPath = "   "
            };

            var error = Assert.Throws<FileNotFoundException>(() =>
                core.Initialize(configuration, Path.Combine(root, "session")));

            Assert.Contains("AmigaCoreNotFound", error.Message, StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task EmptyOptionalFirmwarePathsAreStoredAndLoadedAsNull()
    {
        var root = Path.Combine(Path.GetTempPath(), "GWGUI-Amiga-Store", Guid.NewGuid().ToString("N"));
        try
        {
            var store = new AmigaConfigurationStore(root, root);
            var configuration = AmigaMachineConfiguration.A500(Path.Combine(root, "kick.rom")) with
            {
                ExtendedRomPath = string.Empty,
                RomKeyPath = "   "
            };

            await store.SaveAsync(configuration);
            var loaded = Assert.Single(await store.LoadAllAsync());

            Assert.Null(loaded.ExtendedRomPath);
            Assert.Null(loaded.RomKeyPath);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }
}
