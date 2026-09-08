using GWGUI.Emulation;

namespace GWGUI.App.Functions.Emulation.Machine;

internal static class EmulationRuntimeOptionFunctions
{
    internal static IReadOnlyDictionary<string, string> HotChanges(
        IReadOnlyDictionary<string, string> configured,
        IReadOnlyList<EmulationOption> available)
    {
        var hotKeys = available.Where(option => !option.RequiresRestart)
            .Select(option => option.Key).ToHashSet(StringComparer.Ordinal);
        return configured.Where(option => hotKeys.Contains(option.Key))
            .ToDictionary(option => option.Key, option => option.Value, StringComparer.Ordinal);
    }
}
