using GWGUI.Updates.Contracts;

namespace GWGUI.Updater;

internal sealed class UpdateInstallationTransaction
{
    private readonly UpdateExecutionPlan _plan;
    private readonly string _backupDirectory;
    private readonly HashSet<string> _changedModules = new(StringComparer.OrdinalIgnoreCase);
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
            var application = _plan.Components.FirstOrDefault(item => item.Kind == UpdateComponentKind.Application);
            if (application is not null) ReplaceApplication(application.PreparedDirectory);
            foreach (var module in _plan.Components.Where(item => item.Kind == UpdateComponentKind.Module))
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
        foreach (var module in _plan.Components.Where(item => item.Kind == UpdateComponentKind.Module
                     && _changedModules.Contains(item.ComponentId)))
        {
            var target = Path.Combine(_plan.InstallationDirectory, "Modules", module.ComponentId);
            if (Directory.Exists(target)) Directory.Delete(target, recursive: true);
            var backup = Path.Combine(modulesBackup, module.ComponentId);
            if (Directory.Exists(backup)) CopyDirectory(backup, target);
        }
        if (Directory.Exists(_backupDirectory)) Directory.Delete(_backupDirectory, recursive: true);
        _applicationChanged = false;
        _changedModules.Clear();
    }

    private void ReplaceApplication(string source)
    {
        var backup = Path.Combine(_backupDirectory, "application");
        Directory.CreateDirectory(backup);
        foreach (var entry in ApplicationEntries())
            CopyEntry(entry, Path.Combine(backup, Path.GetFileName(entry)));
        var preservePortable = File.Exists(Path.Combine(_plan.InstallationDirectory, "portable.flag"));
        _applicationChanged = true;
        DeleteApplicationEntries();
        CopyApplicationContents(source, preservePortable);
    }

    private void ReplaceModule(PreparedUpdateComponent module)
    {
        var target = Path.Combine(_plan.InstallationDirectory, "Modules", module.ComponentId);
        var backup = Path.Combine(_backupDirectory, "modules", module.ComponentId);
        if (Directory.Exists(target)) CopyDirectory(target, backup);
        _changedModules.Add(module.ComponentId);
        if (Directory.Exists(target)) Directory.Delete(target, recursive: true);
        CopyDirectory(module.PreparedDirectory, target);
    }

    private IEnumerable<string> ApplicationEntries() => Directory.EnumerateFileSystemEntries(_plan.InstallationDirectory)
        .Where(path => !Path.GetFileName(path).Equals("Data", StringComparison.OrdinalIgnoreCase)
            && !Path.GetFileName(path).Equals("Modules", StringComparison.OrdinalIgnoreCase)
            && !path.Equals(_backupDirectory, StringComparison.OrdinalIgnoreCase));

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
            CopyEntry(entry, Path.Combine(_plan.InstallationDirectory, name));
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
