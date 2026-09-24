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
        var members = HardDiskImageSetResolver.Resolve(fullPath);
        foreach (var member in members) RejectRedirectedPath(member);
        var users = await FindUsersAsync(members, current);
        if (users.Count != 0) { ShowUsers(users); return false; }
        var handles = HardDiskDeletionFile.Open(members);
        try
        {
            if (MessageBox.Show(LocExtension.Get("Emulation.Hdd.DeleteConfirm", string.Join(Environment.NewLine, members)),
                    LocExtension.Get("Emulation.Hdd.Delete"), MessageBoxButton.YesNo,
                    MessageBoxImage.Warning, MessageBoxResult.No) != MessageBoxResult.Yes) return false;
            var confirmedMembers = HardDiskImageSetResolver.Resolve(fullPath);
            if (!members.SequenceEqual(confirmedMembers, StringComparer.OrdinalIgnoreCase))
                throw new IOException("The disk image set changed while deletion was being confirmed.");
            users = await FindUsersAsync(members, current);
            if (users.Count != 0) { ShowUsers(users); return false; }
            HardDiskDeletionFile.Delete(handles, members);
            return true;
        }
        finally
        {
            foreach (var handle in handles) handle.Dispose();
        }
    }

    private static void RejectRedirectedPath(string path)
    {
        // Refuse links so deleting an alias cannot bypass reference checks.
        for (FileSystemInfo? entry = new FileInfo(path); entry is not null;
             entry = entry is FileInfo file ? file.Directory : ((DirectoryInfo)entry).Parent)
            if ((entry.Attributes & FileAttributes.ReparsePoint) != 0)
                throw new IOException(LocExtension.Get("Emulation.Hdd.Link"));
    }

    private static void ShowUsers(IReadOnlyList<string> users) => MessageBox.Show(
        LocExtension.Get("Emulation.Hdd.InUse", string.Join(Environment.NewLine, users)),
        LocExtension.Get("Emulation.Hdd.Delete"), MessageBoxButton.OK, MessageBoxImage.Information);

    private static async Task<IReadOnlyList<string>> FindUsersAsync(IReadOnlyList<string> paths,
        IEmulationConfiguration current)
    {
        var users = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
        var dependencies = new DiskImageDependencyIndex(image =>
            new FileStream(image, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete),
            HardDiskDeletionFile.ResolveReferencePath,
            ResolveInventoryReference);
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
                if (await UsesAsync(settings.MountedMedia, paths, dependencies))
                    users.Add($"{LocExtension.GetForModule(module, module.SummarizeConfiguration(configuration).MachineDisplayResourceKey)} · {configuration.Id}");
            }
        }
        foreach (var session in MachineSession.All)
            if (session.IsPowered && await UsesAsync(session.MountedMedia, paths, dependencies))
                users.Add(LocExtension.Get("Emulation.Hdd.OpenSession", session.DisplayName));
        return users.Order().ToArray();
    }

    private static string? ResolveInventoryReference(
        string childPath,
        DiskImageDependencyReference reference)
    {
        var extension = reference.Uuid is not null ? ".vdi" : reference.Sha1 is not null ? ".chd" : null;
        if (extension is null) return null;
        var matches = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var path in EnumerateNeighborImages(childPath, extension))
        {
            try
            {
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read,
                    FileShare.ReadWrite | FileShare.Delete);
                if (!DiskImageDependencyReader.ReadIdentities(stream).Contains(reference)) continue;
                matches.Add(Path.GetFullPath(path));
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or InvalidDataException or NotSupportedException)
            {
                // An unrelated or inaccessible neighboring image does not belong to this inventory entry.
            }
        }
        return matches.Count switch
        {
            0 => null,
            1 => matches.Single(),
            _ => throw new InvalidDataException("Several disk images expose the same dependency identifier.")
        };
    }

    private static IEnumerable<string> EnumerateNeighborImages(string childPath, string extension)
    {
        var childDirectory = Path.GetDirectoryName(Path.GetFullPath(childPath))!;
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var options = new EnumerationOptions
        {
            RecurseSubdirectories = true,
            IgnoreInaccessible = true,
            AttributesToSkip = FileAttributes.ReparsePoint
        };
        foreach (var path in Directory.EnumerateFiles(childDirectory, "*" + extension, options))
            if (paths.Add(Path.GetFullPath(path))) yield return path;
        var parentDirectory = Directory.GetParent(childDirectory)?.FullName;
        if (parentDirectory is null) yield break;
        foreach (var path in Directory.EnumerateFiles(parentDirectory, "*" + extension, SearchOption.TopDirectoryOnly))
            if (paths.Add(Path.GetFullPath(path))) yield return path;
    }

    private static Task<bool> UsesAsync(IEnumerable<EmulationMedia> media, IReadOnlyList<string> candidates,
        DiskImageDependencyIndex dependencies)
    {
        var paths = media.Where(item => item.Type == EmulationMediaType.HardDisk && !string.IsNullOrWhiteSpace(item.Path))
            .Select(item => item.Path).ToArray();
        return Task.Run(() => paths.Any(image => candidates.Any(candidate => dependencies.Uses(image, candidate))));
    }
}
