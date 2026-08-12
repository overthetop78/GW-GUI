namespace GWGUI.MediaEngine.FileSystems.Coherent;

/// <summary>Construit les avertissements techniques produits pendant l'exploration d'un volume COHERENT.</summary>
internal static class CoherentWarnings
{
    /// <summary>Signale un bloc direct absent ou extérieur à l'image.</summary>
    public static string DirectBlockUnavailable(string name, int block) => $"{name}: le bloc direct COHERENT {block} est absent ou hors image.";
    /// <summary>Signale un bloc indirect absent ou extérieur à l'image.</summary>
    public static string IndirectBlockUnavailable(string name, long block, int level) => $"{name}: le bloc indirect COHERENT {block} de niveau {level} est absent ou hors image.";
    /// <summary>Signale une référence indirecte cyclique ou répétée.</summary>
    public static string IndirectBlockRepeated(string name, int block, int level) => $"{name}: le bloc indirect COHERENT {block} de niveau {level} est référencé plusieurs fois.";
    /// <summary>Signale un cycle dans l'arborescence de répertoires.</summary>
    public static string DirectoryCycle(string name, ushort inode) => $"{name}: l'inode de répertoire COHERENT {inode} forme un cycle.";
    /// <summary>Signale une seconde référence vers un répertoire déjà parcouru.</summary>
    public static string DirectoryRepeated(string name, ushort inode) => $"{name}: l'inode de répertoire COHERENT {inode} a déjà été parcouru.";
    /// <summary>Signale l'échec de lecture d'un inode enfant.</summary>
    public static string ChildInodeUnreadable(string name, InvalidDataException exception) => $"{name}: l'inode enfant COHERENT ne peut pas être lu ({exception.Message}).";
    /// <summary>Signale des octets conservés sous forme de zéros faute de bloc lisible.</summary>
    public static string MissingBytes(string name, int length) => $"{name}: {length} octet(s) COHERENT sont absents et restent positionnés à zéro.";
}
