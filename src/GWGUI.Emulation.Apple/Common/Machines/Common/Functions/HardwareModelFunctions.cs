using GWGUI.Emulation.Apple.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Apple.Common.Machines.Common.Enums;

namespace GWGUI.Emulation.Apple.Common.Machines.Common.Functions;

internal static class HardwareModelFunctions
{
    internal static IReadOnlyList<T> Values<T>(params T[] values) => Array.AsReadOnly(values);
    internal static bool IsFirmwareCompatible(HardwareModelDefinition definition, FirmwareCategory category) =>
        definition.Firmware.Contains(category);
    internal static bool IsMediaCompatible(HardwareModelDefinition definition, MediaCategory category,
        EmulationMediaSlot slot) => definition.Media.Contains(category) && category switch
        {
            MediaCategory.Floppy => slot == EmulationMediaSlot.Floppy0 || slot == EmulationMediaSlot.Floppy1,
            MediaCategory.HardDisk => slot == EmulationMediaSlot.HardDisk0,
            MediaCategory.Cassette => slot == EmulationMediaSlot.Cassette0,
            _ => false
        };
}
