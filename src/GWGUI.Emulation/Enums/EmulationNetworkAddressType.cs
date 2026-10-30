namespace GWGUI.Emulation.Enums;

/// <summary>Identifiants internes stables ; les valeurs ne sont pas les codes numériques transmis sur le réseau.</summary>
public enum EmulationNetworkAddressType
{
    None = 0,
    Ipv4 = 1,
    Ipv6 = 2,
    Mac48 = 3,
    Mac64 = 4,
    EconetStation = 5,
    AppleTalkNode = 6,
    IpxNode = 7,
    DecnetNode = 8,
    X121 = 9,
    ArpanetHostImp = 10,
    ArcnetNode = 11,
    OmninetNode = 12,
    SinclairStation = 13,
    DeviceAddress = 14,
}
