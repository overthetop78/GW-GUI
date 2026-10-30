namespace GWGUI.Emulation.Functions;

/// <summary>Clés de traduction des identifiants ; aucun nom d'enum n'est retourné comme libellé affichable.</summary>
public static class EmulationNetworkValueFunctions
{
    public static string ResourceKey(Enum value)
    {
        Enum normalized = value switch
        {
            EmulationNetworkInterfaceType.Wireless => EmulationNetworkType.WirelessLan,
            EmulationNetworkTransferFormat.EthernetIIFrame => EmulationNetworkProtocol.EthernetII,
            EmulationNetworkTransferFormat.Ieee802LlcFrame => EmulationNetworkProtocol.Ieee802Llc,
            EmulationNetworkTransferFormat.Ieee802SnapFrame => EmulationNetworkProtocol.Ieee802Snap,
            EmulationNetworkTransferFormat.TokenRingFrame => EmulationNetworkType.TokenRing,
            EmulationNetworkTransferFormat.TokenBusFrame => EmulationNetworkType.TokenBus,
            EmulationNetworkTransferFormat.ArcnetFrame => EmulationNetworkType.Arcnet,
            EmulationNetworkTransferFormat.FddiFrame => EmulationNetworkType.Fddi,
            EmulationNetworkTransferFormat.LocalTalkFrame => EmulationNetworkProtocol.LocalTalkLlap,
            EmulationNetworkTransferFormat.EconetFrame => EmulationNetworkProtocol.Econet,
            EmulationNetworkTransferFormat.AunPacket => EmulationNetworkProtocol.Aun,
            EmulationNetworkTransferFormat.PppFrame => EmulationNetworkProtocol.Ppp,
            EmulationNetworkTransferFormat.SlipPacket => EmulationNetworkProtocol.Slip,
            EmulationNetworkTransferFormat.HdlcFrame => EmulationNetworkProtocol.Hdlc,
            EmulationNetworkTransferFormat.LapbFrame => EmulationNetworkProtocol.Lapb,
            EmulationNetworkTransferFormat.X25Packet => EmulationNetworkProtocol.X25,
            EmulationNetworkTransferFormat.FrameRelayFrame => EmulationNetworkType.FrameRelay,
            EmulationNetworkTransferFormat.AtmCell => EmulationNetworkType.Atm,
            EmulationNetworkTransferFormat.Arpanet1822Message => EmulationNetworkInterfaceType.Arpanet1822,
            EmulationNetworkTransferFormat.OmninetFrame => EmulationNetworkType.Omninet,
            EmulationNetworkTransferFormat.SinclairNetworkFrame => EmulationNetworkType.SinclairNetwork,
            EmulationNetworkTransferFormat.Ieee80211Frame => EmulationNetworkType.WirelessLan,
            EmulationNetworkTransferFormat.FibreChannelFrame => EmulationNetworkType.FibreChannel,
            EmulationNetworkTransferFormat.InfiniBandPacket => EmulationNetworkType.InfiniBand,
            EmulationNetworkTransferFormat.GameLinkBitStream => EmulationNetworkType.GameLink,
            EmulationNetworkTransferFormat.MidiByteStream => EmulationNetworkType.Midi,
            EmulationNetworkTransferFormat.Ipv4Packet => EmulationNetworkProtocol.Ipv4,
            EmulationNetworkTransferFormat.Ipv6Packet => EmulationNetworkProtocol.Ipv6,
            EmulationNetworkAddressType.EconetStation => EmulationNetworkProtocol.Econet,
            EmulationNetworkAddressType.AppleTalkNode => EmulationNetworkProtocol.AppleTalkDdp,
            EmulationNetworkAddressType.IpxNode => EmulationNetworkProtocol.Ipx,
            EmulationNetworkAddressType.DecnetNode => EmulationNetworkProtocol.Decnet,
            EmulationNetworkAddressType.ArpanetHostImp => EmulationNetworkType.Arpanet,
            EmulationNetworkAddressType.ArcnetNode => EmulationNetworkType.Arcnet,
            EmulationNetworkAddressType.OmninetNode => EmulationNetworkType.Omninet,
            EmulationNetworkAddressType.SinclairStation => EmulationNetworkType.SinclairNetwork,
            _ => value
        };
        var name = Enum.GetName(normalized.GetType(), normalized) ?? throw new ArgumentOutOfRangeException(nameof(value));
        return string.Concat(EmulationNetworkConstants.ValueResourcePrefix, name);
    }
}
