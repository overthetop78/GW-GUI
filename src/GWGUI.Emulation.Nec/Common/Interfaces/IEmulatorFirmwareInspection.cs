namespace GWGUI.Emulation.Nec.Common.Interfaces;

internal interface IEmulatorFirmwareInspection
{
    Firmware? IdentifyFirmware(Firmware firmware);
}
