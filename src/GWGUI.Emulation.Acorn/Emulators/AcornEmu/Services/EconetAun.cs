using System.Buffers.Binary;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Contracts;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;

/// <summary>Adaptation des échanges Econet en quatre trames au protocole UDP AUN, sans socket possédée.</summary>
/// <remarks>Les échéances sont des secondes émulées. Les callbacks empruntés envoient les datagrammes
/// et mettent les trames BBC en attente ; aucune réponse négative AUN ne devient un accusé BBC positif.</remarks>
internal sealed class EconetAun(byte station, Action<EconetPeer, byte[]> send, Func<byte[], double, bool> enqueue)
{
    private enum Stage { Idle, OutgoingData, OutgoingAck, OutgoingImmediate, IncomingScoutAck, IncomingFinalAck, IncomingImmediate }
    private enum Kind : byte { Broadcast = 1, Unicast, Ack, NegativeAck, Immediate, ImmediateReply }
    private const double Timeout = 0.25, AckDelay = 0.0025;
    private const int MaximumRetries = 3, HeaderLength = 8;
    private Stage _stage;
    private EconetPeer? _peer;
    private byte[] _header = [], _scout = [], _payload = [], _packet = [];
    private uint _sequence;
    private double _deadline;
    private int _retries;
    private readonly List<(EconetPeer Peer, uint Handle, byte[] Reply, double Expires)> _completed = [];
    internal bool Busy => _stage != Stage.Idle;

    internal void Reset() { _stage = Stage.Idle; _peer = null; _header = _scout = _payload = _packet = []; _retries = 0; _deadline = 0; }
    private static uint Handle(ReadOnlySpan<byte> header) => BinaryPrimitives.ReadUInt32LittleEndian(header.Slice(4, 4));
    private static bool SamePeer(EconetPeer left, EconetPeer right) => left.Station == right.Station && left.Network == right.Network && left.EndPoint.Equals(right.EndPoint);
    private bool Matches(EconetPeer peer, ReadOnlySpan<byte> frame) => _peer is not null && SamePeer(_peer, peer) && frame.Length >= 4 && frame[0] == peer.Station && frame[2] == station;
    private static int LongScoutBytes(byte port, byte control) => port != 0 ? 0 : (control & 127) switch { 2 => 8, >= 3 and <= 5 => 4, _ => 0 };
    private byte[] FromPeer(EconetPeer peer, ReadOnlySpan<byte> payload)
    {
        var frame = new byte[4 + payload.Length]; frame[0] = station; frame[2] = peer.Station; frame[3] = peer.Network; payload.CopyTo(frame.AsSpan(4)); return frame;
    }
    private static byte[] Packet(ReadOnlySpan<byte> header, Kind kind, ReadOnlySpan<byte> payload)
    {
        var packet = new byte[HeaderLength + payload.Length]; header.CopyTo(packet); packet[0] = (byte)kind; payload.CopyTo(packet.AsSpan(HeaderLength)); return packet;
    }
    private void Complete(EconetPeer peer, byte[] reply, double now)
    {
        _completed.RemoveAll(item => item.Expires <= now);
        if (_completed.Count == 16) _completed.RemoveAt(0);
        _completed.Add((peer, Handle(reply), reply, now + 2)); send(peer, reply); Reset();
    }

    /// <summary>Reçoit une trame produite par le BBC ; une destination absente n'est jamais acquittée artificiellement.</summary>
    internal void TransmitFrame(ReadOnlySpan<byte> frame, EconetPeer? peer, double now)
    {
        Advance(now);
        if (peer is null || frame.Length < 4 || frame[2] != station) return;
        if (_stage == Stage.OutgoingData && Matches(peer, frame))
        {
            if (frame.SequenceEqual(_scout)) { enqueue(FromPeer(peer, []), now + AckDelay); _deadline = now + Timeout; return; }
            var prefix = _scout.AsSpan(6); var payload = new byte[prefix.Length + frame.Length - 4]; prefix.CopyTo(payload); frame[4..].CopyTo(payload.AsSpan(prefix.Length));
            _packet = Packet(_header, Kind.Unicast, payload); _stage = Stage.OutgoingAck; _retries = 0; _deadline = now + Timeout; send(peer, _packet); return;
        }
        if (_stage == Stage.IncomingScoutAck && Matches(peer, frame) && frame.Length == 4)
        {
            var skip = LongScoutBytes(_header[1], _header[2]);
            if (enqueue(FromPeer(peer, _payload.AsSpan(skip)), now + AckDelay)) { _stage = Stage.IncomingFinalAck; _deadline = now + Timeout; }
            return;
        }
        if (_stage == Stage.IncomingFinalAck && Matches(peer, frame) && frame.Length == 4) { Complete(peer, Packet(_header, Kind.Ack, []), now); return; }
        if (_stage == Stage.IncomingImmediate && Matches(peer, frame)) { Complete(peer, Packet(_header, Kind.ImmediateReply, frame[4..]), now); return; }
        if (_stage != Stage.Idle || frame.Length < 6) return;
        _peer = peer; _scout = frame.ToArray(); _header = new byte[HeaderLength]; _header[1] = frame[5]; _header[2] = (byte)(frame[4] & 127);
        _sequence = unchecked(_sequence + 4); BinaryPrimitives.WriteUInt32LittleEndian(_header.AsSpan(4), _sequence);
        if (frame[0] == 255) { send(peer, Packet(_header, Kind.Broadcast, frame[6..])); Reset(); return; }
        if (frame[5] == 0 && LongScoutBytes(frame[5], frame[4]) == 0)
        {
            _packet = Packet(_header, Kind.Immediate, frame[6..]); _stage = Stage.OutgoingImmediate; _deadline = now + Timeout; send(peer, _packet); return;
        }
        if (frame.Length - 6 != LongScoutBytes(frame[5], frame[4])) { Reset(); return; }
        if (enqueue(FromPeer(peer, []), now + AckDelay)) { _stage = Stage.OutgoingData; _deadline = now + Timeout; }
        else Reset();
    }

    /// <summary>Valide type, correspondant et identifiant avant de remettre les données au BBC.</summary>
    internal void ReceiveDatagram(ReadOnlySpan<byte> packet, EconetPeer peer, double now)
    {
        Advance(now);
        if (packet.Length < HeaderLength || packet[0] is < 1 or > 6) return;
        var kind = (Kind)packet[0]; var handle = Handle(packet);
        if (kind is Kind.Ack or Kind.NegativeAck or Kind.ImmediateReply)
        {
            if (_peer is null || !SamePeer(_peer, peer) || handle != Handle(_header)) return;
            if (_stage == Stage.OutgoingAck && kind == Kind.Ack) { enqueue(FromPeer(peer, []), now); Reset(); }
            else if (_stage == Stage.OutgoingImmediate && kind == Kind.ImmediateReply) { enqueue(FromPeer(peer, packet[HeaderLength..]), now); Reset(); }
            else if (kind == Kind.NegativeAck && _stage is Stage.OutgoingAck or Stage.OutgoingImmediate) Reset();
            return;
        }
        if (kind != Kind.Broadcast)
        {
            var previous = _completed.Find(item => item.Expires > now && SamePeer(item.Peer, peer) && item.Handle == handle);
            if (previous.Reply is not null) { send(peer, previous.Reply); return; }
            if (Busy)
            {
                if (_peer is null || !SamePeer(_peer, peer) || Handle(packet) != Handle(_header)) send(peer, Packet(packet[..HeaderLength], Kind.NegativeAck, []));
                return;
            }
        }
        var data = packet[HeaderLength..]; var extra = kind == Kind.Unicast ? LongScoutBytes(packet[1], packet[2]) : data.Length;
        if (extra > data.Length) return;
        var scout = FromPeer(peer, new byte[2 + extra]); scout[4] = (byte)(packet[2] | 128); scout[5] = packet[1]; data[..extra].CopyTo(scout.AsSpan(6));
        if (kind == Kind.Broadcast) { scout[0] = scout[1] = 255; enqueue(scout, now); return; }
        if (!enqueue(scout, now)) { send(peer, Packet(packet[..HeaderLength], Kind.NegativeAck, [])); return; }
        _header = packet[..HeaderLength].ToArray(); _payload = data.ToArray(); _peer = peer;
        _stage = kind == Kind.Immediate ? Stage.IncomingImmediate : Stage.IncomingScoutAck; _deadline = now + Timeout;
    }

    internal void Advance(double now)
    {
        _completed.RemoveAll(item => item.Expires <= now);
        if (!Busy || now < _deadline) return;
        if (_stage is Stage.OutgoingAck or Stage.OutgoingImmediate && _retries < MaximumRetries)
        {
            _packet[3] = (byte)++_retries; send(_peer!, _packet); _deadline = now + Timeout; return;
        }
        if (_stage is Stage.IncomingScoutAck or Stage.IncomingFinalAck) send(_peer!, Packet(_header, Kind.NegativeAck, []));
        Reset();
    }
}
