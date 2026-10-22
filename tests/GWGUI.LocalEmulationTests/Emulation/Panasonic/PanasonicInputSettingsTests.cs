using System.Net.Http;
using System.Text.Json;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Panasonic.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Panasonic.Common.Machines.Common.Enums;
using GWGUI.Emulation.Panasonic.Common.Machines.Common.Functions;
using GWGUI.Emulation.Panasonic.Emulators.Opera.Interop.Enums;
using GWGUI.Emulation.Panasonic.Emulators.Opera.Interop.Services;
using GWGUI.Emulation.Panasonic.Modules;

namespace GWGUI.Tests.Emulation.Panasonic;

public sealed class PanasonicInputSettingsTests
{
    [Fact]
    public void MixedBindingsPersistAndReachEveryNativeControllerPort()
    {
        using var http = new HttpClient();
        var module = new PanasonicEmulationModule("unused", "unused", http, "unused");
        var initial = new MachineConfiguration(MachineModel.ThreeDO, input: new(Enumerable.Range(0, 8).Select(port => new ControllerBinding(port, DeviceId: "pad")).ToArray()), core: Emulator.Opera);
        var settings = module.DescribeInputSettings(initial);
        Assert.Equal(8, settings.ControllerPorts.Count);
        const string chord = "Keyboard:RightAlt+Mouse:Middle+Controller:pad:A";
        var changed = Assert.IsType<MachineConfiguration>(module.ApplyInputSettings(initial, settings with
        {
            ControllerPorts = settings.ControllerPorts.Select(port => port with
            {
                Bindings = port.Bindings with { Values = new Dictionary<string, string> { ["A"] = chord } }
            }).ToArray()
        }));
        var restored = JsonSerializer.Deserialize<MachineConfiguration>(JsonSerializer.Serialize(changed))!;
        Assert.All(restored.Input.Controllers!, binding => Assert.Equal(chord, binding.Mappings!["A"]));
        var physical = EmulationInputSnapshot.Empty with
        {
            Keys = new HashSet<EmulationKey> { EmulationKey.RightAlt },
            Pointer = EmulationInputSnapshot.Empty.Pointer with { Middle = true },
            Controllers = [EmulationControllerState.Empty with { DeviceId = "pad", Buttons = 1u << 8 }]
        };
        var callback = new InputCallbackHost();
        for (var port = 0; port < 8; port++) callback.SetPeripheral(port, PeripheralCategory.ThreeDOPad);
        callback.Update(InputSnapshotFunctions.Apply(physical, restored.Input, restored.Model, restored.Core), 320, 240);
        for (uint port = 0; port < 8; port++) Assert.Equal(1, callback.StateCallback(port, (uint)NativeDevice.Joypad, 0, (uint)NativeButton.Y));
        foreach (var released in new[]
        {
            physical with { Keys = EmulationInputSnapshot.Empty.Keys },
            physical with { Pointer = EmulationInputSnapshot.Empty.Pointer },
            physical with { Controllers = [EmulationControllerState.Empty with { DeviceId = "pad" }] }
        })
        {
            callback.Update(InputSnapshotFunctions.Apply(released, restored.Input, restored.Model, restored.Core), 320, 240);
            for (uint port = 0; port < 8; port++) Assert.Equal(0, callback.StateCallback(port, (uint)NativeDevice.Joypad, 0, (uint)NativeButton.Y));
        }
        Assert.All(settings.ControllerPorts, port => Assert.Empty(port.Bindings.Values));
    }

    [Theory]
    [InlineData("MoveRight", "Mouse:X+", 7, 0, (uint)NativeMouseControl.X)]
    [InlineData("MoveLeft", "Mouse:X−", -7, 0, (uint)NativeMouseControl.X)]
    [InlineData("MoveDown", "Mouse:Y+", 0, 9, (uint)NativeMouseControl.Y)]
    [InlineData("MoveUp", "Mouse:Y−", 0, -9, (uint)NativeMouseControl.Y)]
    public void RelativeMouseMotionStillReachesNativeMouse(string command, string source, int x, int y, uint control)
    {
        using var http = new HttpClient();
        var module = new PanasonicEmulationModule("unused", "unused", http, "unused");
        var initial = new MachineConfiguration(MachineModel.ThreeDO, input: new([new(0, PeripheralCategory.Mouse)]), core: Emulator.Opera);
        var settings = module.DescribeInputSettings(initial);
        var ports = settings.ControllerPorts.ToArray();
        ports[0] = ports[0] with { Bindings = ports[0].Bindings with { Values = new Dictionary<string, string> { [command] = source } } };
        var changed = Assert.IsType<MachineConfiguration>(module.ApplyInputSettings(initial, settings with { ControllerPorts = ports }));
        var physical = EmulationInputSnapshot.Empty with { Pointer = EmulationInputSnapshot.Empty.Pointer with { DeltaX = x, DeltaY = y } };
        var mapped = InputSnapshotFunctions.Apply(physical, changed.Input, changed.Model, changed.Core);
        var callback = new InputCallbackHost();
        callback.SetPeripheral(0, PeripheralCategory.Mouse);
        callback.Update(mapped, 320, 240);
        Assert.Equal(control == (uint)NativeMouseControl.X ? x : y, callback.StateCallback(0, (uint)NativeDevice.Mouse, 0, control));
    }

    [Fact]
    public void AnalogPressureAndDirectionPairsRemainAvailable()
    {
        using var http = new HttpClient();
        var module = new PanasonicEmulationModule("unused", "unused", http, "unused");
        var initial = new MachineConfiguration(MachineModel.ThreeDO, input: new([new(0, PeripheralCategory.Flightstick)]), core: Emulator.Opera);
        var settings = module.DescribeInputSettings(initial);
        var directions = settings.ControllerPorts[0].Bindings.Definitions.ToDictionary(item => item.Id);
        foreach (var pair in new[] { ("Up", "Down"), ("Left", "Right"), ("PitchUp", "PitchDown"), ("YawLeft", "YawRight"), ("ThrottleUp", "ThrottleDown") })
        {
            Assert.Equal(pair.Item2, directions[pair.Item1].OppositeDirectionId);
            Assert.Equal(pair.Item1, directions[pair.Item2].OppositeDirectionId);
        }
        Assert.All(directions.Values, item => Assert.Empty(item.DefaultBinding));
        var ports = settings.ControllerPorts.ToArray();
        ports[0] = ports[0] with { DeadZonePercent = 0, Bindings = ports[0].Bindings with { Values = new Dictionary<string, string> { ["PitchDown"] = "Keyboard:LeftControl+Controller:pad:LeftStickDown" } } };
        var changed = Assert.IsType<MachineConfiguration>(module.ApplyInputSettings(initial, settings with { ControllerPorts = ports }));
        var physical = EmulationInputSnapshot.Empty with
        {
            Keys = new HashSet<EmulationKey> { EmulationKey.LeftControl },
            Controllers = [EmulationControllerState.Empty with { DeviceId = "pad", LeftY = short.MaxValue / 4 }]
        };
        var mapped = InputSnapshotFunctions.Apply(physical, changed.Input, changed.Model, changed.Core);
        Assert.InRange(mapped.Controllers[0].Controls["PitchDown"], .249f, .251f);
        var callback = new InputCallbackHost();
        callback.SetPeripheral(0, PeripheralCategory.Flightstick);
        callback.Update(mapped, 320, 240);
        Assert.Equal(short.MaxValue / 4, callback.StateCallback(0, (uint)NativeDevice.Analog, 0, 1));
        mapped = InputSnapshotFunctions.Apply(physical with { Keys = EmulationInputSnapshot.Empty.Keys }, changed.Input, changed.Model, changed.Core);
        Assert.Equal(0f, mapped.Controllers[0].Controls["PitchDown"]);
        callback.Update(mapped, 320, 240);
        Assert.Equal(0, callback.StateCallback(0, (uint)NativeDevice.Analog, 0, 1));
    }
}
