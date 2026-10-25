using GWGUI.Emulation.Sony.Emulators.Rpcs3.Constants;
using GWGUI.Emulation.Sony.Emulators.Rpcs3.Dictionaries;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;

namespace GWGUI.Emulation.Sony.Emulators.Rpcs3.Functions;

internal static class StorageFunctions
{
    internal static IReadOnlyList<EmulationMediaDevice> Describe(MachineConfiguration configuration) =>
    [
        new(new EmulationMediaSlot(EmulationMediaCategory.HardDisk, 0), EmulationMediaType.HardDisk, [],
            IsRemovable: false, RequiresMachineRecreation: true, DisplayLabel: "HDD",
            IsPermanent: true, UsesDirectory: true,
            ModelResourceKey: "Emulation.Sony.Storage.InternalHdd",
            TypeResourceKey: "Emulation.Sony.Storage.InternalHdd",
            ConfigurationKind: EmulationStorageConfigurationKind.ManagedStorage,
            ConfigurationOptionDetailedResourceKey: "Emulation.Sony.Storage.ManagedDirectory.Help",
            ConfigurationFields: Fields(configuration, false))
    ];

    internal static IReadOnlyList<EmulationSettingsField> Fields(MachineConfiguration configuration, bool optical) =>
        CoreSettingsFunctions.Blocks(OptionCatalog.All, configuration).SelectMany(block => block.Fields)
            .Where(field => optical ? field.Id == OptionConstants.EmulateBdvdSpeedKey
                : field.Id is OptionConstants.EmulateHddSpeedKey or OptionConstants.DiskCacheSizeKey).ToArray();
}
