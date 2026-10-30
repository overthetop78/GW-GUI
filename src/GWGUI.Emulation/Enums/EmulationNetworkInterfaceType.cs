namespace GWGUI.Emulation.Enums;

/// <summary>Identifiants internes stables ; les valeurs ne sont pas les codes numériques transmis sur le réseau.</summary>
public enum EmulationNetworkInterfaceType
{
    None = 0,
    Ethernet = 1,
    Wireless = 2,
    TokenRing = 3,
    TokenBus = 4,
    Arcnet = 5,
    Fddi = 6,
    LocalTalk = 7,
    Econet = 8,
    SerialPort = 9,
    ParallelPort = 10,
    Modem = 11,
    Isdn = 12,
    X25 = 13,
    Atm = 14,
    FrameRelay = 15,
    Arpanet1822 = 16,
    Omninet = 17,
    SinclairNetwork = 18,
    FibreChannel = 19,
    InfiniBand = 20,
    FireWire = 21,
    UsbNetwork = 22,
    Infrared = 23,
    Bluetooth = 24,
    GameLink = 25,
    Midi = 26,
    SharedPeripheral = 27,
}
