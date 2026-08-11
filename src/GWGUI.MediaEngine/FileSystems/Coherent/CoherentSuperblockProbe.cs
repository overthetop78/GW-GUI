using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.FileSystems.Coherent;

/// <summary>Examine et valide les champs canoniques du superbloc COHERENT.</summary>
internal static class CoherentSuperblockProbe
{
    /// <summary>Taille d'un bloc COHERENT en octets.</summary>
    public const int BlockSize = 512;
    private const int MinimumImageSize = 1_024;
    private const int FileSystemBlockCountOffset = 514;
    private const int VolumeNameOffset = 996;
    private const int PackNameOffset = 1_002;
    private const int NameLength = 6;

    /// <summary>Indique si le contenu présente les marqueurs internes d'un superbloc COHERENT.</summary>
    /// <param name="bytes">Contenu à examiner.</param>
    /// <returns><see langword="true"/> lorsque les noms du volume et du pack sont plausibles.</returns>
    public static bool LooksLikeCoherent(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < MinimumImageSize) return false;
        var name = System.Text.Encoding.ASCII.GetString(bytes.Slice(VolumeNameOffset, NameLength));
        var pack = System.Text.Encoding.ASCII.GetString(bytes.Slice(PackNameOffset, NameLength));
        return (name is "noname" or "xxxxx " || name.StartsWith("xxxxx", StringComparison.Ordinal)) && (pack is "nopack" or "xxxxx\n" || pack.StartsWith("xxxxx", StringComparison.Ordinal));
    }

    /// <summary>Valide la structure, l'alignement et le nombre de blocs déclaré par le superbloc.</summary>
    /// <param name="bytes">Dump COHERENT complet.</param>
    /// <returns>Nombre de blocs déclaré par le système de fichiers.</returns>
    /// <exception cref="InvalidDataException">Le superbloc, l'alignement ou le nombre de blocs est invalide.</exception>
    public static int ReadValidatedFileSystemBlockCount(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length % BlockSize != 0 || !LooksLikeCoherent(bytes)) throw new InvalidDataException("The image does not contain a COHERENT file system.");
        var fileSystemBlocks = checked((int)ReadCanonicalUInt32(bytes.Slice(FileSystemBlockCountOffset, sizeof(uint))));
        if (fileSystemBlocks < 3 || fileSystemBlocks > bytes.Length / BlockSize) throw new InvalidDataException("The COHERENT file-system size is invalid.");
        return fileSystemBlocks;
    }

    /// <summary>Lit un entier 32 bits enregistré dans l'ordre canonique COHERENT.</summary>
    /// <param name="value">Quatre octets canoniques.</param>
    /// <returns>Valeur entière décodée.</returns>
    public static uint ReadCanonicalUInt32(ReadOnlySpan<byte> value)
    {
        if (value.Length < sizeof(uint)) throw new ArgumentException("Four bytes are required.", nameof(value));
        return (uint)(value[2] | value[3] << BitPrimitives.BitsPerByte | value[0] << 16 | value[1] << 24);
    }
}
