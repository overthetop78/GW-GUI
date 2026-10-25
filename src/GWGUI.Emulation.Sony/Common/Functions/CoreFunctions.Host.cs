using CommonCoreHostFunctions = GWGUI.Emulation.Sony.Common.Functions.CoreHostFunctions;
using GWGUI.Emulation;
using GWGUI.Emulation.Interfaces;
using System.Buffers;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.IO.Pipes;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text.Json;

namespace GWGUI.Emulation.Sony.Common.Functions;

/// <summary>Accès commun au contrat de transport et de durée de vie des hôtes de cœur.</summary>
internal static class CoreHostFunctions
{

    /// <summary>Fournit les préparations et données de formats par le contrat public du SDK, qui relaie vers MediaEngine.</summary>
    /// <remarks>Le service est emprunté. Common conserve les chemins de session, les choix du cœur et ses diagnostics.
    /// Les formats et tailles de secteurs viennent du catalogue partagé ; les documents CUE restent opaques au module.
    /// Les flux fournis par l'appelant restent ouverts ; les flux créés pour les préparations sont fermés dans finally.</remarks>
    internal static IEmulationMediaProcessing MediaProcessing => EmulationSdk.MediaProcessing;

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


    /// <summary>Fournit le contrat commun de transport, de statut et de durée de vie des hôtes.</summary>
    /// <value>Service emprunté du SDK, indépendant du nom du module et de son adaptateur.</value>
    /// <remarks>Les adaptateurs passent par cet accès Common pour appeler les codecs et les opérations de session.
    /// Le service expose uniquement son interface publique ; il conserve les implémentations et limites du protocole
    /// dans Emulation. Les flux, vues, processus et cœurs restent possédés par la session appelante, qui les ferme
    /// dans son finally et attend l’arrêt effectif du cœur et de son hôte. Cet accès ne crée aucune session,
    /// ne mémorise aucune ressource reçue et ne requiert aucun enregistrement du nom de module dans le SDK.
    /// Les codecs propres au module conservent leur configuration et leurs diagnostics.</remarks>
    internal static IEmulationHostProtocol Protocol => EmulationSdk.HostProtocol;

    /// <summary>Écrit la version commune puis la commande ; les arguments suivent cet en-tête.</summary>
    internal static void WriteRequestHeader(BinaryWriter writer, HostCommand command) => Protocol.WriteHeader(writer, CoreHostConstants.ProtocolVersion, (byte)command);
    /// <summary>Valide la version avant de consommer la commande.</summary>
    internal static HostCommand ReadRequestHeader(BinaryReader reader) => (HostCommand)Protocol.ReadHeader(reader, ValidateProtocolVersion);
    /// <summary>Écrit la version commune puis le statut précédant le résultat ou le diagnostic.</summary>
    internal static void WriteResponseHeader(BinaryWriter writer, HostResponseStatus status) => Protocol.WriteHeader(writer, CoreHostConstants.ProtocolVersion, (byte)status);
    /// <summary>Valide la version avant de consommer le statut de réponse.</summary>
    internal static HostResponseStatus ReadResponseHeader(BinaryReader reader) => (HostResponseStatus)Protocol.ReadHeader(reader, ValidateProtocolVersion);
    /// <summary>Refuse un flux incompatible avant de lire ses données ; le message standard provient de .NET.</summary>
    internal static void ValidateProtocolVersion(int version)
    {
        if (version != CoreHostConstants.ProtocolVersion) throw new InvalidDataException();
    }

    internal static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    internal static void WriteString(BinaryWriter writer, string? value) => CommonCoreHostFunctions.Protocol.WriteString(writer, value);
    internal static string? ReadString(BinaryReader reader) => CommonCoreHostFunctions.Protocol.ReadString(reader);
    internal static void WriteBytes(BinaryWriter writer, ReadOnlySpan<byte> bytes) => CommonCoreHostFunctions.Protocol.WriteBytes(writer, bytes);
    internal static byte[] ReadBytes(BinaryReader reader) => CommonCoreHostFunctions.Protocol.ReadBytes(reader, CoreHostConstants.HostName);
    internal static void WriteInput(BinaryWriter writer, EmulationInputSnapshot input) => CommonCoreHostFunctions.Protocol.WriteInput(writer, input);
    internal static EmulationInputSnapshot ReadInput(BinaryReader reader) => CommonCoreHostFunctions.Protocol.ReadInput(reader, CoreHostConstants.HostName);
    internal static void WriteSharedFrame(BinaryWriter writer, VideoFrame? frame, MemoryMappedViewAccessor videoMap) => CommonCoreHostFunctions.Protocol.WriteSharedFrame(writer, frame, videoMap, CoreHostConstants.HostName);
    internal static VideoFrame? ReadSharedFrame(BinaryReader reader, MemoryMappedViewAccessor videoMap) => CommonCoreHostFunctions.Protocol.ReadSharedFrame(reader, videoMap, CoreHostConstants.HostName);
    internal static IReadOnlyList<AudioChunk> ReadAudio(BinaryReader reader) => CommonCoreHostFunctions.Protocol.ReadAudio(reader, CoreHostConstants.HostName);
    internal static void WriteLedStates(BinaryWriter writer, IReadOnlyDictionary<int, bool> states) => CommonCoreHostFunctions.Protocol.WriteLedStates(writer, states);
    internal static IReadOnlyDictionary<int, bool> ReadLedStates(BinaryReader reader) => CommonCoreHostFunctions.Protocol.ReadLedStates(reader, CoreHostConstants.HostName);
    /// <summary>Projette l’exception vers le contrat complet, en gardant ses détails natifs.</summary>
    /// <param name="error">Erreur empruntée ; son type, son texte et sa cause restent inchangés.</param>
    /// <returns>Catégorie et code publics lorsque l’erreur en dispose, diagnostic général sinon.</returns>
    internal static HostError CreateError(Exception error) => error is EmulationMessageException structured
        ? new HostError(error.GetType().FullName ?? error.GetType().Name, error.ToString(), structured.MessageData.Category, structured.MessageData.MessageCode, new Dictionary<string, string>(), false)
        : new HostError(error.GetType().FullName ?? error.GetType().Name, error.ToString(), null, null, new Dictionary<string, string>(), false);

    /// <summary>Écrit le diagnostic complet après l’en-tête d’échec, via le codec public partagé.</summary>
    /// <param name="writer">Rédacteur emprunté de la réponse ; reste ouvert.</param>
    /// <param name="error">Exception dont Common conserve les détails et les codes disponibles.</param>
    internal static void WriteError(BinaryWriter writer, Exception error, IReadOnlyList<string> diagnostics) => Protocol.WriteError(writer, CreateError(error), JsonOptions, diagnostics);
    /// <summary>Lit les six champs du diagnostic complet avec les conventions JSON du module.</summary>
    /// <param name="reader">Lecteur emprunté de la réponse ; reste ouvert.</param>
    /// <returns>Diagnostic à restituer par le client Common.</returns>
    internal static HostError ReadError(BinaryReader reader) => Protocol.ReadError<EmulationMessageCategory, EmulationMessageCode>(reader, JsonOptions, static () => new InvalidDataException());
}
