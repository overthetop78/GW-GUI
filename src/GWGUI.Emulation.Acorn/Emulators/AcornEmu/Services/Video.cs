using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;

/// <summary>Registres CRTC/ULA et premier rendu BBC. La RAM et la ROM système sont empruntées à la carte.</summary>
/// <remarks>Le rendu bitmap suit l'organisation par caractères de huit octets. Le premier rendu télétexte utilise
/// les glyphes bitmap de la ROM MOS externe : les arrondis et la double hauteur SAA5050 restent un jalon distinct.</remarks>
internal sealed class Video(byte[] ram, byte[] mos)
{
    private readonly byte[] _crtc = new byte[32];
    private readonly byte[] _palette = new byte[16];
    private byte _selected, _control;
    private long _frame;
    internal int ScreenWrap { get; set; } = 0x5800;

    internal void Reset()
    {
        Array.Clear(_crtc);
        Array.Clear(_palette);
        _selected = _control = 0;
        _frame = 0;
    }
    internal void Select(byte value) => _selected = (byte)(value & 31);
    internal void WriteRegister(byte value) => _crtc[_selected] = value;
    internal byte ReadRegister() => _selected is >= 14 and <= 17 ? _crtc[_selected] : (byte)0;
    internal void WriteUla(ushort address, byte value)
    {
        if ((address & 1) == 0) _control = value;
        else _palette[value >> 4] = (byte)((value & 15) ^ 7);
    }

    /// <summary>Produit une image XRGB8888 possédée par l'appelant ; aucun tampon déjà publié n'est réécrit.</summary>
    internal byte[] Render()
    {
        var pixels = new byte[HardwareConstants.Width * HardwareConstants.Height * 4];
        if ((_control & 2) != 0) Teletext(pixels);
        else Bitmap(pixels);
        _frame++;
        return pixels;
    }
    private static void Pixel(byte[] output, int x, int y, int color)
    {
        if ((uint)x >= HardwareConstants.Width || (uint)y >= HardwareConstants.Height) return;
        var offset = (y * HardwareConstants.Width + x) * 4;
        output[offset] = (byte)((color & 4) != 0 ? 255 : 0);
        output[offset + 1] = (byte)((color & 2) != 0 ? 255 : 0);
        output[offset + 2] = (byte)((color & 1) != 0 ? 255 : 0);
        output[offset + 3] = 255;
    }
    private int Color(int logical)
    {
        var physical = _palette[logical & 15];
        return (physical & 8) != 0 && ((_frame / 25) & 1) != 0 ? (physical ^ 7) & 7 : physical & 7;
    }
    private void Bitmap(byte[] output)
    {
        var columns = Math.Clamp((int)_crtc[1], 1, 80);
        var characterHeight = Math.Clamp((_crtc[9] & 31) + 1, 1, 32);
        var start = ((_crtc[12] & 0x3f) << 11) | (_crtc[13] << 3);
        var bits = (_control >> 2) & 3;
        // Le même champ de caractères donne un nombre de pixels différent à 1 MHz et à 2 MHz.
        var count = Math.Clamp(1 << (bits + ((_control & 16) == 0 ? 1 : 0)), 1, 8);
        var scale = HardwareConstants.Width / (columns * count);
        scale = Math.Max(1, scale);
        for (var y = 0; y < HardwareConstants.Height; y++)
        {
            var row = y / characterHeight;
            var raster = y % characterHeight;
            if (row >= _crtc[6] || raster >= 8) continue;
            for (var column = 0; column < columns; column++)
            {
                var address = start + row * columns * 8 + column * 8 + raster;
                if (address >= 0x8000) address = (address + ScreenWrap) & 0x7fff;
                var data = ram[address & 0x3fff];
                for (var p = 0; p < count; p++)
                {
                    var logical = count == 8 ? ((data >> (7 - p)) & 1) * 15
                        : count == 4 ? (((data >> (7 - p)) & 1) * 5 | ((data >> (3 - p)) & 1) * 10)
                        : ((data >> (7 - p)) & 1) | (((data >> (5 - p)) & 1) << 1) | (((data >> (3 - p)) & 1) << 2) | (((data >> (1 - p)) & 1) << 3);
                    for (var dx = 0; dx < scale; dx++) Pixel(output, (column * count + p) * scale + dx, y, Color(logical));
                }
            }
        }
    }
    private void Teletext(byte[] output)
    {
        var start = (_crtc[12] << 8) | _crtc[13];
        var cursor = (_crtc[14] << 8) | _crtc[15];
        for (var row = 0; row < 25; row++)
        {
            var foreground = 7;
            var background = 0;
            var graphics = false;
            var flash = false;
            for (var column = 0; column < 40; column++)
            {
                var ma = (start + row * 40 + column) & 0x3fff;
                var value = ram[(0x7c00 | (ma & 0x3ff)) & 0x3fff] & 127;
                if (value < 32)
                {
                    if (value is >= 1 and <= 7) { foreground = value; graphics = false; }
                    if (value is >= 17 and <= 23) { foreground = value & 7; graphics = true; }
                    if (value == 8) flash = true;
                    if (value == 9) flash = false;
                    if (value == 28) background = 0;
                    if (value == 29) background = foreground;
                    value = 32;
                }
                for (var y = 0; y < 10; y++)
                    for (var x = 0; x < 8; x++)
                    {
                        var set = false;
                        if (graphics && (value & 32) != 0)
                        {
                            var block = (y < 3 ? 0 : y < 7 ? 2 : 4) + (x < 4 ? 0 : 1);
                            var bit = block == 5 ? 6 : block;
                            set = (value & (1 << bit)) != 0;
                        }
                        else if (y is >= 1 and <= 8 && value >= 32)
                            set = (mos[(value - 32) * 8 + y - 1] & (128 >> x)) != 0;
                        if (flash && (_frame / 25 & 1) != 0) set = false;
                        if (ma == cursor && y >= (_crtc[10] & 31) / 2 && y <= (_crtc[11] & 31) / 2 && (_frame & 16) == 0) set = !set;
                        var color = set ? foreground : background;
                        Pixel(output, column * 16 + x * 2, row * 10 + y, color);
                        Pixel(output, column * 16 + x * 2 + 1, row * 10 + y, color);
                    }
            }
        }
    }
}
