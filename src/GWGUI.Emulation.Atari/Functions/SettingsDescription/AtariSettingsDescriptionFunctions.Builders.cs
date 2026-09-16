using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Functions;

internal static partial class AtariSettingsDescriptionFunctions
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
            RequiresRestart: AtariRuntimeOptionFunctions.RequiresRestart(AtariEmulator.Atari800, id));

    private static EmulationSettingsChoice LocalizedChoice(string fieldId, string value) =>
        (fieldId, value) switch
    {
        (_, AtariEightBitSettingsConstants.None) => new(value, "Emulation.Value.None"),
        (_, AtariEightBitSettingsConstants.Disabled) => new(value, "Emulation.Value.Disabled"),
        (AtariEightBitSettingsConstants.ControllerCompatibilityOptionKey,
            AtariEightBitSettingsConstants.DualStick) =>
            new(value, "Emulation.Atari.Controller.DualStick"),
        (_, AtariEightBitSettingsConstants.Enabled) => new(value, "Emulation.Value.Enabled"),
        (_, AtariEightBitSettingsConstants.AutofireOnButton) =>
            new(value, "Emulation.Atari.Controller.AutofireButton"),
        (_, AtariEightBitSettingsConstants.AutofireAlways) =>
            new(value, "Emulation.Atari.Controller.AutofireAlways"),
        (_, AtariEightBitSettingsConstants.SwapPorts) =>
            new(value, "Emulation.Atari.Controller.SwapPorts"),
        (_, AtariEightBitSettingsConstants.Joy2BPlus) =>
            new(value, "Emulation.Atari.Controller.Joy2BPlus"),
        (_, AtariEightBitSettingsCatalogConstants.Auto) =>
            new(value, AtariSettingsDescriptionFunctionsConstants.VisualAutomatic),
        (_, AtariEightBitSettingsCatalogConstants.Default) => new(value, "Emulation.Value.Default"),
        (_, AtariEightBitSettingsCatalogConstants.Gray) => new(value, "Emulation.Value.Gray"),
        (_, AtariEightBitSettingsCatalogConstants.BlueBrown1) =>
            new(value, "Emulation.Atari.Video.Artifacting.BlueBrown1"),
        (_, AtariEightBitSettingsCatalogConstants.BlueBrown2) =>
            new(value, "Emulation.Atari.Video.Artifacting.BlueBrown2"),
        _ => Invariant(value)
    };

    private static EmulationSettingsField Select(string id, EmulationMachineTab tab, string block,
        string label, string value, IEnumerable<EmulationSettingsChoice> choices, bool isEnabled = true) =>
        new(id, tab, block, label, EmulationSettingsEditor.Selection, value, choices.ToArray(),
            IsEnabled: isEnabled, ExplanationResourceKey: ShortHelp(id),
            DetailedExplanationResourceKey: DetailedHelp(id),
            RequiresRestart: AtariRuntimeOptionFunctions.RequiresRestart(AtariEmulator.Atari800, id));

    private static EmulationSettingsField Toggle(string id, EmulationMachineTab tab, string block,
        string label, bool value, string enabledValue = AtariSettingsDescriptionFunctionsConstants.Enabled, string disabledValue = AtariSettingsDescriptionFunctionsConstants.Disabled) =>
        new(id, tab, block, label, EmulationSettingsEditor.Toggle,
            value ? enabledValue : disabledValue, ExplanationResourceKey: ShortHelp(id),
            DetailedExplanationResourceKey: DetailedHelp(id), EnabledValue: enabledValue, DisabledValue: disabledValue,
            RequiresRestart: AtariRuntimeOptionFunctions.RequiresRestart(AtariEmulator.Atari800, id));

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

    private static string Value(AtariMachineConfiguration configuration, string key, string fallback) =>
        configuration.Options.GetValueOrDefault(key) ?? fallback;

    private static bool Enabled(AtariMachineConfiguration configuration, string key) =>
        Value(configuration, key, AtariEightBitSettingsConstants.Disabled) == AtariEightBitSettingsConstants.Enabled;

    private static EmulationSettingsField ClassicMemory(AtariMachineConfiguration configuration,
        AtariClassicModelDefinition model)
    {
        if (configuration.Model != AtariMachineModel.XlXe)
            return Information(AtariConfigurationOptionConstants.MainMemory, EmulationMachineTab.Ram,
                AtariSettingsDescriptionFunctionsConstants.MainMemory, AtariSettingsDescriptionFunctionsConstants.ResourceMemoryMain, AtariHardwareSettingsFunctions.FormatBytes(model.MainMemoryBytes),
                model.MainMemoryBytes);
        var choices = new[] { 320L, 576L, 1088L }.Select(value =>
            AtariHardwareSettingsFunctions.MemoryKib((int)value)).ToArray();
        return Select(AtariConfigurationOptionConstants.MainMemory, EmulationMachineTab.Ram, AtariSettingsDescriptionFunctionsConstants.MainMemory,
            AtariSettingsDescriptionFunctionsConstants.ResourceMemoryMain, Value(configuration, AtariConfigurationOptionConstants.MainMemory,
                model.MainMemoryBytes.ToString()), choices);
    }
}
