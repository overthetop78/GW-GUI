namespace GWGUI.Emulation.Enums;

/// <summary>Identifiants internes stables ; les valeurs ne sont pas les codes numériques transmis sur le réseau.</summary>
public enum EmulationNetworkType
{
    None = 0,
    Ethernet = 1,
    WirelessLan = 2,
    TokenRing = 3,
    TokenBus = 4,
    Arcnet = 5,
    Fddi = 6,
    LocalTalk = 7,
    Econet = 8,
    Arpanet = 9,
    Omninet = 10,
    SinclairNetwork = 11,
    SerialNetwork = 12,
    ParallelNetwork = 13,
    X25 = 14,
    Isdn = 15,
    Atm = 16,
    FrameRelay = 17,
    FibreChannel = 18,
    InfiniBand = 19,
    FireWire = 20,
    UsbNetwork = 21,
    Infrared = 22,
    BluetoothPan = 23,
    GameLink = 24,
    Midi = 25,
    SharedPeripheral = 26,
}
