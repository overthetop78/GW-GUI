using System.IO;
using GWGUI.App.Functions.Views.Emulation.Settings;
using GWGUI.Emulation;
using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Sony.Common.Dictionaries;
using GWGUI.Emulation.Sony.Common.Machines.Common.Constants;
using GWGUI.Emulation.Sony.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Sony.Common.Machines.Common.Dictionaries;
using GWGUI.Emulation.Sony.Common.Machines.Common.Enums;
using GWGUI.Emulation.Sony.Common.Machines.Common.Functions;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;
using Swan = GWGUI.Emulation.Sony.Emulators.SwanStation.Constants.CoreConstants;
using SwanOptions = GWGUI.Emulation.Sony.Emulators.SwanStation.Constants.OptionConstants;
using Beetle = GWGUI.Emulation.Sony.Emulators.BeetlePsx.Constants.CoreConstants;
using BeetleHw = GWGUI.Emulation.Sony.Emulators.BeetlePsxHw.Constants.CoreConstants;
using Rearmed = GWGUI.Emulation.Sony.Emulators.PcsxRearmed.Constants.CoreConstants;
using Ps1Firmware = GWGUI.Emulation.Sony.Common.Machines.PlayStation.Constants.FirmwareConstants;
using CardFormatter = GWGUI.Emulation.Sony.Common.Machines.PlayStation.Functions.MemoryCardFunctions;

namespace GWGUI.Tests.Emulation.Sony;

public sealed class ConfigurationPresentationTests
{
    [Theory]
    [InlineData(Swan.Id)]
    [InlineData(Beetle.Id)]
    [InlineData(BeetleHw.Id)]
    [InlineData(Rearmed.Id)]
    public void GeneralContainsOnlyMachineInformationAndRomHasOneBios(string emulator)
    {
        var configuration = Configuration(emulator);
        var blocks = SettingsDescriptionFunctions.Create(configuration);
        Assert.All(blocks.Where(block => block.Tab == EmulationMachineTab.General).SelectMany(block => block.Fields), field => Assert.Equal(EmulationSettingsEditor.Information, field.Editor));
        var paths = blocks.Where(block => block.Tab == EmulationMachineTab.Rom).SelectMany(block => block.Fields).Where(field => field.Editor == EmulationSettingsEditor.Path);
        Assert.Equal(Ps1Firmware.SelectedField, Assert.Single(paths).Id);
        Assert.Contains(blocks.Where(block => block.Tab == EmulationMachineTab.Cpu).SelectMany(block => block.Fields), field => field.Editor == EmulationSettingsEditor.Selection);
    }

    [Theory]
    [InlineData(Swan.Id, SwanOptions.BIOSPatchFastBootKey)]
    [InlineData(Beetle.Id, GWGUI.Emulation.Sony.Emulators.BeetlePsx.Constants.OptionConstants.SkipBiosKey)]
    [InlineData(BeetleHw.Id, GWGUI.Emulation.Sony.Emulators.BeetlePsxHw.Constants.OptionConstants.SkipBiosKey)]
    [InlineData(Rearmed.Id, GWGUI.Emulation.Sony.Emulators.PcsxRearmed.Constants.OptionConstants.ShowBiosBootlogoKey)]
    public void BiosAnimationUsesCheckbox(string emulator, string key) =>
        Assert.Equal(EmulationSettingsEditor.Toggle, Fields(emulator).Single(field => field.Id == key).Editor);

    [Fact]
    public void SelectedBiosReplacesAllPreviousSelections()
    {
        var configuration = Configuration(Swan.Id) with
        {
            FirmwarePaths = new Dictionary<string, string>
            {
                [Ps1Firmware.JapanField] = "japan.bin",
                [Ps1Firmware.EuropeField] = "europe.bin"
            }
        };
        var adapter = EmulatorCatalog.CreateAdapters().Single(item => item.EmulatorId == Swan.Id);
        var result = FirmwareConfigurationFunctions.Apply(configuration, new Dictionary<string, string?> { [Ps1Firmware.SelectedField] = "scph1002.rom" }, adapter);
        Assert.Equal("scph1002.rom", Assert.Single(result.FirmwarePaths!).Value);
    }

    [Fact]
    public void Ps1OffersTwoCreateableMemoryCardsAndCd()
    {
        var storage = StorageSettingsFunctions.Describe(Configuration(Swan.Id));
        var cards = storage.AvailableDevices.Where(device => device.MediaType == EmulationMediaType.MemoryCard).ToArray();
        Assert.Equal(2, cards.Length);
        Assert.Equal(new[] { "MC 1", "MC 2" }, cards.Select(card => card.DisplayLabel));
        Assert.All(cards, card =>
        {
            Assert.Equal(EmulationStorageConfigurationKind.MemoryCard, card.ConfigurationKind);
            Assert.Equal(128 * 1024, card.FixedImageSizeBytes);
            Assert.NotEmpty(card.HardDiskFormats!);
            Assert.All(card.HardDiskFormats!, format => Assert.NotNull(format.InitializeBlankImage));
        });
        Assert.Equal("CD", Assert.Single(storage.AvailableDevices, device => device.MediaType == EmulationMediaType.CompactDisc).DisplayLabel);
    }

    [Fact]
    public void BlankCardHasValidHeaderFreeBlocksAndChecksums()
    {
        using var image = new MemoryStream(new byte[128 * 1024], writable: true);
        CardFormatter.InitializeBlank(image);
        var data = image.ToArray();
        Assert.Equal((byte)'M', data[0]);
        Assert.Equal((byte)'C', data[1]);
        for (var frame = 0; frame <= 35; frame++)
        {
            byte checksum = 0;
            for (var offset = 0; offset < 128; offset++) checksum ^= data[frame * 128 + offset];
            Assert.Equal(0, checksum);
            if (frame is >= 1 and <= 15) Assert.Equal(0xA0, data[frame * 128]);
        }
        Assert.Equal(data[..128], data[(63 * 128)..(64 * 128)]);
    }

    [Fact]
    public void FixedRamAndDevelopmentExtensionUseBytes()
    {
        var fields = Fields(Swan.Id);
        var ram = SettingsDescriptionFunctions.Create(Configuration(Swan.Id)).Where(block => block.Tab == EmulationMachineTab.Ram).SelectMany(block => block.Fields);
        Assert.Equal(2 * 1024 * 1024, ram.Sum(EmulationSettingsValuePresentationFunctions.DefaultNumericValue));
        var extension = fields.Single(field => field.Id == SwanOptions.ConsoleEnable8MBRAMKey);
        Assert.Equal(EmulationSettingsEditor.Toggle, extension.Editor);
        Assert.Equal(6 * 1024 * 1024, EmulationSettingsValuePresentationFunctions.DefaultNumericValue(extension with { Value = extension.EnabledValue }));
    }

    [Theory]
    [InlineData("512 KiB", 524288)]
    [InlineData("2 MiB", 2097152)]
    [InlineData("8 MB", 8388608)]
    public void MemoryTotalsRecognizeExistingModuleUnits(string display, long bytes) =>
        Assert.Equal(bytes, EmulationSettingsValuePresentationFunctions.MemoryBytes(display));

    [Fact]
    public void DeviceSpecificSettingsAreFilteredByType()
    {
        var fields = Fields(Rearmed.Id).Where(field => field.Tab == EmulationMachineTab.Controllers).ToArray();
        Assert.DoesNotContain(fields, field => field.Id == GWGUI.Emulation.Sony.Emulators.PcsxRearmed.Constants.OptionConstants.ShowInputSettingsKey);
        var negcon = fields.First(field => field.Id.Contains("negcon", StringComparison.Ordinal));
        Assert.Contains(nameof(ControllerType.NeGcon), negcon.AppliesToControllerIds!);
        Assert.DoesNotContain(nameof(ControllerType.DualShock), negcon.AppliesToControllerIds!);
        var gun = fields.First(field => field.Id.Contains("guncon", StringComparison.Ordinal));
        Assert.Contains(nameof(ControllerType.GunCon), gun.AppliesToControllerIds!);
        Assert.DoesNotContain(nameof(ControllerType.Joystick), gun.AppliesToControllerIds!);
    }

    [Fact]
    public void AnalogButtonAndPeripheralArtworkAreAvailable()
    {
        var input = InputSettingsFunctions.Describe(Configuration(Beetle.Id));
        var choices = input.ControllerPorts.First().ControllerChoices;
        var dualShock = choices.Single(choice => choice.Id == nameof(ControllerType.DualShock));
        Assert.Equal(EmulationControllerVisualIds.SonyDualShock1, dualShock.DefaultVisualId);
        foreach (var type in new[] { ControllerType.AnalogController, ControllerType.AnalogJoystick,
                     ControllerType.Mouse, ControllerType.NeGcon, ControllerType.GunCon, ControllerType.Justifier })
            Assert.NotEmpty(choices.Single(choice => choice.Id == type.ToString()).CompatibleVisualIds!);
        var state = EmulationControllerState.Empty with
        {
            Controls = new EmulationControllerControls(new Dictionary<string, float> { [InputSettingsFunctionsConstants.Analog] = 1f })
        };
        Assert.NotEqual(0u, AnalogToggleFunctions.ButtonMask(state, ControllerType.DualShock));
        Assert.Equal(0u, AnalogToggleFunctions.ButtonMask(state, ControllerType.Joystick));
    }

    [Theory]
    [InlineData(Swan.Id)]
    [InlineData(Beetle.Id)]
    [InlineData(BeetleHw.Id)]
    [InlineData(Rearmed.Id)]
    public void CpuClockIsGroupedAndDiscSettingsBelongToDisc(string emulator)
    {
        var configuration = Configuration(emulator);
        var blocks = SettingsDescriptionFunctions.Create(configuration);
        Assert.Single(blocks, block => block.Id == "cpu");
        var acceleration = Assert.Single(blocks, block => block.Id == "acceleration");
        var frequency = Assert.Single(acceleration.Fields, field => field.FrequencyScaleFieldId is not null);
        Assert.Contains(acceleration.Fields, field => field.Id == frequency.FrequencyScaleFieldId);
        Assert.Equal(33_868_800L, frequency.NumericValue);
        Assert.All(blocks.Where(block => block.Tab is EmulationMachineTab.Cpu or EmulationMachineTab.Video or EmulationMachineTab.Audio).SelectMany(block => block.Fields), field =>
        {
            Assert.False(string.IsNullOrWhiteSpace(field.ExplanationResourceKey), field.Id);
            Assert.False(string.IsNullOrWhiteSpace(field.DetailedExplanationResourceKey), field.Id);
        });
        Assert.DoesNotContain(blocks, block => block.Tab == EmulationMachineTab.Storage);
        var storage = StorageSettingsFunctions.Describe(configuration);
        Assert.Equal(EmulationMediaSlot.Cd0, storage.AvailableDevices[0].Slot);
        Assert.Equal(EmulationStorageConfigurationKind.CompactDiscDrive, storage.AvailableDevices[0].ConfigurationKind);
        Assert.NotEmpty(storage.AvailableDevices[0].ConfigurationFields!);
    }

    [Fact]
    public void ClockPercentageChangesDisplayedFrequency()
    {
        Assert.Equal(33.8688.ToString("0.####") + " MHz", EmulationSettingsValuePresentationFunctions.ScaledFrequency(33_868_800, "100%"));
        Assert.Equal(37.25568.ToString("0.####") + " MHz", EmulationSettingsValuePresentationFunctions.ScaledFrequency(33_868_800, "110"));
        Assert.Null(EmulationSettingsValuePresentationFunctions.ScaledFrequency(33_868_800, "auto"));
    }

    [Fact]
    public void SwanPeripheralOptionsDoNotLeakIntoStandardControllers()
    {
        var fields = Fields(Swan.Id);
        var gun = fields.Single(field => field.Id == SwanOptions.Controller1XScaleKey);
        Assert.Equal([nameof(ControllerType.GunCon)], gun.AppliesToControllerIds);
        var steering = fields.Single(field => field.Id == SwanOptions.Controller1SteeringDeadzoneKey);
        Assert.DoesNotContain(nameof(ControllerType.DualShock), steering.AppliesToControllerIds!);
        Assert.DoesNotContain(nameof(ControllerType.AnalogJoystick), steering.AppliesToControllerIds!);
        var multitap = fields.Single(field => field.Id == SwanOptions.ControllerPortsMultitapModeKey);
        Assert.True(multitap.IsGlobalControllerOption);
        Assert.True(multitap.RefreshSettingsOnChange);
        var bothPorts = Configuration(Swan.Id) with { Options = new Dictionary<string, string> { [SwanOptions.ControllerPortsMultitapModeKey] = SwanOptions.ControllerPortsMultitapModeBothPorts } };
        var eight = InputSettingsFunctions.Describe(bothPorts);
        Assert.Equal(8, eight.ControllerPorts.Count);
        var reduced = InputSettingsFunctions.Apply(Configuration(Swan.Id), eight);
        Assert.Equal(2, InputSettingsFunctions.Describe(reduced).ControllerPorts.Count);
    }

    [Fact]
    public void DiscOptionsRoundTripThroughItsConfiguration()
    {
        var configuration = Configuration(Swan.Id);
        var storage = StorageSettingsFunctions.Describe(configuration);
        var selected = storage with { DeviceSettings = [new EmulationStorageDeviceSettings(
            EmulationMediaSlot.Cd0, CompactDisc: new EmulationCompactDiscDriveSettings(Options: new Dictionary<string, string> { [SwanOptions.CDROMMuteCDAudioKey] = "true" }))] };
        var result = StorageSettingsFunctions.Apply(configuration, selected);
        Assert.Equal("true", result.Options![SwanOptions.CDROMMuteCDAudioKey]);
    }

    [Theory]
    [InlineData(Beetle.Id)]
    [InlineData(BeetleHw.Id)]
    public void AnalogButtonHasNoSecondModeSelector(string emulator)
    {
        var fields = Fields(emulator);
        Assert.DoesNotContain(fields, field => field.Id.EndsWith("_analog_toggle", StringComparison.Ordinal));
        var calibration = fields.Single(field => field.Id.EndsWith("_analog_calibration", StringComparison.Ordinal));
        Assert.Equal("Emulation.Option.Sony.Controller.AnalogCalibration.Short", calibration.ExplanationResourceKey);
        Assert.Contains(nameof(ControllerType.DualShock), calibration.AppliesToControllerIds!);
        Assert.Contains(nameof(ControllerType.NeGcon), calibration.AppliesToControllerIds!);
        Assert.DoesNotContain(nameof(ControllerType.Joystick), calibration.AppliesToControllerIds!);
    }

    private static MachineConfiguration Configuration(string emulator) => new(ModelConstants.PlayStation, emulator);

    [Fact]
    public void VitaWithoutAnAdapterDoesNotDescribeNativeInputs()
    {
        var configuration = new MachineConfiguration(ModelConstants.PsVita, string.Empty);
        Assert.NotEmpty(SettingsDescriptionFunctions.Create(configuration));
        var input = InputSettingsFunctions.Describe(configuration);
        Assert.Empty(input.ControllerPorts);
        Assert.Null(input.Keyboard);
        Assert.Null(input.Mouse);
    }

    [Theory]
    [InlineData(Swan.Id)]
    [InlineData(Beetle.Id)]
    [InlineData(BeetleHw.Id)]
    public void NativeOptionsKeepTheCommonAudioAndVideoControlsSeparate(string emulator)
    {
        var blocks = SettingsDescriptionFunctions.Create(Configuration(emulator));
        var audio = Assert.Single(blocks, block => block.Id == "audio");
        Assert.Equal(new[] { SettingsConstants.AudioEnabled, SettingsConstants.AudioOutput, SettingsConstants.AudioLatency }, audio.Fields.Select(field => field.Id));
        Assert.False(audio.IsCompact);
        Assert.Contains(blocks, block => block.Tab == EmulationMachineTab.Audio && block.Id != audio.Id);
        var basic = blocks.Where(block => block.Tab == EmulationMachineTab.Video && !block.IsCollapsible).ToArray();
        Assert.NotEmpty(basic);
        Assert.DoesNotContain(basic.SelectMany(block => block.Fields), field => field.Id.Contains("crop", StringComparison.OrdinalIgnoreCase) || field.Id.Contains("PGXP", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(blocks, block => block.Tab == EmulationMachineTab.Video && block.IsCollapsible);
        Assert.DoesNotContain(blocks.SelectMany(block => block.Fields), field => field.Id == SwanOptions.GPUTextureFilterKey || field.Id == GWGUI.Emulation.Sony.Emulators.BeetlePsxHw.Constants.OptionConstants.FilterKey);
    }

    [Theory]
    [InlineData(Beetle.Id, false, false, 2)]
    [InlineData(Beetle.Id, true, false, 5)]
    [InlineData(Beetle.Id, false, true, 5)]
    [InlineData(Beetle.Id, true, true, 8)]
    [InlineData(BeetleHw.Id, true, true, 8)]
    public void BeetleMultitapsExposeCorrespondingMemoryCards(string emulator, bool first, bool second, int count)
    {
        var prefix = emulator == Beetle.Id ? "beetle_psx_" : "beetle_psx_hw_";
        var configuration = Configuration(emulator) with { Options = new Dictionary<string, string>
        {
            [prefix + "enable_multitap_port1"] = first ? "enabled" : "disabled",
            [prefix + "enable_multitap_port2"] = second ? "enabled" : "disabled"
        } };
        var cards = StorageSettingsFunctions.Describe(configuration).AvailableDevices.Where(device => device.MediaType == EmulationMediaType.MemoryCard).ToArray();
        Assert.Equal(count, cards.Length);
        Assert.Equal(Enumerable.Range(1, count).Select(index => "MC " + index), cards.Select(card => card.DisplayLabel));
        var names = GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions.MemoryCardFunctions.NativeFileNames(configuration);
        Assert.Equal(count, names.Count);
        Assert.Equal(Enumerable.Range(0, count).Select(index => "mednafen_psx_libretro_shared." + index + ".mcr"), names);
    }
    [Theory]
    [InlineData("pcee2")]
    [InlineData("pcsx2")]
    public void Ps2StorageShowsFixedDiscThenTwoRealMemoryCards(string emulator)
    {
        var configuration = new MachineConfiguration(ModelConstants.PlayStation2, emulator);
        var storage = StorageSettingsFunctions.Describe(configuration);
        Assert.Equal(EmulationMediaSlot.Cd0, storage.ConfiguredSlots[0]);
        var cards = storage.AvailableDevices.Where(device => device.MediaType == EmulationMediaType.MemoryCard).ToArray();
        Assert.Equal(new[] { "MC 1", "MC 2" }, cards.Select(card => card.DisplayLabel));
        Assert.All(cards, card => Assert.Contains(card.Slot, storage.ConfiguredSlots));
        if (emulator == "play")
            Assert.All(cards, card => Assert.True(card.UsesDirectory));
        else
        {
            Assert.All(cards, card => Assert.Equal(8_650_752, card.FixedImageSizeBytes));
            Assert.All(cards, card => Assert.Equal(new[] { ".ps2" }, card.AcceptedExtensions));
        }
        Assert.NotEmpty(SettingsDescriptionFunctions.Create(configuration));
        Assert.NotEmpty(InputSettingsFunctions.Describe(configuration).ControllerPorts);
    }

    [Fact]
    public void Ps2BlankCardMatchesErasedFlashIncludingSpareBytes()
    {
        using var image = new MemoryStream();
        StorageSettingsFunctions.InitializeBlankPs2Card(image);
        Assert.Equal(8_650_752, image.Length);
        Assert.True(image.GetBuffer().AsSpan(0, checked((int)image.Length)).IndexOfAnyExcept((byte)0xFF) < 0);
    }

    [Fact]
    public void Pcee2MultitapsExpandControllersButOnlyTwoNativeCardSelectorsExist()
    {
        var configuration = new MachineConfiguration(ModelConstants.PlayStation2, "pcee2", Options: new Dictionary<string, string> { ["pcsx2_multitap"] = "both" });
        Assert.Equal(8, InputSettingsFunctions.Describe(configuration).ControllerPorts.Count);
        Assert.Equal(2, StorageSettingsFunctions.Describe(configuration).AvailableDevices.Count(device => device.MediaType == EmulationMediaType.MemoryCard));
        Assert.Equal(2, GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions.MemoryCardFunctions.NativeFileNames(configuration).Count);
    }

    [Theory]
    [InlineData("psp_1000", "32768 KiB")]
    [InlineData("psp_2000_3000", "65536 KiB")]
    public void PspModelChangesRamAndKeepsNativeOptionsInTheirTabs(string model, string memory)
    {
        var configuration = new MachineConfiguration(ModelConstants.Psp, "ppsspp", Options: new Dictionary<string, string> { ["ppsspp_psp_model"] = model });
        var blocks = SettingsDescriptionFunctions.Create(configuration);
        var fields = blocks.SelectMany(block => block.Fields).ToArray();
        Assert.Equal(memory, fields.Single(field => field.Id == SettingsConstants.Ram).Value);
        Assert.Equal(EmulationMachineTab.Ram, fields.Single(field => field.Id == "ppsspp_psp_model").Tab);
        Assert.True(fields.Single(field => field.Id == "ppsspp_psp_model").RefreshSettingsOnChange);
        Assert.Equal("acceleration", fields.Single(field => field.Id == "ppsspp_locked_cpu_speed").BlockId);
        Assert.Equal(EmulationMachineTab.General, fields.Single(field => field.Id == "ppsspp_language").Tab);
        Assert.Equal(EmulationMachineTab.Controllers, fields.Single(field => field.Id == "ppsspp_analog_deadzone").Tab);
        Assert.Equal(EmulationSettingsEditor.Toggle, fields.Single(field => field.Id == "ppsspp_fast_memory").Editor);
        Assert.DoesNotContain(fields, field => field.Id == "ppsspp_texture_shader");
        var storage = StorageSettingsFunctions.Describe(configuration);
        Assert.Equal("UMD", storage.AvailableDevices[0].DisplayLabel);
        Assert.Contains(storage.AvailableDevices[0].ConfigurationFields!, field => field.Id == "ppsspp_cache_iso" && field.Editor == EmulationSettingsEditor.Toggle);
        var stick = Assert.Single(storage.AvailableDevices, device => device.MediaType == EmulationMediaType.MemoryCard);
        Assert.True(stick.UsesDirectory);
        Assert.Equal(Path.Combine("Sony", "PSP", "Memory Stick"), stick.DefaultImageSubdirectory);
    }

    [Fact]
    public void Pcsx2UsesItsOwnNativeValuesRatherThanLrps2Options()
    {
        var native = GWGUI.Emulation.Sony.Emulators.Pcsx2.Dictionaries.OptionCatalog.All;
        Assert.Equal(3, native.Count);
        var resolution = native.Single(option => option.Key == "pcsx2_upscale_multiplier");
        Assert.Equal("Native PS2", resolution.DefaultValue);
        Assert.Contains(resolution.Values, choice => choice.Value == "3x Native ~1080p");
        Assert.DoesNotContain(native, option => option.Key == "pcsx2_mtvu");
    }

    private static EmulationSettingsField[] Fields(string emulator) => SettingsDescriptionFunctions.Create(Configuration(emulator)).SelectMany(block => block.Fields).ToArray();

    [Theory]
    [InlineData(ModelConstants.PsVita)]
    [InlineData(ModelConstants.PlayStation3)]
    [InlineData(ModelConstants.PlayStation4)]
    [InlineData(ModelConstants.PlayStation5)]
    public void UnselectedEmulatorPreservesInputsWhenPreviousMachinePortsAreCaptured(string model)
    {
        var previous = InputSettingsFunctions.Describe(Configuration(Swan.Id));
        Assert.NotEmpty(previous.ControllerPorts);
        var configuration = new MachineConfiguration(model, string.Empty);
        Assert.Same(configuration, InputSettingsFunctions.Apply(configuration, previous));
        Assert.Same(configuration, InputSettingsFunctions.Apply(configuration, InputSettingsFunctions.Describe(configuration)));
    }
}
