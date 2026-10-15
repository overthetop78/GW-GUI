using GWGUI.Emulation.Apple.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Apple.Common.Machines.Common.Enums;

namespace GWGUI.Emulation.Apple.Common.Machines.Pippin.Dictionaries;

public static class PippinModelCatalog
{
    // Apple PPDV-ES001-DM: 66 MHz PowerPC 603, 6 MB shared system/video memory.
    // No catalogued core is qualified for this machine; do not substitute Mini vMac.
    public static IReadOnlyList<HardwareModelDefinition> All { get; } = Array.AsReadOnly(new[]
    {
        new HardwareModelDefinition(MachineModel.Pippin, nameof(MachineModel.Pippin), "Emulation.Apple.Model.Pippin", Emulator.None,
            [HardwareCpu.PowerPC603], 66_000_000, 6 * 1024 * 1024, [HardwareRegion.Worldwide],
            [HardwareVideoCapability.Taos], [HardwareAudioCapability.Awacs], [HardwareStorageCapability.CompactDisc],
            [new(HardwarePortCapability.Gamepad, 2)], [FirmwareCategory.SystemRom], [])
    });
}
