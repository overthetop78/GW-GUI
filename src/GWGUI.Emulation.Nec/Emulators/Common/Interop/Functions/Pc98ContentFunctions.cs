using System.IO;
using System.Text;

namespace GWGUI.Emulation.Nec.Emulators.Common.Interop.Functions;

internal static class Pc98ContentFunctions
{
    internal static string Prepare(IReadOnlyList<MediaConfiguration> media,
        string contentDirectory, string systemDirectory,
        string systemSubdirectory, IReadOnlyDictionary<string, string> profiles, IReadOnlyList<string> hardDiskKeys,
        string executableName, int commandBufferLimit, int firstDriveIndex, int driveCount)
    {
        var firmwareDirectory = Path.Combine(systemDirectory, systemSubdirectory);
        Directory.CreateDirectory(firmwareDirectory);
        foreach (var profile in profiles)
        {
            var lines = new List<string> { profile.Value };
            for (var index = firstDriveIndex; index < driveCount; index++)
            {
                var disk = media.Where(item => item.Category == MediaCategory.HardDisk)
                    .FirstOrDefault(item => item.SlotIndex == index || item.SlotIndex is null
                        && media.Where(value => value.Category == MediaCategory.HardDisk).ToList().IndexOf(item) == index);
                lines.Add(hardDiskKeys[index] + '='
                    + (disk is null ? string.Empty : Path.GetFullPath(disk.Path)));
            }
            File.WriteAllLines(Path.Combine(firmwareDirectory, profile.Key), lines, new UTF8Encoding(false));
        }
        var arguments = new List<string> { executableName };
        var floppies = media.Where(item => item.Category == MediaCategory.Floppy).ToArray();
        for (var index = firstDriveIndex; index < driveCount; index++)
        {
            var disk = floppies.FirstOrDefault(item => item.SlotIndex == index)
                ?? floppies.Where(item => item.SlotIndex is null).ElementAtOrDefault(index);
            arguments.Add(disk is null ? "-"
                : '"' + Path.GetFullPath(disk.Path) + '"');
        }
        var command = string.Join(' ', arguments);
        if (Encoding.UTF8.GetByteCount(command) >= commandBufferLimit)
            throw new PathTooLongException();
        var path = Path.Combine(contentDirectory, "startup.cmd");
        File.WriteAllText(path, command, new UTF8Encoding(false));
        return path;
    }
}
