using System.Net.Http;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Atari.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Atari.Common.Machines.Common.Enums;
using GWGUI.Emulation.Atari.Emulators.Common.Interop.Dictionaries;
using GWGUI.Emulation.Atari.Modules;
using GWGUI.Emulation.Atari.Common.Services;
using GWGUI.Emulation.Atari.Common.Machines.AtariST.Dictionaries;
using GWGUI.Emulation.Atari.Common.Machines.AtariST.Enums;
using GWGUI.App.Localization.Extensions;

namespace GWGUI.Tests.Emulation.Atari;

public sealed class AtariEmulatorAdapterTests
{
    [Theory]
    [InlineData(Emulator.BeetleLynx, "beetle-lynx", "mednafen_lynx_libretro.dll")]
    [InlineData(Emulator.GearLynx, "gearlynx", "gearlynx_libretro.dll")]
    [InlineData(Emulator.Handy, "handy", "handy_libretro.dll")]
    [InlineData(Emulator.Holani, "holani", "holani_libretro.dll")]
    public async Task LynxOffersFourAdaptersWithoutChangingItsDefault(Emulator emulator, string id, string dll)
    {
        using var httpClient = new HttpClient();
        var module = new AtariEmulationModule(Path.GetTempPath(), Path.GetTempPath(), httpClient, Path.GetTempPath());
        var configuration = Assert.IsType<MachineConfiguration>(module.CreateConfiguration(nameof(MachineModel.Lynx)));
        Assert.Equal(Emulator.BeetleLynx, configuration.Core);
        Assert.Equal(new[] { "beetle-lynx", "gearlynx", "handy", "holani" },
            CoreCatalog.GetAll(MachineModel.Lynx).Select(entry => entry.Id));
        var selected = Assert.IsType<MachineConfiguration>(await module.UseEmulatorAsync(configuration, id));
        Assert.Equal(emulator, selected.Core);
        Assert.Equal(dll, CoreCatalog.Get(emulator).DllName);
        Assert.Equal("Emulation.Emulator.atari-lynx.Description", CoreCatalog.Get(emulator).DescriptionResourceKey);
        var settings = module.Describe(nameof(MachineModel.Lynx), selected);
        Assert.True(settings.Visibility.Tabs[EmulationMachineTab.Rom]);
        Assert.Contains(settings.Blocks.SelectMany(block => block.Fields), field =>
            field.Id == "configuration.systemFirmware" && field.IsVisible && field.IsEnabled);
        Assert.Throws<ArgumentException>(() => new MachineConfiguration(MachineModel.Atari2600, core: emulator));
        Assert.Throws<ArgumentException>(() => new MachineConfiguration(MachineModel.Atari5200, core: emulator));
    }

    [Fact]
    public void LynxAdaptersUseTheirOwnCartridgeFormats()
    {
        Assert.Equal(new[] { "bll", "lnx", "lyx", "o" },
            GWGUI.Emulation.Atari.Emulators.BeetleLynx.Constants.EmulatorConstants.CartridgeExtensions.Order());
        Assert.Equal(new[] { "bin", "lnx", "lyx", "o" },
            GWGUI.Emulation.Atari.Emulators.GearLynx.Constants.EmulatorConstants.CartridgeExtensions.Order());
        Assert.Equal(new[] { "lnx", "lyx", "o" },
            GWGUI.Emulation.Atari.Emulators.Handy.Constants.EmulatorConstants.CartridgeExtensions.Order());
        Assert.Equal(new[] { "lnx", "o" },
            GWGUI.Emulation.Atari.Emulators.Holani.Constants.EmulatorConstants.CartridgeExtensions.Order());
    }

    [Theory]
    [InlineData(8, 0)]
    [InlineData(0, 8)]
    [InlineData(10, 10)]
    [InlineData(11, 11)]
    [InlineData(3, 3)]
    public void HolaniPreservesPhysicalLynxButtons(int command, int nativeButton)
    {
        var controller = EmulationControllerState.Empty with { Buttons = 1u << command, LeftX = 1234 };
        var snapshot = new EmulationInputSnapshot(new HashSet<EmulationKey>(),
            EmulationInputSnapshot.Empty.Pointer, [controller]);
        var converted = GWGUI.Emulation.Atari.Emulators.Holani.Functions.InputFunctions.ToNative(snapshot);
        Assert.Equal(1u << nativeButton, converted.Controllers[0].Buttons);
        Assert.Equal(controller.LeftX, converted.Controllers[0].LeftX);
        Assert.Equal(1u << command, snapshot.Controllers[0].Buttons);
    }

    [Theory]
    [InlineData(MachineModel.St, "0")]
    [InlineData(MachineModel.Stf, "0")]
    [InlineData(MachineModel.Stfm, "0")]
    [InlineData(MachineModel.MegaSt, "1")]
    [InlineData(MachineModel.Ste, "2")]
    [InlineData(MachineModel.MegaSte, "3")]
    [InlineData(MachineModel.Tt, "4")]
    [InlineData(MachineModel.Falcon, "5")]
    public async Task StMachinesOfferHatariBWithoutChangingHatariDefaults(MachineModel model, string nativeModel)
    {
        using var httpClient = new HttpClient();
        var module = new AtariEmulationModule(Path.GetTempPath(), Path.GetTempPath(), httpClient, Path.GetTempPath());
        var configuration = Assert.IsType<MachineConfiguration>(module.CreateConfiguration(model.ToString()));
        Assert.Equal(Emulator.Hatari, configuration.Core);
        Assert.Equal(new[] { "hatari", "hatari2014", "hatarib" }, CoreCatalog.GetAll(model).Select(entry => entry.Id));
        var selected = Assert.IsType<MachineConfiguration>(await module.UseEmulatorAsync(configuration, "hatarib"));
        Assert.Equal(Emulator.HatariB, selected.Core);
        var options = module.RuntimeOptions(selected);
        var hardware = StModelCatalog.Get(model);
        Assert.Equal(nativeModel, options["hatarib_machine"]);
        Assert.Equal(hardware.DefaultMainMemoryKib.ToString(CultureInfo.InvariantCulture), options["hatarib_memory"]);
        Assert.Equal(hardware.DefaultCpuFrequencyMhz.ToString(CultureInfo.InvariantCulture), options["hatarib_cpu_clock"]);
        Assert.Equal("<etos1024k>", options["hatarib_tos"]);
        Assert.DoesNotContain(options.Keys, key => key.StartsWith("hatari_", StringComparison.Ordinal));
        Assert.Equal(CoreCatalog.Get(Emulator.Hatari).DescriptionResourceKey,
            CoreCatalog.Get(Emulator.HatariB).DescriptionResourceKey);
    }

    [Theory]
    [InlineData(MachineModel.St, "st")]
    [InlineData(MachineModel.Stf, "st")]
    [InlineData(MachineModel.Stfm, "st")]
    [InlineData(MachineModel.MegaSt, "st")]
    [InlineData(MachineModel.Ste, "ste")]
    [InlineData(MachineModel.MegaSte, "ste")]
    [InlineData(MachineModel.Tt, "tt")]
    [InlineData(MachineModel.Falcon, "falcon")]
    public async Task StMachinesOfferHatari2014WithTheSharedLegacyOptions(MachineModel model, string nativeModel)
    {
        using var httpClient = new HttpClient();
        var module = new AtariEmulationModule(Path.GetTempPath(), Path.GetTempPath(), httpClient, Path.GetTempPath());
        var configuration = Assert.IsType<MachineConfiguration>(module.CreateConfiguration(model.ToString()));
        var selected = Assert.IsType<MachineConfiguration>(await module.UseEmulatorAsync(configuration, "hatari2014"));
        Assert.Equal(Emulator.Hatari2014, selected.Core);
        Assert.Equal(Emulator.Hatari, configuration.Core);
        Assert.Equal("hatari2014_libretro.dll", CoreCatalog.Get(selected.Core).DllName);
        Assert.Equal("Hatari2014", CoreCatalog.Get(selected.Core).LibraryName);
        var expected = module.RuntimeOptions(configuration);
        var actual = module.RuntimeOptions(selected);
        Assert.Equal(nativeModel, actual["hatari_machinetype"]);
        Assert.Equal(expected.OrderBy(pair => pair.Key), actual.OrderBy(pair => pair.Key));
        Assert.Equal(CoreCatalog.Get(Emulator.Hatari).DescriptionResourceKey,
            CoreCatalog.Get(selected.Core).DescriptionResourceKey);
    }

    [Fact]
    public void Hatari2014CannotBeSelectedForOtherAtariMachines()
    {
        Assert.Throws<ArgumentException>(() => new MachineConfiguration(MachineModel.Atari2600, core: Emulator.Hatari2014));
        Assert.Throws<ArgumentException>(() => new MachineConfiguration(MachineModel.Atari800, core: Emulator.Hatari2014));
        Assert.Throws<ArgumentException>(() => new MachineConfiguration(MachineModel.Atari5200, core: Emulator.Hatari2014));
        var leds = new Dictionary<int, bool> { [0] = true, [1] = false, [2] = true };
        Assert.Equal(
            GWGUI.Emulation.Atari.Common.Machines.Common.Functions.EmulationMediaActivityFunctions.FromLedStates(Emulator.Hatari, leds),
            GWGUI.Emulation.Atari.Common.Machines.Common.Functions.EmulationMediaActivityFunctions.FromLedStates(Emulator.Hatari2014, leds));
    }

    [Fact]
    public void HatariBUsesItsOwnOptionsAndOnlyStMachines()
    {
        Assert.Throws<ArgumentException>(() => new MachineConfiguration(MachineModel.Atari2600, core: Emulator.HatariB));
        Assert.Throws<ArgumentException>(() => new MachineConfiguration(MachineModel.Atari5200, core: Emulator.HatariB));
        var configuration = new MachineConfiguration(MachineModel.Falcon, core: Emulator.HatariB,
            firmwares: [new FirmwareConfiguration(FirmwareCategory.Tos, "tos.img", false)],
            options: new Dictionary<string, string>
            {
                ["gwgui_atari_main_memory"] = "4194304", ["gwgui_atari_cpu_frequency"] = "32",
                ["gwgui_atari_cpu"] = nameof(StCpu.Motorola68030),
                ["gwgui_atari_fpu"] = nameof(StFpu.Motorola68882),
                ["gwgui_atari_cpu_precision"] = nameof(StCpuPrecision.CycleExact)
            });
        var options = GWGUI.Emulation.Atari.Emulators.HatariB.Functions.OptionFunctions.Apply(configuration);
        Assert.Equal("4096", options["hatarib_memory"]);
        Assert.Equal("32", options["hatarib_cpu_clock"]);
        Assert.Equal("3", options["hatarib_cpu"]);
        Assert.Equal("68882", options["hatarib_fpu"]);
        Assert.Equal("1", options["hatarib_cycle_exact"]);
        Assert.Equal("<tos.img>", options["hatarib_tos"]);
        Assert.Equal("<etos192uk>", GWGUI.Emulation.Atari.Emulators.HatariB.Functions.OptionFunctions.Apply(
            configuration with { Options = new Dictionary<string, string> { ["hatarib_tos"] = "<etos192uk>" } })["hatarib_tos"]);
    }

    [Theory]
    [InlineData("0", "1")]
    [InlineData("1", "0")]
    public void HatariBConvertsTheSharedResetType(string commonValue, string nativeValue)
    {
        var options = GWGUI.Emulation.Atari.Emulators.HatariB.Functions.OptionFunctions.ToNative(
            new Dictionary<string, string> { ["gwgui_atari_reset_type"] = commonValue });
        Assert.Equal(nativeValue, options["hatarib_soft_reset"]);
        Assert.DoesNotContain("gwgui_atari_reset_type", options.Keys);
    }

    [Fact]
    public async Task Atari5200OffersA5200WithoutChangingItsDefaultAdapter()
    {
        using var httpClient = new HttpClient();
        var module = new AtariEmulationModule(Path.GetTempPath(), Path.GetTempPath(),
            httpClient, Path.GetTempPath());
        var configuration = Assert.IsType<MachineConfiguration>(
            module.CreateConfiguration(nameof(MachineModel.Atari5200)));
        Assert.Equal(Emulator.Atari800, configuration.Core);
        Assert.Equal(new[] { "a5200", "atari800" },
            CoreCatalog.GetAll(MachineModel.Atari5200).Select(entry => entry.Id));
        var selected = Assert.IsType<MachineConfiguration>(
            await module.UseEmulatorAsync(configuration, "a5200"));
        Assert.Equal(Emulator.A5200, selected.Core);
        var entry = CoreCatalog.Get(selected.Core);
        Assert.Equal("a5200_libretro.dll", entry.DllName);
        Assert.Equal(new[] { MachineModel.Atari5200 }, entry.Models);
        Assert.Throws<ArgumentException>(() => new MachineConfiguration(
            MachineModel.Atari800Xl, core: Emulator.A5200));
        Assert.Equal("internal", module.RuntimeOptions(selected)["a5200_bios"]);
        Assert.Equal("official", module.RuntimeOptions(selected with
        {
            Firmwares = [new FirmwareConfiguration(FirmwareCategory.Atari5200Bios,
                "5200.rom", false)]
        })["a5200_bios"]);
        Assert.Equal("internal", module.RuntimeOptions(selected with
        {
            Options = new Dictionary<string, string> { ["a5200_bios"] = "internal" },
            Firmwares = [new FirmwareConfiguration(FirmwareCategory.Atari5200Bios,
                "5200.rom", false)]
        })["a5200_bios"]);
    }

    [Theory]
    [InlineData(0, 8)]
    [InlineData(8, 0)]
    [InlineData(1, 9)]
    [InlineData(9, 1)]
    [InlineData(10, 11)]
    [InlineData(11, 13)]
    [InlineData(13, 12)]
    [InlineData(14, 14)]
    public void A5200ConvertsMachineCommandsToNativeButtons(int command, int nativeButton)
    {
        var controller = EmulationControllerState.Empty with
        {
            Buttons = 1u << command,
            LeftX = 1234,
            LeftY = -2345
        };
        var snapshot = new EmulationInputSnapshot(new HashSet<EmulationKey>(),
            EmulationInputSnapshot.Empty.Pointer, [controller]);
        var converted = GWGUI.Emulation.Atari.Emulators.A5200.Functions.InputFunctions.ToNative(snapshot);
        Assert.Equal(1u << nativeButton, converted.Controllers[0].Buttons);
        Assert.Equal(controller.LeftX, converted.Controllers[0].LeftX);
        Assert.Equal(controller.LeftY, converted.Controllers[0].LeftY);
        Assert.Equal(1u << command, snapshot.Controllers[0].Buttons);
    }

    [Fact]
    public void A5200MapsKeypadTwoAndDoesNotExposeUnsupportedControllerPorts()
    {
        var controller = EmulationControllerState.Empty with { Buttons = 1u << 12 };
        var snapshot = new EmulationInputSnapshot(new HashSet<EmulationKey>(),
            EmulationInputSnapshot.Empty.Pointer, [controller, controller, controller, controller]);
        var converted = GWGUI.Emulation.Atari.Emulators.A5200.Functions.InputFunctions.ToNative(snapshot);
        Assert.Equal(2, converted.Controllers.Count);
        Assert.Equal(0u, converted.Controllers[0].Buttons);
        Assert.Equal(0, converted.Controllers[0].RightX);
        Assert.Equal(short.MinValue, converted.Controllers[0].RightY);
    }

    [Fact]
    public async Task Atari2600OffersThreeSeparateStellaAdapters()
    {
        using var httpClient = new HttpClient();
        var module = new AtariEmulationModule(Path.GetTempPath(), Path.GetTempPath(),
            httpClient, Path.GetTempPath());
        var configuration = module.CreateConfiguration(nameof(MachineModel.Atari2600));
        var entries = CoreCatalog.GetAll(MachineModel.Atari2600);
        Assert.Equal(new[] { "stella", "stella2014", "stella2023" }, entries.Select(entry => entry.Id));
        Assert.Equal(3, entries.Select(entry => entry.DllName).Distinct().Count());
        foreach (var entry in entries)
        {
            Assert.Equal(new[] { MachineModel.Atari2600 }, entry.Models);
            var selected = Assert.IsType<MachineConfiguration>(
                await module.UseEmulatorAsync(configuration, entry.Id));
            Assert.Equal(entry.Emulator, selected.Core);
            Assert.Throws<ArgumentException>(() => new MachineConfiguration(
                MachineModel.Atari7800, core: entry.Emulator));
        }
    }

    [Fact]
    public void RenamedStella2023KeepsTheSavedNumericIdentity()
    {
        var original = new MachineConfiguration(MachineModel.Atari2600);
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var json = JsonSerializer.Serialize(original, options);
        using var document = JsonDocument.Parse(json);
        Assert.Equal(2, document.RootElement.GetProperty("core").GetInt32());
        var restored = JsonSerializer.Deserialize<MachineConfiguration>(json, options);
        Assert.NotNull(restored);
        Assert.Equal(Emulator.Stella2023, restored.Core);
        Assert.Equal("stella2023", CoreCatalog.Get(restored.Core).Id);
    }

    [Fact]
    public void MachineConfigurationCrossesTheCoreHostJsonBoundary()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var original = new MachineConfiguration(MachineModel.Atari800Xl);

        var restored = JsonSerializer.Deserialize<MachineConfiguration>(
            JsonSerializer.Serialize(original, options), options);

        Assert.NotNull(restored);
        Assert.Equal(original.Id, restored.Id);
        Assert.Equal(original.Model, restored.Model);
        Assert.Equal(original.Core, restored.Core);
    }

    [Fact]
    public void ConcreteAdaptersUseTheirPhysicalNamespaces()
    {
        var names = typeof(AtariEmulationModule).Assembly.GetTypes().Select(type => type.FullName).ToHashSet();
        Assert.Contains("GWGUI.Emulation.Atari.Emulators.Hatari.Factories.HatariMachineFactory", names);
        Assert.Contains("GWGUI.Emulation.Atari.Emulators.Atari800.Factories.Atari800MachineFactory", names);
        Assert.Contains("GWGUI.Emulation.Atari.Emulators.Stella2023.Factories.Stella2023MachineFactory", names);
        Assert.Contains("GWGUI.Emulation.Atari.Emulators.ProSystem.Factories.ProSystemMachineFactory", names);
        Assert.Contains("GWGUI.Emulation.Atari.Emulators.BeetleLynx.Factories.BeetleLynxMachineFactory", names);
        Assert.Contains("GWGUI.Emulation.Atari.Emulators.VirtualJaguar.Factories.VirtualJaguarMachineFactory", names);
    }

    [Fact]
    public void EngineRegistrationsMatchTheCatalog()
    {
        var field = typeof(Engine).GetField("_adapters", BindingFlags.Instance | BindingFlags.NonPublic);
        var adapters = Assert.IsAssignableFrom<System.Collections.IEnumerable>(field!.GetValue(new Engine()));
        var ids = adapters.Cast<object>().Select(item => item.GetType().GetProperty("Key")!.GetValue(item) as string)
            .Order(StringComparer.Ordinal).ToArray();
        Assert.Equal(CoreCatalog.All.Select(item => item.Id).Order(StringComparer.Ordinal), ids);
        foreach (var entry in CoreCatalog.All)
        {
            var adapter = adapters.Cast<object>().Single(item =>
                string.Equals(item.GetType().GetProperty("Key")!.GetValue(item) as string,
                    entry.Id, StringComparison.Ordinal));
            var value = adapter.GetType().GetProperty("Value")!.GetValue(adapter)!;
            var definition = Assert.IsType<EmulationEmulatorDefinition>(
                value.GetType().GetProperty("Definition")!.GetValue(value));
            Assert.Equal(CoreCatalog.GetDefinition(entry).MachineIds.Order(StringComparer.Ordinal),
                definition.MachineIds.Order(StringComparer.Ordinal));
        }
    }

    [Theory]
    [InlineData("fr-FR", StRegion.France)]
    [InlineData("es-ES", StRegion.Spain)]
    [InlineData("de-DE", StRegion.Germany)]
    [InlineData("en-GB", StRegion.UnitedKingdom)]
    [InlineData("pt-PT", StRegion.UnitedStates)]
    public void AtariStRegionDefaultsToTheApplicationLanguage(string cultureName,
        StRegion expected)
    {
        var previous = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(cultureName);
            Assert.Equal(expected, StModelCatalog.Get(MachineModel.St).DefaultRegion);
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }

    [Fact]
    public void AtariEmulatorsDeclareOnlyTheirHardwareConfigurationDialogs()
    {
        using var httpClient = new HttpClient();
        var module = new AtariEmulationModule(Path.GetTempPath(), Path.GetTempPath(),
            httpClient, Path.GetTempPath());

        foreach (var machine in module.Machines)
        {
            var storage = module.DescribeStorageSettings(module.CreateConfiguration(machine.Id));
            Assert.All(storage.AvailableDevices, device => Assert.Equal(
                device.MediaType switch
                {
                    EmulationMediaType.Floppy => EmulationStorageConfigurationKind.FloppyDrive,
                    EmulationMediaType.HardDisk => EmulationStorageConfigurationKind.HardDiskDrive,
                    _ => EmulationStorageConfigurationKind.None
                }, device.ConfigurationKind));
        }
    }

    [Fact]
    public void AtariComputerKeyboardsListOnlyUniqueMachineKeysWithAutomaticHostBindings()
    {
        using var httpClient = new HttpClient();
        var module = new AtariEmulationModule(Path.GetTempPath(), Path.GetTempPath(),
            httpClient, Path.GetTempPath());
        var tos = Assert.IsType<EmulationInputBindingSet>(module.DescribeInputSettings(
            module.CreateConfiguration(nameof(MachineModel.St))).Keyboard);
        var eightBit = Assert.IsType<EmulationInputBindingSet>(module.DescribeInputSettings(
            module.CreateConfiguration(nameof(MachineModel.Atari800Xl))).Keyboard);

        Assert.DoesNotContain(tos.Definitions, key => key.Id == nameof(EmulationKey.A));
        Assert.DoesNotContain(tos.Definitions, key => key.Id == nameof(EmulationKey.D1));
        Assert.Contains(tos.Definitions, key => key.Id == nameof(EmulationKey.Help));
        Assert.Contains(tos.Definitions, key => key.Id == nameof(EmulationKey.Undo));
        Assert.Contains(tos.Definitions, key => key.Id == nameof(EmulationKey.Break));
        Assert.DoesNotContain(eightBit.Definitions, key => key.Id == nameof(EmulationKey.A));
        Assert.DoesNotContain(eightBit.Definitions, key => key.Id == nameof(EmulationKey.D1));
        Assert.Contains(eightBit.Definitions, key => key.Id == nameof(EmulationKey.AtariOption));
        Assert.Contains(eightBit.Definitions, key => key.Id == nameof(EmulationKey.AtariSelect));
        Assert.Contains(eightBit.Definitions, key => key.Id == nameof(EmulationKey.AtariStart));
        Assert.DoesNotContain(eightBit.Definitions, key => key.Id == nameof(EmulationKey.Numpad0));
        Assert.All(tos.Definitions.Concat(eightBit.Definitions), definition =>
        {
            Assert.True(Enum.TryParse<EmulationKey>(definition.Id, out _));
            Assert.True(Enum.TryParse<EmulationKey>(definition.DefaultBinding, out _));
        });
    }

    [Fact]
    public void AtariConfigurationSummaryDoesNotDisplayAudioState()
    {
        using var httpClient = new HttpClient();
        var module = new AtariEmulationModule(Path.GetTempPath(), Path.GetTempPath(),
            httpClient, Path.GetTempPath());
        var configuration = Assert.IsType<MachineConfiguration>(
            module.CreateConfiguration(nameof(MachineModel.Atari800Xl)));

        Assert.Equal(module.SummarizeConfiguration(configuration with { AudioEnabled = true }).Details,
            module.SummarizeConfiguration(configuration with { AudioEnabled = false }).Details);
    }

    [Fact]
    public void EveryAtariMachineSettingDeclaresLocalizedHelp()
    {
        using var httpClient = new HttpClient();
        var module = new AtariEmulationModule(Path.GetTempPath(), Path.GetTempPath(),
            httpClient, Path.GetTempPath());

        var missing = new HashSet<string>(StringComparer.Ordinal);
        foreach (var machine in module.Machines)
        foreach (var field in module.Describe(machine.Id,
                     module.CreateConfiguration(machine.Id)).Blocks.SelectMany(block => block.Fields))
        {
            if (string.IsNullOrWhiteSpace(field.ExplanationResourceKey))
                missing.Add($"{field.Id}:short-key");
            else if (LocExtension.GetForModule(module, field.ExplanationResourceKey)
                     is var shortHelp && (string.IsNullOrWhiteSpace(shortHelp)
                         || shortHelp == $"[{field.ExplanationResourceKey}]"))
                missing.Add($"{field.Id}:short-text");
            if (string.IsNullOrWhiteSpace(field.DetailedExplanationResourceKey))
                missing.Add($"{field.Id}:detailed-key");
            else if (LocExtension.GetForModule(module, field.DetailedExplanationResourceKey)
                     is var detailedHelp && (string.IsNullOrWhiteSpace(detailedHelp)
                         || detailedHelp == $"[{field.DetailedExplanationResourceKey}]"))
                missing.Add($"{field.Id}:detailed-text");
        }
        Assert.True(missing.Count == 0, string.Join(Environment.NewLine, missing));
    }
}
