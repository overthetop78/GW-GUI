namespace GWGUI.Emulation.Apple.Common.Interfaces;

internal interface IEmulatorPointerCapabilities
{
    bool SupportsPointer(MachineConfiguration configuration);
}
