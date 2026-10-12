using System.IO;
using System.Globalization;

namespace GWGUI.Emulation.Acorn.Emulators.Common.Functions;

internal static class SessionMediaFunctions
{
    internal static SessionMedia Prepare(MediaConfiguration configuration, string sessionDirectory, IReadOnlySet<string> supportedExtensions)
    {
        var source = ContentFunctions.Validate(configuration.Path, supportedExtensions);
        var runtimeDirectory = Path.Combine(sessionDirectory, SessionMediaConstants.SessionDirectoryName,
            string.Format(CultureInfo.InvariantCulture, SessionMediaConstants.SessionInstanceNameFormat, configuration.Slot, Guid.NewGuid().ToString(SessionMediaConstants.UniqueNameFormat)));
        Directory.CreateDirectory(runtimeDirectory);
        var runtimePath = Path.Combine(runtimeDirectory, Path.GetFileName(source));
        File.Copy(source, runtimePath);
        var attributes = File.GetAttributes(runtimePath);
        File.SetAttributes(runtimePath, configuration.IsReadOnly ? attributes | FileAttributes.ReadOnly : attributes & ~FileAttributes.ReadOnly);
        return new SessionMedia(configuration, runtimePath, [source], [runtimePath], !configuration.IsReadOnly);
    }

    internal static void Save(SessionMedia media)
    {
        if (!media.RequiresExplicitSave)
            throw new InvalidOperationException(SessionMediaErrors.ExplicitSaveRequired);
        File.Copy(media.RuntimePath, media.SourcePaths[SessionMediaConstants.FirstMediaIndex], overwrite: true);
    }
}
