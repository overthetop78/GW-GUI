namespace GWGUI.Emulation.Sinclair.Common.Interfaces;

internal interface IEmulatorInitialContentSettings
{
    bool SupportsInitialContent { get; }
    IReadOnlyList<string> ResolveInitialContentPaths(MachineConfiguration configuration);
}
