namespace GWGUI.Emulation.Sharp.Common.Interfaces;

internal interface IEmulatorRuntimeCapabilities : IEmulatorPointerCapabilities
{
    bool SupportsNoContent { get; }
}
