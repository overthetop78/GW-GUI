namespace GWGUI.Emulation.Contracts;

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
    string EnabledValue = EmulationSettingConstants.Enabled,
    string DisabledValue = EmulationSettingConstants.Disabled,
    EmulationDefaultFolderCategory? DefaultFolderCategory = null,
    long? NumericValue = null,
    EmulationSettingsChoiceSource ChoiceSource = EmulationSettingsChoiceSource.Declared,
    bool RefreshSettingsOnChange = false);
