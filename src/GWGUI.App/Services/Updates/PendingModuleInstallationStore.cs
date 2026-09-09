using System.IO;
using GWGUI.App.Contracts.Updates;

namespace GWGUI.App.Services.Updates;

internal sealed class PendingModuleInstallationStore
{
    private readonly Dictionary<string, PendingModuleInstallation> _items =
        new(StringComparer.OrdinalIgnoreCase);

    internal event EventHandler? Changed;

    internal IReadOnlyList<PendingModuleInstallation> Items =>
        _items.Values.OrderBy(item => item.DisplayName, StringComparer.CurrentCultureIgnoreCase).ToArray();

    internal bool Contains(string moduleId) => _items.ContainsKey(moduleId);

    internal bool TryAdd(PendingModuleInstallation installation)
    {
        if (!_items.TryAdd(installation.ModuleId, installation)) return false;
        Changed?.Invoke(this, EventArgs.Empty);
        return true;
    }

    internal void Clear()
    {
        var items = _items.Values.ToArray();
        _items.Clear();
        foreach (var item in items) DeleteDirectory(item.WorkingDirectory);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private static void DeleteDirectory(string path)
    {
        try { if (Directory.Exists(path)) Directory.Delete(path, recursive: true); }
        catch { }
    }
}
