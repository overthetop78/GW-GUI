using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.FileSystems.Coherent;

/// <summary>Examine et valide les champs canoniques du superbloc COHERENT.</summary>
internal static class CoherentFormat
{
    /// <summary>Nombre d'octets d'un entier canonique 32 bits.</summary>
    public const int UInt32Length = sizeof(uint);
    /// <summary>Position de l'octet de poids faible.</summary>
    private const int LowByteOffset = 2;
    /// <summary>Position du deuxième octet.</summary>
    private const int LowMiddleByteOffset = 3;
    /// <summary>Position du troisième octet.</summary>
    private const int HighMiddleByteOffset = 0;
    /// <summary>Position de l'octet de poids fort.</summary>
    private const int HighByteOffset = 1;

    /// <summary>Indique si le contenu présente les marqueurs internes d'un superbloc COHERENT.</summary>
    /// <param name="bytes">Contenu à examiner.</param>
    /// <returns><see langword="true"/> lorsque les noms du volume et du pack sont plausibles.</returns>
    public static bool LooksLikeCoherent(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < CoherentFileSystemLayout.MinimumImageSize) return false;
        var name = System.Text.Encoding.ASCII.GetString(bytes.Slice(CoherentFileSystemLayout.VolumeNameOffset, CoherentFileSystemLayout.NameLength));
        var pack = System.Text.Encoding.ASCII.GetString(bytes.Slice(CoherentFileSystemLayout.PackNameOffset, CoherentFileSystemLayout.NameLength));
        return IsAcceptedName(name, CoherentFileSystemLayout.DefaultVolumeName, CoherentFileSystemLayout.VolumePadding) && IsAcceptedName(pack, CoherentFileSystemLayout.DefaultPackName, CoherentFileSystemLayout.PackPadding);
    }

    /// <summary>Valide la structure, l'alignement et le nombre de blocs déclaré par le superbloc.</summary>
    /// <param name="bytes">Dump COHERENT complet.</param>
    /// <returns>Nombre de blocs déclaré par le système de fichiers.</returns>
    /// <exception cref="InvalidDataException">Le superbloc, l'alignement ou le nombre de blocs est invalide.</exception>
    public static int ReadValidatedFileSystemBlockCount(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length % CoherentFileSystemLayout.BlockSize != 0 || !LooksLikeCoherent(bytes)) throw new InvalidDataException("Le contenu ne contient pas de système de fichiers COHERENT valide.");
        var fileSystemBlocks = ReadDeclaredFileSystemBlockCount(bytes);
        if (fileSystemBlocks < 3 || fileSystemBlocks > bytes.Length / CoherentFileSystemLayout.BlockSize) throw new InvalidDataException("La taille déclarée du système de fichiers COHERENT est invalide.");
        return fileSystemBlocks;
    }

    /// <summary>Lit le nombre de blocs déclaré sans appliquer les limites du dump.</summary>
    public static int ReadDeclaredFileSystemBlockCount(ReadOnlySpan<byte> bytes) => checked((int)ReadCanonicalUInt32(bytes.Slice(CoherentFileSystemLayout.FileSystemBlockCountOffset, UInt32Length)));

    /// <summary>Lit un entier 32 bits dans l'ordre canonique COHERENT 2, 3, 0, 1.</summary>
    public static uint ReadCanonicalUInt32(ReadOnlySpan<byte> value)
    {
        if (value.Length < UInt32Length) throw CoherentExceptions.InsufficientCanonicalLength(value.Length, UInt32Length, nameof(value));
        return (uint)(value[LowByteOffset] | value[LowMiddleByteOffset] << BitPrimitives.BitsPerByte | value[HighMiddleByteOffset] << 16 | value[HighByteOffset] << 24);
    }

    /// <summary>Indique si un champ contient son nom par défaut ou le marqueur de remplacement accepté.</summary>
    private static bool IsAcceptedName(string value, string defaultName, char placeholderPadding) => value == defaultName || value == CoherentFileSystemLayout.PlaceholderName + placeholderPadding;
}
