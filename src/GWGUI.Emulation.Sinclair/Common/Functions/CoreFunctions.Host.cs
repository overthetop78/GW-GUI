using System.Reflection;
using System.Buffers;
using GWGUI.Emulation.Interfaces;
using System.IO;
using System.Globalization;
using System.IO.MemoryMappedFiles;
using System.Numerics;
using System.Runtime.Versioning;
using System.Text.Json;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Sinclair.Common.Functions;
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

    /// <summary>Crée le catalogue de textes du module par le contrat public des messages.</summary>
    /// <param name="assembly">Assembly emprunté qui contient les ressources du module.</param>
    /// <param name="resourcePrefix">Préfixe exact de ses ressources embarquées.</param>
    /// <returns>Catalogue indépendant sous son interface, avec le cache et les règles de repli du SDK.</returns>
    /// <remarks>Le module conserve ses clés et son préfixe. La culture est fournie à chaque recherche ; les cultures parentes, les traductions de repli et la base commune sont parcourues par le même service. Les flux temporaires de ressources sont libérés avant le retour ; ce catalogue ne possède ni assembly chargé ni session de cœur.</remarks>
    internal static IEmulationModuleLocalization CreateLocalization(Assembly assembly, string resourcePrefix) => Messages.CreateLocalization(assembly, resourcePrefix);

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

    internal static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    internal static string CreatePipeName() => CoreHostConstants.PipePrefix + Guid.NewGuid().ToString(CoreHostConstants.UniqueNameFormat);
    internal static string CreateVideoMapName() => CoreHostConstants.VideoMapPrefix + Guid.NewGuid().ToString(CoreHostConstants.UniqueNameFormat);
    internal static void WriteRequestHeader(BinaryWriter writer, HostCommand command) => Protocol.WriteHeader(writer, CoreHostConstants.ProtocolVersion, (byte)command);

    internal static HostCommand ReadRequestHeader(BinaryReader reader) => (HostCommand)Protocol.ReadHeader(reader, ValidateProtocolVersion);

    internal static void WriteResponseHeader(BinaryWriter writer, HostResponseStatus status) => Protocol.WriteHeader(writer, CoreHostConstants.ProtocolVersion, (byte)status);

    internal static HostResponseStatus ReadResponseHeader(BinaryReader reader) => (HostResponseStatus)Protocol.ReadHeader(reader, ValidateProtocolVersion);

    internal static void ValidateProtocolVersion(int version)
    {
        if (version == CoreHostConstants.ProtocolVersion)
            return;
        throw new InvalidDataException(string.Format(CultureInfo.InvariantCulture, CoreHostErrors.ProtocolVersionMismatchFormat, version, CoreHostConstants.ProtocolVersion));
    }

    internal static HostError CreateError(Exception error) => error is EmulationException structured
        ? new HostError(error.GetType().FullName ?? error.GetType().Name, error.Message, structured.Category, structured.Code, structured.Context, structured.IsLocalized)
        : new HostError(error.GetType().FullName ?? error.GetType().Name, error.Message, null, null, new Dictionary<string, string>(), false);
    internal static void WriteError(BinaryWriter writer, Exception error) => Protocol.WriteError(writer, CreateError(error), JsonOptions);
    internal static HostError ReadError(BinaryReader reader) => Protocol.ReadError<ErrorCategory, ErrorCode>(reader, JsonOptions, static () => new InvalidDataException(CoreHostErrors.CommunicationFailed));
    internal static string? ReadString(BinaryReader reader) => CoreHostFunctions.Protocol.ReadString(reader);
    internal static void WriteBytes(BinaryWriter writer, ReadOnlySpan<byte> value) => CoreHostFunctions.Protocol.WriteBytes(writer, value);
    internal static byte[] ReadBytes(BinaryReader reader) => CoreHostFunctions.Protocol.ReadBytes(reader, CoreHostConstants.HostName);
    internal static void WriteInput(BinaryWriter writer, EmulationInputSnapshot input) => CoreHostFunctions.Protocol.WriteInput(writer, input);
    internal static EmulationInputSnapshot ReadInput(BinaryReader reader) => CoreHostFunctions.Protocol.ReadInput(reader, CoreHostConstants.HostName);
    internal static int CalculateVideoSlotCapacity(int frameLength) => CoreHostFunctions.Protocol.CalculateVideoSlotCapacity(
        frameLength, CoreHostConstants.MinimumVideoSlotCapacity, CoreHostConstants.HostName);

    [SupportedOSPlatform(CoreHostConstants.Windows)]
    internal static void WriteResizableSharedFrame(BinaryWriter writer, VideoFrame? frame, SharedVideoWriter video) =>
        CoreHostFunctions.Protocol.WriteResizableSharedFrame(writer, frame, video);

    [SupportedOSPlatform(CoreHostConstants.Windows)]
    internal static VideoFrame? ReadResizableSharedFrame(BinaryReader reader, ref MemoryMappedFile? memory, ref MemoryMappedViewAccessor? view, ref string? activeName) =>
        CoreHostFunctions.Protocol.ReadResizableSharedFrame(reader, ref memory, ref view, ref activeName, CoreHostConstants.MinimumVideoSlotCapacity);
    internal static IReadOnlyList<AudioChunk> ReadAudio(BinaryReader reader) => CoreHostFunctions.Protocol.ReadAudio(reader, CoreHostConstants.HostName);
    internal static void WriteLedStates(BinaryWriter writer, IReadOnlyDictionary<int, bool> states) => CoreHostFunctions.Protocol.WriteLedStates(writer, states);
    internal static IReadOnlyDictionary<int, bool> ReadLedStates(BinaryReader reader) => CoreHostFunctions.Protocol.ReadLedStates(reader, CoreHostConstants.HostName);
    internal static void WriteDiskStatus(BinaryWriter writer, DiskStatus status) => writer.Write(JsonSerializer.Serialize(status, JsonOptions));
    internal static DiskStatus ReadDiskStatus(BinaryReader reader) => JsonSerializer.Deserialize<DiskStatus>(reader.ReadString(), JsonOptions) ?? throw new InvalidDataException(CoreHostErrors.CommunicationFailed);
}
