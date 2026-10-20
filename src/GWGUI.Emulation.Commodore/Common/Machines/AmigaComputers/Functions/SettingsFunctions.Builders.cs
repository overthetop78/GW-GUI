using System.IO;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Functions;

internal static partial class AmigaSettingsDescriptionFunctions
{
    private static EmulationSettingsField Information(string id, EmulationMachineTab tab, string block, string label, string value) => GWGUI.Emulation.Functions.EmulationHardwareSettingsFunctions.Information(id, tab, block, label, value, ShortHelp(id), DetailedHelp(id));

    private static EmulationSettingsField Toggle(string id, EmulationMachineTab tab, string block, string label, bool value, bool refreshSettingsOnChange = false) =>
        GWGUI.Emulation.Functions.EmulationHardwareSettingsFunctions.Toggle(id, tab, block, label, value, ShortHelp(id), DetailedHelp(id), refreshSettingsOnChange);

    private static EmulationSettingsField Number(string id, EmulationMachineTab tab, string block, string label, string value) => GWGUI.Emulation.Functions.EmulationHardwareSettingsFunctions.Number(id, tab, block, label, value, ShortHelp(id), DetailedHelp(id));

    private static EmulationSettingsField Path(string id, string label, string? value) =>
        new(id, EmulationMachineTab.Rom, SettingsBlocks.Firmware, label, EmulationSettingsEditor.Path, value,
            ExplanationResourceKey: ShortHelp(id), DetailedExplanationResourceKey: DetailedHelp(id),
            DefaultFolderCategory: EmulationDefaultFolderCategory.Firmware);

    private static EmulationSettingsField AudioOutput(string? value) => GWGUI.Emulation.Functions.EmulationHardwareSettingsFunctions.AudioOutput(MachineSettingsConstants.AudioOutput, SettingsBlocks.Audio, SettingsResourceKeys.AudioDevice, value ?? string.Empty, [new EmulationSettingsChoice(string.Empty, SettingsResourceKeys.AudioDefaultOutput)], ShortHelp(MachineSettingsConstants.AudioOutput), DetailedHelp(MachineSettingsConstants.AudioOutput));

    private static string? ShortHelp(string id) => AmigaSettingsHelpDictionary.Resources.TryGetValue(id, out var resource) ? resource + SettingsValueConstants.ShortHelpSuffix : null;

    private static string? DetailedHelp(string id) => AmigaSettingsHelpDictionary.Resources.TryGetValue(id, out var resource) ? resource + SettingsValueConstants.DetailedHelpSuffix : null;
}
