using System.Globalization;
using System.IO;
using GWGUI.Emulation.Services;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Constants;

namespace GWGUI.Emulation.Sony.Emulators.Pcsx2.Functions;

internal static class HardDiskRuntimeFunctions
{
    internal static string NativeCapacity(int capacityGiB) => capacityGiB.ToString(CultureInfo.InvariantCulture) + " GiB";

    internal static MediaConfiguration? Selected(MachineConfiguration configuration)
    {
        var disks = (configuration.Media ?? []).Where(media => media.Category == MediaCategory.HardDisk && media.IsInserted).ToArray();
        if (disks.Length > 1) throw new InvalidDataException(nameof(configuration.Media));
        return disks.SingleOrDefault();
    }

    internal static MachineConfiguration Configure(MachineConfiguration configuration)
    {
        var disk = Selected(configuration);
        var options = new Dictionary<string, string>(configuration.Options ?? new Dictionary<string, string>(), StringComparer.Ordinal)
        {
            [OptionConstants.Dev9HddKey] = disk is null ? "disabled" : "enabled",
            [OptionConstants.Dev9HddSizeKey] = NativeCapacity(HardDiskConstants.DefaultCapacityGiB)
        };
        if (disk is not null)
        {
            var size = new FileInfo(disk.Path).Length;
            // ATA IDENTIFY advertises the configured capacity, independently of the
            // file length. Do not round a different image size to a native preset.
            var capacity = size / HardDiskConstants.Gibibyte;
            if (size % HardDiskConstants.Gibibyte != 0 || capacity > HardDiskConstants.CapacitiesGiB.Max()
                || !HardDiskConstants.CapacitiesGiB.Contains((int)capacity))
                throw new InvalidDataException(disk.Path);
            options[OptionConstants.Dev9HddSizeKey] = NativeCapacity((int)capacity);
        }
        return configuration with { Options = options };
    }

    internal static string SystemDirectory(MachineConfiguration configuration, string physicalDirectory) => Selected(configuration) is null ? physicalDirectory : HardDiskConstants.SystemUri;

    internal static (string Path, bool IsReadOnly) Resolve(MachineConfiguration configuration, string systemDirectory, string path)
    {
        var normalized = path.Replace('\\', '/');
        if (!normalized.Equals(HardDiskConstants.SystemUri, StringComparison.Ordinal)
            && !normalized.StartsWith(HardDiskConstants.SystemUri + "/", StringComparison.Ordinal)) return (path, false);
        var relative = normalized[HardDiskConstants.SystemUri.Length..].TrimStart('/');
        if (relative.Equals(HardDiskConstants.NativeFile, StringComparison.OrdinalIgnoreCase))
        {
            var disk = Selected(configuration) ?? throw new FileNotFoundException(null, path);
            return (Path.GetFullPath(disk.Path), disk.IsReadOnly);
        }
        var root = Path.GetFullPath(systemDirectory);
        var resolved = Path.GetFullPath(Path.Combine(root, relative));
        if (!resolved.Equals(root, StringComparison.OrdinalIgnoreCase) && !resolved.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException(path);
        return (resolved, false);
    }
}
