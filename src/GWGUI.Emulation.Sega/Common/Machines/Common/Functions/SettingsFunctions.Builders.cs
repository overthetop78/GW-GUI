using CommonCoreHostFunctions = GWGUI.Emulation.Sega.Common.Functions.CoreHostFunctions;

namespace GWGUI.Emulation.Sega.Common.Machines.Common.Functions;

internal static partial class SettingsDescriptionFunctions
{
    private static EmulationSettingsBlock Block(string id, EmulationMachineTab tab, string title, string icon, int columns, params EmulationSettingsField[] fields) => CommonCoreHostFunctions.HardwareSettings.Block(id, tab, title, icon, columns, fields);

    private static EmulationSettingsField Information(string id, EmulationMachineTab tab, string block, string label, string value) => CommonCoreHostFunctions.HardwareSettings.Information(id, tab, block, label, value, ShortHelp(id), DetailedHelp(id));

    private static EmulationSettingsField Toggle(string id, EmulationMachineTab tab, string block, string label, bool value, bool refreshSettingsOnChange = false) =>
        CommonCoreHostFunctions.HardwareSettings.Toggle(id, tab, block, label, value, ShortHelp(id), DetailedHelp(id), refreshSettingsOnChange);

    private static EmulationSettingsField Select(string id, EmulationMachineTab tab, string block, string label, string value, IEnumerable<EmulationSettingsChoice> choices, bool requiresRestart = false) => CommonCoreHostFunctions.HardwareSettings.Select(id, tab, block, label, value, choices.ToArray(), ShortHelp(id), DetailedHelp(id), requiresRestart: requiresRestart);

    private static EmulationSettingsField AudioOutput(string? value) => CommonCoreHostFunctions.HardwareSettings.AudioOutput(SettingsConstants.AudioOutput, SettingsDescriptionFunctionsConstants.Audio, SettingsDescriptionFunctionsConstants.ResourceAudioDevice, value ?? string.Empty, [new EmulationSettingsChoice(string.Empty, SettingsDescriptionFunctionsConstants.ResourceAudioDefaultOutput)], ShortHelp(SettingsConstants.AudioOutput), DetailedHelp(SettingsConstants.AudioOutput));

    private static string? ShortHelp(string id) => SettingsHelpDictionary.Resources.TryGetValue(id, out var resource) ? resource + ".Short" : null;

    private static string? DetailedHelp(string id) => SettingsHelpDictionary.Resources.TryGetValue(id, out var resource) ? resource + ".Detailed" : null;
}
