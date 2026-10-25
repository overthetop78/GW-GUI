using System.IO;

namespace GWGUI.Emulation.Functions;

/// <summary>Implémentation interne des règles de médias existantes, accessible par le contrat commun.</summary>
internal sealed class EmulationMediaRules : IEmulationMediaRules
{
    public byte[] Serialize(IReadOnlyList<EmulationMedia> media) => EmulationMediaProtocol.Serialize(media);
    public IReadOnlyList<EmulationMedia> Deserialize(ReadOnlySpan<byte> payload) => EmulationMediaProtocol.Deserialize(payload);

    public bool HasInvalidStorageSlots(EmulationStorageSettings settings, IReadOnlyList<EmulationMediaDevice> devices) =>
        settings.ConfiguredSlots.Distinct().Count() != settings.ConfiguredSlots.Count
        || settings.ConfiguredSlots.Any(slot => !devices.Any(device => device.Slot == slot))
        || settings.MountedMedia.Select(media => media.Slot).Distinct().Count() != settings.MountedMedia.Count;

    public bool IsInvalidMountedMedia(EmulationMedia media, IReadOnlyList<EmulationMediaDevice> devices, IReadOnlyList<EmulationMediaSlot> configuredSlots)
    {
        var device = devices.SingleOrDefault(device => device.Slot == media.Slot);
        return device is null || !configuredSlots.Contains(media.Slot) || media.Type != device.MediaType
            || !device.AcceptedExtensions.Contains(Path.GetExtension(media.Path), StringComparer.OrdinalIgnoreCase);
    }

    internal static Dictionary<EmulationMediaSlot, bool> FromLedStates(IReadOnlyDictionary<int, bool> leds) =>
        new Dictionary<EmulationMediaSlot, bool>
    {
        [EmulationMediaSlot.Floppy0] = leds.GetValueOrDefault(0),
        [EmulationMediaSlot.Cassette0] = leds.GetValueOrDefault(1),
        [EmulationMediaSlot.Cartridge0] = leds.GetValueOrDefault(2)
    };

    internal static IReadOnlyDictionary<EmulationMediaSlot, bool> FromLedStatesWithCd(IReadOnlyDictionary<int, bool> leds)
    {
        var activity = FromLedStates(leds);
        activity[EmulationMediaSlot.Cd0] = leds.GetValueOrDefault(3);
        return activity;
    }

    public bool IsCompatible(EmulationMediaSlot slot, EmulationMediaType type) => slot.Category switch
    {
        EmulationMediaCategory.FloppyDrive => type == EmulationMediaType.Floppy,
        EmulationMediaCategory.HardDisk => type == EmulationMediaType.HardDisk,
        EmulationMediaCategory.CompactDiscDrive => type == EmulationMediaType.CompactDisc,
        EmulationMediaCategory.CartridgeSlot => type == EmulationMediaType.Cartridge,
        EmulationMediaCategory.CassetteDrive => type == EmulationMediaType.Cassette,
        _ => false
    };

    public bool SupportsEjection(EmulationMediaType type) => type is EmulationMediaType.Floppy or EmulationMediaType.CompactDisc or EmulationMediaType.Cartridge or EmulationMediaType.Cassette;

    public bool RequiresReadOnly(EmulationMediaType type) => type is EmulationMediaType.CompactDisc or EmulationMediaType.Cartridge;

    public IReadOnlyList<EmulationMedia> Validate(IReadOnlyList<EmulationMedia> media)
    {
        ArgumentNullException.ThrowIfNull(media);
        var occupied = new HashSet<EmulationMediaSlot>();

        foreach (var item in media)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(item.Path);
            if (!IsCompatible(item.Slot, item.Type))
                throw new ArgumentException(string.Format(EmulationMediaErrorMessages.IncompatibleSlotFormat, item.Type, item.Slot), nameof(media));
            if (!occupied.Add(item.Slot))
                throw new ArgumentException(string.Format(EmulationMediaErrorMessages.DuplicateSlotFormat, item.Slot), nameof(media));
            if (RequiresReadOnly(item.Type) && !item.IsReadOnly)
                throw new ArgumentException(string.Format(EmulationMediaErrorMessages.ReadOnlyRequiredFormat, item.Type), nameof(media));
            if (item.IsInserted is false && !SupportsEjection(item.Type))
                throw new ArgumentException(string.Format(EmulationMediaErrorMessages.EjectedConfigurationUnsupportedFormat, item.Type), nameof(media));
        }

        return media;
    }

    public IReadOnlyList<EmulationMedia> Replace(IReadOnlyList<EmulationMedia> media, EmulationMedia replacement)
    {
        ArgumentNullException.ThrowIfNull(media);
        Validate([replacement]);

        var result = media.Where(item => item.Slot != replacement.Slot).Append(replacement).ToArray();
        return Validate(result);
    }

    public IReadOnlyList<EmulationMedia> Eject(IReadOnlyList<EmulationMedia> media, EmulationMediaSlot slot)
    {
        ArgumentNullException.ThrowIfNull(media);
        var existing = media.SingleOrDefault(item => item.Slot == slot);
        if (existing is null)
            return media;
        if (!SupportsEjection(existing.Type))
            throw new InvalidOperationException(string.Format(EmulationMediaErrorMessages.EjectionUnsupportedFormat, existing.Type));

        return Validate(media.Select(item => item.Slot == slot ? item with { IsInserted = false } : item).ToArray());
    }

    /// <inheritdoc/>
    public EmulationStorageSettings RestrictAcceptedExtensions(EmulationStorageSettings settings, EmulationMediaCategory category, IReadOnlySet<string>? extensions)
    {
        ArgumentNullException.ThrowIfNull(settings);
        if (extensions is null) return settings;
        var accepted = extensions.Select(extension => extension.TrimStart('.')).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var changed = false;
        var devices = settings.AvailableDevices.Select(device =>
        {
            if (device.Slot.Category != category) return device;
            var filtered = device.AcceptedExtensions.Where(extension => accepted.Contains(extension.TrimStart('.'))).ToArray();
            if (filtered.Length == device.AcceptedExtensions.Count) return device;
            changed = true;
            return device with { AcceptedExtensions = Array.AsReadOnly(filtered) };
        }).ToArray();
        return changed ? settings with { AvailableDevices = Array.AsReadOnly(devices) } : settings;
    }
}
