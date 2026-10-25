using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;
using VideoConstants = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Machines.BbcMicro.Common.Constants.VideoConstants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Machines.BbcMicro.Common.Services;

/// <summary>Câblage vidéo BBC : bus RAM emprunté, CRTC, ULA et générateur télétexte.</summary>
/// <remarks>Chaque caractère est dessiné pendant son cycle de lecture. Une modification ultérieure de RAM,
/// de palette ou de registres ne redessine pas les pixels déjà balayés. Les images publiées restent immuables.</remarks>
internal sealed class Video
{
    private readonly byte[] _ram;
    private readonly Crtc6845 _crtc = new();
    private readonly TeletextSaa5050 _teletext;
    private readonly byte[] _palette = new byte[16];
    private byte[] _drawing = new byte[VideoConstants.Width * VideoConstants.Height * 4];
    private byte[] _completed = new byte[VideoConstants.Width * VideoConstants.Height * 4];
    private byte _control;
    private int _phase, _cursorRemaining;
    internal int ScreenWrap { get; set; } = 0x5800;
    internal bool Monochrome { get; set; }
    internal bool VSync => _crtc.VSync;
    internal long Frame => _crtc.Frame;

    /// <summary>La RAM reste possédée par la carte ; le générateur conserve sa propre copie de ROM externe.</summary>
    internal Video(byte[] ram, ReadOnlySpan<byte> characters) { _ram = ram; _teletext = new(characters); }
    internal void Reset()
    {
        _crtc.Reset(); _teletext.Reset(); Array.Clear(_palette);
        Array.Clear(_drawing); Array.Clear(_completed); _control = 0; _phase = _cursorRemaining = 0; ScreenWrap = 0x5800;
    }
    internal void Select(byte value) => _crtc.Select(value);
    internal void WriteRegister(byte value) => _crtc.Write(value);
    internal byte ReadRegister() => _crtc.Read();
    internal void WriteUla(ushort address, byte value)
    {
        if ((address & 1) == 0) _control = value;
        else _palette[value >> 4] = (byte)((value & 15) ^ 7);
    }

    /// <summary>Avance depuis l'horloge CPU de 2 MHz ; l'ULA choisit un caractère à 1 ou 2 MHz.</summary>
    internal void Tick()
    {
        _phase ^= 1;
        if ((_control & 16) == 0 && _phase != 0) return;
        if (_crtc.Column == 0) { _teletext.BeginLine(_crtc.Row); _cursorRemaining = 0; }
        if (_crtc.IsProgrammed && _crtc.DisplayEnabled && _crtc.Scanline < VideoConstants.Height)
        {
            if (_crtc.CursorForCell(_crtc.Address)) _cursorRemaining = (_control & 2) != 0 ? 1 : VideoConstants.CursorWidths[_control >> 5];
            if ((_control & 2) != 0) TeletextCharacter();
            else BitmapCharacter();
            if (_cursorRemaining > 0) _cursorRemaining--;
        }
        if (_crtc.Tick() && _crtc.IsProgrammed)
        {
            // L'image terminée est détachée avant de commencer la suivante.
            _completed = _drawing;
            _drawing = new byte[VideoConstants.Width * VideoConstants.Height * 4];
        }
    }

    /// <summary>Retourne une copie de la dernière image terminée, sans relire les registres après coup.</summary>
    internal byte[] Render() => _completed.ToArray();

    private int Color(int logical)
    {
        var physical = _palette[logical & 15];
        return (physical & 8) != 0 && (_control & 1) != 0 ? (physical ^ 7) & 7 : physical & 7;
    }

    private void BitmapCharacter()
    {
        if (_crtc.RasterAddress >= 8) return;
        var address = (_crtc.Address << 3) | _crtc.RasterAddress;
        if (address >= 0x8000) address = (address + ScreenWrap) & 0x7fff;
        var data = _ram[address & (_ram.Length - 1)];
        var count = Math.Clamp(1 << (((_control >> 2) & 3) + ((_control & 16) == 0 ? 1 : 0)), 1, 8);
        var width = (_control & 16) == 0 ? 16 : 8;
        for (var x = 0; x < width; x++)
        {
            var pixel = x * count / width;
            var logical = count == 8 ? ((data >> (7 - pixel)) & 1) * 15
                : count == 4 ? (((data >> (7 - pixel)) & 1) * 5 | ((data >> (3 - pixel)) & 1) * 10)
                : ((data >> (7 - pixel)) & 1) | (((data >> (5 - pixel)) & 1) << 1) | (((data >> (3 - pixel)) & 1) << 2) | (((data >> (1 - pixel)) & 1) << 3);
            var color = Color(logical);
            if (_cursorRemaining > 0) color ^= 7;
            Pixel(_crtc.DisplayColumn * width + x, _crtc.Scanline, color);
        }
    }

    private void TeletextCharacter()
    {
        var data = _ram[(0x7c00 | (_crtc.Address & 0x3ff)) & (_ram.Length - 1)];
        var raster = _crtc.InterlaceVideo ? _crtc.RasterAddress : _crtc.RasterAddress * 2;
        var cell = _teletext.Character(data, raster, _crtc.Frame);
        for (var x = 0; x < 16; x++)
        {
            var color = (cell.Mask & (1 << (11 - x * 12 / 16))) != 0 ? cell.Foreground : cell.Background;
            if (_cursorRemaining > 0) color ^= 7;
            Pixel(_crtc.DisplayColumn * 16 + x, _crtc.Scanline, color);
        }
    }

    private void Pixel(int x, int y, int color)
    {
        if ((uint)x >= VideoConstants.Width || (uint)y >= VideoConstants.Height) return;
        var offset = (y * VideoConstants.Width + x) * 4;
        var red = (color & 1) != 0 ? 255 : 0;
        var green = (color & 2) != 0 ? 255 : 0;
        var blue = (color & 4) != 0 ? 255 : 0;
        if (Monochrome) red = green = blue = (red * 299 + green * 587 + blue * 114) / 1000;
        _drawing[offset] = (byte)blue; _drawing[offset + 1] = (byte)green; _drawing[offset + 2] = (byte)red; _drawing[offset + 3] = 255;
    }
}
