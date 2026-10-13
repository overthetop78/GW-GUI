namespace GWGUI.Emulation.Nec.Common.Interfaces;

internal interface IEmulatorFirmwareSettings
{
    IReadOnlyList<FirmwareSlot> FirmwareSlots { get; }
}
