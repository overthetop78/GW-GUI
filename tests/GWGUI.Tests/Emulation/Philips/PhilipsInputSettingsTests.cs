using System.Net.Http;
using System.Text.Json;
using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Philips.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Philips.Common.Machines.Common.Enums;
using GWGUI.Emulation.Philips.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Philips.Emulators.M2000.Factories;
using GWGUI.Emulation.Philips.Emulators.SameCdi.Factories;
using GWGUI.Emulation.Philips.Modules;
using NativeInput = GWGUI.Emulation.Philips.Emulators.Common.Interop.Constants.InputConstants;

namespace GWGUI.Tests.Emulation.Philips;

public sealed class PhilipsInputSettingsTests
{
    [Fact]
    public void P2000MixedBindingPersistsAndRequiresEverySourceToRemainPressed()
    {
        using var http = new HttpClient();
        var module = new PhilipsEmulationModule("unused", "unused", http, "unused");
        var original = new MachineConfiguration(MachineModel.P2000T);
        var settings = module.DescribeInputSettings(original);
        Assert.Equal(EmulationInputSource.Keyboard | EmulationInputSource.Mouse | EmulationInputSource.Controller, settings.Keyboard!.Sources);
        var target = EmulationKey.PhilipsP2000Start;
        const string binding = "Keyboard:LeftControl+Mouse:Left+Controller:A";
        var values = new Dictionary<string, string>(settings.Keyboard.Values) { [target.ToString()] = binding };
        var changed = Assert.IsType<MachineConfiguration>(module.ApplyInputSettings(original, settings with { Keyboard = settings.Keyboard with { Values = values } }));
        var restored = JsonSerializer.Deserialize<MachineConfiguration>(JsonSerializer.Serialize(changed))!;
        Assert.Equal(binding, restored.Input.Keyboard![target.ToString()]);
        const uint physicalButtonA = 1u << 8;
        var controller = EmulationControllerState.Empty with { Buttons = physicalButtonA };
        var snapshot = EmulationInputSnapshot.Empty with
        {
            Keys = new HashSet<EmulationKey> { EmulationKey.LeftControl, EmulationKey.Z },
            Pointer = EmulationInputSnapshot.Empty.Pointer with { Left = true }, Controllers = [controller]
        };
        var mapper = new M2000MachineFactory();
        var mapped = mapper.MapInput(snapshot, restored.Input, restored.Model);
        Assert.Contains(target, mapped.Keys);
        Assert.Contains(EmulationKey.Z, mapped.Keys);
        var nativeKey = KeyboardFunctions.CreateKeyMap(p2000: true)[target];
        Assert.Equal(1, NativeInputFunctions.State(mapped, NativeInput.PrimaryPort, NativeInput.KeyboardDevice, 0, nativeKey, Emulator.M2000));
        foreach (var released in new[]
        {
            snapshot with { Keys = new HashSet<EmulationKey> { EmulationKey.Z } },
            snapshot with { Pointer = snapshot.Pointer with { Left = false } },
            snapshot with { Controllers = [controller with { Buttons = 0 }] },
            EmulationInputSnapshot.Empty with { Keys = new HashSet<EmulationKey> { EmulationKey.F1 } }
        })
        {
            mapped = mapper.MapInput(released, restored.Input, restored.Model);
            Assert.DoesNotContain(target, mapped.Keys);
            Assert.Equal(0, NativeInputFunctions.State(mapped, NativeInput.PrimaryPort, NativeInput.KeyboardDevice, 0, nativeKey, Emulator.M2000));
        }
    }

    [Theory]
    [InlineData("RightAlt", EmulationKey.RightAlt)]
    [InlineData("LeftShift+A", EmulationKey.LeftShift)]
    public void P2000KeepsSingleModifiersAndKeyboardChordsUsable(string source, EmulationKey modifier)
    {
        var input = new InputConfiguration(Keyboard: new Dictionary<string, string> { [nameof(EmulationKey.PhilipsP2000Start)] = source });
        var snapshot = EmulationInputSnapshot.Empty with { Keys = new HashSet<EmulationKey> { modifier, EmulationKey.A } };
        var mapper = new M2000MachineFactory();
        Assert.Contains(EmulationKey.PhilipsP2000Start, mapper.MapInput(snapshot, input, MachineModel.P2000T).Keys);
        Assert.DoesNotContain(EmulationKey.PhilipsP2000Start, mapper.MapInput(EmulationInputSnapshot.Empty, input, MachineModel.P2000T).Keys);
    }

    [Fact]
    public void EveryControllerDirectionHasAnOppositeWithoutAssigningPhysicalSources()
    {
        using var http = new HttpClient();
        var module = new PhilipsEmulationModule("unused", "unused", http, "unused");
        var opposites = new Dictionary<string, string>
        {
            [EmulationControllerCommandIds.Up] = EmulationControllerCommandIds.Down,
            [EmulationControllerCommandIds.Down] = EmulationControllerCommandIds.Up,
            [EmulationControllerCommandIds.Left] = EmulationControllerCommandIds.Right,
            [EmulationControllerCommandIds.Right] = EmulationControllerCommandIds.Left
        };
        foreach (var model in Enum.GetValues<MachineModel>())
        foreach (var port in module.DescribeInputSettings(new MachineConfiguration(model)).ControllerPorts)
        foreach (var choice in port.ControllerChoices)
        foreach (var action in choice.BindingDefinitions!)
        {
            Assert.Empty(action.DefaultBinding);
            if (opposites.TryGetValue(action.Id, out var opposite))
            {
                Assert.Equal(opposite, action.OppositeDirectionId);
                Assert.Contains(choice.BindingDefinitions!, definition => definition.Id == opposite && definition.OppositeDirectionId == action.Id);
            }
            else Assert.Null(action.OppositeDirectionId);
        }
    }

    [Fact]
    public void CdiKeepsAnalogPressureUntilItsCallbackAndRequiresTheChordModifier()
    {
        var mapper = new SameCdiMachineFactory();
        var input = new InputConfiguration(Controllers: [new(0, PeripheralCategory.CdiGamepad, Mappings: new Dictionary<string, string>
        {
            [EmulationControllerCommandIds.Right] = "Keyboard:LeftControl+Controller:LeftStickRight"
        }, DeadZonePercent: 0)]);
        var snapshot = EmulationInputSnapshot.Empty with
        {
            Keys = new HashSet<EmulationKey> { EmulationKey.LeftControl }, Controllers = [EmulationControllerState.Empty with { LeftX = 8192 }]
        };
        var mapped = mapper.MapInput(snapshot, input, MachineModel.PhilipsCdi);
        Assert.InRange(mapped.Controllers[0].LeftX, 8191, 8193);
        Assert.Equal(mapped.Controllers[0].LeftX, NativeInputFunctions.State(mapped, 0, NativeInput.AnalogDevice, NativeInput.LeftAnalogIndex, NativeInput.AnalogXId, Emulator.SameCdi));
        Assert.Equal(0, mapper.MapInput(snapshot with { Keys = new HashSet<EmulationKey>() }, input, MachineModel.PhilipsCdi).Controllers[0].LeftX);
        Assert.Equal(0, mapper.MapInput(snapshot, new InputConfiguration(), MachineModel.PhilipsCdi).Controllers[0].LeftX);
    }
}
