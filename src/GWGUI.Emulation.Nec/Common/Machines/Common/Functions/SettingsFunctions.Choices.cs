namespace GWGUI.Emulation.Nec.Common.Machines.Common.Functions;

internal static partial class SettingsDescriptionFunctions
{
    internal static EmulationSettingsChoice Invariant(string id, string text,
        long? numericValue = null) =>
        new(id, string.Empty, text, numericValue);

    internal static IEnumerable<EmulationSettingsChoice> InvariantChoices(params string[] values) =>
        values.Select(value => Invariant(value, value));
}
