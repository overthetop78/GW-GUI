using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Coleco.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Coleco.Common.Machines.Common.Enums;
using GWGUI.Emulation.Coleco.Common.Machines.Common.Functions;
using GWGUI.Emulation.Coleco.Modules;

namespace GWGUI.Tests.Emulation.Coleco;

public sealed class ColecoModuleTests
{
    [Theory]
    [InlineData("enabled")]
    [InlineData("disabled")]
    public void BlueMsxFastBootIsInGeneralAndRetainsItsNativeValue(string value)
    {
        using var http = new HttpClient();
        var path = Path.GetTempPath();
        var module = new ColecoEmulationModule(path, path, http, path);
        var configuration = new MachineConfiguration(MachineModel.ColecoVision, core: Emulator.BlueMSX);
        var settings = module.Describe(configuration.MachineId, configuration);
        var fields = settings.Blocks.SelectMany(block => block.Fields).ToArray();
        var boot = Assert.Single(fields, field => field.Id == "patch_coleco_rom");
        Assert.Equal(EmulationMachineTab.General, boot.Tab);
        Assert.Equal(EmulationSettingsEditor.Toggle, boot.Editor);
        Assert.Equal("disabled", boot.Value);
        Assert.Equal("enabled", boot.EnabledValue);
        Assert.Equal("disabled", boot.DisabledValue);
        Assert.True(module.TryGetString(boot.LabelResourceKey, CultureInfo.GetCultureInfo("fr-FR"), out _));
        foreach (var key in new[] { "bluemsx_overscan", "bluemsx_vdp_synctype", "bluemsx_nospritelimits" })
            Assert.Equal(EmulationMachineTab.Video, Assert.Single(fields, field => field.Id == key).Tab);
        var changed = Assert.IsType<MachineConfiguration>(module.ApplySettings(configuration, new Dictionary<string, string?> { [boot.Id] = value }));
        var restored = JsonSerializer.Deserialize<MachineConfiguration>(JsonSerializer.Serialize(changed))!;
        Assert.Equal(value, restored.Options[boot.Id]);
        Assert.Equal(value, module.RuntimeOptions(restored)[boot.Id]);
    }

    [Theory]
    [InlineData(Emulator.GearColeco)]
    [InlineData(Emulator.JollyCV)]
    [InlineData(Emulator.BlueMSX)]
    public void PhysicalMouseDoesNotReachTheCoreWithoutAnExplicitBinding(Emulator core)
    {
        var physical = EmulationInputSnapshot.Empty with
        {
            Pointer = EmulationInputSnapshot.Empty.Pointer with { DeltaX = 12, DeltaY = -9, Left = true, Right = true, Middle = true, Wheel = 3 },
            Controllers = [EmulationControllerState.Empty with { Buttons = uint.MaxValue }]
        };
        var mapped = InputSnapshotFunctions.Apply(physical, new InputConfiguration(), MachineModel.ColecoVision, core);
        Assert.Equal(EmulationInputSnapshot.Empty.Pointer, mapped.Pointer);
        Assert.All(mapped.Controllers, controller => Assert.Equal(0u, controller.Buttons));
        for (uint id = 0; id <= 2; id++)
            Assert.Equal(0, GWGUI.Emulation.Coleco.Emulators.Common.Interop.Functions.NativeInputFunctions.State(mapped, 0, 2, 0, id));
    }

    [Theory]
    [InlineData(0, "SpinnerRight", 1, 0)]
    [InlineData(0, "SpinnerLeft", -1, 0)]
    [InlineData(1, "SpinnerRight", 0, 1)]
    [InlineData(1, "SpinnerLeft", 0, -1)]
    public void GearColecoRollerMapsExplicitDirectionsToBothNativeMouseAxes(int port, string action, int x, int y)
    {
        var input = new InputConfiguration(Controllers: [new(port, PeripheralCategory.Roller, Mappings: new Dictionary<string, string> { [action] = "Keyboard:Z" })]);
        var physical = EmulationInputSnapshot.Empty with
        {
            Keys = new HashSet<EmulationKey> { EmulationKey.Z },
            Pointer = EmulationInputSnapshot.Empty.Pointer with { DeltaX = 12, DeltaY = -9, Left = true }
        };
        var mapped = InputSnapshotFunctions.Apply(physical, input, MachineModel.ColecoVision, Emulator.GearColeco);
        Assert.Equal(x, mapped.Pointer.DeltaX);
        Assert.Equal(y, mapped.Pointer.DeltaY);
        Assert.False(mapped.Pointer.Left);
        Assert.Equal(x, GWGUI.Emulation.Coleco.Emulators.Common.Interop.Functions.NativeInputFunctions.State(mapped, 0, 2, 0, 0));
        Assert.Equal(y, GWGUI.Emulation.Coleco.Emulators.Common.Interop.Functions.NativeInputFunctions.State(mapped, 0, 2, 0, 1));
        Assert.Equal(EmulationInputSnapshot.Empty.Pointer, InputSnapshotFunctions.Apply(physical with { Keys = EmulationInputSnapshot.Empty.Keys }, input, MachineModel.ColecoVision, Emulator.GearColeco).Pointer);
    }

    [Theory]
    [InlineData(MachineModel.ColecoVision)]
    [InlineData(MachineModel.Adam)]
    public void EverySupportedControllerExposesOppositeDirectionsWithoutPhysicalDefaults(MachineModel model)
    {
        foreach (var core in new[] { Emulator.GearColeco, Emulator.JollyCV, Emulator.BlueMSX })
        {
            if (!ConfigurationFunctions.IsCompatible(model, core)) continue;
            var settings = InputSettingsFunctions.Describe(new MachineConfiguration(model, core: core));
            foreach (var port in settings.ControllerPorts)
            {
                Assert.Empty(port.Bindings.Values);
                foreach (var choice in port.ControllerChoices)
                {
                    var definitions = choice.BindingDefinitions!;
                    Assert.All(definitions, action => Assert.Empty(action.DefaultBinding));
                    foreach (var action in definitions.Where(action => action.OppositeDirectionId is not null))
                        Assert.Equal(action.Id, Assert.Single(definitions, other => other.Id == action.OppositeDirectionId).OppositeDirectionId);
                    if (choice.Id == PeripheralCategory.None.ToString()) Assert.Empty(definitions);
                    else
                    {
                        Assert.Equal("Down", Assert.Single(definitions, action => action.Id == "Up").OppositeDirectionId);
                        Assert.Equal("Right", Assert.Single(definitions, action => action.Id == "Left").OppositeDirectionId);
                        var spinner = choice.Id != PeripheralCategory.Standard.ToString() && core != Emulator.BlueMSX
                            && (core != Emulator.GearColeco || port.Number == 1 || choice.Id == PeripheralCategory.Roller.ToString());
                        if (spinner)
                            Assert.Equal("SpinnerRight", Assert.Single(definitions, action => action.Id == "SpinnerLeft").OppositeDirectionId);
                        else Assert.DoesNotContain(definitions, action => action.Id is "SpinnerLeft" or "SpinnerRight");
                        if (choice.Id == PeripheralCategory.SuperAction.ToString())
                        {
                            Assert.Equal(GWGUI.Emulation.Constants.EmulationControllerVisualIds.ColecoSuperActionController, choice.DefaultVisualId);
                            Assert.Equal(definitions, choice.VisualBindingDefinitions![choice.DefaultVisualId!]);
                            Assert.Equal(spinner, choice.VisualCommandIds!.ContainsKey(GWGUI.Emulation.Enums.EmulationControllerVisualControl.StickLeft));
                            Assert.Equal(spinner, choice.VisualCommandIds.ContainsKey(GWGUI.Emulation.Enums.EmulationControllerVisualControl.StickRight));
                            Assert.All(choice.VisualCommandIds.Values, id => Assert.Contains(definitions, action => action.Id == id));
                        }
                        if (choice.Id == PeripheralCategory.Roller.ToString())
                        {
                            Assert.Equal(GWGUI.Emulation.Constants.EmulationControllerVisualIds.ColecoRollerController, choice.DefaultVisualId);
                            var visualDefinitions = choice.VisualBindingDefinitions![choice.DefaultVisualId!];
                            Assert.Equal(definitions.Select(action => action.Id), visualDefinitions.Select(action => action.Id));
                            Assert.All(visualDefinitions, action => Assert.Empty(action.DefaultBinding));
                            var commands = choice.VisualCommandIds!;
                            Assert.Equal(4, commands.Count);
                            var negative = port.Number == 1 ? GWGUI.Emulation.Enums.EmulationControllerVisualControl.StickLeft : GWGUI.Emulation.Enums.EmulationControllerVisualControl.StickUp;
                            var positive = port.Number == 1 ? GWGUI.Emulation.Enums.EmulationControllerVisualControl.StickRight : GWGUI.Emulation.Enums.EmulationControllerVisualControl.StickDown;
                            Assert.Equal("SpinnerLeft", commands[negative]);
                            Assert.Equal("SpinnerRight", commands[positive]);
                            Assert.All(commands.Values, id => Assert.Contains(definitions, action => action.Id == id));
                            if (port.Number == 2)
                            {
                                Assert.Equal("Emulation.Controller.Action.Up", visualDefinitions.Single(action => action.Id == "SpinnerLeft").DisplayResourceKey);
                                Assert.Equal("Emulation.Controller.Action.Down", visualDefinitions.Single(action => action.Id == "SpinnerRight").DisplayResourceKey);
                            }
                        }
                    }
                }
            }
        }
    }

    [Theory]
    [InlineData(0, "SpinnerRight", 10)]
    [InlineData(0, "SpinnerLeft", 11)]
    [InlineData(1, "SpinnerRight", 10)]
    [InlineData(1, "SpinnerLeft", 11)]
    public void JollyCvRotaryCommandsReachTheirNativeButtonOnEachPort(int port, string action, uint nativeButton)
    {
        var input = new InputConfiguration(Controllers: [new(port, PeripheralCategory.Paddle, Mappings: new Dictionary<string, string> { [action] = "Keyboard:Z+Mouse:Middle" })]);
        var physical = EmulationInputSnapshot.Empty with
        {
            Keys = new HashSet<EmulationKey> { EmulationKey.Z },
            Pointer = EmulationInputSnapshot.Empty.Pointer with { Middle = true }
        };
        var mapped = InputSnapshotFunctions.Apply(physical, input, MachineModel.ColecoVision, Emulator.JollyCV);
        Assert.Equal(1, GWGUI.Emulation.Coleco.Emulators.Common.Interop.Functions.NativeInputFunctions.State(mapped, (uint)port, 1, 0, nativeButton));
        Assert.Equal(0u, mapped.Controllers[1 - port].Buttons);
        var released = InputSnapshotFunctions.Apply(physical with { Pointer = EmulationInputSnapshot.Empty.Pointer }, input, MachineModel.ColecoVision, Emulator.JollyCV);
        Assert.Equal(0, GWGUI.Emulation.Coleco.Emulators.Common.Interop.Functions.NativeInputFunctions.State(released, (uint)port, 1, 0, nativeButton));
    }

    [Theory]
    [InlineData(0, "Purple", short.MinValue)]
    [InlineData(0, "Blue", short.MaxValue)]
    [InlineData(1, "Purple", short.MinValue)]
    [InlineData(1, "Blue", short.MaxValue)]
    public void BlueMsxExtraControllerButtonsKeepTheirNativeCarrierOnBothPorts(int port, string action, short expected)
    {
        var input = new InputConfiguration(Controllers: [new(port, PeripheralCategory.SuperAction, Mappings: new Dictionary<string, string> { [action] = "Keyboard:Z" })]);
        var physical = EmulationInputSnapshot.Empty with { Keys = new HashSet<EmulationKey> { EmulationKey.Z } };
        var mapped = InputSnapshotFunctions.Apply(physical, input, MachineModel.ColecoVision, Emulator.BlueMSX);
        Assert.Equal(expected, GWGUI.Emulation.Coleco.Emulators.Common.Interop.Functions.NativeInputFunctions.State(mapped, (uint)port, 5, 1, 1));
        Assert.Equal(0, mapped.Controllers[1 - port].RightY);
        Assert.Equal(0, GWGUI.Emulation.Coleco.Emulators.Common.Interop.Functions.NativeInputFunctions.State(
            InputSnapshotFunctions.Apply(EmulationInputSnapshot.Empty, input, MachineModel.ColecoVision, Emulator.BlueMSX), (uint)port, 5, 1, 1));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BlueMsxAcceptsRawCartridgesWithoutChangingOtherAdapters(bool requiresFilePath)
    {
        var engine = new GWGUI.Emulation.Coleco.Common.Services.Engine();
        var formats = Assert.IsAssignableFrom<GWGUI.Emulation.Coleco.Common.Interfaces.IEmulatorContentFormats>(engine.Adapter(Emulator.BlueMSX));
        var declared = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "col", "rom" };
        var supported = formats.SupportedContentExtensions(declared, requiresFilePath);
        Assert.Contains("bin", supported);
        Assert.Contains("col", supported);
        Assert.DoesNotContain("bin", declared);
        Assert.False(engine.Adapter(Emulator.GearColeco) is GWGUI.Emulation.Coleco.Common.Interfaces.IEmulatorContentFormats);
        Assert.False(engine.Adapter(Emulator.JollyCV) is GWGUI.Emulation.Coleco.Common.Interfaces.IEmulatorContentFormats);
    }
    [Fact]
    public void CartridgeMapperIsConfiguredAndPersistedThroughItsDevice()
    {
        var configuration = new MachineConfiguration(MachineModel.ColecoVision);
        var storage = StorageSettingsFunctions.Describe(configuration);
        var cartridge = storage.AvailableDevices.Single(device => device.Slot == EmulationMediaSlot.Cartridge0);
        Assert.Equal(EmulationStorageConfigurationKind.CartridgeSlot, cartridge.ConfigurationKind);
        var field = Assert.Single(cartridge.ConfigurationFields!);
        Assert.Equal("gearcoleco_mapper", field.Id);
        var value = field.Choices!.Last().Id;
        var changed = storage with { DeviceSettings = [new(EmulationMediaSlot.Cartridge0, Cartridge: new CartridgeSlotSettings(Options: new Dictionary<string, string> { [field.Id] = value }))] };
        var applied = StorageSettingsFunctions.Apply(configuration, changed);
        var restored = JsonSerializer.Deserialize<MachineConfiguration>(JsonSerializer.Serialize(applied))!;
        Assert.Equal(value, restored.Options[field.Id]);
        Assert.Equal(value, StorageSettingsFunctions.Describe(restored).AvailableDevices.Single(device => device.Slot == EmulationMediaSlot.Cartridge0).ConfigurationFields!.Single().Value);
        Assert.Throws<ArgumentException>(() => StorageSettingsFunctions.Apply(configuration, changed with
        {
            DeviceSettings = [new(EmulationMediaSlot.Cartridge0, Cartridge: new CartridgeSlotSettings(Options: new Dictionary<string, string> { [field.Id] = "invalid" }))]
        }));
        Assert.Equal(EmulationStorageConfigurationKind.None, StorageSettingsFunctions.Describe(new MachineConfiguration(MachineModel.ColecoVision, core: Emulator.JollyCV))
            .AvailableDevices.Single(device => device.Slot == EmulationMediaSlot.Cartridge0).ConfigurationKind);
    }
    [Fact]
    public async Task SelectedCoreRemainsSelectedInTheManagerAndFieldsUseTheirActualTab()
    {
        using var http = new HttpClient();
        var directory = Path.GetTempPath();
        var module = new ColecoEmulationModule(directory, directory, http, directory);
        GWGUI.Emulation.Interfaces.IEmulationEmulatorManager manager = module;
        IEmulationConfiguration config = new MachineConfiguration(MachineModel.ColecoVision);
        foreach (var installation in await manager.GetEmulatorInstallationsAsync(config))
        {
            config = await manager.UseEmulatorAsync(config, installation.EmulatorId);
            Assert.Equal(installation.EmulatorId, (await manager.GetEmulatorInstallationAsync(config)).EmulatorId);
        }
        var settings = module.Describe(MachineModel.ColecoVision.ToString());
        var fields = settings.Blocks.SelectMany(block => block.Fields).ToArray();
        Assert.DoesNotContain(fields, field => field.Tab == EmulationMachineTab.General);
        Assert.Equal(EmulationMachineTab.Video, fields.Single(field => field.Id == "gearcoleco_timing").Tab);
        Assert.Equal(EmulationMachineTab.Video, fields.Single(field => field.Id == "gearcoleco_video_chip").Tab);
        Assert.DoesNotContain(fields, field => field.Id == "gearcoleco_mapper");
    }
    [Theory]
    [InlineData(MachineModel.ColecoVision)]
    [InlineData(MachineModel.Adam)]
    public void EveryModelDescribesWithoutInstallationAndHasNoFolderSelectors(MachineModel model)
    {
        using var http = new HttpClient();
        var path = Path.GetTempPath();
        var module = new ColecoEmulationModule(path, path, http, path);
        var config = new MachineConfiguration(model);
        var settings = module.Describe(model.ToString(), config);
        Assert.Equal(model == MachineModel.Adam, settings.Visibility.Tabs[EmulationMachineTab.Keyboard]);
        Assert.DoesNotContain(settings.Blocks.SelectMany(block => block.Fields), field => field.Editor == EmulationSettingsEditor.DirectoryPath);
        Assert.Equal(2, module.DescribeInputSettings(config).ControllerPorts.Count);
        foreach (var field in settings.Blocks.SelectMany(block => block.Fields))
            if (field.LabelResourceKey.StartsWith("Emulation.Coleco.", StringComparison.Ordinal))
                Assert.True(module.TryGetString(field.LabelResourceKey, CultureInfo.GetCultureInfo("fr-FR"), out _), field.LabelResourceKey);
        foreach(var machine in module.Machines)
            Assert.NotNull(typeof(ColecoEmulationModule).Assembly.GetManifestResourceInfo(machine.ImageResourceName!));
    }

    [Theory]
    [InlineData(Emulator.GearColeco, 1)]
    [InlineData(Emulator.JollyCV, 0)]
    [InlineData(Emulator.BlueMSX, 9)]
    public void KeypadReassignmentUsesBothNativePorts(Emulator core, int bit)
    {
        var input = new InputConfiguration(Controllers: Enumerable.Range(0, 2).Select(port => new ControllerBinding(port, PeripheralCategory.Standard, Mappings: new Dictionary<string,string> { ["Keypad1"] = "Keyboard:Z" })).ToArray());
        var snapshot = EmulationInputSnapshot.Empty with { Keys = new HashSet<EmulationKey> { EmulationKey.Z } };
        var result = InputSnapshotFunctions.Apply(snapshot, input, MachineModel.ColecoVision, core);
        var expected = (1u << bit) | (core == Emulator.JollyCV ? 1u << 12 : 0);
        Assert.All(result.Controllers, controller => Assert.Equal(expected, controller.Buttons));
        Assert.All(InputSnapshotFunctions.Apply(EmulationInputSnapshot.Empty, input, MachineModel.ColecoVision, core).Controllers, controller => Assert.Equal(0u, controller.Buttons));
    }

    [Fact]
    public void AdamSpecialKeysAreModifiableAndSurviveJson()
    {
        var config = new MachineConfiguration(MachineModel.Adam, input: new InputConfiguration(Keyboard: new Dictionary<string,string> { ["Smart1"] = "Z" }));
        var restored = JsonSerializer.Deserialize<MachineConfiguration>(JsonSerializer.Serialize(config))!;
        var pressed = EmulationInputSnapshot.Empty with { Keys = new HashSet<EmulationKey> { EmulationKey.Z } };
        Assert.Contains(EmulationKey.AdamSmart1, InputSnapshotFunctions.Apply(pressed, restored.Input, restored.Model, restored.Core).Keys);
        var old = EmulationInputSnapshot.Empty with { Keys = new HashSet<EmulationKey> { EmulationKey.F1 } };
        Assert.DoesNotContain(EmulationKey.AdamSmart1, InputSnapshotFunctions.Apply(old, restored.Input, restored.Model, restored.Core).Keys);
        Assert.Equal(12, InputSettingsFunctions.Describe(restored).Keyboard!.Definitions.Count);
    }

    [Fact]
    public async Task ChangingCoreDropsForeignOptionsAndRetainsMappings()
    {
        using var http = new HttpClient();var path = Path.GetTempPath();
        var module = new ColecoEmulationModule(path,path,http,path);
        var config = new MachineConfiguration(MachineModel.ColecoVision,
            options: new Dictionary<string,string> { ["gearcoleco_timing"] = "Auto" },
            input: new InputConfiguration(Controllers: [new(0, PeripheralCategory.Roller, Mappings: new Dictionary<string,string> { ["FireLeft"] = "Keyboard:Z" })]));
        var selected = (MachineConfiguration)await module.UseEmulatorAsync(config,"bluemsx");
        Assert.Empty(selected.Options);
        Assert.Equal(PeripheralCategory.Standard, selected.Input.Controllers![0].Peripheral);
        Assert.Equal("Keyboard:Z", selected.Input.Controllers[0].Mappings!["FireLeft"]);
        Assert.Throws<ArgumentException>(() => new MachineConfiguration(MachineModel.Adam, core: Emulator.BlueMSX));
    }

    [Fact]
    public void ConfiguredAdamDevicesRemainWithoutMountedMedia()
    {
        var config = new MachineConfiguration(MachineModel.Adam);
        var storage = StorageSettingsFunctions.Describe(config);
        Assert.Equal(5, storage.AvailableDevices.Count);
        var applied = StorageSettingsFunctions.Apply(config, storage with { ConfiguredSlots = storage.AvailableDevices.Select(device => device.Slot).ToArray() });
        Assert.Equal(5, applied.ConfiguredSlots.Count);
        Assert.Empty(applied.Media);
        Assert.Throws<ArgumentException>(() => new MachineConfiguration(MachineModel.ColecoVision, input: new InputConfiguration(Controllers: [new(0, PeripheralCategory.Roller)]), core: Emulator.BlueMSX));
    }
    [Fact]
    public void AdamSpecialKeyCanBeUnassigned()
    {
        var config = new MachineConfiguration(MachineModel.Adam, input: new InputConfiguration(Keyboard: new Dictionary<string,string> { ["Smart1"] = string.Empty }));
        var pressed = EmulationInputSnapshot.Empty with { Keys = new HashSet<EmulationKey> { EmulationKey.F1 } };
        Assert.DoesNotContain(EmulationKey.AdamSmart1, InputSnapshotFunctions.Apply(pressed, config.Input, config.Model, config.Core).Keys);
    }
}
