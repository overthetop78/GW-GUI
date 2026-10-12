using GWGUI.Emulation.Sinclair.Emulators.EightyOne.Constants;

namespace GWGUI.Emulation.Sinclair.Emulators.EightyOne.Functions;
internal static class MediaRuntimeFunctions
{
    internal static LoadedContent Prepare(
        MachineConfiguration configuration,
        string sessionDirectory,
        IReadOnlySet<string> supportedExtensions
    )
    {
        if (configuration.InitialContent is not null || configuration.ConfiguredSlots.Any(slot => slot != EmulationMediaSlot.Cassette0) || configuration.Media.Any(media => media.Category != MediaCategory.Tape || media.Slot != EmulationMediaSlot.Cassette0))
            throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(configuration));
        var media = configuration.Media.SingleOrDefault(media => media.IsInserted);
        if (media is null)
            throw new EmulationException(ErrorCategory.Content, ErrorCode.ContentRequired, ErrorMessages.ContentRequired);
        var path = SessionMediaFunctions.Prepare(media, sessionDirectory, MediaRuntimeConstants.TapeExtensions);
        return ContentFunctions.Create(path, MediaRuntimeConstants.NeedsFullPath, supportedExtensions);
    }
}