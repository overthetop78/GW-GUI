using System.IO;
using System.Text;
using GWGUI.Emulation.Sharp.Emulators.Px68k.Constants;

namespace GWGUI.Emulation.Sharp.Emulators.Px68k.Functions;

internal static class MediaRuntimeFunctions
{
    internal static LoadedContent? Prepare(MachineConfiguration configuration, string sessionDirectory)
    {
        var lines = new List<string> { MediaRuntimeConstants.ConfigurationSection };
        foreach (var media in configuration.Media.Where(media => media.IsInserted).OrderBy(media => media.Slot))
        {
            var extensions = media.Category == MediaCategory.HardDisk ? MediaRuntimeConstants.HardDiskExtensions : MediaRuntimeConstants.FloppyExtensions;
            var path = SessionMediaFunctions.Prepare(media, sessionDirectory, extensions);
            var prefix = media.Category == MediaCategory.HardDisk ? MediaRuntimeConstants.HardDiskKeyPrefix : MediaRuntimeConstants.FloppyKeyPrefix;
            lines.Add(prefix + media.Slot.Index + "=" + path);
        }
        var directory = Path.Combine(sessionDirectory, CoreDirectoryConstants.SystemDirectoryName, FirmwareRuntimeConstants.SystemDirectory);
        Directory.CreateDirectory(directory);
        File.WriteAllLines(Path.Combine(directory, MediaRuntimeConstants.ConfigurationFileName), lines, new UTF8Encoding(false));
        return null;
    }
}
