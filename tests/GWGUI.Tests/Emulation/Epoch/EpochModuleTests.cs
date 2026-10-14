using System.Text.Json;
using System.Globalization;
using GWGUI.Emulation.Epoch.Modules;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Epoch.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Epoch.Common.Machines.Common.Enums;
using GWGUI.Emulation.Epoch.Common.Machines.Common.Functions;
using GWGUI.Emulation.Epoch.Emulators.EmuSCV.Factories;
using GWGUI.Emulation.Epoch.Emulators.PD777.Factories;
using GWGUI.Emulation.Epoch.Emulators.Common.Interop.Functions;
using NativeInput = GWGUI.Emulation.Epoch.Emulators.Common.Interop.Constants.InputConstants;

namespace GWGUI.Tests.Emulation.Epoch;

public sealed class EpochModuleTests
{
    [Theory]
    [InlineData(MachineModel.CassetteVision)]
    [InlineData(MachineModel.CassetteVisionJr)]
    [InlineData(MachineModel.SuperCassetteVision)]
    [InlineData(MachineModel.SuperCassetteVisionYeno)]
    [InlineData(MachineModel.SuperCassetteVisionLady)]
    public void EveryNativeDirectionPublishesItsOppositeInTheEditor(MachineModel model)
    {
        var settings = InputSettingsFunctions.Describe(new MachineConfiguration(model));
        foreach (var port in settings.ControllerPorts)
        {
            var definitions = port.Bindings.Definitions.ToDictionary(action => action.Id, StringComparer.Ordinal);
            var analog = definitions.Values.Where(action => action.Id.StartsWith("Paddle", StringComparison.Ordinal)
                || action.Id.StartsWith("GunX", StringComparison.Ordinal) || action.Id.StartsWith("GunY", StringComparison.Ordinal)).ToArray();
            var cassette = model is MachineModel.CassetteVision or MachineModel.CassetteVisionJr;
            Assert.Equal(cassette ? (model == MachineModel.CassetteVision ? 4 : 0) + (port.Number == 1 ? 4 : 0) : 0, analog.Length);
            foreach (var action in analog)
            {
                Assert.Empty(action.DefaultBinding);
                Assert.NotNull(action.OppositeDirectionId);
                Assert.Equal(action.Id, definitions[action.OppositeDirectionId].OppositeDirectionId);
            }
            if (cassette)
            {
                Assert.Equal("LeverRight", definitions["LeverLeft"].OppositeDirectionId);
                Assert.Equal("LeverLeft", definitions["LeverRight"].OppositeDirectionId);
                Assert.All(definitions.Values.Except(analog).Where(action => action.Id is not ("LeverLeft" or "LeverRight")), action => Assert.Null(action.OppositeDirectionId));
            }
            else
            {
                Assert.Empty(port.Bindings.Values);
                Assert.Equal(4, definitions.Values.Count(action => action.OppositeDirectionId is not null));
                Assert.All(definitions.Values, action => Assert.Empty(action.DefaultBinding));
                Assert.Equal("Down", definitions["Up"].OppositeDirectionId);
                Assert.Equal("Right", definitions["Left"].OppositeDirectionId);
                foreach (var action in definitions.Values.Where(action => action.OppositeDirectionId is not null))
                    Assert.Equal(action.Id, definitions[action.OppositeDirectionId!].OppositeDirectionId);
            }
        }
    }

    [Theory]
    [InlineData(0, "Paddle2")]
    [InlineData(1, "Paddle3")]
    public void PaddlesKeepProgressivePhysicalValuesAndIndependentKeyboardDirections(int port, string paddle)
    {
        var mapper = new PD777MachineFactory();
        var sources = Enumerable.Repeat(EmulationControllerState.Empty, 2).ToArray();
        sources[port] = sources[port] with { LeftX = 8192 };
        var snapshot = EmulationInputSnapshot.Empty with { Controllers = sources };
        var unassigned = mapper.MapInput(snapshot, new InputConfiguration(), MachineModel.CassetteVision);
        Assert.All(unassigned.Controllers, controller => Assert.Equal(0, controller.LeftX));
        var input = new InputConfiguration(Controllers: [new(port, PeripheralCategory.Standard, Mappings: new Dictionary<string, string>
        {
            [paddle + "Decrease"] = "Controller:LeftStickLeft", [paddle + "Increase"] = "Controller:LeftStickRight"
        }, DeadZonePercent: 0)]);
        var mapped = mapper.MapInput(snapshot, input, MachineModel.CassetteVision);
        Assert.InRange(mapped.Controllers[port].LeftX, 8191, 8193);
        Assert.Equal(mapped.Controllers[port].LeftX, NativeInputFunctions.State(mapped, (uint)port, NativeInput.AnalogDevice, NativeInput.LeftAnalogIndex, NativeInput.AnalogXId));
        sources[port] = sources[port] with { LeftX = -16384 };
        mapped = mapper.MapInput(snapshot with { Controllers = sources }, input, MachineModel.CassetteVision);
        Assert.InRange(mapped.Controllers[port].LeftX, -16385, -16383);
        Assert.Equal(0, mapped.Controllers[1 - port].LeftX);
        input = new InputConfiguration(Controllers: [new(port, PeripheralCategory.Standard, Mappings: new Dictionary<string, string>
        {
            [paddle + "Decrease"] = "Keyboard:A", [paddle + "Increase"] = "Keyboard:D"
        })]);
        mapped = mapper.MapInput(EmulationInputSnapshot.Empty with { Keys = new HashSet<EmulationKey> { EmulationKey.A } }, input, MachineModel.CassetteVision);
        Assert.Equal(-short.MaxValue, mapped.Controllers[port].LeftX);
        mapped = mapper.MapInput(EmulationInputSnapshot.Empty with { Keys = new HashSet<EmulationKey> { EmulationKey.D } }, input, MachineModel.CassetteVision);
        Assert.Equal(short.MaxValue, mapped.Controllers[port].LeftX);
        mapped = mapper.MapInput(EmulationInputSnapshot.Empty with { Keys = new HashSet<EmulationKey> { EmulationKey.A, EmulationKey.D } }, input, MachineModel.CassetteVision);
        Assert.Equal(0, mapped.Controllers[port].LeftX);
    }
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


