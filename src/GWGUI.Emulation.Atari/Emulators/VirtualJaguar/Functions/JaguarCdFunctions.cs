using GWGUI.Emulation.Atari.Emulators.VirtualJaguar.Constants;
using GWGUI.Emulation.Atari.Emulators.VirtualJaguar.Exceptions;
using GWGUI.Emulation.Atari.Emulators.VirtualJaguar.Contracts;
using GWGUI.Emulation.Atari.Emulators.VirtualJaguar.Functions;

namespace GWGUI.Emulation.Atari.Emulators.VirtualJaguar.Functions;

internal static class JaguarCdFunctions
{
    internal static bool IsSupported(IReadOnlySet<string> reportedExtensions) =>
        JaguarCdConstants.CompleteDiscExtensions.Any(reportedExtensions.Contains);

    internal static PreparedJaguarCd Prepare(
        MachineConfiguration machine,
        MediaConfiguration media,
        bool needsFullPath,
        IReadOnlySet<string> reportedExtensions)
    {
        if (machine.Model != MachineModel.JaguarCd)
            throw new ArgumentException(JaguarCdErrors.ModelRequired, nameof(machine));
        if (media.Category != MediaCategory.CompactDisc || media.Slot != GWGUI.Emulation.Contracts.EmulationMediaSlot.Cd0)
            throw new ArgumentException(JaguarCdErrors.CompleteDiscRequired, nameof(media));
        var extension = Path.GetExtension(media.Path);
        var normalizedExtension = extension.TrimStart(MediaConstants.ExtensionPrefix);
        if (!JaguarCdConstants.CompleteDiscExtensions.Contains(normalizedExtension)
            || !reportedExtensions.Contains(normalizedExtension))
            throw Unsupported(JaguarCdErrors.CompleteDiscRequired);
        var path = ContentFunctions.Validate(media.Path, reportedExtensions);
        ValidateReadable(path);
        var activityPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { Path.GetFullPath(path) };
        if (extension.Equals(JaguarCdConstants.CueExtension, StringComparison.OrdinalIgnoreCase))
            foreach (var track in ValidateCueTracks(path)) activityPaths.Add(track);
        return new PreparedJaguarCd(media, path,
            needsFullPath || JaguarCdConstants.RequiresFullPath, activityPaths);
    }

    internal static void RejectForStandardJaguar(
        MachineModel model,
        MediaConfiguration media)
    {
        if (model != MachineModel.JaguarCd && media.Category == MediaCategory.CompactDisc)
            throw new ArgumentException(JaguarCdErrors.ModelRequired, nameof(media));
    }

    internal static EmulationException Unsupported(string message) =>
        new(ErrorCategory.Content, ErrorCode.ContentUnsupported,
            message);

    private static IReadOnlyList<string> ValidateCueTracks(string cuePath)
    {
        var directory = Path.GetDirectoryName(cuePath) ?? string.Empty;
        List<string> tracks = [];
        foreach (var line in File.ReadLines(cuePath))
        {
            var trimmed = line.Trim();
            if (!trimmed.StartsWith(JaguarCdConstants.CueFileDirective,
                    StringComparison.OrdinalIgnoreCase)) continue;
            var firstQuote = trimmed.IndexOf(JaguarCdConstants.CueQuotedPathDelimiter);
            var lastQuote = trimmed.LastIndexOf(JaguarCdConstants.CueQuotedPathDelimiter);
            if (firstQuote == JaguarCdConstants.MissingCueDelimiterIndex || lastQuote <= firstQuote) continue;
            var trackPath = trimmed[(firstQuote + JaguarCdConstants.CueContentStartOffset)..lastQuote];
            var fullTrackPath = Path.GetFullPath(Path.Combine(directory, trackPath));
            if (!File.Exists(fullTrackPath))
                throw Unsupported(JaguarCdErrors.MissingCueTrack);
            tracks.Add(fullTrackPath);
        }
        if (tracks.Count == 0) throw Unsupported(JaguarCdErrors.EmptyCue);
        return tracks;
    }

    private static void ValidateReadable(string path)
    {
        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            throw new EmulationException(ErrorCategory.Content, ErrorCode.ContentNotFound,
                JaguarCdErrors.FileUnreadable,
                new Dictionary<string, string> { [ErrorContextConstants.Path] = path }, exception);
        }
    }
}
