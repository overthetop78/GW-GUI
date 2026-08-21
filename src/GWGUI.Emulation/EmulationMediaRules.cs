namespace GWGUI.Emulation;

public static class EmulationMediaRules
{
    public static bool IsCompatible(EmulationMediaSlot slot, EmulationMediaType type) => slot.Category switch
    {
        EmulationMediaCategory.FloppyDrive => type == EmulationMediaType.Floppy,
        EmulationMediaCategory.HardDisk => type == EmulationMediaType.HardDisk,
        EmulationMediaCategory.CompactDiscDrive => type == EmulationMediaType.CompactDisc,
        EmulationMediaCategory.CartridgeSlot => type == EmulationMediaType.Cartridge,
        EmulationMediaCategory.CassetteDrive => type == EmulationMediaType.Cassette,
        _ => false
    };

    public static bool SupportsEjection(EmulationMediaType type) => type is
        EmulationMediaType.Floppy or EmulationMediaType.CompactDisc or
        EmulationMediaType.Cartridge or EmulationMediaType.Cassette;

    public static bool RequiresReadOnly(EmulationMediaType type) => type is
        EmulationMediaType.CompactDisc or EmulationMediaType.Cartridge;

    public static IReadOnlyList<EmulationMedia> Validate(IReadOnlyList<EmulationMedia> media)
    {
        ArgumentNullException.ThrowIfNull(media);
        var occupied = new HashSet<EmulationMediaSlot>();

        foreach (var item in media)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(item.Path);
            if (!IsCompatible(item.Slot, item.Type))
                throw new ArgumentException($"Media type '{item.Type}' is not compatible with slot '{item.Slot}'.", nameof(media));
            if (!occupied.Add(item.Slot))
                throw new ArgumentException($"Media slot '{item.Slot}' is occupied more than once.", nameof(media));
            if (RequiresReadOnly(item.Type) && !item.IsReadOnly)
                throw new ArgumentException($"Media type '{item.Type}' must be read-only.", nameof(media));
            if (item.IsInserted is false && !SupportsEjection(item.Type))
                throw new ArgumentException($"Media type '{item.Type}' cannot remain configured while ejected.", nameof(media));
        }

        return media;
    }

    public static IReadOnlyList<EmulationMedia> Replace(
        IReadOnlyList<EmulationMedia> media,
        EmulationMedia replacement)
    {
        ArgumentNullException.ThrowIfNull(media);
        Validate([replacement]);

        var result = media.Where(item => item.Slot != replacement.Slot).Append(replacement).ToArray();
        return Validate(result);
    }

    public static IReadOnlyList<EmulationMedia> Eject(
        IReadOnlyList<EmulationMedia> media,
        EmulationMediaSlot slot)
    {
        ArgumentNullException.ThrowIfNull(media);
        var existing = media.SingleOrDefault(item => item.Slot == slot);
        if (existing is null)
            return media;
        if (!SupportsEjection(existing.Type))
            throw new InvalidOperationException($"Media type '{existing.Type}' cannot be ejected.");

        return Validate(media.Select(item => item.Slot == slot ? item with { IsInserted = false } : item).ToArray());
    }
}
