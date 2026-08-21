namespace GWGUI.Emulation.Atari;

internal static class AtariHatariContentFunctions
{
    internal static AtariHatariContent? Prepare(AtariMachineConfiguration configuration,
        string sessionDirectory, IReadOnlySet<string> supportedExtensions)
    {
        var media = configuration.Media.Where(item => item.IsInserted)
            .OrderBy(item => item.MountOrder)
            .ThenBy(item => item.Path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (media.Length > AtariHatariContentConstants.MaximumPrimaryContentCount)
            throw new InvalidOperationException(AtariHatariContentErrors.MultiplePrimaryContentUnsupported);
        if (media.Length == AtariHatariContentConstants.FirstContentIndex) return null;
        var selected = media[AtariHatariContentConstants.FirstContentIndex];
        if (selected.Category == AtariMediaCategory.Floppy)
        {
            var sessionMedia = AtariSessionMediaFunctions.Prepare(selected, sessionDirectory, supportedExtensions);
            return new AtariHatariContent(selected, sessionMedia.RuntimePath, sessionMedia, null);
        }
        if (selected.Category is AtariMediaCategory.HardDisk or AtariMediaCategory.Directory)
        {
            var storage = AtariHatariStorageFunctions.Prepare(configuration.Model, selected, supportedExtensions);
            return new AtariHatariContent(selected, storage.RuntimePath, null, storage);
        }
        throw new InvalidDataException(AtariHatariContentErrors.ContentTypeUnsupported);
    }

    internal static void Cleanup(AtariHatariContent? content) =>
        AtariHatariStorageFunctions.Cleanup(content?.Storage);
}
