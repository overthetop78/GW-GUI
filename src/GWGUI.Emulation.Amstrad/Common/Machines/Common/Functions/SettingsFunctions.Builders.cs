namespace GWGUI.Emulation.Amstrad.Common.Machines.Common.Functions;

internal static partial class SettingsDescriptionFunctions
{
    private static EmulationSettingsBlock Block(string id, EmulationMachineTab tab,
        string title, string icon, int columns, params EmulationSettingsField[] fields) =>
        new(id, tab, title, fields, icon, columns);

    private static EmulationSettingsField Information(string id, EmulationMachineTab tab,
        string block, string label, string value) =>
        new(id, tab, block, label, EmulationSettingsEditor.Information, value);

    private static EmulationSettingsField Toggle(string id, EmulationMachineTab tab,
        string block, string label, bool value, bool refreshSettingsOnChange = false) =>
        new(id, tab, block, label, EmulationSettingsEditor.Toggle,
            value ? SettingsDescriptionFunctionsConstants.Enabled
                : SettingsDescriptionFunctionsConstants.Disabled,
            RefreshSettingsOnChange: refreshSettingsOnChange);

    private static EmulationSettingsField Number(string id, EmulationMachineTab tab,
        string block, string label, string value) =>
        new(id, tab, block, label, EmulationSettingsEditor.Number, value);

    private static EmulationSettingsField Select(string id, EmulationMachineTab tab,
        string block, string label, string value, IEnumerable<EmulationSettingsChoice> choices,
        bool requiresRestart = false) => new(id, tab, block, label,
            EmulationSettingsEditor.Selection, value, choices.ToArray(), RequiresRestart: requiresRestart);

    private static EmulationSettingsField AudioOutput(string? value) => new(
        SettingsConstants.AudioOutput, EmulationMachineTab.Audio,
        SettingsDescriptionFunctionsConstants.Audio,
        SettingsDescriptionFunctionsConstants.ResourceAudioDevice,
        EmulationSettingsEditor.Selection, value ?? string.Empty,
        [new EmulationSettingsChoice(string.Empty,
            SettingsDescriptionFunctionsConstants.ResourceAudioDefaultOutput)],
        ChoiceSource: EmulationSettingsChoiceSource.AudioOutputDevices);
}
