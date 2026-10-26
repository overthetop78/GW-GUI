using System.Buffers.Binary;
using System.Net;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Contracts;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;

namespace GWGUI.Tests.Emulation;

/// <summary>Transactions AUN sans réseau réel : les deux sorties sont observées séparément.</summary>
public sealed class AcornEmuAunTests
{
    private static readonly EconetPeer Peer = new(0, 254, new(IPAddress.Loopback, 32768));
    private sealed class Link
    {
        internal readonly List<byte[]> Packets = [];
        internal readonly List<(byte[] Frame, double Ready)> Frames = [];
        internal readonly EconetAun Aun;
        internal Link(bool accept = true) => Aun = new(2, (_, packet) => Packets.Add(packet.ToArray()), (frame, ready) => { if (accept) Frames.Add((frame, ready)); return accept; });
    }
    private static byte[] Packet(byte type, uint handle, byte port = 0, byte control = 0, params byte[] payload)
    {
        var packet = new byte[8 + payload.Length]; packet[0] = type; packet[1] = port; packet[2] = control;
        BinaryPrimitives.WriteUInt32LittleEndian(packet.AsSpan(4), handle); payload.CopyTo(packet, 8); return packet;
    }
    private static void Start(Link link)
    {
        link.Aun.TransmitFrame([254, 0, 2, 0, 0x80, 0x99], Peer, 0);
        link.Aun.TransmitFrame([254, 0, 2, 0, 10, 20], Peer, 0.01);
    }

    [Fact]
    public void OutgoingUnicastWaitsForTheMatchingPeerAndHandle()
    {
        var link = new Link(); link.Aun.TransmitFrame([254, 0, 2, 0, 0x80, 0x99], Peer, 0);
        Assert.Empty(link.Packets); Assert.Equal(new byte[] { 2, 0, 254, 0 }, Assert.Single(link.Frames).Frame); Assert.Equal(0.0025, link.Frames[0].Ready);
        link.Aun.TransmitFrame([254, 0, 2, 0, 10, 20], Peer, 0.01);
        Assert.Equal(Packet(2, 4, 0x99, 0, 10, 20), Assert.Single(link.Packets));
        link.Aun.ReceiveDatagram(Packet(3, 8), Peer, 0.02); Assert.True(link.Aun.Busy); Assert.Single(link.Frames);
        link.Aun.ReceiveDatagram(Packet(3, 4), Peer with { Station = 253 }, 0.02); Assert.True(link.Aun.Busy);
        // Les accusés Linux AUN laissent port et contrôle à zéro : seul le handle et le correspondant les identifient.
        link.Aun.ReceiveDatagram(Packet(3, 4), Peer, 0.02); Assert.False(link.Aun.Busy); Assert.Equal(2, link.Frames.Count);
        Assert.Equal(new byte[] { 2, 0, 254, 0 }, link.Frames[1].Frame);
    }

    [Fact]
    public void NegativeAckNeverProducesAPositiveBbcAck()
    {
        var link = new Link(); Start(link); link.Aun.ReceiveDatagram(Packet(4, 4), Peer, 0.02);
        Assert.False(link.Aun.Busy); Assert.Single(link.Frames);
    }

    [Fact]
    public void IncomingUnicastIsAcknowledgedOnlyAfterTheBbcFinalAck()
    {
        var link = new Link(); var packet = Packet(2, 0x1234, 0x99, 0, 0x11, 0x22);
        link.Aun.ReceiveDatagram(packet, Peer, 0);
        Assert.Equal(new byte[] { 2, 0, 254, 0, 0x80, 0x99 }, Assert.Single(link.Frames).Frame); Assert.Empty(link.Packets);
        link.Aun.TransmitFrame([254, 0, 2, 0], Peer, 0.01);
        Assert.Equal(new byte[] { 2, 0, 254, 0, 0x11, 0x22 }, link.Frames[1].Frame); Assert.Empty(link.Packets);
        link.Aun.TransmitFrame([254, 0, 2, 0], Peer, 0.02); Assert.Equal(Packet(3, 0x1234, 0x99), Assert.Single(link.Packets)); Assert.False(link.Aun.Busy);
        link.Aun.ReceiveDatagram(packet, Peer, 0.03); Assert.Equal(2, link.Packets.Count); Assert.Equal(2, link.Frames.Count);
    }

    [Fact]
    public void OutgoingTimeoutRetriesAreBoundedAndKeepTheSameHandle()
    {
        var link = new Link(); Start(link);
        for (var index = 1; index <= 4; index++) link.Aun.Advance(0.01 + index * 0.251);
        Assert.False(link.Aun.Busy); Assert.Equal(4, link.Packets.Count); Assert.Single(link.Frames);
        for (var index = 0; index < link.Packets.Count; index++) { Assert.Equal(index, link.Packets[index][3]); Assert.Equal(4u, BinaryPrimitives.ReadUInt32LittleEndian(link.Packets[index].AsSpan(4))); }
    }

    [Fact]
    public void IncomingRefusalAndTimeoutReturnNegativeAck()
    {
        var refused = new Link(false); refused.Aun.ReceiveDatagram(Packet(2, 12, 0x99, 0, 5), Peer, 0);
        Assert.Equal(4, Assert.Single(refused.Packets)[0]); Assert.False(refused.Aun.Busy);
        var timed = new Link(); timed.Aun.ReceiveDatagram(Packet(2, 16, 0x99, 0, 5), Peer, 0); timed.Aun.Advance(0.251);
        Assert.Equal(4, Assert.Single(timed.Packets)[0]); Assert.False(timed.Aun.Busy);
    }

    [Fact]
    public void ImmediateRequestAndReplyRetainTheirPayloads()
    {
        var outgoing = new Link(); outgoing.Aun.TransmitFrame([254, 0, 2, 0, 0x81, 0, 0x44], Peer, 0);
        Assert.Equal(Packet(5, 4, 0, 1, 0x44), Assert.Single(outgoing.Packets)); Assert.Empty(outgoing.Frames);
        outgoing.Aun.ReceiveDatagram(Packet(6, 4, 0, 0, 0x55, 0x66), Peer, 0.01);
        Assert.Equal(new byte[] { 2, 0, 254, 0, 0x55, 0x66 }, Assert.Single(outgoing.Frames).Frame);
        var incoming = new Link(); incoming.Aun.ReceiveDatagram(Packet(5, 20, 0, 1, 0x44), Peer, 0);
        Assert.Equal(new byte[] { 2, 0, 254, 0, 0x81, 0, 0x44 }, Assert.Single(incoming.Frames).Frame);
        incoming.Aun.TransmitFrame([254, 0, 2, 0, 0x55, 0x66], Peer, 0.01);
        Assert.Equal(Packet(6, 20, 0, 1, 0x55, 0x66), Assert.Single(incoming.Packets));
    }

    [Fact]
    public void TruncatedLongScoutDoesNotConstructAnImmediateRequest()
    {
        var link = new Link(); link.Aun.TransmitFrame([254, 0, 2, 0, 0x85, 0], Peer, 0);
        Assert.Empty(link.Packets); Assert.Empty(link.Frames); Assert.False(link.Aun.Busy);
    }

    [Fact]
    public void BroadcastKeepsItsActualLengthAndDoesNotStartAHandshake()
    {
        var link = new Link(); link.Aun.TransmitFrame([255, 255, 2, 0, 0x80, 0x99, 1, 2, 3], Peer, 0);
        Assert.Equal(Packet(1, 4, 0x99, 0, 1, 2, 3), Assert.Single(link.Packets)); Assert.False(link.Aun.Busy);
        link.Aun.ReceiveDatagram(Packet(1, 24, 0x99, 0, 8, 9), Peer, 0.01);
        Assert.Equal(new byte[] { 255, 255, 254, 0, 0x80, 0x99, 8, 9 }, Assert.Single(link.Frames).Frame); Assert.False(link.Aun.Busy);
    }

    [Fact]
    public void LongImmediateScoutPrefixIsCarriedOnceAlongsideTheData()
    {
        var link = new Link(); link.Aun.TransmitFrame([254, 0, 2, 0, 0x82, 0, 1, 2, 3, 4, 5, 6, 7, 8], Peer, 0);
        link.Aun.TransmitFrame([254, 0, 2, 0, 9, 10], Peer, 0.01);
        Assert.Equal(Packet(2, 4, 0, 2, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10), Assert.Single(link.Packets));
        var incoming = new Link(); incoming.Aun.ReceiveDatagram(link.Packets[0], Peer, 0);
        Assert.Equal(new byte[] { 2, 0, 254, 0, 0x82, 0, 1, 2, 3, 4, 5, 6, 7, 8 }, Assert.Single(incoming.Frames).Frame);
        incoming.Aun.TransmitFrame([254, 0, 2, 0], Peer, 0.01);
        Assert.Equal(new byte[] { 2, 0, 254, 0, 9, 10 }, incoming.Frames[1].Frame);
    }

    [Fact]
    public void UnknownDestinationAndTruncatedPacketDoNotCreateAnAck()
    {
        var link = new Link(); link.Aun.TransmitFrame([254, 0, 2, 0, 0x80, 0x99], null, 0);
        link.Aun.ReceiveDatagram([2, 0], Peer, 0); link.Aun.ReceiveDatagram(Packet(2, 28, 0, 2, 1), Peer, 0);
        Assert.Empty(link.Frames); Assert.Empty(link.Packets); Assert.False(link.Aun.Busy);
    }
}
