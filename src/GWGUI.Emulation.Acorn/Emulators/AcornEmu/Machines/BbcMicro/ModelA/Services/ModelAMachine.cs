using GWGUI.Emulation.Acorn.Common.Machines.BbcMicro.Constants;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Machines.BbcMicro.Common.Services;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Interfaces;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants;
using CassetteConstants = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Machines.BbcMicro.Common.Constants.CassetteConstants;
using BbcCassetteFunctions = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Machines.BbcMicro.Common.Functions.CassetteFunctions;
using DiskConstants = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Machines.BbcMicro.Common.Constants.DiskConstants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Machines.BbcMicro.ModelA.Services;

/// <summary>Carte BBC Micro Model A : 16 ou 32 Kio physiques, ROM externes, VIA système et registres vidéo.</summary>
/// <remarks>Les composants ne connaissent pas les services de l'application. Cette carte possède leur câblage et leur horloge.
/// À 16 Kio, les adresses 4000–7FFF sont des alias de la RAM basse ; l'extension 32 Kio les rend indépendantes.
/// Le bus CPU et la vidéo partagent la même mémoire physique.</remarks>
internal sealed class ModelAMachine : ICpuBus, IAcornMachine
{
    private readonly byte[] _ram;
    private readonly byte[] _mos;
    private readonly SidewaysRoms _roms;
    private readonly Disks _disks;
    private readonly Winchester _winchester;
    private readonly Econet _econet;
    private readonly Via6522 _via = new();
    private readonly Keyboard _keyboard = new();
    private readonly Audio _audio = new();
    private readonly Acia6850 _acia = new(CassetteConstants.CpuClock, CassetteConstants.SerialClock);
    private readonly Cassette _cassette;
    private byte _latch = 255;
    private int _keyboardColumn;
    private long _ticks, _nextFrame = BbcMicroModelAConstants.CyclesPerFrame;
    private bool _break;
    internal Cpu6502 Cpu { get; }
    internal Video Video { get; }
    public bool Irq => _via.Irq || _acia.Irq || _winchester.Irq;
    public bool Nmi => _disks.Nmi || _econet.Nmi;
    public Action<ReadOnlyMemory<byte>>? NetworkFrameTransmitted { get => _econet.FrameTransmitted; set => _econet.FrameTransmitted = value; }
    public Action<int>? NetworkPoll { get; set; }
    public bool ReceiveNetworkFrame(ReadOnlySpan<byte> frame, bool validFcs = true) => _econet.Receive(frame, validFcs);
    public void SetNetworkLine(bool clockPresent, bool clearToSend = true, bool idle = true) => _econet.SetLine(clockPresent, clearToSend, idle);
    public void AttachFloppy(int index, IEmulationDiskImage? image) => _disks.Attach(index, image);
    public bool FloppyActive(int index) => _disks.Active(index);
    public void ConfigureFloppySpeed(int index, int percent) => _disks.SetSpeed(index, percent);
    public void AttachHardDisk(int unit, IEmulationDiskImage? image) => _winchester.Attach(unit, image);
    public bool HardDiskActive(int unit) => _winchester.Active(unit);
    public void ConfigureHardDisk(int unit, int latencyMilliseconds, int speedPercent, int? writeLatencyMilliseconds = null) => _winchester.Configure(unit, latencyMilliseconds, speedPercent, writeLatencyMilliseconds);
    public IEmulatorCassetteTransport CassetteTransport => _cassette;
    public void InsertCassette(EmulationCassetteSignal signal, bool writable = false) => _cassette.Insert(signal, writable);
    public bool CassetteHasChanges => _cassette.HasChanges;
    public EmulationCassetteSignal RecordedCassette => _cassette.RecordedSignal;
    public void MarkCassetteSaved() => _cassette.MarkSaved();
    public void RefreshCassetteIndex(EmulationCassetteSignal signal) => _cassette.RefreshIndex(signal);
    public void ConfigureCassette(EmulationCassetteSettings settings) => _cassette.Configure(settings);
    public void EjectCassette() => _cassette.Eject();

    internal ModelAMachine(ReadOnlySpan<byte> mos, ReadOnlySpan<byte> basic, int memoryBytes = (int)BbcMicroModelConstants.SixteenKibibytes, ReadOnlySpan<byte> characters = default, bool diskInterface = false, IReadOnlyDictionary<int, byte[]>? optionRoms = null, bool wd1770 = false, bool winchesterInterface = false, bool econetInterface = false, int econetStation = 1, int econetCyclesPerByte = Mc6854Constants.DefaultCyclesPerByte)
    {
        if (mos.Length != BbcMicroFirmwareConstants.BankSize || basic.Length != BbcMicroFirmwareConstants.BankSize) throw new ArgumentException("MOS et BASIC doivent chacun contenir 16 384 octets.");
        if (memoryBytes != BbcMicroModelConstants.SixteenKibibytes && memoryBytes != BbcMicroModelConstants.ThirtyTwoKibibytes) throw new ArgumentOutOfRangeException(nameof(memoryBytes));
        _ram = new byte[memoryBytes];
        _mos = mos.ToArray();
        _roms = new SidewaysRoms(basic, optionRoms);
        _disks = new Disks(diskInterface, wd1770);
        _winchester = new Winchester(winchesterInterface);
        _econet = new Econet(econetInterface, econetStation, econetCyclesPerByte);
        Video = new Video(_ram, characters);
        _cassette = new Cassette(_acia);
        _via.ReadPortA = () => (_latch & 8) == 0 ? _keyboard.Read(_via.OutputA) : (byte)255;
        _via.ReadPortB = () => 255;
        _via.WritePortB = WriteLatch;
        _via.WritePortA = value => { if ((_latch & 1) == 0) _audio.Write(value); };
        Cpu = new Cpu6502(this);
        Reset(true);
    }
    public void Reset(bool cold)
    {
        if (cold) { Array.Clear(_ram); _via.Reset(); Video.Reset(); _audio.Reset(); _cassette.ResetHardware(); _roms.Reset(); _latch = 255; }
        _disks.Reset();
        _winchester.Reset();
        _econet.Reset();
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
        while (_ticks < _nextFrame) { if (!BbcCassetteFunctions.TryDirectLoad(Cpu, this, _cassette)) Cpu.Step(); }
        _nextFrame += BbcMicroModelAConstants.CyclesPerFrame;
        return Video.Render();
    }
    /// <summary>La carte remet ses échantillons au cœur ; le Common possède la sortie audio physique.</summary>
    public short[] DrainAudio() => _audio.DrainSamples();
    public void Tick()
    {
        _ticks++;
        if ((_ticks & 127) == 0) NetworkPoll?.Invoke(128);
        _audio.Tick();
        _cassette.Tick();
        _disks.Tick();
        _winchester.Tick();
        _econet.Tick();
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
        if (address < 0xc000) return _roms.Read(address);
        if (address < 0xfc00 || address >= 0xff00) return _mos[address & 0x3fff];
        if (address is >= AcornScsiConstants.Start and < AcornScsiConstants.End) return _winchester.Read(address);
        if (address < 0xfe00) return 255;
        if (address < 0xfe08) return (address & 1) != 0 ? Video.ReadRegister() : (byte)0;
        if (address < 0xfe10) return _acia.Read((address & 1) != 0);
        if (address < 0xfe18) return _cassette.ReadUla();
        if (address < 0xfe20) return _econet.ReadStation();
        if (address < 0xfe28) { _econet.EnableNmi(); return 0xfe; }
        if (address is >= 0xfe40 and < 0xfe60) return _via.Read(address & 15);
        if (address is >= DiskConstants.ControllerStart and < DiskConstants.ControllerEnd) return _disks.Read(address);
        if (address is >= 0xfea0 and < 0xfec0) return _econet.Read(address);
        return 0xfe;
    }
    public void Write(ushort address, byte value)
    {
        if (address < 0x8000) { _ram[address & (_ram.Length - 1)] = value; return; }
        if (address is >= AcornScsiConstants.Start and < AcornScsiConstants.End) { _winchester.Write(address, value); return; }
        if (address is >= 0xfe00 and < 0xfe08)
        {
            if ((address & 1) == 0) Video.Select(value); else Video.WriteRegister(value);
        }
        else if (address is >= 0xfe08 and < 0xfe10) _acia.Write((address & 1) != 0, value);
        else if (address is >= 0xfe10 and < 0xfe18) _cassette.WriteUla(value);
        else if (address is >= 0xfe18 and < 0xfe20) _econet.DisableNmi();
        else if (address is >= 0xfe20 and < 0xfe30) Video.WriteUla(address, value);
        else if (address is >= DiskConstants.RomSelectStart and < DiskConstants.RomSelectEnd) _roms.Select(value);
        else if (address is >= 0xfe40 and < 0xfe60) _via.Write(address & 15, value);
        else if (address is >= DiskConstants.ControllerStart and < DiskConstants.ControllerEnd) _disks.Write(address, value);
        else if (address is >= 0xfea0 and < 0xfec0) _econet.Write(address, value);
    }
    /// <summary>PB0–PB2 désignent la sortie du verrou ; PB3 fournit le niveau à mémoriser.</summary>
    private void WriteLatch(byte value)
    {
        var previous = _latch;
        var mask = 1 << (value & 7);
        _latch = (value & 8) != 0 ? (byte)(_latch | mask) : (byte)(_latch & ~mask);
        // IC32 sortie 0 : validation active basse du PSG sur le bus lent PA0–PA7.
        if ((previous & 1) != 0 && (_latch & 1) == 0) _audio.Write(_via.OutputA);
        Video.ScreenWrap = ((_latch >> 4) & 3) switch { 0 => 0x4000, 1 => 0x6000, 2 => 0x3000, _ => 0x5800 };
    }
}
