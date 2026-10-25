using GWGUI.Emulation.Apple.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Apple.Common.Machines.Common.Enums;
using GWGUI.Emulation.Apple.Common.Machines.Pippin.Constants;

namespace GWGUI.Emulation.Apple.Common.Machines.Pippin.Dictionaries;

public static class PippinModelCatalog
{
    // Apple PPDV-ES001-DM: 66 MHz PowerPC 603, 6 MB shared system/video memory.
    // No catalogued core is qualified for this machine; do not substitute Mini vMac.
    public static IReadOnlyList<HardwareModelDefinition> All { get; } = Array.AsReadOnly(new[]
    {
        new HardwareModelDefinition(MachineModel.Pippin, nameof(MachineModel.Pippin), PippinModelConstants.ResourcePrefix + nameof(MachineModel.Pippin), Emulator.None,
            [HardwareCpu.PowerPC603], PippinModelConstants.CpuFrequencyHz, PippinModelConstants.SharedMemoryBytes, [HardwareRegion.Worldwide],
            [HardwareVideoCapability.Taos], [HardwareAudioCapability.Awacs], [HardwareStorageCapability.CompactDisc],
            [new(HardwarePortCapability.Gamepad, PippinModelConstants.ControllerPortCount)], [FirmwareCategory.SystemRom], [])
    });
}
