namespace GWGUI.Emulation.Enums;

/// <summary>Identifiants internes stables ; les valeurs ne sont pas les codes numériques transmis sur le réseau.</summary>
public enum EmulationNetworkTransport
{
    None = 0,
    InProcess = 1,
    Udp = 2,
    Tcp = 3,
    Ipx = 4,
    NamedPipe = 5,
    UnixSocket = 6,
    SharedMemory = 7,
    SerialPort = 8,
    Tap = 9,
    Pcap = 10,
}
