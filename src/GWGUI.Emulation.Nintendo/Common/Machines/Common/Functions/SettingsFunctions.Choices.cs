namespace GWGUI.Emulation.Nintendo.Common.Machines.Common.Functions;

internal static partial class SettingsDescriptionFunctions
{
    private static EmulationSettingsChoice Invariant(string id, string text, long? numericValue = null) => GWGUI.Emulation.Functions.EmulationHardwareSettingsFunctions.InvariantChoice(id, text, numericValue);

    private static IEnumerable<EmulationSettingsChoice> InvariantChoices(params string[] values) => values.Select(value => Invariant(value, value));
}
