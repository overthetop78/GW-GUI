using System.IO;

namespace GWGUI.Emulation.Sinclair.Emulators.Common.Interop.Functions;

internal static class SessionMediaFunctions
{
    internal static string Prepare(MediaConfiguration media, string sessionDirectory, IReadOnlyList<string> extensions)
    {
        var accepted = extensions.Select(extension => extension.TrimStart(MediaConstants.ExtensionPrefix)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var source = ContentFunctions.Validate(media.Path, accepted);
        if (!media.IsReadOnly) return source;
        var directory = Path.Combine(sessionDirectory, CoreDirectoryConstants.ContentDirectoryName);
        Directory.CreateDirectory(directory);
        var target = Path.Combine(directory, media.Slot.ToString() + Path.GetExtension(source).ToLowerInvariant());
        File.Copy(source, target, overwrite: true);
        File.SetAttributes(target, FileAttributes.Normal);
        return target;
    }
}
