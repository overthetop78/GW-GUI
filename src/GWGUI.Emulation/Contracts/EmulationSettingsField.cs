namespace GWGUI.Emulation;

public sealed record EmulationSettingsField(
    string Id,
    EmulationMachineTab Tab,
    string BlockId,
    string LabelResourceKey,
    EmulationSettingsEditor Editor,
    string? Value,
    IReadOnlyList<EmulationSettingsChoice>? Choices = null,
    bool IsEnabled = true,
    bool IsVisible = true,
    string? ExplanationResourceKey = null,
    string EnabledValue = "enabled",
    string DisabledValue = "disabled",
    EmulationDefaultFolderCategory? DefaultFolderCategory = null,
    long? NumericValue = null,
    EmulationSettingsChoiceSource ChoiceSource = EmulationSettingsChoiceSource.Declared);
