using System.Text.Json;
using System.Globalization;
using GWGUI.Emulation.Epoch.Modules;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Epoch.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Epoch.Common.Machines.Common.Enums;
using GWGUI.Emulation.Epoch.Common.Machines.Common.Functions;
using GWGUI.Emulation.Epoch.Emulators.EmuSCV.Factories;

namespace GWGUI.Tests.Emulation.Epoch;

public sealed class EpochModuleTests
{
    [Fact]
    public void EveryDisplayedEpochLabelResolvesFromTheActualModuleCatalog()
    {
        using var http = new System.Net.Http.HttpClient();
        var module = new EpochEmulationModule("unused", "unused", http, "unused");
        var cultures = typeof(EpochEmulationModule).Assembly.GetManifestResourceNames().Where(name => name.EndsWith(".resources", StringComparison.Ordinal)).Select(name => name[..^10].Split('.').Last()).Distinct()
            .Where(name => name != "00-Base").Select(CultureInfo.GetCultureInfo).ToArray();
        foreach (var model in Enum.GetValues<MachineModel>())
        {
            var config = new MachineConfiguration(model);
            var input = module.DescribeInputSettings(config);
            var keys = module.Describe(model.ToString(), config).Blocks.SelectMany(block => block.Fields).Select(field => field.LabelResourceKey)
                .Concat(input.ControllerPorts.SelectMany(port => port.ControllerChoices).Select(choice => choice.DisplayResourceKey))
                .Concat(input.ControllerPorts.SelectMany(port => port.Bindings.Definitions).Select(action => action.DisplayResourceKey))
                .Where(key => key.StartsWith("Emulation.Epoch.", StringComparison.Ordinal)).Distinct();
            foreach (var culture in cultures)
                foreach (var key in keys)
                    Assert.True(module.TryGetString(key, culture, out var text) && !string.IsNullOrWhiteSpace(text), culture.Name + ": " + key);
        }
    }
    [Theory]
    [InlineData(MachineModel.CassetteVision, Emulator.PD777)]
    [InlineData(MachineModel.CassetteVisionJr, Emulator.PD777)]
    [InlineData(MachineModel.SuperCassetteVision, Emulator.EmuSCV)]
    [InlineData(MachineModel.SuperCassetteVisionYeno, Emulator.EmuSCV)]
    [InlineData(MachineModel.SuperCassetteVisionLady, Emulator.EmuSCV)]
    public void ModelUsesItsNativeCoreAndEditablePhysicalControls(MachineModel model, Emulator core)
    {
        var config = new MachineConfiguration(model);
        Assert.Equal(core, config.Core);
        var settings = InputSettingsFunctions.Describe(config);
        Assert.Equal(2, settings.ControllerPorts.Count);
        foreach (var port in settings.ControllerPorts)
        {
            var choice = Assert.Single(port.ControllerChoices);
            if (core == Emulator.EmuSCV)
                Assert.Contains(choice.DefaultVisualId!, choice.CompatibleVisualIds!);
            else
                Assert.Empty(choice.CompatibleVisualIds!);
            Assert.All(choice.VisualCommandIds!, pair => Assert.Contains(port.Bindings.Definitions, action => action.Id == pair.Value));
            Assert.DoesNotContain(port.Bindings.Definitions, action => action.Id.Contains("Amiga", StringComparison.Ordinal));
        }
        var fields = SettingsDescriptionFunctions.Create(config).SelectMany(block => block.Fields);
        Assert.DoesNotContain(fields, field => field.Editor == EmulationSettingsEditor.DirectoryPath);
        Assert.Single(StorageSettingsFunctions.Describe(config).AvailableDevices);
    }

    [Fact]
    public void ReassignedConsolePowerSurvivesPersistenceWithoutItsOldHostAlias()
    {
        var config = new MachineConfiguration(MachineModel.SuperCassetteVision, input: new InputConfiguration(Controllers: [new(0, PeripheralCategory.Standard, Mappings: new Dictionary<string, string> { ["Power"] = "Keyboard:Z" })]));
        var restored = JsonSerializer.Deserialize<MachineConfiguration>(JsonSerializer.Serialize(config))!;
        var pressed = EmulationInputSnapshot.Empty with { Keys = new HashSet<EmulationKey> { EmulationKey.Z } };
        var mapper = new EmuSCVMachineFactory();
        Assert.Contains(EmulationKey.EpochPower, InputSnapshotFunctions.Apply(pressed, restored.Input, restored.Model, mapper).Keys);
        var old = EmulationInputSnapshot.Empty with { Keys = new HashSet<EmulationKey> { EmulationKey.P } };
        Assert.DoesNotContain(EmulationKey.EpochPower, InputSnapshotFunctions.Apply(old, restored.Input, restored.Model, mapper).Keys);
    }

    [Fact]
    public void UnassignedActionsAndSelectedVisualRemainValidAfterApplyingEditorValues()
    {
        var config = new MachineConfiguration(MachineModel.SuperCassetteVision);
        var settings = InputSettingsFunctions.Describe(config);
        var ports = settings.ControllerPorts.Select(port => port with
        {
            VisualId = port.ControllerChoices[0].DefaultVisualId,
            Bindings = port.Bindings with { Values = port.Bindings.Definitions.ToDictionary(action => action.Id, _ => string.Empty) }
        }).ToArray();
        var applied = InputSettingsFunctions.Apply(config, settings with { ControllerPorts = ports });
        var restored = JsonSerializer.Deserialize<MachineConfiguration>(JsonSerializer.Serialize(applied))!;
        Assert.All(restored.Input.Controllers!, port => Assert.All(port.Mappings!, pair => Assert.Empty(pair.Value)));
        Assert.Equal(ports[0].VisualId, InputSettingsFunctions.Describe(restored).ControllerPorts[0].VisualId);
    }
}


