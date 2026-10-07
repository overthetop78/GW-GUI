using System.IO;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Functions;

internal static partial class AmigaSettingsDescriptionFunctions
{
private static EmulationSettingsField Information(string id, EmulationMachineTab tab, string block,
        string label, string value) => new(id, tab, block, label, EmulationSettingsEditor.Information, value,
            ExplanationResourceKey: ShortHelp(id), DetailedExplanationResourceKey: DetailedHelp(id));

    private static EmulationSettingsField Toggle(string id, EmulationMachineTab tab, string block,
        string label, bool value, bool refreshSettingsOnChange = false) =>
        new(id, tab, block, label, EmulationSettingsEditor.Toggle,
            value ? SettingsValueConstants.Enabled : SettingsValueConstants.Disabled,
            ExplanationResourceKey: ShortHelp(id), DetailedExplanationResourceKey: DetailedHelp(id),
            RefreshSettingsOnChange: refreshSettingsOnChange);

    private static EmulationSettingsField Number(string id, EmulationMachineTab tab, string block,
        string label, string value) => new(id, tab, block, label, EmulationSettingsEditor.Number, value,
            ExplanationResourceKey: ShortHelp(id), DetailedExplanationResourceKey: DetailedHelp(id));

    private static EmulationSettingsField Path(string id, string label, string? value) =>
        new(id, EmulationMachineTab.Rom, SettingsBlocks.Firmware, label, EmulationSettingsEditor.Path, value,
            ExplanationResourceKey: ShortHelp(id), DetailedExplanationResourceKey: DetailedHelp(id),
            DefaultFolderCategory: EmulationDefaultFolderCategory.Firmware);

    private static EmulationSettingsField AudioOutput(string? value) => new(
        MachineSettingsConstants.AudioOutput, EmulationMachineTab.Audio, SettingsBlocks.Audio, SettingsResourceKeys.AudioDevice,
        EmulationSettingsEditor.Selection, value ?? string.Empty,
        [new EmulationSettingsChoice(string.Empty, SettingsResourceKeys.AudioDefaultOutput)],
        ChoiceSource: EmulationSettingsChoiceSource.AudioOutputDevices,
        ExplanationResourceKey: ShortHelp(MachineSettingsConstants.AudioOutput),
        DetailedExplanationResourceKey: DetailedHelp(MachineSettingsConstants.AudioOutput));

    private static string? ShortHelp(string id) => AmigaSettingsHelpDictionary.Resources.TryGetValue(id, out var resource)
        ? resource + SettingsValueConstants.ShortHelpSuffix : null;

    private static string? DetailedHelp(string id) => AmigaSettingsHelpDictionary.Resources.TryGetValue(id, out var resource)
        ? resource + SettingsValueConstants.DetailedHelpSuffix : null;
}
