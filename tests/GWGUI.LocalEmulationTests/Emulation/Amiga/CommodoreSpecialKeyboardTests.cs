using GWGUI.Emulation;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Functions;
using GWGUI.Emulation.Commodore.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Commodore.Common.Machines.Common.Enums;
using ViceKeyboard = GWGUI.Emulation.Commodore.Emulators.VICE.Common.Dictionaries.KeyboardDictionary;
using ViceKeyboardConstants = GWGUI.Emulation.Commodore.Emulators.VICE.Common.Constants.KeyboardConstants;
using KeyboardBindings = GWGUI.Emulation.Commodore.Common.Machines.Common.Functions.KeyboardBindingFunctions;
using InputSettings = GWGUI.Emulation.Commodore.Common.Machines.Common.Functions.InputSettingsFunctions;
using NativeInput = GWGUI.Emulation.Commodore.Emulators.Common.Interop.Services.ExternalHostCallbacks;

namespace GWGUI.Tests.Emulation.Amiga;

public sealed class CommodoreSpecialKeyboardTests
{
    [Theory]
    [InlineData("Pet2001", false)]
    [InlineData("Pet3008", false)]
    [InlineData("Pet3016", false)]
    [InlineData("Pet3032", false)]
    [InlineData("Pet4016", false)]
    [InlineData("Pet4032", false)]
    [InlineData("Pet3032B", true)]
    [InlineData("Pet4032B", true)]
    [InlineData("Pet8032", true)]
    [InlineData("Pet8096", true)]
    [InlineData("Pet8296", true)]
    [InlineData("SuperPet", true)]
    public void PetSpecialProfilesUseTheirOwnMatrixAndEditableAssociations(string model, bool business)
    {
        var configuration = new MachineConfiguration(model, Emulator.ViceXPet);
        Assert.True(ViceKeyboard.Supports(configuration));
        var settings = InputSettings.Describe(configuration);
        var keyboard = settings.Keyboard!;
        Assert.Equal(business ? 7 : 6, keyboard.Definitions.Count);
        Assert.DoesNotContain(keyboard.Definitions, item => item.Id is nameof(EmulationKey.A) or nameof(EmulationKey.LeftAmiga) or nameof(EmulationKey.CommodoreRestore));
        var native = ViceKeyboard.Overrides(configuration);
        Assert.Equal(EmulationKey.CommodoreRunStop, native[business ? 277u : 27u]);
        Assert.Equal(business ? EmulationKey.CommodoreRepeat : (EmulationKey?)null, native[305u]);
        Assert.Equal(EmulationKey.CommodoreArrowUp, native[business ? 127u : 91u]);
        var values = new Dictionary<string, string>(keyboard.Values) { [nameof(EmulationKey.CommodoreRunStop)] = nameof(EmulationKey.F10), [nameof(EmulationKey.CommodoreShiftLock)] = string.Empty };
        var changed = InputSettings.Apply(configuration, settings with { Keyboard = keyboard with { Values = values } });
        var restored = InputSettings.Describe(changed).Keyboard!;
        Assert.Equal(nameof(EmulationKey.F10), restored.Values[nameof(EmulationKey.CommodoreRunStop)]);
        Assert.Equal(string.Empty, restored.Values[nameof(EmulationKey.CommodoreShiftLock)]);
        var normalized = KeyboardBindings.Normalize(changed.Input, ViceKeyboard.Defaults(configuration), ViceKeyboard.LegacyTargets(configuration));
        var mapped = EmulationSdk.InputProcessing.MapKeyboard(new HashSet<EmulationKey> { EmulationKey.F10, EmulationKey.A, EmulationKey.Numpad1 }, normalized.KeyboardMappings);
        Assert.Contains(EmulationKey.CommodoreRunStop, mapped);
        Assert.Contains(EmulationKey.A, mapped);
        Assert.Contains(EmulationKey.Numpad1, mapped);
        Assert.DoesNotContain(EmulationKey.CommodoreShiftLock, mapped);
    }

    [Fact]
    public void MechanicalShiftLockTogglesOnPressAndPreservesPhysicalShift()
    {
        var configuration = new MachineConfiguration("Pet2001", Emulator.ViceXPet);
        var toggles = ViceKeyboard.ToggleExpansions(configuration);
        var momentary = ViceKeyboard.Expansions(configuration);
        var active = new HashSet<EmulationKey>();
        var previous = new HashSet<EmulationKey>();
        IReadOnlySet<EmulationKey> Poll(params EmulationKey[] keys) => KeyboardBindings.Expand(new HashSet<EmulationKey>(keys), momentary, toggles, active, previous);
        Assert.Contains(EmulationKey.LeftShift, Poll(EmulationKey.CommodoreShiftLock));
        Assert.Contains(EmulationKey.LeftShift, Poll(EmulationKey.CommodoreShiftLock));
        Assert.Contains(EmulationKey.LeftShift, Poll(EmulationKey.A));
        Assert.Contains(EmulationKey.LeftShift, Poll(EmulationKey.CommodoreShiftLock, EmulationKey.LeftShift));
        Assert.DoesNotContain(EmulationKey.LeftShift, Poll(EmulationKey.A));
        Assert.Empty(active);
    }
    [Theory]
    [InlineData("positional", 306u)]
    [InlineData("symbolic", 9u)]
    public void OnlySpecialKeysAreDescribedAndTheNativeKeymapIsRespected(string keymap, uint commodoreCode)
    {
        var configuration = Configuration(keymap);
        var definitions = InputSettings.Describe(configuration).Keyboard!.Definitions;
        Assert.Equal(8, definitions.Count);
        Assert.All(definitions, definition => { Assert.StartsWith("Commodore", definition.Id); Assert.NotNull(definition.InvariantDisplayValue); });
        Assert.DoesNotContain(definitions, definition => definition.Id is nameof(EmulationKey.A) or nameof(EmulationKey.F1) or nameof(EmulationKey.LeftAmiga));
        Assert.Equal(EmulationKey.CommodoreKey, ViceKeyboard.Overrides(configuration)[commodoreCode]);
        Assert.Equal(EmulationKey.CommodoreArrowLeft, ViceKeyboard.Overrides(configuration)[96]);
    }

    [Theory]
    [InlineData("positional")]
    [InlineData("symbolic")]
    public void LegacyAssociationMigratesAndTheOldPhysicalKeyStopsTriggeringRestore(string keymap)
    {
        var configuration = Configuration(keymap);
        var input = new InputConfiguration(KeyboardBindings: new Dictionary<string, string> { [nameof(EmulationKey.PageUp)] = nameof(EmulationKey.F10) });
        var normalized = KeyboardBindings.Normalize(input, ViceKeyboard.Defaults(configuration), ViceKeyboard.LegacyTargets(configuration));
        Assert.Equal(nameof(EmulationKey.F10), normalized.KeyboardBindings![nameof(EmulationKey.CommodoreRestore)]);
        Assert.DoesNotContain(nameof(EmulationKey.PageUp), normalized.KeyboardBindings.Keys);
        Assert.Contains(EmulationKey.CommodoreRestore, EmulationSdk.InputProcessing.MapKeyboard(new HashSet<EmulationKey> { EmulationKey.F10 }, normalized.KeyboardMappings));
        Assert.DoesNotContain(EmulationKey.CommodoreRestore, EmulationSdk.InputProcessing.MapKeyboard(new HashSet<EmulationKey> { EmulationKey.PageUp }, normalized.KeyboardMappings));
        Assert.Contains(EmulationKey.A, EmulationSdk.InputProcessing.MapKeyboard(new HashSet<EmulationKey> { EmulationKey.A }, normalized.KeyboardMappings));
    }

    [Fact]
    public void ExplicitErasedAssociationWinsOverLegacyValue()
    {
        var configuration = Configuration("positional");
        var input = new InputConfiguration(KeyboardBindings: new Dictionary<string, string> { [nameof(EmulationKey.PageUp)] = nameof(EmulationKey.F10), [nameof(EmulationKey.CommodoreRestore)] = string.Empty });
        var normalized = KeyboardBindings.Normalize(input, ViceKeyboard.Defaults(configuration), ViceKeyboard.LegacyTargets(configuration));
        Assert.Equal(string.Empty, normalized.KeyboardBindings![nameof(EmulationKey.CommodoreRestore)]);
        Assert.DoesNotContain(nameof(EmulationKey.CommodoreRestore), normalized.KeyboardMappings!.Keys);
        Assert.DoesNotContain(EmulationKey.CommodoreRestore, EmulationSdk.InputProcessing.MapKeyboard(new HashSet<EmulationKey> { EmulationKey.PageUp, EmulationKey.F10 }, normalized.KeyboardMappings));
    }

    [Fact]
    public void CustomKeymapIsNotInterpretedAsTheStandardTable()
    {
        Assert.False(ViceKeyboard.Supports(Configuration("symbolic-user")));
        Assert.False(ViceKeyboard.Supports(Configuration("positional-user")));
    }

    [Theory]
    [InlineData("Vic20", "positional")]
    [InlineData("Vic20", "symbolic")]
    [InlineData("Vic21", "positional")]
    [InlineData("Vic21", "symbolic")]
    public void VicSpecialKeysAreEditableThroughTheCommonSettingsBridge(string model, string keymap)
    {
        var configuration = new MachineConfiguration(model, Emulator.ViceXVic, Options: new Dictionary<string, string> { [ViceKeyboardConstants.KeymapOption] = keymap });
        var settings = InputSettings.Describe(configuration);
        var keyboard = settings.Keyboard!;
        Assert.Equal(8, keyboard.Definitions.Count);
        Assert.All(keyboard.Definitions, definition => Assert.StartsWith("Commodore", definition.Id));
        var values = new Dictionary<string, string>(keyboard.Values) { [nameof(EmulationKey.CommodoreRestore)] = nameof(EmulationKey.F10), [nameof(EmulationKey.CommodoreShiftLock)] = string.Empty };
        var changed = InputSettings.Apply(configuration, settings with { Keyboard = keyboard with { Values = values } });
        Assert.Equal(nameof(EmulationKey.F10), changed.Input!.KeyboardBindings![nameof(EmulationKey.CommodoreRestore)]);
        Assert.Equal(string.Empty, changed.Input.KeyboardBindings[nameof(EmulationKey.CommodoreShiftLock)]);
        Assert.Contains(EmulationKey.CommodoreRestore, EmulationSdk.InputProcessing.MapKeyboard(new HashSet<EmulationKey> { EmulationKey.F10 }, changed.Input.KeyboardMappings));
        Assert.DoesNotContain(EmulationKey.CommodoreRestore, EmulationSdk.InputProcessing.MapKeyboard(new HashSet<EmulationKey> { EmulationKey.PageUp }, changed.Input.KeyboardMappings));
        Assert.DoesNotContain(EmulationKey.CommodoreShiftLock, EmulationSdk.InputProcessing.MapKeyboard(new HashSet<EmulationKey> { EmulationKey.CapsLock }, changed.Input.KeyboardMappings));
        Assert.Equal(EmulationKey.CommodoreRestore, ViceKeyboard.Overrides(configuration)[280]);
    }

    [Theory]
    [InlineData("C128", "positional", 286u)]
    [InlineData("C128", "symbolic", 315u)]
    [InlineData("C128D", "positional", 286u)]
    [InlineData("C128D", "symbolic", 315u)]
    public void C128SpecificKeysCanBeReassignedAndCleared(string model, string keymap, uint helpCode)
    {
        var configuration = new MachineConfiguration(model, Emulator.ViceX128, Options: new Dictionary<string, string> { [ViceKeyboardConstants.KeymapOption] = keymap });
        var settings = InputSettings.Describe(configuration);
        var keyboard = settings.Keyboard!;
        Assert.Equal(keymap == "symbolic" ? 12 : 13, keyboard.Definitions.Count);
        Assert.DoesNotContain(keyboard.Definitions, definition => definition.Id is nameof(EmulationKey.A) or nameof(EmulationKey.F5) or nameof(EmulationKey.LeftAmiga));
        Assert.All(keyboard.Definitions, definition => Assert.NotNull(definition.InvariantDisplayValue));
        var values = new Dictionary<string, string>(keyboard.Values) { [nameof(EmulationKey.Commodore40_80Display)] = nameof(EmulationKey.F10), [nameof(EmulationKey.CommodoreNoScroll)] = string.Empty };
        var changed = InputSettings.Apply(configuration, settings with { Keyboard = keyboard with { Values = values } });
        var keys = EmulationSdk.InputProcessing.MapKeyboard(new HashSet<EmulationKey> { EmulationKey.F10 }, changed.Input!.KeyboardMappings);
        Assert.Contains(EmulationKey.Commodore40_80Display, keys);
        Assert.DoesNotContain(nameof(EmulationKey.CommodoreNoScroll), changed.Input.KeyboardMappings!.Keys);
        Assert.Equal(EmulationKey.Help, ViceKeyboard.Overrides(configuration)[helpCode]);
        Assert.Equal(EmulationKey.CommodoreAsciiDinCaps, ViceKeyboard.Overrides(configuration)[keymap == "symbolic" ? 307u : 285u]);
        if (keymap == "symbolic")
        {
            Assert.Equal(EmulationKey.CommodoreArrowLeft, ViceKeyboard.Overrides(configuration)[96]);
            Assert.Equal(EmulationKey.CommodoreArrowUp, ViceKeyboard.Overrides(configuration)[281]);
            Assert.Equal(EmulationKey.CommodoreKey, ViceKeyboard.Overrides(configuration)[308]);
            Assert.Equal(EmulationKey.CommodoreLineFeed, ViceKeyboard.Overrides(configuration)[279]);
            Assert.Equal(EmulationKey.Commodore40_80Display, ViceKeyboard.Overrides(configuration)[290]);
            Assert.Equal(EmulationKey.CommodoreNoScroll, ViceKeyboard.Overrides(configuration)[291]);
            Assert.DoesNotContain(27u, ViceKeyboard.Overrides(configuration).Keys);
            Assert.Empty(ViceKeyboard.Expansions(configuration));
        }
    }

    [Fact]
    public void NativeCompoundsPreserveOrdinaryKeysAndHeldModifiersAfterSpecialKeyRelease()
    {
        var configuration = new MachineConfiguration("C64", Emulator.ViceX64, Options: new Dictionary<string, string> { [ViceKeyboardConstants.KeymapOption] = "symbolic" });
        var expansions = ViceKeyboard.Expansions(configuration);
        var ordinary = NativeInput.ExpandKeyboardKeys(new HashSet<EmulationKey> { EmulationKey.Backquote, EmulationKey.D6 }, expansions);
        Assert.DoesNotContain(EmulationKey.LeftShift, ordinary);
        var both = NativeInput.ExpandKeyboardKeys(new HashSet<EmulationKey> { EmulationKey.CommodoreArrowLeft, EmulationKey.CommodoreArrowUp }, expansions);
        Assert.True(both.SetEquals(new[] { EmulationKey.CommodoreArrowLeft, EmulationKey.LeftShift, EmulationKey.D6 }));
        var releasedOne = NativeInput.ExpandKeyboardKeys(new HashSet<EmulationKey> { EmulationKey.CommodoreArrowUp }, expansions);
        Assert.True(releasedOne.SetEquals(new[] { EmulationKey.LeftShift, EmulationKey.D6 }));
        var physicalHeld = NativeInput.ExpandKeyboardKeys(new HashSet<EmulationKey> { EmulationKey.LeftShift, EmulationKey.Backquote }, expansions);
        Assert.True(physicalHeld.SetEquals(new[] { EmulationKey.LeftShift, EmulationKey.Backquote }));
        Assert.Empty(NativeInput.ExpandKeyboardKeys(new HashSet<EmulationKey>(), expansions));
    }

    [Theory]
    [InlineData("C16", "positional", 277u)]
    [InlineData("C16", "symbolic", 92u)]
    [InlineData("Plus4", "positional", 277u)]
    [InlineData("Plus4", "symbolic", 92u)]
    [InlineData("C232", "positional", 277u)]
    [InlineData("C232", "symbolic", 92u)]
    [InlineData("V364", "positional", 277u)]
    [InlineData("V364", "symbolic", 92u)]
    public void Plus4ExposesOnlyItsOwnPhysicalSpecialKeys(string model, string keymap, uint poundCode)
    {
        var configuration = new MachineConfiguration(model, Emulator.ViceXPlus4, Options: new Dictionary<string, string> { [ViceKeyboardConstants.KeymapOption] = keymap });
        var settings = InputSettings.Describe(configuration);
        var keyboard = settings.Keyboard!;
        Assert.Equal(6, keyboard.Definitions.Count);
        Assert.DoesNotContain(keyboard.Definitions,
            definition => definition.Id is nameof(EmulationKey.CommodoreRestore) or nameof(EmulationKey.CommodoreArrowLeft) or nameof(EmulationKey.CommodoreArrowUp) or nameof(EmulationKey.CommodoreLineFeed) or nameof(EmulationKey.LeftAmiga) or nameof(EmulationKey.A));
        Assert.All(keyboard.Definitions, definition => Assert.NotNull(definition.InvariantDisplayValue));
        Assert.Equal(EmulationKey.CommodorePound, ViceKeyboard.Overrides(configuration)[poundCode]);
        Assert.Equal(EmulationKey.Help, ViceKeyboard.Overrides(configuration)[289]);
        Assert.DoesNotContain(280u, ViceKeyboard.Overrides(configuration).Keys);
        Assert.DoesNotContain(96u, ViceKeyboard.Overrides(configuration).Keys);
        var values = new Dictionary<string, string>(keyboard.Values) { [nameof(EmulationKey.Help)] = nameof(EmulationKey.F10), [nameof(EmulationKey.CommodoreShiftLock)] = string.Empty };
        var changed = InputSettings.Apply(configuration, settings with { Keyboard = keyboard with { Values = values } });
        Assert.Contains(EmulationKey.Help, EmulationSdk.InputProcessing.MapKeyboard(new HashSet<EmulationKey> { EmulationKey.F10 }, changed.Input!.KeyboardMappings));
        Assert.DoesNotContain(EmulationKey.Help, EmulationSdk.InputProcessing.MapKeyboard(new HashSet<EmulationKey> { EmulationKey.F8 }, changed.Input.KeyboardMappings));
        Assert.DoesNotContain(nameof(EmulationKey.CommodoreShiftLock), changed.Input.KeyboardMappings!.Keys);
        Assert.Empty(ViceKeyboard.Expansions(configuration));
    }

    [Theory]
    [InlineData("CbmII510", Emulator.ViceXCbm5x0)]
    [InlineData("CbmII610", Emulator.ViceXCbm2)]
    [InlineData("CbmII620", Emulator.ViceXCbm2)]
    [InlineData("CbmII620Plus", Emulator.ViceXCbm2)]
    [InlineData("CbmII710", Emulator.ViceXCbm2)]
    [InlineData("CbmII720", Emulator.ViceXCbm2)]
    [InlineData("CbmII720Plus", Emulator.ViceXCbm2)]
    public void CbmSpecialKeysUseTheirOwnFixedMatrixAndEditableBindings(string model, Emulator emulator)
    {
        var configuration = new MachineConfiguration(model, emulator);
        var settings = InputSettings.Describe(configuration);
        var keyboard = settings.Keyboard!;
        Assert.Equal(8, keyboard.Definitions.Count);
        Assert.DoesNotContain(keyboard.Definitions, key => key.Id is nameof(EmulationKey.A) or nameof(EmulationKey.CommodoreRestore) or nameof(EmulationKey.CommodoreArrowLeft) or nameof(EmulationKey.LeftAmiga));
        var expected = new Dictionary<uint, EmulationKey> { [305] = EmulationKey.CommodoreKey,
            [308] = EmulationKey.CommodoreRunStop,
            [301] = EmulationKey.CommodoreShiftLock,
            [278] = EmulationKey.CommodoreClearHome,
            [277] = EmulationKey.CommodorePound,
            [280] = EmulationKey.CommodoreOffReverse,
            [281] = EmulationKey.CommodoreNormalGraphic,
            [279] = EmulationKey.CommodoreKeypadClearEntry };
        foreach (var item in expected) Assert.Equal(item.Value, ViceKeyboard.Overrides(configuration)[item.Key]);
        Assert.Equal("OFF/RVS", keyboard.Definitions.Single(key => key.Id == nameof(EmulationKey.CommodoreOffReverse)).InvariantDisplayValue);
        Assert.Equal("NORM/GRAPH", keyboard.Definitions.Single(key => key.Id == nameof(EmulationKey.CommodoreNormalGraphic)).InvariantDisplayValue);
        var values = new Dictionary<string, string>(keyboard.Values) { [nameof(EmulationKey.CommodoreOffReverse)] = nameof(EmulationKey.F10), [nameof(EmulationKey.CommodoreNormalGraphic)] = string.Empty };
        var changed = InputSettings.Apply(configuration, settings with { Keyboard = keyboard with { Values = values } });
        Assert.Contains(EmulationKey.CommodoreOffReverse, EmulationSdk.InputProcessing.MapKeyboard(new HashSet<EmulationKey> { EmulationKey.F10 }, changed.Input!.KeyboardMappings));
        Assert.DoesNotContain(EmulationKey.CommodoreOffReverse, EmulationSdk.InputProcessing.MapKeyboard(new HashSet<EmulationKey> { EmulationKey.PageUp }, changed.Input.KeyboardMappings));
        Assert.DoesNotContain(nameof(EmulationKey.CommodoreNormalGraphic), changed.Input.KeyboardMappings!.Keys);
        Assert.Empty(ViceKeyboard.Expansions(configuration));
        Assert.Equal(ViceKeyboard.Defaults(configuration), ViceKeyboard.Defaults(configuration with { Options = new Dictionary<string, string> { [ViceKeyboardConstants.KeymapOption] = "symbolic-user" } }));
    }

    [Fact]
    public void MissingKeymapUsesTheNativePositionalDefault()
    {
        var missing = new MachineConfiguration("C64", Emulator.ViceX64);
        Assert.Equal(ViceKeyboard.Defaults(Configuration("positional")), ViceKeyboard.Defaults(missing));
        Assert.Equal(EmulationKey.CommodorePound, ViceKeyboard.Overrides(missing)[277]);
        Assert.Equal(EmulationKey.CommodoreArrowUp, ViceKeyboard.Overrides(missing)[127]);
    }

    [Fact]
    public void UnqualifiedKeyboardDoesNotExposeTheGlobalSdkAndPreservesInputBindings()
    {
        var configuration = Configuration("positional-user") with { Input = new InputConfiguration(KeyboardBindings: new Dictionary<string, string> { [nameof(EmulationKey.A)] = nameof(EmulationKey.F10) }) };
        var settings = InputSettings.Describe(configuration);
        Assert.NotNull(settings.Keyboard);
        Assert.Empty(settings.Keyboard.Definitions);
        Assert.Equal(nameof(EmulationKey.F10), settings.Keyboard.Values[nameof(EmulationKey.A)]);
        var changed = InputSettings.Apply(configuration, settings);
        Assert.Equal(nameof(EmulationKey.F10), changed.Input!.KeyboardBindings![nameof(EmulationKey.A)]);
        Assert.Contains(EmulationKey.A, EmulationSdk.InputProcessing.MapKeyboard(new HashSet<EmulationKey> { EmulationKey.F10 }, changed.Input.KeyboardMappings));
        Assert.Contains(EmulationKey.B, EmulationSdk.InputProcessing.MapKeyboard(new HashSet<EmulationKey> { EmulationKey.B }, changed.Input.KeyboardMappings));
    }

    private static MachineConfiguration Configuration(string keymap) => new("C64", Emulator.ViceX64, Options: new Dictionary<string, string> { [ViceKeyboardConstants.KeymapOption] = keymap });
}
