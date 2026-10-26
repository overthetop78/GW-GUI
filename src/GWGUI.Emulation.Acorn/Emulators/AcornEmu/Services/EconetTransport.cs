using System.Net;
using System.Net.Sockets;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Contracts;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Interfaces;
using GWGUI.Emulation.Acorn.Common.Machines.BbcMicro.Constants;
using C = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.NetworkConstants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;

/// <summary>Possède une socket UDP non bloquante ; le cœur avance ce transport sur son propre thread.</summary>
internal sealed class EconetTransport : IDisposable
{
    private readonly Socket _socket;
    private readonly EconetSettings _settings;
    private readonly IAcornMachine _machine;
    private readonly EconetAun? _aun;
    private readonly byte[] _buffer = new byte[65535];
    private readonly Queue<(byte[] Frame, double Ready, double Expires)> _pending = new();
    private long _cycles;
    private double _flagFillUntil;
    private bool _disposed;
    private double Now => (double)_cycles / BbcMicroModelConstants.CpuFrequencyHz;

    internal EconetTransport(EconetSettings settings, IAcornMachine machine)
    {
        _settings = settings; _machine = machine;
        if (settings.Mode == C.Aun) _aun = new(settings.Station, Send, Enqueue);
        Socket? socket = null;
        try
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.ExclusiveAddressUse = true;
            socket.Bind(settings.LocalEndPoint); socket.Blocking = false; _socket = socket; socket = null;
        }
        finally { socket?.Dispose(); }
    }

    private bool Enqueue(byte[] frame, double ready)
    {
        if (_pending.Count >= 16) return false;
        _pending.Enqueue((frame, ready, ready + 0.25)); return true;
    }

    private void Send(EconetPeer peer, byte[] packet)
    {
        if (packet.Length > 65507) return;
        try { _socket.SendTo(packet, SocketFlags.None, peer.EndPoint); }
        catch (SocketException error) when (error.SocketErrorCode is SocketError.WouldBlock or SocketError.ConnectionReset or SocketError.ConnectionRefused) { }
    }

    internal void Transmit(ReadOnlyMemory<byte> memory)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var frame = memory.Span;
        if (frame.Length < 4 || frame[2] != _settings.Station) return;
        var destinations = frame[0] == 255 ? _settings.Peers : _settings.Peers.Where(peer => peer.Station == memory.Span[0] && peer.Network == memory.Span[1]).ToArray();
        foreach (var peer in destinations.DistinctBy(peer => peer.EndPoint))
            if (_aun is null) Send(peer, frame.ToArray()); else _aun.TransmitFrame(frame, peer, Now);
        if (_aun is null) _flagFillUntil = Now + 0.25;
        UpdateLine();
    }
    private void UpdateLine() => _machine.SetNetworkLine(true, true, _pending.Count == 0 && (_aun is null ? Now >= _flagFillUntil : !_aun.Busy));

    /// <summary>Appelé tous les 128 cycles BBC, y compris pendant l'attente active d'un accusé par la ROM.</summary>
    internal void Advance(int cycles)
    {
        ObjectDisposedException.ThrowIf(_disposed, this); _cycles += cycles; _aun?.Advance(Now);
        for (var count = 0; count < 32 && _socket.Poll(0, SelectMode.SelectRead); count++)
        {
            EndPoint source = new IPEndPoint(IPAddress.Any, 0); int size;
            try { size = _socket.ReceiveFrom(_buffer, SocketFlags.None, ref source); }
            catch (SocketException error) when (error.SocketErrorCode is SocketError.WouldBlock or SocketError.ConnectionReset or SocketError.ConnectionRefused or SocketError.MessageSize) { continue; }
            var packet = _buffer.AsSpan(0, size);
            var sourceStation = size >= 4 ? packet[2] : -1;
            var peer = _settings.Peers.FirstOrDefault(item => item.EndPoint.Equals(source) && (_aun is not null || item.Station == sourceStation));
            if (peer is null) continue;
            if (_aun is not null) _aun.ReceiveDatagram(packet, peer, Now);
            else if (size >= 4 && packet[0] is var destination && (destination == _settings.Station || destination == 255) && packet[2] == peer.Station && (packet[3] == peer.Network || packet[3] == 0)) { Enqueue(packet.ToArray(), Now); _flagFillUntil = Now + 0.25; }
        }
        while (_pending.TryPeek(out var item))
        {
            if (Now >= item.Expires) { _pending.Dequeue(); continue; }
            if (Now < item.Ready || !_machine.ReceiveNetworkFrame(item.Frame)) break;
            _pending.Dequeue(); break;
        }
        UpdateLine();
    }

    internal void Reset() { _aun?.Reset(); _pending.Clear(); }
    public void Dispose()
    {
        if (_disposed) return;
        try { _socket.Dispose(); }
        finally { _disposed = true; _pending.Clear(); _aun?.Reset(); }
    }
}
