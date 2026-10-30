namespace GWGUI.Emulation.Interfaces;

/// <summary>Projection réseau de la configuration opaque d'un module, sans ouvrir de connexion.</summary>
public interface IEmulationNetworkManager
{
    IReadOnlyList<EmulationNetworkEndpoint> DescribeNetworks(IEmulationConfiguration configuration);
}
