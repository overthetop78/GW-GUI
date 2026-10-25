using System.Net.Http;
using System.Text.Json;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Sharp.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Sharp.Common.Machines.Common.Enums;
using GWGUI.Emulation.Sharp.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Sharp.Modules;
using NativeInput = GWGUI.Emulation.Sharp.Emulators.Common.Interop.Constants.InputConstants;

namespace GWGUI.Tests.Emulation.Sharp;

public sealed class SharpInputSettingsTests
{
    [Theory]
    [InlineData(MachineModel.X68000, Emulator.Px68k, EmulationKey.SharpOption1)]
    [InlineData(MachineModel.X1, Emulator.X1, EmulationKey.SharpGraph)]
    public void MixedKeyboardBindingPersistsAndReachesNativeInput(MachineModel model, Emulator emulator, EmulationKey target)
    {
        using var http = new HttpClient();
        var module = new SharpEmulationModule("unused", "unused", http, "unused");
        var initial = new MachineConfiguration(model, core: emulator);
        var settings = module.DescribeInputSettings(initial);
        Assert.Equal(EmulationInputSource.Keyboard | EmulationInputSource.Mouse | EmulationInputSource.Controller, settings.Keyboard!.Sources);
        const string chord = "Keyboard:RightControl+Mouse:Middle+Controller:A";
        var values = new Dictionary<string, string>(settings.Keyboard.Values) { [target.ToString()] = chord };
        var changed = Assert.IsType<MachineConfiguration>(module.ApplyInputSettings(initial, settings with { Keyboard = settings.Keyboard with { Values = values } }));
        var restored = JsonSerializer.Deserialize<MachineConfiguration>(JsonSerializer.Serialize(changed))!;
        Assert.Equal(chord, restored.Input.KeyboardBindings![target.ToString()]);
        var physical = EmulationControllerState.Empty with { Buttons = 1u << 8 };
        var snapshot = EmulationInputSnapshot.Empty with
        {
            Keys = new HashSet<EmulationKey> { EmulationKey.RightControl, EmulationKey.Z },
            Pointer = EmulationInputSnapshot.Empty.Pointer with { Middle = true }, Controllers = [physical]
        };
        var mapped = NativeInputMappingFunctions.Apply(snapshot, restored.Input, model, emulator);
        Assert.Contains(target, mapped.Keys);
        Assert.Contains(EmulationKey.Z, mapped.Keys);
        var keyMap = KeyboardFunctions.CreateKeyMap(emulator);
        var nativeCode = keyMap[target];
        Assert.Equal(1, NativeInputFunctions.State(mapped, Codes(mapped), 0, NativeInput.KeyboardDevice, 0, nativeCode, emulator));
        foreach (var released in new[]
        {
            snapshot with { Keys = new HashSet<EmulationKey> { EmulationKey.Z } },
            snapshot with { Pointer = snapshot.Pointer with { Middle = false } },
            snapshot with { Controllers = [EmulationControllerState.Empty] }
        })
        {
            mapped = NativeInputMappingFunctions.Apply(released, restored.Input, model, emulator);
            Assert.DoesNotContain(target, mapped.Keys);
            Assert.Equal(0, NativeInputFunctions.State(mapped, Codes(mapped), 0, NativeInput.KeyboardDevice, 0, nativeCode, emulator));
        }
        IReadOnlySet<uint> Codes(EmulationInputSnapshot input) => input.Keys.Where(keyMap.ContainsKey).Select(key => keyMap[key]).ToHashSet();
    }

    [Theory]
    [InlineData("LeftAlt")]
    [InlineData("Keyboard:LeftAlt")]
    public void SimpleKeyboardSourcesKeepTheExistingSubstitution(string source)
    {
        var snapshot = EmulationInputSnapshot.Empty with { Keys = new HashSet<EmulationKey> { EmulationKey.LeftAlt, EmulationKey.A } };
        var input = new InputConfiguration(KeyboardBindings: new Dictionary<string, string> { [nameof(EmulationKey.SharpGraph)] = source });
        var mapped = NativeInputMappingFunctions.Apply(snapshot, input, MachineModel.X1, Emulator.X1);
        Assert.Contains(EmulationKey.SharpGraph, mapped.Keys);
        Assert.DoesNotContain(EmulationKey.LeftAlt, mapped.Keys);
        Assert.Contains(EmulationKey.A, mapped.Keys);
    }

    [Fact]
    public void MouseAndControllerEditorsAcceptTheSameMixedKeyboardSources()
    {
        using var http = new HttpClient();
        var module = new SharpEmulationModule("unused", "unused", http, "unused");
        var initial = new MachineConfiguration(MachineModel.X68000, core: Emulator.Px68k);
        var settings = module.DescribeInputSettings(initial);
        const string chord = "Keyboard:LeftControl+Keyboard:A+Mouse:Middle";
        var ports = settings.ControllerPorts.Select(port => port with
        {
            Bindings = port.Bindings with { Values = new Dictionary<string, string> { ["Fire1"] = chord } }
        }).ToArray();
        var changed = Assert.IsType<MachineConfiguration>(module.ApplyInputSettings(initial, settings with
        {
            ControllerPorts = ports,
            Mouse = settings.Mouse! with { Values = new Dictionary<string, string> { ["Left"] = chord } }
        }));
        Assert.All(changed.Input.Controllers!, binding => Assert.Equal(chord, binding.Mappings!["Fire1"]));
        Assert.Equal(chord, changed.Input.MouseMappings!["Left"]);
    }
}
