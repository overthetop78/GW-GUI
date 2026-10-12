using System.IO;
using System.Text;
using GWGUI.Emulation.Sinclair.Emulators.Fuse.Constants;

namespace GWGUI.Emulation.Sinclair.Emulators.Fuse.Functions;
internal static class MediaRuntimeFunctions
{
    internal static IReadOnlyList<EmulationMediaDevice> Devices(MachineConfiguration configuration)
    {
        var result = new List<EmulationMediaDevice>
        {
            new(EmulationMediaSlot.Cassette0, EmulationMediaType.Cassette, MediaRuntimeConstants.TapeExtensions, RequiresMachineRecreation: true)
        };
        var floppyExtensions = configuration.Model switch
        {
            MachineModel.SpectrumPlus3 => MediaRuntimeConstants.Plus3Extensions,
            MachineModel.SpectrumPlus3E => MediaRuntimeConstants.Plus3EExtensions,
            MachineModel.Pentagon128K or MachineModel.Pentagon512K or MachineModel.Pentagon1024K or MachineModel.Scorpion256K => MediaRuntimeConstants.TrDosExtensions,
            _ => Array.Empty<string>()};
        if (floppyExtensions.Count != 0)
            result.Add(new(EmulationMediaSlot.Floppy0, EmulationMediaType.Floppy, floppyExtensions, RequiresMachineRecreation: !CanExchange(configuration, EmulationMediaSlot.Floppy0)));
        if (configuration.Model is MachineModel.TimexTc2068 or MachineModel.TimexTs2068)
            result.Add(new(EmulationMediaSlot.Cartridge0, EmulationMediaType.Cartridge, MediaRuntimeConstants.CartridgeExtensions, RequiresMachineRecreation: true));
        return result;
    }

    internal static bool CanExchange(MachineConfiguration configuration, EmulationMediaSlot slot) => slot == EmulationMediaSlot.Floppy0 && configuration.Model is MachineModel.SpectrumPlus3 or MachineModel.Pentagon128K or MachineModel.Pentagon512K or MachineModel.Pentagon1024K or MachineModel.Scorpion256K;
    private static MediaConfiguration? InitialMedia(MachineConfiguration configuration) => configuration.Media.Where(media => media.IsInserted && media.Category != MediaCategory.Cartridge).OrderBy(media => media.Category == MediaCategory.Tape ? 0 : 1).FirstOrDefault();
    internal static LoadedContent? Prepare(MachineConfiguration configuration, string sessionDirectory, IReadOnlySet<string> supportedExtensions)
    {
        var devices = Devices(configuration);
        if (configuration.ConfiguredSlots.Any(slot => !devices.Any(device => device.Slot == slot)))
            throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(configuration));
        foreach (var media in configuration.Media)
            if (!devices.Any(device => device.Slot == media.Slot))
                throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(configuration));
        if (configuration.InitialContent is { Length: > 0 })
            return ContentFunctions.Create(PrepareInitialContent(configuration, sessionDirectory), MediaRuntimeConstants.NeedsFullPath, supportedExtensions);
        var mediaToLoad = InitialMedia(configuration);
        if (mediaToLoad is null)
            return null;
        var device = devices.Single(device => device.Slot == mediaToLoad.Slot);
        var path = SessionMediaFunctions.Prepare(mediaToLoad, sessionDirectory, device.AcceptedExtensions);
        return ContentFunctions.Create(path, MediaRuntimeConstants.NeedsFullPath, supportedExtensions);
    }

    internal static void CompleteLoad(MachineConfiguration configuration, string sessionDirectory, ExternalHostCallbacks callbacks)
    {
        var initial = configuration.InitialContent is null ? InitialMedia(configuration) : null;
        var devices = Devices(configuration);
        foreach (var media in configuration.Media.Where(media => media.IsInserted && media != initial).OrderBy(media => media.Category == MediaCategory.Cartridge ? 0 : 1))
        {
            var device = devices.Single(device => device.Slot == media.Slot);
            var path = SessionMediaFunctions.Prepare(media, sessionDirectory, device.AcceptedExtensions);
            callbacks.DiskControl.Insert(path);
        }
    }

    internal static IReadOnlyList<string> InitialContentPaths(MachineConfiguration configuration)
    {
        if (configuration.InitialContent is not { Length: > 0 } path)
            return Array.Empty<string>();
        var accepted = MediaRuntimeConstants.InitialContentExtensions.Select(extension => extension.TrimStart('.')).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var source = ContentFunctions.Validate(path, accepted);
        if (!string.Equals(Path.GetExtension(source), MediaRuntimeConstants.PlaylistExtension, StringComparison.OrdinalIgnoreCase))
            return new[]
            {
                source
            };
        if (configuration.Media.Any(media => media.IsInserted && media.Slot == EmulationMediaSlot.Floppy0))
            throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(configuration));
        var directory = Path.GetDirectoryName(source)!;
        var images = File.ReadAllLines(source).Select(line => line.Trim(' ', '\t', '\r')).Where(line => line.Length > 0 && line[0] != MediaRuntimeConstants.PlaylistComment).Select(line => Path.GetFullPath(Path.IsPathRooted(line) ? line : Path.Combine(directory, line))).ToArray();
        if (images.Length == 0 || images.Length > MediaRuntimeConstants.MaximumPlaylistImages)
            throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(configuration));
        var devices = Devices(configuration);
        foreach (var image in images)
        {
            if (!devices.Any(device => device.MediaType == EmulationMediaType.Floppy && device.AcceptedExtensions.Contains(Path.GetExtension(image), StringComparer.OrdinalIgnoreCase)))
                throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(configuration));
            if (!File.Exists(image))
                throw new FileNotFoundException(ErrorMessages.ContentFileMissing, image);
        }

        return new[]
        {
            source
        }.Concat(images).ToArray();
    }

    private static string PrepareInitialContent(MachineConfiguration configuration, string sessionDirectory)
    {
        var paths = InitialContentPaths(configuration);
        var source = paths[0];
        var directory = Path.Combine(sessionDirectory, MediaRuntimeConstants.InitialContentDirectory);
        Directory.CreateDirectory(directory);
        var target = Path.Combine(directory, MediaRuntimeConstants.InitialContentFileName + Path.GetExtension(source));
        if (Encoding.UTF8.GetByteCount(target) >= MediaRuntimeConstants.MaximumNativePathLength)
            throw new PathTooLongException(target);
        if (string.Equals(Path.GetExtension(source), MediaRuntimeConstants.PlaylistExtension, StringComparison.OrdinalIgnoreCase))
        {
            var images = new List<string>();
            for (var index = 1; index < paths.Count; index++)
            {
                var image = Path.Combine(directory, index.ToString(System.Globalization.CultureInfo.InvariantCulture) + Path.GetExtension(paths[index]));
                if (Encoding.UTF8.GetByteCount(image) >= MediaRuntimeConstants.MaximumNativePathLength)
                    throw new PathTooLongException(image);
                File.Copy(paths[index], image, overwrite: true);
                File.SetAttributes(image, FileAttributes.Normal);
                images.Add(image);
            }

            File.WriteAllLines(target, images, new UTF8Encoding(false));
        }
        else
            File.Copy(source, target, overwrite: true);
        File.SetAttributes(target, FileAttributes.Normal);
        return target;
    }
}