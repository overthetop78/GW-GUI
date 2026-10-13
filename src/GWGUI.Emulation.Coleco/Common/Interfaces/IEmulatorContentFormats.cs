namespace GWGUI.Emulation.Coleco.Common.Interfaces;

internal interface IEmulatorContentFormats
{
    IReadOnlySet<string> SupportedContentExtensions(IReadOnlySet<string> declaredExtensions, bool requiresFilePath);
}
