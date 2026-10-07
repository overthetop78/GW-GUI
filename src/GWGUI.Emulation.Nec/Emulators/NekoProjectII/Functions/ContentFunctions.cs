using System.IO;
using System.Text;
using ContentConstants = GWGUI.Emulation.Nec.Emulators.NekoProjectII.Constants.ContentConstants;
using FirmwareConstants = GWGUI.Emulation.Nec.Emulators.NekoProjectII.Constants.FirmwareConstants;
using StorageConstants = GWGUI.Emulation.Nec.Emulators.NekoProjectII.Constants.StorageConstants;

namespace GWGUI.Emulation.Nec.Emulators.NekoProjectII.Functions;

internal static class ContentFunctions
{
    internal static string Prepare(IReadOnlyList<MediaConfiguration> media,
        string contentDirectory, string systemDirectory)
    {
        var firmwareDirectory = Path.Combine(systemDirectory, FirmwareConstants.SystemSubdirectory);
        Directory.CreateDirectory(firmwareDirectory);
        foreach (var profile in ContentConstants.Profiles)
        {
            var lines = new List<string> { profile.Value };
            for (var index = StorageConstants.FirstDriveIndex; index < StorageConstants.DriveCount; index++)
            {
                var disk = media.Where(item => item.Category == MediaCategory.HardDisk)
                    .FirstOrDefault(item => item.SlotIndex == index || item.SlotIndex is null
                        && media.Where(value => value.Category == MediaCategory.HardDisk).ToList().IndexOf(item) == index);
                lines.Add(ContentConstants.HardDiskKeys[index] + ContentConstants.Assignment
                    + (disk is null ? string.Empty : Path.GetFullPath(disk.Path)));
            }
            File.WriteAllLines(Path.Combine(firmwareDirectory, profile.Key), lines, new UTF8Encoding(false));
        }
        var arguments = new List<string> { ContentConstants.ExecutableName };
        var floppies = media.Where(item => item.Category == MediaCategory.Floppy).ToArray();
        for (var index = StorageConstants.FirstDriveIndex; index < StorageConstants.DriveCount; index++)
        {
            var disk = floppies.FirstOrDefault(item => item.SlotIndex == index)
                ?? floppies.Where(item => item.SlotIndex is null).ElementAtOrDefault(index);
            arguments.Add(disk is null ? ContentConstants.EmptyDriveArgument
                : ContentConstants.Quote + Path.GetFullPath(disk.Path) + ContentConstants.Quote);
        }
        var command = string.Join(ContentConstants.ArgumentSeparator, arguments);
        if (Encoding.UTF8.GetByteCount(command) >= ContentConstants.CommandBufferLimit)
            throw new PathTooLongException();
        var path = Path.Combine(contentDirectory, ContentConstants.CommandFileName);
        File.WriteAllText(path, command, new UTF8Encoding(false));
        return path;
    }
}
