namespace GWGUI.Emulation.Amstrad.Emulators.Common.Interop.Functions;

internal static partial class CoreSettingsDescriptionFunctions
{
    private static EmulationSettingsChoice Invariant(string id, string text,
        long? numericValue = null) =>
        new(id, string.Empty, text, numericValue);

    private static IEnumerable<EmulationSettingsChoice> InvariantChoices(params string[] values) =>
        values.Select(value => Invariant(value, value));
}
