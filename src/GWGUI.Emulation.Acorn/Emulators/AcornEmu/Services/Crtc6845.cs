namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;

/// <summary>Compteurs et registres du CRTC 6845, avancés à l'horloge des caractères.</summary>
/// <remarks>Le composant produit des adresses et des signaux ; il ne lit ni RAM ni palette.
/// Les registres d'adresse écran sont verrouillés au début de l'image suivante.</remarks>
internal sealed class Crtc6845
{
    private readonly byte[] _registers = new byte[18];
    private byte _selected;
    private int _lineAddress, _adjust, _syncLines;
    private bool _adjusting;
    internal int Column { get; private set; }
    internal int Row { get; private set; }
    internal int Raster { get; private set; }
    internal int Scanline { get; private set; }
    internal long Frame { get; private set; }
    internal int this[int index] => _registers[index];
    internal bool InterlaceVideo => (_registers[8] & 3) == 3;
    internal int RasterAddress => Raster | (InterlaceVideo ? (int)(Frame & 1) : 0);
    internal int DisplayColumn => Column - ((_registers[8] >> 4) & 3);
    internal bool DisplayEnabled => !_adjusting && Row < _registers[6] && DisplayColumn >= 0 && DisplayColumn < _registers[1] && ((_registers[8] >> 4) & 3) != 3;
    internal int Address => (_lineAddress + DisplayColumn) & 0x3fff;
    internal bool VSync => _syncLines > 0;
    internal bool HSync => Column >= _registers[2] && Column < _registers[2] + ((_registers[3] & 15) is 0 ? 16 : _registers[3] & 15);
    internal bool IsProgrammed => _registers[0] > 0 && _registers[1] > 0 && _registers[4] > 0 && _registers[6] > 0;

    internal void Select(byte value) => _selected = (byte)(value & 31);
    internal byte Read() => _selected is >= 14 and <= 17 ? _registers[_selected] : (byte)0;
    internal void Write(byte value)
    {
        if (_selected >= _registers.Length) return;
        var mask = _selected switch { 4 or 6 or 7 => 127, 5 or 9 or 11 => 31, 10 => 127, 12 or 14 or 16 => 63, _ => 255 };
        if (_selected < 16) _registers[_selected] = (byte)(value & mask);
    }

    /// <summary>Respecte position, forme, masquage et périodes de clignotement du curseur matériel.</summary>
    internal bool Cursor
    {
        get
        {
            var skew = (_registers[8] >> 6) & 3;
            return skew != 3 && DisplayEnabled && CursorForCell((_lineAddress + Column - skew) & 0x3fff);
        }
    }

    /// <summary>Position du curseur dans le flux utile, après alignement des retards des sorties CRTC et des glyphes.</summary>
    internal bool CursorForCell(int address)
    {
        if (((_registers[8] >> 6) & 3) == 3 || address != ((_registers[14] << 8) | _registers[15])) return false;
        var mode = (_registers[10] >> 5) & 3;
        if (mode == 1 || mode == 2 && (Frame & 16) != 0 || mode == 3 && (Frame & 32) != 0) return false;
        var start = _registers[10] & 31;
        var end = _registers[11] & 31;
        return start <= end ? RasterAddress >= start && RasterAddress <= end : RasterAddress >= start || RasterAddress <= end;
    }

    /// <summary>Avance un caractère ; retourne true quand une nouvelle image commence.</summary>
    internal bool Tick()
    {
        if (++Column <= _registers[0]) return false;
        Column = 0; Scanline++;
        if (_syncLines > 0) _syncLines--;
        if (_adjusting)
        {
            if (++_adjust >= _registers[5]) return NewFrame();
        }
        else if (Raster + (InterlaceVideo ? 2 : 1) <= _registers[9]) Raster += InterlaceVideo ? 2 : 1;
        else
        {
            Raster = 0;
            _lineAddress = (_lineAddress + _registers[1]) & 0x3fff;
            if (++Row > _registers[4])
            {
                if (_registers[5] == 0) return NewFrame();
                _adjusting = true; _adjust = 0;
            }
        }
        if (!_adjusting && Row == _registers[7] && Raster == 0) _syncLines = 16;
        return false;
    }

    private bool NewFrame()
    {
        Row = Raster = Scanline = _adjust = 0; _adjusting = false;
        _lineAddress = (_registers[12] << 8) | _registers[13];
        Frame++;
        if (_registers[7] == 0) _syncLines = 16;
        return true;
    }

    internal void Reset()
    {
        Array.Clear(_registers); _selected = 0;
        Column = Row = Raster = Scanline = _lineAddress = _adjust = _syncLines = 0;
        Frame = 0; _adjusting = false;
    }
}
