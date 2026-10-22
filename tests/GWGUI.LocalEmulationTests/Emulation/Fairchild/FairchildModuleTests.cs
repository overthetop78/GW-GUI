using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Fairchild.Modules;
using GWGUI.Emulation.Fairchild.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Fairchild.Common.Machines.Common.Enums;
using GWGUI.Emulation.Fairchild.Common.Machines.Common.Functions;
using GWGUI.Emulation.Fairchild.Emulators.FreeChaF.Factories;

namespace GWGUI.Tests.Emulation.Fairchild;

public sealed class FairchildModuleTests
{
    [Theory]
    [InlineData(MachineModel.ChannelF)]
    [InlineData(MachineModel.ChannelFSystemII)]
    public void ControllerDirectionsExposeOppositeCommandsWithoutPhysicalDefaults(MachineModel model)
    {
        var settings = InputSettingsFunctions.Describe(new MachineConfiguration(model));
        Assert.Equal(2, settings.ControllerPorts.Count);
        foreach (var port in settings.ControllerPorts)
        {
            Assert.Empty(port.Bindings.Values);
            var definitions = port.Bindings.Definitions;
            Assert.All(definitions, action => Assert.Empty(action.DefaultBinding));
            Assert.Equal(8, definitions.Count(action => action.OppositeDirectionId is not null));
            foreach (var action in definitions.Where(action => action.OppositeDirectionId is not null))
                Assert.Equal(action.Id, Assert.Single(definitions, other => other.Id == action.OppositeDirectionId).OppositeDirectionId);
            Assert.Equal("Down", Assert.Single(definitions, action => action.Id == "Up").OppositeDirectionId);
            Assert.Equal("Right", Assert.Single(definitions, action => action.Id == "Left").OppositeDirectionId);
            Assert.Equal("RotateRight", Assert.Single(definitions, action => action.Id == "RotateLeft").OppositeDirectionId);
            Assert.Equal("Pull", Assert.Single(definitions, action => action.Id == "Push").OppositeDirectionId);
        }
    }

    [Theory]
    [InlineData(MachineModel.ChannelF)]
    [InlineData(MachineModel.ChannelFSystemII)]
    public void EveryDisplayedModuleLabelResolvesInEveryEmbeddedCulture(MachineModel model)
    {
        var http = new HttpClient();
        try
        {
            var module = new FairchildEmulationModule("unused", "unused", http, "unused");
            var configuration = new MachineConfiguration(model);
            var fields = module.Describe(model.ToString(), configuration).Blocks.SelectMany(block => block.Fields).ToArray();
            var input = module.DescribeInputSettings(configuration);
            var keys = fields.Select(field => field.LabelResourceKey).Concat(fields.Select(field => field.ExplanationResourceKey).OfType<string>())
                .Concat(fields.SelectMany(field => field.Choices ?? []).Select(choice => choice.DisplayResourceKey))
                .Concat(input.ControllerPorts.SelectMany(port => port.ControllerChoices).Select(choice => choice.DisplayResourceKey))
                .Concat(input.ControllerPorts.SelectMany(port => port.Bindings.Definitions).Select(action => action.DisplayResourceKey))
                .Append(module.Machines.Single(machine => machine.Id == model.ToString()).DisplayResourceKey)
                .Where(key => key.StartsWith("Emulation.Fairchild.", StringComparison.Ordinal)).Distinct().ToArray();
            var cultures = typeof(FairchildEmulationModule).Assembly.GetManifestResourceNames().Where(name => name.EndsWith(".resources", StringComparison.Ordinal)).Select(name => name[..^10].Split('.').Last()).Distinct().Where(name => name != "00-Base").ToArray();
            Assert.Equal(29, cultures.Length);
            foreach (var name in cultures)
                foreach (var key in keys)
                    Assert.True(module.TryGetString(key, CultureInfo.GetCultureInfo(name), out var text) && !string.IsNullOrWhiteSpace(text), name + ": " + key);
            Assert.DoesNotContain(fields, field => field.Editor == EmulationSettingsEditor.DirectoryPath);
            Assert.Equal(2, fields.Count(field => field.Tab == EmulationMachineTab.Rom));
        }
        finally { http.Dispose(); }
    }

    [Fact]
    public void ReassignedHandleMovementSurvivesSerializationAndSuppressesTheOldControllerSource()
    {
        var config = new MachineConfiguration(MachineModel.ChannelF, input: new InputConfiguration(Controllers: [new(0, PeripheralCategory.Standard, Mappings: new Dictionary<string, string> { ["Push"] = "Keyboard:Z" })]));
        var restored = JsonSerializer.Deserialize<MachineConfiguration>(JsonSerializer.Serialize(config))!;
        var keyboard = EmulationInputSnapshot.Empty with { Keys = new HashSet<EmulationKey> { EmulationKey.Z } };
        var mapper = new FreeChaFMachineFactory();
        Assert.NotEqual(0u, InputSnapshotFunctions.Apply(keyboard, restored, mapper).Controllers[0].Buttons & 1u);
        var original = EmulationInputSnapshot.Empty with { Controllers = [EmulationControllerState.Empty with { Buttons = 1u }] };
        Assert.Equal(0u, InputSnapshotFunctions.Apply(original, restored, mapper).Controllers[0].Buttons & 1u);
    }

    [Fact]
    public void EmptyAssignmentsAndTheSelectedVisualSurviveApplyingEditorValues()
    {
        var config = new MachineConfiguration(MachineModel.ChannelFSystemII);
        var settings = InputSettingsFunctions.Describe(config);
        Assert.All(settings.ControllerPorts, port => Assert.All(port.Bindings.Definitions, action => Assert.Empty(action.DefaultBinding)));
        var physical = EmulationInputSnapshot.Empty with
        {
            Keys = new HashSet<EmulationKey> { EmulationKey.Up, EmulationKey.Z },
            Controllers = [EmulationControllerState.Empty with { Buttons = uint.MaxValue }]
        };
        var mapper = new FreeChaFMachineFactory();
        Assert.All(InputSnapshotFunctions.Apply(physical, config, mapper).Controllers, port => Assert.Equal(0u, port.Buttons));
        var ports = settings.ControllerPorts.Select(port => port with
        {
            VisualId = port.ControllerChoices[0].DefaultVisualId,
            Bindings = port.Bindings with { Values = port.Bindings.Definitions.ToDictionary(action => action.Id, _ => string.Empty) }
        }).ToArray();
        var applied = InputSettingsFunctions.Apply(config, settings with { ControllerPorts = ports });
        var restored = JsonSerializer.Deserialize<MachineConfiguration>(JsonSerializer.Serialize(applied))!;
        Assert.All(restored.Input.Controllers!, port => Assert.All(port.Mappings!, pair => Assert.Empty(pair.Value)));
        Assert.Equal(ports[0].VisualId, InputSettingsFunctions.Describe(restored).ControllerPorts[0].VisualId);
        Assert.All(InputSnapshotFunctions.Apply(EmulationInputSnapshot.Empty, restored, mapper).Controllers, port => Assert.Equal(0u, port.Buttons));
    }
}
