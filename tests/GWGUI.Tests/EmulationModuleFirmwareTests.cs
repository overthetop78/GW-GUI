using GWGUI.Emulation;
using GWGUI.Emulation.Amiga;
using GWGUI.Emulation.Atari;
using GWGUI.MediaEngine.Exploration.Scp;
using System.IO;
using System.Net.Http;

namespace GWGUI.Tests;

public sealed class EmulationModuleFirmwareTests
{
    [Fact]
    public async Task AmigaModuleScansItsOwnFirmwareDirectory()
    {
        var root = NewRoot();
        try
        {
            var module = new AmigaEmulationModule(Path.Combine(root, "configurations"), root,
                new HttpClient(), Path.Combine(root, "cores"));
            var configuration = module.CreateConfiguration("A500");
            var directory = module.GetFirmwareDirectory("A500");
            Directory.CreateDirectory(directory);
            await File.WriteAllBytesAsync(Path.Combine(directory, "unknown.rom"), [1, 2, 3]);

            var result = await module.ScanFirmwareAsync("A500", configuration);

            Assert.Single(result);
            Assert.Equal(EmulationFirmwareCompatibility.Incompatible, result[0].Compatibility);
        }
        finally { DeleteRoot(root); }
    }

    [Fact]
    public async Task AtariModuleScansOnlyTheSelectedMachineFamilyDirectory()
    {
        var root = NewRoot();
        try
        {
            var module = new AtariEmulationModule(Path.Combine(root, "configurations"), root,
                new HttpClient(), Path.Combine(root, "cores"));
            var configuration = module.CreateConfiguration(nameof(AtariMachineModel.Atari400));
            var selectedDirectory = module.GetFirmwareDirectory(nameof(AtariMachineModel.Atari400));
            Directory.CreateDirectory(selectedDirectory);
            await File.WriteAllBytesAsync(Path.Combine(selectedDirectory, "candidate.rom"), [1]);
            var otherDirectory = module.GetFirmwareDirectory(nameof(AtariMachineModel.Lynx));
            Directory.CreateDirectory(otherDirectory);
            await File.WriteAllBytesAsync(Path.Combine(otherDirectory, "lynxboot.img"), [2]);

            var result = await module.ScanFirmwareAsync(nameof(AtariMachineModel.Atari400), configuration);

            Assert.Single(result);
            Assert.StartsWith(selectedDirectory, result[0].Path, StringComparison.OrdinalIgnoreCase);
        }
        finally { DeleteRoot(root); }
    }

    private static string NewRoot() => Path.Combine(Path.GetTempPath(), $"gwgui-module-firmware-{Guid.NewGuid():N}");

    private static void DeleteRoot(string root)
    {
        if (Directory.Exists(root)) Directory.Delete(root, true);
    }
}
