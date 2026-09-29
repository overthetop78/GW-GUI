namespace GWGUI.Emulation.Sega.Common.Machines.Common.Functions;

internal static partial class SettingsDescriptionFunctions
{
    private static EmulationSettingsChoice Invariant(string id, string text,
        long? numericValue = null) =>
        new(id, string.Empty, text, numericValue);
}
