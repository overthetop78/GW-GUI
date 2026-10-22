namespace GWGUI.Emulation.Snk.Common.Interfaces;

internal interface IEmulatorMediaAdapter : IEmulatorMediaCapabilities
{
    IReadOnlyList<string> ResolveInitialContentPaths(MachineConfiguration configuration);
}
