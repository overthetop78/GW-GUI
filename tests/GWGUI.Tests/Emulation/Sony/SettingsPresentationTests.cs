using System.Net.Http;
using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Sony.Common.Machines.Common.Constants;
using GWGUI.Emulation.Sony.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Sony.Common.Machines.Common.Functions;
using GWGUI.Emulation.Sony.Modules;

namespace GWGUI.Tests.Emulation.Sony;

public sealed class SettingsPresentationTests
{
    [Theory]
    [InlineData("pcsx2")]
    [InlineData("pcee2")]
    [InlineData("play")]
    public void UnsupportedBuzzKeepsTwentyIndependentAssignmentsAfterSaving(string emulator)
    {
        var configuration = new MachineConfiguration(ModelConstants.PlayStation2, emulator);
        var settings = InputSettingsFunctions.Describe(configuration);
        var usb = settings.ControllerPorts.First(port => port.IsUsb);
        var buzz = Assert.Single(usb.ControllerChoices, choice => choice.Id == "BuzzBuzzers");
        Assert.False(buzz.IsSupportedByEmulator);
        Assert.Equal(20, buzz.BindingDefinitions!.Count);
        Assert.Equal(20, buzz.BindingDefinitions.Select(definition => definition.Id).Distinct().Count());
        Assert.Equal(20, buzz.VisualCommandIds!.Values.Distinct().Count());
        Assert.All(buzz.VisualCommandIds.Values, command => Assert.Contains(buzz.BindingDefinitions, definition => definition.Id == command));
        var values = buzz.BindingDefinitions.Select((definition, index) => (definition.Id, Binding: "Keyboard:F" + (index + 1)))
            .ToDictionary(item => item.Id, item => item.Binding);
        var edited = usb with { SelectedControllerId = buzz.Id, VisualId = EmulationControllerVisualIds.SonyBuzz,
            Bindings = usb.Bindings with { Values = values } };
        var saved = InputSettingsFunctions.Apply(configuration, settings with
        { ControllerPorts = settings.ControllerPorts.Select(port => port.Number == usb.Number ? edited : port).ToArray() });
        var restored = Assert.Single(InputSettingsFunctions.Describe(saved).ControllerPorts, port => port.Number == usb.Number);
        Assert.Equal(buzz.Id, restored.SelectedControllerId);
        Assert.Equal(EmulationControllerVisualIds.SonyBuzz, restored.VisualId);
        Assert.Equal(20, restored.Bindings.Values.Count);
        Assert.All(values, mapping => Assert.Equal(mapping.Value, restored.Bindings.Values[mapping.Key]));
    }

    [Theory]
    [InlineData("pcsx2")]
    [InlineData("pcee2")]
    [InlineData("play")]
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
