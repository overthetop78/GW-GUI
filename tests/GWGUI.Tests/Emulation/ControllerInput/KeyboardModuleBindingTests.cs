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
using AtariMachines = GWGUI.Emulation.Atari.Common.Machines.Common;
using AmstradMachines = GWGUI.Emulation.Amstrad.Common.Machines.Common;
using AtariKeyboard = GWGUI.Emulation.Atari.Emulators.Common.Interop.Services.KeyboardState;
using MicrosoftMachines = GWGUI.Emulation.Microsoft.Common.Machines.Common;
using NecMachines = GWGUI.Emulation.Nec.Common.Machines.Common;
using NintendoMachines = GWGUI.Emulation.Nintendo.Common.Machines.Common;
using SegaMachines = GWGUI.Emulation.Sega.Common.Machines.Common;
using SonyMachines = GWGUI.Emulation.Sony.Common.Machines.Common;

namespace GWGUI.Tests.Emulation.ControllerInput;

public sealed class KeyboardModuleBindingTests
{
    [Theory]
    [InlineData("Acorn", EmulationKey.RightAlt)]
    [InlineData("Apple", EmulationKey.RightAlt)]
    [InlineData("Coleco", EmulationKey.RightAlt)]
    [InlineData("Atari", EmulationKey.RightAlt)]
    [InlineData("Acorn", EmulationKey.F24)]
    [InlineData("Apple", EmulationKey.F24)]
    [InlineData("Coleco", EmulationKey.F24)]
    [InlineData("Atari", EmulationKey.F24)]
    [InlineData("Acorn", EmulationKey.Oem102)]
    [InlineData("Apple", EmulationKey.Oem102)]
    [InlineData("Coleco", EmulationKey.Oem102)]
    [InlineData("Atari", EmulationKey.Oem102)]
    [InlineData("Acorn", EmulationKey.MediaPlayPause)]
    [InlineData("Apple", EmulationKey.MediaPlayPause)]
    [InlineData("Coleco", EmulationKey.MediaPlayPause)]
    [InlineData("Atari", EmulationKey.MediaPlayPause)]
    [InlineData("Acorn", EmulationKey.NumpadSeparator)]
    [InlineData("Apple", EmulationKey.NumpadSeparator)]
    [InlineData("Coleco", EmulationKey.NumpadSeparator)]
    [InlineData("Atari", EmulationKey.NumpadSeparator)]
    public void MixedSpecialKeyBindingPersistsAndReachesNativeCallback(string family, EmulationKey hostKey)
    {
        using var http = new HttpClient();
        var events = new List<(bool Pressed, uint Code)>();
        var scenario = Create(family, http, events);
        var settings = scenario.Manager.DescribeInputSettings(scenario.Configuration);
        Assert.Equal(EmulationInputSource.Keyboard | EmulationInputSource.Mouse | EmulationInputSource.Controller, settings.Keyboard!.Sources);
        var chord = $"Keyboard:{hostKey}+Mouse:Middle+Controller:A";
        var values = new Dictionary<string, string>(settings.Keyboard.Values) { [scenario.TargetId] = chord };
        var changed = scenario.Manager.ApplyInputSettings(scenario.Configuration, settings with { Keyboard = settings.Keyboard with { Values = values } });
        var restored = (IEmulationConfiguration)JsonSerializer.Deserialize(JsonSerializer.Serialize(changed, changed.GetType()), changed.GetType())!;
        Assert.Equal(chord, scenario.Manager.DescribeInputSettings(restored).Keyboard!.Values[scenario.TargetId]);
        var physical = EmulationInputSnapshot.Empty with
        {
            Keys = new HashSet<EmulationKey> { hostKey, EmulationKey.Z },
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
    [InlineData("Atari", "LeftControl")]
    [InlineData("Atari", "Keyboard:LeftControl")]
    [InlineData("Atari", "leftcontrol")]
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
            case "Atari":
                var atari = new AtariKeyboard();
                var module = new GWGUI.Emulation.Atari.Modules.AtariEmulationModule("unused", "unused", http, "unused");
                return new(module, module.CreateConfiguration("St"), nameof(EmulationKey.LeftAmiga), EmulationKey.LeftAmiga, AtariKeyboard.Mappings[EmulationKey.LeftAmiga],
                    (snapshot, configuration) => AtariMachines.Functions.InputSnapshotFunctions.Apply(snapshot, ((AtariMachines.Contracts.MachineConfiguration)configuration).Input, ((AtariMachines.Contracts.MachineConfiguration)configuration).Model),
                    keys => atari.Publish(keys, (pressed, code, character, modifiers) => events.Add((pressed, code))));
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

    [Theory]
    [InlineData("Atari")]
    [InlineData("Amstrad")]
    [InlineData("Microsoft")]
    [InlineData("Nec")]
    [InlineData("Nintendo")]
    [InlineData("Sega")]
    [InlineData("Sony")]
    public void LegacyKeyboardMappingsPreserveMixedSourcesAndHistoricalFallback(string family)
    {
        const string chord = "Keyboard:RightAlt+Mouse:Middle+Controller:A";
        var bindings = new Dictionary<string, string> { [nameof(EmulationKey.F5)] = chord };
        var historical = new Dictionary<string, EmulationKey> { [nameof(EmulationKey.F5)] = EmulationKey.LeftControl };
        var physical = EmulationInputSnapshot.Empty with
        {
            Keys = new HashSet<EmulationKey> { EmulationKey.RightAlt, EmulationKey.Z },
            Pointer = EmulationInputSnapshot.Empty.Pointer with { Middle = true },
            Controllers = [EmulationControllerState.Empty with { Buttons = 1u << 8 }]
        };
        Assert.Contains(EmulationKey.F5, Map(physical, bindings).Keys);
        Assert.Contains(EmulationKey.Z, Map(physical, bindings).Keys);
        foreach (var released in new[]
        {
            physical with { Keys = new HashSet<EmulationKey> { EmulationKey.Z } },
            physical with { Pointer = EmulationInputSnapshot.Empty.Pointer },
            physical with { Controllers = [EmulationControllerState.Empty] }
        }) Assert.DoesNotContain(EmulationKey.F5, Map(released, bindings).Keys);
        var old = physical with { Keys = new HashSet<EmulationKey> { EmulationKey.LeftControl, EmulationKey.Z } };
        Assert.Contains(EmulationKey.F5, Map(old, null).Keys);
        Assert.DoesNotContain(EmulationKey.LeftControl, Map(old, null).Keys);
        Assert.DoesNotContain(EmulationKey.F5, Map(old, new Dictionary<string, string> { [nameof(EmulationKey.F5)] = string.Empty }).Keys);

        EmulationInputSnapshot Map(EmulationInputSnapshot snapshot, IReadOnlyDictionary<string, string>? values) => family switch
        {
            "Atari" => AtariMachines.Functions.InputSnapshotFunctions.Apply(snapshot, Restore(new AtariMachines.Contracts.InputConfiguration(KeyboardMappings: historical, KeyboardBindings: values)), AtariMachines.Enums.MachineModel.St),
            "Amstrad" => AmstradMachines.Functions.InputSnapshotFunctions.Apply(snapshot, Restore(new AmstradMachines.Contracts.InputConfiguration(KeyboardMappings: historical, KeyboardBindings: values)), false),
            "Microsoft" => MicrosoftMachines.Functions.InputSnapshotFunctions.Apply(snapshot, Restore(new MicrosoftMachines.Contracts.InputConfiguration(KeyboardMappings: historical, KeyboardBindings: values)), false),
            "Nec" => NecMachines.Functions.InputSnapshotFunctions.Apply(snapshot, Restore(new NecMachines.Contracts.InputConfiguration(KeyboardMappings: historical, KeyboardBindings: values)), false),
            "Nintendo" => NintendoMachines.Functions.InputSnapshotFunctions.Apply(snapshot, "Nes", Restore(new NintendoMachines.Contracts.InputConfiguration(KeyboardMappings: historical, KeyboardBindings: values)), false),
            "Sega" => SegaMachines.Functions.InputSnapshotFunctions.Apply(snapshot, Restore(new SegaMachines.Contracts.InputConfiguration(KeyboardMappings: historical, KeyboardBindings: values)), false),
            "Sony" => SonyMachines.Functions.InputSnapshotFunctions.Apply(snapshot, Restore(new SonyMachines.Contracts.InputConfiguration(KeyboardMappings: historical, KeyboardBindings: values)), false),
            _ => throw new ArgumentOutOfRangeException(nameof(family))
        };
        static T Restore<T>(T input) => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(input))!;
    }

    [Fact]
    public void AmstradSpecialKeyEditorKeepsMixedBindingThroughApplyAndPersistence()
    {
        var configuration = new AmstradMachines.Contracts.MachineConfiguration("cpc-6128", "caprice32");
        var settings = AmstradMachines.Functions.InputSettingsFunctions.Describe(configuration);
        Assert.Equal(EmulationInputSource.Keyboard | EmulationInputSource.Mouse | EmulationInputSource.Controller, settings.Keyboard!.Sources);
        const string chord = "Keyboard:LeftControl+Mouse:Left+Controller:B";
        var values = new Dictionary<string, string>(settings.Keyboard.Values) { [nameof(EmulationKey.AmstradCopy)] = chord };
        var changed = AmstradMachines.Functions.InputSettingsFunctions.Apply(configuration, settings with { Keyboard = settings.Keyboard with { Values = values } });
        var restored = JsonSerializer.Deserialize<AmstradMachines.Contracts.MachineConfiguration>(JsonSerializer.Serialize(changed))!;
        Assert.Equal(chord, AmstradMachines.Functions.InputSettingsFunctions.Describe(restored).Keyboard!.Values[nameof(EmulationKey.AmstradCopy)]);
        var physical = EmulationInputSnapshot.Empty with
        {
            Keys = new HashSet<EmulationKey> { EmulationKey.LeftControl },
            Pointer = EmulationInputSnapshot.Empty.Pointer with { Left = true },
            Controllers = [EmulationControllerState.Empty with { Buttons = 1u << 0 }]
        };
        Assert.Contains(EmulationKey.AmstradCopy, AmstradMachines.Functions.InputSnapshotFunctions.Apply(physical, restored.Input, false).Keys);
        Assert.DoesNotContain(EmulationKey.AmstradCopy, AmstradMachines.Functions.InputSnapshotFunctions.Apply(physical with { Controllers = [] }, restored.Input, false).Keys);
    }

    private sealed record Scenario(IEmulationInputSettingsManager Manager, IEmulationConfiguration Configuration, string TargetId, EmulationKey Target, uint NativeCode,
        Func<EmulationInputSnapshot, IEmulationConfiguration, EmulationInputSnapshot> Map, Action<IReadOnlySet<EmulationKey>> Publish);
}
