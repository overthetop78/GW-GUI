using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Nintendo.Modules;
using GWGUI.Emulation.Sega.Modules;
using GWGUI.Emulation.Sony.Modules;
using System.Net.Http;

namespace GWGUI.Tests.Emulation.MachineAdapters;

internal static class ConsoleFamilyMediaScenarios
{
    public static void OpticalMediaRoundTrip()
    {
        using var http = new HttpClient();
        var contextRoot = Path.Combine(Path.GetTempPath(), "gwgui-console-media-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(contextRoot);
        try
        {
            var nintendo = new NintendoEmulationModuleFactory().Create(
                new EmulationModuleContext(contextRoot, contextRoot, http));
            var sega = new SegaEmulationModuleFactory().Create(
                new EmulationModuleContext(contextRoot, contextRoot, http));
            var sony = new SonyEmulationModuleFactory().Create(
                new EmulationModuleContext(contextRoot, contextRoot, http));

            AssertOpticalDevice(nintendo, "GameCube", "gamecube.iso");
            AssertOpticalDevice(sega, "Dreamcast", "dreamcast.gdi");
            AssertOpticalDevice(sony, "PlayStation", "playstation.cue");
        }
        finally
        {
            if (Directory.Exists(contextRoot)) Directory.Delete(contextRoot, true);
        }
    }

    public static void CoreOptionsRemainPersistedForConsoleFamilies()
    {
        using var http = new HttpClient();
        var root = Path.Combine(Path.GetTempPath(), "gwgui-console-options-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var context = new EmulationModuleContext(root, root, http);
            AssertOptionRoundTrip(new NintendoEmulationModuleFactory().Create(context), "Nes", "mesen.option", "enabled");
            AssertOptionRoundTrip(new SegaEmulationModuleFactory().Create(context), "Dreamcast", "flycast.option", "enabled");
            AssertOptionRoundTrip(new SonyEmulationModuleFactory().Create(context), "PlayStation", "swanstation.option", "enabled");
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    private static void AssertOpticalDevice(IEmulationModule module, string machineId, string path)
    {
        var configuration = module.CreateConfiguration(machineId);
        var storage = ((IEmulationStorageSettingsManager)module).DescribeStorageSettings(configuration);
        var device = Assert.Single(storage.AvailableDevices,
            item => item.Slot == EmulationMediaSlot.Cd0);
        Assert.Equal(EmulationMediaType.CompactDisc, device.MediaType);
        Assert.Contains(".cue", device.AcceptedExtensions, StringComparer.OrdinalIgnoreCase);
        Assert.Contains(EmulationMediaSlot.Cd0, storage.ConfiguredSlots);

        var mounted = storage with
        {
            ConfiguredSlots = [EmulationMediaSlot.Cd0],
            MountedMedia = [new EmulationMedia(path, EmulationMediaSlot.Cd0,
                EmulationMediaType.CompactDisc, true, true)]
        };
        var applied = ((IEmulationStorageSettingsManager)module)
            .ApplyStorageSettings(configuration, mounted);
        var appliedSettings = ((IEmulationStorageSettingsManager)module)
            .DescribeStorageSettings(applied);
        var media = Assert.Single(appliedSettings.MountedMedia);
        Assert.Equal(EmulationMediaType.CompactDisc, media.Type);
        Assert.Equal(EmulationMediaSlot.Cd0, media.Slot);
        Assert.Equal(Path.GetFullPath(path), media.Path);
    }

    private static void AssertOptionRoundTrip(IEmulationModule module,
        string machineId, string key, string value)
    {
        var configuration = module.CreateConfiguration(machineId);
        var changed = module.ApplySettings(configuration,
            new Dictionary<string, string?> { [key] = value });
        var options = ((IEmulationModule)module).RuntimeOptions(changed);
        Assert.Equal(value, options[key]);
    }
}
