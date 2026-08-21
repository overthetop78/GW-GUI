using GWGUI.Emulation;

namespace GWGUI.App.Controls;

public sealed class EmulationConfigurationSavedEventArgs(IEmulationConfiguration configuration) : EventArgs
{
    public IEmulationConfiguration Configuration { get; } = configuration;
}
