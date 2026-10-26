using C = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.Mc6854Constants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;

/// <summary>Registres et horloge de l'ADLC, indépendants du BBC, des sockets et des fichiers.</summary>
/// <remarks>Le transport remet des trames déjà délimitées et sans FCS. Les FIFO avancent avec Tick,
/// et non avec les lectures CPU. La carte propriétaire relie Irq à sa porte NMI.</remarks>
internal sealed partial class Mc6854
{
    private byte _control1, _control2, _control3, _control4, _receiveStatus;
    private int _priority, _cycle, _idleCycles;
    private byte _readReceiveStatus;
    private byte _acknowledgedReceive;
    private bool _ctsRaised, _lineIdle = true;
    private readonly int _cyclesPerByte;
    private bool _clockPresent, _clearToSend;
    internal Action<ReadOnlyMemory<byte>>? FrameTransmitted { get; set; }
    internal bool Irq => (StatusOne() & C.Interrupt) != 0;
    internal bool Receiving => _incoming is not null || _receive.Count != 0;
    internal bool Transmitting => _transmitting || _transmit.Count != 0;
    internal bool Rts => (_control2 & C.Rts) != 0;

    internal Mc6854(int cyclesPerByte = C.DefaultCyclesPerByte)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(cyclesPerByte);
        _cyclesPerByte = cyclesPerByte; Reset();
    }

    /// <summary>Réinitialise la puce sans modifier l'horloge fournie par le réseau ni son destinataire.</summary>
    internal void Reset()
    {
        _control1 = C.ReceiveReset | C.TransmitReset; _control2 = _control3 = _control4 = _receiveStatus = 0;
        _priority = _cycle = _idleCycles = 0; _readReceiveStatus = _acknowledgedReceive = 0; _ctsRaised = false; ResetReceive(); ResetTransmit();
    }

    internal void SetLine(bool clockPresent, bool clearToSend = true, bool idle = true)
    {
        if (_clockPresent && !clockPresent && Receiving) { ResetReceive(); _receiveStatus |= C.AbortReceived; }
        if (!_clockPresent && clockPresent) _receiveStatus |= C.Idle;
        _clockPresent = clockPresent; _clearToSend = clearToSend;
        _lineIdle = idle;
        if ((!clockPresent || !clearToSend) && (_control1 & C.TransmitReset) == 0) _ctsRaised = true;
        if (!idle) { _receiveStatus &= unchecked((byte)~C.Idle); _idleCycles = 0; }
    }

    internal byte Read(int register)
    {
        if ((register & 3) > 1) return ReadReceive();
        var receive = StatusTwo(); _readReceiveStatus |= receive;
        return (register & 3) == 0 ? StatusOne() : receive;
    }

    internal void Write(int register, byte value)
    {
        switch (register & 3)
        {
            case 0:
                _control1 = value;
                if ((value & C.ReceiveReset) != 0) { ResetReceive(); _receiveStatus = _readReceiveStatus = _acknowledgedReceive = 0; _priority = 0; }
                if ((value & C.TransmitReset) != 0) { ResetTransmit(); _ctsRaised = false; }
                if ((value & C.Discontinue) != 0) { ResetReceive(); _control1 &= unchecked((byte)~C.Discontinue); }
                break;
            case 1 when (_control1 & C.AddressControl) != 0: _control3 = value; break;
            case 1:
                _control2 = (byte)(value & ~(C.ClearReceive | C.ClearTransmit | C.LastData));
                if ((value & C.ClearReceive) != 0)
                {
                    var idle = (byte)(_receiveStatus & C.Idle);
                    _acknowledgedReceive |= _readReceiveStatus;
                    _receiveStatus &= (byte)~_readReceiveStatus;
                    // Une ligne toujours inactive reste lisible, mais son état acquitté ne redemande pas une NMI.
                    if (_lineIdle && !Receiving && !Transmitting) _receiveStatus |= idle;
                    _readReceiveStatus = 0; _flagDetected = false; _idleCycles = 0;
                    _priority = (value & C.Prioritize) != 0 && _priority is > 0 and < 4 ? _priority + 1 : 0;
                }
                if ((value & C.ClearTransmit) != 0) { _underrun = false; _transmitComplete = false; _ctsRaised = false; }
                if ((value & C.LastData) != 0) MarkTransmitEnd();
                break;
            case 3 when (_control1 & C.AddressControl) != 0:
                _control4 = (byte)(value & ~C.TransmitAbort);
                if ((value & C.TransmitAbort) != 0) ResetTransmit();
                break;
            default: WriteTransmit(value, (register & 3) == 3); break;
        }
    }

    internal void Tick()
    {
        if (!_clockPresent) return;
        if (_lineIdle && !Receiving && !Transmitting && (_control1 & C.ReceiveReset) == 0)
        {
            if (++_idleCycles >= _cyclesPerByte * 2) { _receiveStatus |= C.Idle; _idleCycles = 0; }
        }
        else { _idleCycles = 0; _receiveStatus &= unchecked((byte)~C.Idle); }
        if (++_cycle < _cyclesPerByte) return;
        _cycle = 0; TickTransmit(); TickReceive();
    }

    private byte StatusTwo()
    {
        var status = _receiveStatus;
        if (!_clockPresent) status |= C.NoClock;
        if ((_control1 & C.ReceiveReset) == 0)
        {
            if (_receive.Count != 0 && _receive[0].Address) status |= C.AddressPresent;
            if (_receive.Count >= ((_control2 & C.TwoByte) != 0 ? 2 : 1)) status |= C.ReceiveAvailable;
        }
        _acknowledgedReceive &= status;
        if ((_control2 & C.Prioritize) != 0)
        {
            if (_priority <= 1 && (status & C.ReceiveErrors) != 0) { _priority = 1; status &= C.ReceiveErrors; }
            else if (_priority <= 2 && (status & C.Idle) != 0) { _priority = 2; status &= C.Idle; }
            else if (_priority <= 3 && (status & C.AddressPresent) != 0) { _priority = 3; status &= C.AddressPresent; }
            else if ((status & C.ReceiveAvailable) != 0) { _priority = 4; status &= unchecked((byte)~C.FrameValid); }
            else _priority = 0;
        }
        return status;
    }

    private byte StatusOne()
    {
        var receive = StatusTwo(); var status = 0;
        if ((receive & C.ReceiveAvailable) != 0) status |= C.DataAvailable;
        if ((receive & ~C.ReceiveAvailable & ~_acknowledgedReceive) != 0) status |= C.StatusTwoRequest;
        if ((_control3 & C.Loop) != 0) status |= C.LoopStatus;
        if (_flagDetected && (_control3 & C.FlagDetectEnable) != 0) status |= C.FlagDetected;
        if (!_clockPresent || !_clearToSend || _ctsRaised) status |= C.NotClearToSend;
        if (_underrun) status |= C.TransmitUnderrun;
        if ((_control1 & C.TransmitReset) == 0 && ((_control2 & C.FrameCompleteMode) != 0 ? _transmitComplete : _transmit.Count <= ((_control2 & C.TwoByte) != 0 ? 1 : 2) && (status & C.NotClearToSend) == 0)) status |= C.TransmitAvailable;
        var rxInterrupt = (_control1 & (C.ReceiveReset | C.ReceiveInterrupt)) == C.ReceiveInterrupt &&
            ((status & (C.StatusTwoRequest | C.FlagDetected)) != 0 || (_control1 & C.ReceiveDma) == 0 && (status & C.DataAvailable) != 0);
        var txInterrupt = (_control1 & (C.TransmitReset | C.TransmitInterrupt)) == C.TransmitInterrupt &&
            ((status & (C.TransmitUnderrun | C.NotClearToSend)) != 0 || (_control1 & C.TransmitDma) == 0 && (status & C.TransmitAvailable) != 0);
        if (rxInterrupt || txInterrupt) status |= C.Interrupt;
        return (byte)status;
    }
}
