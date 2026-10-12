namespace GWGUI.Emulation.Philips.Common.Interfaces;

internal interface IEmulatorVideoSettings
{
    (int Width, int Height)? GetOutputResolution(MachineConfiguration configuration);
}
