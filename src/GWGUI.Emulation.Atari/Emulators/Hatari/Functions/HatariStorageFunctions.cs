using GWGUI.Emulation.Atari.Emulators.Hatari.Constants;
using GWGUI.Emulation.Atari.Emulators.Hatari.Exceptions;
using GWGUI.Emulation.Atari.Emulators.Hatari.Contracts;
using GWGUI.Emulation.Atari.Emulators.Hatari.Functions;
using GWGUI.Emulation.Atari.Emulators.Hatari.Services;

namespace GWGUI.Emulation.Atari.Emulators.Hatari.Functions;

internal static class HatariStorageFunctions
{
    internal static HatariStorage? Prepare(MachineConfiguration configuration,
        IReadOnlySet<string> supportedExtensions)
    {
        var storage = configuration.Media
            .Where(IsStorage)
            .Where(media => media.IsInserted)
            .OrderBy(media => media.MountOrder)
            .ThenBy(media => media.Path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (storage.Length > HatariStorageConstants.MaximumPrimaryStorageCount)
            throw new InvalidOperationException(HatariStorageErrors.MultiplePrimaryStorageUnsupported);
        return storage.Length == HatariStorageConstants.FirstStorageIndex
            ? null
            : Prepare(configuration.Model, storage[HatariStorageConstants.FirstStorageIndex], supportedExtensions);
    }

    internal static HatariStorage Prepare(MachineModel model, MediaConfiguration media,
        IReadOnlySet<string> supportedExtensions)
    {
        try { return PrepareImage(model, media, supportedExtensions); }
        catch (Exception error) when (error is ArgumentException or InvalidDataException)
        {
            throw new EmulationException(ErrorCategory.Content, ErrorCode.ContentUnsupported,
                error.Message, new Dictionary<string, string> { [ErrorContextConstants.Path] = media.Path }, error);
        }
    }

    private static HatariStorage PrepareImage(MachineModel model, MediaConfiguration media,
        IReadOnlySet<string> supportedExtensions)
    {
        var bus = ResolveBus(media);
        ValidateModel(model, bus);
        if (bus == StorageBus.Gemdos) return PrepareGemdos(media, supportedExtensions);
        if (!File.Exists(media.Path)) throw new FileNotFoundException(HatariStorageErrors.StorageMissing, media.Path);
        var expectedExtension = bus == StorageBus.Acsi
            ? HatariStorageConstants.AcsiExtension
            : HatariStorageConstants.IdeExtension;
        if (!string.Equals(Path.GetExtension(media.Path), expectedExtension, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException(HatariStorageErrors.StorageExtensionInvalid);
        ContentFunctions.Validate(media.Path, supportedExtensions);
        var format = HardDiskFormats.For(model).Single(item => item.Extension == expectedExtension);
        GWGUI.Emulation.HardDisks.HardDiskImageValidation.ValidateExisting(media.Path, format);
        ValidateImageAccess(media);
        return new HatariStorage(media, bus, Path.GetFullPath(media.Path),
            [new HatariStorageVolume(NormalizeMountPoint(media.MountPoint), Path.GetFullPath(media.Path),
                media.MountOrder)], false);
    }

    internal static IReadOnlyDictionary<string, string> ApplyWriteProtection(
        IReadOnlyDictionary<string, string> options, HatariStorage? storage)
    {
        var result = new Dictionary<string, string>(options, StringComparer.Ordinal);
        if (storage is not null)
            result[HatariStorageConstants.HardDriveWriteProtectionOption] = storage.Configuration.IsReadOnly
                ? HatariStorageConstants.WriteProtectionEnabled
                : HatariStorageConstants.WriteProtectionDisabled;
        return result;
    }

    internal static void Cleanup(HatariStorage? storage)
    {
        if (storage?.OwnsMarker == true && File.Exists(storage.RuntimePath)) File.Delete(storage.RuntimePath);
    }

    internal static StorageBus ResolveBus(MediaConfiguration media)
    {
        if (media.Category == MediaCategory.Directory) return StorageBus.Gemdos;
        if (media.Category != MediaCategory.HardDisk)
            throw new InvalidDataException(HatariStorageErrors.StorageTypeInvalid);
        if (media.StorageBus is { } configured) return configured;
        return Path.GetExtension(media.Path).ToLowerInvariant() switch
        {
            HatariStorageConstants.AcsiExtension => StorageBus.Acsi,
            HatariStorageConstants.IdeExtension => StorageBus.Ide,
            _ => throw new InvalidDataException(HatariStorageErrors.StorageExtensionInvalid)
        };
    }

    private static HatariStorage PrepareGemdos(MediaConfiguration media,
        IReadOnlySet<string> supportedExtensions)
    {
        if (!Directory.Exists(media.Path))
        {
            if (File.Exists(media.Path) && string.Equals(Path.GetExtension(media.Path),
                    HatariStorageConstants.GemdosMarkerExtension, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException(HatariStorageErrors.GemdosRequiresDirectory);
            throw new DirectoryNotFoundException(HatariStorageErrors.StorageMissing);
        }
        if (!supportedExtensions.Contains(HatariStorageConstants.GemdosMarkerExtension
                .TrimStart(MediaConstants.ExtensionPrefix)))
            throw new InvalidDataException(HatariStorageErrors.StorageExtensionInvalid);
        var directory = Path.GetFullPath(media.Path).TrimEnd(Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar);
        var marker = directory + HatariStorageConstants.GemdosMarkerExtension;
        if (File.Exists(marker) || Directory.Exists(marker))
            throw new IOException(HatariStorageErrors.MarkerAlreadyExists);
        var volumes = ReadGemdosVolumes(directory, media.MountPoint, media.MountOrder);
        File.WriteAllBytes(marker, []);
        return new HatariStorage(media, StorageBus.Gemdos, marker,
            volumes, true);
    }

    private static IReadOnlyList<HatariStorageVolume> ReadGemdosVolumes(
        string directory, string? configuredMountPoint, int mountOrder)
    {
        var children = Directory.EnumerateDirectories(directory).ToArray();
        var partitions = children
            .Where(path => IsPartitionName(Path.GetFileName(path)))
            .OrderBy(path => Path.GetFileName(path), StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (children.Length > HatariStorageConstants.FirstStorageIndex && partitions.Length == children.Length)
            return partitions.Select((path, index) => new HatariStorageVolume(
                Path.GetFileName(path).ToUpperInvariant(), path, mountOrder + index)).ToArray();
        return [new HatariStorageVolume(NormalizeMountPoint(configuredMountPoint), directory, mountOrder)];
    }

    private static bool IsPartitionName(string name) =>
        name.Length == HatariStorageConstants.PartitionDirectoryNameLength &&
        char.ToUpperInvariant(name[HatariStorageConstants.FirstStorageIndex]) is
            >= HatariStorageConstants.FirstGemdosPartitionLetter and
            <= HatariStorageConstants.LastGemdosPartitionLetter;

    private static string NormalizeMountPoint(string? mountPoint)
    {
        var value = string.IsNullOrWhiteSpace(mountPoint)
            ? HatariStorageConstants.DefaultGemdosMountPoint
            : mountPoint.Trim().TrimEnd(':').ToUpperInvariant();
        if (!IsPartitionName(value)) throw new ArgumentException(HatariStorageErrors.MountPointInvalid);
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
            throw new InvalidOperationException(HatariStorageErrors.StorageNotSupportedByModel);
    }
}
