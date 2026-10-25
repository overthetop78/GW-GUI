using System.Net.Http;
using System.Text.Json;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Wenquxing.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Wenquxing.Common.Machines.Common.Enums;
using GWGUI.Emulation.Wenquxing.Common.Machines.Common.Functions;
using GWGUI.Emulation.Wenquxing.Emulators.Wqxemu.Interop.Functions;
using GWGUI.Emulation.Wenquxing.Emulators.Wqxemu.Interop.Services;
using GWGUI.Emulation.Wenquxing.Modules;

namespace GWGUI.Tests.Emulation.Wenquxing;

public sealed class WenquxingInputSettingsTests
{
    [Theory]
    [InlineData(EmulationKey.Left, 0x250u)]
    [InlineData(EmulationKey.Up, 0x251u)]
    [InlineData(EmulationKey.Right, 0x252u)]
    [InlineData(EmulationKey.Down, 0x253u)]
    [InlineData(EmulationKey.PageUp, 0x254u)]
    [InlineData(EmulationKey.PageDown, 0x255u)]
    public void NavigationUsesTheWqxemuBridgeCodesOnEveryModel(EmulationKey key, uint expectedCode)
    {
        foreach (var model in Enum.GetValues<MachineModel>())
        {
            Assert.Equal(expectedCode, KeyboardFunctions.CreateKeyMap(model)[key]);
            var keyboard = new KeyboardState(model);
            var events = new List<(bool Pressed, uint Code)>();
            keyboard.Publish(new HashSet<EmulationKey> { key }, (pressed, code, character, modifiers) => events.Add((pressed, code)));
            keyboard.Publish(new HashSet<EmulationKey> { key }, (pressed, code, character, modifiers) => events.Add((pressed, code)));
            keyboard.Publish(new HashSet<EmulationKey>(), (pressed, code, character, modifiers) => events.Add((pressed, code)));
            keyboard.Release((pressed, code, character, modifiers) => events.Add((pressed, code)));
            Assert.Equal(new[] { (true, expectedCode), (false, expectedCode) }, events);
        }
    }

    [Theory]
    [InlineData(MachineModel.NC1020)]
    [InlineData(MachineModel.PC1000)]
    [InlineData(MachineModel.CC800)]
    [InlineData(MachineModel.NC2000)]
    [InlineData(MachineModel.NC3000)]
    public void ModelHotkeysMatchTheNativeHardwareLayout(MachineModel model)
    {
        using var http = new HttpClient();
        var module = new WenquxingEmulationModule("unused", "unused", http, "unused");
        var settings = module.DescribeInputSettings(new MachineConfiguration(model)).Keyboard!;
        var native = KeyboardFunctions.CreateKeyMap(model);
        (EmulationKey Key, string Source, uint Code)[] expected = model == MachineModel.NC3000 ?
        [
            (EmulationKey.WenquxingGame, "F5", 286),
            (EmulationKey.WenquxingCalculator, "F6", 287),
            (EmulationKey.WenquxingClock, "F7", 288),
            (EmulationKey.WenquxingDictionary, "F9", 290),
            (EmulationKey.WenquxingLexicon, "F10", 291),
            (EmulationKey.WenquxingStudy, "F11", 292)
        ] :
        [
            (EmulationKey.WenquxingDictionary, "F5", 286),
            (EmulationKey.WenquxingCard, "F6", 287),
            (EmulationKey.WenquxingCalculator, "F7", 288),
            (EmulationKey.WenquxingMemo, "F8", 289),
            (EmulationKey.WenquxingData, "F9", 290),
            (EmulationKey.WenquxingClock, "F10", 291),
            (EmulationKey.WenquxingNetwork, "F11", 292)
        ];
        var ids = expected.Select(item => item.Key.ToString()).Concat(new[]
        {
            nameof(EmulationKey.WenquxingPower), nameof(EmulationKey.WenquxingInputMethod), nameof(EmulationKey.WenquxingHelp)
        }).ToHashSet(StringComparer.Ordinal);
        Assert.True(ids.SetEquals(settings.Definitions.Select(definition => definition.Id)));
        foreach (var (key, source, code) in expected)
        {
            Assert.Equal(source, settings.Values[key.ToString()]);
            Assert.Equal(code, native[key]);
        }
        var unsupported = model == MachineModel.NC3000
            ? new[] { EmulationKey.WenquxingCard, EmulationKey.WenquxingMemo, EmulationKey.WenquxingData, EmulationKey.WenquxingNetwork }
            : new[] { EmulationKey.WenquxingGame, EmulationKey.WenquxingLexicon, EmulationKey.WenquxingStudy };
        foreach (var key in unsupported) Assert.False(native.ContainsKey(key));
    }

    [Theory]
    [InlineData(MachineModel.NC1020)]
    [InlineData(MachineModel.PC1000)]
    [InlineData(MachineModel.CC800)]
    [InlineData(MachineModel.NC2000)]
    [InlineData(MachineModel.NC3000)]
    public void MixedDictionaryBindingPersistsAndReachesNativeKeyboard(MachineModel model)
    {
        using var http = new HttpClient();
        var module = new WenquxingEmulationModule("unused", "unused", http, "unused");
        var initial = new MachineConfiguration(model);
        var settings = module.DescribeInputSettings(initial);
        Assert.Equal(EmulationInputSource.Keyboard | EmulationInputSource.Mouse | EmulationInputSource.Controller, settings.Keyboard!.Sources);
        Assert.Null(settings.Mouse);
        Assert.Empty(settings.ControllerPorts);
        const string chord = "Keyboard:RightAlt+Mouse:Middle+Controller:A";
        var values = new Dictionary<string, string>(settings.Keyboard.Values) { [nameof(EmulationKey.WenquxingDictionary)] = chord };
        var changed = Assert.IsType<MachineConfiguration>(module.ApplyInputSettings(initial, settings with { Keyboard = settings.Keyboard with { Values = values } }));
        var restored = JsonSerializer.Deserialize<MachineConfiguration>(JsonSerializer.Serialize(changed))!;
        Assert.Equal("RightAlt+Mouse:Middle+Controller:A", restored.Input.KeyboardBindings![nameof(EmulationKey.WenquxingDictionary)]);
        var physical = EmulationInputSnapshot.Empty with
        {
            Keys = new HashSet<EmulationKey> { EmulationKey.RightAlt, EmulationKey.Z },
            Pointer = EmulationInputSnapshot.Empty.Pointer with { Middle = true },
            Controllers = [EmulationControllerState.Empty with { Buttons = 1u << 8 }]
        };
        var mapped = InputSnapshotFunctions.Apply(physical, restored.Input, model, restored.Core);
        Assert.Contains(EmulationKey.WenquxingDictionary, mapped.Keys);
        Assert.Contains(EmulationKey.Z, mapped.Keys);
        Assert.Empty(mapped.Controllers);
        Assert.Equal(EmulationInputSnapshot.Empty.Pointer, mapped.Pointer);
        var keyboard = new KeyboardState(model);
        var events = new List<(bool Pressed, uint Code)>();
        keyboard.Publish(mapped.Keys, (pressed, code, character, modifiers) => events.Add((pressed, code)));
        var native = KeyboardFunctions.CreateKeyMap(model)[EmulationKey.WenquxingDictionary];
        Assert.Contains((true, native), events);
        foreach (var released in new[]
        {
            physical with { Keys = new HashSet<EmulationKey> { EmulationKey.Z } },
            physical with { Pointer = EmulationInputSnapshot.Empty.Pointer },
            physical with { Controllers = [EmulationControllerState.Empty] }
        })
        {
            keyboard.Publish(mapped.Keys, (pressed, code, character, modifiers) => { });
            var after = InputSnapshotFunctions.Apply(released, restored.Input, model, restored.Core);
            Assert.DoesNotContain(EmulationKey.WenquxingDictionary, after.Keys);
            events.Clear();
            keyboard.Publish(after.Keys, (pressed, code, character, modifiers) => events.Add((pressed, code)));
            Assert.Contains((false, native), events);
        }
    }

    [Theory]
    [InlineData("LeftControl")]
    [InlineData("Keyboard:LeftControl")]
    public void SimpleBindingStillSubstitutesItsPhysicalKey(string source)
    {
        var input = new InputConfiguration(new Dictionary<string, string> { [nameof(EmulationKey.WenquxingDictionary)] = source });
        var physical = EmulationInputSnapshot.Empty with { Keys = new HashSet<EmulationKey> { EmulationKey.LeftControl, EmulationKey.Z } };
        var mapped = InputSnapshotFunctions.Apply(physical, input, MachineModel.NC1020, Emulator.Wqxemu);
        Assert.Contains(EmulationKey.WenquxingDictionary, mapped.Keys);
        Assert.Contains(EmulationKey.Z, mapped.Keys);
        Assert.DoesNotContain(EmulationKey.LeftControl, mapped.Keys);
    }
}
