using System.Net.Http;
using System.Text.Json;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Watara.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Watara.Common.Machines.Common.Enums;
using GWGUI.Emulation.Watara.Common.Machines.Common.Functions;
using GWGUI.Emulation.Watara.Emulators.Potator.Interop.Services;
using GWGUI.Emulation.Watara.Modules;

namespace GWGUI.Tests.Emulation.Watara;

public sealed class WataraInputSettingsTests
{
    [Fact]
    public void MixedBindingPersistsAndReachesPotatorCallback()
    {
        using var http = new HttpClient();
        var module = new WataraEmulationModule("unused", "unused", http, "unused");
        var initial = new MachineConfiguration(MachineModel.Supervision, core: Emulator.Potator);
        var settings = module.DescribeInputSettings(initial);
        var port = Assert.Single(settings.ControllerPorts);
        const string chord = "Keyboard:RightAlt+Mouse:Middle+Controller:A";
        var changed = Assert.IsType<MachineConfiguration>(module.ApplyInputSettings(initial, settings with
        {
            ControllerPorts = [port with { Bindings = port.Bindings with { Values = new Dictionary<string, string> { ["A"] = chord } } }]
        }));
        var restored = JsonSerializer.Deserialize<MachineConfiguration>(JsonSerializer.Serialize(changed))!;
        Assert.Equal(chord, Assert.Single(restored.Input.Controllers!).Mappings!["A"]);
        var physical = EmulationInputSnapshot.Empty with
        {
            Keys = new HashSet<EmulationKey> { EmulationKey.RightAlt },
            Pointer = EmulationInputSnapshot.Empty.Pointer with { Middle = true },
            Controllers = [EmulationControllerState.Empty with { Buttons = 1u << 8 }]
        };
        var callback = new InputCallbackHost();
        callback.Update(InputSnapshotFunctions.Apply(physical, restored.Input, restored.Model, restored.Core));
        Assert.Equal(1, callback.StateCallback(0, 1, 0, 8));
        foreach (var released in new[]
        {
            physical with { Keys = EmulationInputSnapshot.Empty.Keys },
            physical with { Pointer = EmulationInputSnapshot.Empty.Pointer },
            physical with { Controllers = [EmulationControllerState.Empty] }
        })
        {
            callback.Update(InputSnapshotFunctions.Apply(released, restored.Input, restored.Model, restored.Core));
            Assert.Equal(0, callback.StateCallback(0, 1, 0, 8));
        }
        callback.Release();
        Assert.Equal(0, callback.StateCallback(0, 1, 0, 8));
    }

    [Theory]
    [InlineData("A", "Mouse:NotAButton")]
    [InlineData("A", "Controller:")]
    [InlineData("A", "Keyboard:NotAKey")]
    [InlineData("UnknownCommand", "Keyboard:A")]
    public void InvalidSourcesAndCommandsAreRejected(string command, string source)
    {
        using var http = new HttpClient();
        var module = new WataraEmulationModule("unused", "unused", http, "unused");
        var initial = new MachineConfiguration(MachineModel.Supervision, core: Emulator.Potator);
        var settings = module.DescribeInputSettings(initial);
        var port = Assert.Single(settings.ControllerPorts);
        Assert.Throws<ArgumentException>(() => module.ApplyInputSettings(initial, settings with
        {
            ControllerPorts = [port with { Bindings = port.Bindings with { Values = new Dictionary<string, string> { [command] = source } } }]
        }));
    }

    [Fact]
    public void DirectionsArePairedAndUnassignedInputStaysNeutral()
    {
        using var http = new HttpClient();
        var module = new WataraEmulationModule("unused", "unused", http, "unused");
        var initial = new MachineConfiguration(MachineModel.Supervision, core: Emulator.Potator);
        var port = Assert.Single(module.DescribeInputSettings(initial).ControllerPorts);
        Assert.Empty(port.Bindings.Values);
        Assert.All(port.Bindings.Definitions, item => Assert.Empty(item.DefaultBinding));
        var directions = port.Bindings.Definitions.ToDictionary(item => item.Id);
        Assert.Equal("Right", directions["Left"].OppositeDirectionId);
        Assert.Equal("Left", directions["Right"].OppositeDirectionId);
        Assert.Equal("Down", directions["Up"].OppositeDirectionId);
        Assert.Equal("Up", directions["Down"].OppositeDirectionId);
        var physical = EmulationInputSnapshot.Empty with { Controllers = [EmulationControllerState.Empty with { Buttons = uint.MaxValue, LeftX = short.MaxValue }] };
        var mapped = InputSnapshotFunctions.Apply(physical, initial.Input, initial.Model, initial.Core);
        Assert.Equal(0u, Assert.Single(mapped.Controllers).Buttons);
        Assert.Equal(0, Assert.Single(mapped.Controllers).LeftX);
    }
}
