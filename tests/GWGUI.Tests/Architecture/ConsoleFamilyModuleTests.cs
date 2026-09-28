using System.Reflection;
using System.Net.Http;
using System.IO;
using System.Text.Json;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Constants;
using GWGUI.Emulation;
using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Nec.Modules;
using GWGUI.Emulation.Nintendo.Modules;
using NintendoModelConstants = GWGUI.Emulation.Nintendo.Common.Machines.Common.Constants.ModelConstants;
using NintendoModelCatalog = GWGUI.Emulation.Nintendo.Common.Machines.Common.Dictionaries.ModelCatalog;
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
using GWGUI.Emulation.Sony.Modules;
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
    public void ConsoleFamiliesRejectMachinesWithoutAnInstalledAdapter()
    {
        var root = Path.Combine(Path.GetTempPath(), "gwgui-console-unsupported-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using var http = new HttpClient();
            var context = new EmulationModuleContext(root, root, http);
            var cases = new (IEmulationModule Module, string MachineId)[]
            {
                (new SonyEmulationModuleFactory().Create(context), "PsVita"),
                (new MicrosoftEmulationModuleFactory().Create(context), "Xbox")
            };

            foreach (var (module, machineId) in cases)
                Assert.Throws<NotSupportedException>(() => module.CreateConfiguration(machineId));
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
    public void NecBeetlePceSelectsPcEngineModels()
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
                Assert.Equal("beetle_pce_fast", configuration.EmulatorId);
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
            Assert.Contains(EmulationMediaSlot.Cartridge0, storage.ConfiguredSlots);
            Assert.Contains(EmulationMediaSlot.Cartridge1, storage.ConfiguredSlots);
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
            (FirmwareConstants.MegaCdEuropeBiosMd5, ModelConstants.MegaDrive),
            (FirmwareConstants.MegaCdUnitedStatesBiosMd5, ModelConstants.MegaDrive),
            (FirmwareConstants.MegaCdJapanBiosMd5, ModelConstants.MegaDrive),
            (FirmwareConstants.MasterSystemEuropeBiosMd5, ModelConstants.MasterSystem),
            (FirmwareConstants.MasterSystemJapanBiosMd5, ModelConstants.MasterSystem),
            (FirmwareConstants.GameGearBiosMd5, ModelConstants.GameGear),
            (FirmwareConstants.SaturnBiosMd5, ModelConstants.Saturn),
            (FirmwareConstants.DreamcastBiosMd5, ModelConstants.Dreamcast)
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
        Assert.False(FirmwareCatalog.TryIdentifyKnown("00000000000000000000000000000000",
            out _));

        static (string Name, string Version, string[] Models, string[] FileNames) AssertIdentity(string md5)
        {
            Assert.True(FirmwareCatalog.TryIdentifyKnown(md5, out var identity));
            return identity;
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
