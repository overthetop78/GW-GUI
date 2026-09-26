using System.Globalization;
using GWGUI.MediaEngine;
using GWGUI.MediaEngine.Constants;

namespace GWGUI.Emulation.Atari.Emulators.Libretro.Functions;

internal static class ScpMediaFunctions
{
    private static readonly MediaEngineComposition MediaEngine = MediaEngineComposition.CreateDefault();

    internal static bool IsScp(string path) => Path.GetExtension(path).Equals(
        DiskImageFileExtensions.Scp, StringComparison.OrdinalIgnoreCase);

    internal static SessionMedia Prepare(MachineConfiguration configuration,
        MediaConfiguration media, string sessionDirectory, IReadOnlySet<string> supportedExtensions)
    {
        if (!IsScp(media.Path))
            return SessionMediaFunctions.Prepare(media, sessionDirectory, supportedExtensions);
        if (media.Category != MediaCategory.Floppy)
            throw new InvalidDataException(ScpMediaFunctionsErrors.FloppyMediaRequired);

        var extension = configuration.Family switch
        {
            MachineFamily.EightBit => DiskImageFileExtensions.Atr,
            MachineFamily.St => DiskImageFileExtensions.St,
            _ => throw new NotSupportedException(
                $"SCP conversion is not supported for Atari family '{configuration.Family}'.")
        };
        var runtimeDirectory = Path.Combine(sessionDirectory, SessionMediaConstants.SessionDirectoryName,
            string.Format(CultureInfo.InvariantCulture, SessionMediaConstants.SessionInstanceNameFormat,
                media.Slot, Guid.NewGuid().ToString(SessionMediaConstants.UniqueNameFormat)));
        Directory.CreateDirectory(runtimeDirectory);
        var runtimeName = string.Format(CultureInfo.InvariantCulture,
            SessionMediaConstants.RuntimeFileNameFormat,
            SessionMediaConstants.RuntimeFileNumberOffset,
            Path.GetFileNameWithoutExtension(media.Path) + extension);
        var runtimePath = Path.Combine(runtimeDirectory, runtimeName);
        var converter = MediaEngine.Conversion.AtariScpRuntimeConverter;
        if (configuration.Family == MachineFamily.St)
            converter.ConvertToStAsync(media.Path, runtimePath).GetAwaiter().GetResult();
        else
            converter.ConvertToAtrAsync(media.Path, runtimePath).GetAwaiter().GetResult();
        ContentFunctions.Validate(runtimePath, supportedExtensions);
        return new SessionMedia(media, runtimePath, [Path.GetFullPath(media.Path)], [runtimePath], false);
    }
}
