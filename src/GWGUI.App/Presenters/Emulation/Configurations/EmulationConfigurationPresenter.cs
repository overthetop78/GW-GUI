using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Localization.Extensions;
using GWGUI.Emulation;

namespace GWGUI.App.Presenters.Emulation.Configurations;

internal static class EmulationConfigurationPresenter
{
    internal static string DisplayName(IEmulationModule module, IEmulationConfiguration configuration)
    {
        var summary = module.SummarizeConfiguration(configuration);
        var identifier = configuration.Id.ToString(ControlVisualConstants.IdentifierFormat)
            [..ControlVisualConstants.DisplayIdentifierLength];
        return string.Join(ControlVisualConstants.DetailSeparator,
            new[] { LocExtension.Get(summary.MachineDisplayResourceKey) }
                .Concat(summary.Details)
                .Append(identifier));
    }
}
