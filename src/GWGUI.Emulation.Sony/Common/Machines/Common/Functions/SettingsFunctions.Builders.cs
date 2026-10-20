using FieldBuilders = GWGUI.Emulation.Functions.EmulationHardwareSettingsFunctions;

namespace GWGUI.Emulation.Sony.Common.Machines.Common.Functions;

internal static partial class SettingsDescriptionFunctions
{
    private static EmulationSettingsBlock Block(string id, EmulationMachineTab tab, string title, string icon, int columns, params EmulationSettingsField[] fields) => FieldBuilders.Block(id, tab, title, icon, columns, fields, isCompact: tab is EmulationMachineTab.Cpu or EmulationMachineTab.Video);

    private static EmulationSettingsField Information(string id, EmulationMachineTab tab, string block, string label, string value) => FieldBuilders.Information(id, tab, block, label, value, ShortHelp(id), DetailedHelp(id));

    private static EmulationSettingsField Toggle(string id, EmulationMachineTab tab, string block, string label, bool value, bool refreshSettingsOnChange = false) =>
        FieldBuilders.Toggle(id, tab, block, label, value, ShortHelp(id), DetailedHelp(id), refreshSettingsOnChange);

    private static EmulationSettingsField Select(string id, EmulationMachineTab tab, string block, string label, string value, IEnumerable<EmulationSettingsChoice> choices, bool requiresRestart = false) => FieldBuilders.Select(id, tab, block, label, value, choices.ToArray(), ShortHelp(id), DetailedHelp(id), requiresRestart: requiresRestart);

    private static EmulationSettingsField AudioOutput(string? value) => FieldBuilders.AudioOutput(SettingsConstants.AudioOutput, SettingsDescriptionFunctionsConstants.Audio, SettingsDescriptionFunctionsConstants.ResourceAudioDevice, value ?? string.Empty, [new EmulationSettingsChoice(string.Empty, SettingsDescriptionFunctionsConstants.ResourceAudioDefaultOutput)], ShortHelp(SettingsConstants.AudioOutput), DetailedHelp(SettingsConstants.AudioOutput));

    private static string? ShortHelp(string id) => SettingsHelpDictionary.Resources.TryGetValue(id, out var resource) ? resource + ".Short" : null;

    private static string? DetailedHelp(string id) => SettingsHelpDictionary.Resources.TryGetValue(id, out var resource) ? resource + ".Detailed" : null;
}
