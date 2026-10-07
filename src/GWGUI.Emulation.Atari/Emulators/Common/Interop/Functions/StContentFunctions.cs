using System.IO;

namespace GWGUI.Emulation.Atari.Emulators.Common.Interop.Functions;

internal static class StContentFunctions
{
    internal static StContent? Prepare(MachineConfiguration configuration,
        string sessionDirectory, IReadOnlySet<string> supportedExtensions)
    {
        var media = configuration.Media.Where(item => item.IsInserted)
            .OrderBy(item => item.MountOrder)
            .ThenBy(item => item.Path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (media.Length > StContentConstants.MaximumPrimaryContentCount)
        {
            var hardDisk = media.SingleOrDefault(item => item.Category == MediaCategory.HardDisk);
            var floppy = media.SingleOrDefault(item => item.Category == MediaCategory.Floppy);
            if (media.Length != 2 || hardDisk is null || floppy is null || floppy.Slot != EmulationMediaSlot.Floppy0)
                throw new InvalidOperationException(StContentErrors.MultiplePrimaryContentUnsupported);
            var storage = StStorageFunctions.Prepare(configuration.Model, hardDisk, supportedExtensions);
            var prepared = ScpMediaFunctions.Prepare(configuration, floppy, sessionDirectory, supportedExtensions);
            return new StContent(hardDisk, storage.RuntimePath, null, storage, prepared);
        }
        if (media.Length == StContentConstants.FirstContentIndex) return null;
        var selected = media[StContentConstants.FirstContentIndex];
        if (selected.Category == MediaCategory.Floppy)
        {
            var sessionMedia = ScpMediaFunctions.Prepare(configuration, selected, sessionDirectory,
                supportedExtensions);
            return new StContent(selected, sessionMedia.RuntimePath, sessionMedia, null);
        }
        if (selected.Category is MediaCategory.HardDisk or MediaCategory.Directory)
        {
            var storage = StStorageFunctions.Prepare(configuration.Model, selected, supportedExtensions);
            return new StContent(selected, storage.RuntimePath, null, storage);
        }
        throw new InvalidDataException(StContentErrors.ContentTypeUnsupported);
    }

    internal static void Cleanup(StContent? content) =>
        StStorageFunctions.Cleanup(content?.Storage);
}
