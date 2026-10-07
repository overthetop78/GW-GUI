namespace GWGUI.Emulation.Amstrad.Emulators.CrocoDS.Constants;

internal static class EmulatorConstants
{
    internal const string Id = "crocods";
    internal const string LibraryName = Id;
    internal const string DisplayName = "CrocoDS";
    internal const string MachineId = "cpc-6128";
    internal const string DescriptionResourceKey = "Emulation.Emulator.crocods.Description";
    internal static IReadOnlySet<string> Extensions { get; } =
        new HashSet<string>([StorageSettingsFunctionsConstants.Dsk, StorageSettingsFunctionsConstants.Sna, StorageSettingsFunctionsConstants.Kcr], StringComparer.OrdinalIgnoreCase);
    internal const string CoreHostCommand = "--amstrad-crocods-core-host";
    internal const string OptionKeyPrefix = "crocods_";
    internal const int MaximumInsertedMediaCount = 1;
}
