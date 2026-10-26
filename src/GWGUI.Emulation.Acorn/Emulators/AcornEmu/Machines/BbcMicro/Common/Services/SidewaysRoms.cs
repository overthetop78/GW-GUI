using C = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Machines.BbcMicro.Common.Constants.DiskConstants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Machines.BbcMicro.Common.Services;

/// <summary>Banques ROM latérales et verrou ROMSEL : des données externes, aucune ROM intégrée au code.</summary>
internal sealed class SidewaysRoms
{
    private readonly byte[]?[] _banks = new byte[C.RomBankCount][];
    private int _selected = C.BasicBank;
    internal SidewaysRoms(ReadOnlySpan<byte> basic, IReadOnlyDictionary<int, byte[]>? additional = null)
    {
        if (basic.Length != C.RomBankSize) throw new ArgumentException(nameof(basic));
        _banks[C.BasicBank] = basic.ToArray();
        foreach (var bank in additional ?? new Dictionary<int, byte[]>())
        {
            if (bank.Key < 0 || bank.Key >= C.RomBankCount || bank.Key == C.BasicBank || bank.Value.Length != C.RomBankSize) throw new ArgumentException(nameof(additional));
            _banks[bank.Key] = bank.Value.ToArray();
        }
    }
    internal void Select(byte value) => _selected = value & (C.RomBankCount - 1);
    internal byte Read(ushort address) => _banks[_selected] is { } data ? data[address & (C.RomBankSize - 1)] : (byte)255;
    internal void Reset() => _selected = C.BasicBank;
}
