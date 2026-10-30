namespace GWGUI.Emulation.Interfaces;

/// <summary>La session possède cette publication et la libère dans son finally de décharge ou d'échec.</summary>
public interface IEmulationNetworkRegistration : IDisposable
{
    /// <summary>Publie les coordonnées réellement apprises pendant l'exécution, sans changer l'identité de l'interface.</summary>
    void Update(EmulationNetworkEndpoint endpoint);
}
