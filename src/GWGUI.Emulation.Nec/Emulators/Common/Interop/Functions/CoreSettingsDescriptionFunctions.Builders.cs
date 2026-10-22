using CommonCoreHostFunctions = GWGUI.Emulation.Nec.Common.Functions.CoreHostFunctions;

namespace GWGUI.Emulation.Nec.Emulators.Common.Interop.Functions;

internal static partial class CoreSettingsDescriptionFunctions
{
    internal static EmulationSettingsBlock Block(string id, EmulationMachineTab tab, string title, string icon, int columns, params EmulationSettingsField[] fields) => CommonCoreHostFunctions.HardwareSettings.Block(id, tab, title, icon, columns, fields);

    internal static EmulationSettingsField Information(string id, EmulationMachineTab tab, string block, string label, string value) => CommonCoreHostFunctions.HardwareSettings.Information(id, tab, block, label, value, ShortHelp(id), DetailedHelp(id));

    internal static EmulationSettingsField Toggle(string id, EmulationMachineTab tab, string block, string label, bool value, bool refreshSettingsOnChange = false) =>
        CommonCoreHostFunctions.HardwareSettings.Toggle(id, tab, block, label, value, ShortHelp(id), DetailedHelp(id), refreshSettingsOnChange);

    internal static EmulationSettingsField Select(string id,
        EmulationMachineTab tab,
        string block,
        string label,
        string value,
        IEnumerable<EmulationSettingsChoice> choices,
        bool isEnabled = true) => CommonCoreHostFunctions.HardwareSettings.Select(id, tab, block, label, value, choices.ToArray(), ShortHelp(id), DetailedHelp(id), isEnabled: isEnabled);

    private static EmulationSettingsField FirmwarePath(string? value) => new(
        SettingsConstants.FirmwarePath, EmulationMachineTab.Rom,
        SettingsDescriptionFunctionsConstants.Firmware,
        SettingsDescriptionFunctionsConstants.ResourceFirmwareSystemRom,
        EmulationSettingsEditor.Path, value,
        ExplanationResourceKey: ShortHelp(SettingsConstants.FirmwarePath),
        DetailedExplanationResourceKey: DetailedHelp(SettingsConstants.FirmwarePath),
        DefaultFolderCategory: EmulationDefaultFolderCategory.Firmware);

    private static EmulationSettingsField AudioOutput(string? value) => CommonCoreHostFunctions.HardwareSettings.AudioOutput(SettingsConstants.AudioOutput, SettingsDescriptionFunctionsConstants.Audio, SettingsDescriptionFunctionsConstants.ResourceAudioDevice, value ?? string.Empty, [new EmulationSettingsChoice(string.Empty, SettingsDescriptionFunctionsConstants.ResourceAudioDefaultOutput)], ShortHelp(SettingsConstants.AudioOutput), DetailedHelp(SettingsConstants.AudioOutput));

    private static string? ShortHelp(string id) => SettingsHelpDictionary.Resources.TryGetValue(id, out var resource) ? resource + ".Short" : null;

    private static string? DetailedHelp(string id) => SettingsHelpDictionary.Resources.TryGetValue(id, out var resource) ? resource + ".Detailed" : null;
}
