using GWGUI.Emulation.Atari.Emulators.Hatari.Constants;
using GWGUI.Emulation.Atari.Emulators.Hatari.Contracts;
using GWGUI.Emulation.Atari.Emulators.Hatari.Functions;
using GWGUI.Emulation.Atari.Emulators.Hatari.Services;

namespace GWGUI.Emulation.Atari.Emulators.Hatari.Functions;

internal static class HatariContentFunctions
{
    internal static HatariContent? Prepare(MachineConfiguration configuration,
        string sessionDirectory, IReadOnlySet<string> supportedExtensions)
    {
        var media = configuration.Media.Where(item => item.IsInserted)
            .OrderBy(item => item.MountOrder)
            .ThenBy(item => item.Path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (media.Length > HatariContentConstants.MaximumPrimaryContentCount)
        {
            var hardDisk = media.SingleOrDefault(item => item.Category == MediaCategory.HardDisk);
            var floppy = media.SingleOrDefault(item => item.Category == MediaCategory.Floppy);
            if (media.Length != 2 || hardDisk is null || floppy is null || floppy.Slot != EmulationMediaSlot.Floppy0)
                throw new InvalidOperationException(HatariContentErrors.MultiplePrimaryContentUnsupported);
            var storage = HatariStorageFunctions.Prepare(configuration.Model, hardDisk, supportedExtensions);
            var prepared = ScpMediaFunctions.Prepare(configuration, floppy, sessionDirectory, supportedExtensions);
            return new HatariContent(hardDisk, storage.RuntimePath, null, storage, prepared);
        }
        if (media.Length == HatariContentConstants.FirstContentIndex) return null;
        var selected = media[HatariContentConstants.FirstContentIndex];
        if (selected.Category == MediaCategory.Floppy)
        {
            var sessionMedia = ScpMediaFunctions.Prepare(configuration, selected, sessionDirectory,
                supportedExtensions);
            return new HatariContent(selected, sessionMedia.RuntimePath, sessionMedia, null);
        }
        if (selected.Category is MediaCategory.HardDisk or MediaCategory.Directory)
        {
            var storage = HatariStorageFunctions.Prepare(configuration.Model, selected, supportedExtensions);
            return new HatariContent(selected, storage.RuntimePath, null, storage);
        }
        throw new InvalidDataException(HatariContentErrors.ContentTypeUnsupported);
    }

    internal static void Cleanup(HatariContent? content) =>
        HatariStorageFunctions.Cleanup(content?.Storage);
}
