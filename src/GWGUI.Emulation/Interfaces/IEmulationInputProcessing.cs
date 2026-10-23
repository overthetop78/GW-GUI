using GWGUI.Emulation.Interop;

namespace GWGUI.Emulation.Interfaces;

/// <summary>Traitements sans état des instantanés d’entrées échangés avec les cœurs.</summary>
/// <remarks>Le service ne possède aucun périphérique et ne conserve aucun état de session.
/// Les accumulateurs créés possèdent leur état indépendant ; IEmulationInputFrames permet également de copier les collections reçues.</remarks>
public interface IEmulationInputProcessing
{
    /// <summary>Crée un accumulateur indépendant qui emprunte les collections et conserve les mouvements relatifs jusqu’à leur consommation.</summary>
    /// <returns>Contrat appartenant à l’appelant ; le service ne conserve pas l’instance créée.</returns>
    IEmulationInputAccumulator CreateInputAccumulator();

    /// <summary>Crée un stockage indépendant qui copie les collections et publie un instantané stable par frame.</summary>
    /// <remarks>Update additionne les quatre composantes relatives avec saturation ; Poll les consomme une fois et conserve le dernier état absolu.</remarks>
    /// <returns>Stockage appartenant à l’appelant, sans périphérique ni ressource native possédée.</returns>
    IEmulationInputFrames CreateInputFrames();

    /// <summary>Convertit les noms de touches reconnus en valeurs typées, en conservant la comparaison ordinale des commandes.</summary>
    IReadOnlyDictionary<string, EmulationKey> ToKeys(IReadOnlyDictionary<string, string> values);
    /// <summary>Convertit les touches en noms ; pour une source absente, utilise le comparateur de dictionnaire vide fourni.</summary>
    IReadOnlyDictionary<string, string> ToStrings(IReadOnlyDictionary<string, EmulationKey>? values, IEqualityComparer<string>? emptyComparer = null);
    /// <summary>Accepte une source vide ou un préfixe clavier, souris ou contrôleur selon les comparaisons existantes.</summary>
    bool IsValidSourcePrefix(string? source);

    /// <summary>Calcule le masque natif des touches Majuscule, Contrôle et Alt.</summary>
    /// <param name="keys">Touches appuyées empruntées pendant le calcul ; le service ne conserve pas cet ensemble.</param>
    /// <returns>Masque natif des modificateurs ; les touches droites et gauches utilisent les mêmes bits.</returns>
    ushort Modifiers(IReadOnlySet<EmulationKey> keys);

    /// <summary>Calcule le caractère associé à un code natif selon Majuscule et le verrouillage des majuscules.</summary>
    /// <param name="code">Code clavier natif, distinct de la valeur numérique de l’énumération EmulationKey.</param>
    /// <param name="keys">Touches appuyées empruntées pour déterminer les modificateurs.</param>
    /// <returns>Caractère natif, ou zéro lorsque le code ne produit pas de caractère imprimable.</returns>
    uint Character(uint code, IReadOnlySet<EmulationKey> keys);

    /// <summary>Complète une table de correspondances avec une plage continue de touches et de codes.</summary>
    /// <param name="map">Table mutable appartenant à l’appelant ; les entrées de la plage sont écrites sur place.</param>
    /// <param name="firstKey">Première touche de la plage dans l’énumération.</param>
    /// <param name="firstCode">Premier code natif, incrémenté pour chaque touche.</param>
    /// <param name="count">Nombre de touches à écrire ; une valeur non positive ne produit aucune écriture.</param>
    void AddRange(IDictionary<EmulationKey, uint> map, EmulationKey firstKey, uint firstCode, int count);

    /// <summary>Publie les relâchements avant les pressions, suivant les politiques fournies par l’adaptateur.</summary>
    /// <param name="keys">Touches courantes empruntées pour cette publication.</param>
    /// <param name="keyMap">Correspondances empruntées ; les touches absentes de cette table ne produisent pas d’événement.</param>
    /// <param name="modifiers">Masque natif transmis tel quel à chaque événement.</param>
    /// <param name="previous">Référence de l’état précédent du propriétaire, remplacée par une copie des touches après succès.</param>
    /// <param name="keyboardEvent">Référence du callback enregistré par le propriétaire ; les remplacements réentrants restent visibles.</param>
    /// <param name="isModifier">Politique de priorité : modificateurs relâchés après les touches et pressés avant elles.</param>
    /// <param name="characterFor">Politique de caractère appliquée aux pressions ; les relâchements transmettent zéro.</param>
    /// <remarks>Le propriétaire vérifie la présence du callback avant publication. Une exception interrompt la publication
    /// et empêche sa copie finale de l’état. Les changements réentrants de previous et keyboardEvent restent visibles ;
    /// aucune ressource ni référence de session n’est conservée par le service.</remarks>
    void PublishTransitions(IReadOnlySet<EmulationKey> keys, IReadOnlyDictionary<EmulationKey, uint> keyMap, ushort modifiers,
        ref IReadOnlySet<EmulationKey> previous, ref ExternalCoreApi.KeyboardEvent? keyboardEvent,
        Func<EmulationKey, bool> isModifier, Func<uint, IReadOnlySet<EmulationKey>, uint> characterFor);

    /// <summary>Publie les transitions de codes natifs sans remplacer les ensembles détenus par l’appelant.</summary>
    /// <typeparam name="TSet">Type d’ensemble de codes choisi par le propriétaire pour son état de session.</typeparam>
    /// <param name="current">Codes courants empruntés ; aucun ensemble de remplacement n’est créé.</param>
    /// <param name="previous">Référence de l’ensemble précédent, permettant de voir une réaffectation réentrante entre les deux phases.</param>
    /// <param name="keyboardEvent">Callback emprunté pour cette publication ; le service ne le mémorise pas.</param>
    /// <param name="modifiers">Masque natif transmis sans transformation.</param>
    /// <param name="orderReleased">Ordre des codes relâchés ; cette phase précède les pressions.</param>
    /// <param name="orderPressed">Ordre des codes pressés, évalué après les relâchements.</param>
    /// <param name="characterFor">Caractère calculé pour chaque pression ; les relâchements transmettent zéro.</param>
    /// <remarks>Le propriétaire valide le callback et conserve la responsabilité de mémoriser current après succès.
    /// Les exceptions des politiques et du callback sont propagées ; aucune fermeture ni destruction du callback emprunté n’a lieu.</remarks>
    void PublishCodeTransitions<TSet>(TSet current, ref TSet previous, ExternalCoreApi.KeyboardEvent keyboardEvent, ushort modifiers,
        Func<IEnumerable<uint>, IEnumerable<uint>> orderReleased, Func<IEnumerable<uint>, IEnumerable<uint>> orderPressed, Func<uint, uint> characterFor) where TSet : IReadOnlySet<uint>;
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
    /// <remarks>Une valeur nulle, vide ou blanche est acceptée comme dans la validation commune existante.</remarks>
    bool IsValidBinding(string? binding);

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

    /// <summary>Lit les boutons, pressions, axes, souris, touches et dispositifs de pointage selon les identifiants natifs génériques.</summary>
    /// <param name="snapshot">Entrées empruntées et stables pendant le callback.</param>
    /// <param name="port">Indice natif du port demandé.</param>
    /// <param name="device">Code natif : manette 1, souris 2, clavier 3, pistolet 4, analogique 5 ou stylet 6 ; les sous-types conservent la classe de leur octet bas.</param>
    /// <param name="index">Indice de composante du périphérique.</param>
    /// <param name="id">Identifiant de bouton, touche ou axe dans le contrat natif.</param>
    /// <returns>Valeur native de l’entrée ; zéro pour une source absente ou un périphérique non couvert par ce sélecteur générique.</returns>
    /// <remarks>Pour le pointage, LeftX/LeftY contiennent les coordonnées absolues signées ; leurs extrémités indiquent
    /// une position hors écran. Le pistolet utilise B pour le tir, A/X/Y pour ses auxiliaires, les directions,
    /// Start, Select et Reload. Le stylet utilise A pour l’appui ; un seul contact est représenté par port.
    /// Les pressions analogiques se lisent à l’index 2, sur les seize identifiants de boutons natifs.</remarks>
    short ReadNativeInput(EmulationInputSnapshot snapshot, uint port, uint device, uint index, uint id);
    /// <summary>Lit une commande native de souris à partir du pointeur de la frame.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    short NativeMouseState(EmulationPointerState pointer, uint port, uint index, uint id);

    /// <summary>Lit un bouton ou le masque natif de la manette, avec la capacité fournie par l’adaptation.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    short NativeJoypadState(uint buttons, uint id, uint maximumButtonCount);

    /// <summary>Lit un bouton ou le masque natif de la manette, avec la capacité fournie par l’adaptation.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    short NativeJoypadState(EmulationControllerState controller, uint id, uint maximumButtonCount);

    /// <summary>Lit la composante native d’un stick ou la pression d’un bouton à l’index analogique 2.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    short NativeAnalogState(EmulationControllerState controller, uint index, uint id);

    /// <summary>Résout la valeur invariante d’un libellé de liaison selon sa clé de ressource.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    string? BindingDisplayValue(string resourceKey, string? invariant = null);

    /// <summary>Additionne deux déplacements relatifs en saturant aux limites du type Int32.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    int SaturatingAdd(int left, int right);

    /// <summary>Projette les associations de souris vers les noms de commandes, sans conserver la collection reçue.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    Dictionary<string, string> MouseBindings<TAction>(IReadOnlyDictionary<string, TAction>? mappings, TAction none) where TAction : struct, Enum;

    /// <summary>Reconstruit les associations typées de souris en conservant les règles de sélection existantes.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    Dictionary<string, TAction>? CreateMouseMappings<TAction>(EmulationInputBindingSet? bindings, TAction none) where TAction : struct, Enum;

    /// <summary>Copie les seules associations dont la commande et la source ne sont pas vides.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    Dictionary<string, string> CopyNonEmptyBindings(IReadOnlyDictionary<string, string> values);

    /// <summary>Vérifie les identifiants autorisés et la syntaxe des sources par le prédicat fourni.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    bool HasInvalidBindings(IReadOnlyDictionary<string, string>? bindings, IReadOnlySet<string> commandIds, Func<string, bool> isValidSource);

    /// <summary>Contrôle le nombre de ports et leur numérotation continue à partir de un.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    bool HasInvalidControllerPortNumbers(EmulationInputSettings settings, int count);

    /// <summary>Résout les valeurs par défaut et valide strictement les associations de clavier fournies.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    Dictionary<string, string> ResolveKeyboardBindings(IReadOnlyList<InputBindingDefinition> definitions, IReadOnlyDictionary<string, string>? input);

    /// <summary>Projette les valeurs proposées sur les commandes définies et leurs valeurs par défaut.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    Dictionary<string, string> ResolveBindings(IReadOnlyList<InputBindingDefinition> definitions, IReadOnlyDictionary<string, string>? values);

    /// <summary>Construit les associations de ports après validation des périphériques par le module.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    TBinding[] CreateControllerBindings<TPeripheral, TBinding>(EmulationInputSettings settings, Func<int, TPeripheral, bool> supportsPeripheral, Func<EmulationControllerPort, TPeripheral, IReadOnlyDictionary<string, string>, TBinding> createBinding) where TPeripheral : struct, Enum;

    /// <summary>Projette les touches ou les sources physiques vers les touches émulées.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    IReadOnlySet<EmulationKey> MapKeyboard(IReadOnlySet<EmulationKey> keys, IReadOnlyDictionary<string, EmulationKey>? mappings);

    /// <summary>Extrait l’identité physique éventuellement inscrite dans une expression de source.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    string? ParseControllerDeviceId(string? source);

    /// <summary>Résout le contrôleur désigné par une expression en conservant la politique de repli.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    EmulationControllerState ResolveSourceController(string source, IReadOnlyList<EmulationControllerState> controllers, EmulationControllerState fallback, bool fallbackWhenMissing);

    /// <summary>Retire un préfixe reconnu sans modifier la comparaison ni normaliser la source.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    bool TryRemovePrefix(string? value, string prefix, out string source);

    /// <summary>Produit l’état des commandes physiques de souris à partir du pointeur emprunté.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    IReadOnlyDictionary<string, bool> PhysicalMouse(EmulationPointerState pointer);

    /// <summary>Lit la valeur d’une commande physique de contrôleur.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    float ControllerSourceValue(string source, EmulationControllerState controller);

    /// <summary>Évalue l’appui d’une commande de contrôleur selon les règles existantes.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    bool IsControllerSourcePressed(string source, EmulationControllerState controller);

    /// <summary>Évalue l’appui d’une commande de contrôleur selon les règles existantes.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    bool IsControllerSourcePressed(string source, EmulationControllerState controller, float value);

    /// <summary>Calcule un axe signé à partir de ses commandes opposées et des sources physiques.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    short MapControllerAxis(IReadOnlyDictionary<string, string> mappings, string positiveCommand, string negativeCommand, EmulationControllerState controller, IReadOnlyList<EmulationControllerState> physical, IReadOnlySet<EmulationKey> keys, IReadOnlyDictionary<string, bool> mouse);

    /// <summary>Calcule une pression encodée à partir de l’association de la commande.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    short MapControllerPressure(IReadOnlyDictionary<string, string> mappings, string command, EmulationControllerState controller, IReadOnlyList<EmulationControllerState> physical, IReadOnlySet<EmulationKey> keys, IReadOnlyDictionary<string, bool> mouse);

    /// <summary>Évalue la pression d’une expression de clavier, souris ou contrôleur.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    float InputSourcePressure(string source, EmulationControllerState controller, IReadOnlyList<EmulationControllerState> physical, IReadOnlySet<EmulationKey> keys, IReadOnlyDictionary<string, bool> mouse);

    /// <summary>Évalue la pression de la commande physique d’un contrôleur.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    float ControllerSourcePressure(string source, EmulationControllerState controller);

    /// <summary>Résout l’expression opposée d’un axe reconnu.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    string? OppositeControllerAxisSource(string source);

    /// <summary>Évalue l’appui d’une expression de source à partir de l’instantané physique.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    bool IsMappedInputPressed(string mapping, EmulationControllerState controller, EmulationInputSnapshot snapshot);

    /// <summary>Convertit une amplitude normalisée vers un axe signé sur seize bits.</summary>
    /// <remarks>Les données reçues sont empruntées pendant le calcul. Cette opération ne possède aucun périphérique physique.</remarks>
    short NormalizedAxis(float value);
}
