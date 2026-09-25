using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Common.Functions;

internal static partial class SettingsDescriptionFunctions
{

    private static EmulationSettingsBlock Block(string id, EmulationMachineTab tab, string title,
        string icon, int columns, params EmulationSettingsField[] fields) =>
        new(id, tab, title, fields, icon, columns);

    private static EmulationSettingsField Select(string id, EmulationMachineTab tab, string block,
        string label, string value, IEnumerable<string> choices, bool isEnabled = true) =>
        new(id, tab, block, label, EmulationSettingsEditor.Selection, value,
            choices.Select(choice => LocalizedChoice(id, choice)).ToArray(),
            IsEnabled: isEnabled, ExplanationResourceKey: ShortHelp(id),
            DetailedExplanationResourceKey: DetailedHelp(id),
            RequiresRestart: RuntimeOptionFunctions.RequiresRestart(Emulator.Atari800, id));

    private static EmulationSettingsChoice LocalizedChoice(string fieldId, string value) =>
        (fieldId, value) switch
    {
        (_, EightBitSettingsConstants.None) => new(value, "Emulation.Value.None"),
        (_, EightBitSettingsConstants.Disabled) => new(value, "Emulation.Value.Disabled"),
        (EightBitSettingsConstants.ControllerCompatibilityOptionKey,
            EightBitSettingsConstants.DualStick) =>
            new(value, "Emulation.Atari.Controller.DualStick"),
        (_, EightBitSettingsConstants.Enabled) => new(value, "Emulation.Value.Enabled"),
        (_, EightBitSettingsConstants.AutofireOnButton) =>
            new(value, "Emulation.Atari.Controller.AutofireButton"),
        (_, EightBitSettingsConstants.AutofireAlways) =>
            new(value, "Emulation.Atari.Controller.AutofireAlways"),
        (_, EightBitSettingsConstants.SwapPorts) =>
            new(value, "Emulation.Atari.Controller.SwapPorts"),
        (_, EightBitSettingsConstants.Joy2BPlus) =>
            new(value, "Emulation.Atari.Controller.Joy2BPlus"),
        (_, EightBitSettingsCatalogConstants.Auto) =>
            new(value, SettingsDescriptionFunctionsConstants.VisualAutomatic),
        (_, EightBitSettingsCatalogConstants.Default) => new(value, "Emulation.Value.Default"),
        (_, EightBitSettingsCatalogConstants.Gray) => new(value, "Emulation.Value.Gray"),
        (_, EightBitSettingsCatalogConstants.BlueBrown1) =>
            new(value, "Emulation.Atari.Video.Artifacting.BlueBrown1"),
        (_, EightBitSettingsCatalogConstants.BlueBrown2) =>
            new(value, "Emulation.Atari.Video.Artifacting.BlueBrown2"),
        _ => Invariant(value)
    };

    private static EmulationSettingsField Select(string id, EmulationMachineTab tab, string block,
        string label, string value, IEnumerable<EmulationSettingsChoice> choices, bool isEnabled = true) =>
        new(id, tab, block, label, EmulationSettingsEditor.Selection, value, choices.ToArray(),
            IsEnabled: isEnabled, ExplanationResourceKey: ShortHelp(id),
            DetailedExplanationResourceKey: DetailedHelp(id),
            RequiresRestart: RuntimeOptionFunctions.RequiresRestart(Emulator.Atari800, id));

    private static EmulationSettingsField Toggle(string id, EmulationMachineTab tab, string block,
        string label, bool value, string enabledValue = SettingsDescriptionFunctionsConstants.Enabled, string disabledValue = SettingsDescriptionFunctionsConstants.Disabled) =>
        new(id, tab, block, label, EmulationSettingsEditor.Toggle,
            value ? enabledValue : disabledValue, ExplanationResourceKey: ShortHelp(id),
            DetailedExplanationResourceKey: DetailedHelp(id), EnabledValue: enabledValue, DisabledValue: disabledValue,
            RequiresRestart: RuntimeOptionFunctions.RequiresRestart(Emulator.Atari800, id));

    private static EmulationSettingsField Path(string id, EmulationMachineTab tab, string block,
        string label, string? value) => new(id, tab, block, label, EmulationSettingsEditor.Path, value,
            ExplanationResourceKey: ShortHelp(id), DetailedExplanationResourceKey: DetailedHelp(id),
            DefaultFolderCategory: EmulationDefaultFolderCategory.Firmware);

    private static EmulationSettingsField Information(string id, EmulationMachineTab tab, string block,
        string label, string value, long? numericValue = null) => new(id, tab, block, label,
            EmulationSettingsEditor.Information, value, IsEnabled: false,
            ExplanationResourceKey: ShortHelp(id), DetailedExplanationResourceKey: DetailedHelp(id),
            NumericValue: numericValue);

    private static string? ShortHelp(string id) => FieldHelpResources.TryGetValue(id, out var resource)
        ? resource + ".Short" : null;

    private static string? DetailedHelp(string id) => FieldHelpResources.TryGetValue(id, out var resource)
        ? resource + ".Detailed" : null;

    private static string Value(MachineConfiguration configuration, string key, string fallback) =>
        configuration.Options.GetValueOrDefault(key) ?? fallback;

    private static bool Enabled(MachineConfiguration configuration, string key) =>
        Value(configuration, key, EightBitSettingsConstants.Disabled) == EightBitSettingsConstants.Enabled;

    private static EmulationSettingsField ClassicMemory(MachineConfiguration configuration,
        ClassicModelDefinition model)
    {
        if (configuration.Model != MachineModel.XlXe)
            return Information(ConfigurationOptionConstants.MainMemory, EmulationMachineTab.Ram,
                SettingsDescriptionFunctionsConstants.MainMemory, SettingsDescriptionFunctionsConstants.ResourceMemoryMain, HardwareSettingsFunctions.FormatBytes(model.MainMemoryBytes),
                model.MainMemoryBytes);
        var choices = new[] { 320L, 576L, 1088L }.Select(value =>
            HardwareSettingsFunctions.MemoryKib((int)value)).ToArray();
        return Select(ConfigurationOptionConstants.MainMemory, EmulationMachineTab.Ram, SettingsDescriptionFunctionsConstants.MainMemory,
            SettingsDescriptionFunctionsConstants.ResourceMemoryMain, Value(configuration, ConfigurationOptionConstants.MainMemory,
                model.MainMemoryBytes.ToString()), choices);
    }
}
