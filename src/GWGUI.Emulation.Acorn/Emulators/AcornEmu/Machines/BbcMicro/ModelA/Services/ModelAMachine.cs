using GWGUI.Emulation.Acorn.Common.Machines.BbcMicro.Constants;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Machines.BbcMicro.Common.Services;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Interfaces;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Machines.BbcMicro.ModelA.Services;

/// <summary>Carte BBC Micro Model A : 16 ou 32 Kio physiques, ROM externes, VIA système et registres vidéo.</summary>
/// <remarks>Les composants ne connaissent pas les services de l'application. Cette carte possède leur câblage et leur horloge.
/// À 16 Kio, les adresses 4000–7FFF sont des alias de la RAM basse ; l'extension 32 Kio les rend indépendantes.
/// Le bus CPU et la vidéo partagent la même mémoire physique.</remarks>
internal sealed class ModelAMachine : ICpuBus, IAcornMachine
{
    private readonly byte[] _ram;
    private readonly byte[] _mos, _basic;
    private readonly Via6522 _via = new();
    private readonly Keyboard _keyboard = new();
    private byte _latch = 255;
    private int _keyboardColumn;
    private long _ticks, _nextFrame = BbcMicroModelAConstants.CyclesPerFrame;
    private bool _break;
    internal Cpu6502 Cpu { get; }
    internal Video Video { get; }
    public bool Irq => _via.Irq;

    internal ModelAMachine(ReadOnlySpan<byte> mos, ReadOnlySpan<byte> basic, int memoryBytes = (int)BbcMicroModelConstants.SixteenKibibytes, ReadOnlySpan<byte> characters = default)
    {
        if (mos.Length != BbcMicroFirmwareConstants.BankSize || basic.Length != BbcMicroFirmwareConstants.BankSize) throw new ArgumentException("MOS et BASIC doivent chacun contenir 16 384 octets.");
        if (memoryBytes != BbcMicroModelConstants.SixteenKibibytes && memoryBytes != BbcMicroModelConstants.ThirtyTwoKibibytes) throw new ArgumentOutOfRangeException(nameof(memoryBytes));
        _ram = new byte[memoryBytes];
        _mos = mos.ToArray();
        _basic = basic.ToArray();
        Video = new Video(_ram, characters);
        _via.ReadPortA = () => (_latch & 8) == 0 ? _keyboard.Read(_via.OutputA) : (byte)255;
        _via.ReadPortB = () => 255;
        _via.WritePortB = WriteLatch;
        Cpu = new Cpu6502(this);
        Reset(true);
    }
    public void Reset(bool cold)
    {
        if (cold) { Array.Clear(_ram); _via.Reset(); Video.Reset(); _latch = 255; }
        Cpu.Reset();
    }
    public void SetInput(EmulationInputSnapshot snapshot)
    {
        _keyboard.SetInput(snapshot.Keys, snapshot.HostCharacters);
        if (_keyboard.BreakPressed && !_break) Reset(false);
        _break = _keyboard.BreakPressed;
    }
    /// <summary>Change le raccordement vidéo sans réinitialiser les registres, la RAM ou le programme exécuté.</summary>
    public void SetOption(string key, string value)
    {
        if (key != EmulatorConstants.VideoOutput || !EmulatorConstants.VideoOutputs.Contains(value, StringComparer.Ordinal)) throw new ArgumentOutOfRangeException(nameof(key));
        Video.Monochrome = value == EmulatorConstants.MonochromeOutput;
    }
    public byte[] RunFrame()
    {
        while (_ticks < _nextFrame) Cpu.Step();
        _nextFrame += BbcMicroModelAConstants.CyclesPerFrame;
        return Video.Render();
    }
    public void Tick()
    {
        _ticks++;
        var sync = Video.VSync;
        Video.Tick();
        if (!sync && Video.VSync) _via.VSync();
        if ((_ticks & 1) == 0)
        {
            _via.Tick();
            if ((_latch & 8) != 0) _keyboardColumn = (_keyboardColumn + 1) & 15;
            else _keyboardColumn = _via.OutputA & 15;
            _via.SetKeyboardLine(_keyboard.ColumnPressed(_keyboardColumn));
        }
    }
    public byte Read(ushort address)
    {
        if (address < 0x8000) return _ram[address & (_ram.Length - 1)];
        if (address < 0xc000) return _basic[address & 0x3fff];
        if (address < 0xfc00 || address >= 0xff00) return _mos[address & 0x3fff];
        if (address < 0xfe00) return 255;
        if (address < 0xfe08) return (address & 1) != 0 ? Video.ReadRegister() : (byte)0;
        if (address < 0xfe10) return (address & 1) == 0 ? (byte)2 : (byte)0;
        if (address is >= 0xfe40 and < 0xfe60) return _via.Read(address & 15);
        return 0xfe;
    }
    public void Write(ushort address, byte value)
    {
        if (address < 0x8000) { _ram[address & (_ram.Length - 1)] = value; return; }
        if (address is >= 0xfe00 and < 0xfe08)
        {
            if ((address & 1) == 0) Video.Select(value); else Video.WriteRegister(value);
        }
        else if (address is >= 0xfe20 and < 0xfe30) Video.WriteUla(address, value);
        else if (address is >= 0xfe40 and < 0xfe60) _via.Write(address & 15, value);
    }
    /// <summary>PB0–PB2 désignent la sortie du verrou ; PB3 fournit le niveau à mémoriser.</summary>
    private void WriteLatch(byte value)
    {
        var mask = 1 << (value & 7);
        _latch = (value & 8) != 0 ? (byte)(_latch | mask) : (byte)(_latch & ~mask);
        Video.ScreenWrap = ((_latch >> 4) & 3) switch { 0 => 0x4000, 1 => 0x6000, 2 => 0x3000, _ => 0x5800 };
    }
}
