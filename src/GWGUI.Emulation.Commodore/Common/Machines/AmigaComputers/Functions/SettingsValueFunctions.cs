namespace GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Functions;

internal static class SettingsValueFunctions
{
    internal static T Parse<T>(IReadOnlyDictionary<T, EmulationSettingsChoice> choices, string value, T fallback) where T : notnull =>
        choices.FirstOrDefault(choice => string.Equals(choice.Value.Id, value, StringComparison.Ordinal)).Key is { } selected
            && choices.TryGetValue(selected, out var choice) && choice.Id == value ? selected : fallback;
}
