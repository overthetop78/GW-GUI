namespace GWGUI.Emulation.Sinclair.Common.Interfaces;

internal interface IEmulatorRuntimeCapabilities
{
    bool SupportsPointerSettings { get; }
    bool SupportsNoContent { get; }
}
