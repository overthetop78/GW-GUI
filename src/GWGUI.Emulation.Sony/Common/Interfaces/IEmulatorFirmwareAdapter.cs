namespace GWGUI.Emulation.Sony.Common.Interfaces;

internal interface IEmulatorFirmwareAdapter
{
    bool RequiresExternalFirmware { get; }
    IReadOnlyList<FirmwareSlot> GetFirmwareSlots(MachineConfiguration configuration);
}
