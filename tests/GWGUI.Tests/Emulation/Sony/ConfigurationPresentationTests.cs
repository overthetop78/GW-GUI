using System.IO;
using System.Net.Http;
using GWGUI.App.Functions.Emulation.Machine;
using GWGUI.VideoPresentation.Enums;
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
    [InlineData("pcee2")]
    [InlineData("pcsx2")]
    public void Ps2SupplementaryRomSelectionsRemainIndependentFromTheBios(string emulator)
    {
        using var http = new HttpClient();
        var module = new GWGUI.Emulation.Sony.Modules.SonyEmulationModule(Environment.CurrentDirectory, Environment.CurrentDirectory, http, Environment.CurrentDirectory);
        var original = new MachineConfiguration(ModelConstants.PlayStation2, emulator,
            FirmwarePaths: new Dictionary<string, string> { ["firmware.system"] = "selected-bios.bin" });
        var selected = Assert.IsType<MachineConfiguration>(module.ApplySettings(original, new Dictionary<string, string?>
        {
            ["firmware.rom1"] = "selected.rom1", ["firmware.rom2"] = "selected.rom2",
            ["firmware.nvram"] = "selected.nvm", ["firmware.mechanics"] = "selected.mec"
        }));
        var fields = SettingsDescriptionFunctions.Create(selected).Where(block => block.Tab == EmulationMachineTab.Rom)
            .SelectMany(block => block.Fields).Where(field => field.Editor == EmulationSettingsEditor.Path).ToArray();
        Assert.Equal("selected-bios.bin", Assert.Single(fields, field => field.Id == "firmware.system").Value);
        Assert.Equal("selected.rom1", Assert.Single(fields, field => field.Id == "firmware.rom1").Value);
        Assert.Equal("selected.rom2", Assert.Single(fields, field => field.Id == "firmware.rom2").Value);
        Assert.Equal("selected.nvm", Assert.Single(fields, field => field.Id == "firmware.nvram").Value);
        Assert.Equal("selected.mec", Assert.Single(fields, field => field.Id == "firmware.mechanics").Value);
        Assert.Equal(5, fields.Length);
        foreach (var field in fields.Where(field => field.Id != "firmware.system"))
        {
            Assert.False(string.IsNullOrWhiteSpace(field.ExplanationResourceKey));
            Assert.False(string.IsNullOrWhiteSpace(field.DetailedExplanationResourceKey));
            Assert.Equal(EmulationDefaultFolderCategory.Firmware, field.DefaultFolderCategory);
        }
        Assert.DoesNotContain(fields, field => field.Id == "firmware.extended");
        var cleared = Assert.IsType<MachineConfiguration>(module.ApplySettings(selected, new Dictionary<string, string?> { ["firmware.rom1"] = null }));
        Assert.False(cleared.FirmwarePaths!.ContainsKey("firmware.rom1"));
        Assert.Equal("selected-bios.bin", cleared.FirmwarePaths["firmware.system"]);
        Assert.Equal("selected.rom2", cleared.FirmwarePaths["firmware.rom2"]);
        Assert.Equal("selected.nvm", cleared.FirmwarePaths["firmware.nvram"]);
        Assert.Equal("selected.mec", cleared.FirmwarePaths["firmware.mechanics"]);
        var automaticNvram = Assert.IsType<MachineConfiguration>(module.ApplySettings(selected, new Dictionary<string, string?> { ["firmware.nvram"] = null }));
        Assert.False(automaticNvram.FirmwarePaths!.ContainsKey("firmware.nvram"));
        Assert.Equal("selected-bios.bin", automaticNvram.FirmwarePaths["firmware.system"]);
        Assert.Equal("selected.rom1", automaticNvram.FirmwarePaths["firmware.rom1"]);
        Assert.Equal("selected.mec", automaticNvram.FirmwarePaths["firmware.mechanics"]);
        Assert.DoesNotContain(selected.Options!.Keys, key => key.StartsWith("firmware.", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("rom1.bin", "firmware.rom1")]
    [InlineData("ROM1.BIN", "firmware.rom1")]
    [InlineData("console.rom1", "firmware.rom1")]
    [InlineData("console.bin.rom1", "firmware.rom1")]
    [InlineData("rom2.bin", "firmware.rom2")]
    [InlineData("ROM2.BIN", "firmware.rom2")]
    [InlineData("console.rom2", "firmware.rom2")]
    [InlineData("console.bin.rom2", "firmware.rom2")]
    [InlineData("console.nvm", "firmware.nvram")]
    [InlineData("CONSOLE.NVM", "firmware.nvram")]
    [InlineData("console.mec", "firmware.mechanics")]
    [InlineData("CONSOLE.MEC", "firmware.mechanics")]
    [InlineData("scph39001.bin", "firmware.system")]
    [InlineData("unrelated.txt", null)]
    public void Ps2DetectedDumpsTargetTheirOwnFieldBeforeTheGenericBinExtension(string name, string? destination)
    {
        foreach (var emulator in new[] { "pcee2", "pcsx2" })
        {
            var configuration = new MachineConfiguration(ModelConstants.PlayStation2, emulator);
            var adapter = EmulatorCatalog.CreateAdapters().Single(item => item.EmulatorId == emulator);
            var slots = FirmwareConfigurationFunctions.Slots(configuration, adapter);
            var matches = FirmwareConfigurationFunctions.MatchingSlots(configuration.Model, slots, name);
            if (destination is null) Assert.Empty(matches);
            else Assert.Equal(destination, Assert.Single(matches).FieldId);
        }
    }

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
    public void BiosAnimationUsesCheckbox(string emulator, string key) => Assert.Equal(EmulationSettingsEditor.Toggle, Fields(emulator).Single(field => field.Id == key).Editor);

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
    public void MemoryTotalsRecognizeExistingModuleUnits(string display, long bytes) => Assert.Equal(bytes, EmulationSettingsValuePresentationFunctions.MemoryBytes(display));

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
        foreach (var field in audio.Fields)
        {
            var help = field.Id == SettingsConstants.AudioEnabled ? "Emulation.Help.Audio.Enabled"
                : field.Id == SettingsConstants.AudioOutput ? "Emulation.Help.Audio.Output" : "Emulation.Help.Audio.Latency";
            Assert.Equal(help + ".Short", field.ExplanationResourceKey);
            Assert.Equal(help + ".Detailed", field.DetailedExplanationResourceKey);
        }
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
        Assert.Equal(10, InputSettingsFunctions.Describe(configuration).ControllerPorts.Count);
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

    [Theory]
    [InlineData(null, false)]
    [InlineData("auto", false)]
    [InlineData("opengl", false)]
    [InlineData("vulkan", true)]
    [InlineData("d3d11", false)]
    [InlineData("none", false)]
    public void PspGraphicsApplicationPolicyMatchesConfigurationAndNativeMetadata(string? backend, bool vulkan)
    {
        var values = new Dictionary<string, string>();
        if (backend is not null) values["ppsspp_backend"] = backend;
        var originalValues = values.ToArray();
        var configuration = new MachineConfiguration(ModelConstants.Psp, "ppsspp", Options: values);
        var originalCatalog = GWGUI.Emulation.Sony.Emulators.Ppsspp.Dictionaries.OptionCatalog.All.ToArray();
        var declared = GWGUI.Emulation.Sony.Emulators.Ppsspp.Functions.PpssppOptionFunctions.Options(configuration);
        var native = declared.Select(option => option with { RequiresRestart = false }).ToArray();
        var merged = CoreOptionMetadataFunctions.Merge(native, declared);
        var fields = SettingsDescriptionFunctions.Create(configuration).SelectMany(block => block.Fields).ToArray();
        foreach (var key in new[] { "ppsspp_backend", "ppsspp_software_rendering", "ppsspp_inflight_frames" })
        {
            Assert.True(fields.Single(field => field.Id == key).RequiresRestart);
            Assert.True(merged.Single(option => option.Key == key).RequiresRestart);
        }
        var msaa = merged.Single(option => option.Key == "ppsspp_mulitsample_level");
        Assert.True(msaa.RequiresRestart);
        Assert.Equal(vulkan, msaa.IsVisible);
        Assert.Equal(vulkan, fields.Any(field => field.Id == msaa.Key));
        foreach (var key in new[] { "ppsspp_internal_resolution", "ppsspp_cropto16x9" })
        {
            Assert.Equal(vulkan, fields.Single(field => field.Id == key).RequiresRestart);
            Assert.Equal(vulkan, merged.Single(option => option.Key == key).RequiresRestart);
            Assert.False(originalCatalog.Single(option => option.Key == key).RequiresRestart);
        }
        Assert.False(merged.Single(option => option.Key == "ppsspp_fast_memory").RequiresRestart);
        Assert.Equal(originalValues, values.ToArray());
        Assert.Equal(originalCatalog, GWGUI.Emulation.Sony.Emulators.Ppsspp.Dictionaries.OptionCatalog.All);
    }

    [Fact]
    public void Pcsx2SettingsUseTheValuesOfTheOfficialLrps2Binary()
    {
        var native = GWGUI.Emulation.Sony.Emulators.Pcsx2.Dictionaries.OptionCatalog.All;
        var resolution = native.Single(option => option.Key == "pcsx2_upscale_multiplier");
        Assert.Equal("1x (Native)", resolution.DefaultValue);
        Assert.Equal(new[] { "1x (Native)", "2x", "4x", "8x" }, resolution.Values.Select(choice => choice.Value));
        var renderer = native.Single(option => option.Key == "pcsx2_renderer");
        Assert.Contains(renderer.Values, choice => choice.Value == "Software (HW)");
        Assert.Contains(renderer.Values, choice => choice.Value == "Software (SW)");
        Assert.DoesNotContain(renderer.Values, choice => choice.Value is "Software" or "Null");
    }

    [Theory]
    [InlineData("1x (Native)", "Software (HW)")]
    [InlineData("2x", "Software (HW)")]
    [InlineData("4x", "Software (HW)")]
    [InlineData("8x", "Software (HW)")]
    [InlineData("1x (Native)", "Software (SW)")]
    [InlineData("2x", "Software (SW)")]
    [InlineData("4x", "Software (SW)")]
    [InlineData("8x", "Software (SW)")]
    public void Pcsx2NativeSelectionsStayUnchangedThroughPresentationAndRuntime(string resolution, string renderer)
    {
        var configuration = new MachineConfiguration(ModelConstants.PlayStation2, "pcsx2", Options: new Dictionary<string, string>
        {
            ["pcsx2_upscale_multiplier"] = resolution,
            ["pcsx2_renderer"] = renderer
        });
        var native = GWGUI.Emulation.Sony.Emulators.Pcsx2.Functions.Pcsx2OptionFunctions.ToNative(configuration);
        Assert.Equal(resolution, native.Options!["pcsx2_upscale_multiplier"]);
        Assert.Equal(renderer, native.Options["pcsx2_renderer"]);
        Assert.Equal(resolution, configuration.Options!["pcsx2_upscale_multiplier"]);
        Assert.Equal(renderer, configuration.Options["pcsx2_renderer"]);
        var fields = SettingsDescriptionFunctions.Create(configuration).SelectMany(block => block.Fields).ToArray();
        Assert.Equal(resolution, fields.Single(field => field.Id == "pcsx2_upscale_multiplier").Value);
        Assert.Equal(renderer, fields.Single(field => field.Id == "pcsx2_renderer").Value);
    }

    [Fact]
    public void Pcsx2SeparatesPortCalibrationFromCpuVideoAndDiscSettings()
    {
        var configuration = new MachineConfiguration(ModelConstants.PlayStation2, "pcsx2");
        var fields = SettingsDescriptionFunctions.Create(configuration).SelectMany(block => block.Fields).ToArray();
        foreach (var port in new[] { 1, 2 })
        {
            var deadzone = Assert.Single(fields, field => field.Id == "pcsx2_axis_deadzone" + port);
            Assert.Equal(EmulationMachineTab.Controllers, deadzone.Tab);
            Assert.Equal(port, deadzone.ControllerPortNumber);
            Assert.Equal(EmulationSettingsEditor.Slider, deadzone.Editor);
            Assert.Equal(0, deadzone.NumericRange!.Minimum);
            Assert.Equal(50, deadzone.NumericRange.Maximum);
            Assert.Equal("15", deadzone.Value);
        }
        Assert.Equal(EmulationMachineTab.Cpu, fields.Single(field => field.Id == "pcsx2_ee_cpu").Tab);
        Assert.Equal(EmulationMachineTab.Video, fields.Single(field => field.Id == "pcsx2_deinterlace_mode").Tab);
        var cd = Assert.Single(StorageSettingsFunctions.Describe(configuration).AvailableDevices, device => device.MediaType == EmulationMediaType.CompactDisc);
        Assert.Equal(EmulationMachineTab.Storage, cd.ConfigurationFields!.Single(field => field.Id == "pcsx2_fastcdvd").Tab);
        Assert.DoesNotContain(fields, field => field.Id == "pcsx2_bios");
        Assert.DoesNotContain(fields, field => field.Id == "pcsx2_dev9_hdd");
    }

    [Theory]
    [InlineData(50, "200", 50)]
    [InlineData(75, "100", 75)]
    [InlineData(100, null, 100)]
    [InlineData(150, null, 150)]
    [InlineData(200, null, 200)]
    [InlineData(20, "150", 50)]
    [InlineData(250, "100", 200)]
    [InlineData(int.MinValue, null, 50)]
    [InlineData(int.MaxValue, null, 200)]
    [InlineData(null, "150", 150)]
    [InlineData(null, "invalid", 50)]
    [InlineData(null, null, 50)]
    public void Pcee2UsesOneAudioLatencyForTheCommonControlAndNativeOptions(int? commonLatency, string? legacyLatency, int expected)
    {
        var options = new Dictionary<string, string> { ["pcsx2_upscale_multiplier"] = "2" };
        if (legacyLatency is not null) options["pcsx2_audio_buffer_ms"] = legacyLatency;
        var original = options.ToArray();
        var configuration = new MachineConfiguration(ModelConstants.PlayStation2, "pcee2", Options: options,
            Audio: commonLatency is { } latency ? new AudioConfiguration("device-id", latency) : null);
        var adapter = new GWGUI.Emulation.Sony.Emulators.Pcee2.Factories.Pcee2MachineFactory();
        var audio = adapter.GetAudioConfiguration(configuration);
        var fields = SettingsDescriptionFunctions.Create(configuration).SelectMany(block => block.Fields).ToArray();
        var field = Assert.Single(fields, item => item.Id == SettingsConstants.AudioLatency);
        Assert.Equal(expected, audio.LatencyMilliseconds);
        Assert.Equal(configuration.Audio?.OutputDeviceId, audio.OutputDeviceId);
        Assert.Equal(expected.ToString(), field.Value);
        Assert.Equal(new[] { "50", "75", "100", "150", "200" }, field.Choices!.Select(choice => choice.Id));
        Assert.DoesNotContain(fields, item => item.Id == "pcsx2_audio_buffer_ms");
        var native = adapter.GetRuntimeOptions(configuration);
        Assert.Equal(expected.ToString(), native["pcsx2_audio_buffer_ms"]);
        Assert.Equal("2", native["pcsx2_upscale_multiplier"]);
        Assert.Equal(original, options.ToArray());
        var declared = GWGUI.Emulation.Sony.Emulators.Pcee2.Dictionaries.OptionCatalog.All;
        var nativeMetadata = declared.Select(option => option with { IsVisible = true }).ToArray();
        Assert.False(CoreOptionMetadataFunctions.Merge(nativeMetadata, declared).Single(option => option.Key == "pcsx2_audio_buffer_ms").IsVisible);
    }

    [Fact]
    public void Pcee2ApplySettingsFeedsTheCommonLatencyToRuntimeOptionsAndPreservesLegacySettings()
    {
        using var http = new HttpClient();
        var module = new GWGUI.Emulation.Sony.Modules.SonyEmulationModule(Environment.CurrentDirectory, Environment.CurrentDirectory, http, Environment.CurrentDirectory);
        var legacy = new MachineConfiguration(ModelConstants.PlayStation2, "pcee2",
            Options: new Dictionary<string, string> { ["pcsx2_audio_buffer_ms"] = "200" });
        var preserved = Assert.IsType<MachineConfiguration>(module.ApplySettings(legacy, new Dictionary<string, string?>()));
        Assert.Equal(200, preserved.Audio!.LatencyMilliseconds);
        var changed = Assert.IsType<MachineConfiguration>(module.ApplySettings(preserved, new Dictionary<string, string?> { [SettingsConstants.AudioLatency] = "75" }));
        Assert.Equal(75, changed.Audio!.LatencyMilliseconds);
        Assert.Equal("75", module.RuntimeOptions(changed)["pcsx2_audio_buffer_ms"]);
        Assert.Equal("200", legacy.Options!["pcsx2_audio_buffer_ms"]);
        Assert.Null(legacy.Audio);
    }

    public static IEnumerable<object[]> GraphicsApiConfigurations()
    {
        var adapters = new[]
        {
            (ModelConstants.PlayStation, "swanstation", "swanstation_GPU_Renderer", "OpenGL", "OpenGL", "OpenGL", "OpenGL"),
            (ModelConstants.PlayStation, "mednafen_psx_hw", "beetle_psx_hw_renderer", "hardware_gl", "hardware_gl", "hardware_gl", "hardware_gl"),
            (ModelConstants.PlayStation2, "pcee2", "pcsx2_renderer", "opengl", "vulkan", "vulkan", "vulkan"),
            (ModelConstants.PlayStation2, "pcsx2", "pcsx2_renderer", "OpenGL", "OpenGL", "OpenGL", "OpenGL"),
            (ModelConstants.PlayStation3, "rpcs3", "rpcs3_renderer", "opengl", "vulkan", "vulkan", "vulkan"),
            (ModelConstants.Psp, "ppsspp", "ppsspp_backend", "opengl", "auto", "auto", "auto")
        };
        foreach (var (model, emulator, key, opengl, vulkan, direct3d, automatic) in adapters)
        {
            yield return [model, emulator, key, EmulationVideoRenderer.OpenGL, opengl];
            yield return [model, emulator, key, EmulationVideoRenderer.Vulkan, vulkan];
            yield return [model, emulator, key, EmulationVideoRenderer.Direct3D11, direct3d];
            yield return [model, emulator, key, EmulationVideoRenderer.Wpf, automatic];
        }
    }

    [Theory]
    [InlineData("rpcs3_aspect_ratio", "16:9", EmulationMachineTab.Video, "16:9")]
    [InlineData("rpcs3_aspect_ratio", "4:3", EmulationMachineTab.Video, "16:9")]
    [InlineData("rpcs3_disk_cache_size", "unlimited", EmulationMachineTab.Storage, "1gb")]
    [InlineData("rpcs3_disk_cache_size", "1gb", EmulationMachineTab.Storage, "1gb")]
    [InlineData("rpcs3_disk_cache_size", "5gb", EmulationMachineTab.Storage, "1gb")]
    [InlineData("rpcs3_disk_cache_size", "wipe", EmulationMachineTab.Storage, "1gb")]
    [InlineData("rpcs3_vblank_rate", "list", EmulationMachineTab.Video, "60")]
    public void Rpcs3PresentationAndCacheChoicesPersistAndReachTheCore(string key, string value, EmulationMachineTab tab, string defaultValue)
    {
        using var http = new HttpClient();
        var module = new GWGUI.Emulation.Sony.Modules.SonyEmulationModule(Environment.CurrentDirectory, Environment.CurrentDirectory, http, Environment.CurrentDirectory);
        var original = new MachineConfiguration(ModelConstants.PlayStation3, "rpcs3");
        var field = module.Describe(original.Model, original).Blocks.SelectMany(block => block.Fields).Single(field => field.Id == key);
        Assert.Equal(tab, field.Tab);
        Assert.Equal(defaultValue, field.Value);
        Assert.Contains(field.Choices!, choice => choice.Id == value);
        if (key == "rpcs3_disk_cache_size")
        {
            var storage = module.DescribeStorageSettings(original);
            Assert.DoesNotContain(storage.AvailableDevices.SelectMany(device => device.ConfigurationFields ?? []), item => item.Id == key);
            Assert.Contains(storage.AvailableDevices, device => device.Slot.Category == EmulationMediaCategory.CompactDiscDrive && (device.ConfigurationFields?.Count ?? 0) > 0);
        }
        if (key == "rpcs3_aspect_ratio")
        {
            Assert.True(field.RequiresRestart);
            Assert.Equal(value, field.Choices!.Single(choice => choice.Id == value).InvariantDisplayValue);
        }
        var changed = Assert.IsType<MachineConfiguration>(module.ApplySettings(original, new Dictionary<string, string?> { [key] = value }));
        Assert.Equal(value, module.RuntimeOptions(changed)[key]);
        var restored = System.Text.Json.JsonSerializer.Deserialize<MachineConfiguration>(System.Text.Json.JsonSerializer.Serialize(changed))!;
        Assert.Equal(value, module.RuntimeOptions(restored)[key]);
        var restoredField = module.Describe(restored.Model, restored).Blocks.SelectMany(block => block.Fields).Single(field => field.Id == key);
        Assert.Equal(value, restoredField.Value);
        Assert.Null(original.Options);
    }

    [Theory]
    [InlineData("swanstation", "swanstation_GPU_Renderer", "Auto")]
    [InlineData("mednafen_psx_hw", "beetle_psx_hw_renderer", "hardware")]
    public void NativeDefaultRemainsAValidHardwareModeWithoutAnApplicationPreference(string emulator, string key, string expected)
    {
        using var http = new HttpClient();
        var module = new GWGUI.Emulation.Sony.Modules.SonyEmulationModule(Environment.CurrentDirectory, Environment.CurrentDirectory, http, Environment.CurrentDirectory);
        var configuration = new MachineConfiguration(ModelConstants.PlayStation, emulator);
        var field = module.Describe(ModelConstants.PlayStation, configuration).Blocks.SelectMany(block => block.Fields).Single(field => field.Id == key);
        Assert.Equal(expected, field.Value);
        var hardware = Assert.Single(field.Choices!, choice => choice.DisplayResourceKey == "Emulation.Option.Common.Value.HardwareRendering");
        Assert.Equal(expected, hardware.Id);
        Assert.Equal(2, field.Choices!.Count);
        Assert.Equal(expected, module.RuntimeOptions(configuration)[key]);
    }

    [Theory]
    [MemberData(nameof(GraphicsApiConfigurations))]
    public void SharedVideoRendererSelectsNativeApiAndLeavesOnlyOneHardwareMode(string model, string emulator, string key, EmulationVideoRenderer renderer, string expected)
    {
        using var http = new HttpClient();
        var module = new GWGUI.Emulation.Sony.Modules.SonyEmulationModule(Environment.CurrentDirectory, Environment.CurrentDirectory, http, Environment.CurrentDirectory);
        var originalOptions = new Dictionary<string, string> { ["unrelated"] = "kept" };
        var configuration = new MachineConfiguration(model, emulator, Options: originalOptions);
        var result = Assert.IsType<MachineConfiguration>(EmulationVideoConfigurationFunctions.Apply(module, configuration, renderer));
        Assert.Equal(EmulationVideoConfigurationFunctions.GraphicsApi(renderer), result.GraphicsApi);
        Assert.Equal(expected, result.Options![key]);
        Assert.Equal(expected, module.RuntimeOptions(result)[key]);
        Assert.Equal("kept", result.Options["unrelated"]);
        Assert.Single(originalOptions);
        Assert.Null(configuration.GraphicsApi);
        var field = module.Describe(model, result).Blocks.SelectMany(block => block.Fields).Single(field => field.Id == key);
        var hardware = Assert.Single(field.Choices!, choice => choice.DisplayResourceKey == "Emulation.Option.Common.Value.HardwareRendering");
        Assert.Equal(expected, hardware.Id);
        Assert.Equal(expected, field.Value);
        Assert.True(field.RefreshSettingsOnChange);
        if (emulator == "ppsspp")
            Assert.DoesNotContain(module.Describe(model, result).Blocks.SelectMany(block => block.Fields), item => item.Id == "ppsspp_mulitsample_level");
        var json = System.Text.Json.JsonSerializer.Serialize(result);
        var restored = System.Text.Json.JsonSerializer.Deserialize<MachineConfiguration>(json)!;
        Assert.Equal(result.GraphicsApi, restored.GraphicsApi);
        Assert.Equal(expected, module.RuntimeOptions(restored)[key]);
    }

    [Theory]
    [InlineData(ModelConstants.PlayStation, "swanstation", "swanstation_GPU_Renderer", "Software", "Software")]
    [InlineData(ModelConstants.PlayStation, "mednafen_psx_hw", "beetle_psx_hw_renderer", "software", "software")]
    [InlineData(ModelConstants.PlayStation2, "pcee2", "pcsx2_renderer", "software", "software")]
    [InlineData(ModelConstants.PlayStation2, "pcsx2", "pcsx2_renderer", "Software (HW)", "Software (HW)")]
    [InlineData(ModelConstants.PlayStation2, "pcsx2", "pcsx2_renderer", "Software (SW)", "Software (SW)")]
    [InlineData(ModelConstants.PlayStation2, "pcsx2", "pcsx2_renderer", "paraLLEl-GS", "paraLLEl-GS")]
    [InlineData(ModelConstants.PlayStation2, "pcsx2", "pcsx2_renderer", "D3D12", "D3D12")]
    [InlineData(ModelConstants.PlayStation3, "rpcs3", "rpcs3_renderer", "null", "null")]
    [InlineData(ModelConstants.Psp, "ppsspp", "ppsspp_backend", "none", "none")]
    public void SharedApiPreservesSoftwareAndSpecialModesUntilHardwareIsSelected(string model, string emulator, string key, string previous, string expected)
    {
        using var http = new HttpClient();
        var module = new GWGUI.Emulation.Sony.Modules.SonyEmulationModule(Environment.CurrentDirectory, Environment.CurrentDirectory, http, Environment.CurrentDirectory);
        var original = new MachineConfiguration(model, emulator, Options: new Dictionary<string, string> { [key] = previous });
        var configured = Assert.IsType<MachineConfiguration>(EmulationVideoConfigurationFunctions.Apply(module, original, EmulationVideoRenderer.OpenGL));
        Assert.Equal(expected, configured.Options![key]);
        Assert.Equal(expected, module.RuntimeOptions(configured)[key]);
        Assert.Equal(previous, original.Options![key]);
        var field = module.Describe(model, configured).Blocks.SelectMany(block => block.Fields).Single(field => field.Id == key);
        Assert.Equal(expected, field.Value);
        Assert.Contains(field.Choices!, choice => choice.Id == expected);
        if (expected == "D3D12") Assert.Equal("D3D12", Assert.Single(field.Choices!, choice => choice.Id == expected).InvariantDisplayValue);
        var hardware = Assert.Single(field.Choices!, choice => choice.DisplayResourceKey == "Emulation.Option.Common.Value.HardwareRendering");
        var changed = module.ApplySettings(configured, new Dictionary<string, string?> { [key] = hardware.Id });
        Assert.Equal(hardware.Id, module.RuntimeOptions(changed)[key]);
    }

    [Fact]
    public void VideoPreferenceDoesNotInventOptionsForMachinesWithoutAnEmulator()
    {
        using var http = new HttpClient();
        var module = new GWGUI.Emulation.Sony.Modules.SonyEmulationModule(Environment.CurrentDirectory, Environment.CurrentDirectory, http, Environment.CurrentDirectory);
        var configuration = new MachineConfiguration(ModelConstants.PlayStation5, string.Empty);
        var result = Assert.IsType<MachineConfiguration>(EmulationVideoConfigurationFunctions.Apply(module, configuration, EmulationVideoRenderer.Vulkan));
        Assert.Equal(EmulationGraphicsApi.Vulkan, result.GraphicsApi);
        Assert.Null(result.Options);
        Assert.Empty(module.RuntimeOptions(result));
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
