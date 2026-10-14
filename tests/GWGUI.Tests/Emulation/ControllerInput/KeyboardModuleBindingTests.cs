using System.Net.Http;
using System.Text.Json;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Interfaces;
using AcornMachines = GWGUI.Emulation.Acorn.Common.Machines.Common;
using AppleMachines = GWGUI.Emulation.Apple.Common.Machines.Common;
using ColecoMachines = GWGUI.Emulation.Coleco.Common.Machines.Common;
using AcornKeyboard = GWGUI.Emulation.Acorn.Emulators.Common.Interop.Services.KeyboardState;
using AppleKeyboard = GWGUI.Emulation.Apple.Emulators.Common.Interop.Services.KeyboardState;
using ColecoKeyboard = GWGUI.Emulation.Coleco.Emulators.Common.Interop.Services.KeyboardState;

namespace GWGUI.Tests.Emulation.ControllerInput;

public sealed class KeyboardModuleBindingTests
{
    [Theory]
    [InlineData("Acorn")]
    [InlineData("Apple")]
    [InlineData("Coleco")]
    public void MixedSpecialKeyBindingPersistsAndReachesNativeCallback(string family)
    {
        using var http = new HttpClient();
        var events = new List<(bool Pressed, uint Code)>();
        var scenario = Create(family, http, events);
        var settings = scenario.Manager.DescribeInputSettings(scenario.Configuration);
        Assert.Equal(EmulationInputSource.Keyboard | EmulationInputSource.Mouse | EmulationInputSource.Controller, settings.Keyboard!.Sources);
        const string chord = "Keyboard:RightAlt+Mouse:Middle+Controller:A";
        var values = new Dictionary<string, string>(settings.Keyboard.Values) { [scenario.TargetId] = chord };
        var changed = scenario.Manager.ApplyInputSettings(scenario.Configuration, settings with { Keyboard = settings.Keyboard with { Values = values } });
        var restored = (IEmulationConfiguration)JsonSerializer.Deserialize(JsonSerializer.Serialize(changed, changed.GetType()), changed.GetType())!;
        Assert.Equal(chord, scenario.Manager.DescribeInputSettings(restored).Keyboard!.Values[scenario.TargetId]);
        var physical = EmulationInputSnapshot.Empty with
        {
            Keys = new HashSet<EmulationKey> { EmulationKey.RightAlt, EmulationKey.Z },
            Pointer = EmulationInputSnapshot.Empty.Pointer with { Middle = true },
            Controllers = [EmulationControllerState.Empty with { Buttons = 1u << 8 }]
        };
        var mapped = scenario.Map(physical, restored);
        Assert.Contains(scenario.Target, mapped.Keys);
        Assert.Contains(EmulationKey.Z, mapped.Keys);
        if (family == "Apple") Assert.DoesNotContain(EmulationKey.RightAlt, mapped.Keys);
        scenario.Publish(mapped.Keys);
        Assert.Contains((true, scenario.NativeCode), events);
        foreach (var released in new[]
        {
            physical with { Keys = new HashSet<EmulationKey> { EmulationKey.Z } },
            physical with { Pointer = EmulationInputSnapshot.Empty.Pointer },
            physical with { Controllers = [EmulationControllerState.Empty] }
        })
        {
            scenario.Publish(mapped.Keys);
            var after = scenario.Map(released, restored);
            Assert.DoesNotContain(scenario.Target, after.Keys);
            events.Clear();
            scenario.Publish(after.Keys);
            Assert.Contains((false, scenario.NativeCode), events);
        }
        scenario.Publish(EmulationInputSnapshot.Empty.Keys);
    }

    [Theory]
    [InlineData("Acorn", "LeftControl")]
    [InlineData("Acorn", "Keyboard:LeftControl")]
    [InlineData("Acorn", "leftcontrol")]
    [InlineData("Apple", "LeftControl")]
    [InlineData("Apple", "Keyboard:LeftControl")]
    [InlineData("Apple", "leftcontrol")]
    [InlineData("Coleco", "LeftControl")]
    [InlineData("Coleco", "Keyboard:LeftControl")]
    [InlineData("Coleco", "leftcontrol")]
    public void SimpleSpecialKeyBindingStillSubstitutesItsPhysicalKey(string family, string source)
    {
        using var http = new HttpClient();
        var scenario = Create(family, http, []);
        var settings = scenario.Manager.DescribeInputSettings(scenario.Configuration);
        var values = new Dictionary<string, string>(settings.Keyboard!.Values) { [scenario.TargetId] = source };
        var changed = scenario.Manager.ApplyInputSettings(scenario.Configuration, settings with { Keyboard = settings.Keyboard with { Values = values } });
        var physical = EmulationInputSnapshot.Empty with { Keys = new HashSet<EmulationKey> { EmulationKey.LeftControl, EmulationKey.Z } };
        var mapped = scenario.Map(physical, changed);
        Assert.Contains(scenario.Target, mapped.Keys);
        Assert.Contains(EmulationKey.Z, mapped.Keys);
        Assert.DoesNotContain(EmulationKey.LeftControl, mapped.Keys);
    }

    private static Scenario Create(string family, HttpClient http, List<(bool Pressed, uint Code)> events)
    {
        switch (family)
        {
            case "Acorn":
                var acorn = new AcornKeyboard();
                return new(new GWGUI.Emulation.Acorn.Modules.AcornEmulationModule("unused", "unused", http, "unused"), new AcornMachines.Contracts.MachineConfiguration(AcornMachines.Enums.MachineModel.BbcMaster128),
                    nameof(EmulationKey.AcornCopy), EmulationKey.AcornCopy, AcornKeyboard.Mappings[EmulationKey.AcornCopy],
                    (snapshot, configuration) => AcornMachines.Functions.InputSnapshotFunctions.Apply(snapshot, ((AcornMachines.Contracts.MachineConfiguration)configuration).Input),
                    keys => acorn.Publish(keys, (pressed, code, character, modifiers) => events.Add((pressed, code))));
            case "Apple":
                var apple = new AppleKeyboard();
                return new(new GWGUI.Emulation.Apple.Modules.AppleEmulationModule("unused", "unused", http, "unused"), new AppleMachines.Contracts.MachineConfiguration(AppleMachines.Enums.MachineModel.AppleIIe),
                    nameof(EmulationKey.AppleOpenApple), EmulationKey.AppleOpenApple, AppleKeyboard.Mappings[EmulationKey.AppleOpenApple],
                    (snapshot, configuration) => AppleMachines.Functions.InputSnapshotFunctions.Apply(snapshot, ((AppleMachines.Contracts.MachineConfiguration)configuration).Input, ((AppleMachines.Contracts.MachineConfiguration)configuration).Model),
                    keys => apple.Publish(keys, (pressed, code, character, modifiers) => events.Add((pressed, code))));
            case "Coleco":
                var coleco = new ColecoKeyboard();
                return new(new GWGUI.Emulation.Coleco.Modules.ColecoEmulationModule("unused", "unused", http, "unused"), new ColecoMachines.Contracts.MachineConfiguration(ColecoMachines.Enums.MachineModel.Adam),
                    "Smart1", EmulationKey.AdamSmart1, ColecoKeyboard.Mappings[EmulationKey.AdamSmart1],
                    (snapshot, configuration) => ColecoMachines.Functions.InputSnapshotFunctions.Apply(snapshot, ((ColecoMachines.Contracts.MachineConfiguration)configuration).Input, ((ColecoMachines.Contracts.MachineConfiguration)configuration).Model, ((ColecoMachines.Contracts.MachineConfiguration)configuration).Core),
                    keys => coleco.Publish(keys, (pressed, code, character, modifiers) => events.Add((pressed, code))));
            default:
                throw new ArgumentOutOfRangeException(nameof(family));
        }
    }

    private sealed record Scenario(IEmulationInputSettingsManager Manager, IEmulationConfiguration Configuration, string TargetId, EmulationKey Target, uint NativeCode,
        Func<EmulationInputSnapshot, IEmulationConfiguration, EmulationInputSnapshot> Map, Action<IReadOnlySet<EmulationKey>> Publish);
}
