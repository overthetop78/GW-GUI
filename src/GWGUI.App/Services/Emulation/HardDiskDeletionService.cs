using System.IO;
using System.Windows;
using GWGUI.App.Localization.Extensions;
using GWGUI.Emulation;
using GWGUI.Emulation.HardDisks;

namespace GWGUI.App.Services.Emulation;

internal static class HardDiskDeletionService
{
    internal static async Task<bool> DeleteAsync(string path, IEmulationConfiguration current)
    {
        var fullPath = Path.GetFullPath(path);
        // Refuse links so deleting an alias cannot bypass reference checks.
        for (FileSystemInfo? entry = new FileInfo(fullPath); entry is not null;
             entry = entry is FileInfo file ? file.Directory : ((DirectoryInfo)entry).Parent)
            if ((entry.Attributes & FileAttributes.ReparsePoint) != 0)
                throw new IOException(LocExtension.Get("Emulation.Hdd.Link"));

        var users = await FindUsersAsync(fullPath, current);
        if (users.Count != 0) { ShowUsers(users); return false; }
        using var handle = HardDiskDeletionFile.Open(fullPath);
        if (MessageBox.Show(LocExtension.Get("Emulation.Hdd.DeleteConfirm", fullPath),
                LocExtension.Get("Emulation.Hdd.Delete"), MessageBoxButton.YesNo,
                MessageBoxImage.Warning, MessageBoxResult.No) != MessageBoxResult.Yes) return false;
        users = await FindUsersAsync(fullPath, current);
        if (users.Count != 0) { ShowUsers(users); return false; }
        HardDiskDeletionFile.Delete(handle);
        return true;
    }

    private static void ShowUsers(IReadOnlyList<string> users) => MessageBox.Show(
        LocExtension.Get("Emulation.Hdd.InUse", string.Join(Environment.NewLine, users)),
        LocExtension.Get("Emulation.Hdd.Delete"), MessageBoxButton.OK, MessageBoxImage.Information);

    private static async Task<IReadOnlyList<string>> FindUsersAsync(string path,
        IEmulationConfiguration current)
    {
        var users = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
        var dependencies = new DiskImageDependencyIndex(image =>
            new FileStream(image, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete),
            HardDiskDeletionFile.ResolveReferencePath);
        foreach (var module in EmulationModuleRegistry.Modules)
        {
            if (module is not IEmulationStorageSettingsManager manager) continue;
            var savedConfigurations = (await module.LoadConfigurationsAsync()).ToArray();
            var savedIds = savedConfigurations.Select(item => item.Id).ToHashSet();
            var configurations = savedConfigurations
                .Concat(EmulationConfigurationDraftStore.All.Where(item => item.ModuleId == module.Id
                    && !savedIds.Contains(item.Id)))
                .Where(item => item.Id != current.Id || item.ModuleId != current.ModuleId)
                .GroupBy(item => item.Id)
                .Select(group => group.First());
            foreach (var configuration in configurations)
            {
                var settings = manager.DescribeStorageSettings(configuration);
                if (await UsesAsync(settings.MountedMedia, path, dependencies))
                    users.Add($"{LocExtension.GetForModule(module, module.SummarizeConfiguration(configuration).MachineDisplayResourceKey)} · {configuration.Id}");
            }
        }
        foreach (var session in MachineSession.All)
            if (session.IsPowered && await UsesAsync(session.MountedMedia, path, dependencies))
                users.Add(LocExtension.Get("Emulation.Hdd.OpenSession", session.DisplayName));
        return users.Order().ToArray();
    }

    private static Task<bool> UsesAsync(IEnumerable<EmulationMedia> media, string path, DiskImageDependencyIndex dependencies)
    {
        var paths = media.Where(item => item.Type == EmulationMediaType.HardDisk && !string.IsNullOrWhiteSpace(item.Path))
            .Select(item => item.Path).ToArray();
        return Task.Run(() => paths.Any(image => dependencies.Uses(image, path)));
    }
}
