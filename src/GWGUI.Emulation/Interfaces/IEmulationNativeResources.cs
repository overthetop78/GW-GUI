using GWGUI.Emulation.Interop;
using GWGUI.Emulation.Contracts;

namespace GWGUI.Emulation.Interfaces;

/// <summary>Construction des ressources natives communes, sans exposition de leurs classes d’implémentation.</summary>
/// <remarks>Le service est emprunté et sans ressource de session. Chaque résultat possède ses propres
/// allocations ; l’appelant les libère dans un finally, sauf transfert réussi à un autre propriétaire.</remarks>
public interface IEmulationNativeResources
{
    /// <summary>Crée le contrôle commun des images de disques d’un cœur.</summary>
    /// <param name="policy">Messages et règles empruntés au module, lus au moment du traitement correspondant.</param>
    /// <param name="createContent">Fabrique empruntée de contenu possédé par chaque insertion ; null pour le chargement natif par chemin seul.</param>
    /// <param name="mediaExists">Validation empruntée facultative ; null conserve les validations existantes de chaque mode.</param>
    /// <returns>Contrôle dont les callbacks sont empruntés au cœur ; la session les détache par Clear avant de décharger ce code.</returns>
    /// <remarks>La présence d’une fabrique conserve les règles de contenu chargé, de lecture du nombre
    /// d’images et de restauration propres à ce mode. Aucun callback ni fabrique n’est exécuté pendant la construction.</remarks>
    IEmulationDiskControl CreateDiskControl(IEmulationDiskControlPolicy policy, Func<string, IEmulationLoadedContent>? createContent = null, Func<string, bool>? mediaExists = null);

    /// <summary>Copie un texte en UTF-8 terminé par zéro dans une allocation native indépendante.</summary>
    /// <param name="value">Texte à copier ; null est refusé.</param>
    /// <param name="minimumByteCapacity">Capacité minimale en octets, terminateur compris ; le complément est nul.</param>
    /// <returns>Chaîne possédée par l’appelant ; sa capacité couvre toujours le texte et son terminateur.</returns>
    IEmulationNativeString CreateString(string value, int minimumByteCapacity = 0);

    /// <summary>Copie un texte en ANSI sous Windows si demandé, en UTF-8 dans les autres cas.</summary>
    /// <param name="value">Texte à copier ; null est refusé.</param>
    /// <param name="useWindowsAnsi">Sélection de l’encodage ANSI Windows pour les cœurs qui l’exigent.</param>
    /// <returns>Chaîne possédée par l’appelant, libérée après son dernier emprunt natif.</returns>
    IEmulationNativeString CreateString(string value, bool useWindowsAnsi);

    /// <summary>Construit une chaîne UTF-8 et en transfère la propriété au dictionnaire si l’ajout réussit.</summary>
    /// <param name="owner">Propriétaire qui libérera les chaînes enregistrées lors de son nettoyage.</param>
    /// <param name="key">Clé transmise à Add ; les règles et erreurs du dictionnaire sont conservées.</param>
    /// <param name="value">Texte à copier dans la nouvelle allocation.</param>
    /// <returns>Emprunt de la chaîne désormais possédée par le dictionnaire.</returns>
    /// <remarks>Si l’ajout échoue, la nouvelle allocation est libérée et l’exception d’origine est propagée.</remarks>
    IEmulationNativeString AddString(IDictionary<string, IEmulationNativeString> owner, string key, string value);

    /// <summary>Prépare le descripteur natif d’un fichier pour le contrat de chargement du cœur.</summary>
    /// <param name="absolutePath">Chemin déjà validé par le module, copié dans le descripteur.</param>
    /// <param name="needsFullPath">True pour transmettre seulement le chemin ; false pour copier aussi les octets du fichier.</param>
    /// <param name="useWindowsAnsiPath">Encodage ANSI du chemin sous Windows si le cœur l’exige.</param>
    /// <returns>Contenu possédé par l’appelant, qui libère ensemble descripteur, chemin et données.</returns>
    /// <remarks>La construction libère toutes ses allocations si une étape échoue. Elle ne modifie aucun fichier.</remarks>
    IEmulationLoadedContent CreateLoadedContent(string absolutePath, bool needsFullPath, bool useWindowsAnsiPath);

    /// <summary>Crée le propriétaire du système de fichiers virtuel natif commun.</summary>
    /// <param name="readObserver">Callback emprunté appelé après une lecture, avec le chemin résolu et le nombre d’octets lus ; null pour aucune notification.</param>
    /// <param name="pathResolver">Callback emprunté associant un chemin du cœur à un chemin de stockage et à sa politique de lecture seule ; null pour conserver les chemins et droits habituels.</param>
    /// <returns>Propriétaire indépendant, à libérer après le dernier appel du cœur à ses callbacks natifs.</returns>
    /// <remarks>La construction conserve les délégués pour leur durée de vie native. Les fichiers sont ouverts
    /// uniquement sur demande du cœur. Le propriétaire ferme dans Dispose les fichiers et répertoires restants.</remarks>
    IEmulationVirtualFileSystem CreateVirtualFileSystem(Action<string, long>? readObserver = null, Func<string, (string Path, bool IsReadOnly)>? pathResolver = null);

    /// <summary>Charge une bibliothèque native existante après validation de son chemin absolu.</summary>
    /// <param name="absolutePath">Chemin complet du fichier de bibliothèque, déjà sélectionné par le module.</param>
    /// <returns>Propriétaire indépendant de la bibliothèque ; l’appelant le libère après la déinitialisation du cœur.</returns>
    /// <remarks>La bibliothèque peut exécuter du code natif au chargement. Cette opération ne découvre pas
    /// les modules et ne remplace pas leur sélection : elle charge seulement le fichier demandé par Common.</remarks>
    IEmulationCoreLibrary CreateLibrary(string absolutePath);

    /// <summary>Valide le chemin de bibliothèque avec les erreurs contextualisées fournies par le module.</summary>
    /// <param name="absolutePath">Chemin exact conservé dans les diagnostics ; aucune normalisation supplémentaire.</param>
    /// <param name="requireAbsolute">Fabrique appelée uniquement si le chemin n’est pas absolu.</param>
    /// <param name="missingFile">Fabrique appelée uniquement si le chemin absolu ne désigne pas un fichier existant.</param>
    /// <remarks>La validation du chemin absolu précède celle du fichier ; les erreurs des fabriques sont propagées.</remarks>
    void ValidateLibraryPath(string absolutePath, Func<Exception> requireAbsolute, Func<string, Exception> missingFile);

    /// <summary>Charge temporairement une bibliothèque, valide ses exports et sa version puis copie ses métadonnées.</summary>
    /// <typeparam name="T">Résultat du module, indépendant des adresses natives après le retour.</typeparam>
    /// <param name="absolutePath">Chemin absolu de la bibliothèque à inspecter.</param>
    /// <param name="expectedApiVersion">Version exacte de l’ABI attendue par l’adaptateur.</param>
    /// <param name="validateExports">Validation empruntant la bibliothèque seulement pendant l’appel ; elle ne la libère pas.</param>
    /// <param name="unsupportedApi">Fabrique d’erreur pour une version différente de celle attendue.</param>
    /// <param name="readInfo">Copie les données de SystemInfo avant la libération de la bibliothèque ; ses pointeurs sont empruntés.</param>
    /// <param name="missingExport">Contextualise une erreur d’export absent.</param>
    /// <param name="invalidLibrary">Contextualise une image de bibliothèque invalide.</param>
    /// <returns>Résultat indépendant de la bibliothèque, qui est libérée avant le retour même après une erreur.</returns>
    T InspectLibrary<T>(string absolutePath, uint expectedApiVersion, Action<IEmulationCoreLibrary> validateExports, Func<uint, Exception> unsupportedApi,
        Func<ExternalCoreApi.SystemInfo, T> readInfo, Func<EntryPointNotFoundException, Exception> missingExport, Func<BadImageFormatException, Exception> invalidLibrary);

    /// <summary>Valide l’identité du cœur puis copie les champs natifs selon les règles du module.</summary>
    /// <typeparam name="TExpected">Type de sélection de l’émulateur défini dans Common.</typeparam>
    /// <typeparam name="TInfo">Contrat de métadonnées produit par Common.</typeparam>
    /// <param name="nativeInfo">Structure empruntée, dont les chaînes doivent rester valides pendant la copie.</param>
    /// <param name="expected">Sélection utilisée pour contrôler l’identité et construire le résultat.</param>
    /// <param name="expectedNameFor">Fournit le nom attendu, comparé sans distinction de casse ordinale.</param>
    /// <param name="parseExtensions">Copie les extensions seulement après validation de l’identité.</param>
    /// <param name="identityMismatch">Fabrique d’erreur recevant le nom attendu puis le nom reçu.</param>
    /// <param name="create">Construit le résultat avec la sélection, les chaînes copiées, les extensions et les deux indicateurs natifs.</param>
    /// <returns>Métadonnées créées après validation ; les erreurs des callbacks et leur ordre sont conservés.</returns>
    TInfo ReadInfo<TExpected, TInfo>(ExternalCoreApi.SystemInfo nativeInfo, TExpected expected, Func<TExpected, string> expectedNameFor, Func<nint, IReadOnlySet<string>> parseExtensions,
        Func<string, string, Exception> identityMismatch, Func<TExpected, string, string, IReadOnlySet<string>, bool, bool, TInfo> create);

    /// <summary>Copie la liste d’extensions UTF-8 native, séparée par des barres verticales, sous sa normalisation commune.</summary>
    /// <param name="value">Pointeur emprunté vers le texte terminé par zéro ; zéro représente une liste vide.</param>
    /// <returns>Ensemble indépendant utilisant la comparaison ordinale sans distinction de casse, sans les points initiaux des extensions.</returns>
    HashSet<string> ParseExtensions(nint value);

    /// <summary>Crée le propriétaire d’un catalogue d’options avec les descriptions et règles du module.</summary>
    /// <typeparam name="TOption">Description d’option adaptée par les délégués empruntés.</typeparam>
    /// <typeparam name="TValue">Description d’un choix de valeur dans le catalogue du module.</typeparam>
    /// <param name="configured">Valeurs configurées copiées à la construction, avec leur identité de clés.</param>
    /// <param name="keyFor">Lit la clé stable d’une description.</param>
    /// <param name="defaultFor">Lit sa valeur par défaut.</param>
    /// <param name="choicesFor">Lit les choix acceptés ; une liste vide conserve les règles de valeur libre existantes.</param>
    /// <param name="valueFor">Lit l’identité d’un choix de valeur.</param>
    /// <param name="withCurrent">Produit une description actualisée avec sa valeur sélectionnée.</param>
    /// <param name="withVisibility">Produit une description actualisée avec sa visibilité native.</param>
    /// <param name="initialize">Applique la valeur et la visibilité initiales selon la politique du module.</param>
    /// <param name="present">Présentation finale facultative des descriptions ; null pour les conserver.</param>
    /// <returns>Propriétaire des valeurs natives, à libérer après la fin des callbacks de la session.</returns>
    IEmulationCoreOptionHost<TOption> CreateCoreOptionHost<TOption, TValue>(IReadOnlyDictionary<string, string> configured, Func<TOption, string> keyFor, Func<TOption, string> defaultFor,
        Func<TOption, IReadOnlyList<TValue>> choicesFor, Func<TValue, string> valueFor, Func<TOption, string, TOption> withCurrent, Func<TOption, bool, TOption> withVisibility,
        Func<TOption, string, bool, TOption> initialize, Func<TOption, TOption>? present = null) where TOption : class;

    /// <summary>Crée le propriétaire d’options déclarées par les commandes natives de variables.</summary>
    /// <typeparam name="TOption">Description d’option produite par le module.</typeparam>
    /// <param name="values">Valeurs configurées, copiées sans modification du dictionnaire reçu.</param>
    /// <param name="createOption">Produit une description à partir de la clé, du nom, du défaut, de la valeur courante et des choix natifs copiés.</param>
    /// <param name="accepts">Valide une nouvelle valeur selon les descriptions du module.</param>
    /// <param name="withCurrent">Produit la description actualisée après un changement de valeur.</param>
    /// <returns>Propriétaire à libérer par la session ou à transférer explicitement à son hôte d’environnement.</returns>
    IEmulationVariableOptionHost<TOption> CreateVariableOptionHost<TOption>(IReadOnlyDictionary<string, string> values,
        Func<string, string, string, string, IReadOnlyList<string>, TOption> createOption, Func<TOption, string, bool> accepts, Func<TOption, string, TOption> withCurrent);

    /// <summary>Crée un propriétaire indépendant des callbacks audio stéréo et de leur file bornée.</summary>
    /// <returns>Propriétaire dont le cœur emprunte les délégués ; la session le libère après l’arrêt des callbacks.</returns>
    IEmulationAudioCallbackHost CreateAudioCallbackHost();

    /// <summary>Crée un propriétaire indépendant du callback vidéo et de sa dernière image copiée.</summary>
    /// <returns>Propriétaire à libérer par la session après le dernier callback du cœur.</returns>
    IEmulationVideoCallbackHost CreateVideoCallbackHost();

    /// <summary>Crée le propriétaire des commandes d’environnement et de leurs ressources natives.</summary>
    /// <param name="video">Propriétaire vidéo emprunté, actualisé par les annonces de format et de géométrie.</param>
    /// <param name="audio">Propriétaire audio emprunté, actualisé par les annonces de fréquence.</param>
    /// <param name="options">Valeurs configurées transmises à la fabrique d’options du module.</param>
    /// <param name="systemDirectory">Dossier système de la session, créé si nécessaire et exposé au cœur.</param>
    /// <param name="contentDirectory">Dossier de contenu de la session, créé si nécessaire et exposé au cœur.</param>
    /// <param name="saveDirectory">Dossier des sauvegardes, créé si nécessaire et exposé au cœur.</param>
    /// <param name="assetsDirectory">Dossier des assets de la session, créé si nécessaire.</param>
    /// <param name="createOptions">Fabrique empruntée ; le résultat qu’elle crée est possédé puis libéré par l’hôte d’environnement.</param>
    /// <returns>Propriétaire de ses options, chaînes et VFS ; les propriétaires audio et vidéo restent à l’appelant.</returns>
    /// <remarks>Une construction interrompue libère les seules ressources déjà acquises. Les dossiers reçus
    /// restent sous la responsabilité de la session et ne sont pas supprimés par cet hôte.</remarks>
    IEmulationEnvironmentCallbackHost CreateEnvironmentCallbackHost(IEmulationVideoCallbackHost video, IEmulationAudioCallbackHost audio, IReadOnlyDictionary<string, string> options,
        string systemDirectory, string contentDirectory, string saveDirectory, string assetsDirectory, Func<IReadOnlyDictionary<string, string>, IEmulationVariableOptionHost<EmulationEnabledOption>> createOptions);
}
