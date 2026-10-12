using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Bandai.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Bandai.Common.Machines.Common.Dictionaries;
using GWGUI.Emulation.Bandai.Common.Machines.Common.Enums;
using GWGUI.Emulation.Bandai.Common.Machines.Common.Functions;
using GWGUI.Emulation.Bandai.Modules;

namespace GWGUI.Tests.Emulation.Bandai;

public sealed class BandaiModuleTests
{
    [Theory]
    [InlineData(MachineModel.WonderSwan)]
    [InlineData(MachineModel.WonderSwanColor)]
    [InlineData(MachineModel.SwanCrystal)]
    [InlineData(MachineModel.Playdia)]
    [InlineData(MachineModel.TamagotchiP1)]
    public void UnassignedEditorRowsSurviveApplyAndPersistence(MachineModel model)
    {
        var configuration = new MachineConfiguration(model);
        var settings = InputSettingsFunctions.Describe(configuration);
        var port = Assert.Single(settings.ControllerPorts);
        var values = port.Bindings.Definitions.ToDictionary(definition => definition.Id, _ => string.Empty);
        var applied = InputSettingsFunctions.Apply(configuration, settings with
        {
            ControllerPorts = [port with { Bindings = port.Bindings with { Values = values } }]
        });
        var restored = RoundTrip(applied);
        Assert.Equal(values.Count, Assert.Single(restored.Input.Controllers!).Mappings!.Count);
        Assert.All(restored.Input.Controllers![0].Mappings!, mapping => Assert.Equal(string.Empty, mapping.Value));
        Assert.Equal(0u, Assert.Single(InputSnapshotFunctions.Apply(EmulationInputSnapshot.Empty, restored.Input, model).Controllers).Buttons);
        Assert.Throws<ArgumentException>(() => InputSettingsFunctions.Apply(configuration, settings with
        {
            ControllerPorts = [port with { Bindings = port.Bindings with
                { Values = new Dictionary<string, string> { ["UnknownAction"] = string.Empty } } }]
        }));
    }

    [Theory]
    [InlineData(MachineModel.WonderSwan, 12, true, false, true)]
    [InlineData(MachineModel.WonderSwanColor, 12, true, false, true)]
    [InlineData(MachineModel.SwanCrystal, 12, true, false, true)]
    [InlineData(MachineModel.Playdia, 8, false, false, true)]
    [InlineData(MachineModel.TamagotchiP1, 3, true, true, false)]
    public void EveryMachineHasOnlyRelevantTabsAndControls(MachineModel model, int buttons,
        bool hardware, bool rom, bool storage)
    {
        using var http = new HttpClient();
        var path = Path.GetTempPath();
        var module = new BandaiEmulationModule(path, path, http, path);
        var configuration = new MachineConfiguration(model);
        var settings = module.Describe(model.ToString(), configuration);
        Assert.Equal(hardware, settings.Visibility.Tabs[EmulationMachineTab.Cpu]);
        Assert.Equal(hardware, settings.Visibility.Tabs[EmulationMachineTab.Ram]);
        Assert.Equal(rom, settings.Visibility.Tabs[EmulationMachineTab.Rom]);
        Assert.Equal(storage, settings.Visibility.Tabs[EmulationMachineTab.Storage]);
        Assert.False(settings.Visibility.Tabs[EmulationMachineTab.Keyboard]);
        Assert.False(settings.Visibility.Tabs[EmulationMachineTab.Mouse]);
        Assert.DoesNotContain(settings.Blocks.SelectMany(block => block.Fields), field => field.Editor == EmulationSettingsEditor.DirectoryPath);
        Assert.DoesNotContain(settings.Blocks.SelectMany(block => block.Fields), field => field.Tab == EmulationMachineTab.General && field.Id.StartsWith("wswan_", StringComparison.Ordinal));
        var input = InputSettingsFunctions.Describe(configuration);
        Assert.Null(input.Keyboard);
        Assert.Null(input.Mouse);
        Assert.Equal(buttons, Assert.Single(input.ControllerPorts).Bindings.Definitions.Count);
        var controller = Assert.Single(Assert.Single(input.ControllerPorts).ControllerChoices);
        Assert.NotEqual(PeripheralCategory.None.ToString(), controller.Id);
        Assert.Single(controller.CompatibleVisualIds!);
        Assert.Contains(model.ToString(), module.Machines.Select(machine => machine.Id));
    }

    [Theory]
    [InlineData(MachineModel.WonderSwan, "A", 8)]
    [InlineData(MachineModel.Playdia, "A", 8)]
    [InlineData(MachineModel.TamagotchiP1, "Execute", 0)]
    public void PhysicalKeyReassignmentIsPersistedAndApplied(MachineModel model, string action, int nativeBit)
    {
        var configuration = new MachineConfiguration(model);
        var settings = InputSettingsFunctions.Describe(configuration);
        var port = Assert.Single(settings.ControllerPorts);
        var applied = InputSettingsFunctions.Apply(configuration, settings with
        {
            ControllerPorts = [port with { Bindings = port.Bindings with
                { Values = new Dictionary<string, string> { [action] = "Keyboard:Z" } } }]
        });
        var restored = RoundTrip(applied);
        Assert.Equal("Keyboard:Z", Assert.Single(restored.Input.Controllers!).Mappings![action]);
        var pressed = EmulationInputSnapshot.Empty with { Keys = new HashSet<EmulationKey> { EmulationKey.Z } };
        Assert.Equal(1u << nativeBit, Assert.Single(InputSnapshotFunctions.Apply(pressed, restored.Input, model).Controllers).Buttons);
        var oldKey = EmulationInputSnapshot.Empty with { Keys = new HashSet<EmulationKey> { EmulationKey.X } };
        Assert.Equal(0u, Assert.Single(InputSnapshotFunctions.Apply(oldKey, restored.Input, model).Controllers).Buttons);
    }

    [Theory]
    [InlineData(MachineModel.WonderSwan, true)]
    [InlineData(MachineModel.Playdia, false)]
    public void FixedDeviceRemainsWithoutMountedMediaAfterReload(MachineModel model, bool cartridge)
    {
        var slot = cartridge ? EmulationMediaSlot.Cartridge0 : EmulationMediaSlot.Cd0;
        var restored = RoundTrip(new MachineConfiguration(model));
        var storage = StorageSettingsFunctions.Describe(restored);
        Assert.Equal(slot, Assert.Single(storage.ConfiguredSlots));
        Assert.True(Assert.Single(storage.AvailableDevices).IsPermanent);
        Assert.Empty(storage.MountedMedia);
        Assert.Throws<ArgumentException>(() => StorageSettingsFunctions.Apply(restored,
            storage with { ConfiguredSlots = [] }));
    }

    [Fact]
    public void IncompatibleCoreAndNativeValuesAreRejected()
    {
        Assert.Throws<ArgumentException>(() => new MachineConfiguration(MachineModel.Playdia, core: Emulator.BeetleWonderSwan));
        Assert.Throws<ArgumentException>(() => new MachineConfiguration(MachineModel.WonderSwan,
            options: new Dictionary<string, string> { ["wswan_rotate_display"] = "invalid" }));
    }

    [Fact]
    public void NativeDefaultsRemainProtocolValuesAndAreAccepted()
    {
        foreach (var model in Enum.GetValues<MachineModel>())
        {
            var configuration = new MachineConfiguration(model);
            var nativeFields = SettingsDescriptionFunctions.Create(configuration).SelectMany(block => block.Fields)
                .Where(field => field.Id.StartsWith("wswan_", StringComparison.Ordinal)
                    || field.Id.StartsWith("playdiaemu_", StringComparison.Ordinal));
            _ = new MachineConfiguration(model, options: nativeFields.ToDictionary(field => field.Id, field => field.Value!));
        }
    }

    [Fact]
    public void AllArtworkAndLocalizedDescriptionsResolve()
    {
        using var http = new HttpClient();
        var path = Path.GetTempPath();
        var module = new BandaiEmulationModule(path, path, http, path);
        var resources = typeof(BandaiEmulationModule).Assembly.GetManifestResourceNames();
        Assert.Contains(module.BrandImageResourceName, resources);
        foreach (var machine in module.Machines)
        {
            Assert.Contains(machine.ImageResourceName!, resources);
            foreach (var culture in new[] { CultureInfo.GetCultureInfo("en-US"), CultureInfo.GetCultureInfo("fr-FR") })
            {
                Assert.True(module.TryGetString(machine.DisplayResourceKey, culture, out var name));
                Assert.False(string.IsNullOrWhiteSpace(name));
                foreach (var field in module.Describe(machine.Id).Blocks.SelectMany(block => block.Fields).Where(field => field.LabelResourceKey.StartsWith("Emulation.Bandai.", StringComparison.Ordinal)))
                    Assert.True(module.TryGetString(field.LabelResourceKey, culture, out _), field.LabelResourceKey);
            }
        }
    }

    private static MachineConfiguration RoundTrip(MachineConfiguration configuration)
    {
        var pathBase = Path.GetTempPath();
        var document = ConfigurationStoreFunctions.ToDocument(configuration, pathBase);
        var json = JsonSerializer.SerializeToElement(document, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        return ConfigurationStoreFunctions.FromDocument(ConfigurationStoreFunctions.Deserialize(json), pathBase);
    }
}
