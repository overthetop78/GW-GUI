namespace GWGUI.Emulation.Sinclair.Common.Interfaces;

internal interface IEmulatorRuntimeCapabilities : IEmulatorPointerCapabilities
{
    bool SupportsNoContent { get; }
}
