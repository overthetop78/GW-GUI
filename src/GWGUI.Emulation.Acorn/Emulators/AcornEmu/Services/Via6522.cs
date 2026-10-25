namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;

/// <summary>VIA 6522 indépendant de son câblage BBC : ports, compteurs, contrôle et interruptions.</summary>
/// <remarks>Tick représente un cycle à 1 MHz. Les fonctions des broches sont fournies par la carte propriétaire.
/// Les acquittements à la lecture restent dans le VIA pour les logiciels accédant directement aux registres.</remarks>
internal sealed class Via6522
{
    private readonly byte[] _registers = new byte[16];
    private int _timer1, _timer2, _latch1;
    private bool _timer1Running, _timer2Running, _ca2;
    private byte _flags, _enabled;
    internal Func<byte>? ReadPortA { get; set; }
    internal Func<byte>? ReadPortB { get; set; }
    internal Action<byte>? WritePortA { get; set; }
    internal Action<byte>? WritePortB { get; set; }
    internal byte OutputA => _registers[1];
    internal bool Irq => (_flags & _enabled & 0x7f) != 0;

    internal void Reset()
    {
        Array.Clear(_registers);
        _timer1 = _timer2 = _latch1 = 0xffff;
        _flags = _enabled = 0;
        _timer1Running = _timer2Running = _ca2 = false;
    }

    /// <summary>Événement de synchronisation verticale raccordé à CA1 sur le BBC.</summary>
    internal void VSync() => _flags |= 2;
    /// <summary>Niveau de détection clavier sur CA2 ; le front actif dépend du PCR.</summary>
    internal void SetKeyboardLine(bool level)
    {
        if (level != _ca2 && level == ((_registers[12] & 4) != 0)) _flags |= 1;
        _ca2 = level;
    }
    internal void Tick()
    {
        if (_timer1Running && --_timer1 < 0)
        {
            _flags |= 0x40;
            if ((_registers[11] & 0x40) != 0) _timer1 = _latch1 + 1;
            else { _timer1Running = false; _timer1 = 0xffff; }
        }
        if (_timer2Running && (_registers[11] & 0x20) == 0 && --_timer2 < 0)
        {
            _flags |= 0x20; _timer2Running = false; _timer2 = 0xffff;
        }
    }
    internal byte Read(int register)
    {
        register &= 15;
        switch (register)
        {
            case 0: _flags &= 0xe7; return (byte)((_registers[0] & _registers[2]) | ((ReadPortB?.Invoke() ?? 0xff) & ~_registers[2]));
            case 1: _flags &= 0xfc; goto case 15;
            case 15: return (byte)((_registers[1] & _registers[3]) | ((ReadPortA?.Invoke() ?? 0xff) & ~_registers[3]));
            case 4: _flags &= 0xbf; return (byte)_timer1;
            case 5: return (byte)(_timer1 >> 8);
            case 6: return (byte)_latch1;
            case 7: return (byte)(_latch1 >> 8);
            case 8: _flags &= 0xdf; return (byte)_timer2;
            case 9: return (byte)(_timer2 >> 8);
            case 13: return (byte)(_flags | (Irq ? 128 : 0));
            case 14: return (byte)(_enabled | 128);
            default: return _registers[register];
        }
    }
    internal void Write(int register, byte value)
    {
        register &= 15;
        switch (register)
        {
            case 0: _registers[0] = value; _flags &= 0xe7; WritePortB?.Invoke(value); break;
            case 1: _flags &= 0xfc; goto case 15;
            case 15: _registers[1] = value; WritePortA?.Invoke(value); break;
            case 4: case 6: _latch1 = (_latch1 & 0xff00) | value; break;
            case 5: _latch1 = (_latch1 & 255) | value << 8; _timer1 = _latch1; _timer1Running = true; _flags &= 0xbf; break;
            case 7: _latch1 = (_latch1 & 255) | value << 8; _flags &= 0xbf; break;
            case 8: _registers[8] = value; break;
            case 9: _timer2 = _registers[8] | value << 8; _timer2Running = true; _flags &= 0xdf; break;
            case 13: _flags &= (byte)~value; break;
            case 14: _enabled = (value & 128) != 0 ? (byte)(_enabled | (value & 127)) : (byte)(_enabled & ~value); break;
            default: _registers[register] = value; break;
        }
    }
}
