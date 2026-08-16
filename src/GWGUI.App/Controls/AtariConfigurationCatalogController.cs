using GWGUI.Emulation.Atari;
using GWGUI.App.Localization;

namespace GWGUI.App.Controls;

internal sealed class AtariConfigurationCatalogController
{
    private readonly AtariConfigurationStore _store;
    private Func<Guid, bool> _isActive = static _ => false;

    internal AtariConfigurationCatalogController(AtariConfigurationStore store) => _store = store;

    internal void ConfigureActiveCheck(Func<Guid, bool>? isActive) =>
        _isActive = isActive ?? (static _ => false);

    internal bool IsActive(Guid id) => _isActive(id);

    internal Task<IReadOnlyList<AtariMachineConfiguration>> LoadAsync() => _store.LoadAllAsync();

    internal async Task SaveAsync(AtariMachineConfiguration configuration)
    {
        EnsureInactive(configuration.Id);
        await _store.SaveAsync(configuration).ConfigureAwait(false);
    }

    internal void Delete(Guid id)
    {
        EnsureInactive(id);
        _store.Delete(id);
    }

    private void EnsureInactive(Guid id)
    {
        if (_isActive(id)) throw new InvalidOperationException(
            LocExtension.Get(AtariConfigurationCatalogConstants.ActiveConfigurationErrorResource));
    }
}
