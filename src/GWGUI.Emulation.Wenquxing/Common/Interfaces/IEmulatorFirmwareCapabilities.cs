using GWGUI.Emulation.Wenquxing.Common.Machines.Common.Contracts;

namespace GWGUI.Emulation.Wenquxing.Common.Interfaces;

internal interface IEmulatorFirmwareCapabilities
{
    IReadOnlyList<FirmwareRequirement> DescribeFirmware(MachineConfiguration configuration);
    bool IsFirmwareCompatible(MachineConfiguration configuration, FirmwareRole role, string path);
}