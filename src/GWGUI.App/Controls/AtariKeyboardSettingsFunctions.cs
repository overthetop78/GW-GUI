using GWGUI.App.Input;
using GWGUI.App.Localization;
using GWGUI.Emulation;
using GWGUI.Emulation.Atari;

namespace GWGUI.App.Controls;

internal static class AtariKeyboardSettingsFunctions
{
    internal static IReadOnlyList<InputBindingDefinition> Definitions(AtariMachineModel model)
    {
        var core = AtariCompatibilityCatalog.Get(model).Core;
        var keys = core == AtariCoreKind.Atari800 ? AtariKeyboardSettingsConstants.Atari800SpecialKeys : AtariKeyboardSettingsConstants.ComputerSpecialKeys;
        if (model == AtariMachineModel.Atari400) keys = keys.Where(key => key != EmulationKey.Help).ToArray();
        var machineKeys = core == AtariCoreKind.Atari800 ? keys : AtariKeyboardSettingsConstants.FunctionKeys.Concat(keys);
        return machineKeys.Distinct().Select(key => new InputBindingDefinition(key.ToString(), Label(key),
            AtariKeyboardSettingsConstants.Defaults.TryGetValue(key, out var hostKey) ? hostKey.ToString() : key.ToString())).ToArray();
    }

    internal static EmulationKey Parse(string binding)
    {
        if (Enum.TryParse<EmulationKey>(binding, true, out var direct)) return direct;
        if (!KeyboardChord.TryParse(binding, out var chord) || chord.Keys.Count != AtariInputSettingsConstants.InclusiveEndpointCount)
            return EmulationKey.Unknown;
        return EmulationKeyMapper.TryMap(chord.Keys[AtariInputSettingsConstants.FirstPort], out var mapped) ? mapped : EmulationKey.Unknown;
    }

    private static string Label(EmulationKey key) => key switch
    {
        EmulationKey.AtariOption => "Option",
        EmulationKey.AtariSelect => "Select",
        EmulationKey.AtariStart => "Start",
        EmulationKey.Help => LocExtension.GetInvariant("Emulation.Key.AtariHelp"),
        EmulationKey.AtariUndo => LocExtension.GetInvariant("Emulation.Key.AtariUndo"),
        EmulationKey.AtariBreak => LocExtension.GetInvariant("Emulation.Key.AtariBreak"), _ => key.ToString()
    };
}
