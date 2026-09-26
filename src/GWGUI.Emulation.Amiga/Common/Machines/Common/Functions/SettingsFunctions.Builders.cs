using GWGUI.Emulation;

namespace GWGUI.Emulation.Amiga.Common.Machines.Common.Functions;

internal static partial class SettingsDescriptionFunctions
{
private static EmulationSettingsField Information(string id, EmulationMachineTab tab, string block,
        string label, string value) => new(id, tab, block, label, EmulationSettingsEditor.Information, value,
            ExplanationResourceKey: ShortHelp(id), DetailedExplanationResourceKey: DetailedHelp(id));

    private static EmulationSettingsField Toggle(string id, EmulationMachineTab tab, string block,
        string label, bool value, bool refreshSettingsOnChange = false) =>
        new(id, tab, block, label, EmulationSettingsEditor.Toggle,
            value ? SettingsDescriptionFunctionsConstants.Enabled : SettingsDescriptionFunctionsConstants.Disabled,
            ExplanationResourceKey: ShortHelp(id), DetailedExplanationResourceKey: DetailedHelp(id),
            RefreshSettingsOnChange: refreshSettingsOnChange);

    private static EmulationSettingsField Number(string id, EmulationMachineTab tab, string block,
        string label, string value) => new(id, tab, block, label, EmulationSettingsEditor.Number, value,
            ExplanationResourceKey: ShortHelp(id), DetailedExplanationResourceKey: DetailedHelp(id));

    private static EmulationSettingsField Path(string id, string label, string? value) =>
        new(id, EmulationMachineTab.Rom, SettingsDescriptionFunctionsConstants.Firmware, label, EmulationSettingsEditor.Path, value,
            ExplanationResourceKey: ShortHelp(id), DetailedExplanationResourceKey: DetailedHelp(id),
            DefaultFolderCategory: EmulationDefaultFolderCategory.Firmware);

    private static EmulationSettingsField AudioOutput(string? value) => new(
        SettingsConstants.AudioOutput, EmulationMachineTab.Audio, SettingsDescriptionFunctionsConstants.Audio, SettingsDescriptionFunctionsConstants.ResourceAudioDevice,
        EmulationSettingsEditor.Selection, value ?? string.Empty,
        [new EmulationSettingsChoice(string.Empty, SettingsDescriptionFunctionsConstants.ResourceAudioDefaultOutput)],
        ChoiceSource: EmulationSettingsChoiceSource.AudioOutputDevices);

    private static string? ShortHelp(string id) => FieldHelpResources.TryGetValue(id, out var resource)
        ? resource + ".Short" : null;

    private static string? DetailedHelp(string id) => FieldHelpResources.TryGetValue(id, out var resource)
        ? resource + ".Detailed" : null;
}
