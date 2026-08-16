namespace GWGUI.Emulation.Atari;

internal static class AtariHatariStorageFunctions
{
    internal static AtariHatariStorage? Prepare(AtariMachineConfiguration configuration,
        IReadOnlySet<string> supportedExtensions)
    {
        var storage = configuration.Media
            .Where(IsStorage)
            .Where(media => media.IsInserted)
            .OrderBy(media => media.MountOrder)
            .ThenBy(media => media.Path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (storage.Length > AtariHatariStorageConstants.MaximumPrimaryStorageCount)
            throw new InvalidOperationException(AtariHatariStorageErrors.MultiplePrimaryStorageUnsupported);
        return storage.Length == AtariHatariStorageConstants.FirstStorageIndex
            ? null
            : Prepare(configuration.Model, storage[AtariHatariStorageConstants.FirstStorageIndex], supportedExtensions);
    }

    internal static AtariHatariStorage Prepare(AtariMachineModel model, AtariMediaConfiguration media,
        IReadOnlySet<string> supportedExtensions)
    {
        var bus = ResolveBus(media);
        ValidateModel(model, bus);
        if (bus == AtariStorageBus.Gemdos) return PrepareGemdos(media, supportedExtensions);
        if (!File.Exists(media.Path)) throw new FileNotFoundException(AtariHatariStorageErrors.StorageMissing, media.Path);
        var expectedExtension = bus == AtariStorageBus.Acsi
            ? AtariHatariStorageConstants.AcsiExtension
            : AtariHatariStorageConstants.IdeExtension;
        if (!string.Equals(Path.GetExtension(media.Path), expectedExtension, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException(AtariHatariStorageErrors.StorageExtensionInvalid);
        Cores.AtariContentFunctions.Validate(media.Path, supportedExtensions);
        ValidateImageAccess(media);
        return new AtariHatariStorage(media, bus, Path.GetFullPath(media.Path),
            [new AtariHatariStorageVolume(NormalizeMountPoint(media.MountPoint), Path.GetFullPath(media.Path),
                media.MountOrder)], false);
    }

    internal static IReadOnlyDictionary<string, string> ApplyWriteProtection(
        IReadOnlyDictionary<string, string> options, AtariHatariStorage? storage)
    {
        var result = new Dictionary<string, string>(options, StringComparer.Ordinal);
        if (storage is not null)
            result[AtariHatariStorageConstants.HardDriveWriteProtectionOption] = storage.Configuration.IsReadOnly
                ? AtariHatariStorageConstants.WriteProtectionEnabled
                : AtariHatariStorageConstants.WriteProtectionDisabled;
        return result;
    }

    internal static void Cleanup(AtariHatariStorage? storage)
    {
        if (storage?.OwnsMarker == true && File.Exists(storage.RuntimePath)) File.Delete(storage.RuntimePath);
    }

    internal static AtariStorageBus ResolveBus(AtariMediaConfiguration media)
    {
        if (media.Kind == AtariMediaKind.Directory) return AtariStorageBus.Gemdos;
        if (media.Kind != AtariMediaKind.HardDisk)
            throw new InvalidDataException(AtariHatariStorageErrors.StorageTypeInvalid);
        if (media.StorageBus is { } configured) return configured;
        return Path.GetExtension(media.Path).ToLowerInvariant() switch
        {
            AtariHatariStorageConstants.AcsiExtension => AtariStorageBus.Acsi,
            AtariHatariStorageConstants.IdeExtension => AtariStorageBus.Ide,
            _ => throw new InvalidDataException(AtariHatariStorageErrors.StorageExtensionInvalid)
        };
    }

    private static AtariHatariStorage PrepareGemdos(AtariMediaConfiguration media,
        IReadOnlySet<string> supportedExtensions)
    {
        if (!Directory.Exists(media.Path))
        {
            if (File.Exists(media.Path) && string.Equals(Path.GetExtension(media.Path),
                    AtariHatariStorageConstants.GemdosMarkerExtension, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException(AtariHatariStorageErrors.GemdosRequiresDirectory);
            throw new DirectoryNotFoundException(AtariHatariStorageErrors.StorageMissing);
        }
        if (!supportedExtensions.Contains(AtariHatariStorageConstants.GemdosMarkerExtension
                .TrimStart(AtariConstants.ExtensionPrefix)))
            throw new InvalidDataException(AtariHatariStorageErrors.StorageExtensionInvalid);
        var directory = Path.GetFullPath(media.Path).TrimEnd(Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar);
        var marker = directory + AtariHatariStorageConstants.GemdosMarkerExtension;
        if (File.Exists(marker) || Directory.Exists(marker))
            throw new IOException(AtariHatariStorageErrors.MarkerAlreadyExists);
        var volumes = ReadGemdosVolumes(directory, media.MountPoint, media.MountOrder);
        File.WriteAllBytes(marker, []);
        return new AtariHatariStorage(media, AtariStorageBus.Gemdos, marker,
            volumes, true);
    }

    private static IReadOnlyList<AtariHatariStorageVolume> ReadGemdosVolumes(
        string directory, string? configuredMountPoint, int mountOrder)
    {
        var children = Directory.EnumerateDirectories(directory).ToArray();
        var partitions = children
            .Where(path => IsPartitionName(Path.GetFileName(path)))
            .OrderBy(path => Path.GetFileName(path), StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (children.Length > AtariHatariStorageConstants.FirstStorageIndex && partitions.Length == children.Length)
            return partitions.Select((path, index) => new AtariHatariStorageVolume(
                Path.GetFileName(path).ToUpperInvariant(), path, mountOrder + index)).ToArray();
        return [new AtariHatariStorageVolume(NormalizeMountPoint(configuredMountPoint), directory, mountOrder)];
    }

    private static bool IsPartitionName(string name) =>
        name.Length == AtariHatariStorageConstants.PartitionDirectoryNameLength &&
        char.ToUpperInvariant(name[AtariHatariStorageConstants.FirstStorageIndex]) is
            >= AtariHatariStorageConstants.FirstGemdosPartitionLetter and
            <= AtariHatariStorageConstants.LastGemdosPartitionLetter;

    private static string NormalizeMountPoint(string? mountPoint)
    {
        var value = string.IsNullOrWhiteSpace(mountPoint)
            ? AtariHatariStorageConstants.DefaultGemdosMountPoint
            : mountPoint.Trim().TrimEnd(':').ToUpperInvariant();
        if (!IsPartitionName(value)) throw new ArgumentException(AtariHatariStorageErrors.MountPointInvalid);
        return value;
    }

    private static bool IsStorage(AtariMediaConfiguration media) =>
        media.Kind is AtariMediaKind.HardDisk or AtariMediaKind.Directory;

    private static void ValidateImageAccess(AtariMediaConfiguration media)
    {
        var access = media.IsReadOnly ? FileAccess.Read : FileAccess.ReadWrite;
        using var stream = new FileStream(media.Path, FileMode.Open, access, FileShare.Read);
    }

    private static void ValidateModel(AtariMachineModel model, AtariStorageBus bus)
    {
        var required = bus switch
        {
            AtariStorageBus.Acsi => AtariStStorageCapability.Acsi,
            AtariStorageBus.Ide => AtariStStorageCapability.Ide,
            AtariStorageBus.Gemdos => AtariStStorageCapability.GemdosDirectory,
            _ => throw new ArgumentOutOfRangeException(nameof(bus))
        };
        if (!AtariStModelCatalog.Get(model).Storage.Contains(required))
            throw new InvalidOperationException(AtariHatariStorageErrors.StorageNotSupportedByModel);
    }
}
