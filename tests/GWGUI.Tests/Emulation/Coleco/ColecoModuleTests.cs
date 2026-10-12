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
        var changed = storage with { DeviceSettings = [new(EmulationMediaSlot.Cartridge0,
            Cartridge: new CartridgeSlotSettings(Options: new Dictionary<string, string> { [field.Id] = value }))] };
        var applied = StorageSettingsFunctions.Apply(configuration, changed);
        var restored = JsonSerializer.Deserialize<MachineConfiguration>(JsonSerializer.Serialize(applied))!;
        Assert.Equal(value, restored.Options[field.Id]);
        Assert.Equal(value, StorageSettingsFunctions.Describe(restored).AvailableDevices.Single(device => device.Slot == EmulationMediaSlot.Cartridge0).ConfigurationFields!.Single().Value);
        Assert.Throws<ArgumentException>(() => StorageSettingsFunctions.Apply(configuration, changed with
        {
            DeviceSettings = [new(EmulationMediaSlot.Cartridge0,
                Cartridge: new CartridgeSlotSettings(Options: new Dictionary<string, string> { [field.Id] = "invalid" }))]
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
        var input = new InputConfiguration(Controllers: Enumerable.Range(0, 2).Select(port =>
            new ControllerBinding(port, PeripheralCategory.Standard, Mappings: new Dictionary<string,string> { ["Keypad1"] = "Keyboard:Z" })).ToArray());
        var snapshot = EmulationInputSnapshot.Empty with { Keys = new HashSet<EmulationKey> { EmulationKey.Z } };
        var result = InputSnapshotFunctions.Apply(snapshot, input, MachineModel.ColecoVision, core);
        var expected = (1u << bit) | (core == Emulator.JollyCV ? 1u << 12 : 0);
        Assert.All(result.Controllers, controller => Assert.Equal(expected, controller.Buttons));
        Assert.All(InputSnapshotFunctions.Apply(EmulationInputSnapshot.Empty, input, MachineModel.ColecoVision, core).Controllers, controller => Assert.Equal(0u, controller.Buttons));
    }

    [Fact]
    public void AdamSpecialKeysAreModifiableAndSurviveJson()
    {
        var config = new MachineConfiguration(MachineModel.Adam, input: new InputConfiguration(
            Keyboard: new Dictionary<string,string> { ["Smart1"] = "Z" }));
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
            input: new InputConfiguration(Controllers: [new(0, PeripheralCategory.Roller,
                Mappings: new Dictionary<string,string> { ["FireLeft"] = "Keyboard:Z" })]));
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
        var config = new MachineConfiguration(MachineModel.Adam, input: new InputConfiguration(
            Keyboard: new Dictionary<string,string> { ["Smart1"] = string.Empty }));
        var pressed = EmulationInputSnapshot.Empty with { Keys = new HashSet<EmulationKey> { EmulationKey.F1 } };
        Assert.DoesNotContain(EmulationKey.AdamSmart1, InputSnapshotFunctions.Apply(pressed, config.Input, config.Model, config.Core).Keys);
    }
}
