namespace GWGUI.Emulation.Example.Modules;

/// <summary>Fabrique intégrée chargée par son interface publique, identique à celles des modules externes.</summary>
/// <remarks>L’absence de module.json est volontaire : Example suit la version et la distribution de l’application.</remarks>
public sealed class ExampleModuleFactory : IEmulationModuleFactory
{
    /// <summary>Identifiant invariant commun à la fabrique et à la façade du mock.</summary>
    public string Id => "example";
    /// <summary>Construit la façade ; le contexte est emprunté et le cœur ne démarre qu’au lancement d’une machine.</summary>
    public IEmulationModule Create(EmulationModuleContext context) => new ExampleEmulationModule(context);
}
