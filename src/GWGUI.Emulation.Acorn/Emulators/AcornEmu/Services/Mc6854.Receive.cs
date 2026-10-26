using C = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.Mc6854Constants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;

/// <summary>Réception cadencée, repères d'adresse et de fin de trame, sans tamponner au-delà de la FIFO matérielle.</summary>
internal sealed partial class Mc6854
{
    private readonly record struct ReceiveByte(byte Value, bool Address, bool Last, bool Valid);
    private readonly List<ReceiveByte> _receive = new(C.FifoSize);
    private byte[]? _incoming;
    private int _incomingOffset;
    private bool _incomingValid, _flagDetected;

    /// <summary>Prend une copie de la trame ; refuse une réception pendant reset ou avant acquittement de la précédente.</summary>
    internal bool ReceiveFrame(ReadOnlySpan<byte> frame, bool validFcs = true)
    {
        if (frame.Length is 0 or > C.MaximumFrameBytes || !_clockPresent || (_control1 & C.ReceiveReset) != 0 || Receiving || (_receiveStatus & C.ReceiveErrors) != 0) return false;
        _incoming = frame.ToArray(); _incomingOffset = 0; _incomingValid = validFcs;
        _flagDetected = true; _receiveStatus &= unchecked((byte)~C.Idle); _priority = 0;
        return true;
    }

    private void ResetReceive() { _receive.Clear(); _incoming = null; _incomingOffset = 0; _flagDetected = false; }

    private byte ReadReceive()
    {
        if ((_control1 & C.ReceiveReset) != 0 || _receive.Count == 0) return 0;
        var result = _receive[0].Value; _receive.RemoveAt(0); ObserveReceiveFront();
        return result;
    }

    private void ObserveReceiveFront()
    {
        if (_receive.Count != 0 && _receive[0].Last) _receiveStatus |= _receive[0].Valid ? C.FrameValid : C.FcsError;
    }

    private void TickReceive()
    {
        if (_incoming is null || (_control1 & C.ReceiveReset) != 0) return;
        if (_receive.Count == C.FifoSize)
        {
            _receiveStatus |= C.ReceiveOverrun; _incoming = null; return;
        }
        var bits = 5 + ((_control4 >> 3) & 3);
        _receive.Add(new((byte)(_incoming[_incomingOffset] & ((1 << bits) - 1)), _incomingOffset == 0, _incomingOffset == _incoming.Length - 1, _incomingValid));
        if (++_incomingOffset == _incoming.Length) _incoming = null;
        ObserveReceiveFront();
    }
}
