using System.Net.Http;
using System.Text.Json;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Sinclair.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Sinclair.Common.Machines.Common.Enums;
using GWGUI.Emulation.Sinclair.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Sinclair.Modules;
using NativeInput = GWGUI.Emulation.Sinclair.Emulators.Common.Interop.Constants.InputConstants;

namespace GWGUI.Tests.Emulation.Sinclair;

public sealed class SinclairInputSettingsTests
{
    [Fact]
    public void SymbolShiftMixedBindingPersistsAndReachesTheNativeKeyboard()
    {
        using var http = new HttpClient();
        var module = new SinclairEmulationModule("unused", "unused", http, "unused");
        var initial = new MachineConfiguration(MachineModel.Spectrum48K, core: Emulator.Fuse);
        var settings = module.DescribeInputSettings(initial);
        Assert.Equal(EmulationInputSource.Keyboard | EmulationInputSource.Mouse | EmulationInputSource.Controller, settings.Keyboard!.Sources);
        const string chord = "Keyboard:RightAlt+Mouse:Middle+Controller:A";
        var values = new Dictionary<string, string>(settings.Keyboard.Values) { [nameof(EmulationKey.SinclairSymbolShift)] = chord };
        var changed = Assert.IsType<MachineConfiguration>(module.ApplyInputSettings(initial, settings with { Keyboard = settings.Keyboard with { Values = values } }));
        var restored = JsonSerializer.Deserialize<MachineConfiguration>(JsonSerializer.Serialize(changed))!;
        Assert.Equal(chord, restored.Input.KeyboardBindings![nameof(EmulationKey.SinclairSymbolShift)]);
        var physical = EmulationControllerState.Empty with { Buttons = 1u << 8 };
        var snapshot = EmulationInputSnapshot.Empty with
        {
            Keys = new HashSet<EmulationKey> { EmulationKey.RightAlt, EmulationKey.Z },
            Pointer = EmulationInputSnapshot.Empty.Pointer with { Middle = true }, Controllers = [physical]
        };
        var keyMap = KeyboardFunctions.CreateKeyMap(Emulator.Fuse);
        var targetCode = keyMap[EmulationKey.SinclairSymbolShift];
        var mapped = NativeInputMappingFunctions.Apply(snapshot, restored.Input, restored.Model, restored.Core);
        Assert.Contains(EmulationKey.SinclairSymbolShift, mapped.Keys);
        Assert.Contains(EmulationKey.Z, mapped.Keys);
        Assert.Equal(1, State(mapped));
        foreach (var released in new[]
        {
            snapshot with { Keys = new HashSet<EmulationKey> { EmulationKey.Z } },
            snapshot with { Pointer = snapshot.Pointer with { Middle = false } },
            snapshot with { Controllers = [EmulationControllerState.Empty] }
        })
        {
            mapped = NativeInputMappingFunctions.Apply(released, restored.Input, restored.Model, restored.Core);
            Assert.DoesNotContain(EmulationKey.SinclairSymbolShift, mapped.Keys);
            Assert.Equal(0, State(mapped));
        }
        short State(EmulationInputSnapshot input) => NativeInputFunctions.State(input, input.Keys.Where(keyMap.ContainsKey).Select(key => keyMap[key]).ToHashSet(),
            NativeInput.FuseKeyboardPort, NativeInput.SinclairKeyboard, 0, targetCode, Emulator.Fuse);
    }

    [Theory]
    [InlineData("LeftControl")]
    [InlineData("Keyboard:LeftControl")]
    public void SimpleSymbolShiftBindingStillSubstitutesItsPhysicalKey(string source)
    {
        var snapshot = EmulationInputSnapshot.Empty with { Keys = new HashSet<EmulationKey> { EmulationKey.LeftControl, EmulationKey.A } };
        var input = new InputConfiguration(KeyboardBindings: new Dictionary<string, string> { [nameof(EmulationKey.SinclairSymbolShift)] = source });
        var mapped = NativeInputMappingFunctions.Apply(snapshot, input, MachineModel.Spectrum48K, Emulator.Fuse);
        Assert.Contains(EmulationKey.SinclairSymbolShift, mapped.Keys);
        Assert.DoesNotContain(EmulationKey.LeftControl, mapped.Keys);
        Assert.Contains(EmulationKey.A, mapped.Keys);
    }

    [Fact]
    public void Zx81OrdinaryKeysRemainUnchangedWithoutSpecialBindings()
    {
        var snapshot = EmulationInputSnapshot.Empty with { Keys = new HashSet<EmulationKey> { EmulationKey.LeftShift, EmulationKey.A, EmulationKey.Numpad1 } };
        var mapped = NativeInputMappingFunctions.Apply(snapshot, new InputConfiguration(), MachineModel.Zx81, Emulator.EightyOne);
        Assert.True(snapshot.Keys.SetEquals(mapped.Keys));
    }

    [Fact]
    public void MouseAndBothControllerPortsAcceptMixedSources()
    {
        using var http = new HttpClient();
        var module = new SinclairEmulationModule("unused", "unused", http, "unused");
        var initial = new MachineConfiguration(MachineModel.Spectrum48K, core: Emulator.Fuse);
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
        Assert.Equal(2, changed.Input.Controllers!.Count);
        Assert.All(changed.Input.Controllers, binding => Assert.Equal(chord, binding.Mappings!["Fire1"]));
        Assert.Equal(chord, changed.Input.MouseMappings!["Left"]);
    }

    [Fact]
    public void DirectionPairsAreAvailableOnBothPortsWithoutPhysicalDefaults()
    {
        using var http = new HttpClient();
        var module = new SinclairEmulationModule("unused", "unused", http, "unused");
        var settings = module.DescribeInputSettings(new MachineConfiguration(MachineModel.Spectrum48K, core: Emulator.Fuse));
        var opposites = new Dictionary<string, string> { ["Up"] = "Down", ["Down"] = "Up", ["Left"] = "Right", ["Right"] = "Left" };
        Assert.Equal(2, settings.ControllerPorts.Count);
        foreach (var port in settings.ControllerPorts)
        foreach (var choice in port.ControllerChoices)
        foreach (var action in choice.BindingDefinitions!)
        {
            Assert.Empty(action.DefaultBinding);
            Assert.Equal(opposites.GetValueOrDefault(action.Id), action.OppositeDirectionId);
        }
    }

    [Fact]
    public void NoSelectedCoreDoesNotSelectAnUnavailableController()
    {
        using var http = new HttpClient();
        var module = new SinclairEmulationModule("unused", "unused", http, "unused");
        foreach (var model in Enum.GetValues<MachineModel>())
        {
            var settings = module.DescribeInputSettings(new MachineConfiguration(model, core: Emulator.None));
            foreach (var port in settings.ControllerPorts)
            {
                Assert.Equal(nameof(PeripheralCategory.None), port.SelectedControllerId);
                Assert.Contains(port.ControllerChoices, choice => choice.Id == port.SelectedControllerId);
                Assert.Empty(port.Bindings.Values);
            }
        }
    }
}
