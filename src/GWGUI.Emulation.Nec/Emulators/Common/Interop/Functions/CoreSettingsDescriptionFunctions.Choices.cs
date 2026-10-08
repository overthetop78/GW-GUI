namespace GWGUI.Emulation.Nec.Emulators.Common.Interop.Functions;

internal static partial class CoreSettingsDescriptionFunctions
{
    internal static EmulationSettingsChoice Invariant(string id, string text,
        long? numericValue = null) =>
        new(id, string.Empty, text, numericValue);

    internal static IEnumerable<EmulationSettingsChoice> InvariantChoices(params string[] values) =>
        values.Select(value => Invariant(value, value));
}
