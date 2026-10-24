using GWGUI.Emulation.Example.Common.Services;
namespace GWGUI.Emulation.Example.Modules;

/// <summary>Fabrique du module Example chargée par son manifeste et son interface publique, comme les autres modules.</summary>
/// <remarks>Le build fournit systématiquement ce module avec l’application, y compris sans sélection d’émulateurs supplémentaires.</remarks>
public sealed class ExampleModuleFactory : IEmulationModuleFactory
{
    /// <summary>Identifiant invariant commun à la fabrique et à la façade du mock.</summary>
    public string Id => "example";
    /// <summary>Construit la façade ; le contexte est emprunté et le cœur ne démarre qu’au lancement d’une machine.</summary>
    public IEmulationModule Create(EmulationModuleContext context) => Engine.Create(context);
}
