using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;
using C = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.Mc6854Constants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Machines.BbcMicro.Common.Services;

/// <summary>Câblage BBC de l'ADLC : station, registres répétés et verrou de validation NMI.</summary>
/// <remarks>Le transport est emprunté par ses callbacks ; le cœur conserve sa possession et sa fermeture.</remarks>
internal sealed class Econet
{
    private readonly bool _enabled;
    private readonly byte _station;
    private readonly Mc6854 _controller;
    private bool _nmiEnabled, _driving;
    private bool _clockPresent, _clearToSend = true, _idle = true;
    internal bool Nmi => _enabled && _nmiEnabled && _controller.Irq;
    internal Action<ReadOnlyMemory<byte>>? FrameTransmitted { get => _controller.FrameTransmitted; set => _controller.FrameTransmitted = value; }

    internal Econet(bool enabled, int station, int cyclesPerByte = C.DefaultCyclesPerByte)
    {
        if (station is < 1 or > 254) throw new ArgumentOutOfRangeException(nameof(station));
        _enabled = enabled; _station = (byte)station; _controller = new(cyclesPerByte);
    }
    internal void Reset() { _nmiEnabled = _driving = false; _controller.Reset(); UpdateLine(); }
    internal void Tick()
    {
        if (!_enabled) return;
        _controller.Tick();
        if (_driving && !_controller.Rts && !_controller.Transmitting) UpdateLine();
    }
    internal void SetLine(bool clockPresent, bool clearToSend, bool idle)
    {
        _clockPresent = clockPresent; _clearToSend = clearToSend; _idle = idle; UpdateLine();
    }
    // La carte BBC renvoie CTS haut lorsque RTS n'active pas son émetteur ; NFS utilise ce retour pour sonder l'horloge.
    private void UpdateLine()
    {
        if (!_enabled) return;
        if (_controller.Rts) _driving = true;
        else if (!_controller.Transmitting) _driving = false;
        _controller.SetLine(_clockPresent, _clearToSend && _driving, _idle);
    }
    internal bool Receive(ReadOnlySpan<byte> frame, bool validFcs) => _enabled && _controller.ReceiveFrame(frame, validFcs);

    /// <summary>FE18 masque la seule NMI réseau ; la lecture retourne les liens de station.</summary>
    internal byte ReadStation() { _nmiEnabled = false; return _enabled ? _station : (byte)0xfe; }
    internal void DisableNmi() => _nmiEnabled = false;
    internal void EnableNmi() { if (_enabled) _nmiEnabled = true; }
    internal byte Read(ushort address) => _enabled ? _controller.Read(address & 3) : (byte)0xfe;
    internal void Write(ushort address, byte value) { if (_enabled) { _controller.Write(address & 3, value); UpdateLine(); } }
}
