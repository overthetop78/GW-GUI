using GWGUI.Emulation.Contracts;

namespace GWGUI.Emulation.Example.Common.Constants;

/// <summary>Extensions des contenus proposés par le module et ordre initial de leurs montages.</summary>
/// <remarks>Les listes de sélection emploient le point initial de l'extension. La description de chaque
/// lecteur retient les formats compatibles avec le modèle. L'ordre de montage désigne une position logique
/// dans la configuration, indépendante du chemin et du contenu du fichier.</remarks>
public static class MediaConstants
{
    /// <summary>Caractère précédant le nom d’une extension de fichier.</summary>
    internal const char ExtensionPrefix = '.';
    /// <summary>Position initiale d’un média lorsqu’aucun autre ordre de montage n’est indiqué.</summary>
    public const int DefaultMountOrder = 0;
    /// <summary>Extensions des images de cartouche représentées par les profils du module.</summary>
    /// <remarks>Cette liste regroupe les formats existants de cette catégorie ; la description du lecteur ou de l'adaptateur filtre les formats selon le profil sélectionné.</remarks>
    internal static IReadOnlyList<string> CartridgeExtensions { get; } = Array.AsReadOnly<string>([".ws", ".wsc", ".pc2", ".bin", ".col", ".cv", ".rom", ".zip"]);
    /// <summary>Extensions des images de disquettes proposées par les profils concernés.</summary>
    /// <remarks>Cette liste regroupe les formats existants de cette catégorie ; la description du lecteur ou de l'adaptateur filtre les formats selon le profil sélectionné.</remarks>
    internal static IReadOnlyList<string> FloppyExtensions { get; } = Array.AsReadOnly<string>([".dsk"]);
    /// <summary>Images de cassette proposées par les lecteurs de démonstration.</summary>
    internal static IReadOnlyList<string> CassetteExtensions { get; } = Array.AsReadOnly<string>([".wav", ".tap", ".cas", ".cdt", ".tzx", ".uef", ".ddp"]);
    /// <summary>Images de disque dur pouvant être montées par les profils fictifs.</summary>
    internal static IReadOnlyList<string> HardDiskExtensions { get; } = Array.AsReadOnly<string>([".img", ".ima", ".hdf", VhdExtension, VhdxExtension]);
    /// <summary>Extension retenue par le module pour créer une image dans le conteneur VHD.</summary>
    internal const string VhdExtension = ".vhd";
    /// <summary>Extension retenue par le module pour créer une image dans le conteneur VHDX.</summary>
    internal const string VhdxExtension = ".vhdx";
    /// <summary>Images de carte mémoire utilisées par la simulation, sans écriture dans le contenu utilisateur.</summary>
    internal static IReadOnlyList<string> MemoryCardExtensions { get; } = Array.AsReadOnly<string>([".sav", ".mcd", ".mem", ".bin"]);

    /// <summary>Emplacements qui imposent une recréation dans la politique de base du module.</summary>
    /// <remarks>Cette liste appartient au module et reste protégée contre les modifications. Une liste vide autorise les échanges à chaud de base.</remarks>
    internal static IReadOnlyList<EmulationMediaSlot> RestartSlots { get; } = Array.AsReadOnly<EmulationMediaSlot>([]);
}

