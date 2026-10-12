using C64Models = GWGUI.Emulation.Commodore.Common.Machines.C64.Constants.ModelConstants;
using VicModels = GWGUI.Emulation.Commodore.Common.Machines.Vic20.Constants.ModelConstants;
using C128Models = GWGUI.Emulation.Commodore.Common.Machines.C128.Constants.ModelConstants;
using Plus4Models = GWGUI.Emulation.Commodore.Common.Machines.Plus4.Constants.ModelConstants;
using PetModels = GWGUI.Emulation.Commodore.Common.Machines.Pet.Constants.ModelConstants;
using CbmModels = GWGUI.Emulation.Commodore.Common.Machines.CbmII.Constants.ModelConstants;
using CommodoreKeyboard = GWGUI.Emulation.Commodore.Common.Machines.Common.Constants.KeyboardConstants;
using NativeKeyboard = GWGUI.Emulation.Commodore.Emulators.VICE.Common.Constants.KeyboardConstants;

namespace GWGUI.Emulation.Commodore.Emulators.VICE.Common.Dictionaries;
internal static class KeyboardDictionary
{
    internal static bool Supports(MachineConfiguration configuration) => IsPet(configuration) || IsCbm(configuration) || (configuration.Model is C64Models.C64 or C64Models.C64Dtv or C64Models.C64SuperCpu or VicModels.Vic20 or VicModels.Vic21 or C128Models.C128 or C128Models.C128D || IsPlus4(configuration)) && configuration.Options?.GetValueOrDefault(NativeKeyboard.KeymapOption)is null or NativeKeyboard.PositionalKeymap or NativeKeyboard.SymbolicKeymap;
    private static bool IsPetGraphics(MachineConfiguration configuration) => configuration.Model is PetModels.Pet2001 or PetModels.Pet3008 or PetModels.Pet3016 or PetModels.Pet3032 or PetModels.Pet4016 or PetModels.Pet4032;
    private static bool IsPetBusiness(MachineConfiguration configuration) => configuration.Model is PetModels.Pet3032B or PetModels.Pet4032B or PetModels.Pet8032 or PetModels.Pet8096 or PetModels.Pet8296 or PetModels.SuperPet;
    private static bool IsPet(MachineConfiguration configuration) => IsPetGraphics(configuration) || IsPetBusiness(configuration);
    private static bool IsCbm(MachineConfiguration configuration) => configuration.Model is CbmModels.CbmII510 or CbmModels.CbmII610 or CbmModels.CbmII620 or CbmModels.CbmII620Plus or CbmModels.CbmII710 or CbmModels.CbmII720 or CbmModels.CbmII720Plus;
    private static bool IsSymbolic(MachineConfiguration configuration) => configuration.Options?.GetValueOrDefault(NativeKeyboard.KeymapOption) == NativeKeyboard.SymbolicKeymap;
    private static bool IsC128(MachineConfiguration configuration) => configuration.Model is C128Models.C128 or C128Models.C128D;
    private static bool IsPlus4(MachineConfiguration configuration) => configuration.Model is Plus4Models.C16 or Plus4Models.Plus4 or Plus4Models.C232 or Plus4Models.V364;
    private static IReadOnlyDictionary<EmulationKey, NativeKeyboardKey> NativeKeys(MachineConfiguration configuration)
    {
        if (IsPetGraphics(configuration))
            return new Dictionary<EmulationKey, NativeKeyboardKey>
            {
                [EmulationKey.CommodoreRunStop] = NativeKeyboardKey.Escape,
                [EmulationKey.CommodoreClearHome] = NativeKeyboardKey.Delete,
                [EmulationKey.CommodoreOffReverse] = NativeKeyboardKey.LeftControl,
                [EmulationKey.CommodoreArrowLeft] = NativeKeyboardKey.Minus,
                [EmulationKey.CommodoreArrowUp] = NativeKeyboardKey.LeftBracket
            };
        if (IsPetBusiness(configuration))
            return new Dictionary<EmulationKey, NativeKeyboardKey>
            {
                [EmulationKey.CommodoreRunStop] = NativeKeyboardKey.Insert,
                [EmulationKey.CommodoreShiftLock] = NativeKeyboardKey.CapsLock,
                [EmulationKey.CommodoreClearHome] = NativeKeyboardKey.Home,
                [EmulationKey.CommodoreOffReverse] = NativeKeyboardKey.LeftControl,
                [EmulationKey.CommodoreArrowLeft] = NativeKeyboardKey.Backquote,
                [EmulationKey.CommodoreArrowUp] = NativeKeyboardKey.Delete,
                [EmulationKey.CommodoreRepeat] = NativeKeyboardKey.RightControl
            };
        if (IsCbm(configuration))
            return new Dictionary<EmulationKey, NativeKeyboardKey>
            {
                [EmulationKey.CommodoreKey] = NativeKeyboardKey.RightControl,
                [EmulationKey.CommodoreRunStop] = NativeKeyboardKey.LeftAlt,
                [EmulationKey.CommodoreShiftLock] = NativeKeyboardKey.CapsLock,
                [EmulationKey.CommodoreClearHome] = NativeKeyboardKey.Home,
                [EmulationKey.CommodorePound] = NativeKeyboardKey.Insert,
                [EmulationKey.CommodoreOffReverse] = NativeKeyboardKey.PageUp,
                [EmulationKey.CommodoreNormalGraphic] = NativeKeyboardKey.PageDown,
                [EmulationKey.CommodoreKeypadClearEntry] = NativeKeyboardKey.End
            };
        if (IsPlus4(configuration))
            return new Dictionary<EmulationKey, NativeKeyboardKey>
            {
                [EmulationKey.CommodoreKey] = IsSymbolic(configuration) ? NativeKeyboardKey.Tab : NativeKeyboardKey.LeftControl,
                [EmulationKey.CommodoreRunStop] = NativeKeyboardKey.Escape,
                [EmulationKey.CommodoreShiftLock] = NativeKeyboardKey.CapsLock,
                [EmulationKey.CommodoreClearHome] = NativeKeyboardKey.Home,
                [EmulationKey.CommodorePound] = IsSymbolic(configuration) ? NativeKeyboardKey.Backslash : NativeKeyboardKey.Insert,
                [EmulationKey.Help] = NativeKeyboardKey.F8
            };
        var keys = new Dictionary<EmulationKey, NativeKeyboardKey>
        {
            [EmulationKey.CommodoreKey] = IsSymbolic(configuration) ? NativeKeyboardKey.Tab : NativeKeyboardKey.LeftControl,
            [EmulationKey.CommodoreRunStop] = NativeKeyboardKey.Escape,
            [EmulationKey.CommodoreShiftLock] = NativeKeyboardKey.CapsLock,
            [EmulationKey.CommodoreClearHome] = NativeKeyboardKey.Home,
            [EmulationKey.CommodoreRestore] = NativeKeyboardKey.PageUp,
            [EmulationKey.CommodorePound] = IsSymbolic(configuration) ? NativeKeyboardKey.Backslash : NativeKeyboardKey.Insert,
            [EmulationKey.CommodoreArrowLeft] = NativeKeyboardKey.Backquote,
            [EmulationKey.CommodoreArrowUp] = NativeKeyboardKey.Delete
        };
        if (IsSymbolic(configuration) && configuration.Model is C64Models.C64 or C64Models.C64Dtv or C64Models.C64SuperCpu)
            keys.Remove(EmulationKey.CommodoreArrowUp);
        if (IsC128(configuration))
        {
            keys[EmulationKey.Help] = IsSymbolic(configuration) ? NativeKeyboardKey.Help : NativeKeyboardKey.F5;
            keys[EmulationKey.CommodoreLineFeed] = IsSymbolic(configuration) ? NativeKeyboardKey.End : NativeKeyboardKey.F6;
            keys[EmulationKey.Commodore40_80Display] = IsSymbolic(configuration) ? NativeKeyboardKey.F9 : NativeKeyboardKey.F7;
            keys[EmulationKey.CommodoreNoScroll] = IsSymbolic(configuration) ? NativeKeyboardKey.F10 : NativeKeyboardKey.F8;
            keys[EmulationKey.CommodoreAsciiDinCaps] = IsSymbolic(configuration) ? NativeKeyboardKey.RightAlt : NativeKeyboardKey.F4;
            if (IsSymbolic(configuration))
            {
                keys[EmulationKey.CommodoreKey] = NativeKeyboardKey.LeftAlt;
                keys[EmulationKey.CommodoreArrowUp] = NativeKeyboardKey.PageDown;
                keys.Remove(EmulationKey.CommodoreRunStop);
            }
        }

        return keys;
    }

    internal static IReadOnlyDictionary<EmulationKey, EmulationKey> Defaults(MachineConfiguration configuration)
    {
        var defaults = NativeKeys(configuration).ToDictionary(item => item.Key, item => item.Value == NativeKeyboardKey.Pause ? EmulationKey.Break : Enum.Parse<EmulationKey>(item.Value.ToString()));
        if (IsPetGraphics(configuration))
            defaults[EmulationKey.CommodoreShiftLock] = EmulationKey.CapsLock;
        return defaults;
    }

    internal static IReadOnlyDictionary<EmulationKey, EmulationKey> LegacyTargets(MachineConfiguration configuration)
    {
        var targets = Defaults(configuration).ToDictionary(item => item.Value, item => item.Key);
        if (IsPetGraphics(configuration))
            targets[EmulationKey.RightControl] = EmulationKey.CommodoreRunStop;
        return targets;
    }

    internal static IReadOnlyList<InputBindingDefinition> Definitions(MachineConfiguration configuration)
    {
        var defaults = Defaults(configuration);
        return NativeKeys(configuration).Keys.Concat(Expansions(configuration).Keys).Concat(ToggleExpansions(configuration).Keys).Select(key => InputSettingsFunctions.Definition(
                key.ToString(),
                IsPetBusiness(configuration) && key == EmulationKey.CommodoreRunStop ? CommodoreKeyboard.PetBusinessStop : CommodoreKeyboard.SpecialKeys[key],
                defaults.TryGetValue(key, out var physical) ? physical.ToString() : string.Empty
            )).ToArray();
    }

    internal static IReadOnlyDictionary<uint, EmulationKey?> Overrides(MachineConfiguration configuration)
    {
        var map = NativeKeys(configuration).ToDictionary(item => (uint)item.Value, item => (EmulationKey? )item.Key);
        if (IsPetGraphics(configuration))
        {
            map[(uint)NativeKeyboardKey.RightControl] = null;
            map[(uint)NativeKeyboardKey.CapsLock] = null;
        }

        return map;
    }

    internal static IReadOnlyDictionary<EmulationKey, IReadOnlyList<EmulationKey>> Expansions(MachineConfiguration configuration) => IsSymbolic(configuration) && configuration.Model is C64Models.C64 or C64Models.C64Dtv or C64Models.C64SuperCpu 
        ? new Dictionary<EmulationKey, IReadOnlyList<EmulationKey>>
    {
        [EmulationKey.CommodoreArrowUp] = new[]
        {
            EmulationKey.LeftShift,
            EmulationKey.D6
        }
    }


        : new Dictionary<EmulationKey, IReadOnlyList<EmulationKey>>();
    internal static IReadOnlyDictionary<EmulationKey, IReadOnlyList<EmulationKey>> ToggleExpansions(MachineConfiguration configuration) => IsPetGraphics(configuration) 
        ? new Dictionary<EmulationKey, IReadOnlyList<EmulationKey>>
    {
        [EmulationKey.CommodoreShiftLock] = new[]
        {
            EmulationKey.LeftShift
        }
    }


        : new Dictionary<EmulationKey, IReadOnlyList<EmulationKey>>();
}