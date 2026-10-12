namespace GWGUI.Emulation.Sharp.Common.Interfaces;

internal interface IEmulatorRuntimeCapabilities
{
    bool SupportsPointerSettings { get; }
    bool SupportsNoContent { get; }
}
