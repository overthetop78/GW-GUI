namespace GWGUI.Emulation;

public sealed record EmulationSettingsRule(
    EmulationSettingsRuleCategory Category,
    string SourceFieldId,
    string TargetFieldId,
    string ComparedValue);
