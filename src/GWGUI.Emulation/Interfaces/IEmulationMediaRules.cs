namespace GWGUI.Emulation.Interfaces;

/// <summary>Règles communes des montages et documents de médias, sans ouverture des contenus référencés.</summary>
/// <remarks>Common décrit les lecteurs disponibles et leurs extensions. Les collections reçues sont empruntées ; les opérations conservent les comparaisons, contrôles et erreurs existants et ne possèdent aucun fichier utilisateur.</remarks>
public interface IEmulationMediaRules
{
    /// <summary>Détecte les emplacements configurés en double, inconnus et les montages en double.</summary>
    bool HasInvalidStorageSlots(EmulationStorageSettings settings, IReadOnlyList<EmulationMediaDevice> devices);
    /// <summary>Contrôle le lecteur, le type, sa configuration et l’extension du montage proposé.</summary>
    bool IsInvalidMountedMedia(EmulationMedia media, IReadOnlyList<EmulationMediaDevice> devices, IReadOnlyList<EmulationMediaSlot> configuredSlots);
    /// <summary>Indique si la catégorie de l’emplacement accepte le type selon les règles communes existantes.</summary>
    bool IsCompatible(EmulationMediaSlot slot, EmulationMediaType type);
    /// <summary>Indique si le type permet une éjection selon les règles communes.</summary>
    bool SupportsEjection(EmulationMediaType type);
    /// <summary>Indique si le type exige un montage en lecture seule.</summary>
    bool RequiresReadOnly(EmulationMediaType type);
    /// <summary>Valide les chemins, emplacements, types, doublons et contraintes puis retourne la collection reçue.</summary>
    IReadOnlyList<EmulationMedia> Validate(IReadOnlyList<EmulationMedia> media);
    /// <summary>Remplace le montage de son emplacement puis valide la collection produite, sans modifier la source.</summary>
    IReadOnlyList<EmulationMedia> Replace(IReadOnlyList<EmulationMedia> media, EmulationMedia replacement);
    /// <summary>Marque le contenu éjecté après contrôle du type ; conserve la source si l’emplacement est absent.</summary>
    IReadOnlyList<EmulationMedia> Eject(IReadOnlyList<EmulationMedia> media, EmulationMediaSlot slot);
    /// <summary>Valide puis sérialise les montages dans leur document JSON existant ; le tableau appartient à l’appelant.</summary>
    byte[] Serialize(IReadOnlyList<EmulationMedia> media);
    /// <summary>Relit et valide le document existant sans ouvrir les fichiers des montages.</summary>
    IReadOnlyList<EmulationMedia> Deserialize(ReadOnlySpan<byte> payload);

    /// <summary>Restreint les extensions des lecteurs d'une catégorie sans modifier les descriptions empruntées.</summary>
    /// <param name="settings">Description du stockage conservant les lecteurs, montages et paramètres du modèle.</param>
    /// <param name="category">Catégorie des lecteurs auxquels appliquer la restriction du backend.</param>
    /// <param name="extensions">Extensions autorisées, avec ou sans point initial ; null conserve la description reçue.</param>
    /// <returns>Description filtrée sans ajout de format ; même instance lorsqu'aucune extension n'est retirée.</returns>
    /// <remarks>La comparaison ignore la casse. Un ensemble vide retire toutes les extensions de la catégorie,
    /// sans retirer ses lecteurs ni changer leurs autres propriétés. Les montages et emplacements configurés
    /// restent empruntés et inchangés ; aucun fichier n'est ouvert, converti ou supprimé.</remarks>
    EmulationStorageSettings RestrictAcceptedExtensions(EmulationStorageSettings settings, EmulationMediaCategory category, IReadOnlySet<string>? extensions);
}
