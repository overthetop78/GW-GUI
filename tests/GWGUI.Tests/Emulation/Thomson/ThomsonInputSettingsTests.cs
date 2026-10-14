using System.Net.Http;
using System.Text.Json;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Thomson.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Thomson.Common.Machines.Common.Enums;
using GWGUI.Emulation.Thomson.Common.Machines.Common.Functions;
using GWGUI.Emulation.Thomson.Emulators.Theodore.Interop.Functions;
using GWGUI.Emulation.Thomson.Emulators.Theodore.Interop.Services;
using GWGUI.Emulation.Thomson.Modules;

namespace GWGUI.Tests.Emulation.Thomson;

public sealed class ThomsonInputSettingsTests
{
    [Theory]
    [InlineData(MachineModel.Mo5)]
    [InlineData(MachineModel.Mo6)]
    [InlineData(MachineModel.To7)]
    [InlineData(MachineModel.To770)]
    [InlineData(MachineModel.To8)]
    [InlineData(MachineModel.To8D)]
    [InlineData(MachineModel.To9)]
    [InlineData(MachineModel.To9Plus)]
    public void MixedStopBindingPersistsAndReachesNativeCallback(MachineModel model)
    {
        using var http = new HttpClient();
        var module = new ThomsonEmulationModule("unused", "unused", http, "unused");
        var initial = new MachineConfiguration(model);
        var settings = module.DescribeInputSettings(initial);
        Assert.Equal(EmulationInputSource.Keyboard | EmulationInputSource.Mouse | EmulationInputSource.Controller, settings.Keyboard!.Sources);
        const string chord = "Keyboard:RightAlt+Mouse:Middle+Controller:A";
        var values = new Dictionary<string, string>(settings.Keyboard.Values) { [nameof(EmulationKey.ThomsonStop)] = chord };
        var changed = Assert.IsType<MachineConfiguration>(module.ApplyInputSettings(initial, settings with { Keyboard = settings.Keyboard with { Values = values } }));
        var restored = JsonSerializer.Deserialize<MachineConfiguration>(JsonSerializer.Serialize(changed))!;
        Assert.Equal(chord, restored.Input.Keyboard![nameof(EmulationKey.ThomsonStop)]);
        var snapshot = EmulationInputSnapshot.Empty with
        {
            Keys = new HashSet<EmulationKey> { EmulationKey.RightAlt, EmulationKey.Z },
            Pointer = EmulationInputSnapshot.Empty.Pointer with { Middle = true },
            Controllers = [EmulationControllerState.Empty with { Buttons = 1u << 8 }]
        };
        var mapped = InputSnapshotFunctions.Apply(snapshot, restored.Input, model, restored.Core);
        Assert.Contains(EmulationKey.ThomsonStop, mapped.Keys);
        Assert.Contains(EmulationKey.Z, mapped.Keys);
        var state = new KeyboardState(model);
        var events = new List<(bool Pressed, uint Code)>();
        state.Publish(mapped.Keys, (pressed, code, character, modifiers) => events.Add((pressed, code)));
        var nativeCode = KeyboardFunctions.CreateKeyMap(model)[EmulationKey.ThomsonStop];
        Assert.Contains((true, nativeCode), events);
        foreach (var released in new[]
        {
            snapshot with { Keys = new HashSet<EmulationKey> { EmulationKey.Z } },
            snapshot with { Pointer = snapshot.Pointer with { Middle = false } },
            snapshot with { Controllers = [EmulationControllerState.Empty] }
        })
        {
            mapped = InputSnapshotFunctions.Apply(released, restored.Input, model, restored.Core);
            Assert.DoesNotContain(EmulationKey.ThomsonStop, mapped.Keys);
            events.Clear();
            state.Publish(snapshot.Keys.Append(EmulationKey.ThomsonStop).ToHashSet(), (pressed, code, character, modifiers) => { });
            state.Publish(mapped.Keys, (pressed, code, character, modifiers) => events.Add((pressed, code)));
            Assert.Contains((false, nativeCode), events);
        }
    }

    [Theory]
    [InlineData("LeftControl")]
    [InlineData("Keyboard:LeftControl")]
    public void SimpleBindingStillSubstitutesItsPhysicalKey(string source)
    {
        var input = new InputConfiguration(Keyboard: new Dictionary<string, string> { [nameof(EmulationKey.ThomsonStop)] = source });
        var physical = EmulationInputSnapshot.Empty with { Keys = new HashSet<EmulationKey> { EmulationKey.LeftControl, EmulationKey.Z } };
        var mapped = InputSnapshotFunctions.Apply(physical, input, MachineModel.Mo5, Emulator.Theodore);
        Assert.Contains(EmulationKey.ThomsonStop, mapped.Keys);
        Assert.Contains(EmulationKey.Z, mapped.Keys);
        Assert.DoesNotContain(EmulationKey.LeftControl, mapped.Keys);
    }

    [Fact]
    public void MouseAndBothPortsAcceptMixedBindingsWithoutImplicitControllerMapping()
    {
        using var http = new HttpClient();
        var module = new ThomsonEmulationModule("unused", "unused", http, "unused");
        var initial = new MachineConfiguration(MachineModel.Mo5);
        var settings = module.DescribeInputSettings(initial);
        foreach (var port in settings.ControllerPorts)
        {
            Assert.Empty(port.Bindings.Values);
            var directions = port.Bindings.Definitions.ToDictionary(item => item.Id);
            Assert.All(directions.Values, item => Assert.Empty(item.DefaultBinding));
            Assert.Equal("Right", directions["Left"].OppositeDirectionId);
            Assert.Equal("Left", directions["Right"].OppositeDirectionId);
            Assert.Equal("Down", directions["Up"].OppositeDirectionId);
            Assert.Equal("Up", directions["Down"].OppositeDirectionId);
        }
        const string chord = "Keyboard:LeftControl+Keyboard:A+Mouse:Middle";
        var changed = Assert.IsType<MachineConfiguration>(module.ApplyInputSettings(initial, settings with
        {
            Mouse = settings.Mouse! with { Values = new Dictionary<string, string> { ["Left"] = chord } },
            ControllerPorts = settings.ControllerPorts.Select(port => port with
            {
                Bindings = port.Bindings with { Values = new Dictionary<string, string> { ["Fire"] = chord } }
            }).ToArray()
        }));
        var snapshot = EmulationInputSnapshot.Empty with
        {
            Keys = new HashSet<EmulationKey> { EmulationKey.LeftControl, EmulationKey.A },
            Pointer = EmulationInputSnapshot.Empty.Pointer with { Middle = true }
        };
        var mapped = InputSnapshotFunctions.Apply(snapshot, changed.Input, changed.Model, changed.Core);
        Assert.True(mapped.Pointer.Left);
        Assert.All(mapped.Controllers, item => Assert.NotEqual(0u, item.Buttons));
        mapped = InputSnapshotFunctions.Apply(snapshot with { Pointer = EmulationInputSnapshot.Empty.Pointer }, changed.Input, changed.Model, changed.Core);
        Assert.False(mapped.Pointer.Left);
        Assert.All(mapped.Controllers, item => Assert.Equal(0u, item.Buttons));
    }
}
