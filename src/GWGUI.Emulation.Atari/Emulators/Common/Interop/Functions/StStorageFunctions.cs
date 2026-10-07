using System.IO;

namespace GWGUI.Emulation.Atari.Emulators.Common.Interop.Functions;

internal static class StStorageFunctions
{
    internal static StStorage? Prepare(MachineConfiguration configuration,
        IReadOnlySet<string> supportedExtensions)
    {
        var storage = configuration.Media
            .Where(IsStorage)
            .Where(media => media.IsInserted)
            .OrderBy(media => media.MountOrder)
            .ThenBy(media => media.Path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (storage.Length > StStorageConstants.MaximumPrimaryStorageCount)
            throw new InvalidOperationException(StStorageErrors.MultiplePrimaryStorageUnsupported);
        return storage.Length == StStorageConstants.FirstStorageIndex
            ? null
            : Prepare(configuration.Model, storage[StStorageConstants.FirstStorageIndex], supportedExtensions);
    }

    internal static StStorage Prepare(MachineModel model, MediaConfiguration media,
        IReadOnlySet<string> supportedExtensions)
    {
        try { return PrepareImage(model, media, supportedExtensions); }
        catch (Exception error) when (error is ArgumentException or InvalidDataException)
        {
            throw new EmulationException(ErrorCategory.Content, ErrorCode.ContentUnsupported,
                error.Message, new Dictionary<string, string> { [ErrorContextConstants.Path] = media.Path }, error);
        }
    }

    private static StStorage PrepareImage(MachineModel model, MediaConfiguration media,
        IReadOnlySet<string> supportedExtensions)
    {
        var bus = ResolveBus(media);
        ValidateModel(model, bus);
        if (bus == StorageBus.Gemdos) return PrepareGemdos(media, supportedExtensions);
        if (!File.Exists(media.Path)) throw new FileNotFoundException(StStorageErrors.StorageMissing, media.Path);
        var expectedExtension = bus == StorageBus.Acsi
            ? StStorageConstants.AcsiExtension
            : StStorageConstants.IdeExtension;
        if (!string.Equals(Path.GetExtension(media.Path), expectedExtension, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException(StStorageErrors.StorageExtensionInvalid);
        ContentFunctions.Validate(media.Path, supportedExtensions);
        var format = HardDiskFormats.For(model).Single(item => item.Extension == expectedExtension);
        GWGUI.Emulation.HardDisks.HardDiskImageValidation.ValidateExisting(media.Path, format);
        ValidateImageAccess(media);
        return new StStorage(media, bus, Path.GetFullPath(media.Path),
            [new StStorageVolume(NormalizeMountPoint(media.MountPoint), Path.GetFullPath(media.Path),
                media.MountOrder)], false);
    }

    internal static void Cleanup(StStorage? storage)
    {
        if (storage?.OwnsMarker == true && File.Exists(storage.RuntimePath)) File.Delete(storage.RuntimePath);
    }

    internal static StorageBus ResolveBus(MediaConfiguration media)
    {
        if (media.Category == MediaCategory.Directory) return StorageBus.Gemdos;
        if (media.Category != MediaCategory.HardDisk)
            throw new InvalidDataException(StStorageErrors.StorageTypeInvalid);
        if (media.StorageBus is { } configured) return configured;
        return Path.GetExtension(media.Path).ToLowerInvariant() switch
        {
            StStorageConstants.AcsiExtension => StorageBus.Acsi,
            StStorageConstants.IdeExtension => StorageBus.Ide,
            _ => throw new InvalidDataException(StStorageErrors.StorageExtensionInvalid)
        };
    }

    private static StStorage PrepareGemdos(MediaConfiguration media,
        IReadOnlySet<string> supportedExtensions)
    {
        if (!Directory.Exists(media.Path))
        {
            if (File.Exists(media.Path) && string.Equals(Path.GetExtension(media.Path),
                    StStorageConstants.GemdosMarkerExtension, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException(StStorageErrors.GemdosRequiresDirectory);
            throw new DirectoryNotFoundException(StStorageErrors.StorageMissing);
        }
        if (!supportedExtensions.Contains(StStorageConstants.GemdosMarkerExtension
                .TrimStart(MediaConstants.ExtensionPrefix)))
            throw new InvalidDataException(StStorageErrors.StorageExtensionInvalid);
        var directory = Path.GetFullPath(media.Path).TrimEnd(Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar);
        var marker = directory + StStorageConstants.GemdosMarkerExtension;
        if (File.Exists(marker) || Directory.Exists(marker))
            throw new IOException(StStorageErrors.MarkerAlreadyExists);
        var volumes = ReadGemdosVolumes(directory, media.MountPoint, media.MountOrder);
        File.WriteAllBytes(marker, []);
        return new StStorage(media, StorageBus.Gemdos, marker,
            volumes, true);
    }

    private static IReadOnlyList<StStorageVolume> ReadGemdosVolumes(
        string directory, string? configuredMountPoint, int mountOrder)
    {
        var children = Directory.EnumerateDirectories(directory).ToArray();
        var partitions = children
            .Where(path => IsPartitionName(Path.GetFileName(path)))
            .OrderBy(path => Path.GetFileName(path), StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (children.Length > StStorageConstants.FirstStorageIndex && partitions.Length == children.Length)
            return partitions.Select((path, index) => new StStorageVolume(
                Path.GetFileName(path).ToUpperInvariant(), path, mountOrder + index)).ToArray();
        return [new StStorageVolume(NormalizeMountPoint(configuredMountPoint), directory, mountOrder)];
    }

    private static bool IsPartitionName(string name) =>
        name.Length == StStorageConstants.PartitionDirectoryNameLength &&
        char.ToUpperInvariant(name[StStorageConstants.FirstStorageIndex]) is
            >= StStorageConstants.FirstGemdosPartitionLetter and
            <= StStorageConstants.LastGemdosPartitionLetter;

    private static string NormalizeMountPoint(string? mountPoint)
    {
        var value = string.IsNullOrWhiteSpace(mountPoint)
            ? StStorageConstants.DefaultGemdosMountPoint
            : mountPoint.Trim().TrimEnd(':').ToUpperInvariant();
        if (!IsPartitionName(value)) throw new ArgumentException(StStorageErrors.MountPointInvalid);
        return value;
    }

    private static bool IsStorage(MediaConfiguration media) =>
        media.Category is MediaCategory.HardDisk or MediaCategory.Directory;

    private static void ValidateImageAccess(MediaConfiguration media)
    {
        var access = media.IsReadOnly ? FileAccess.Read : FileAccess.ReadWrite;
        using var stream = new FileStream(media.Path, FileMode.Open, access, FileShare.Read);
    }

    private static void ValidateModel(MachineModel model, StorageBus bus)
    {
        var required = bus switch
        {
            StorageBus.Acsi => StStorageCapability.Acsi,
            StorageBus.Ide => StStorageCapability.Ide,
            StorageBus.Gemdos => StStorageCapability.GemdosDirectory,
            _ => throw new ArgumentOutOfRangeException(nameof(bus))
        };
        if (!StModelCatalog.Get(model).Storage.Contains(required))
            throw new InvalidOperationException(StStorageErrors.StorageNotSupportedByModel);
    }
}
