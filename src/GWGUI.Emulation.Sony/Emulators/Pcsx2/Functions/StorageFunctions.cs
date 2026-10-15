using GWGUI.Emulation.HardDisks;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Constants;

namespace GWGUI.Emulation.Sony.Emulators.Pcsx2.Functions;

internal static class StorageFunctions
{
    private static readonly IReadOnlyList<HardDiskImageFormat> Formats = CreateFormats();

    internal static IReadOnlyList<EmulationMediaDevice> Describe(MachineConfiguration configuration) => configuration.Model != ModelConstants.PlayStation2 ? [] :
    [
        new(EmulationMediaSlot.HardDisk0, EmulationMediaType.HardDisk, [HardDiskConstants.Extension, HardDiskConstants.ImageExtension],
            IsRemovable: false, RequiresMachineRecreation: true, DisplayLabel: HardDiskConstants.InterfaceName,
            ModelResourceKey: OptionConstants.Dev9HddLabel, HardDiskFormats: Formats)
    ];

    private static IReadOnlyList<HardDiskImageFormat> CreateFormats()
    {
        var registry = DiskFormatRegistry.CreateDefault();
        var capacities = HardDiskConstants.CapacitiesGiB.Select(capacity => capacity * HardDiskConstants.Gibibyte).ToArray();
        return new[] { HardDiskConstants.Extension, HardDiskConstants.ImageExtension }.Select(extension => registry.Describe(new HardDiskImageFormat(
            HardDiskConstants.FormatId, extension, HardDiskConstants.InterfaceName, capacities.Max(), HardDiskConstants.DefaultCapacityGiB * HardDiskConstants.Gibibyte)
        {
            MinimumBytes = capacities.Min(),
            AllowedCapacitiesBytes = capacities
        })).ToArray();
    }
}
