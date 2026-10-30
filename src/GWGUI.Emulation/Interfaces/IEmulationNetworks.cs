namespace GWGUI.Emulation.Interfaces;

/// <summary>Registre de présence et de compatibilité ; ne transforme et ne réécrit aucun paquet.</summary>
public interface IEmulationNetworks
{
    long Version { get; }
    IReadOnlyList<EmulationNetworkEndpoint> Endpoints { get; }
    event EventHandler? Changed;
    void SetConfigurations(IEmulationModule module, IReadOnlyList<IEmulationConfiguration> configurations);
    IEmulationNetworkRegistration Register(EmulationNetworkEndpoint endpoint,
        Func<EmulationNetworkEndpoint, EmulationNetworkCompatibility> nativeRule);
    EmulationNetworkCompatibility Compatibility(EmulationNetworkEndpoint first, EmulationNetworkEndpoint second);
    IReadOnlyList<EmulationNetworkEndpoint> Peers(EmulationNetworkEndpoint endpoint);
}
