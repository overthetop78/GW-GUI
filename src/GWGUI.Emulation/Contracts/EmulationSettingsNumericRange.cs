namespace GWGUI.Emulation.Contracts;

public sealed record EmulationSettingsNumericRange(
    double Minimum,
    double Maximum,
    double Step,
    string UnitSuffix = "");
