using GWGUI.App.Services.Emulation;

namespace GWGUI.App.Functions.Views.Emulation.Settings;

internal static class EmulationConfigurationPersistenceFunctions
{
    internal static async ValueTask<bool> PersistAsync(
        IEmulationModule module,
        IEmulationConfiguration configuration,
        bool hasSavedConfiguration,
        CancellationToken cancellationToken = default)
    {
        if (!hasSavedConfiguration)
        {
            EmulationConfigurationDraftStore.Set(module.Id, configuration);
            return false;
        }

        EmulationVideoPresentationProfiles.Store.Get(module.Id, configuration.Id);
        await module.SaveConfigurationAsync(configuration, cancellationToken);
        return true;
    }
}
