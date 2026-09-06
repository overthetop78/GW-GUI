namespace GWGUI.Emulation.Atari.Functions;

internal static class AtariJaguarCdFunctions
{
    internal static bool IsSupported(IReadOnlySet<string> reportedExtensions) =>
        AtariJaguarCdConstants.CompleteDiscExtensions.Any(reportedExtensions.Contains);

    internal static AtariPreparedJaguarCd Prepare(
        AtariMachineConfiguration machine,
        AtariMediaConfiguration media,
        bool needsFullPath,
        IReadOnlySet<string> reportedExtensions)
    {
        if (machine.Model != AtariMachineModel.JaguarCd)
            throw new ArgumentException(AtariJaguarCdErrors.ModelRequired, nameof(machine));
        if (media.Category != AtariMediaCategory.CompactDisc || media.Slot != GWGUI.Emulation.Contracts.EmulationMediaSlot.Cd0)
            throw new ArgumentException(AtariJaguarCdErrors.CompleteDiscRequired, nameof(media));
        var extension = Path.GetExtension(media.Path);
        var normalizedExtension = extension.TrimStart(AtariConstants.ExtensionPrefix);
        if (!AtariJaguarCdConstants.CompleteDiscExtensions.Contains(normalizedExtension)
            || !reportedExtensions.Contains(normalizedExtension))
            throw Unsupported(AtariJaguarCdErrors.CompleteDiscRequired);
        var path = AtariContentFunctions.Validate(media.Path, reportedExtensions);
        ValidateReadable(path);
        var activityPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { Path.GetFullPath(path) };
        if (extension.Equals(AtariJaguarCdConstants.CueExtension, StringComparison.OrdinalIgnoreCase))
            foreach (var track in ValidateCueTracks(path)) activityPaths.Add(track);
        return new AtariPreparedJaguarCd(media, path,
            needsFullPath || AtariJaguarCdConstants.RequiresFullPath, activityPaths);
    }

    internal static void RejectForStandardJaguar(
        AtariMachineModel model,
        AtariMediaConfiguration media)
    {
        if (model != AtariMachineModel.JaguarCd && media.Category == AtariMediaCategory.CompactDisc)
            throw new ArgumentException(AtariJaguarCdErrors.ModelRequired, nameof(media));
    }

    internal static AtariEmulationException Unsupported(string message) =>
        new(AtariErrorCategory.Content, AtariErrorCode.ContentUnsupported,
            message);

    private static IReadOnlyList<string> ValidateCueTracks(string cuePath)
    {
        var directory = Path.GetDirectoryName(cuePath) ?? string.Empty;
        List<string> tracks = [];
        foreach (var line in File.ReadLines(cuePath))
        {
            var trimmed = line.Trim();
            if (!trimmed.StartsWith(AtariJaguarCdConstants.CueFileDirective,
                    StringComparison.OrdinalIgnoreCase)) continue;
            var firstQuote = trimmed.IndexOf(AtariJaguarCdConstants.CueQuotedPathDelimiter);
            var lastQuote = trimmed.LastIndexOf(AtariJaguarCdConstants.CueQuotedPathDelimiter);
            if (firstQuote == AtariJaguarCdConstants.MissingCueDelimiterIndex || lastQuote <= firstQuote) continue;
            var trackPath = trimmed[(firstQuote + AtariJaguarCdConstants.CueContentStartOffset)..lastQuote];
            var fullTrackPath = Path.GetFullPath(Path.Combine(directory, trackPath));
            if (!File.Exists(fullTrackPath))
                throw Unsupported(AtariJaguarCdErrors.MissingCueTrack);
            tracks.Add(fullTrackPath);
        }
        if (tracks.Count == 0) throw Unsupported(AtariJaguarCdErrors.EmptyCue);
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
            throw new AtariEmulationException(AtariErrorCategory.Content, AtariErrorCode.ContentNotFound,
                AtariJaguarCdErrors.FileUnreadable,
                new Dictionary<string, string> { [AtariConstants.PathContextKey] = path }, exception);
        }
    }
}
