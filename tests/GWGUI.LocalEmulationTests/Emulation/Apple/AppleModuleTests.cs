using System.IO;
using System.Text.Json;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Apple.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Apple.Common.Machines.Common.Constants;
using GWGUI.Emulation.Apple.Common.Machines.Common.Dictionaries;
using GWGUI.Emulation.Apple.Common.Machines.Common.Enums;
using GWGUI.Emulation.Apple.Common.Machines.Common.Functions;
using GWGUI.Emulation.Apple.Emulators.Common.Interop.Dictionaries;
using GWGUI.Emulation.Apple.Emulators.Common.Interop.Functions;

namespace GWGUI.Tests.Emulation.Apple;

public sealed class AppleModuleTests
{
    [Theory]
    [InlineData(MachineModel.AppleIIe, Emulator.AppleWin, "applewin-hdv", ".hdv", "SmartPort")]
    [InlineData(MachineModel.MacintoshII, Emulator.MiniVmac, "minivmac-raw", ".img", "Macintosh")]
    public void DiskFormatsKeepTheirNativeMetadataWithOrWithoutAnEmulatorSelection(MachineModel model, Emulator emulator, string id, string extension, string interfaceName)
    {
        var selected = new MachineConfiguration(model, core: emulator);
        var adapter = new GWGUI.Emulation.Apple.Common.Services.Engine().Adapter(selected);
        Assert.Equal(emulator == Emulator.MiniVmac, adapter.ReplacesMountedMediaOnInsert(selected));
        Assert.Empty(adapter.GetHardDiskFormats(selected, EmulationMediaSlot.Floppy0));
        Assert.Empty(adapter.GetHardDiskFormats(selected, new EmulationMediaSlot(EmulationMediaSlot.HardDisk0.Category, 1)));
        foreach (var configuration in new[] { selected, selected with { Core = Emulator.None } })
        {
            var disk = Assert.Single(StorageSettingsFunctions.Describe(configuration).AvailableDevices, device => device.Slot == EmulationMediaSlot.HardDisk0);
            var format = Assert.Single(disk.HardDiskFormats!);
            Assert.Equal(id, format.Id);
            Assert.Equal(extension, format.Extension);
            Assert.Equal(interfaceName, format.InterfaceName);
            Assert.Equal(0x007FFFFFL * 512, format.MaximumBytes);
            Assert.Equal(32L * 1024 * 1024, format.DefaultBytes);
            Assert.Equal(new[] { GWGUI.Emulation.HardDisks.HardDiskPreparation.Blank }, format.SupportedPreparations);
            Assert.True(format.Operations.HasFlag(GWGUI.Emulation.HardDisks.DiskFormatOperations.Create));
            Assert.Null(format.InitializeBlankImage);
            Assert.Equal(configuration.Core, configuration == selected ? emulator : Emulator.None);
        }
    }

    [Theory]
    [InlineData(MachineModel.AppleII)]
    [InlineData(MachineModel.AppleIIPlus)]
    [InlineData(MachineModel.AppleIIJPlus)]
    [InlineData(MachineModel.AppleIIe)]
    [InlineData(MachineModel.AppleIIeEnhanced)]
    public void AppleTwoUsesCommonVideoEffectsAndDisplaysItsEmbeddedRom(MachineModel model)
    {
        using var client = new System.Net.Http.HttpClient();
        var module = new GWGUI.Emulation.Apple.Modules.AppleEmulationModule(".", ".", client, ".");
        var configuration = new MachineConfiguration(model);
        var fields = module.Describe(configuration.MachineId, configuration).Blocks.SelectMany(block => block.Fields).ToArray();
        Assert.Equal("Emulation.Cpu.SpeedOriginal", Assert.Single(fields, field => field.Id == SettingsConstants.CpuFrequency).LabelResourceKey);
        Assert.DoesNotContain(fields, field => field.Id is "applewin_video_mode" or "applewin_video_style");
        Assert.Contains(fields, field => field.Id == "expansion.video");
        Assert.Contains(fields, field => field.Id == "applewin_video_refresh_rate");
        Assert.Equal("Color (RGB Card/Monitor)", module.RuntimeOptions(configuration)["applewin_video_mode"]);
        Assert.Equal("560 x 192", module.RuntimeOptions(configuration)["applewin_video_style"]);
        var firmware = Assert.Single(fields, field => field.Id == SettingsConstants.Firmware);
        Assert.Equal(EmulationSettingsEditor.Information, firmware.Editor);
        Assert.Equal(SettingsConstants.ResourceEmbeddedFirmware, Assert.Single(firmware.Choices!).DisplayResourceKey);
        const string prefix = "GWGUI.Emulation.Apple.Resources.Emulation.";
        var cultures = typeof(GWGUI.Emulation.Apple.Modules.AppleEmulationModule).Assembly.GetManifestResourceNames()
            .Where(name => name.StartsWith(prefix, StringComparison.Ordinal) && name.EndsWith(".resources", StringComparison.Ordinal))
            .Select(name => name[prefix.Length..^".resources".Length]).Where(name => name != "00-Base").ToArray();
        Assert.Equal(29, cultures.Length);
        foreach (var culture in cultures)
            Assert.True(module.TryGetString(SettingsConstants.ResourceEmbeddedFirmware, System.Globalization.CultureInfo.GetCultureInfo(culture), out _));
    }

    [Fact]
    public void MacintoshTwoKeepsItsExternalRomPathEditor()
    {
        using var client = new System.Net.Http.HttpClient();
        var module = new GWGUI.Emulation.Apple.Modules.AppleEmulationModule(".", ".", client, ".");
        var configuration = new MachineConfiguration(MachineModel.MacintoshII);
        var field = Assert.Single(module.Describe(configuration.MachineId, configuration).Blocks.SelectMany(block => block.Fields), field => field.Id == SettingsConstants.Firmware);
        Assert.Equal(EmulationSettingsEditor.Path, field.Editor);
        var updated = (MachineConfiguration)module.ApplySettings(configuration, new Dictionary<string, string?> { [field.Id] = "MacII.ROM" });
        Assert.Equal("MacII.ROM", Assert.Single(updated.Firmwares).Path);
    }

    [Fact]
    public void ExpansionCardsUseTheirFunctionalTabsAndRefreshTheirSlotAvailability()
    {
        using var client = new System.Net.Http.HttpClient();
        var module = new GWGUI.Emulation.Apple.Modules.AppleEmulationModule(".", ".", client, ".");
        var configuration = new MachineConfiguration(MachineModel.AppleII);
        var settings = module.Describe(configuration.MachineId, configuration);
        var fields = settings.Blocks.SelectMany(block => block.Fields).ToDictionary(field => field.Id);
        Assert.True(settings.Visibility.Tabs[EmulationMachineTab.Mouse]);
        Assert.DoesNotContain(fields.Values, field => field.Tab == EmulationMachineTab.General);
        Assert.Equal(EmulationMachineTab.Video, fields["expansion.video"].Tab);
        Assert.Equal(EmulationMachineTab.Cpu, fields["expansion.z80"].Tab);
        Assert.Equal(EmulationMachineTab.Mouse, fields["expansion.mouse"].Tab);
        foreach (var id in new[] { "expansion.mockingboard", "expansion.phasor", "expansion.sam" }) Assert.Equal(EmulationMachineTab.Audio, fields[id].Tab);
        Assert.False(fields["expansion.mouse"].IsEnabled); // The native default owns slot 4.
        Assert.All(fields.Values.Where(field => field.Id.StartsWith("expansion.")), field => Assert.True(field.RefreshSettingsOnChange));
        Assert.DoesNotContain("applewin_slot7", fields.Keys);
    }

    [Fact]
    public void CardsInDifferentTabsCannotReplaceAnOccupiedSlot()
    {
        using var client = new System.Net.Http.HttpClient();
        var module = new GWGUI.Emulation.Apple.Modules.AppleEmulationModule(".", ".", client, ".");
        var configuration = new MachineConfiguration(MachineModel.AppleII);
        MachineConfiguration Apply(MachineConfiguration current, string id, string value) => (MachineConfiguration)module.ApplySettings(current, new Dictionary<string, string?> { [id] = value });
        EmulationSettingsField Field(MachineConfiguration current, string id) => module.Describe(current.MachineId, current).Blocks.SelectMany(block => block.Fields).Single(field => field.Id == id);
        configuration = Apply(configuration, "expansion.z80", EmulationSettingConstants.Enabled);
        Assert.Equal("Z80 SoftCard", configuration.Options["applewin_slot5"]);
        Assert.False(Field(configuration, "expansion.sam").IsEnabled);
        Assert.DoesNotContain(Field(configuration, "expansion.mockingboard").Choices!, choice => choice.Id is "5" or "4+5");
        Assert.Equal(configuration.Options, Apply(configuration, "expansion.sam", EmulationSettingConstants.Enabled).Options);
        configuration = Apply(configuration, "expansion.z80", EmulationSettingConstants.Disabled);
        configuration = Apply(configuration, "expansion.mockingboard", "none");
        configuration = Apply(configuration, "expansion.mouse", EmulationSettingConstants.Enabled);
        configuration = Apply(configuration, "expansion.phasor", "5");
        Assert.Equal("Mouse Card", configuration.Options["applewin_slot4"]);
        Assert.Equal("Phasor", configuration.Options["applewin_slot5"]);
        Assert.False(Field(configuration, "expansion.z80").IsEnabled);
        configuration = Apply(configuration, "expansion.phasor", "none");
        configuration = Apply(configuration, "expansion.sam", EmulationSettingConstants.Enabled);
        Assert.Equal("SAM", configuration.Options["applewin_slot5"]);
        Assert.DoesNotContain(configuration.Options.Keys, key => key.StartsWith("expansion."));
    }

    [Theory]
    [InlineData("expansion.mockingboard", "Mockingboard C")]
    [InlineData("expansion.phasor", "Phasor")]
    public void AudioCardsCanUseBothNativeSlots(string id, string card)
    {
        using var client = new System.Net.Http.HttpClient();
        var module = new GWGUI.Emulation.Apple.Modules.AppleEmulationModule(".", ".", client, ".");
        var configuration = new MachineConfiguration(MachineModel.AppleII, options: new Dictionary<string, string> { ["applewin_slot4"] = "Empty" });
        var updated = (MachineConfiguration)module.ApplySettings(configuration, new Dictionary<string, string?> { [id] = "4+5" });
        Assert.Equal(card, updated.Options["applewin_slot4"]);
        Assert.Equal(card, updated.Options["applewin_slot5"]);
    }

    [Fact]
    public void HardDiskDeviceAutomaticallyControlsSlotSeven()
    {
        var configuration = new MachineConfiguration(MachineModel.AppleII);
        var settings = StorageSettingsFunctions.Describe(configuration);
        var added = StorageSettingsFunctions.Apply(configuration, settings with { ConfiguredSlots = settings.ConfiguredSlots.Append(EmulationMediaSlot.HardDisk0).ToArray() });
        Assert.Equal("Hard Disk Controller", added.Options["applewin_slot7"]);
        var removed = StorageSettingsFunctions.Apply(added, StorageSettingsFunctions.Describe(added) with { ConfiguredSlots = settings.ConfiguredSlots });
        Assert.Equal("Empty", removed.Options["applewin_slot7"]);
        Assert.Empty(removed.Media);
    }

    [Fact]
    public void EnabledAppleMouseCardExposesTwoAssignableButtonsAndPointerCapture()
    {
        using var client = new System.Net.Http.HttpClient();
        var module = new GWGUI.Emulation.Apple.Modules.AppleEmulationModule(".", ".", client, ".");
        var configuration = new MachineConfiguration(MachineModel.AppleII, options: new Dictionary<string, string> { ["applewin_slot4"] = "Empty" });
        Assert.False(InputSettingsFunctions.SupportsPointer(configuration));
        Assert.Null(module.DescribeInputSettings(configuration).Mouse);
        configuration = (MachineConfiguration)module.ApplySettings(configuration, new Dictionary<string, string?> { ["expansion.mouse"] = EmulationSettingConstants.Enabled });
        Assert.True(InputSettingsFunctions.SupportsPointer(configuration));
        var settings = module.DescribeInputSettings(configuration);
        Assert.Equal(new[] { "Left", "Right" }, settings.Mouse!.Definitions.Select(item => item.Id));
        configuration = (MachineConfiguration)module.ApplyInputSettings(configuration, settings with
        {
            Mouse = settings.Mouse with { Values = new Dictionary<string, string> { ["Right"] = "Keyboard:A" } }
        });
        Assert.True(InputSnapshotFunctions.Apply(Pressed(EmulationKey.A), configuration.Input, configuration.Model).Pointer.Right);
        Assert.False(InputSnapshotFunctions.Apply(Pressed(EmulationKey.B), configuration.Input, configuration.Model).Pointer.Right);
    }

    [Theory]
    [InlineData(MachineModel.AppleIIe, EmulationKey.AppleOpenApple, EmulationKey.LeftAlt)]
    [InlineData(MachineModel.AppleIIe, EmulationKey.AppleSolidApple, EmulationKey.RightAlt)]
    [InlineData(MachineModel.MacintoshII, EmulationKey.MacintoshCommand, EmulationKey.LeftAlt)]
    [InlineData(MachineModel.MacintoshII, EmulationKey.MacintoshKeypadClear, EmulationKey.Delete)]
    [InlineData(MachineModel.MacintoshII, EmulationKey.MacintoshKeypadEquals, EmulationKey.Insert)]
    public void ErasedSpecialKeyRemainsErasedAfterPersistenceAndDoesNotRestoreItsDefault(MachineModel model, EmulationKey target, EmulationKey previous)
    {
        var configuration = new MachineConfiguration(model);
        var settings = InputSettingsFunctions.Describe(configuration);
        var applied = InputSettingsFunctions.Apply(configuration, settings with { Keyboard = settings.Keyboard! with { Values = settings.Keyboard.Definitions.ToDictionary(item => item.Id, item => item.Id == target.ToString() ? string.Empty : item.DefaultBinding, StringComparer.Ordinal) } });
        var document = ConfigurationStoreFunctions.ToDocument(applied, Path.GetTempPath());
        var json = JsonSerializer.SerializeToElement(document, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var restored = ConfigurationStoreFunctions.FromDocument(ConfigurationStoreFunctions.Deserialize(json), Path.GetTempPath());
        Assert.Equal(string.Empty, InputSettingsFunctions.Describe(restored).Keyboard!.Values[target.ToString()]);
        var snapshot = InputSnapshotFunctions.Apply(Pressed(previous, EmulationKey.A, EmulationKey.Numpad1), restored.Input, model);
        Assert.DoesNotContain(target, snapshot.Keys);
        Assert.Contains(EmulationKey.A, snapshot.Keys);
        Assert.Contains(EmulationKey.Numpad1, snapshot.Keys);
        var native = KeyboardFunctions.CreateKeyMap();
        Assert.DoesNotContain(snapshot.Keys.Where(native.ContainsKey).Select(key => native[key]), code => code == native[target]);
    }
    [Theory]
    [InlineData(MachineModel.AppleII, true, 0, 1)]
    [InlineData(MachineModel.AppleIIPlus, true, 0, 1)]
    [InlineData(MachineModel.AppleIIJPlus, true, 0, 1)]
    [InlineData(MachineModel.AppleIIe, true, 2, 1)]
    [InlineData(MachineModel.AppleIIeEnhanced, true, 2, 1)]
    [InlineData(MachineModel.Macintosh128K, false, 3, 0)]
    [InlineData(MachineModel.Macintosh512K, false, 3, 0)]
    [InlineData(MachineModel.MacintoshPlus, false, 3, 0)]
    [InlineData(MachineModel.MacintoshClassic, false, 3, 0)]
    [InlineData(MachineModel.MacintoshII, true, 3, 0)]
    [InlineData(MachineModel.Pippin, false, 0, 0)]
    public void SelectionDescribesPhysicalModelWithoutClaimingUnsupportedRuntime(MachineModel model, bool executable, int keys, int ports)
    {
        var configuration = new MachineConfiguration(model);
        var blocks = SettingsDescriptionFunctions.Create(configuration);
        Assert.DoesNotContain(blocks.SelectMany(block => block.Fields), field => field.Editor == EmulationSettingsEditor.DirectoryPath || field.Id == SettingsConstants.ResourceUnavailableModel);
        var settings = InputSettingsFunctions.Describe(configuration);
        Assert.Equal(executable, ConfigurationFunctions.SupportsRuntime(model));
        Assert.Equal(executable, CoreCatalog.GetAll(model).Count > 0);
        Assert.Equal(keys, settings.Keyboard!.Definitions.Count);
        Assert.Equal(ports, settings.ControllerPorts.Count);
        Assert.DoesNotContain(settings.Keyboard.Definitions, definition => definition.Id is nameof(EmulationKey.A) or nameof(EmulationKey.LeftAmiga));
    }

    [Theory]
    [InlineData(MachineModel.AppleIIe, EmulationKey.AppleOpenApple, EmulationKey.LeftAlt)]
    [InlineData(MachineModel.AppleIIe, EmulationKey.AppleSolidApple, EmulationKey.RightAlt)]
    [InlineData(MachineModel.MacintoshII, EmulationKey.MacintoshCommand, EmulationKey.LeftAlt)]
    [InlineData(MachineModel.MacintoshII, EmulationKey.MacintoshKeypadClear, EmulationKey.Delete)]
    [InlineData(MachineModel.MacintoshII, EmulationKey.MacintoshKeypadEquals, EmulationKey.Insert)]
    public void ReassignmentSurvivesPersistenceAndRemovesNativeDefaultAlias(MachineModel model, EmulationKey target, EmulationKey previous)
    {
        var configuration = new MachineConfiguration(model);
        var settings = InputSettingsFunctions.Describe(configuration);
        var changed = settings with
        {
            Keyboard = settings.Keyboard! with
            {
                Values = new Dictionary<string, string> { [target.ToString()] = nameof(EmulationKey.Z) }
            }
        };
        var applied = InputSettingsFunctions.Apply(configuration, changed);
        var pathBase = Path.GetTempPath();
        var document = ConfigurationStoreFunctions.ToDocument(applied, pathBase);
        var json = JsonSerializer.SerializeToElement(document, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var restored = ConfigurationStoreFunctions.FromDocument(ConfigurationStoreFunctions.Deserialize(json), pathBase);
        Assert.Equal(nameof(EmulationKey.Z), InputSettingsFunctions.Describe(restored).Keyboard!.Values[target.ToString()]);
        var mapped = InputSnapshotFunctions.Apply(Pressed(EmulationKey.Z, EmulationKey.A), restored.Input, model);
        Assert.Contains(target, mapped.Keys);
        Assert.Contains(EmulationKey.A, mapped.Keys);
        var old = InputSnapshotFunctions.Apply(Pressed(previous), restored.Input, model);
        var codes = KeyboardFunctions.CreateKeyMap();
        Assert.DoesNotContain(old.Keys.Where(codes.ContainsKey).Select(key => codes[key]), code => code == codes[target]);
    }

    [Fact]
    public void MacintoshMouseButtonCanBeAssignedToKeyboardAndPersisted()
    {
        var configuration = new MachineConfiguration(MachineModel.MacintoshII);
        var settings = InputSettingsFunctions.Describe(configuration);
        var applied = InputSettingsFunctions.Apply(configuration, settings with
        {
            Mouse = settings.Mouse! with
            {
                Values = new Dictionary<string, string> { ["Left"] = "Keyboard:Space" }
            }
        });
        Assert.True(InputSnapshotFunctions.Apply(Pressed(EmulationKey.Space), applied.Input, applied.Model).Pointer.Left);
        var physical = EmulationInputSnapshot.Empty with
        {
            Pointer = EmulationInputSnapshot.Empty.Pointer with { Left = true }
        };
        Assert.False(InputSnapshotFunctions.Apply(physical, applied.Input, applied.Model).Pointer.Left);
        Assert.Equal("Keyboard:Space", InputSettingsFunctions.Describe(applied).Mouse!.Values["Left"]);
    }

    [Fact]
    public void BothNativeJoystickButtonsAreAssignable()
    {
        var configuration = new MachineConfiguration(MachineModel.AppleIIe);
        var settings = InputSettingsFunctions.Describe(configuration);
        var port = Assert.Single(settings.ControllerPorts);
        Assert.Contains(port.Bindings.Definitions, definition => definition.Id == EmulationControllerCommandIds.Fire2);
        var applied = InputSettingsFunctions.Apply(configuration, settings with
        {
            ControllerPorts = [port with
            {
                Bindings = port.Bindings with
                {
                    Values = new Dictionary<string, string>
                    {
                        [EmulationControllerCommandIds.Fire2] = "Keyboard:Space"
                    }
                }
            }]
        });
        var input = InputSnapshotFunctions.Apply(Pressed(EmulationKey.Space), applied.Input, applied.Model);
        Assert.Equal(1u << EmulationInputMappingConstants.BIndex, input.Controllers[0].Buttons);
    }

    [Fact]
    public void SingleDiskFrontendRejectsMultipleMountedImages()
    {
        Assert.Throws<ArgumentException>(() => new MachineConfiguration(MachineModel.MacintoshII, media: [new("floppy.dsk", MediaCategory.Floppy, EmulationMediaSlot.Floppy0), new("disk.img", MediaCategory.HardDisk, EmulationMediaSlot.HardDisk0)]));
    }

    [Theory]
    [InlineData(MachineModel.AppleIIe)]
    [InlineData(MachineModel.MacintoshII)]
    public void EmptyHardDiskDeviceSurvivesPersistenceAndCanBeRemoved(MachineModel model)
    {
        var configuration = new MachineConfiguration(model);
        var settings = StorageSettingsFunctions.Describe(configuration);
        var applied = StorageSettingsFunctions.Apply(configuration, settings with
        {
            ConfiguredSlots = settings.ConfiguredSlots.Append(EmulationMediaSlot.HardDisk0).ToArray()
        });
        var pathBase = Path.GetTempPath();
        var document = ConfigurationStoreFunctions.ToDocument(applied, pathBase);
        var json = JsonSerializer.SerializeToElement(document, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var restored = ConfigurationStoreFunctions.FromDocument(ConfigurationStoreFunctions.Deserialize(json), pathBase);
        var reloaded = StorageSettingsFunctions.Describe(restored);
        Assert.Contains(EmulationMediaSlot.HardDisk0, reloaded.ConfiguredSlots);
        var hardDisk = Assert.Single(reloaded.AvailableDevices, device => device.Slot == EmulationMediaSlot.HardDisk0);
        Assert.Equal(EmulationStorageConfigurationKind.HardDiskDrive, hardDisk.ConfigurationKind);
        Assert.NotEmpty(hardDisk.HardDiskFormats!);
        Assert.Empty(reloaded.MountedMedia);
        var removed = StorageSettingsFunctions.Apply(restored, reloaded with
        {
            ConfiguredSlots = reloaded.ConfiguredSlots.Where(slot => slot != EmulationMediaSlot.HardDisk0).ToArray()
        });
        Assert.DoesNotContain(EmulationMediaSlot.HardDisk0, StorageSettingsFunctions.Describe(removed).ConfiguredSlots);
        Assert.Contains(EmulationMediaSlot.Floppy0, StorageSettingsFunctions.Describe(removed).ConfiguredSlots);
        Assert.Equal(model == MachineModel.AppleIIe, StorageSettingsFunctions.Describe(removed).ConfiguredSlots.Contains(EmulationMediaSlot.Floppy1));
    }

    [Theory]
    [InlineData(MachineModel.AppleII)]
    [InlineData(MachineModel.AppleIIPlus)]
    [InlineData(MachineModel.AppleIIJPlus)]
    [InlineData(MachineModel.AppleIIe)]
    [InlineData(MachineModel.AppleIIeEnhanced)]
    public void AppleJoystickVisualKeepsAnalogDirectionsAndEmptyPhysicalBindings(MachineModel model)
    {
        var configuration = new MachineConfiguration(model);
        var settings = InputSettingsFunctions.Describe(configuration);
        var port = Assert.Single(settings.ControllerPorts);
        var joystick = Assert.Single(port.ControllerChoices, choice => choice.Id == nameof(PeripheralCategory.AnalogJoystick));
        Assert.Equal(EmulationControllerVisualIds.AppleStandardJ665, joystick.DefaultVisualId);
        Assert.Contains(EmulationControllerVisualIds.AppleStandardJ665, joystick.CompatibleVisualIds!);
        Assert.Contains(EmulationControllerVisualIds.AppleA2M2002, joystick.CompatibleVisualIds!);
        Assert.Contains(EmulationControllerVisualIds.AppleChMachII, joystick.CompatibleVisualIds!);
        Assert.Contains(EmulationControllerVisualIds.AppleChMachIII, joystick.CompatibleVisualIds!);
        Assert.Contains(EmulationControllerVisualIds.AppleKraftPremiumII, joystick.CompatibleVisualIds!);
        Assert.Equal(5, joystick.CompatibleVisualIds!.Count);
        Assert.Equal(6, joystick.VisualCommandIds!.Count);
        Assert.Equal(EmulationControllerCommandIds.Fire1, joystick.VisualCommandIds[EmulationControllerVisualControl.PrimaryAction]);
        Assert.Empty(port.Bindings.Values);
        var kraftBindings = joystick.VisualBindingDefinitions![EmulationControllerVisualIds.AppleKraftPremiumII];
        Assert.Equal(5, kraftBindings.Count);
        Assert.Contains(kraftBindings, definition => definition.Id == EmulationControllerCommandIds.Fire1);
        Assert.DoesNotContain(kraftBindings, definition => definition.Id == EmulationControllerCommandIds.Fire2);
        Assert.All(kraftBindings.Take(4), definition => Assert.NotNull(definition.OppositeDirectionId));
        Assert.All(port.Bindings.Definitions.Take(4), definition => Assert.NotNull(definition.OppositeDirectionId));
        foreach (var visualId in joystick.CompatibleVisualIds)
        {
            var restored = InputSettingsFunctions.Apply(configuration, settings with
            {
                ControllerPorts = [port with { VisualId = visualId }]
            });
            var restoredPort = Assert.Single(InputSettingsFunctions.Describe(restored).ControllerPorts);
            Assert.Equal(visualId, restoredPort.VisualId);
            Assert.Empty(restoredPort.Bindings.Values);
        }
    }

    [Fact]
    public async Task PippinIsListedWithoutAdvertisingAnIncompatibleEmulator()
    {
        using var client = new System.Net.Http.HttpClient();
        var module = new GWGUI.Emulation.Apple.Modules.AppleEmulationModule(".", ".", client, ".");
        Assert.Contains(module.Machines, machine => machine.Id == nameof(MachineModel.Pippin));
        var configuration = (MachineConfiguration)module.CreateConfiguration(nameof(MachineModel.Pippin));
        Assert.Equal(MachineFamily.Pippin, configuration.Family);
        Assert.Equal(Emulator.None, configuration.Core);
        Assert.Empty(await module.GetEmulatorInstallationsAsync(configuration));
        var settings = module.Describe(configuration.MachineId, configuration);
        Assert.True(settings.Visibility.Tabs[EmulationMachineTab.General]);
        Assert.False(settings.Visibility.Tabs[EmulationMachineTab.Controllers]);
        Assert.False(settings.Visibility.Tabs[EmulationMachineTab.Storage]);
        Assert.Empty(await module.ScanFirmwareAsync(configuration.MachineId, configuration));
        var document = ConfigurationStoreFunctions.ToDocument(configuration, Path.GetTempPath());
        var json = JsonSerializer.SerializeToElement(document, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var reloaded = ConfigurationStoreFunctions.FromDocument(ConfigurationStoreFunctions.Deserialize(json), Path.GetTempPath());
        Assert.Equal(MachineModel.Pippin, reloaded.Model);
        Assert.Equal(Emulator.None, reloaded.Core);
    }

    [Fact]
    public void EveryPhysicalMachineHasExternalArtwork()
    {
        foreach (var machine in MachineCatalog.All)
            Assert.True(File.Exists(Path.Combine(AppContext.BaseDirectory, machine.ImagePath!)), machine.ImagePath);
    }

    private static EmulationInputSnapshot Pressed(params EmulationKey[] keys) => EmulationInputSnapshot.Empty with { Keys = keys.ToHashSet() };
}
