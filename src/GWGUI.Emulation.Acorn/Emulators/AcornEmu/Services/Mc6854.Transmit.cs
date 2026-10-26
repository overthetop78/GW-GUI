using C = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.Mc6854Constants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;

/// <summary>FIFO d'émission, octet final et détection de manque de données à la cadence du réseau.</summary>
internal sealed partial class Mc6854
{
    private readonly record struct TransmitByte(byte Value, bool Last);
    private readonly List<TransmitByte> _transmit = new(C.FifoSize);
    private readonly List<byte> _outgoing = new();
    private bool _transmitting, _transmitComplete, _underrun, _finishPending;

    private void ResetTransmit()
    {
        _transmit.Clear(); _outgoing.Clear(); _transmitting = _underrun = _finishPending = false; _transmitComplete = true;
    }

    private void WriteTransmit(byte value, bool last)
    {
        if ((_control1 & C.TransmitReset) != 0) return;
        if (_transmit.Count == C.FifoSize) { _underrun = true; return; }
        var bits = 5 + ((_control4 >> 1) & 3);
        _transmit.Add(new((byte)(value & ((1 << bits) - 1)), last)); _transmitComplete = false;
    }

    private void MarkTransmitEnd()
    {
        if (_transmit.Count != 0) _transmit[^1] = _transmit[^1] with { Last = true };
        else if (_transmitting) _finishPending = true;
    }

    private void TickTransmit()
    {
        if ((_control1 & C.TransmitReset) != 0 || !_clearToSend) return;
        if (_transmit.Count == 0)
        {
            if (_finishPending) CompleteTransmission();
            else if (_transmitting) { _underrun = true; _transmitting = false; _outgoing.Clear(); }
            return;
        }
        var next = _transmit[0]; _transmit.RemoveAt(0); _transmitting = true; _outgoing.Add(next.Value);
        if (_outgoing.Count > C.MaximumFrameBytes) { _underrun = true; ResetTransmit(); _underrun = true; return; }
        if (next.Last) CompleteTransmission();
    }

    private void CompleteTransmission()
    {
        var frame = _outgoing.ToArray(); _outgoing.Clear(); _transmitting = _finishPending = false; _transmitComplete = true;
        if ((_control3 & (C.Loop | C.Test)) == C.Test) ReceiveFrame(frame);
        else FrameTransmitted?.Invoke(frame);
    }
}
