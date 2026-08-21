namespace GWGUI.Emulation;

public readonly record struct EmulationMediaSlot(EmulationMediaCategory Category, int Index)
    : IComparable<EmulationMediaSlot>
{
    public static EmulationMediaSlot Floppy0 { get; } = new(EmulationMediaCategory.FloppyDrive, 0);
    public static EmulationMediaSlot Floppy1 { get; } = new(EmulationMediaCategory.FloppyDrive, 1);
    public static EmulationMediaSlot Floppy2 { get; } = new(EmulationMediaCategory.FloppyDrive, 2);
    public static EmulationMediaSlot Floppy3 { get; } = new(EmulationMediaCategory.FloppyDrive, 3);
    public static EmulationMediaSlot HardDisk0 { get; } = new(EmulationMediaCategory.HardDisk, 0);
    public static EmulationMediaSlot Cd0 { get; } = new(EmulationMediaCategory.CompactDiscDrive, 0);
    public static EmulationMediaSlot Cartridge0 { get; } = new(EmulationMediaCategory.CartridgeSlot, 0);
    public static EmulationMediaSlot Cassette0 { get; } = new(EmulationMediaCategory.CassetteDrive, 0);

    public int ProtocolValue => Category switch
    {
        EmulationMediaCategory.FloppyDrive when Index is >= 0 and <= 3 => Index,
        EmulationMediaCategory.HardDisk when Index == 0 => 4,
        EmulationMediaCategory.CompactDiscDrive when Index == 0 => 5,
        EmulationMediaCategory.CartridgeSlot when Index == 0 => 6,
        EmulationMediaCategory.CassetteDrive when Index == 0 => 7,
        _ => throw new InvalidOperationException(EmulationMediaSlotConstants.MissingProtocolValueMessage)
    };

    public static EmulationMediaSlot FromProtocolValue(int value) => value switch
    {
        0 => Floppy0,
        1 => Floppy1,
        2 => Floppy2,
        3 => Floppy3,
        4 => HardDisk0,
        5 => Cd0,
        6 => Cartridge0,
        7 => Cassette0,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };

    public static bool TryParse(string? value, out EmulationMediaSlot slot)
    {
        slot = default;
        if (string.IsNullOrWhiteSpace(value)) return false;
        foreach (var item in EmulationMediaSlotDictionaries.Prefixes)
        {
            if (!value.StartsWith(item.Key, StringComparison.OrdinalIgnoreCase)
                || !int.TryParse(value.AsSpan(item.Key.Length), out var index) || index < 0) continue;
            slot = new EmulationMediaSlot(item.Value, index);
            return true;
        }
        return false;
    }

    public int CompareTo(EmulationMediaSlot other)
    {
        var categoryComparison = Category.CompareTo(other.Category);
        return categoryComparison != 0 ? categoryComparison : Index.CompareTo(other.Index);
    }

    public override string ToString() => Category switch
    {
        EmulationMediaCategory.FloppyDrive => $"Floppy{Index}",
        EmulationMediaCategory.HardDisk => $"HardDisk{Index}",
        EmulationMediaCategory.CompactDiscDrive => $"Cd{Index}",
        EmulationMediaCategory.CartridgeSlot => $"Cartridge{Index}",
        EmulationMediaCategory.CassetteDrive => $"Cassette{Index}",
        _ => $"{Category}{Index}"
    };
}
