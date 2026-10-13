namespace GWGUI.Emulation.Amstrad.Common.Interfaces;

internal interface IEmulatorCapabilities
{
    bool SupportsMouse { get; }
    bool SupportsVideoSettings { get; }
}
