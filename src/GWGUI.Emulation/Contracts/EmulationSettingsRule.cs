namespace GWGUI.Emulation.Contracts;

public sealed record EmulationSettingsRule(
    EmulationSettingsRuleCategory Category,
    string SourceFieldId,
    string TargetFieldId,
    string ComparedValue);
