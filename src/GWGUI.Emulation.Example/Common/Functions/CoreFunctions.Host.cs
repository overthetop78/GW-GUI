using System.Buffers;
using GWGUI.Emulation.Interfaces;
using System.IO;
using System.Globalization;
using System.IO.MemoryMappedFiles;
using System.Numerics;
using System.Runtime.Versioning;
using System.Text.Json;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Example.Common.Functions;

/// <summary>Lecture et écriture du protocole reliant Common au processus d’hôte d’un cœur.</summary>
/// <remarks>Les lecteurs et rédacteurs sont empruntés : ces méthodes ne ferment pas le transport de l’appelant.
/// Le client et l’hôte doivent employer les mêmes codes, version, conventions JSON et ordre de champs binaires.
/// Les opérations sur mémoire vidéo peuvent remplacer les ressources possédées par leur appelant ; celui-ci
/// les libère dans son finally de session après la fin effective des échanges.</remarks>
internal static class CoreHostFunctions
{
    /// <summary>Fournit les opérations de configuration et de fichiers par leur interface publique commune.</summary>
    /// <remarks>Le module conserve ses chemins, verrous, projections et annulation ; le service possède les flux qu’il ouvre.</remarks>
    internal static IEmulationConfigurationFiles ConfigurationFiles => EmulationSdk.ConfigurationFiles;

    /// <summary>Fournit les sauvegardes et empreintes partagées par leur interface publique.</summary>
    /// <remarks>Le module choisit les données à identifier et conserve la propriété des flux empruntés.
    /// Les algorithmes, la sérialisation canonique et les vérifications de format restent dans Emulation.</remarks>
    internal static IEmulationStateFiles StateFiles => EmulationSdk.StateFiles;

    /// <summary>Fournit les règles communes de lecteurs, montages et documents de médias par leur interface publique.</summary>
    /// <remarks>Les lecteurs et chemins restent décrits par Common. Ce service ne possède ni fichier ni lecteur physique et ne remplace pas les contraintes spécifiques du module.</remarks>
    internal static IEmulationMediaRules MediaRules => EmulationSdk.MediaRules;

    /// <summary>Fournit les parcours de firmware par le contrat public commun.</summary>
    /// <remarks>Le module fournit son inspecteur et conserve ses candidats ; le service conserve les parcours, tris et annulations existants.</remarks>
    internal static IEmulationFirmwareProcessing FirmwareProcessing => EmulationSdk.FirmwareProcessing;

    /// <summary>Fournit la découverte des adaptateurs et la projection des catalogues par leur contrat public.</summary>
    /// <remarks>Les définitions et les identités restent déclarées par le module ; aucun registre de modules connus n’est nécessaire.</remarks>
    internal static IEmulationAdapters Adapters => EmulationSdk.Adapters;

    /// <summary>Fournit le cycle de vie commun par le contrat public d’exécution.</summary>
    /// <remarks>La machine conserve ses propriétaires ; elle attend leur arrêt avant le nettoyage protégé par finally.</remarks>
    internal static IEmulationMachineExecution MachineExecution => EmulationSdk.MachineExecution;

    /// <summary>Fournit les opérations vidéo communes par leur interface publique.</summary>
    /// <remarks>Les copies conservent leur allocation existante ; les pointeurs et surfaces appartiennent à leur appelant.</remarks>
    internal static IEmulationVideoProcessing VideoProcessing => EmulationSdk.VideoProcessing;

    /// <summary>Fournit la contextualisation et les erreurs publiques par le contrat commun des messages.</summary>
    /// <remarks>Le service emprunté conserve les causes et les identités ; il ne possède ni machine ni cœur.</remarks>
    internal static IEmulationMessages Messages => EmulationSdk.Messages;

    /// <summary>Fournit la construction et le formatage communs des réglages matériels et audio.</summary>
    /// <value>Service emprunté, sans configuration ni caractéristique de machine stockée dans cet accès Common.</value>
    /// <remarks>Le module conserve ses caractéristiques, choix et ressources spécifiques, et les transmet
    /// au contrat commun. Le SDK construit les mêmes champs et blocs avec leurs clés partagées ; les valeurs
    /// invariantes ne sont pas recopiées dans les traductions. Les délégués restent empruntés pendant l’appel,
    /// et leurs lectures, les cultures de formatage et les valeurs de repli conservent leur ordre existant.</remarks>
    internal static IEmulationHardwareSettings HardwareSettings => EmulationSdk.HardwareSettings;

    /// <summary>Fournit le contrat commun des structures, commandes et callbacks d’environnement natifs.</summary>
    /// <value>Service emprunté, sans propriétaire de cœur, bibliothèque ou état de session dans cet accès Common.</value>
    /// <remarks>Les adaptateurs transmettent leurs opérations, bornes et politiques au même traitement SDK.
    /// Les textes reçus sont copiés ; les pointeurs de code, exports et plages mémoire restent empruntés
    /// à leurs propriétaires. Le fournisseur conserve ses délégués pendant leur enregistrement et cesse
    /// de les utiliser avant la libération du code natif. Les buffers temporaires de lecture sont libérés
    /// par le traitement existant. Les paramètres ref/out, lectures différées et restaurations après erreur
    /// conservent leur ordre ; le service ne sélectionne pas les capacités à la place du module.</remarks>
    internal static IEmulationCoreEnvironmentProcessing EnvironmentProcessing => EmulationSdk.EnvironmentProcessing;

    /// <summary>Fournit les traitements communs d’options natives et de leurs champs de réglage.</summary>
    /// <value>Service emprunté, sans catalogue ni ressource de session conservés dans cet accès Common.</value>
    /// <remarks>Le module garde ses descriptions, clés, valeurs et choix de présentation. Il fournit leurs
    /// lecteurs et projections au contrat commun pour reprendre les mêmes copies et validations dans le SDK.
    /// Les tables natives restent empruntées pendant les copies ; les dictionnaires mutables et paramètres
    /// ref/out conservent leurs effets sur les données du module. Un callback natif retourné reste emprunté
    /// au code du cœur : son propriétaire cesse de l’appeler avant de décharger ce code.
    /// Cet accès porte le même nom dans chaque Common et ne dépend d’aucune identité de module connue du SDK.</remarks>
    internal static IEmulationCoreOptionProcessing OptionProcessing => EmulationSdk.OptionProcessing;

    /// <summary>Fournit les allocations natives communes sous leur contrat public.</summary>
    /// <value>Service emprunté du SDK ; il ne possède aucune ressource de session et ne doit pas être détruit.</value>
    /// <remarks>Les adaptateurs demandent ici leurs chaînes et descripteurs de contenu, au lieu de construire
    /// les classes internes du SDK. Chaque résultat appartient à l’appelant jusqu’à un transfert explicite.
    /// La session libère ses résultats dans son finally, après le dernier emprunt par le cœur.
    /// AddString transfère la chaîne au dictionnaire uniquement si son ajout réussit ; les autres créations
    /// laissent la propriété au code appelant. Les pointeurs exposés restent des emprunts et ne sont pas libérés
    /// directement par l’adaptateur. Capacités, encodages et mode chemin seul restent choisis par le module.</remarks>
    internal static IEmulationNativeResources NativeResources => EmulationSdk.NativeResources;

    /// <summary>Crée le jeu tournant de tableaux vidéo de la session par le contrat public du SDK.</summary>
    /// <param name="bufferCount">Nombre d’emplacements choisi par le module ; chaque emplacement peut retenir un tableau loué.</param>
    /// <param name="pool">Pool emprunté, ou null pour le pool partagé ; le propriétaire du jeu ne détruit pas ce pool.</param>
    /// <returns>Jeu possédé par l’appelant, à libérer après la dernière utilisation des images dans son finally.</returns>
    internal static IEmulationVideoBufferSet CreateVideoBufferSet(int bufferCount, ArrayPool<byte>? pool = null) => EmulationSdk.CreateVideoBufferSet(bufferCount, pool);

    /// <summary>Crée le propriétaire des segments vidéo partagés par le contrat public du SDK.</summary>
    /// <param name="namePrefix">Préfixe de session transmis au lecteur distant.</param>
    /// <param name="capacity">Politique existante du module, qui calcule la capacité en octets d’un emplacement.</param>
    /// <param name="separator">Séparateur entre le préfixe de session et le nom unique de génération.</param>
    /// <param name="nameFormat">Format de l’identifiant unique, conservé tel quel par la fabrique.</param>
    /// <returns>Rédacteur possédé par l’appelant ; il libère ses vues et segments lors de Dispose.</returns>
    /// <remarks>La session appelle Dispose dans son finally après la dernière écriture. Les vues exposées par
    /// l’interface restent empruntées aux codecs : ceux-ci ne les ferment pas et ne les conservent pas après remplacement.</remarks>
    internal static IEmulationSharedVideoWriter CreateSharedVideoWriter(string namePrefix, Func<int, int> capacity, string separator, string nameFormat)
        => EmulationSdk.CreateSharedVideoWriter(namePrefix, capacity, separator, nameFormat);


    /// <summary>Fournit le contrat commun de transport, de statut et de durée de vie des hôtes.</summary>
    /// <value>Service emprunté du SDK, indépendant du nom du module et de son adaptateur.</value>
    /// <remarks>Les adaptateurs passent par cet accès Common pour appeler les codecs et les opérations de session.
    /// Le service expose uniquement son interface publique ; il conserve les implémentations et limites du protocole
    /// dans Emulation. Les flux, vues, processus et cœurs restent possédés par la session appelante, qui les ferme
    /// dans son finally et attend l’arrêt effectif du cœur et de son hôte. Cet accès ne crée aucune session,
    /// ne mémorise aucune ressource reçue et ne requiert aucun enregistrement du nom de module dans le SDK.
    /// Les codecs propres au module conservent leur configuration et leurs diagnostics.</remarks>
    internal static IEmulationHostProtocol Protocol => EmulationSdk.HostProtocol;

    /// <summary>Conventions JSON Web communes aux diagnostics et aux statuts de disque échangés.</summary>
    /// <remarks>Les deux extrémités utilisent cette même définition ; conserver les options stables après leur première utilisation.</remarks>
    internal static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>Produit le nom du canal de commande propre à une nouvelle session.</summary>
    /// <returns>Préfixe de module suivi d’un GUID au format configuré, indépendant des autres sessions.</returns>
    internal static string CreatePipeName() => CoreHostConstants.PipePrefix + Guid.NewGuid().ToString(CoreHostConstants.UniqueNameFormat);
    /// <summary>Produit le nom initial du bloc de vidéo partagée propre à une nouvelle session.</summary>
    /// <returns>Préfixe vidéo suivi d’un GUID ; le rédacteur peut ajouter une génération en cas de redimensionnement.</returns>
    internal static string CreateVideoMapName() => CoreHostConstants.VideoMapPrefix + Guid.NewGuid().ToString(CoreHostConstants.UniqueNameFormat);

    /// <summary>Écrit la version de protocole sur quatre octets, puis le code de commande sur un octet.</summary>
    /// <param name="writer">Rédacteur emprunté, positionné au début de l’en-tête à produire.</param>
    /// <param name="command">Commande déterminant les arguments écrits après l’en-tête.</param>
    internal static void WriteRequestHeader(BinaryWriter writer, HostCommand command)
    {
        writer.Write(CoreHostConstants.ProtocolVersion);
        writer.Write((byte)command);
    }

    /// <summary>Lit et contrôle la version, puis lit le code de commande de la requête.</summary>
    /// <param name="reader">Lecteur emprunté, positionné au début de l’en-tête.</param>
    /// <returns>Code de commande ; la répartition des commandes détermine ensuite si ce code est reconnu.</returns>
    /// <exception cref="InvalidDataException">La version reçue diffère de la version attendue.</exception>
    internal static HostCommand ReadRequestHeader(BinaryReader reader)
    {
        ValidateProtocolVersion(reader.ReadInt32());
        return (HostCommand)reader.ReadByte();
    }

    /// <summary>Écrit la version de protocole sur quatre octets, puis le statut de réponse sur un octet.</summary>
    /// <param name="writer">Rédacteur emprunté de la réponse.</param>
    /// <param name="status">Statut indiquant si la suite contient un résultat ou un diagnostic d’échec.</param>
    internal static void WriteResponseHeader(BinaryWriter writer, HostResponseStatus status)
    {
        writer.Write(CoreHostConstants.ProtocolVersion);
        writer.Write((byte)status);
    }

    /// <summary>Lit et contrôle la version, puis lit le statut de réponse.</summary>
    /// <param name="reader">Lecteur emprunté, positionné au début de la réponse.</param>
    /// <returns>Statut que l’appelant contrôle avant de lire un résultat ou une erreur.</returns>
    /// <exception cref="InvalidDataException">La version reçue diffère de la version attendue.</exception>
    internal static HostResponseStatus ReadResponseHeader(BinaryReader reader)
    {
        ValidateProtocolVersion(reader.ReadInt32());
        return (HostResponseStatus)reader.ReadByte();
    }

    /// <summary>Refuse un échange dont le format n’a pas la version attendue par cette extrémité.</summary>
    /// <param name="version">Version entière lue dans l’en-tête reçu.</param>
    /// <exception cref="InvalidDataException">Le diagnostic indique une incompatibilité de version.</exception>
    internal static void ValidateProtocolVersion(int version)
    {
        if (version == CoreHostConstants.ProtocolVersion) return;
        throw new InvalidDataException(string.Format(CultureInfo.InvariantCulture, CoreHostErrors.ProtocolVersionMismatchFormat, version, CoreHostConstants.ProtocolVersion));
    }

    /// <summary>Projette une exception vers le contrat de diagnostic sérialisable du transport.</summary>
    /// <param name="error">Exception empruntée ; son type et son message sont conservés.</param>
    /// <returns>Diagnostic avec catégorie, code, contexte et localisation pour une EmulationException ; diagnostic général sinon.</returns>
    /// <remarks>Le nom de type est une information de diagnostic : la lecture ne reconstruit pas une exception par réflexion.</remarks>
    internal static HostError CreateError(Exception error) => error is EmulationException structured
        ? new HostError(error.GetType().FullName ?? error.GetType().Name, error.Message, structured.Category, structured.Code, structured.Context, structured.IsLocalized)
        : new HostError(error.GetType().FullName ?? error.GetType().Name, error.Message, null, null, new Dictionary<string, string>(), false);

    /// <summary>Sérialise le diagnostic en JSON puis l’écrit comme chaîne binaire.</summary>
    /// <param name="writer">Rédacteur emprunté, après l’en-tête de réponse en échec.</param>
    /// <param name="error">Exception dont les données seront transportées.</param>
    internal static void WriteError(BinaryWriter writer, Exception error) => writer.Write(JsonSerializer.Serialize(CreateError(error), JsonOptions));

    /// <summary>Lit la chaîne JSON du diagnostic et reconstruit son contrat de données.</summary>
    /// <param name="reader">Lecteur emprunté, après l’en-tête de réponse en échec.</param>
    /// <returns>Données du diagnostic ; la politique de levée d’exception appartient au récepteur.</returns>
    /// <exception cref="InvalidDataException">Le JSON représente une valeur nulle au lieu d’un diagnostic.</exception>
    internal static HostError ReadError(BinaryReader reader) => JsonSerializer.Deserialize<HostError>(reader.ReadString(), JsonOptions) ?? throw new InvalidDataException(CoreHostErrors.CommunicationFailed);
    /// <summary>Lit un booléen de présence, puis une chaîne uniquement lorsque ce marqueur vaut true.</summary>
    /// <param name="reader">Lecteur emprunté d’une valeur texte facultative.</param>
    /// <returns>Chaîne reçue ou null lorsque le marqueur indique une absence.</returns>
    internal static string? ReadString(BinaryReader reader) => CoreHostFunctions.Protocol.ReadString(reader);
    /// <summary>Écrit la longueur du bloc sur quatre octets, puis ses octets.</summary>
    /// <param name="writer">Rédacteur emprunté du bloc.</param>
    /// <param name="value">Octets empruntés pendant l’appel, sans transfert de propriété.</param>
    internal static void WriteBytes(BinaryWriter writer, ReadOnlySpan<byte> value) => CoreHostFunctions.Protocol.WriteBytes(writer, value);
    /// <summary>Lit un bloc binaire en contrôlant la longueur annoncée et la présence de tous ses octets.</summary>
    /// <param name="reader">Lecteur emprunté, positionné sur la longueur du bloc.</param>
    /// <returns>Nouveau tableau possédé par l’appelant.</returns>
    /// <exception cref="InvalidDataException">La longueur est négative ou dépasse la limite du transport.</exception>
    /// <exception cref="EndOfStreamException">Le flux se termine avant la fin du bloc annoncé.</exception>
    internal static byte[] ReadBytes(BinaryReader reader) => CoreHostFunctions.Protocol.ReadBytes(reader, CoreHostConstants.HostName);
    /// <summary>Écrit les touches, le pointeur et les contrôleurs selon le format d’entrée commun.</summary>
    /// <param name="writer">Rédacteur emprunté de la requête.</param>
    /// <param name="input">Instantané emprunté, dont les collections doivent rester stables pendant l’appel.</param>
    internal static void WriteInput(BinaryWriter writer, EmulationInputSnapshot input) => CoreHostFunctions.Protocol.WriteInput(writer, input);
    /// <summary>Reconstruit les entrées en contrôlant les nombres de touches, contrôleurs et commandes.</summary>
    /// <param name="reader">Lecteur emprunté, positionné au début des données d’entrée.</param>
    /// <returns>Instantané composé des valeurs et collections lues dans le transport.</returns>
    /// <exception cref="InvalidDataException">Une quantité annoncée est hors des limites du protocole.</exception>
    internal static EmulationInputSnapshot ReadInput(BinaryReader reader) => CoreHostFunctions.Protocol.ReadInput(reader, CoreHostConstants.HostName);
    /// <summary>Calcule la capacité en octets d’un emplacement vidéo, arrondie à une puissance de deux.</summary>
    /// <param name="frameLength">Nombre d’octets de l’image à transporter.</param>
    /// <returns>Capacité au moins égale à l’image et au minimum du module.</returns>
    /// <exception cref="InvalidDataException">La longueur est nulle, négative ou supérieure au maximum du transport.</exception>
    internal static int CalculateVideoSlotCapacity(int frameLength) => CoreHostFunctions.Protocol.CalculateVideoSlotCapacity(
        frameLength, CoreHostConstants.MinimumVideoSlotCapacity, CoreHostConstants.HostName);

    /// <summary>Copie l’image dans le bloc partagé redimensionnable et écrit ses métadonnées dans la réponse.</summary>
    /// <param name="writer">Rédacteur emprunté des métadonnées de transport.</param>
    /// <param name="frame">Image empruntée ; null écrit seulement un marqueur d’absence.</param>
    /// <param name="video">Propriétaire vidéo emprunté, conservé entre les frames et libéré par le propriétaire de session.</param>
    [SupportedOSPlatform(CoreHostConstants.Windows)]
    internal static void WriteResizableSharedFrame(BinaryWriter writer, VideoFrame? frame, SharedVideoWriter video) =>
        CoreHostFunctions.Protocol.WriteResizableSharedFrame(writer, frame, video);

    /// <summary>Lit les métadonnées de vidéo partagée puis copie les pixels dans une nouvelle image.</summary>
    /// <param name="reader">Lecteur emprunté des métadonnées reçues.</param>
    /// <param name="memory">Bloc possédé par l’appelant ; l’ancien bloc est libéré si le nom reçu change.</param>
    /// <param name="view">Vue possédée par l’appelant, remplacée avec le bloc et libérée avant celui-ci.</param>
    /// <param name="activeName">Nom du bloc actuellement ouvert, mis à jour lors d’un remplacement.</param>
    /// <returns>Image dont les pixels sont copiés, ou null lorsque le marqueur indique une absence.</returns>
    /// <remarks>L’appelant libère la vue et le bloc conservés dans son finally, même après erreur de lecture.</remarks>
    [SupportedOSPlatform(CoreHostConstants.Windows)]
    internal static VideoFrame? ReadResizableSharedFrame(BinaryReader reader, ref MemoryMappedFile? memory, ref MemoryMappedViewAccessor? view, ref string? activeName) =>
        CoreHostFunctions.Protocol.ReadResizableSharedFrame(reader, ref memory, ref view, ref activeName, CoreHostConstants.MinimumVideoSlotCapacity);
    /// <summary>Lit les blocs audio annoncés et contrôle leurs dimensions et leur disposition de canaux.</summary>
    /// <param name="reader">Lecteur emprunté, positionné sur le nombre de blocs audio.</param>
    /// <returns>Blocs dont les échantillons sont contenus dans de nouveaux tableaux.</returns>
    internal static IReadOnlyList<AudioChunk> ReadAudio(BinaryReader reader) => CoreHostFunctions.Protocol.ReadAudio(reader, CoreHostConstants.HostName);
    /// <summary>Écrit le nombre de témoins d’activité puis leurs paires indice et état.</summary>
    /// <param name="writer">Rédacteur emprunté des statuts.</param>
    /// <param name="states">Dictionnaire emprunté, stable pendant l’appel.</param>
    internal static void WriteLedStates(BinaryWriter writer, IReadOnlyDictionary<int, bool> states) => CoreHostFunctions.Protocol.WriteLedStates(writer, states);
    /// <summary>Lit les témoins d’activité en contrôlant la quantité annoncée.</summary>
    /// <param name="reader">Lecteur emprunté, positionné sur le nombre de témoins.</param>
    /// <returns>Nouveau dictionnaire associant chaque indice de témoin à son état.</returns>
    internal static IReadOnlyDictionary<int, bool> ReadLedStates(BinaryReader reader) => CoreHostFunctions.Protocol.ReadLedStates(reader, CoreHostConstants.HostName);

    /// <summary>Écrit le statut multidisque sous forme de chaîne JSON.</summary>
    /// <param name="writer">Rédacteur emprunté de la réponse.</param>
    /// <param name="status">Description de la liste des disques, de la sélection et de l’éjection.</param>
    internal static void WriteDiskStatus(BinaryWriter writer, DiskStatus status) => writer.Write(JsonSerializer.Serialize(status, JsonOptions));

    /// <summary>Lit la chaîne JSON représentant le statut multidisque.</summary>
    /// <param name="reader">Lecteur emprunté, positionné sur le statut sérialisé.</param>
    /// <returns>Contrat de données du lecteur multidisque.</returns>
    /// <exception cref="InvalidDataException">Le JSON représente une valeur nulle au lieu d’un statut.</exception>
    internal static DiskStatus ReadDiskStatus(BinaryReader reader) => JsonSerializer.Deserialize<DiskStatus>(reader.ReadString(), JsonOptions) ?? throw new InvalidDataException(CoreHostErrors.CommunicationFailed);

}
