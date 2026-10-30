using System.Net;

namespace GWGUI.Emulation.Contracts;

/// <summary>Interface d'une machine ; les types natifs et les coordonnées hôtes sont conservés séparément.</summary>
public sealed record EmulationNetworkEndpoint(
    string ModuleId,
    Guid ConfigurationId,
    string MachineId,
    string InterfaceId,
    EmulationNetworkInterfaceType InterfaceType,
    EmulationNetworkType Network,
    IReadOnlyList<EmulationNetworkProtocol> Protocols,
    EmulationNetworkTransferFormat TransferFormat,
    EmulationNetworkTransport Transport,
    IPAddress? IpAddress = null,
    int? Port = null,
    string? HostEndpoint = null,
    IReadOnlyList<EmulationNetworkAddress>? Addresses = null,
    IReadOnlyList<EmulationNetworkRelay>? Relays = null,
    bool IsRunning = false);
