using GWGUI.Emulation.Acorn.Common.Machines.BbcMicro.Constants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;

/// <summary>Générateur télétexte SAA505x : ROM externe, attributs de ligne et mosaïques.</summary>
/// <remarks>La copie de ROM appartient au composant. Les commandes sont traitées à chaque passage de ligne,
/// avec leurs effets « à cette cellule » ou « à la cellule suivante », sans modifier la RAM écran.</remarks>
internal sealed class TeletextSaa5050
{
    private readonly byte[] _characters;
    private int _foreground, _background, _held = 32, _row = -1;
    private bool _graphics, _flash, _conceal, _separated, _hold, _double, _bottom, _rowDouble, _heldSeparated;
    internal TeletextSaa5050(ReadOnlySpan<byte> characters)
    {
        if (characters.Length != 0 && characters.Length != BbcMicroFirmwareConstants.CharacterRomSize) throw new ArgumentException(nameof(characters));
        _characters = characters.ToArray();
    }

    internal void Reset() { _row = -1; _bottom = _rowDouble = false; BeginLine(0); }
    internal void BeginLine(int row)
    {
        if (row != _row)
        {
            _bottom = row != 0 && !_bottom && _rowDouble;
            _rowDouble = false; _row = row;
        }
        _foreground = 7; _background = 0; _held = 32;
        _graphics = _flash = _conceal = _separated = _hold = _double = _heldSeparated = false;
    }

    /// <summary>Décode une cellule ; les douze bits du masque représentent une ligne de pixels télétexte.</summary>
    internal (int Mask, int Foreground, int Background) Character(int data, int raster, long frame)
    {
        data &= 127;
        var previousGraphics = _graphics;
        var previousDouble = _double;
        var previousHold = _hold;
        var foreground = _foreground;
        var separated = _separated;
        if (data < 32)
        {
            // Attributs prenant effet sur la cellule qui porte la commande.
            switch (data)
            {
                case 9: _flash = false; break;
                case 12: _double = false; break;
                case 24: _conceal = true; break;
                case 25: _separated = false; break;
                case 26: _separated = true; break;
                case 28: _background = 0; break;
                case 29: _background = _foreground; break;
                case 30: _hold = true; break;
            }
        }
        var glyph = data;
        if (data < 32)
        {
            glyph = previousGraphics && (previousHold || _hold) && _double == previousDouble ? _held : 32;
            separated = _heldSeparated;
        }
        else if (_graphics && (data & 32) != 0) { _held = data; _heldSeparated = _separated; }
        _rowDouble |= _double;
        var mask = 0;
        var fontRow = _double ? (_bottom ? 5 : 0) + raster / 4 : raster / 2;
        if ((!_bottom || _double) && !_conceal && (!_flash || frame % 64 < 48))
            mask = _graphics && (glyph & 32) != 0 ? Mosaic(glyph, fontRow, separated) : Glyph(glyph, fontRow, raster);
        var result = (mask, foreground, _background);
        if (data is >= 1 and <= 7) { _foreground = data; _graphics = _conceal = false; }
        if (data is >= 17 and <= 23) { _foreground = data & 7; _graphics = true; _conceal = false; }
        if (data == 8) _flash = true;
        if (data == 13) { _double = true; _rowDouble = true; }
        if (data == 31) _hold = false;
        if (!_graphics || previousDouble != _double) _held = 32;
        return result;
    }

    private int Glyph(int data, int row, int raster)
    {
        if (data < 32 || data > 127 || row is < 0 or >= 10 || _characters.Length == 0) return 0;
        var current = Expanded(_characters[(data - 32) * 10 + row]);
        var neighbour = row + ((raster & 1) == 0 ? -1 : 1);
        var adjacent = neighbour is >= 0 and < 10 ? Expanded(_characters[(data - 32) * 10 + neighbour]) : 0;
        return current | ((current >> 1) & adjacent & ~(adjacent >> 1)) | ((current << 1) & adjacent & ~(adjacent << 1));
    }

    private static int Expanded(int value)
    {
        var result = 0;
        for (var bit = 0; bit < 5; bit++) if ((value & (1 << bit)) != 0) result |= 3 << (bit * 2);
        return result;
    }

    private static int Mosaic(int data, int row, bool separated)
    {
        if (row is < 0 or >= 10 || separated && row is 2 or 6 or 9) return 0;
        var block = row < 3 ? 0 : row < 7 ? 2 : 4;
        return ((data & (1 << block)) != 0 ? separated ? 0xf80 : 0xfc0 : 0) | ((data & (1 << (block == 4 ? 6 : block + 1))) != 0 ? separated ? 0x03e : 0x03f : 0);
    }
}
