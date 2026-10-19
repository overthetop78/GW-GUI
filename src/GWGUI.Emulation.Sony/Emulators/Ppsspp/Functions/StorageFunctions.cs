namespace GWGUI.Emulation.Sony.Emulators.Ppsspp.Functions;

internal static class StorageFunctions
{
    internal static string? ResolveSaveDirectory(MachineConfiguration configuration, string? fallback) =>
        configuration.Options?.GetValueOrDefault(StorageSettingsFunctionsConstants.MemoryCardPathOptionPrefix + 0) is { Length: > 0 } directory ? directory : fallback;
}
