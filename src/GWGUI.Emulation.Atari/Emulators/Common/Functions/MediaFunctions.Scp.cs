using System.IO;
using System.Globalization;
using CommonCoreHostFunctions = GWGUI.Emulation.Atari.Common.Functions.CoreHostFunctions;

namespace GWGUI.Emulation.Atari.Emulators.Common.Functions;

internal static class ScpMediaFunctions
{


    internal static bool IsScp(string path) => Path.GetExtension(path).Equals(CommonCoreHostFunctions.MediaProcessing.FileExtensions["Scp"], StringComparison.OrdinalIgnoreCase);

    internal static SessionMedia Prepare(MachineConfiguration configuration, MediaConfiguration media, string sessionDirectory, IReadOnlySet<string> supportedExtensions)
    {
        if (!IsScp(media.Path))
            return SessionMediaFunctions.Prepare(media, sessionDirectory, supportedExtensions);
        if (media.Category != MediaCategory.Floppy)
            throw new InvalidDataException(ScpMediaFunctionsErrors.FloppyMediaRequired);

        var extension = configuration.Family switch
        {
            MachineFamily.EightBit => CommonCoreHostFunctions.MediaProcessing.FileExtensions["Atr"],
            MachineFamily.St => CommonCoreHostFunctions.MediaProcessing.FileExtensions["St"],
            _ => throw new NotSupportedException($"SCP conversion is not supported for Atari family '{configuration.Family}'.")
        };
        var runtimeDirectory = Path.Combine(sessionDirectory, SessionMediaConstants.SessionDirectoryName,
            string.Format(CultureInfo.InvariantCulture, SessionMediaConstants.SessionInstanceNameFormat, media.Slot, Guid.NewGuid().ToString(SessionMediaConstants.UniqueNameFormat)));
        Directory.CreateDirectory(runtimeDirectory);
        var runtimeName = string.Format(CultureInfo.InvariantCulture, SessionMediaConstants.RuntimeFileNameFormat, SessionMediaConstants.RuntimeFileNumberOffset, Path.GetFileNameWithoutExtension(media.Path) + extension);
        var runtimePath = Path.Combine(runtimeDirectory, runtimeName);
        CommonCoreHostFunctions.MediaProcessing.ConvertScpAsync(media.Path, runtimePath, extension).GetAwaiter().GetResult();
        ContentFunctions.Validate(runtimePath, supportedExtensions);
        return new SessionMedia(media, runtimePath, [Path.GetFullPath(media.Path)], [runtimePath], false);
    }
}
