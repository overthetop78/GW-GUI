namespace GWGUI.Emulation.Sony.Emulators.Pcsx2.Constants;

internal static class OptionRangeConstants
{
    internal static readonly EmulationSettingsNumericRange Deadzone = new(0, 50, 1, "%");
    internal static readonly EmulationSettingsNumericRange Sensitivity = new(80, 200, 1, "%");
    internal static readonly EmulationSettingsNumericRange Vibration = new(0, 100, 5, "%");
    internal static readonly EmulationSettingsNumericRange SoftwareThreads = new(0, 11, 1);
}
