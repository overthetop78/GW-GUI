using System.Reflection;
using System.Net.Http;
using System.IO;
using System.Text.Json;
using System.Xml;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Constants;
using GWGUI.Emulation;
using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Nec.Modules;
using GWGUI.Emulation.Nintendo.Modules;
using NintendoEmulatorCatalog = GWGUI.Emulation.Nintendo.Common.Dictionaries.EmulatorCatalog;
using NintendoModelConstants = GWGUI.Emulation.Nintendo.Common.Machines.Common.Constants.ModelConstants;
using NintendoModelCatalog = GWGUI.Emulation.Nintendo.Common.Machines.Common.Dictionaries.ModelCatalog;
using NintendoSettingsConstants = GWGUI.Emulation.Nintendo.Common.Machines.Common.Constants.SettingsConstants;
using GWGUI.Emulation.Sega.Modules;
using GWGUI.Emulation.Sega.Common.Contracts;
using GWGUI.Emulation.Sega.Common.Dictionaries;
using GWGUI.Emulation.Sega.Common.Machines.Common.Constants;
using GWGUI.Emulation.Sega.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Sega.Common.Machines.Common.Dictionaries;
using GWGUI.Emulation.Sega.Common.Machines.Common.Enums;
using GWGUI.Emulation.Sega.Emulators.GenesisPlusGX.Constants;
using GWGUI.Emulation.Sega.Emulators.GenesisPlusGX.Contracts;
using GWGUI.Emulation.Sega.Emulators.GenesisPlusGX.Services;
using GWGUI.Emulation.Sega.Emulators.GenesisPlusGX.Functions;
using FlycastExternalCore = GWGUI.Emulation.Sega.Emulators.Flycast.Services.ExternalCore;
using FlycastExternalCoreConstants = GWGUI.Emulation.Sega.Emulators.Flycast.Constants.ExternalCoreConstants;
using YabauseExternalCore = GWGUI.Emulation.Sega.Emulators.Yabause.Services.ExternalCore;
using YabauseExternalCoreConstants = GWGUI.Emulation.Sega.Emulators.Yabause.Constants.ExternalCoreConstants;
using GWGUI.Emulation.Sony.Modules;
using SonyEmulatorCatalog = GWGUI.Emulation.Sony.Common.Dictionaries.EmulatorCatalog;
using SonyModelCatalog = GWGUI.Emulation.Sony.Common.Machines.Common.Dictionaries.ModelCatalog;
using GWGUI.Emulation.Microsoft.Modules;

namespace GWGUI.Tests.Architecture;

public sealed class ConsoleFamilyModuleTests
{
    [Fact]
    public void ConsoleFamilyModulesExposeTheSameCommonFileLayout()
    {
        var root = RepositoryRoot();
        var reference = CommonFiles(root, "Sega");
        foreach (var family in new[] { "Nintendo", "Sony", "Microsoft", "Nec" })
            Assert.Equal(reference, CommonFiles(root, family));
    }

    [Fact]
    public void ConsoleFamilyFactoriesCreateTheDeclaredModules()
    {
        var root = Path.Combine(Path.GetTempPath(), "gwgui-console-module-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using var http = new HttpClient();
            var context = new EmulationModuleContext(root, root, http);
            var modules = new IEmulationModule[]
            {
                new SegaEmulationModuleFactory().Create(context),
                new NintendoEmulationModuleFactory().Create(context),
                new SonyEmulationModuleFactory().Create(context),
                new MicrosoftEmulationModuleFactory().Create(context),
                new NecEmulationModuleFactory().Create(context)
            };
            Assert.Equal(new[] { "microsoft", "nec", "nintendo", "sega", "sony" },
                modules.Select(module => module.Id).Order(StringComparer.Ordinal));
            Assert.All(modules, module => Assert.NotEmpty(module.Machines));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void ConsoleFamiliesExposeOptionsWithoutAnInstalledAdapter()
    {
        var root = Path.Combine(Path.GetTempPath(), "gwgui-console-unsupported-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using var http = new HttpClient();
            var context = new EmulationModuleContext(root, root, http);
            var cases = new (IEmulationModule Module, string MachineId)[]
            {
                (new NintendoEmulationModuleFactory().Create(context), NintendoModelConstants.WiiU),
                (new NintendoEmulationModuleFactory().Create(context), NintendoModelConstants.Switch),
                (new SonyEmulationModuleFactory().Create(context), "PsVita"),
                (new SonyEmulationModuleFactory().Create(context), "PlayStation3"),
                (new SonyEmulationModuleFactory().Create(context), "PlayStation4"),
                (new SonyEmulationModuleFactory().Create(context), "PlayStation5"),
                (new MicrosoftEmulationModuleFactory().Create(context), "Xbox")
            };

            foreach (var (module, machineId) in cases)
            {
                var configuration = module.CreateConfiguration(machineId);
                Assert.Equal(machineId, configuration.MachineId);
                Assert.Equal(string.Empty, configuration.GetType()
                    .GetProperty("EmulatorId")?.GetValue(configuration));
            }
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void NintendoMesenSelectsNesAndFamicomDiskSystem()
    {
        var root = Path.Combine(Path.GetTempPath(), "gwgui-nintendo-adapter-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using var http = new HttpClient();
            var context = new EmulationModuleContext(root, root, http);
            var module = new NintendoEmulationModuleFactory().Create(context);
            var nes = Assert.IsType<GWGUI.Emulation.Nintendo.Common.Machines.Common.Contracts.MachineConfiguration>(
                module.CreateConfiguration("Nes"));
            var fds = Assert.IsType<GWGUI.Emulation.Nintendo.Common.Machines.Common.Contracts.MachineConfiguration>(
                module.CreateConfiguration("FamicomDisk"));
            Assert.Equal("mesen", nes.EmulatorId);
            Assert.Equal("mesen", fds.EmulatorId);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void NintendoModelCatalogContainsNintendoModelsOnly()
    {
        var expected = new[]
        {
            NintendoModelConstants.GameWatch, NintendoModelConstants.Nes,
            NintendoModelConstants.FamicomDisk, NintendoModelConstants.Snes,
            NintendoModelConstants.VirtualBoy, NintendoModelConstants.Nintendo64,
            NintendoModelConstants.GameBoy, NintendoModelConstants.GameBoyColor,
            NintendoModelConstants.GameBoyAdvance, NintendoModelConstants.NintendoDs,
            NintendoModelConstants.Nintendo3Ds, NintendoModelConstants.GameCube,
            NintendoModelConstants.Wii, NintendoModelConstants.WiiU,
            NintendoModelConstants.Switch
        };
        Assert.Equal(expected, NintendoModelCatalog.All.Select(model => model.Id));
        Assert.DoesNotContain(NintendoModelCatalog.All, model => model.BackendModel is
            "sg1000" or "mastersystem" or "megadrive" or "saturn" or "dreamcast");
        Assert.All(NintendoModelCatalog.All, model => Assert.False(string.IsNullOrWhiteSpace(model.BackendModel)));
    }

    [Fact]
    public void NintendoAdaptersPublishOnlyCatalogModels()
    {
        var modelIds = NintendoModelCatalog.All.Select(model => model.Id)
            .ToHashSet(StringComparer.Ordinal);
        Assert.All(NintendoEmulatorCatalog.All, definition =>
            Assert.All(definition.MachineIds, machineId => Assert.Contains(machineId, modelIds)));
    }

    [Fact]
    public void NintendoAdaptersCreateEveryPublishedModelConfiguration()
    {
        var root = Path.Combine(Path.GetTempPath(), "gwgui-nintendo-adapter-matrix-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using var http = new HttpClient();
            var module = new NintendoEmulationModuleFactory().Create(new EmulationModuleContext(root, root, http));
            foreach (var definition in NintendoEmulatorCatalog.All)
            foreach (var machineId in definition.MachineIds)
            {
                var configuration = Assert.IsType<GWGUI.Emulation.Nintendo.Common.Machines.Common.Contracts.MachineConfiguration>(
                    module.CreateConfiguration(machineId));
                Assert.Equal(definition.Id, configuration.EmulatorId);
            }
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void NintendoStorageAndHardwareSettingsMatchEachPublishedModel()
    {
        var root = Path.Combine(Path.GetTempPath(), "gwgui-nintendo-settings-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using var http = new HttpClient();
            var module = new NintendoEmulationModuleFactory().Create(new EmulationModuleContext(root, root, http));
            var supported = NintendoModelCatalog.All.Where(model => NintendoEmulatorCatalog.All
                .Any(definition => definition.MachineIds.Contains(model.Id, StringComparer.Ordinal)));
            foreach (var model in supported)
            {
                var configuration = Assert.IsType<GWGUI.Emulation.Nintendo.Common.Machines.Common.Contracts.MachineConfiguration>(
                    module.CreateConfiguration(model.Id));
                var storage = Assert.IsAssignableFrom<IEmulationStorageSettingsManager>(module)
                    .DescribeStorageSettings(configuration);
                var cartridge = storage.AvailableDevices.Where(device => device.Slot == EmulationMediaSlot.Cartridge0)
                    .Select(device => device.AcceptedExtensions).SingleOrDefault();
                var optical = storage.AvailableDevices.Where(device => device.Slot == EmulationMediaSlot.Cd0)
                    .Select(device => device.AcceptedExtensions).SingleOrDefault();
                if (model.Id == NintendoModelConstants.FamicomDisk)
                {
                    var floppy = Assert.Single(storage.AvailableDevices,
                        device => device.Slot == EmulationMediaSlot.Floppy0);
                    Assert.Equal([".fds", ".m3u"], floppy.AcceptedExtensions);
                    Assert.DoesNotContain(storage.AvailableDevices,
                        device => device.AcceptedExtensions.Contains(".dsk"));
                }
                else if (model.Id is NintendoModelConstants.GameCube or NintendoModelConstants.Wii)
                    Assert.Equal([".cue", ".chd", ".iso", ".gcm"], optical);
                else
                    Assert.Null(optical);
                if (model.SupportsCartridgeSlot)
                    Assert.NotNull(cartridge);
                else
                    Assert.Null(cartridge);

                var fields = module.Describe(model.Id, configuration).Blocks.SelectMany(block => block.Fields).ToArray();
                Assert.DoesNotContain(fields, field => field.Id is "configuration.videoResolution"
                    or "configuration.videoMonitor" or "configuration.videoIntensity"
                    or "configuration.videoCrop" or "configuration.floppySound");
                Assert.Equal(EmulationSettingsEditor.Information,
                    fields.Single(field => field.Id == NintendoSettingsConstants.Ram).Editor);
                Assert.Equal($"{model.RamKib} KiB",
                    fields.Single(field => field.Id == NintendoSettingsConstants.Ram).Value);
                Assert.Equal(string.Join(" / ", model.Processors),
                    fields.Single(field => field.Id == NintendoSettingsConstants.Model + ".cpu").Value);
                Assert.Equal(model.VideoChip,
                    fields.Single(field => field.Id == NintendoSettingsConstants.Model + ".video").Value);
                Assert.Equal(model.AudioChip,
                    fields.Single(field => field.Id == NintendoSettingsConstants.Model + ".audio").Value);
            }
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void NintendoResourceCatalogsMatchPublishedSettings()
    {
        var root = RepositoryRoot();
        var resources = Path.Combine(root, "src", "GWGUI.Emulation.Nintendo", "Resources");
        var obsolete = new HashSet<string>(StringComparer.Ordinal)
        {
            "Emulation.Nintendo.Video.Monitor",
            "Emulation.Nintendo.Video.Intensity",
            "Emulation.Nintendo.Video.Monitor.Color",
            "Emulation.Nintendo.Video.Monitor.Green",
            "Emulation.Nintendo.Video.Monitor.White",
            "Emulation.Nintendo.Help.Video.Resolution.Short",
            "Emulation.Nintendo.Help.Video.Resolution.Detailed",
            "Emulation.Nintendo.Help.Video.Monitor.Short",
            "Emulation.Nintendo.Help.Video.Monitor.Detailed",
            "Emulation.Nintendo.Help.Video.Intensity.Short",
            "Emulation.Nintendo.Help.Video.Intensity.Detailed",
            "Emulation.Nintendo.Help.Video.Crop.Short",
            "Emulation.Nintendo.Help.Video.Crop.Detailed",
            "Emulation.Nintendo.Help.Audio.FloppySound.Short",
            "Emulation.Nintendo.Help.Audio.FloppySound.Detailed"
        };
        var baseEntries = ResxEntries(Path.Combine(resources, "00-Base", "Emulation.resx"));
        var englishEntries = ResxEntries(Path.Combine(resources, "en-US", "Emulation.resx"));
        Assert.DoesNotContain(baseEntries.Keys, key => obsolete.Contains(key));
        Assert.DoesNotContain(englishEntries.Keys, key => obsolete.Contains(key));
        Assert.Equal("System ROM", baseEntries["Emulation.Nintendo.Firmware.Integrated"]);
        Assert.Equal("System ROM", englishEntries["Emulation.Nintendo.Firmware.Integrated"]);
        Assert.Equal("Displays the RAM provided by the selected Nintendo machine.",
            englishEntries["Emulation.Nintendo.Help.Memory.Ram.Detailed"]);
        Assert.DoesNotContain("Mesen", englishEntries["Emulation.Nintendo.Firmware.Integrated"],
            StringComparison.Ordinal);
        Assert.DoesNotContain("Mesen", englishEntries["Emulation.Nintendo.Help.Memory.Ram.Detailed"],
            StringComparison.Ordinal);
        Assert.DoesNotContain("Mesen", englishEntries["Emulation.Nintendo.Help.Firmware.Integrated.Short"],
            StringComparison.Ordinal);
        Assert.DoesNotContain("Mesen", englishEntries["Emulation.Nintendo.Help.Firmware.Integrated.Detailed"],
            StringComparison.Ordinal);
        var forbiddenFamilyNames = new[] { "Caprice32", "Caprice 32", "Amstrad", "GenesisPlusGX", "Sega" };
        Assert.DoesNotContain(baseEntries.Values, value => forbiddenFamilyNames.Any(name =>
            value.Contains(name, StringComparison.OrdinalIgnoreCase)));
        Assert.DoesNotContain(englishEntries.Values, value => forbiddenFamilyNames.Any(name =>
            value.Contains(name, StringComparison.OrdinalIgnoreCase)));
        foreach (var culture in Directory.EnumerateDirectories(resources)
                     .Where(path => !Path.GetFileName(path).Equals("00-Base", StringComparison.Ordinal)
                         && !Path.GetFileName(path).Equals("en-US", StringComparison.Ordinal)))
        {
            var entries = ResxEntries(Path.Combine(culture, "Emulation.resx"));
            Assert.DoesNotContain(entries.Keys, key => obsolete.Contains(key));
            Assert.DoesNotContain("Mesen", entries["Emulation.Nintendo.Firmware.Integrated"],
                StringComparison.Ordinal);
            Assert.Equal(englishEntries.Keys.Order(StringComparer.Ordinal), entries.Keys.Order(StringComparer.Ordinal));
            Assert.DoesNotContain(entries.Values, value => forbiddenFamilyNames.Any(name =>
                value.Contains(name, StringComparison.OrdinalIgnoreCase)));
        }

        static IReadOnlyDictionary<string, string> ResxEntries(string path)
        {
            var document = new XmlDocument();
            document.Load(path);
            return document.SelectNodes("/root/data")!.Cast<XmlElement>()
                .ToDictionary(element => element.GetAttribute("name"),
                    element => element.SelectSingleNode("value")?.InnerText ?? string.Empty,
                    StringComparer.Ordinal);
        }
    }

    [Fact]
    public void SonyResourceCatalogsUseCommonCategoriesAndOnlySonyText()
    {
        var root = RepositoryRoot();
        var resources = Path.Combine(root, "src", "GWGUI.Emulation.Sony", "Resources");
        var baseEntries = ReadEntries(Path.Combine(resources, "00-Base"));
        var englishEntries = ReadEntries(Path.Combine(resources, "en-US"));
        var expectedModels = new[] { "PlayStation", "PlayStation2", "Psp", "PsVita", "PlayStation3", "PlayStation4", "PlayStation5" };
        Assert.Contains("Emulation.Family.Sony", baseEntries.Keys);
        Assert.All(expectedModels, model => Assert.Contains("Emulation.Sony.Model." + model, baseEntries.Keys));
        Assert.DoesNotContain(englishEntries.Keys, key => key == "Emulation.Family.Sony"
            || key.StartsWith("Emulation.Sony.Model.", StringComparison.Ordinal));
        Assert.Contains("Emulation.Error.Pcsx2.HostConfigurationInvalid", englishEntries.Keys);
        Assert.Contains("Emulation.Emulator.pcsx2.Description", englishEntries.Keys);
        Assert.DoesNotContain(baseEntries.Keys, key => key.StartsWith("Emulation.Error.", StringComparison.Ordinal)
            || key.StartsWith("Emulation.Emulator.", StringComparison.Ordinal));
        var forbidden = new[] { "Caprice32", "Amstrad", "GenesisPlusGX", "Sega", "Nintendo", "SonyCore" };
        Assert.DoesNotContain(englishEntries.Values, value => forbidden.Any(name =>
            value.Contains(name, StringComparison.OrdinalIgnoreCase)));
        var files = Directory.EnumerateFiles(Path.Combine(resources, "en-US"), "*.resx")
            .Select(Path.GetFileName).Order(StringComparer.Ordinal).ToArray();
        foreach (var culture in Directory.EnumerateDirectories(resources)
                     .Where(path => !Path.GetFileName(path).Equals("00-Base", StringComparison.Ordinal)
                         && !Path.GetFileName(path).Equals("en-US", StringComparison.Ordinal)))
        {
            Assert.False(File.Exists(Path.Combine(culture, "Emulation.resx")));
            Assert.Equal(files, Directory.EnumerateFiles(culture, "*.resx")
                .Select(Path.GetFileName).Order(StringComparer.Ordinal));
            var entries = ReadEntries(culture);
            Assert.Equal(englishEntries.Keys.Order(StringComparer.Ordinal), entries.Keys.Order(StringComparer.Ordinal));
            Assert.DoesNotContain(entries.Values, value => forbidden.Any(name =>
                value.Contains(name, StringComparison.OrdinalIgnoreCase)));
        }

        static IReadOnlyDictionary<string, string> ReadEntries(string directory)
        {
            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var path in Directory.EnumerateFiles(directory, "*.resx"))
            {
                var document = new XmlDocument();
                document.Load(path);
                foreach (var element in document.SelectNodes("/root/data")!.Cast<XmlElement>())
                    result.Add(element.GetAttribute("name"), element.SelectSingleNode("value")?.InnerText ?? string.Empty);
            }
            return result;
        }
    }

    [Fact]
    public void MicrosoftCatalogContainsOnlyXboxHardwareWithFixedComponents()
    {
        var models = GWGUI.Emulation.Microsoft.Common.Machines.Common.Dictionaries.ModelCatalog.All;
        Assert.Equal(["Xbox", "Xbox360"], models.Select(model => model.Id));
        var xbox = Assert.Single(models, model => model.Id == "Xbox");
        Assert.Equal(64 * 1024, xbox.RamKib);
        Assert.Equal(["Intel Pentium III Coppermine"], xbox.Processors);
        Assert.Equal("NVIDIA NV2A", xbox.VideoChip);
        Assert.Equal("NVIDIA MCPX / AC'97", xbox.AudioChip);
        Assert.True(xbox.HasBuiltInCompactDiscDrive);
        Assert.True(xbox.SupportsCompactDiscDrive);
        var xbox360 = Assert.Single(models, model => model.Id == "Xbox360");
        Assert.Equal(512 * 1024, xbox360.RamKib);
        Assert.False(xbox360.SupportsCompactDiscDrive);
        Assert.DoesNotContain(models, model => model.BackendModel is
            "sg1000" or "mastersystem" or "megadrive" or "saturn" or "dreamcast");
    }

    [Fact]
    public void MicrosoftStorageAndSettingsExposeOnlySupportedHardware()
    {
        var root = Path.Combine(Path.GetTempPath(), "gwgui-microsoft-settings-tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using var http = new HttpClient();
            var module = new MicrosoftEmulationModuleFactory().Create(
                new EmulationModuleContext(root, root, http));
            foreach (var model in GWGUI.Emulation.Microsoft.Common.Machines.Common.Dictionaries.ModelCatalog.All)
            {
                var configuration = new GWGUI.Emulation.Microsoft.Common.Machines.Common.Contracts.MachineConfiguration(
                    model.Id, "unavailable", Media: []);
                var storage = Assert.IsAssignableFrom<IEmulationStorageSettingsManager>(module)
                    .DescribeStorageSettings(configuration);
                if (model.Id == "Xbox")
                {
                    var optical = Assert.Single(storage.AvailableDevices,
                        device => device.Slot == EmulationMediaSlot.Cd0);
                    Assert.Equal([".iso"], optical.AcceptedExtensions);
                }
                else
                    Assert.Empty(storage.AvailableDevices);
                var fields = module.Describe(model.Id, configuration).Blocks
                    .SelectMany(block => block.Fields).ToArray();
                Assert.DoesNotContain(fields, field => field.Id is "configuration.videoResolution"
                    or "configuration.videoMonitor" or "configuration.videoIntensity"
                    or "configuration.videoCrop" or "configuration.floppySound");
                Assert.Equal(EmulationSettingsEditor.Information,
                    fields.Single(field => field.Id == "configuration.ramKib").Editor);
                Assert.Equal($"{model.RamKib} KiB",
                    fields.Single(field => field.Id == "configuration.ramKib").Value);
                Assert.Equal(string.Join(" / ", model.Processors),
                    fields.Single(field => field.Id == "configuration.model.cpu").Value);
                Assert.Equal(model.VideoChip,
                    fields.Single(field => field.Id == "configuration.model.video").Value);
                Assert.Equal(model.AudioChip,
                    fields.Single(field => field.Id == "configuration.model.audio").Value);
            }
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void MicrosoftResourceCatalogUsesCommonCategoriesAndNoUninstalledCoreText()
    {
        var root = RepositoryRoot();
        var resources = Path.Combine(root, "src", "GWGUI.Emulation.Microsoft", "Resources");
        var baseEntries = ReadEntries(Path.Combine(resources, "00-Base"));
        var englishEntries = ReadEntries(Path.Combine(resources, "en-US"));
        Assert.Contains("Emulation.Family.Microsoft", baseEntries.Keys);
        Assert.Contains("Emulation.Microsoft.Model.Xbox", baseEntries.Keys);
        Assert.Contains("Emulation.Microsoft.Model.Xbox360", baseEntries.Keys);
        Assert.DoesNotContain(englishEntries.Keys, key => key == "Emulation.Family.Microsoft"
            || key.StartsWith("Emulation.Microsoft.Model.", StringComparison.Ordinal));
        Assert.DoesNotContain(englishEntries.Keys, key => key.Contains("MicrosoftCore",
            StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(englishEntries.Values, value => value.Contains("MicrosoftCore",
            StringComparison.OrdinalIgnoreCase));
        var files = Directory.EnumerateFiles(Path.Combine(resources, "en-US"), "*.resx")
            .Select(Path.GetFileName).Order(StringComparer.Ordinal).ToArray();
        foreach (var culture in Directory.EnumerateDirectories(resources)
                     .Where(path => !Path.GetFileName(path).Equals("00-Base", StringComparison.Ordinal)
                         && !Path.GetFileName(path).Equals("en-US", StringComparison.Ordinal)))
        {
            Assert.False(File.Exists(Path.Combine(culture, "Emulation.resx")));
            Assert.Equal(files, Directory.EnumerateFiles(culture, "*.resx")
                .Select(Path.GetFileName).Order(StringComparer.Ordinal));
            var entries = ReadEntries(culture);
            Assert.Equal(englishEntries.Keys.Order(StringComparer.Ordinal),
                entries.Keys.Order(StringComparer.Ordinal));
            Assert.DoesNotContain(entries.Values, value => value.Contains("MicrosoftCore",
                StringComparison.OrdinalIgnoreCase));
        }

        static IReadOnlyDictionary<string, string> ReadEntries(string directory)
        {
            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var path in Directory.EnumerateFiles(directory, "*.resx"))
            {
                var document = new XmlDocument();
                document.Load(path);
                foreach (var element in document.SelectNodes("/root/data")!.Cast<XmlElement>())
                    result.Add(element.GetAttribute("name"),
                        element.SelectSingleNode("value")?.InnerText ?? string.Empty);
            }
            return result;
        }
    }

    [Fact]
    public void NintendoCoreOptionAdaptersForwardOnlyPersistedValues()
    {
        var input = new GWGUI.Emulation.Nintendo.Common.Machines.Common.Contracts.MachineConfiguration(
            "Nes", "mesen", new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["core_option"] = "selected"
            });
        var adapters = new Func<
            GWGUI.Emulation.Nintendo.Common.Machines.Common.Contracts.MachineConfiguration,
            GWGUI.Emulation.Nintendo.Common.Machines.Common.Contracts.MachineConfiguration>[]
        {
            GWGUI.Emulation.Nintendo.Emulators.BeetleVb.Functions.BeetleVbOptionFunctions.ToNative,
            GWGUI.Emulation.Nintendo.Emulators.Citra.Functions.CitraOptionFunctions.ToNative,
            GWGUI.Emulation.Nintendo.Emulators.Dolphin.Functions.DolphinOptionFunctions.ToNative,
            GWGUI.Emulation.Nintendo.Emulators.Gambatte.Functions.GambatteOptionFunctions.ToNative,
            GWGUI.Emulation.Nintendo.Emulators.GameWatch.Functions.GameWatchOptionFunctions.ToNative,
            GWGUI.Emulation.Nintendo.Emulators.MelonDs.Functions.MelonDsOptionFunctions.ToNative,
            GWGUI.Emulation.Nintendo.Emulators.Mesen.Functions.MesenOptionFunctions.ToNative,
            GWGUI.Emulation.Nintendo.Emulators.Mgba.Functions.MgbaOptionFunctions.ToNative,
            GWGUI.Emulation.Nintendo.Emulators.Mupen64PlusNext.Functions.Mupen64PlusNextOptionFunctions.ToNative,
            GWGUI.Emulation.Nintendo.Emulators.Snes9x.Functions.Snes9xOptionFunctions.ToNative
        };
        foreach (var adapter in adapters)
        {
            var native = adapter(input);
            Assert.Equal(input.Options, native.Options);
        }
    }

    [Fact]
    public void NintendoBeetleVbSelectsVirtualBoy()
    {
        var root = Path.Combine(Path.GetTempPath(), "gwgui-nintendo-beetlevb-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using var http = new HttpClient();
            var context = new EmulationModuleContext(root, root, http);
            var module = new NintendoEmulationModuleFactory().Create(context);
            var configuration = Assert.IsType<GWGUI.Emulation.Nintendo.Common.Machines.Common.Contracts.MachineConfiguration>(
                module.CreateConfiguration("VirtualBoy"));
            Assert.Equal("beetle_vb", configuration.EmulatorId);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void NintendoGameWatchSelectsGameWatchCore()
    {
        var root = Path.Combine(Path.GetTempPath(), "gwgui-nintendo-gamewatch-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using var http = new HttpClient();
            var context = new EmulationModuleContext(root, root, http);
            var module = new NintendoEmulationModuleFactory().Create(context);
            var configuration = Assert.IsType<GWGUI.Emulation.Nintendo.Common.Machines.Common.Contracts.MachineConfiguration>(
                module.CreateConfiguration("GameWatch"));
            Assert.Equal("gw", configuration.EmulatorId);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void NintendoCitraSelectsNintendo3Ds()
    {
        var root = Path.Combine(Path.GetTempPath(), "gwgui-nintendo-citra-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using var http = new HttpClient();
            var context = new EmulationModuleContext(root, root, http);
            var module = new NintendoEmulationModuleFactory().Create(context);
            var configuration = Assert.IsType<GWGUI.Emulation.Nintendo.Common.Machines.Common.Contracts.MachineConfiguration>(
                module.CreateConfiguration("Nintendo3Ds"));
            Assert.Equal("citra", configuration.EmulatorId);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void NintendoDolphinSelectsGameCubeAndWii()
    {
        var root = Path.Combine(Path.GetTempPath(), "gwgui-nintendo-dolphin-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using var http = new HttpClient();
            var context = new EmulationModuleContext(root, root, http);
            var module = new NintendoEmulationModuleFactory().Create(context);
            foreach (var machineId in new[] { "GameCube", "Wii" })
            {
                var configuration = Assert.IsType<GWGUI.Emulation.Nintendo.Common.Machines.Common.Contracts.MachineConfiguration>(
                    module.CreateConfiguration(machineId));
                Assert.Equal("dolphin", configuration.EmulatorId);
            }
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void NecBeetlePceFastSelectsPcEngineModels()
    {
        var root = Path.Combine(Path.GetTempPath(), "gwgui-nec-adapter-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using var http = new HttpClient();
            var context = new EmulationModuleContext(root, root, http);
            var module = new NecEmulationModuleFactory().Create(context);
            foreach (var machineId in new[] { "PcEngine", "CoreGrafx", "SuperGrafx", "PcEngineDuo", "TurboExpress" })
            {
                var configuration = Assert.IsType<GWGUI.Emulation.Nec.Common.Machines.Common.Contracts.MachineConfiguration>(
                    module.CreateConfiguration(machineId));
                Assert.Equal(machineId == "SuperGrafx" ? "beetle_sgx" : "beetle_pce_fast", configuration.EmulatorId);
            }
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void NintendoSnes9xSelectsSuperNintendo()
    {
        var root = Path.Combine(Path.GetTempPath(), "gwgui-nintendo-snes9x-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using var http = new HttpClient();
            var context = new EmulationModuleContext(root, root, http);
            var module = new NintendoEmulationModuleFactory().Create(context);
            var snes = Assert.IsType<GWGUI.Emulation.Nintendo.Common.Machines.Common.Contracts.MachineConfiguration>(
                module.CreateConfiguration("Snes"));
            Assert.Equal("snes9x", snes.EmulatorId);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void SonySwanStationSelectsPlayStation()
    {
        var root = Path.Combine(Path.GetTempPath(), "gwgui-sony-swanstation-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using var http = new HttpClient();
            var context = new EmulationModuleContext(root, root, http);
            var module = new SonyEmulationModuleFactory().Create(context);
            var playStation = Assert.IsType<GWGUI.Emulation.Sony.Common.Machines.Common.Contracts.MachineConfiguration>(
                module.CreateConfiguration("PlayStation"));
            Assert.Equal("swanstation", playStation.EmulatorId);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void NintendoGambatteSelectsGameBoyModels()
    {
        var root = Path.Combine(Path.GetTempPath(), "gwgui-nintendo-gambatte-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using var http = new HttpClient();
            var context = new EmulationModuleContext(root, root, http);
            var module = new NintendoEmulationModuleFactory().Create(context);
            foreach (var machineId in new[] { "GameBoy", "GameBoyColor" })
            {
                var configuration = Assert.IsType<GWGUI.Emulation.Nintendo.Common.Machines.Common.Contracts.MachineConfiguration>(
                    module.CreateConfiguration(machineId));
                Assert.Equal("gambatte", configuration.EmulatorId);
            }
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void NintendoMgbaSelectsGameBoyAdvance()
    {
        var root = Path.Combine(Path.GetTempPath(), "gwgui-nintendo-mgba-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using var http = new HttpClient();
            var context = new EmulationModuleContext(root, root, http);
            var module = new NintendoEmulationModuleFactory().Create(context);
            var configuration = Assert.IsType<GWGUI.Emulation.Nintendo.Common.Machines.Common.Contracts.MachineConfiguration>(
                module.CreateConfiguration("GameBoyAdvance"));
            Assert.Equal("mgba", configuration.EmulatorId);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void NintendoMupen64PlusNextSelectsNintendo64()
    {
        var root = Path.Combine(Path.GetTempPath(), "gwgui-nintendo-mupen64-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using var http = new HttpClient();
            var context = new EmulationModuleContext(root, root, http);
            var module = new NintendoEmulationModuleFactory().Create(context);
            var configuration = Assert.IsType<GWGUI.Emulation.Nintendo.Common.Machines.Common.Contracts.MachineConfiguration>(
                module.CreateConfiguration("Nintendo64"));
            Assert.Equal("mupen64plus-next", configuration.EmulatorId);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void NintendoMelonDsSelectsNintendoDs()
    {
        var root = Path.Combine(Path.GetTempPath(), "gwgui-nintendo-melonds-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using var http = new HttpClient();
            var context = new EmulationModuleContext(root, root, http);
            var module = new NintendoEmulationModuleFactory().Create(context);
            var configuration = Assert.IsType<GWGUI.Emulation.Nintendo.Common.Machines.Common.Contracts.MachineConfiguration>(
                module.CreateConfiguration("NintendoDs"));
            Assert.Equal("melonds", configuration.EmulatorId);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void SonyPcsx2SelectsPlayStation2()
    {
        var root = Path.Combine(Path.GetTempPath(), "gwgui-sony-pcsx2-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using var http = new HttpClient();
            var context = new EmulationModuleContext(root, root, http);
            var module = new SonyEmulationModuleFactory().Create(context);
            var configuration = Assert.IsType<GWGUI.Emulation.Sony.Common.Machines.Common.Contracts.MachineConfiguration>(
                module.CreateConfiguration("PlayStation2"));
            Assert.Equal("pcsx2", configuration.EmulatorId);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void SonyPpssppSelectsPortable()
    {
        var root = Path.Combine(Path.GetTempPath(), "gwgui-sony-ppsspp-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using var http = new HttpClient();
            var context = new EmulationModuleContext(root, root, http);
            var module = new SonyEmulationModuleFactory().Create(context);
            var configuration = Assert.IsType<GWGUI.Emulation.Sony.Common.Machines.Common.Contracts.MachineConfiguration>(
                module.CreateConfiguration("Psp"));
            Assert.Equal("ppsspp", configuration.EmulatorId);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void SonyModelsAndStorageMatchPublishedHardware()
    {
        var root = Path.Combine(Path.GetTempPath(), "gwgui-sony-settings-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using var http = new HttpClient();
            var module = new SonyEmulationModuleFactory().Create(
                new EmulationModuleContext(root, root, http));
            var expected = new[] { "PlayStation", "PlayStation2", "Psp", "PsVita", "PlayStation3", "PlayStation4", "PlayStation5" };
            Assert.Equal(expected, SonyModelCatalog.All.Select(model => model.Id));
            Assert.DoesNotContain(SonyModelCatalog.All, model => model.BackendModel is
                "sg1000" or "mastersystem" or "megadrive" or "saturn" or "dreamcast");
            var supported = SonyModelCatalog.All.Where(model => SonyEmulatorCatalog.All
                .Any(definition => definition.MachineIds.Contains(model.Id, StringComparer.Ordinal)));
            foreach (var model in supported)
            {
                var configuration = Assert.IsType<GWGUI.Emulation.Sony.Common.Machines.Common.Contracts.MachineConfiguration>(
                    module.CreateConfiguration(model.Id));
                var storage = Assert.IsAssignableFrom<IEmulationStorageSettingsManager>(module)
                    .DescribeStorageSettings(configuration);
                var optical = Assert.Single(storage.AvailableDevices,
                    device => device.Slot == EmulationMediaSlot.Cd0);
                Assert.Contains(EmulationMediaSlot.Cd0, storage.ConfiguredSlots);
                Assert.DoesNotContain(storage.AvailableDevices,
                    device => device.Slot == EmulationMediaSlot.Cartridge0);
                if (model.Id == "Psp")
                    Assert.Equal([".iso", ".chd"], optical.AcceptedExtensions);
                else
                    Assert.Equal([".cue", ".bin", ".chd", ".iso", ".ccd", ".mds"],
                        optical.AcceptedExtensions);

                var fields = module.Describe(model.Id, configuration).Blocks.SelectMany(block => block.Fields).ToArray();
                Assert.DoesNotContain(fields, field => field.Id is "configuration.videoResolution"
                    or "configuration.videoMonitor" or "configuration.videoIntensity"
                    or "configuration.videoCrop" or "configuration.floppySound");
                Assert.Equal(EmulationSettingsEditor.Information,
                    fields.Single(field => field.Id == "configuration.ramKib").Editor);
                Assert.Equal($"{model.RamKib} KiB",
                    fields.Single(field => field.Id == "configuration.ramKib").Value);
                Assert.Equal(string.Join(" / ", model.Processors),
                    fields.Single(field => field.Id == "configuration.model.cpu").Value);
                Assert.Equal(model.VideoChip,
                    fields.Single(field => field.Id == "configuration.model.video").Value);
                Assert.Equal(model.AudioChip,
                    fields.Single(field => field.Id == "configuration.model.audio").Value);
                Assert.Equal(model.CpuFrequency,
                    fields.Single(field => field.Id == "configuration.model.frequency").Value);
            }
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void NecBeetlePcfxSelectsPcFx()
    {
        var root = Path.Combine(Path.GetTempPath(), "gwgui-nec-beetle-pcfx-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using var http = new HttpClient();
            var context = new EmulationModuleContext(root, root, http);
            var module = new NecEmulationModuleFactory().Create(context);
            var configuration = Assert.IsType<GWGUI.Emulation.Nec.Common.Machines.Common.Contracts.MachineConfiguration>(
                module.CreateConfiguration("PcFx"));
            Assert.Equal("beetle_pcfx_fast", configuration.EmulatorId);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void SegaFlycastSelectsDreamcast()
    {
        var root = Path.Combine(Path.GetTempPath(), "gwgui-sega-flycast-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using var http = new HttpClient();
            var context = new EmulationModuleContext(root, root, http);
            var module = new SegaEmulationModuleFactory().Create(context);
            var configuration = Assert.IsType<GWGUI.Emulation.Sega.Common.Machines.Common.Contracts.MachineConfiguration>(
                module.CreateConfiguration("Dreamcast"));
            Assert.Equal("flycast", configuration.EmulatorId);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void SegaFlycastSelectsNaomiModels()
    {
        var root = Path.Combine(Path.GetTempPath(), "gwgui-sega-flycast-naomi-tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using var http = new HttpClient();
            var module = new SegaEmulationModuleFactory().Create(new EmulationModuleContext(root, root, http));
            foreach (var model in new[] { ModelConstants.Naomi, ModelConstants.Naomi2,
                ModelConstants.Atomiswave })
            {
                var configuration = Assert.IsType<MachineConfiguration>(module.CreateConfiguration(model));
                Assert.Equal("flycast", configuration.EmulatorId);
                Assert.Equal(model == ModelConstants.Atomiswave ? ModelConstants.Ram16384Kib : ModelConstants.Ram32768Kib,
                    ModelCatalog.Get(model).RamKib);
            }
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void SegaAdaptersSelectTheMatchingCoreForSaturnAndSc3000()
    {
        var root = Path.Combine(Path.GetTempPath(), "gwgui-sega-adapter-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using var http = new HttpClient();
            var module = new SegaEmulationModule(root, root, http, Path.Combine(root, "Core"));
            var saturn = Assert.IsType<MachineConfiguration>(module.CreateConfiguration("Saturn"));
            var sc3000 = Assert.IsType<MachineConfiguration>(module.CreateConfiguration("Sc3000"));
            Assert.Equal("yabause", saturn.EmulatorId);
            Assert.Equal("genesisplusgx", sc3000.EmulatorId);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void SegaPicoDrivePublishesVerifiedModelsWithoutInventingAddonMachines()
    {
        var definition = Assert.Single(EmulatorCatalog.All,
            item => item.Id.Equals("picodrive", StringComparison.Ordinal));
        Assert.Equal([ModelConstants.GameGear, ModelConstants.MasterSystem, ModelConstants.MegaDrive,
                ModelConstants.Sc3000, ModelConstants.Sg1000],
            definition.MachineIds.Order(StringComparer.Ordinal));
        Assert.DoesNotContain(ModelCatalog.All,
            model => model.Id is "MegaCd" or "ThirtyTwoX");
    }

    [Fact]
    public void SegaMasterSystemExposesItsCartridgeAndSegaCardSlots()
    {
        var root = Path.Combine(Path.GetTempPath(), "gwgui-sega-storage-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using var http = new HttpClient();
            var module = new SegaEmulationModuleFactory().Create(new EmulationModuleContext(root, root, http));
            var configuration = Assert.IsType<MachineConfiguration>(module.CreateConfiguration("MasterSystem"));
            var storage = Assert.IsAssignableFrom<IEmulationStorageSettingsManager>(module)
                .DescribeStorageSettings(configuration);
            var cartridge = Assert.Single(storage.AvailableDevices,
                device => device.Slot == EmulationMediaSlot.Cartridge0);
            var segaCard = Assert.Single(storage.AvailableDevices,
                device => device.Slot == EmulationMediaSlot.Cartridge1);
            Assert.Contains(".sms", cartridge.AcceptedExtensions);
            Assert.Contains(".mv", segaCard.AcceptedExtensions);
            Assert.Equal(EmulationStorageConfigurationKind.CartridgeSlot, segaCard.ConfigurationKind);
            Assert.Equal("Emulation.Sega.MasterSystem.ThreeDGlasses",
                segaCard.ConfigurationOptionResourceKey);
            Assert.Contains(EmulationMediaSlot.Cartridge0, storage.ConfiguredSlots);
            Assert.Contains(EmulationMediaSlot.Cartridge1, storage.ConfiguredSlots);
            Assert.False(Assert.Single(storage.DeviceSettings!,
                item => item.Slot == EmulationMediaSlot.Cartridge1).Cartridge!.OptionEnabled);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void SegaMasterSystemKeepsRamInformativeAndMovesThreeDGlassesToSegaCard()
    {
        var root = Path.Combine(Path.GetTempPath(), "gwgui-sega-master-system-settings-tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using var http = new HttpClient();
            var module = new SegaEmulationModuleFactory().Create(new EmulationModuleContext(root, root, http));
            var configuration = Assert.IsType<MachineConfiguration>(module.CreateConfiguration(ModelConstants.MasterSystem));
            var fields = module.Describe(ModelConstants.MasterSystem, configuration).Blocks
                .SelectMany(block => block.Fields).ToDictionary(field => field.Id, StringComparer.Ordinal);
            Assert.Equal(EmulationSettingsEditor.Information, fields[SettingsConstants.Ram].Editor);
            Assert.Equal(EmulationSettingsEditor.Information, fields[SettingsConstants.VideoChip].Editor);
            Assert.Equal(ModelConstants.VideoSega3155246, fields[SettingsConstants.VideoChip].Value);
            Assert.Equal(EmulationSettingsEditor.Information, fields[SettingsConstants.AudioChip].Editor);
            Assert.Equal(ModelConstants.AudioSn76489, fields[SettingsConstants.AudioChip].Value);
            Assert.DoesNotContain(fields, field => field.Key is "configuration.videoResolution"
                or "configuration.videoMonitor" or "configuration.videoIntensity"
                or "configuration.videoCrop" or "configuration.floppySound");
            Assert.DoesNotContain(fields, field => field.Key == SettingsConstants.MasterSystemThreeDGlasses);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void SegaMarkThreeSegaCardDoesNotExposeMasterSystemOnlyOptions()
    {
        var root = Path.Combine(Path.GetTempPath(), "gwgui-sega-mark-three-storage-tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using var http = new HttpClient();
            var module = new SegaEmulationModuleFactory().Create(new EmulationModuleContext(root, root, http));
            var configuration = Assert.IsType<MachineConfiguration>(module.CreateConfiguration(ModelConstants.MarkIII));
            var segaCard = Assert.Single(Assert.IsAssignableFrom<IEmulationStorageSettingsManager>(module)
                .DescribeStorageSettings(configuration).AvailableDevices,
                device => device.Slot == EmulationMediaSlot.Cartridge1);
            Assert.Equal(EmulationStorageConfigurationKind.None, segaCard.ConfigurationKind);
            Assert.Null(segaCard.ConfigurationOptionResourceKey);
            Assert.Null(segaCard.ConfigurationOptionDetailedResourceKey);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void SegaCardConfigurationPersistsItsGenericOptionAndRemovesCardMediaWhenEnabled()
    {
        var root = Path.Combine(Path.GetTempPath(), "gwgui-sega-card-settings-tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var cardPath = Path.Combine(root, "game.mv");
        File.WriteAllBytes(cardPath, new byte[0x4000]);
        try
        {
            using var http = new HttpClient();
            var module = new SegaEmulationModuleFactory().Create(new EmulationModuleContext(root, root, http));
            var configuration = Assert.IsType<MachineConfiguration>(module.CreateConfiguration(ModelConstants.MasterSystem)) with
            {
                Media = [new MediaConfiguration(cardPath, MediaCategory.Cartridge,
                    EmulationMediaSlot.Cartridge1, IsInserted: true)]
            };
            var manager = Assert.IsAssignableFrom<IEmulationStorageSettingsManager>(module);
            var described = manager.DescribeStorageSettings(configuration);
            var enabled = described with
            {
                DeviceSettings = [new EmulationStorageDeviceSettings(
                    EmulationMediaSlot.Cartridge1, Cartridge: new CartridgeSlotSettings(true))]
            };
            var applied = Assert.IsType<MachineConfiguration>(manager.ApplyStorageSettings(configuration, enabled));
            Assert.Equal("True", applied.Options![SettingsConstants.MasterSystemThreeDGlasses]);
            Assert.DoesNotContain(applied.Media!, media => media.Slot == EmulationMediaSlot.Cartridge1);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void SegaAtomiswaveExposesFlycastCartridgeStorage()
    {
        var root = Path.Combine(Path.GetTempPath(), "gwgui-sega-atomiswave-storage-tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using var http = new HttpClient();
            var module = new SegaEmulationModuleFactory().Create(new EmulationModuleContext(root, root, http));
            var configuration = Assert.IsType<MachineConfiguration>(
                module.CreateConfiguration(ModelConstants.Atomiswave));
            var storage = Assert.IsAssignableFrom<IEmulationStorageSettingsManager>(module)
                .DescribeStorageSettings(configuration);
            var cartridge = Assert.Single(storage.AvailableDevices,
                device => device.Slot == EmulationMediaSlot.Cartridge0);
            Assert.Equal([".zip"], cartridge.AcceptedExtensions);
            Assert.Contains(EmulationMediaSlot.Cartridge0, storage.ConfiguredSlots);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void SegaCatalogListsOfficialPeripheralsIndependentlyOfCoreSupport()
    {
        var model = ModelCatalog.Get("MegaDrive");
        var types = ControllerCatalog.Types(model);
        Assert.Contains(ControllerType.SegaMegaDriveThreeButton, types);
        Assert.Contains(ControllerType.SegaMegaDriveSixButton, types);
        Assert.Contains(ControllerType.SegaMegaMouse, types);
        Assert.Contains(ControllerType.SegaMenacer, types);
        Assert.Contains(ControllerType.SegaActivator, types);
    }

    [Fact]
    public void SegaCatalogKeepsMegaCdAndThirtyTwoXAsMegaDriveExtensions()
    {
        Assert.DoesNotContain(ModelCatalog.All, model => model.Id is "MegaCd" or "ThirtyTwoX");
        Assert.Contains(ModelCatalog.All, model => model.Id == "MegaDrive");
    }

    [Fact]
    public void SegaMegaDriveStorageExposesEveryPublishedCartridgeExtension()
    {
        var root = Path.Combine(Path.GetTempPath(), "gwgui-sega-megadrive-storage-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using var http = new HttpClient();
            var module = new SegaEmulationModuleFactory().Create(new EmulationModuleContext(root, root, http));
            var configuration = Assert.IsType<MachineConfiguration>(module.CreateConfiguration(ModelConstants.MegaDrive));
            var storage = Assert.IsAssignableFrom<IEmulationStorageSettingsManager>(module).DescribeStorageSettings(configuration);
            var cartridge = Assert.Single(storage.AvailableDevices, device => device.Slot == EmulationMediaSlot.Cartridge0);
            Assert.Equal(
                [".md", ".mdx", ".sgd", ".smd", ".bms", ".68k", ".gen", ".32x"],
                cartridge.AcceptedExtensions);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void SegaOpticalStorageMatchesTheSelectedMachine()
    {
        var root = Path.Combine(Path.GetTempPath(), "gwgui-sega-optical-storage-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using var http = new HttpClient();
            var module = new SegaEmulationModuleFactory().Create(new EmulationModuleContext(root, root, http));
            var saturn = Assert.IsType<MachineConfiguration>(module.CreateConfiguration(ModelConstants.Saturn));
            var storageManager = Assert.IsAssignableFrom<IEmulationStorageSettingsManager>(module);
            var saturnDisc = Assert.Single(storageManager.DescribeStorageSettings(saturn).AvailableDevices,
                device => device.Slot == EmulationMediaSlot.Cd0);
            Assert.Equal([".cue", ".ccd", ".chd", ".iso"], saturnDisc.AcceptedExtensions);

            var megaDrive = Assert.IsType<MachineConfiguration>(module.CreateConfiguration(ModelConstants.MegaDrive)) with
            {
                Options = new Dictionary<string, string>
                {
                    [SettingsConstants.MegaCdEnabled] = SettingsDescriptionFunctionsConstants.Enabled,
                    [SettingsConstants.MegaCdModel] = ModelConstants.MegaCdI
                }
            };
            var megaCd = Assert.Single(storageManager.DescribeStorageSettings(megaDrive).AvailableDevices,
                device => device.Slot == EmulationMediaSlot.Cd0);
            Assert.Equal([".cue", ".chd", ".iso", ".gdi", ".cdi"], megaCd.AcceptedExtensions);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void SegaFirmwareSettingsExposeExternalRomPathsForDiscMachines()
    {
        var root = Path.Combine(Path.GetTempPath(), "gwgui-sega-firmware-settings-tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using var http = new HttpClient();
            var module = new SegaEmulationModule(root, root, http, Path.Combine(root, "Core"));
            var saturn = Assert.IsType<MachineConfiguration>(module.CreateConfiguration(ModelConstants.Saturn));
            var dreamcast = Assert.IsType<MachineConfiguration>(module.CreateConfiguration(ModelConstants.Dreamcast));
            var naomi = Assert.IsType<MachineConfiguration>(module.CreateConfiguration(ModelConstants.Naomi));
            var naomi2 = Assert.IsType<MachineConfiguration>(module.CreateConfiguration(ModelConstants.Naomi2));
            var atomiswave = Assert.IsType<MachineConfiguration>(module.CreateConfiguration(ModelConstants.Atomiswave));
            var markIii = Assert.IsType<MachineConfiguration>(module.CreateConfiguration(ModelConstants.MarkIII));
            var masterSystem = Assert.IsType<MachineConfiguration>(module.CreateConfiguration(ModelConstants.MasterSystem));
            var gameGear = Assert.IsType<MachineConfiguration>(module.CreateConfiguration(ModelConstants.GameGear));
            var megaDrive = Assert.IsType<MachineConfiguration>(module.CreateConfiguration(ModelConstants.MegaDrive)) with
            {
                Options = new Dictionary<string, string>
                {
                    [SettingsConstants.MegaCdEnabled] = SettingsDescriptionFunctionsConstants.Enabled
                }
            };

            Assert.Equal(EmulationSettingsEditor.Path, FirmwareField(module, saturn).Editor);
            Assert.Equal(EmulationSettingsEditor.Path, FirmwareField(module, dreamcast).Editor);
            Assert.Equal(EmulationSettingsEditor.Path, FirmwareField(module, naomi).Editor);
            Assert.Equal(EmulationSettingsEditor.Path, FirmwareField(module, naomi2).Editor);
            Assert.Equal(EmulationSettingsEditor.Path, FirmwareField(module, atomiswave).Editor);
            Assert.Equal(EmulationSettingsEditor.Path, FirmwareField(module, markIii).Editor);
            Assert.Equal(EmulationSettingsEditor.Path, FirmwareField(module, masterSystem).Editor);
            Assert.Equal(EmulationSettingsEditor.Path, FirmwareField(module, gameGear).Editor);
            Assert.Equal(EmulationSettingsEditor.Path, FirmwareField(module, megaDrive).Editor);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    private static EmulationSettingsField FirmwareField(SegaEmulationModule module,
        MachineConfiguration configuration) => module.Describe(configuration.Model, configuration).Blocks
            .Single(block => block.Fields.Any(field => field.Id == SettingsConstants.FirmwarePath
                || field.Id == SettingsConstants.FirmwareIntegrated)).Fields.Single();

    [Fact]
    public async Task SegaMegaDriveAddonsAreDisabledUntilEnabledAndValidateMedia()
    {
        var root = Path.Combine(Path.GetTempPath(), "gwgui-sega-addon-tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var thirtyTwoXPath = Path.Combine(root, "game.32x");
        var compactDiscPath = Path.Combine(root, "game.cue");
        File.WriteAllText(thirtyTwoXPath, "placeholder");
        File.WriteAllText(compactDiscPath, "placeholder");
        try
        {
            using var http = new HttpClient();
            var module = new SegaEmulationModuleFactory().Create(
                new EmulationModuleContext(root, root, http));
            var megaDrive = Assert.IsType<MachineConfiguration>(
                module.CreateConfiguration(ModelConstants.MegaDrive));
            var disabledStorage = Assert.IsAssignableFrom<IEmulationStorageSettingsManager>(module)
                .DescribeStorageSettings(megaDrive);
            Assert.DoesNotContain(disabledStorage.ConfiguredSlots, slot =>
                slot == EmulationMediaSlot.Cd0);
            var settings = module.Describe(ModelConstants.MegaDrive, megaDrive);
            var fields = settings.Blocks.SelectMany(block => block.Fields)
                .ToDictionary(field => field.Id, StringComparer.Ordinal);
            Assert.Equal("disabled", fields[SettingsConstants.MegaCdEnabled].Value);
            Assert.Equal("disabled", fields[SettingsConstants.MegaDriveThirtyTwoX].Value);

            var invalid = megaDrive with
            {
                Media = [new MediaConfiguration(thirtyTwoXPath, MediaCategory.Cartridge,
                    EmulationMediaSlot.Cartridge0)]
            };
            await Assert.ThrowsAsync<InvalidDataException>(() =>
                module.SaveConfigurationAsync(invalid).AsTask());

            var enabled = megaDrive with
            {
                Options = new Dictionary<string, string>
                {
                    [SettingsConstants.MegaCdEnabled] = SettingsDescriptionFunctionsConstants.Enabled,
                    [SettingsConstants.MegaCdModel] = ModelConstants.MegaCdII,
                    [SettingsConstants.MegaDriveThirtyTwoX] = SettingsDescriptionFunctionsConstants.Enabled
                },
                Media =
                [
                    new MediaConfiguration(thirtyTwoXPath, MediaCategory.Cartridge,
                        EmulationMediaSlot.Cartridge0),
                    new MediaConfiguration(compactDiscPath, MediaCategory.CompactDisc,
                        EmulationMediaSlot.Cd0)
                ]
            };
            var enabledStorage = Assert.IsAssignableFrom<IEmulationStorageSettingsManager>(module)
                .DescribeStorageSettings(enabled);
            Assert.Contains(enabledStorage.AvailableDevices,
                device => device.Slot == EmulationMediaSlot.Cd0);
            await module.SaveConfigurationAsync(enabled).AsTask();

            var loaded = Assert.Single(await module.LoadConfigurationsAsync());
            var loadedConfiguration = Assert.IsType<MachineConfiguration>(loaded);
            Assert.True(loadedConfiguration.MegaCdEnabled);
            Assert.Equal(ModelConstants.MegaCdII, loadedConfiguration.MegaCdModel);
            Assert.True(loadedConfiguration.ThirtyTwoXEnabled);
            Assert.Contains(loadedConfiguration.Media!, media =>
                media.Slot == EmulationMediaSlot.Cartridge0
                && Path.GetExtension(media.Path).Equals(".32x", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(loadedConfiguration.Media!, media =>
                media.Slot == EmulationMediaSlot.Cd0
                && Path.GetExtension(media.Path).Equals(".cue", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void GenesisPlusGxFiltersConfigurationOptionsToItsPublishedCatalog()
    {
        var options = new Dictionary<string, string>
        {
            ["genesis_plus_gx_region"] = "pal",
            [SettingsConstants.MegaCdEnabled] = SettingsDescriptionFunctionsConstants.Enabled,
            ["custom"] = "discarded"
        };
        var catalog = new[]
        {
            new CoreOption("genesis_plus_gx_region", "Region", null, null,
                "ntsc", "ntsc", [new CoreOptionValue("ntsc", "NTSC"),
                new CoreOptionValue("pal", "PAL")])
        };

        var filtered = GenesisPlusGXOptionFunctions.FilterToCoreOptions(options, catalog);

        Assert.Equal("pal", filtered["genesis_plus_gx_region"]);
        Assert.DoesNotContain(SettingsConstants.MegaCdEnabled, filtered.Keys);
        Assert.DoesNotContain("custom", filtered.Keys);
    }

    [Fact]
    public void GenesisPlusGxRejectsAnUnsupportedConfiguredExtension()
    {
        var supported = new HashSet<string>(["md", "m3u"], StringComparer.OrdinalIgnoreCase);

        Assert.Throws<InvalidDataException>(() => ExternalCore.ValidateConfiguredExtensions(
            supported, ["game.md", "game.32x"]));
    }

    [Fact]
    public void GenesisPlusGxDoesNotBuildAPlaylistForMultipleMedia()
    {
        var media = new[]
        {
            new MediaConfiguration("game.md", MediaCategory.Cartridge,
                EmulationMediaSlot.Cartridge0),
            new MediaConfiguration("game.cue", MediaCategory.CompactDisc,
                EmulationMediaSlot.Cd0)
        };

        Assert.Throws<InvalidOperationException>(() => ExternalCore.PrepareContentPath(media));
    }

    [Fact]
    public void SegaOpticalAdaptersOrderMediaAndValidateFloppyPlaylists()
    {
        var root = Path.Combine(Path.GetTempPath(), "gwgui-sega-media-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var media = new[]
            {
                new MediaConfiguration(Path.Combine(root, "disc-b.cue"), MediaCategory.CompactDisc,
                    EmulationMediaSlot.Cd0, MountOrder: 2),
                new MediaConfiguration(Path.Combine(root, "disc-a.cue"), MediaCategory.CompactDisc,
                    EmulationMediaSlot.Cd0, MountOrder: 1),
                new MediaConfiguration(Path.Combine(root, "ejected.cue"), MediaCategory.CompactDisc,
                    EmulationMediaSlot.Cd0, IsInserted: false, MountOrder: 0)
            };
            var configuration = new MachineConfiguration(ModelConstants.Dreamcast, "flycast", Media: media);
            var flycastOrdered = FlycastExternalCore.ResolveConfiguredMedia(configuration);
            var yabauseOrdered = YabauseExternalCore.ResolveConfiguredMedia(configuration);
            Assert.Equal([media[1], media[0]], flycastOrdered);
            Assert.Equal([media[1], media[0]], yabauseOrdered);
            Assert.Equal(Path.GetFullPath(media[1].Path),
                FlycastExternalCore.PrepareContentPath(flycastOrdered, root));
            Assert.Equal(Path.GetFullPath(media[1].Path),
                YabauseExternalCore.PrepareContentPath(yabauseOrdered, root));

            var flycastFloppies = Enumerable.Range(0, FlycastExternalCoreConstants.MaximumPlaylistEntries)
                .Select(index => new MediaConfiguration(Path.Combine(root, $"flycast-{index}.dsk"),
                    MediaCategory.Floppy, EmulationMediaSlot.Floppy0, MountOrder: index)).ToArray();
            var flycastPlaylist = FlycastExternalCore.PrepareContentPath(flycastFloppies, root);
            Assert.Equal(Path.Combine(root, FlycastExternalCoreConstants.PlaylistName), flycastPlaylist);
            Assert.Equal(flycastFloppies.Select(item => Path.GetFullPath(item.Path)),
                File.ReadAllLines(flycastPlaylist!));
            Assert.Throws<ArgumentOutOfRangeException>(() => FlycastExternalCore.PrepareContentPath(
                flycastFloppies.Append(flycastFloppies[0]).ToArray(), root));

            var yabauseFloppies = Enumerable.Range(0, YabauseExternalCoreConstants.MaximumPlaylistEntries)
                .Select(index => new MediaConfiguration(Path.Combine(root, $"yabause-{index}.dsk"),
                    MediaCategory.Floppy, EmulationMediaSlot.Floppy0, MountOrder: index)).ToArray();
            var yabausePlaylist = YabauseExternalCore.PrepareContentPath(yabauseFloppies, root);
            Assert.Equal(Path.Combine(root, YabauseExternalCoreConstants.PlaylistName), yabausePlaylist);
            Assert.Equal(yabauseFloppies.Select(item => Path.GetFullPath(item.Path)),
                File.ReadAllLines(yabausePlaylist!));
            Assert.Throws<ArgumentOutOfRangeException>(() => YabauseExternalCore.PrepareContentPath(
                yabauseFloppies.Append(yabauseFloppies[0]).ToArray(), root));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void SegaAdaptersOnlyPublishModelsFromTheMachineCatalog()
    {
        var machineIds = ModelCatalog.All.Select(model => model.Id)
            .ToHashSet(StringComparer.Ordinal);
        Assert.All(EmulatorCatalog.All, definition =>
            Assert.All(definition.MachineIds, machineId => Assert.Contains(machineId, machineIds)));
        Assert.All(ModelCatalog.All, model => Assert.NotEmpty(EmulatorCatalog.GetAll(model.Id)));
    }

    [Fact]
    public void SegaInputSettingsNormalizeOfficialControllerVisuals()
    {
        var root = Path.Combine(Path.GetTempPath(), "gwgui-sega-input-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using var http = new HttpClient();
            var emulation = new SegaEmulationModuleFactory().Create(
                new EmulationModuleContext(root, root, http));
            var module = Assert.IsAssignableFrom<IEmulationInputSettingsManager>(emulation);
            var megaDrive = Assert.IsType<MachineConfiguration>(emulation.CreateConfiguration("MegaDrive"));
            var configured = megaDrive with
            {
                Input = new InputConfiguration(ControllerBindings:
                    [new ControllerBinding(0, ControllerType.SegaMegaDriveThreeButton,
                        VisualId: EmulationControllerVisualIds.MegaDrive6)])
            };
            var described = module.DescribeInputSettings(configured);
            Assert.Equal(2, described.ControllerPorts.Count);
            var port = described.ControllerPorts[0];
            Assert.Equal(EmulationControllerVisualIds.MegaDrive3, port.VisualId);
            Assert.Contains(port.ControllerChoices,
                choice => choice.Id == ControllerType.SegaMegaDriveThreeButton.ToString());
            Assert.Contains(port.ControllerChoices,
                choice => choice.Id == ControllerType.SegaMegaDriveSixButton.ToString());

            var masterSystem = Assert.IsType<MachineConfiguration>(
                emulation.CreateConfiguration("MasterSystem"));
            var masterPorts = module.DescribeInputSettings(masterSystem).ControllerPorts;
            Assert.Equal(2, masterPorts.Count);
            var masterPort = masterPorts[0];
            Assert.Contains(masterPort.ControllerChoices,
                choice => choice.Id == ControllerType.SegaLightPhaser.ToString());
            Assert.Equal(EmulationControllerVisualIds.MasterSystem,
                masterPort.ControllerChoices.Single(choice => choice.Id ==
                    ControllerType.SegaMasterSystemController.ToString()).DefaultVisualId);

            var saturn = Assert.IsType<MachineConfiguration>(
                emulation.CreateConfiguration(ModelConstants.Saturn));
            var saturnPort = module.DescribeInputSettings(saturn).ControllerPorts[0];
            Assert.Equal(EmulationControllerVisualIds.Saturn, saturnPort.VisualId);
            Assert.Equal(EmulationControllerVisualIds.Saturn,
                saturnPort.ControllerChoices.Single(choice => choice.Id ==
                    ControllerType.SegaSaturnController.ToString()).DefaultVisualId);

            var dreamcast = Assert.IsType<MachineConfiguration>(
                emulation.CreateConfiguration(ModelConstants.Dreamcast));
            var dreamcastPort = module.DescribeInputSettings(dreamcast).ControllerPorts[0];
            Assert.Equal(EmulationControllerVisualIds.Dreamcast, dreamcastPort.VisualId);
            Assert.Equal(EmulationControllerVisualIds.Dreamcast,
                dreamcastPort.ControllerChoices.Single(choice => choice.Id ==
                    ControllerType.SegaDreamcastController.ToString()).DefaultVisualId);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void SegaInputSettingsExposeAResourceForEveryPublishedPeripheral()
    {
        var root = Path.Combine(Path.GetTempPath(), "gwgui-sega-input-label-tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using var http = new HttpClient();
            var module = new SegaEmulationModuleFactory().Create(
                new EmulationModuleContext(root, root, http));
            var manager = Assert.IsAssignableFrom<IEmulationInputSettingsManager>(module);
            foreach (var model in ModelCatalog.All.Where(item => item.ControllerPortCount > 0))
            {
                var configuration = Assert.IsType<MachineConfiguration>(
                    module.CreateConfiguration(model.Id));
                var settings = manager.DescribeInputSettings(configuration);
                foreach (var choice in settings.ControllerPorts.SelectMany(
                    port => port.ControllerChoices))
                    Assert.False(string.IsNullOrWhiteSpace(choice.DisplayResourceKey));
            }
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task SegaInputSettingsRoundTripPreservesControllerIdentityBindingsAndVisual()
    {
        var root = Path.Combine(Path.GetTempPath(), "gwgui-sega-input-roundtrip-tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using var http = new HttpClient();
            var module = Assert.IsAssignableFrom<IEmulationInputSettingsManager>(
                new SegaEmulationModuleFactory().Create(new EmulationModuleContext(root, root, http)));
            var emulation = Assert.IsAssignableFrom<IEmulationModule>(module);
            var original = Assert.IsType<MachineConfiguration>(emulation.CreateConfiguration("MegaDrive")) with
            {
                Input = new InputConfiguration(ControllerBindings:
                [
                    new ControllerBinding(0, ControllerType.SegaMegaDriveSixButton,
                        DeviceId: "gameinput:pad-0",
                        ButtonMappings: new Dictionary<string, string>
                        {
                            ["A"] = "Controller:gameinput:pad-0:ButtonA",
                            ["B"] = "Controller:gameinput:pad-0:ButtonB"
                        },
                        VisualId: EmulationControllerVisualIds.MegaDrive6)
                ])
            };

            var described = module.DescribeInputSettings(original);
            var applied = Assert.IsType<MachineConfiguration>(
                module.ApplyInputSettings(original, described));
            var appliedBinding = Assert.Single(applied.Input!.ControllerBindings!, binding => binding.Port == 0);
            Assert.Equal(ControllerType.SegaMegaDriveSixButton, appliedBinding.Type);
            Assert.Equal("gameinput:pad-0", appliedBinding.DeviceId);
            Assert.Equal(EmulationControllerVisualIds.MegaDrive6, appliedBinding.VisualId);
            Assert.Equal(original.Input!.ControllerBindings![0].ButtonMappings,
                appliedBinding.ButtonMappings);

            await emulation.SaveConfigurationAsync(applied);
            var loaded = Assert.Single(await emulation.LoadConfigurationsAsync());
            var loadedConfiguration = Assert.IsType<MachineConfiguration>(loaded);
            var loadedBinding = Assert.Single(loadedConfiguration.Input!.ControllerBindings!,
                binding => binding.Port == 0);
            Assert.Equal(appliedBinding.Type, loadedBinding.Type);
            Assert.Equal(appliedBinding.DeviceId, loadedBinding.DeviceId);
            Assert.Equal(appliedBinding.VisualId, loadedBinding.VisualId);
            Assert.Equal(appliedBinding.ButtonMappings, loadedBinding.ButtonMappings);

            var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            var restored = JsonSerializer.Deserialize<MachineConfiguration>(
                JsonSerializer.Serialize(applied, jsonOptions), jsonOptions);
            var restoredBinding = Assert.Single(restored!.Input!.ControllerBindings!,
                binding => binding.Port == 0);
            Assert.Equal(appliedBinding.Type, restoredBinding.Type);
            Assert.Equal(appliedBinding.DeviceId, restoredBinding.DeviceId);
            Assert.Equal(appliedBinding.VisualId, restoredBinding.VisualId);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void GenesisPlusGxUsesOnlyCorePublishedDevicesAndMapsLightGunPointer()
    {
        var devices = new IReadOnlyList<ControllerDevice>[]
        {
            [new ControllerDevice("Joypad", 1), new ControllerDevice("Mouse", 2)]
        };
        Assert.Equal(1u, ExternalCore.ResolveControllerDevice(devices[0],
            ControllerType.SegaMegaDriveThreeButton));
        Assert.Equal(2u, ExternalCore.ResolveControllerDevice(devices[0],
            ControllerType.SegaMegaMouse));
        Assert.Equal(0u, ExternalCore.ResolveControllerDevice(devices[0],
            ControllerType.SegaLightPhaser));

        var root = Path.Combine(Path.GetTempPath(), "gwgui-sega-lightgun-tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using var callbacks = new ExternalHostCallbacks(
                Path.Combine(root, "system"), Path.Combine(root, "content"),
                Path.Combine(root, "saves"), new Dictionary<string, string>());
            callbacks.Input = EmulationInputSnapshot.Empty with
            {
                Pointer = EmulationInputSnapshot.Empty.Pointer with
                {
                    DeltaX = 2,
                    DeltaY = -1,
                    Left = true
                }
            };
            callbacks.InputPoll();
            Assert.Equal((short)(ExternalHostCallbacksConstants.PointerCoordinateCenter
                + 2 * ExternalHostCallbacksConstants.PointerCoordinateScale),
                callbacks.ReadInputState(0, ExternalHostCallbacksConstants.LightGunDevice, 0,
                    ExternalHostCallbacksConstants.LightGunScreenX));
            Assert.Equal((short)(ExternalHostCallbacksConstants.PointerCoordinateCenter
                - ExternalHostCallbacksConstants.PointerCoordinateScale),
                callbacks.ReadInputState(0, ExternalHostCallbacksConstants.LightGunDevice, 0,
                    ExternalHostCallbacksConstants.LightGunScreenY));
            Assert.Equal((short)1, callbacks.ReadInputState(0,
                ExternalHostCallbacksConstants.LightGunDevice, 0,
                ExternalHostCallbacksConstants.LightGunTrigger));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void SegaFirmwareCatalogIdentifiesOnlyVerifiedSystemProfiles()
    {
        var verified = new[]
        {
            (FirmwareConstants.MegaDriveBiosMd5, ModelConstants.MegaDrive),
            (FirmwareConstants.MegaDriveBiosAlternateMd5, ModelConstants.MegaDrive),
            (FirmwareConstants.MegaCdEuropeBiosMd5, ModelConstants.MegaDrive),
            (FirmwareConstants.MegaCdUnitedStatesBiosMd5, ModelConstants.MegaDrive),
            (FirmwareConstants.MegaCdJapanBiosMd5, ModelConstants.MegaDrive),
            (FirmwareConstants.MasterSystemEuropeBiosMd5, ModelConstants.MasterSystem),
            (FirmwareConstants.MasterSystemEuropeBiosAlternateMd5, ModelConstants.MasterSystem),
            (FirmwareConstants.MasterSystemUnitedStatesBiosAlternateMd5, ModelConstants.MasterSystem),
            (FirmwareConstants.MasterSystemJapanBiosMd5, ModelConstants.MasterSystem),
            (FirmwareConstants.GameGearBiosMd5, ModelConstants.GameGear),
            (FirmwareConstants.SaturnBiosMd5, ModelConstants.Saturn),
            (FirmwareConstants.SaturnBiosEuropeUnitedStatesMd5, ModelConstants.Saturn),
            (FirmwareConstants.SaturnBiosJapanV101Md5, ModelConstants.Saturn),
            (FirmwareConstants.DreamcastBiosMd5, ModelConstants.Dreamcast),
            (FirmwareConstants.NaomiBiosMd5, ModelConstants.Naomi),
            (FirmwareConstants.Naomi2BiosMd5, ModelConstants.Naomi2),
            (FirmwareConstants.AtomiswaveBiosMd5, ModelConstants.Atomiswave)
        };
        foreach (var (md5, model) in verified)
        {
            Assert.True(FirmwareCatalog.TryIdentifyKnown(md5, out var identity));
            Assert.Contains(model, identity.Models);
            Assert.NotEmpty(identity.FileNames);
        }
        Assert.Equal([FirmwareConstants.SaturnBiosFileName],
            AssertIdentity(FirmwareConstants.SaturnBiosMd5).FileNames);
        Assert.Equal([FirmwareConstants.DreamcastBiosRelativeFileName],
            AssertIdentity(FirmwareConstants.DreamcastBiosMd5).FileNames);
        Assert.Equal([FirmwareConstants.NaomiBiosRelativeFileName],
            AssertIdentity(FirmwareConstants.NaomiBiosMd5).FileNames);
        Assert.Equal([FirmwareConstants.Naomi2BiosRelativeFileName],
            AssertIdentity(FirmwareConstants.Naomi2BiosMd5).FileNames);
        Assert.Equal([FirmwareConstants.AtomiswaveBiosRelativeFileName],
            AssertIdentity(FirmwareConstants.AtomiswaveBiosMd5).FileNames);
        Assert.Equal([FirmwareConstants.MegaDriveBiosFileName],
            AssertIdentity(FirmwareConstants.MegaDriveBiosAlternateMd5).FileNames);
        Assert.Equal([FirmwareConstants.MasterSystemEuropeBiosFileName],
            AssertIdentity(FirmwareConstants.MasterSystemEuropeBiosAlternateMd5).FileNames);
        Assert.Equal([FirmwareConstants.MasterSystemUnitedStatesBiosFileName],
            AssertIdentity(FirmwareConstants.MasterSystemUnitedStatesBiosAlternateMd5).FileNames);
        Assert.False(FirmwareCatalog.TryIdentifyKnown("00000000000000000000000000000000",
            out _));

        static (string Name, string Version, string[] Models, string[] FileNames) AssertIdentity(string md5)
        {
            Assert.True(FirmwareCatalog.TryIdentifyKnown(md5, out var identity));
            return identity;
        }
    }

    [Fact]
    public async Task SegaFirmwareScanLeavesUnknownCustomFirmwareNonSelectable()
    {
        var root = Path.Combine(Path.GetTempPath(), "gwgui-sega-firmware-scan-tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using var http = new HttpClient();
            var sega = Assert.IsType<SegaEmulationModule>(new SegaEmulationModuleFactory().Create(
                new EmulationModuleContext(root, root, http)));
            var manager = Assert.IsAssignableFrom<IEmulationFirmwareManager>(sega);
            var configuration = Assert.IsType<MachineConfiguration>(
                sega.CreateConfiguration(ModelConstants.MegaDrive));
            var firmwarePath = Path.Combine(sega.GetFirmwareDirectory(ModelConstants.MegaDrive),
                "custom-sega-firmware.bin");
            Directory.CreateDirectory(Path.GetDirectoryName(firmwarePath)!);
            await File.WriteAllBytesAsync(firmwarePath, Enumerable.Repeat((byte)0x5a, 4096).ToArray());

            var candidate = Assert.Single(await manager.ScanFirmwareAsync(
                ModelConstants.MegaDrive, configuration));
            Assert.Equal("custom-sega-firmware.bin", candidate.DisplayName);
            Assert.Equal(EmulationFirmwareCompatibility.Incompatible, candidate.Compatibility);
            Assert.Null(candidate.DestinationFieldId);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void SegaNeutralResourcesContainOnlyInvariantEntries()
    {
        var root = RepositoryRoot();
        var baseDirectory = Path.Combine(root, "src", "GWGUI.Emulation.Sega", "Resources", "00-Base");
        var fallbackDirectory = Path.Combine(root, "src", "GWGUI.Emulation.Sega", "Resources", "en-US");
        foreach (var baseFile in Directory.EnumerateFiles(baseDirectory, "*.resx"))
        {
            var fallbackFile = Path.Combine(fallbackDirectory, Path.GetFileName(baseFile));
            Assert.True(File.Exists(fallbackFile), fallbackFile);
            var duplicateKeys = ResxKeys(baseFile).Intersect(ResxKeys(fallbackFile)).ToArray();
            Assert.Empty(duplicateKeys);
        }

        static IReadOnlySet<string> ResxKeys(string path)
        {
            var document = new XmlDocument();
            document.Load(path);
            return document.SelectNodes("/root/data")!.Cast<XmlElement>()
                .Select(element => element.GetAttribute("name"))
                .ToHashSet(StringComparer.Ordinal);
        }
    }

    [Fact]
    public void SegaChipLabelsArePresentInEveryTranslatedCulture()
    {
        var root = RepositoryRoot();
        var resources = Path.Combine(root, "src", "GWGUI.Emulation.Sega", "Resources");
        Assert.Contains("Emulation.Sega.Video.Chipset", ResxKeys(
            Path.Combine(resources, "00-Base", "Video.resx")));
        Assert.Contains("Emulation.Sega.Audio.Chip", ResxKeys(
            Path.Combine(resources, "00-Base", "Machine.resx")));
        Assert.Empty(ResxKeys(Path.Combine(resources, "en-US", "Video.resx"))
            .Intersect(["Emulation.Sega.Video.Chipset"]));
        Assert.Empty(ResxKeys(Path.Combine(resources, "en-US", "Machine.resx"))
            .Intersect(["Emulation.Sega.Audio.Chip"]));
        foreach (var culture in Directory.EnumerateDirectories(resources)
                     .Where(path => !Path.GetFileName(path).Equals("00-Base", StringComparison.Ordinal)
                         && !Path.GetFileName(path).Equals("en-US", StringComparison.Ordinal)))
        {
            Assert.Contains("Emulation.Sega.Video.Chipset", ResxKeys(Path.Combine(culture, "Video.resx")));
            Assert.Contains("Emulation.Sega.Audio.Chip", ResxKeys(Path.Combine(culture, "Machine.resx")));
        }

        static IReadOnlySet<string> ResxKeys(string path)
        {
            var document = new XmlDocument();
            document.Load(path);
            return document.SelectNodes("/root/data")!.Cast<XmlElement>()
                .Select(element => element.GetAttribute("name"))
                .ToHashSet(StringComparer.Ordinal);
        }
    }

    [Fact]
    public void SegaHelpCataloguesContainOnlyReferencedSettings()
    {
        var root = RepositoryRoot();
        var resources = Path.Combine(root, "src", "GWGUI.Emulation.Sega", "Resources");
        var referenced = new[]
        {
            "General.Model", "General.Emulator", "MasterSystem.Variant",
            "MasterSystem.ThreeDGlasses", "MegaDrive.Model", "MegaDrive.MegaCd",
            "MegaDrive.MegaCd.Enabled", "MegaDrive.32X", "MegaDrive.Region",
            "MegaDrive.VideoStandard", "Cpu.Model", "Cpu.Frequency", "Memory.Ram",
            "Firmware.Integrated", "Audio.Enabled", "Audio.Output"
        }.SelectMany(name => new[]
        {
            "Emulation.Sega.Help." + name + ".Short",
            "Emulation.Sega.Help." + name + ".Detailed"
        }).ToHashSet(StringComparer.Ordinal);
        var obsolete = new[]
        {
            "Emulation.Sega.Help.Video.Resolution.Short",
            "Emulation.Sega.Help.Video.Resolution.Detailed",
            "Emulation.Sega.Help.Video.Monitor.Short",
            "Emulation.Sega.Help.Video.Monitor.Detailed",
            "Emulation.Sega.Help.Video.Intensity.Short",
            "Emulation.Sega.Help.Video.Intensity.Detailed",
            "Emulation.Sega.Help.Video.Crop.Short",
            "Emulation.Sega.Help.Video.Crop.Detailed",
            "Emulation.Sega.Help.Audio.FloppySound.Short",
            "Emulation.Sega.Help.Audio.FloppySound.Detailed"
        };
        foreach (var directory in Directory.EnumerateDirectories(resources)
                     .Where(path => !Path.GetFileName(path).Equals("00-Base",
                         StringComparison.Ordinal)))
        {
            var keys = ResxKeys(Path.Combine(directory, "Help.resx"));
            Assert.All(referenced, key => Assert.Contains(key, keys));
            Assert.DoesNotContain(keys, key => obsolete.Contains(key, StringComparer.Ordinal));
        }

        static IReadOnlySet<string> ResxKeys(string path)
        {
            var document = new XmlDocument();
            document.Load(path);
            return document.SelectNodes("/root/data")!.Cast<XmlElement>()
                .Select(element => element.GetAttribute("name"))
                .ToHashSet(StringComparer.Ordinal);
        }
    }

    [Fact]
    public void SecondCartridgeSlotHasAStableHostProtocolValue()
    {
        Assert.Equal(8, EmulationMediaSlot.Cartridge1.ProtocolValue);
        Assert.Equal(EmulationMediaSlot.Cartridge1, EmulationMediaSlot.FromProtocolValue(8));
    }

    private static string RepositoryRoot() => Path.GetFullPath(Path.Combine(
        Path.GetDirectoryName(typeof(ConsoleFamilyModuleTests).Assembly.Location)!,
        "..", "..", "..", "..", ".."));

    private static string[] CommonFiles(string root, string family) =>
        Directory.EnumerateFiles(Path.Combine(root, "src", $"GWGUI.Emulation.{family}", "Common"),
                "*.cs", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(
                Path.Combine(root, "src", $"GWGUI.Emulation.{family}", "Common"), path))
            .Order(StringComparer.Ordinal)
            .ToArray();
}
