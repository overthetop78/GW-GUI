using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Interop;

namespace GWGUI.Emulation.Interfaces;

/// <summary>Traitements d’environnement communs des cœurs natifs, sans propriétaire de session dans le service.</summary>
/// <remarks>Le service est emprunté. Les pointeurs, exports et callbacks reçus restent possédés par leur fournisseur.
/// Les textes sont copiés ; les adresses de code ou de plages mémoire retournées restent empruntées au cœur.
/// Le fournisseur garde ses délégués vivants pendant leur enregistrement et cesse de les utiliser avant le
/// déchargement du code natif. Les politiques, bornes et diagnostics restent fournis par Common.
/// Les effets des paramètres ref/out et l’ordre des callbacks et lectures sont conservés.</remarks>
public interface IEmulationCoreEnvironmentProcessing
{
    /// <summary>Copie une structure native non nulle puis la transmet au récepteur emprunté.</summary>
    /// <typeparam name="T">Type du module ou du délégué, conservé par le contrat et ses contraintes existantes.</typeparam>
    /// <param name="data">Adresse native empruntée de la structure prévue par la commande, valide pendant l’appel.</param>
    /// <param name="apply">Récepteur emprunté de la structure copiée.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    bool ApplyStructure<T>(nint data, Action<T> apply) where T : struct;

    /// <summary>Applique géométrie, cadence et fréquence audio avec les contrôles existants de valeurs finies et positives.</summary>
    /// <param name="info">Géométrie et unités temporelles copiées depuis l’annonce du cœur.</param>
    /// <param name="applyGeometry">Récepteur emprunté de la géométrie, appelé avant la validation des unités temporelles.</param>
    /// <param name="setFramesPerSecond">Récepteur emprunté de la cadence positive et finie, en frames par seconde.</param>
    /// <param name="setSampleRate">Récepteur emprunté de la fréquence audio arrondie et contrôlée, en Hz.</param>
    void ApplySystemAvInfo(ExternalCoreApi.SystemAvInfo info, Action<ExternalCoreApi.Geometry> applyGeometry, Action<double> setFramesPerSecond, Action<int> setSampleRate);

    /// <summary>Copie les ports et leurs périphériques selon les bornes reçues et la politique de copie existante.</summary>
    /// <typeparam name="TPort">Type du module ou du délégué, conservé par le contrat et ses contraintes existantes.</typeparam>
    /// <typeparam name="TDevice">Type du module ou du délégué, conservé par le contrat et ses contraintes existantes.</typeparam>
    /// <param name="data">Adresse native empruntée de la structure prévue par la commande, valide pendant l’appel.</param>
    /// <param name="maximumPorts">Nombre maximal de ports à copier.</param>
    /// <param name="maximumDevices">Nombre maximal de périphériques à copier par port.</param>
    /// <param name="createDevice">Constructeur emprunté d’un périphérique depuis son nom copié et son identifiant natif.</param>
    /// <param name="createPort">Constructeur emprunté d’un port depuis ses périphériques copiés.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    IReadOnlyList<TPort> CopyControllerPorts<TPort, TDevice>(nint data, int maximumPorts, int maximumDevices, Func<string, uint, TDevice> createDevice, Func<IReadOnlyList<TDevice>, TPort> createPort);

    /// <summary>Copie et publie un diagnostic UTF-8 selon la politique de texte vide du module.</summary>
    /// <param name="textPointer">Pointeur emprunté vers un texte UTF-8 terminé par zéro, éventuellement nul.</param>
    /// <param name="publish">Récepteur emprunté du texte accepté ; ses exceptions sont propagées.</param>
    /// <param name="allowWhitespace">True pour accepter un texte contenant seulement des espaces ; false pour le filtrer.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    string? CopyDiagnostic(nint textPointer, Action<string> publish, bool allowWhitespace = false);

    /// <summary>Copie les descriptions d’entrées dans les types du module selon la borne reçue.</summary>
    /// <typeparam name="T">Type du module ou du délégué, conservé par le contrat et ses contraintes existantes.</typeparam>
    /// <param name="data">Adresse native empruntée de la structure prévue par la commande, valide pendant l’appel.</param>
    /// <param name="maximumCount">Borne de copie reçue du module, sans nouvelle règle de troncature.</param>
    /// <param name="create">Constructeur emprunté du descripteur typé depuis ses champs natifs copiés.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    IReadOnlyList<T> CopyInputDescriptors<T>(nint data, int maximumCount, Func<uint, uint, uint, uint, string, T> create);

    /// <summary>Copie les descriptions des plages mémoire sans prendre possession des adresses natives décrites.</summary>
    /// <typeparam name="T">Type du module ou du délégué, conservé par le contrat et ses contraintes existantes.</typeparam>
    /// <param name="data">Adresse native empruntée de la structure prévue par la commande, valide pendant l’appel.</param>
    /// <param name="maximumCount">Borne de copie reçue du module, sans nouvelle règle de troncature.</param>
    /// <param name="create">Constructeur emprunté du descripteur typé depuis ses champs natifs copiés.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    IReadOnlyList<T> CopyMemoryMap<T>(nint data, int maximumCount, Func<ulong, nint, nuint, nuint, nuint, nuint, nuint, string?, T> create);

    /// <summary>Copie le modèle de journalisation UTF-8 en remplaçant ses conversions par le marqueur du module.</summary>
    /// <param name="format">Adresse empruntée du modèle de journalisation UTF-8 terminé par zéro.</param>
    /// <param name="argumentMarker">Marqueur invariant du module inséré pour chaque conversion.</param>
    /// <param name="conversionCharacters">Caractères terminant les conversions de format reconnues.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    string CopyNativeLogTemplate(nint format, string argumentMarker, string conversionCharacters);

    /// <summary>Lit la culture d’interface courante pour produire l’identifiant de langue du protocole natif.</summary>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    uint CurrentLanguage();

    /// <summary>Route les options restantes et publie une seule fois le diagnostic de chaque commande inconnue.</summary>
    /// <param name="command">Identifiant invariant de la commande annoncée par le cœur.</param>
    /// <param name="data">Adresse native empruntée de la structure prévue par la commande, valide pendant l’appel.</param>
    /// <param name="options">Propriétaire emprunté des opérations du catalogue d’options.</param>
    /// <param name="unknownCommands">Ensemble mutable du module qui déduplique les commandes inconnues.</param>
    /// <param name="diagnostics">Collection mutable du module recevant les diagnostics nouveaux.</param>
    /// <param name="createUnknownDiagnostic">Fournisseur emprunté du texte de diagnostic de la commande inconnue.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    bool HandleRemainingCommand(uint command, nint data, IEmulationCoreOptionCommands options, ISet<uint> unknownCommands, ICollection<string> diagnostics, Func<uint, string> createUnknownDiagnostic);

    /// <summary>Enregistre les six callbacks empruntés immédiatement après leur lecture, dans l’ordre existant.</summary>
    /// <param name="exports">Table empruntée d’exports natifs, dont le code doit rester chargé pendant les appels.</param>
    /// <param name="callbacks">Fournisseur emprunté des opérations ou délégués d’environnement ; il en garde la propriété et la durée de vie.</param>
    void InstallCallbacks(ExternalCoreExports exports, IEmulationCoreCallbacks callbacks);

    /// <summary>Résout un délégué natif facultatif ; la variante à source conserve ses deux lectures distinctes si la première adresse est non nulle.</summary>
    /// <typeparam name="T">Type du module ou du délégué, conservé par le contrat et ses contraintes existantes.</typeparam>
    /// <typeparam name="TSource">Type du module ou du délégué, conservé par le contrat et ses contraintes existantes.</typeparam>
    /// <param name="source">Source empruntée dont la valeur d’adresse peut changer entre les lectures.</param>
    /// <param name="readPointer">Lecteur emprunté appelé une fois si l’adresse est nulle, deux fois sinon ; la seconde adresse sert à la conversion.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    T? OptionalDelegate<T, TSource>(TSource source, Func<TSource, nint> readPointer) where T : Delegate;

    /// <summary>Résout un délégué natif facultatif ; la variante à source conserve ses deux lectures distinctes si la première adresse est non nulle.</summary>
    /// <typeparam name="T">Type du module ou du délégué, conservé par le contrat et ses contraintes existantes.</typeparam>
    /// <param name="pointer">Adresse native empruntée du code à convertir ; l’appelant garde le code chargé pendant l’usage du délégué.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    T? OptionalDelegate<T>(nint pointer) where T : Delegate;

    /// <summary>Copie un chemin ou libellé de disque dans une chaîne gérée avec un buffer temporaire libéré dans finally.</summary>
    /// <typeparam name="T">Type du module ou du délégué, conservé par le contrat et ses contraintes existantes.</typeparam>
    /// <param name="getter">Délégué emprunté de lecture du chemin ou libellé natif ; null produit null sans allocation.</param>
    /// <param name="index">Index reçu du module ; ses validations et la disponibilité du lecteur restent à son propriétaire.</param>
    /// <param name="bufferSize">Taille en octets du buffer temporaire choisi par le module.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    string? ReadDiskText<T>(T? getter, int index, int bufferSize) where T : Delegate;

    /// <summary>Lit le format vidéo selon les formats acceptés par ce profil natif et conserve son erreur différée.</summary>
    /// <param name="data">Adresse native empruntée de la structure prévue par la commande, valide pendant l’appel.</param>
    /// <param name="createUnsupportedError">Fabrique d’erreur appelée seulement lorsque le format n’est pas accepté.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    EmulationPixelFormat ReadLegacyPixelFormat(nint data, Func<int, Exception> createUnsupportedError);

    /// <summary>Résout un délégué natif obligatoire ou produit l’erreur fournie pour une adresse nulle.</summary>
    /// <typeparam name="T">Type du module ou du délégué, conservé par le contrat et ses contraintes existantes.</typeparam>
    /// <param name="pointer">Adresse native empruntée du code à convertir ; l’appelant garde le code chargé pendant l’usage du délégué.</param>
    /// <param name="createError">Fabrique différée d’erreur pour l’absence du callback obligatoire.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    T RequiredDelegate<T>(nint pointer, Func<Exception> createError) where T : Delegate;

    /// <summary>Sélectionne une image et restaure la sélection et l’insertion précédentes si une étape échoue.</summary>
    /// <param name="index">Index reçu du module ; ses validations et la disponibilité du lecteur restent à son propriétaire.</param>
    /// <param name="getImageIndex">Lecteur emprunté de l’index courant avant la sélection.</param>
    /// <param name="getEjectState">Lecteur emprunté de l’état d’éjection avant la sélection.</param>
    /// <param name="setEjectState">Commande empruntée d’éjection ou d’insertion, utilisée aussi pour la restauration après échec.</param>
    /// <param name="setImageIndex">Commande empruntée de sélection d’image, utilisée aussi pour la restauration après échec.</param>
    /// <param name="ejectError">Fabrique d’erreur différée si l’éjection initiale échoue.</param>
    /// <param name="selectError">Fabrique d’erreur différée si la sélection échoue.</param>
    /// <param name="insertError">Fabrique d’erreur différée si l’insertion échoue.</param>
    void SelectDisk(int index, Func<uint> getImageIndex, Func<bool> getEjectState,
        Func<bool, bool> setEjectState, Func<uint, bool> setImageIndex,
        Func<Exception> ejectError, Func<Exception> selectError, Func<Exception> insertError);

    /// <summary>Écrit le callback d’activité des lecteurs dans la table native empruntée.</summary>
    /// <param name="data">Adresse native empruntée de la structure prévue par la commande, valide pendant l’appel.</param>
    /// <param name="setState">Délégué emprunté d’activation ou d’état, conservé vivant par son fournisseur pendant l’enregistrement natif.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    bool SetLedInterface(nint data, ExternalCoreApi.SetLedState setState);

    /// <summary>Écrit le callback de journalisation dans la table native empruntée, en conservant la signature du fournisseur.</summary>
    /// <param name="data">Adresse native empruntée de la structure prévue par la commande, valide pendant l’appel.</param>
    /// <param name="log">Délégué de journalisation emprunté ; son fournisseur le conserve pendant tout l’enregistrement natif.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    bool SetLogInterface(nint data, ExternalCoreApi.LogCallback log);

    /// <summary>Écrit le callback de journalisation dans la table native empruntée, en conservant la signature du fournisseur.</summary>
    /// <typeparam name="TLog">Type du module ou du délégué, conservé par le contrat et ses contraintes existantes.</typeparam>
    /// <param name="data">Adresse native empruntée de la structure prévue par la commande, valide pendant l’appel.</param>
    /// <param name="log">Délégué de journalisation emprunté ; son fournisseur le conserve pendant tout l’enregistrement natif.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    bool SetLogInterface<TLog>(nint data, TLog log) where TLog : Delegate;

    /// <summary>Valide le format vidéo annoncé et actualise le format commun seulement si la requête est acceptée.</summary>
    /// <param name="data">Adresse native empruntée de la structure prévue par la commande, valide pendant l’appel.</param>
    /// <param name="pixelFormat">Format courant du module, remplacé seulement après validation de la valeur native.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    bool SetPixelFormat(nint data, ref EmulationPixelFormat pixelFormat);

    /// <summary>Écrit le callback de vibration dans la table native empruntée.</summary>
    /// <param name="data">Adresse native empruntée de la structure prévue par la commande, valide pendant l’appel.</param>
    /// <param name="setState">Délégué emprunté d’activation ou d’état, conservé vivant par son fournisseur pendant l’enregistrement natif.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    bool SetRumbleInterface(nint data, ExternalCoreApi.SetRumbleState setState);

    /// <summary>Écrit les callbacks d’activation et de lecture des capteurs dans la table native empruntée.</summary>
    /// <param name="data">Adresse native empruntée de la structure prévue par la commande, valide pendant l’appel.</param>
    /// <param name="setState">Délégué emprunté d’activation ou d’état, conservé vivant par son fournisseur pendant l’enregistrement natif.</param>
    /// <param name="getInput">Délégué emprunté de lecture d’un capteur, conservé vivant par son fournisseur.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    bool SetSensorInterface(nint data, ExternalCoreApi.SetSensorState setState, ExternalCoreApi.GetSensorInput getInput);

    /// <summary>Copie les ports avec les politiques explicites de dépassement et de noms vides du module.</summary>
    /// <typeparam name="TPort">Type du module ou du délégué, conservé par le contrat et ses contraintes existantes.</typeparam>
    /// <typeparam name="TDevice">Type du module ou du délégué, conservé par le contrat et ses contraintes existantes.</typeparam>
    /// <param name="data">Adresse native empruntée de la structure prévue par la commande, valide pendant l’appel.</param>
    /// <param name="maximumPorts">Nombre maximal de ports à copier.</param>
    /// <param name="maximumDevices">Nombre maximal de périphériques à copier par port.</param>
    /// <param name="createDevice">Constructeur emprunté d’un périphérique depuis son nom copié et son identifiant natif.</param>
    /// <param name="createPort">Constructeur emprunté d’un port depuis ses périphériques copiés.</param>
    /// <param name="rejectOverflow">True pour refuser un port dépassant la borne ; false pour conserver la troncature existante.</param>
    /// <param name="skipEmptyNames">True pour omettre les descriptions de périphérique dont le nom est vide.</param>
    /// <param name="result">Ports copiés en sortie ; les résultats partiels et refus conservent la politique existante.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    bool TryCopyControllerPorts<TPort, TDevice>(nint data, int maximumPorts, int maximumDevices,
        Func<string, uint, TDevice> createDevice, Func<IReadOnlyList<TDevice>, TPort> createPort,
        bool rejectOverflow, bool skipEmptyNames, out IReadOnlyList<TPort> result);

    /// <summary>Route une commande reconnue vers le contrat d’environnement fourni.</summary>
    /// <param name="command">Identifiant invariant de la commande annoncée par le cœur.</param>
    /// <param name="data">Adresse native empruntée de la structure prévue par la commande, valide pendant l’appel.</param>
    /// <param name="callbacks">Fournisseur emprunté des opérations ou délégués d’environnement ; il en garde la propriété et la durée de vie.</param>
    /// <param name="result">Réponse de l’opération en sortie ; les routeurs renvoient séparément si la commande est reconnue.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    bool TryHandleCallbackCommand(uint command, nint data, IEmulationCoreEnvironmentCommands callbacks, out bool result);

    /// <summary>Route le profil de commandes natif et conserve sa politique de réponse aux enregistrements d’options.</summary>
    /// <param name="command">Identifiant invariant de la commande annoncée par le cœur.</param>
    /// <param name="data">Adresse native empruntée de la structure prévue par la commande, valide pendant l’appel.</param>
    /// <param name="callbacks">Fournisseur emprunté des opérations ou délégués d’environnement ; il en garde la propriété et la durée de vie.</param>
    /// <param name="options">Propriétaire emprunté des opérations du catalogue d’options.</param>
    /// <param name="result">Réponse de l’opération en sortie ; les routeurs renvoient séparément si la commande est reconnue.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    bool TryHandleLegacyCallbackCommand(uint command, nint data, IEmulationCoreEnvironmentCommands callbacks, IEmulationCoreOptionCommands options, out bool result);

    /// <summary>Route le profil de commandes natif avec la politique fournie pour les contrôles de disque nuls.</summary>
    /// <param name="command">Identifiant invariant de la commande annoncée par le cœur.</param>
    /// <param name="data">Adresse native empruntée de la structure prévue par la commande, valide pendant l’appel.</param>
    /// <param name="callbacks">Fournisseur emprunté des opérations ou délégués d’environnement ; il en garde la propriété et la durée de vie.</param>
    /// <param name="captureNullDiskControls">Politique du module pour transmettre aussi les requêtes nulles de contrôle de disques.</param>
    /// <param name="result">Réponse de l’opération en sortie ; les routeurs renvoient séparément si la commande est reconnue.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    bool TryHandleLegacyEnvironmentCommand(uint command, nint data, IEmulationCoreEnvironmentCommands callbacks, bool captureNullDiskControls, out bool result);

    /// <summary>Répond aux requêtes de capacités du profil natif selon les versions reçues.</summary>
    /// <param name="command">Identifiant invariant de la commande annoncée par le cœur.</param>
    /// <param name="data">Adresse native empruntée de la structure prévue par la commande, valide pendant l’appel.</param>
    /// <param name="coreOptionsVersion">Version des commandes d’options annoncée par le module.</param>
    /// <param name="messageInterfaceVersion">Version de l’interface de messages annoncée par le module.</param>
    /// <param name="result">Réponse de l’opération en sortie ; les routeurs renvoient séparément si la commande est reconnue.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    bool TryHandleLegacyQuery(uint command, nint data, uint coreOptionsVersion, uint messageInterfaceVersion, out bool result);

    /// <summary>Route une commande d’options reconnue vers son propriétaire typé.</summary>
    /// <param name="command">Identifiant invariant de la commande annoncée par le cœur.</param>
    /// <param name="data">Adresse native empruntée de la structure prévue par la commande, valide pendant l’appel.</param>
    /// <param name="options">Propriétaire emprunté des opérations du catalogue d’options.</param>
    /// <param name="result">Réponse de l’opération en sortie ; les routeurs renvoient séparément si la commande est reconnue.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    bool TryHandleOptionCommand(uint command, nint data, IEmulationCoreOptionCommands options, out bool result);

    /// <summary>Répond aux requêtes de versions, de capacités et de langue sans conserver d’état de session.</summary>
    /// <param name="command">Identifiant invariant de la commande annoncée par le cœur.</param>
    /// <param name="data">Adresse native empruntée de la structure prévue par la commande, valide pendant l’appel.</param>
    /// <param name="coreOptionsVersion">Version des commandes d’options annoncée par le module.</param>
    /// <param name="messageInterfaceVersion">Version de l’interface de messages annoncée par le module.</param>
    /// <param name="inputCapabilities">Masque de capacités des périphériques annoncé par le module.</param>
    /// <param name="result">Réponse de l’opération en sortie ; les routeurs renvoient séparément si la commande est reconnue.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    bool TryHandleStatelessCommand(uint command, nint data, uint coreOptionsVersion, uint messageInterfaceVersion, ulong inputCapabilities, out bool result);

    /// <summary>Écrit un booléen natif sur un octet dans la destination empruntée si elle est non nulle.</summary>
    /// <param name="destination">Adresse native empruntée, accessible en écriture pour la taille du type annoncé.</param>
    /// <param name="value">Valeur ou adresse à écrire selon la surcharge ; aucune propriété de l’adresse n’est transférée.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    bool WriteBoolean(nint destination, bool value);

    /// <summary>Écrit un entier natif sur 32 bits ; la surcharge non signée conserve sa conversion contrôlée avant l’écriture.</summary>
    /// <param name="destination">Adresse native empruntée, accessible en écriture pour la taille du type annoncé.</param>
    /// <param name="value">Valeur ou adresse à écrire selon la surcharge ; aucune propriété de l’adresse n’est transférée.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    bool WriteInteger(nint destination, int value);

    /// <summary>Écrit un entier natif sur 32 bits ; la surcharge non signée conserve sa conversion contrôlée avant l’écriture.</summary>
    /// <param name="destination">Adresse native empruntée, accessible en écriture pour la taille du type annoncé.</param>
    /// <param name="value">Valeur ou adresse à écrire selon la surcharge ; aucune propriété de l’adresse n’est transférée.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    bool WriteInteger(nint destination, uint value);

    /// <summary>Écrit un pointeur dans la destination empruntée si elle est non nulle.</summary>
    /// <param name="destination">Adresse native empruntée, accessible en écriture pour la taille du type annoncé.</param>
    /// <param name="value">Valeur ou adresse à écrire selon la surcharge ; aucune propriété de l’adresse n’est transférée.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    bool WritePointer(nint destination, nint value);

    /// <summary>Écrit les bits d’un entier non signé sur 64 bits dans la destination empruntée.</summary>
    /// <param name="destination">Adresse native empruntée, accessible en écriture pour la taille du type annoncé.</param>
    /// <param name="value">Valeur ou adresse à écrire selon la surcharge ; aucune propriété de l’adresse n’est transférée.</param>
    /// <returns>Résultat du traitement existant, sans transfert de propriété des entrées empruntées.</returns>
    bool WriteUnsignedLong(nint destination, ulong value);
}
