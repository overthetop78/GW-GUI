using System.IO;
using GWGUI.Emulation.Constants;
using GWGUI.Updates.Contracts;

namespace GWGUI.Updater;

internal sealed class UpdateInstallationTransaction
{
    private readonly UpdateExecutionPlan _plan;
    private readonly string _backupDirectory;
    private readonly Dictionary<string, bool> _changedModules = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, bool> _changedAssets = new(StringComparer.OrdinalIgnoreCase);
    private bool _applicationChanged;

    internal UpdateInstallationTransaction(UpdateExecutionPlan plan, string workDirectory)
    {
        _plan = plan;
        _backupDirectory = Path.Combine(plan.InstallationDirectory, $".gwgui-update-backup-{plan.TransactionId}");
    }

    internal void Apply()
    {
        if (Directory.Exists(_backupDirectory)) throw new IOException("The transaction backup already exists.");
        Directory.CreateDirectory(_backupDirectory);
        try
        {
            var application = _plan.Components.FirstOrDefault(item => item.Operation == UpdateComponentOperation.UpdateApplication);
            if (application is not null) ReplaceApplication(application.PreparedDirectory);
            foreach (var module in _plan.Components.Where(item => item.Operation is UpdateComponentOperation.InstallModule or UpdateComponentOperation.UpdateModule))
                ReplaceModule(module);
        }
        catch
        {
            Restore();
            throw;
        }
    }

    internal void Commit()
    {
        if (Directory.Exists(_backupDirectory)) Directory.Delete(_backupDirectory, recursive: true);
        _applicationChanged = false;
        _changedModules.Clear();
        _changedAssets.Clear();
    }

    internal void Restore()
    {
        if (!_applicationChanged && _changedModules.Count == 0 && !Directory.Exists(_backupDirectory)) return;
        var applicationBackup = Path.Combine(_backupDirectory, "application");
        if (_applicationChanged && Directory.Exists(applicationBackup))
        {
            DeleteApplicationEntries();
            CopyDirectoryContents(applicationBackup, _plan.InstallationDirectory);
        }
        var modulesBackup = Path.Combine(_backupDirectory, "modules");
        foreach (var module in _plan.Components.Where(item => _changedModules.ContainsKey(item.ComponentId)))
        {
            var target = Path.Combine(_plan.InstallationDirectory, "Modules", module.ComponentId);
            if (Directory.Exists(target)) Directory.Delete(target, recursive: true);
            var backup = Path.Combine(modulesBackup, module.ComponentId);
            if (_changedModules[module.ComponentId] && Directory.Exists(backup)) CopyDirectory(backup, target);
        }
        foreach (var assets in _changedAssets)
        {
            var target = ModuleAssetsDirectory(_plan.InstallationDirectory, assets.Key);
            if (Directory.Exists(target)) Directory.Delete(target, recursive: true);
            var backup = ModuleAssetsDirectory(_backupDirectory, assets.Key);
            if (assets.Value && Directory.Exists(backup)) CopyDirectory(backup, target);
        }
        if (Directory.Exists(_backupDirectory)) Directory.Delete(_backupDirectory, recursive: true);
        _applicationChanged = false;
        _changedModules.Clear();
        _changedAssets.Clear();
    }

    private static string ModuleAssetsDirectory(string root, string moduleId) =>
        Path.Combine(root, EmulationAssetPaths.AssetsDirectory, EmulationAssetPaths.ModulesDirectory, moduleId);

    private void ReplaceApplication(string source)
    {
        var backup = Path.Combine(_backupDirectory, "application");
        Directory.CreateDirectory(backup);
        foreach (var entry in ApplicationEntries())
            CopyEntry(entry, Path.Combine(backup, Path.GetRelativePath(_plan.InstallationDirectory, entry)));
        var preservePortable = File.Exists(Path.Combine(_plan.InstallationDirectory, "portable.flag"));
        _applicationChanged = true;
        DeleteApplicationEntries();
        CopyApplicationContents(source, preservePortable);
    }

    private void ReplaceModule(PreparedUpdateComponent module)
    {
        var target = Path.Combine(_plan.InstallationDirectory, "Modules", module.ComponentId);
        var backup = Path.Combine(_backupDirectory, "modules", module.ComponentId);
        var existed = Directory.Exists(target);
        if (module.Operation == UpdateComponentOperation.InstallModule && existed)
            throw new IOException($"Module '{module.ComponentId}' is already installed.");
        if (module.Operation == UpdateComponentOperation.UpdateModule && !existed)
            throw new DirectoryNotFoundException($"Installed module '{module.ComponentId}' was not found.");
        if (existed) CopyDirectory(target, backup);
        var assetTarget = ModuleAssetsDirectory(_plan.InstallationDirectory, module.ComponentId);
        var assetsExisted = Directory.Exists(assetTarget);
        if (assetsExisted) CopyDirectory(assetTarget, ModuleAssetsDirectory(_backupDirectory, module.ComponentId));
        _changedAssets.Add(module.ComponentId, assetsExisted);
        _changedModules.Add(module.ComponentId, existed);
        if (existed) Directory.Delete(target, recursive: true);
        CopyDirectory(module.PreparedDirectory, target);
        if (Directory.Exists(assetTarget)) Directory.Delete(assetTarget, recursive: true);
        var assetSource = ModuleAssetsDirectory(module.PreparedDirectory, module.ComponentId);
        if (Directory.Exists(assetSource)) CopyDirectory(assetSource, assetTarget);
        var stagedAssets = Path.Combine(target, EmulationAssetPaths.AssetsDirectory);
        if (Directory.Exists(stagedAssets)) Directory.Delete(stagedAssets, recursive: true);
    }

    private IEnumerable<string> ApplicationEntries() => Directory.EnumerateFileSystemEntries(_plan.InstallationDirectory)
        .Where(path => !Path.GetFileName(path).Equals("Data", StringComparison.OrdinalIgnoreCase)
            && !Path.GetFileName(path).Equals("Modules", StringComparison.OrdinalIgnoreCase)
            && !path.Equals(_backupDirectory, StringComparison.OrdinalIgnoreCase))
        .SelectMany(path => Directory.Exists(path) && Path.GetFileName(path).Equals(EmulationAssetPaths.AssetsDirectory, StringComparison.OrdinalIgnoreCase)
            ? Directory.EnumerateFileSystemEntries(path).Where(asset => !Path.GetFileName(asset).Equals(EmulationAssetPaths.ModulesDirectory, StringComparison.OrdinalIgnoreCase))
            : new[] { path });

    private void DeleteApplicationEntries()
    {
        foreach (var entry in ApplicationEntries().ToArray())
        {
            if (Directory.Exists(entry)) Directory.Delete(entry, recursive: true);
            else File.Delete(entry);
        }
    }

    private void CopyApplicationContents(string source, bool preservePortable)
    {
        foreach (var entry in Directory.EnumerateFileSystemEntries(source))
        {
            var name = Path.GetFileName(entry);
            if (name.Equals("Data", StringComparison.OrdinalIgnoreCase)
                || name.Equals("Modules", StringComparison.OrdinalIgnoreCase)
                || name.Equals("portable.flag", StringComparison.OrdinalIgnoreCase) && !preservePortable) continue;
            if (Directory.Exists(entry) && name.Equals(EmulationAssetPaths.AssetsDirectory, StringComparison.OrdinalIgnoreCase))
            {
                foreach (var asset in Directory.EnumerateFileSystemEntries(entry))
                    if (!Path.GetFileName(asset).Equals(EmulationAssetPaths.ModulesDirectory, StringComparison.OrdinalIgnoreCase))
                        CopyEntry(asset, Path.Combine(_plan.InstallationDirectory, name, Path.GetFileName(asset)));
            }
            else CopyEntry(entry, Path.Combine(_plan.InstallationDirectory, name));
        }
    }

    private static void CopyDirectoryContents(string source, string destination)
    {
        foreach (var entry in Directory.EnumerateFileSystemEntries(source))
            CopyEntry(entry, Path.Combine(destination, Path.GetFileName(entry)));
    }

    private static void CopyEntry(string source, string destination)
    {
        if (Directory.Exists(source)) CopyDirectory(source, destination);
        else
        {
            EnsureRegular(source);
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(source, destination, overwrite: true);
        }
    }

    private static void CopyDirectory(string source, string destination)
    {
        EnsureRegular(source);
        Directory.CreateDirectory(destination);
        foreach (var directory in Directory.EnumerateDirectories(source, "*", SearchOption.AllDirectories))
        {
            EnsureRegular(directory);
            Directory.CreateDirectory(Path.Combine(destination, Path.GetRelativePath(source, directory)));
        }
        foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            EnsureRegular(file);
            var target = Path.Combine(destination, Path.GetRelativePath(source, file));
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(file, target, overwrite: true);
        }
    }

    private static void EnsureRegular(string path)
    {
        if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
            throw new InvalidDataException($"A reparse point cannot be updated: {path}");
    }
}
