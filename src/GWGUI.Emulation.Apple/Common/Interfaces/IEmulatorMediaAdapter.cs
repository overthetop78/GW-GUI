namespace GWGUI.Emulation.Apple.Common.Interfaces;

internal interface IEmulatorMediaAdapter
{
    IReadOnlyList<string> GetMediaDependencies(string path);
}
