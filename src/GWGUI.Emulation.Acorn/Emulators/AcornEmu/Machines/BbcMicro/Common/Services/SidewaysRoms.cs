using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Contracts;
using E = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.SidewaysExpansionConstants;
using C = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Machines.BbcMicro.Common.Constants.DiskConstants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Machines.BbcMicro.Common.Services;

/// <summary>Banques ROM latérales et verrou ROMSEL : des données externes, aucune ROM intégrée au code.</summary>
internal sealed class SidewaysRoms
{
    private readonly byte[]?[] _banks = new byte[C.RomBankCount][];
    private readonly bool[] _writable = new bool[C.RomBankCount];
    private readonly bool[] _protected = new bool[C.RomBankCount];
    private readonly int _basicBank;
    private readonly bool _broadcastToBankZero;
    private int _selected;
    internal SidewaysRoms(ReadOnlySpan<byte> basic, IReadOnlyDictionary<int, byte[]>? additional = null, SidewaysExpansionSettings? expansion = null)
    {
        if (basic.Length != C.RomBankSize) throw new ArgumentException(nameof(basic));
        expansion ??= new();
        if (expansion.Modules is < 0 or > E.SocketCount || expansion.AriesB12 && !E.AriesModels.Values.Contains(expansion.AriesRamBytes)) throw new ArgumentOutOfRangeException(nameof(expansion));
        _basicBank = expansion.MapNativeBank(C.BasicBank);
        _selected = _basicBank;
        _broadcastToBankZero = expansion.AriesB12;
        _banks[_basicBank] = basic.ToArray();
        foreach (var bank in additional ?? new Dictionary<int, byte[]>())
        {
            if (bank.Key < 0 || bank.Key >= C.RomBankCount || bank.Key == _basicBank || bank.Value.Length != C.RomBankSize) throw new ArgumentException(nameof(additional));
            _banks[bank.Key] = bank.Value.ToArray();
        }
        for (var bank = 0; bank < C.RomBankCount; bank++)
        {
            if (!expansion.IsRamBank(bank)) continue;
            if (_banks[bank] is not null) throw new ArgumentException(nameof(additional));
            _banks[bank] = new byte[expansion.RamBankBytes(bank)];
            _writable[bank] = true;
        }
    }
    internal void Select(byte value)
    {
        _selected = value & (C.RomBankCount - 1);
        if (_broadcastToBankZero || !_writable[_selected]) return;
        if ((value & E.ProtectCommandMask) == E.ProtectCommand) _protected[_selected] = true;
        else if ((value & E.ProtectCommandMask) == E.UnprotectCommand) _protected[_selected] = false;
    }

    /// <summary>Seules les banques RAM présentes et non protégées acceptent l'écriture.</summary>
    internal void Write(ushort address, byte value)
    {
        var target = _broadcastToBankZero ? 0 : _selected;
        var offset = address & (C.RomBankSize - 1);
        if (_writable[target] && !_protected[target] && offset < _banks[target]!.Length) _banks[target]![offset] = value;
    }
    internal byte Read(ushort address) => _banks[_selected] is { } data && (address & (C.RomBankSize - 1)) < data.Length ? data[address & (C.RomBankSize - 1)] : (byte)255;
    internal void Reset()
    {
        _selected = _basicBank;
        Array.Clear(_protected);
        for (var bank = 0; bank < C.RomBankCount; bank++)
            if (_writable[bank]) Array.Clear(_banks[bank]!);
    }
}
