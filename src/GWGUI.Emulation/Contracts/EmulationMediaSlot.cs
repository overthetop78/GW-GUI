namespace GWGUI.Emulation.Contracts;

public readonly record struct EmulationMediaSlot(EmulationMediaCategory Category, int Index)
    : IComparable<EmulationMediaSlot>
{
    public static EmulationMediaSlot Floppy0 { get; } = new(EmulationMediaCategory.FloppyDrive, EmulationMediaSlotConstants.FirstIndex);
    public static EmulationMediaSlot Floppy1 { get; } = new(EmulationMediaCategory.FloppyDrive, EmulationMediaSlotConstants.SecondIndex);
    public static EmulationMediaSlot Floppy2 { get; } = new(EmulationMediaCategory.FloppyDrive, EmulationMediaSlotConstants.ThirdIndex);
    public static EmulationMediaSlot Floppy3 { get; } = new(EmulationMediaCategory.FloppyDrive, EmulationMediaSlotConstants.FourthIndex);
    public static EmulationMediaSlot HardDisk0 { get; } = new(EmulationMediaCategory.HardDisk, EmulationMediaSlotConstants.FirstIndex);
    public static EmulationMediaSlot Cd0 { get; } = new(EmulationMediaCategory.CompactDiscDrive, EmulationMediaSlotConstants.FirstIndex);
    public static EmulationMediaSlot Cartridge0 { get; } = new(EmulationMediaCategory.CartridgeSlot, EmulationMediaSlotConstants.FirstIndex);
    public static EmulationMediaSlot Cassette0 { get; } = new(EmulationMediaCategory.CassetteDrive, EmulationMediaSlotConstants.FirstIndex);

    public int ProtocolValue => Category switch
    {
        EmulationMediaCategory.FloppyDrive when Index is >= EmulationMediaSlotConstants.FirstIndex
            and <= EmulationMediaSlotConstants.FourthIndex => Index,
        EmulationMediaCategory.HardDisk when Index == EmulationMediaSlotConstants.FirstIndex => EmulationMediaSlotConstants.HardDiskProtocolValue,
        EmulationMediaCategory.CompactDiscDrive when Index == EmulationMediaSlotConstants.FirstIndex => EmulationMediaSlotConstants.CompactDiscProtocolValue,
        EmulationMediaCategory.CartridgeSlot when Index == EmulationMediaSlotConstants.FirstIndex => EmulationMediaSlotConstants.CartridgeProtocolValue,
        EmulationMediaCategory.CassetteDrive when Index == EmulationMediaSlotConstants.FirstIndex => EmulationMediaSlotConstants.CassetteProtocolValue,
        _ => throw new InvalidOperationException(EmulationMediaSlotConstants.MissingProtocolValueMessage)
    };

    public static EmulationMediaSlot FromProtocolValue(int value) => value switch
    {
        EmulationMediaSlotConstants.FirstIndex => Floppy0,
        EmulationMediaSlotConstants.SecondIndex => Floppy1,
        EmulationMediaSlotConstants.ThirdIndex => Floppy2,
        EmulationMediaSlotConstants.FourthIndex => Floppy3,
        EmulationMediaSlotConstants.HardDiskProtocolValue => HardDisk0,
        EmulationMediaSlotConstants.CompactDiscProtocolValue => Cd0,
        EmulationMediaSlotConstants.CartridgeProtocolValue => Cartridge0,
        EmulationMediaSlotConstants.CassetteProtocolValue => Cassette0,
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
        EmulationMediaCategory.FloppyDrive => $"{EmulationMediaSlotConstants.FloppyPrefix}{Index}",
        EmulationMediaCategory.HardDisk => $"{EmulationMediaSlotConstants.HardDiskPrefix}{Index}",
        EmulationMediaCategory.CompactDiscDrive => $"{EmulationMediaSlotConstants.CompactDiscPrefix}{Index}",
        EmulationMediaCategory.CartridgeSlot => $"{EmulationMediaSlotConstants.CartridgePrefix}{Index}",
        EmulationMediaCategory.CassetteDrive => $"{EmulationMediaSlotConstants.CassettePrefix}{Index}",
        _ => $"{Category}{Index}"
    };
}
