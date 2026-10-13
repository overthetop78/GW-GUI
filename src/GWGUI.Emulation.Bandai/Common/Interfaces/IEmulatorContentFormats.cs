namespace GWGUI.Emulation.Bandai.Common.Interfaces;

internal interface IEmulatorContentFormats
{
    IReadOnlySet<string> SupportedContentExtensions(IReadOnlySet<string> declaredExtensions, bool requiresFilePath);
}
