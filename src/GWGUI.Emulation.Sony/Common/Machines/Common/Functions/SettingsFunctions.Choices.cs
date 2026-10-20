namespace GWGUI.Emulation.Sony.Common.Machines.Common.Functions;

internal static partial class SettingsDescriptionFunctions
{
    private static EmulationSettingsChoice Invariant(string id, string text, long? numericValue = null) => GWGUI.Emulation.Functions.EmulationHardwareSettingsFunctions.InvariantChoice(id, text, numericValue);
}
