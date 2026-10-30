namespace GWGUI.Emulation.Enums;

/// <summary>Identifiants internes stables ; les valeurs ne sont pas les codes numériques transmis sur le réseau.</summary>
public enum EmulationNetworkRelayType
{
    None = 0,
    Repeater = 1,
    Hub = 2,
    Switch = 3,
    Bridge = 4,
    Router = 5,
    Gateway = 6,
    Proxy = 7,
    Nat = 8,
    PeripheralMultiplexer = 9,
}
