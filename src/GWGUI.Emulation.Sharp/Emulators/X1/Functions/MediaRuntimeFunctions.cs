using System.IO;
using GWGUI.Emulation.Sharp.Emulators.X1.Constants;

namespace GWGUI.Emulation.Sharp.Emulators.X1.Functions;

internal static class MediaRuntimeFunctions
{
    internal static LoadedContent Prepare(MachineConfiguration configuration, string sessionDirectory, IReadOnlySet<string> supportedExtensions)
    {
        var media = configuration.Media.SingleOrDefault(media => media.IsInserted);
        if (media is null) throw new EmulationException(ErrorCategory.Content, ErrorCode.ContentRequired, ErrorMessages.ContentRequired);
        var path = SessionMediaFunctions.Prepare(media, sessionDirectory, MediaRuntimeConstants.FloppyExtensions);
        return ContentFunctions.Create(path, true, supportedExtensions);
    }
}
