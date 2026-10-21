namespace GWGUI.Emulation.Interfaces;

/// <summary>Traitements sans état des instantanés d’entrées échangés avec les cœurs.</summary>
/// <remarks>Le service ne possède aucun périphérique et ne conserve aucun état de session.
/// L’accumulation des mouvements entre deux frames appartient au contrat IEmulationInputFrames.</remarks>
public interface IEmulationInputProcessing
{
    /// <summary>Résout une commande dans la table de boutons commune utilisée par les adaptations natives.</summary>
    /// <param name="commandId">Identifiant technique de commande, comparé sans distinction de casse.</param>
    /// <returns>Index du bouton à partir de zéro, ou −1 si la commande n’appartient pas à cette table.</returns>
    /// <remarks>L’index désigne un bit du masque de boutons, pas une touche clavier ni un numéro de port.
    /// Les adaptations conservent leurs associations propres et résolvent leur commande native par ce contrat.</remarks>
    int ControllerButtonIndex(string commandId);

    /// <summary>Copie les collections d’un instantané pour les stabiliser pendant son utilisation par un cœur.</summary>
    /// <param name="snapshot">Entrées empruntées ; null représente un ensemble d’entrées vide.</param>
    /// <returns>Instantané dont les touches, le pointeur et la collection de contrôleurs sont copiés.</returns>
    /// <remarks>Les axes, boutons et déplacements conservent leurs valeurs. Cette copie n’applique ni zone morte
    /// supplémentaire ni consommation des mouvements relatifs. L’appelant conserve la propriété de ses collections initiales.</remarks>
    EmulationInputSnapshot Freeze(EmulationInputSnapshot? snapshot);

    /// <summary>Projette les sources physiques vers les touches émulées suivant les associations configurées.</summary>
    /// <param name="snapshot">Instantané physique emprunté pendant le calcul.</param>
    /// <param name="mappings">Expressions des sources indexées par touche émulée, ou null pour les entrées directes.</param>
    /// <returns>Ensemble stable des touches émulées actuellement appuyées.</returns>
    IReadOnlySet<EmulationKey> MapKeyboard(EmulationInputSnapshot snapshot, IReadOnlyDictionary<string, string>? mappings);

    /// <summary>Résout le contrôleur physique associé à une identité ou à un indice de repli.</summary>
    /// <param name="deviceId">Identité physique facultative annoncée par l’hôte.</param>
    /// <param name="controllers">Instantanés physiques empruntés de la frame.</param>
    /// <param name="fallbackIndex">Indice utilisé uniquement en l’absence d’une identité explicite.</param>
    /// <returns>État du contrôleur trouvé, ou état vide si la source est absente.</returns>
    EmulationControllerState ResolveController(string? deviceId, IReadOnlyList<EmulationControllerState> controllers, int fallbackIndex);

    /// <summary>Évalue une association de clavier, souris ou contrôleur avec les règles de pression existantes.</summary>
    /// <param name="source">Expression de source configurée par l’utilisateur.</param>
    /// <param name="controller">Contrôleur utilisé lorsque l’expression ne désigne pas une autre source.</param>
    /// <param name="snapshot">Instantané physique emprunté contenant toutes les sources disponibles.</param>
    /// <returns>Pression normalisée calculée par le traitement commun.</returns>
    float InputSourcePressure(string source, EmulationControllerState controller, EmulationInputSnapshot snapshot);

    /// <summary>Applique la zone morte aux axes et gâchettes physiques avant leur encodage natif.</summary>
    /// <param name="controller">État physique emprunté, conservé inchangé.</param>
    /// <param name="percent">Zone morte exprimée en pourcentage de la course.</param>
    /// <returns>Nouvel état avec les composantes proches du repos neutralisées.</returns>
    EmulationControllerState ApplyDeadZones(EmulationControllerState controller, int percent);

    /// <summary>Contrôle la syntaxe d’une association avec les règles communes du clavier et des sources préfixées.</summary>
    /// <param name="binding">Expression de source ; une valeur vide décrit une action non associée.</param>
    /// <returns>True lorsque l’expression est admise par le traitement commun.</returns>
    bool IsValidBinding(string binding);

    /// <summary>Résout le contrôleur explicitement nommé dans une expression de source.</summary>
    /// <param name="source">Expression d’association éventuellement préfixée par une identité physique.</param>
    /// <param name="controllers">Sources physiques empruntées de la frame.</param>
    /// <param name="fallback">Source employée lorsque l’expression ne précise pas une identité.</param>
    /// <returns>Source sélectionnée, ou état vide lorsque l’identité explicite est absente.</returns>
    EmulationControllerState ResolveSourceController(string source, IReadOnlyList<EmulationControllerState> controllers, EmulationControllerState fallback);

    /// <summary>Évalue la pression d’une association avec la règle de zone morte existante.</summary>
    /// <param name="mapping">Expression de source configurée pour la commande.</param>
    /// <param name="controller">Source physique résolue pour cette expression.</param>
    /// <param name="snapshot">Autres entrées physiques empruntées pour les expressions combinées.</param>
    /// <param name="deadZone">Zone morte exprimée en pourcentage.</param>
    /// <returns>Amplitude normalisée entre zéro et un, neutralisée selon les règles communes.</returns>
    float MappedInputMagnitude(string mapping, EmulationControllerState controller, EmulationInputSnapshot snapshot, int deadZone);

    /// <summary>Fournit les correspondances génériques de touches vers les codes du clavier natif.</summary>
    /// <returns>Table indépendante construite par le traitement commun existant.</returns>
    IReadOnlyDictionary<EmulationKey, uint> CreateKeyMap();

    /// <summary>Lit les boutons, axes, souris ou touches de l’instantané selon les identifiants natifs génériques.</summary>
    /// <param name="snapshot">Entrées empruntées et stables pendant le callback.</param>
    /// <param name="port">Indice natif du port demandé.</param>
    /// <param name="device">Code natif : manette 1, souris 2, clavier 3 ou axes analogiques 5.</param>
    /// <param name="index">Indice de composante du périphérique.</param>
    /// <param name="id">Identifiant de bouton, touche ou axe dans le contrat natif.</param>
    /// <returns>Valeur native de l’entrée ; zéro pour une source absente ou un périphérique non couvert par ce sélecteur générique.</returns>
    short ReadNativeInput(EmulationInputSnapshot snapshot, uint port, uint device, uint index, uint id);
}
