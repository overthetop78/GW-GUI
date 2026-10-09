namespace GWGUI.Emulation.Sony.Common.Machines.Common.Functions;

internal static partial class InputSettingsFunctions
{
    private static IReadOnlyList<InputBindingDefinition> KeyboardDefinitions() =>
        Enum.GetValues<EmulationKey>()
            .Where(key => key is not (EmulationKey.Unknown or EmulationKey.LeftAmiga
                or EmulationKey.RightAmiga or EmulationKey.Undo or EmulationKey.Break
                or EmulationKey.AtariOption or EmulationKey.AtariSelect or EmulationKey.AtariStart
                or EmulationKey.AcornF0 or EmulationKey.AcornCopy or EmulationKey.AcornShiftLock
                or EmulationKey.AcornKeypadHash or EmulationKey.AcornKeypadDelete or EmulationKey.AcornKeypadComma))
            .Select(key => new InputBindingDefinition(key.ToString(),
                key == EmulationKey.Help ? InputSettingsFunctionsConstants.ResourceKeyHelp : key.ToString(),
                key.ToString(), key == EmulationKey.Help ? null : key.ToString()))
            .ToArray();
}
