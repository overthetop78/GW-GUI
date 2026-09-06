using System.IO;
using GWGUI.Emulation.Interfaces;

namespace GWGUI.App.Services.Emulation;

internal sealed class MachineQuickStates(
    Func<IEmulationSavedStates> states, string path,
    Func<string, bool>? exists = null, Action<string>? createDirectory = null)
{
    private readonly Func<string, bool> _exists = exists ?? File.Exists;
    private readonly Action<string> _createDirectory = createDirectory ?? (directory => Directory.CreateDirectory(directory));

    internal bool IsSupported => states().IsSupported;
    internal bool IsAvailable => IsSupported && _exists(path);

    internal async Task<bool> SaveAsync(CancellationToken cancellationToken = default)
    {
        var current = states();
        if (!current.IsSupported) return false;
        cancellationToken.ThrowIfCancellationRequested();
        var folder = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(folder)) _createDirectory(folder);
        await current.SaveAsync(path, cancellationToken);
        return true;
    }

    internal async Task<bool> LoadAsync(CancellationToken cancellationToken = default)
    {
        var current = states();
        if (!current.IsSupported || !_exists(path)) return false;
        cancellationToken.ThrowIfCancellationRequested();
        await current.LoadAsync(path, cancellationToken);
        return true;
    }
}
