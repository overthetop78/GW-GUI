using System.Globalization;
using GWGUI.MediaEngine.Composition;
using GWGUI.MediaEngine.Definitions;

namespace GWGUI.Emulation.Atari.Functions;

internal static class AtariScpMediaFunctions
{
    internal static bool IsScp(string path) => Path.GetExtension(path).Equals(
        DiskImageFileExtensions.Scp, StringComparison.OrdinalIgnoreCase);

    internal static AtariSessionMedia Prepare(AtariMachineConfiguration configuration,
        AtariMediaConfiguration media, string sessionDirectory, IReadOnlySet<string> supportedExtensions)
    {
        if (!IsScp(media.Path))
            return AtariSessionMediaFunctions.Prepare(media, sessionDirectory, supportedExtensions);
        if (media.Category != AtariMediaCategory.Floppy)
            throw new InvalidDataException(AtariScpMediaFunctionsConstants.AnAtariSCPImageMustBeMountedAsFloppyMedia);

        var extension = configuration.Family switch
        {
            AtariMachineFamily.EightBit => DiskImageFileExtensions.Atr,
            AtariMachineFamily.St => DiskImageFileExtensions.St,
            _ => throw new NotSupportedException(
                $"SCP conversion is not supported for Atari family '{configuration.Family}'.")
        };
        var runtimeDirectory = Path.Combine(sessionDirectory, AtariSessionMediaConstants.SessionDirectoryName,
            string.Format(CultureInfo.InvariantCulture, AtariSessionMediaConstants.SessionInstanceNameFormat,
                media.Slot, Guid.NewGuid().ToString(AtariSessionMediaConstants.UniqueNameFormat)));
        Directory.CreateDirectory(runtimeDirectory);
        var runtimeName = string.Format(CultureInfo.InvariantCulture,
            AtariSessionMediaConstants.RuntimeFileNameFormat,
            AtariSessionMediaConstants.RuntimeFileNumberOffset,
            Path.GetFileNameWithoutExtension(media.Path) + extension);
        var runtimePath = Path.Combine(runtimeDirectory, runtimeName);
        var converter = MediaEngineFactory.CreateAtariScpRuntimeConversionService();
        if (configuration.Family == AtariMachineFamily.St)
            converter.ConvertToStAsync(media.Path, runtimePath).GetAwaiter().GetResult();
        else
            converter.ConvertToAtrAsync(media.Path, runtimePath).GetAwaiter().GetResult();
        AtariContentFunctions.Validate(runtimePath, supportedExtensions);
        return new AtariSessionMedia(media, runtimePath, [Path.GetFullPath(media.Path)], [runtimePath], false);
    }
}