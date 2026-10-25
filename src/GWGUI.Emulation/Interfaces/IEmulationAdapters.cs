using System.Reflection;

namespace GWGUI.Emulation.Interfaces;

/// <summary>Découverte des adaptateurs d’un assembly de module, sans déclaration préalable de son identité.</summary>
/// <remarks>Les instances créées appartiennent au module. Le service ne conserve ni leur assembly ni leur état.
/// Les constructeurs doivent préparer des descriptions et des fabriques, sans démarrer de cœur ou de processus.</remarks>
public interface IEmulationAdapters
{
    /// <summary>Résout le libellé du modèle qui correspond à l’identité de la configuration empruntée.</summary>
    string DisplayResourceKey(IReadOnlyList<EmulationMachineDefinition> models, IEmulationConfiguration configuration);
    /// <summary>Convertit et valide une identité dans l’enum de modèles définie par le module.</summary>
    TModel Parse<TModel>(string id) where TModel : struct, Enum;
    /// <summary>Projette l’entrée de catalogue du module vers sa définition publique avec les identités et ressources existantes.</summary>
    EmulationEmulatorDefinition ToDefinition<TEmulator, TMachineModel>(EmulationCoreCatalogEntry<TEmulator, TMachineModel> entry);

    /// <summary>Construit les types concrets fermés implémentant le contrat demandé, dans l’ordre ordinal de leur nom complet.</summary>
    /// <typeparam name="T">Contrat interne ou public choisi par le module pour ses adaptateurs.</typeparam>
    /// <param name="assembly">Assembly du module, indépendant de son nom et de son auteur.</param>
    /// <returns>Instances construites immédiatement ; les classes abstraites et les types génériques ouverts sont exclus.</returns>
    IReadOnlyList<T> Create<T>(Assembly assembly) where T : class;

    /// <summary>Sélectionne les fabriques concrètes satisfaisant simultanément les deux contrats.</summary>
    /// <typeparam name="TFactory">Type de fabrique attendu par le catalogue.</typeparam>
    /// <typeparam name="TAdapter">Contrat supplémentaire imposé aux types découverts.</typeparam>
    /// <param name="assembly">Assembly contenant les fabriques du module.</param>
    /// <returns>Séquence différée dans l’ordre de découverte des types ; le catalogue applique ensuite son tri métier.</returns>
    /// <remarks>Ce parcours conserve sa politique existante : les constructeurs s’exécutent à l’énumération,
    /// et une fabrique générique ouverte sélectionnée échoue à l’instanciation.</remarks>
    IEnumerable<TFactory> Create<TFactory, TAdapter>(Assembly assembly) where TFactory : class;
}
