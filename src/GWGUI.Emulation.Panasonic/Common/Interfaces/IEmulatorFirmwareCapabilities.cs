using GWGUI.Emulation.Panasonic.Common.Machines.Common.Contracts;

namespace GWGUI.Emulation.Panasonic.Common.Interfaces;

internal interface IEmulatorFirmwareCapabilities
{
    IReadOnlyList<FirmwareRequirement> DescribeFirmware(MachineConfiguration configuration);
    bool IsFirmwareCompatible(MachineConfiguration configuration, FirmwareRole role, string path);
}