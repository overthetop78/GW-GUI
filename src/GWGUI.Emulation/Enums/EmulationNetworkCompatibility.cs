namespace GWGUI.Emulation.Enums;

/// <summary>Identifiants internes stables ; les valeurs ne sont pas les codes numériques transmis sur le réseau.</summary>
public enum EmulationNetworkCompatibility
{
    NotEvaluated = 0,
    Compatible = 1,
    SameMachine = 2,
    MachineOffline = 3,
    InterfaceDisconnected = 4,
    NetworkMismatch = 5,
    FormatMismatch = 6,
    TransportMismatch = 7,
    NativeParametersMismatch = 8,
    AddressConflict = 9,
    InterfaceMismatch = 10,
}
