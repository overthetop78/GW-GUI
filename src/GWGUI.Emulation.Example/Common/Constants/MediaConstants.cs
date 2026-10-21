namespace GWGUI.Emulation.Example.Common.Constants;


/// <summary>Conventions de représentation des extensions de médias et de leur ordre de montage.</summary>
/// <remarks>Le séparateur technique est utilisé dans les listes reçues du cœur ; le séparateur lisible
/// sert à présenter les extensions dans les diagnostics. L’ordre de montage est une position logique,
/// indépendante du chemin du fichier et de son contenu.</remarks>
public static class MediaConstants
{
    /// <summary>Séparateur technique entre les extensions annoncées par un cœur.</summary>
    internal const char SupportedExtensionSeparator = '|';
    /// <summary>Caractère précédant le nom d’une extension de fichier.</summary>
    internal const char ExtensionPrefix = '.';
    /// <summary>Séparateur lisible entre les extensions présentées dans un diagnostic.</summary>
    internal const string ExtensionListSeparator = ", ";
    /// <summary>Position initiale d’un média lorsqu’aucun autre ordre de montage n’est indiqué.</summary>
    public const int DefaultMountOrder = 0;
    /// <summary>Extension des fichiers de mémoire sauvegardée conservés entre les sessions.</summary>
    internal const string BatterySaveExtension = ".sav";
    /// <summary>Extension des descriptions CUE référençant les pistes d’une image optique.</summary>
    internal const string CueExtension = ".cue";
    /// <summary>Extension des captures de flux SCP, à convertir lorsque le cœur demande une image sectorielle.</summary>
    /// <remarks>Cette extension identifie le conteneur de capture ; elle ne désigne pas le format logique de la disquette.</remarks>
    internal const string ScpExtension = ".scp";
    /// <summary>Taille maximale en octets d’un fichier de mémoire sauvegardée : 16 Mio.</summary>
    internal const int MaximumBatterySaveBytes = 16 * 1024 * 1024;
    /// <summary>Identifiant natif de la mémoire sauvegardée dans les opérations du cœur.</summary>
    internal const uint BatteryMemoryId = 0;
    /// <summary>Taille maximale de 12 288 octets pour le profil de validation de ROM à mots de deux octets.</summary>
    /// <remarks>Cette limite décrit ce profil de contenu ; elle ne limite pas toutes les images ou ROM du module.</remarks>
    internal const int MaximumRomBytes = 12288;
    /// <summary>Nombre d’octets par instruction du profil ROM validé ; sa taille doit être un multiple de deux.</summary>
    internal const int RomInstructionBytes = 2;
    /// <summary>Empreinte MD5 du contenu reconnu par ce profil de validation de ROM.</summary>
    /// <remarks>Le contrôle existant distingue un contenu connu d’un contenu seulement conforme aux contraintes de taille.
    /// Cette empreinte est une donnée de reconnaissance du profil, pas un identifiant de module.</remarks>
    internal const string RomMd5 = "3fce172403c27274e59723bbb9f6d8e9";
    /// <summary>Extensions des images de cartouche représentées par les profils du module.</summary>
    /// <remarks>Cette liste regroupe les formats existants de cette catégorie ; la description du lecteur ou de l'adaptateur filtre les formats selon le profil sélectionné.</remarks>
    internal static IReadOnlyList<string> CartridgeExtensions { get; } = Array.AsReadOnly<string>([".ws", ".wsc", ".pc2", ".bin", ".col", ".cv", ".rom", ".zip"]);
    /// <summary>Extensions des images et descriptions de disques optiques acceptées par les profils concernés.</summary>
    /// <remarks>Cette liste regroupe les formats existants de cette catégorie ; la description du lecteur ou de l'adaptateur filtre les formats selon le profil sélectionné.</remarks>
    internal static IReadOnlyList<string> CompactDiscExtensions { get; } = Array.AsReadOnly<string>([".cue", ".zip", ".iso", ".bin"]);
    /// <summary>Extensions des images de disquettes proposées par les profils concernés.</summary>
    /// <remarks>Cette liste regroupe les formats existants de cette catégorie ; la description du lecteur ou de l'adaptateur filtre les formats selon le profil sélectionné.</remarks>
    internal static IReadOnlyList<string> FloppyExtensions { get; } = Array.AsReadOnly<string>([".dsk"]);
    /// <summary>Extensions des images de cassette de données de type DataPack.</summary>
    /// <remarks>Cette liste regroupe les formats existants de cette catégorie ; la description du lecteur ou de l'adaptateur filtre les formats selon le profil sélectionné.</remarks>
    internal static IReadOnlyList<string> DataPackExtensions { get; } = Array.AsReadOnly<string>([".ddp"]);
    /// <summary>Images de cassette proposées par les lecteurs de démonstration.</summary>
    internal static IReadOnlyList<string> CassetteExtensions { get; } = Array.AsReadOnly<string>([".wav", ".tap", ".cas", ".cdt", ".tzx", ".uef", ".ddp"]);
    /// <summary>Images de disque dur pouvant être montées par les profils fictifs.</summary>
    internal static IReadOnlyList<string> HardDiskExtensions { get; } = Array.AsReadOnly<string>([".img", ".ima", ".hdf", ".vhd", ".vhdx"]);
    /// <summary>Images de carte mémoire utilisées par la simulation, sans écriture dans le contenu utilisateur.</summary>
    internal static IReadOnlyList<string> MemoryCardExtensions { get; } = Array.AsReadOnly<string>([".sav", ".mcd", ".mem", ".bin"]);
}

