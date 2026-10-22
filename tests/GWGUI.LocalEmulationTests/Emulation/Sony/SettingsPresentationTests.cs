using System.Net.Http;
using System.IO;
using System.Globalization;
using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Sony.Common.Machines.Common.Constants;
using GWGUI.Emulation.Sony.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Sony.Common.Machines.Common.Enums;
using GWGUI.Emulation.Sony.Common.Machines.Common.Functions;
using GWGUI.Emulation.Sony.Modules;

namespace GWGUI.Tests.Emulation.Sony;

public sealed class SettingsPresentationTests
{
    [Theory]
    [InlineData("fr-FR", "French", "french")]
    [InlineData("fr-CA", "French", "french")]
    [InlineData("en-US", "English", "english")]
    [InlineData("pt-BR", "Portuguese", "portuguese")]
    [InlineData("ja-JP", "Japanese", "japanese")]
    [InlineData("zh-Hant", "Chinese Traditional", "chinese_trad")]
    [InlineData("zh-TW", "Chinese Traditional", "chinese_trad")]
    [InlineData("zh-HK", "Chinese Traditional", "chinese_trad")]
    [InlineData("zh-Hans", "Chinese Simplified", "chinese_simp")]
    [InlineData("ar-SA", "English", "english")]
    public void SystemLanguageUsesApplicationCultureInsteadOfSavedManualChoice(string culture, string psp, string ps3)
    {
        foreach (var (model, emulator, key, expected) in new[]
        {
            (ModelConstants.Psp, "ppsspp", "ppsspp_language", psp),
            (ModelConstants.PlayStation3, "rpcs3", "rpcs3_language", ps3)
        })
        {
            var original = new MachineConfiguration(model, emulator, Options: new Dictionary<string, string> { [key] = "old-manual-selection" });
            var native = GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions.SystemLanguageFunctions.Configure(original, CultureInfo.GetCultureInfo(culture));
            Assert.Equal(expected, native.Options![key]);
            Assert.Equal("old-manual-selection", original.Options![key]);
            Assert.DoesNotContain(SettingsDescriptionFunctions.Create(original).SelectMany(block => block.Fields), field => field.Id == key);
        }
    }

    [Fact]
    public void Rpcs3CpuGroupsSeparatePrecisionExecutionAndScheduling()
    {
        var blocks = SettingsDescriptionFunctions.Create(new MachineConfiguration(ModelConstants.PlayStation3, "rpcs3"))
            .Where(block => block.Tab == EmulationMachineTab.Cpu).ToArray();
        foreach (var (title, ids) in new[]
        {
            ("Recompilation", new[] { "rpcs3_ppu_decoder", "rpcs3_spu_decoder" }),
            ("PpuAccuracy", new[] { "rpcs3_ppu_set_fpcc", "rpcs3_accurate_dfma" }),
            ("SpuAccuracy", new[] { "rpcs3_spu_xfloat_accuracy", "rpcs3_spu_accurate_dma" }),
            ("SpuExecution", new[] { "rpcs3_spu_loop_detection", "rpcs3_spu_cache", "rpcs3_preferred_spu_threads" }),
            ("Scheduling", new[] { "rpcs3_clocks_scale", "rpcs3_thread_scheduler", "rpcs3_ppu_threads" })
        })
        {
            var block = Assert.Single(blocks, block => block.TitleResourceKey == "Emulation.Sony.Cpu." + title);
            Assert.True(block.IsCompact);
            foreach (var id in ids) Assert.Contains(block.Fields, field => field.Id == id);
        }
        Assert.DoesNotContain(blocks, block => block.TitleResourceKey is "Emulation.Sony.Cpu.Timing" or "Emulation.Sony.Cpu.VectorUnits");
        var fields = blocks.SelectMany(block => block.Fields).ToArray();
        Assert.Equal(fields.Length, fields.Select(field => field.Id).Distinct().Count());
    }

    [Fact]
    public void PspUsesNativeResolutionDimensionsAndDedicatedCheatsTab()
    {
        var configuration = new MachineConfiguration(ModelConstants.Psp, "ppsspp");
        var fields = SettingsDescriptionFunctions.Create(configuration).SelectMany(block => block.Fields).ToArray();
        var resolution = Assert.Single(fields, field => field.Id == "ppsspp_internal_resolution");
        Assert.Equal("1× (480 × 272 px)", Assert.Single(resolution.Choices!, choice => choice.Id == "480x272").InvariantDisplayValue);
        Assert.Equal("2× (960 × 544 px)", Assert.Single(resolution.Choices!, choice => choice.Id == "960x544").InvariantDisplayValue);
        Assert.Equal(EmulationMachineTab.Cheats, Assert.Single(fields, field => field.Id == "ppsspp_cheats").Tab);
        var port = Assert.Single(InputSettingsFunctions.Describe(configuration).ControllerPorts);
        Assert.Equal(16, port.Bindings.Definitions.Count);
        var controller = Assert.Single(port.ControllerChoices, choice => choice.Id == "Joystick");
        Assert.Equal(new[] { EmulationControllerVisualIds.SonyPsp1000, EmulationControllerVisualIds.SonyPsp2000,
            EmulationControllerVisualIds.SonyPsp3000, EmulationControllerVisualIds.SonyPspGo }, controller.CompatibleVisualIds);
        Assert.Equal(EmulationControllerVisualIds.SonyPsp1000, controller.DefaultVisualId);
    }

    [Fact]
    public void MemoryStickRemovalKeepsItsDirectoryAndUpdatesTheNativeInsertionFlag()
    {
        var directory = Path.GetTempPath();
        var configuration = new MachineConfiguration(ModelConstants.Psp, "ppsspp", Options: new Dictionary<string, string>
        { [StorageSettingsFunctionsConstants.MemoryCardPathOptionPrefix + 0] = directory });
        var storage = StorageSettingsFunctions.Describe(configuration);
        var stick = Assert.Single(storage.AvailableDevices, device => device.MediaType == EmulationMediaType.MemoryCard);
        Assert.False(stick.IsPermanent);
        Assert.Equal(EmulationStorageConfigurationKind.None, stick.ConfigurationKind);
        Assert.Contains(stick.Slot, storage.ConfiguredSlots);
        var removed = StorageSettingsFunctions.Apply(configuration, storage with
        { ConfiguredSlots = storage.ConfiguredSlots.Where(slot => slot != stick.Slot).ToArray(), DeviceSettings = [] });
        Assert.Equal("disabled", removed.Options!["ppsspp_memstick_inserted"]);
        Assert.Equal(directory, removed.Options[StorageSettingsFunctionsConstants.MemoryCardPathOptionPrefix + 0]);
        Assert.Equal(directory, GWGUI.Emulation.Sony.Emulators.Ppsspp.Functions.StorageFunctions.ResolveSaveDirectory(removed, "fallback"));
        var restored = StorageSettingsFunctions.Apply(removed, storage);
        Assert.Equal("enabled", restored.Options!["ppsspp_memstick_inserted"]);
        Assert.Equal(directory, restored.Options[StorageSettingsFunctionsConstants.MemoryCardPathOptionPrefix + 0]);
    }

    [Fact]
    public void Rpcs3SeparatesBluRayInternalStorageNetworkAndSystemOptions()
    {
        var configuration = new MachineConfiguration(ModelConstants.PlayStation3, "rpcs3");
        var storage = StorageSettingsFunctions.Describe(configuration);
        var disc = Assert.Single(storage.AvailableDevices, device => device.MediaType == EmulationMediaType.CompactDisc);
        Assert.Equal("Blu-ray", disc.DisplayLabel);
        Assert.Equal("rpcs3_emulate_bdvd_speed", Assert.Single(disc.ConfigurationFields!).Id);
        var hdd = Assert.Single(storage.AvailableDevices, device => device.MediaType == EmulationMediaType.HardDisk);
        Assert.True(hdd.IsPermanent);
        Assert.Contains(hdd.Slot, storage.ConfiguredSlots);
        Assert.Equal(EmulationStorageConfigurationKind.ManagedStorage, hdd.ConfigurationKind);
        Assert.Null(hdd.HardDiskFormats);
        Assert.Equal(new[] { "rpcs3_disk_cache_size", "rpcs3_emulate_hdd_speed" }, hdd.ConfigurationFields!.Select(field => field.Id).Order());
        var cache = hdd.ConfigurationFields.Single(field => field.Id == "rpcs3_disk_cache_size");
        Assert.Equal("Emulation.Storage.Cache.Unlimited", cache.Choices![0].DisplayResourceKey);
        Assert.Null(cache.Choices[0].InvariantDisplayValue);
        var fields = SettingsDescriptionFunctions.Create(configuration).SelectMany(block => block.Fields).ToArray();
        Assert.DoesNotContain(fields, field => field.Id is "rpcs3_disk_cache_size" or "rpcs3_emulate_hdd_speed" or "rpcs3_emulate_bdvd_speed");
        Assert.Equal(EmulationMachineTab.Ethernet, Assert.Single(fields, field => field.Id == "rpcs3_network_enabled").Tab);
        Assert.DoesNotContain(fields, field => field.Id == "rpcs3_language");
        Assert.Equal(EmulationMachineTab.Cheats, Assert.Single(fields, field => field.Id == "rpcs3_database_override").Tab);
        Assert.Equal(EmulationMachineTab.Rom, Assert.Single(fields, field => field.Id == "rpcs3_license_area").Tab);
        Assert.True(Assert.Single(fields, field => field.Id == "rpcs3_enter_button").IsGlobalControllerOption);
        Assert.Equal(EmulationMachineTab.Controllers, Assert.Single(fields, field => field.Id == "rpcs3_enter_button").Tab);
        foreach (var key in new[] { "rpcs3_show_trophy_popups", "rpcs3_show_ppu_compilation_hint", "rpcs3_show_shader_compilation_hint" })
            Assert.Equal(EmulationMachineTab.Video, Assert.Single(fields, field => field.Id == key).Tab);
        Assert.Equal(EmulationMachineTab.Video, Assert.Single(fields, field => field.Id == "rpcs3_disable_hw_blending").Tab);
        var selected = StorageSettingsFunctions.Apply(configuration, storage with
        { DeviceSettings = [new(hdd.Slot, Options: new Dictionary<string, string> { ["rpcs3_disk_cache_size"] = "wipe" })] });
        Assert.Equal("wipe", selected.Options!["rpcs3_disk_cache_size"]);
    }

    [Fact]
    public void Rpcs3ControllerCountPreservesHiddenAssignments()
    {
        var configuration = new MachineConfiguration(ModelConstants.PlayStation3, "rpcs3",
            Input: new InputConfiguration(ControllerBindings: [new ControllerBinding(6, ControllerType.Joystick,
                ButtonMappings: new Dictionary<string, string> { ["B"] = "Keyboard:A" })]));
        var one = InputSettingsFunctions.Describe(configuration);
        Assert.Single(one.ControllerPorts);
        var smaller = InputSettingsFunctions.Apply(configuration, one);
        Assert.Equal("Keyboard:A", smaller.Input!.ControllerBindings!.Single(binding => binding.Port == 6).ButtonMappings!["B"]);
        var expanded = smaller with { Options = new Dictionary<string, string> { ["input.controllerCount"] = "7" } };
        var seven = InputSettingsFunctions.Describe(expanded);
        Assert.Equal(7, seven.ControllerPorts.Count);
        Assert.Equal("Keyboard:A", seven.ControllerPorts[6].Bindings.Values["B"]);
        var selector = Assert.Single(SettingsDescriptionFunctions.Create(expanded).SelectMany(block => block.Fields), field => field.Id == "input.controllerCount");
        Assert.True(selector.IsGlobalControllerOption);
        Assert.True(selector.RefreshSettingsOnChange);
    }

    [Theory]
    [InlineData("disabled", "0,1")]
    [InlineData("port1", "0,1,2,3,4")]
    [InlineData("port2", "0,1,5,6,7")]
    [InlineData("both", "0,1,2,3,4,5,6,7")]
    public void Pcee2MultitapExposesThePhysicalMemoryCardSlots(string mode, string indexes)
    {
        var configuration = new MachineConfiguration(ModelConstants.PlayStation2, "pcee2",
            Options: new Dictionary<string, string> { ["pcsx2_multitap"] = mode });
        var expected = indexes.Split(',').Select(int.Parse).ToArray();
        var cards = StorageSettingsFunctions.Describe(configuration).AvailableDevices
            .Where(device => device.MediaType == EmulationMediaType.MemoryCard).ToArray();
        Assert.Equal(expected, cards.Select(card => card.Slot.Index));
        Assert.Equal(expected.Select(GWGUI.Emulation.Sony.Emulators.Pcee2.Functions.MemoryCardFunctions.NativeFileName),
            GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions.MemoryCardFunctions.NativeFileNames(configuration));
        Assert.All(cards, card => Assert.Equal(
            GWGUI.Emulation.Sony.Emulators.Pcee2.Functions.MemoryCardFunctions.DisplayLabel(card.Slot.Index), card.DisplayLabel));
        Assert.All(cards, card => Assert.Null(card.DisplayLabelResourceKey));
    }

    [Fact]
    public void Pcee2MultitapCardsUseTheMatchingIniKeysAndKeepUnrelatedSettings()
    {
        var configuration = new MachineConfiguration(ModelConstants.PlayStation2, "pcee2",
            Options: new Dictionary<string, string>
            {
                ["pcsx2_multitap"] = "port2",
                [StorageSettingsFunctionsConstants.MemoryCardEnabledOptionPrefix + 5] = "True",
                [StorageSettingsFunctionsConstants.MemoryCardPathOptionPrefix + 5] = "selected.ps2"
            });
        var lines = GWGUI.Emulation.Sony.Emulators.Pcee2.Functions.MemoryCardFunctions.BuildSettings(configuration,
            ["[MemoryCards]", "Slot1_Enable = true", "Multitap2_Slot2_Enable = false", "[Other]", "Keep = yes"]);
        Assert.Contains("Slot1_Enable = true", lines);
        Assert.Contains("[Other]", lines);
        Assert.Contains("Keep = yes", lines);
        Assert.Contains("Multitap2_Slot2_Enable = true", lines);
        Assert.Contains("Multitap2_Slot2_Filename = Mcd-Multitap2-Slot02.ps2", lines);
        Assert.Contains("Multitap1_Slot2_Enable = false", lines);
        Assert.Contains("Multitap2_Slot3_Enable = false", lines);
        Assert.Single(lines, line => line.StartsWith("Multitap2_Slot2_Enable =", StringComparison.Ordinal));
        var disabled = configuration with { Options = new Dictionary<string, string> { ["pcsx2_multitap"] = "disabled" } };
        var cleared = GWGUI.Emulation.Sony.Emulators.Pcee2.Functions.MemoryCardFunctions.BuildSettings(disabled, lines);
        Assert.Contains("Multitap2_Slot2_Enable = false", cleared);
        Assert.DoesNotContain("Multitap2_Slot2_Enable = true", cleared);
    }

    [Theory]
    [InlineData("pcsx2")]
    [InlineData("pcee2")]
    [InlineData("play")]
    public void ControllerChoicesContainOnlySupportedDevices(string emulator)
    {
        var configuration = new MachineConfiguration(ModelConstants.PlayStation2, emulator);
        var settings = InputSettingsFunctions.Describe(configuration);
        Assert.All(settings.ControllerPorts, port =>
        {
            Assert.DoesNotContain(port.ControllerChoices, choice => choice.Id == "BuzzBuzzers");
            Assert.All(port.ControllerChoices, choice => Assert.True(
                GWGUI.Emulation.Sony.Common.Machines.Common.Dictionaries.ControllerCatalog.IsSupported(
                    configuration, Enum.Parse<GWGUI.Emulation.Sony.Common.Machines.Common.Enums.ControllerType>(choice.Id),
                    port.Number - 1, port.IsUsb)));
            Assert.Contains(port.ControllerChoices, choice => choice.Id == port.SelectedControllerId);
        });
    }

    [Theory]
    [InlineData("pcsx2", "None,Keyboard,Mouse")]
    [InlineData("pcee2", "None,GunCon2")]
    [InlineData("play", "None")]
    public void UsbChoicesComeFromTheEmulatorAdapter(string emulator, string expected)
    {
        var settings = InputSettingsFunctions.Describe(new MachineConfiguration(ModelConstants.PlayStation2, emulator));
        Assert.All(settings.ControllerPorts.Where(port => port.IsUsb),
            port => Assert.Equal(expected.Split(','), port.ControllerChoices.Select(choice => choice.Id)));
    }

    [Fact]
    public void SelectedPcee2GunConIsSentThroughTheNativeLightgunOption()
    {
        var configuration = new MachineConfiguration(ModelConstants.PlayStation2, "pcee2",
            Input: new InputConfiguration(ControllerBindings:
                [new ControllerBinding(0, GWGUI.Emulation.Sony.Common.Machines.Common.Enums.ControllerType.GunCon2),
                 new ControllerBinding(1, GWGUI.Emulation.Sony.Common.Machines.Common.Enums.ControllerType.None)]));
        var native = GWGUI.Emulation.Sony.Emulators.Pcee2.Functions.ControllerConfigurationFunctions.Configure(configuration);
        Assert.Equal(GWGUI.Emulation.Sony.Emulators.Pcee2.Constants.OptionConstants.LightgunUsb1,
            native.Options![GWGUI.Emulation.Sony.Emulators.Pcee2.Constants.OptionConstants.LightgunKey]);
        var route = Assert.Single(GWGUI.Emulation.Sony.Emulators.Pcee2.Functions.ControllerConfigurationFunctions.CreatePorts(native),
            port => port.ConfigurationPort == 0);
        Assert.Equal(0u, route.NativePort);
        Assert.False(route.CanChangeWhileRunning);
    }

    [Theory]
    [InlineData(GWGUI.Emulation.Sony.Common.Machines.Common.Enums.ControllerType.Keyboard, "USB Keyboard", 3u)]
    [InlineData(GWGUI.Emulation.Sony.Common.Machines.Common.Enums.ControllerType.Mouse, "USB Mouse", 2u)]
    public void SelectedLrps2UsbDeviceIsSentAsTheAdvertisedNativeType(
        GWGUI.Emulation.Sony.Common.Machines.Common.Enums.ControllerType type, string name, uint id)
    {
        var configuration = new MachineConfiguration(ModelConstants.PlayStation2, "pcsx2",
            Input: new InputConfiguration(UsbBindings: [new ControllerBinding(0, type)]));
        var native = InputSettingsFunctions.NativeUsbType(configuration, 0,
            GWGUI.Emulation.Sony.Common.Machines.Common.Enums.ControllerType.Joystick);
        Assert.Equal(type, native);
        IReadOnlyList<IReadOnlyList<GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts.ControllerDevice>> devices =
            [[new GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts.ControllerDevice(name, id)]];
        Assert.Equal(id, GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions.ControllerPortFunctions.Resolve(native, 0, devices));
    }

    [Theory]
    [InlineData("pcsx2")]
    public void UsbKeyboardAndMouseUseTheirDedicatedTabs(string emulator)
    {
        var configuration = new MachineConfiguration(ModelConstants.PlayStation2, emulator);
        var original = InputSettingsFunctions.Describe(configuration);
        Assert.False(InputSettingsFunctions.HasUsbKeyboard(configuration));
        Assert.False(InputSettingsFunctions.HasUsbMouse(configuration));
        var usbNumber = 0;
        var edited = original with { ControllerPorts = original.ControllerPorts.Select(port => !port.IsUsb ? port
            : port with { SelectedControllerId = usbNumber++ == 0 ? "Keyboard" : "Mouse" }).ToArray() };
        var saved = InputSettingsFunctions.Apply(configuration, edited);
        Assert.True(InputSettingsFunctions.HasUsbKeyboard(saved));
        Assert.True(InputSettingsFunctions.HasUsbMouse(saved));
        var restored = InputSettingsFunctions.Describe(saved);
        Assert.NotEmpty(restored.Keyboard!.Definitions);
        Assert.NotEmpty(restored.Mouse!.Definitions);
        Assert.All(restored.ControllerPorts.Where(port => port.IsUsb), port => Assert.Empty(port.Bindings.Definitions));
        var removed = InputSettingsFunctions.Apply(saved, restored with
        { ControllerPorts = restored.ControllerPorts.Select(port => port.IsUsb ? port with { SelectedControllerId = "None" } : port).ToArray() });
        Assert.False(InputSettingsFunctions.HasUsbKeyboard(removed));
        Assert.False(InputSettingsFunctions.HasUsbMouse(removed));
    }

    [Theory]
    [InlineData(ModelConstants.PlayStation, "swanstation")]
    [InlineData(ModelConstants.PlayStation2, "pcsx2")]
    public void ModelSpecificAsciiButtonSurvivesConfigurationRoundTrip(string model, string emulator)
    {
        var configuration = new MachineConfiguration(model, emulator);
        var settings = InputSettingsFunctions.Describe(configuration);
        var port = settings.ControllerPorts.First(item => !item.IsUsb);
        var edited = port with { SelectedControllerId = "Joystick", VisualId = EmulationControllerVisualIds.SonyAsciiSpecializedPad,
            Bindings = port.Bindings with { Values = new Dictionary<string, string> { ["Slow"] = "Keyboard:F1" } } };
        var saved = InputSettingsFunctions.Apply(configuration, settings with
        { ControllerPorts = settings.ControllerPorts.Select(item => item.Number == port.Number ? edited : item).ToArray() });
        var restored = Assert.Single(InputSettingsFunctions.Describe(saved).ControllerPorts, item => item.Number == port.Number);
        Assert.Contains(restored.Bindings.Definitions, definition => definition.Id == "Slow");
        Assert.Equal("Keyboard:F1", restored.Bindings.Values["Slow"]);
    }

    [Theory]
    [InlineData(ModelConstants.PlayStation, "swanstation")]
    [InlineData(ModelConstants.PlayStation2, "pcsx2")]
    [InlineData(ModelConstants.PlayStation2, "pcee2")]
    [InlineData(ModelConstants.PlayStation2, "play")]
    public void DanceMatKeepsAllPrintedControlsAndIndependentDirections(string model, string emulator)
    {
        var port = InputSettingsFunctions.Describe(new MachineConfiguration(model, emulator)).ControllerPorts
            .First(item => item.ControllerChoices.Any(choice => choice.Id == "DanceMat"));
        var mat = Assert.Single(port.ControllerChoices, choice => choice.Id == "DanceMat");
        Assert.Equal(EmulationControllerVisualIds.SonyDanceMat, mat.DefaultVisualId);
        Assert.Equal(10, mat.BindingDefinitions!.Count);
        Assert.Equal(10, mat.VisualCommandIds!.Count);
        foreach (var key in new[] { "Up", "Down", "Left", "Right" })
            Assert.Null(Assert.Single(mat.BindingDefinitions, binding => binding.Id == key).OppositeDirectionId);
        Assert.Equal(1u, GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions.ControllerPortFunctions.Resolve(
            GWGUI.Emulation.Sony.Common.Machines.Common.Enums.ControllerType.DanceMat, 0, []));
    }

    [Theory]
    [InlineData("pcsx2", true)]
    [InlineData("pcee2", false)]
    [InlineData("play", false)]
    public void Ps2DvdOnlyOffersConfigurationWhenNativeFieldsExist(string emulator, bool configurable)
    {
        var device = Assert.Single(StorageSettingsFunctions.Describe(new MachineConfiguration(ModelConstants.PlayStation2, emulator))
            .AvailableDevices, item => item.MediaType == EmulationMediaType.CompactDisc);
        Assert.Equal("DVD", device.DisplayLabel);
        Assert.Equal("Emulation.Sony.Storage.DVD.Device", device.TypeResourceKey);
        Assert.True(device.IsPermanent);
        Assert.Equal(configurable ? EmulationStorageConfigurationKind.CompactDiscDrive : EmulationStorageConfigurationKind.None, device.ConfigurationKind);
        Assert.Equal(configurable, device.ConfigurationFields!.Count > 0);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Pcee2EthernetPreservesUnrelatedNativeSettings(bool enabled)
    {
        string[] original = ["[Other]", "Value = keep", "[DEV9/Eth]", "EthEnable = old", "EthApi = old", "Custom = keep", "[Next]", "Value = also keep"];
        var updated = GWGUI.Emulation.Sony.Emulators.Pcee2.Functions.NetworkFunctions.BuildSettings(original, enabled);
        Assert.Contains("EthEnable = " + (enabled ? "true" : "false"), updated);
        Assert.Contains("EthApi = Sockets", updated);
        Assert.Contains("EthDevice = Auto", updated);
        Assert.Contains("InterceptDHCP = true", updated);
        Assert.Contains("Custom = keep", updated);
        Assert.Contains("Value = keep", updated);
        Assert.Contains("Value = also keep", updated);
        Assert.Equal("EthEnable = old", original[3]);
        Assert.Equal(updated, GWGUI.Emulation.Sony.Emulators.Pcee2.Functions.NetworkFunctions.BuildSettings(updated, enabled));
    }

    [Fact]
    public void Pcee2UsesNamedNativeModesAndToggleRecompilers()
    {
        var fields = SettingsDescriptionFunctions.Create(new MachineConfiguration(ModelConstants.PlayStation2, "pcee2"))
            .SelectMany(block => block.Fields).ToArray();
        var deinterlace = Assert.Single(fields, field => field.Id == "pcsx2_deinterlace_mode");
        Assert.Equal(EmulationSettingsEditor.Selection, deinterlace.Editor);
        Assert.Equal(10, deinterlace.Choices!.Count);
        Assert.Equal("Emulation.Sony.Video.Deinterlace.9", Assert.Single(deinterlace.Choices, choice => choice.Id == "9").DisplayResourceKey);
        var clock = Assert.Single(fields, field => field.Id == "pcsx2_ee_cycle_rate");
        Assert.Equal("50 %", Assert.Single(clock.Choices!, choice => choice.Id == "-3").InvariantDisplayValue);
        Assert.Equal("100 %", Assert.Single(clock.Choices!, choice => choice.Id == "0").InvariantDisplayValue);
        foreach (var key in new[] { "cpu_recompiler", "rec_ee", "rec_iop", "rec_vu0", "rec_vu1" })
            Assert.Equal(EmulationSettingsEditor.Toggle, Assert.Single(fields, field => field.Id == "pcsx2_" + key).Editor);
        Assert.Equal(EmulationMachineTab.Ethernet, Assert.Single(fields, field => field.Id == "network.ethernetEnabled").Tab);
    }

    [Theory]
    [InlineData("pcsx_rearmed", "pcsx_rearmed_multitap", "disabled", false, false)]
    [InlineData("pcsx_rearmed", "pcsx_rearmed_multitap", "port 1", true, false)]
    [InlineData("pcsx_rearmed", "pcsx_rearmed_multitap", "port 2", false, true)]
    [InlineData("pcsx_rearmed", "pcsx_rearmed_multitap", "ports 1 and 2", true, true)]
    [InlineData("swanstation", "swanstation_ControllerPorts_MultitapMode", "Disabled", false, false)]
    [InlineData("swanstation", "swanstation_ControllerPorts_MultitapMode", "Port1Only", true, false)]
    [InlineData("swanstation", "swanstation_ControllerPorts_MultitapMode", "Port2Only", false, true)]
    [InlineData("swanstation", "swanstation_ControllerPorts_MultitapMode", "BothPorts", true, true)]
    [InlineData("pcee2", "pcsx2_multitap", "disabled", false, false)]
    [InlineData("pcee2", "pcsx2_multitap", "port1", true, false)]
    [InlineData("pcee2", "pcsx2_multitap", "port2", false, true)]
    [InlineData("pcee2", "pcsx2_multitap", "both", true, true)]
    public void MultitapUsesTwoSwitchesAndRoundTripsNativeValues(string emulator, string key, string native, bool first, bool second)
    {
        var configuration = new MachineConfiguration(emulator == "pcee2" ? ModelConstants.PlayStation2 : ModelConstants.PlayStation,
            emulator, Options: new Dictionary<string, string> { [key] = native });
        var fields = SettingsDescriptionFunctions.Create(configuration).SelectMany(block => block.Fields).ToArray();
        Assert.DoesNotContain(fields, field => field.Id == key);
        var port1 = Assert.Single(fields, field => field.Id == key + ".port1");
        var port2 = Assert.Single(fields, field => field.Id == key + ".port2");
        Assert.Equal(EmulationSettingsEditor.Toggle, port1.Editor);
        Assert.Equal(EmulationSettingsEditor.Toggle, port2.Editor);
        Assert.Equal(first ? EmulationSettingConstants.Enabled : EmulationSettingConstants.Disabled, port1.Value);
        Assert.Equal(second ? EmulationSettingConstants.Enabled : EmulationSettingConstants.Disabled, port2.Value);
        using var http = new HttpClient();
        var module = new SonyEmulationModule(Environment.CurrentDirectory, Environment.CurrentDirectory, http, Environment.CurrentDirectory);
        var updated = Assert.IsType<MachineConfiguration>(module.ApplySettings(configuration, new Dictionary<string, string?>
        {
            [port1.Id] = port1.Value, [port2.Id] = port2.Value
        }));
        Assert.Equal(native, updated.Options![key]);
        Assert.DoesNotContain(updated.Options.Keys, option => option.StartsWith(key + ".port", StringComparison.Ordinal));
    }

    [Fact]
    public void CdReadAheadNamesWholeDiscAndPreservesSectorCounts()
    {
        var fields = StorageSettingsFunctions.Describe(new MachineConfiguration(ModelConstants.PlayStation, "pcsx_rearmed"))
            .AvailableDevices.Single(device => device.MediaType == EmulationMediaType.CompactDisc).ConfigurationFields!;
        var field = Assert.Single(fields, item => item.Id == "pcsx_rearmed_cd_readahead");
        Assert.Equal("Visual.SectorsUnit", field.ValueUnitResourceKey);
        Assert.Equal("12", field.Value);
        Assert.Equal("12", Assert.Single(field.Choices!, choice => choice.Id == "12").InvariantDisplayValue);
        var whole = Assert.Single(field.Choices!, choice => choice.Id == "333000");
        Assert.Equal("Emulation.Sony.CD.ReadWholeDisc", whole.DisplayResourceKey);
        Assert.Null(whole.InvariantDisplayValue);
        Assert.Equal("Emulation.Option.Sony.Value.Disabled", Assert.Single(field.Choices!, choice => choice.Id == "0").DisplayResourceKey);
        var swan = StorageSettingsFunctions.Describe(new MachineConfiguration(ModelConstants.PlayStation, "swanstation"))
            .AvailableDevices.Single(device => device.MediaType == EmulationMediaType.CompactDisc).ConfigurationFields!
            .Single(item => item.Id == "swanstation_CDROM_ReadaheadSectors");
        Assert.Equal("Visual.SectorsUnit", swan.ValueUnitResourceKey);
        Assert.True(swan.UseChoiceLabelsForSlider);
    }

    [Fact]
    public void BeetleHardwareSeparatesFiltersTexturePacksAndCaches()
    {
        var blocks = SettingsDescriptionFunctions.Create(new MachineConfiguration(ModelConstants.PlayStation, "mednafen_psx_hw"));
        var filters = Assert.Single(blocks, block => block.TitleResourceKey == "Emulation.Sony.Video.ImageFilters");
        Assert.Contains(filters.Fields, field => field.Id == "beetle_psx_hw_msaa");
        Assert.Contains(filters.Fields, field => field.Id == "beetle_psx_hw_super_sampling");
        var packs = Assert.Single(blocks, block => block.TitleResourceKey == "Emulation.Sony.Video.TexturePacks");
        Assert.Contains(packs.Fields, field => field.Id == "beetle_psx_hw_hd_dump_mode");
        var cache = Assert.Single(blocks, block => block.TitleResourceKey == "Emulation.Sony.Video.TextureCache");
        Assert.Contains(cache.Fields, field => field.Id == "beetle_psx_hw_hd_cache_vram_budget");
        Assert.Contains(cache.Fields, field => field.Id == "beetle_psx_hw_hd_cache_ram_budget");
    }
}
