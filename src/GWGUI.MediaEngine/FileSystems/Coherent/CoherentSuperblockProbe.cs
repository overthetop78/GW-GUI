namespace GWGUI.MediaEngine.FileSystems.Coherent;

/// <summary>Examine et valide les champs canoniques du superbloc COHERENT.</summary>
internal static class CoherentSuperblockProbe
{
    /// <summary>Taille d'un bloc COHERENT en octets.</summary>
    /// <summary>Indique si le contenu présente les marqueurs internes d'un superbloc COHERENT.</summary>
    /// <param name="bytes">Contenu à examiner.</param>
    /// <returns><see langword="true"/> lorsque les noms du volume et du pack sont plausibles.</returns>
    public static bool LooksLikeCoherent(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < CoherentSuperblockLayout.MinimumImageSize) return false;
        var name = System.Text.Encoding.ASCII.GetString(bytes.Slice(CoherentSuperblockLayout.VolumeNameOffset, CoherentSuperblockLayout.NameLength));
        var pack = System.Text.Encoding.ASCII.GetString(bytes.Slice(CoherentSuperblockLayout.PackNameOffset, CoherentSuperblockLayout.NameLength));
        return IsAcceptedName(name, CoherentSuperblockLayout.DefaultVolumeName, CoherentSuperblockLayout.VolumePadding) && IsAcceptedName(pack, CoherentSuperblockLayout.DefaultPackName, CoherentSuperblockLayout.PackPadding);
    }

    /// <summary>Valide la structure, l'alignement et le nombre de blocs déclaré par le superbloc.</summary>
    /// <param name="bytes">Dump COHERENT complet.</param>
    /// <returns>Nombre de blocs déclaré par le système de fichiers.</returns>
    /// <exception cref="InvalidDataException">Le superbloc, l'alignement ou le nombre de blocs est invalide.</exception>
    public static int ReadValidatedFileSystemBlockCount(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length % CoherentSuperblockLayout.BlockSize != 0 || !LooksLikeCoherent(bytes)) throw new InvalidDataException("Le contenu ne contient pas de système de fichiers COHERENT valide.");
        var fileSystemBlocks = ReadDeclaredFileSystemBlockCount(bytes);
        if (fileSystemBlocks < 3 || fileSystemBlocks > bytes.Length / CoherentSuperblockLayout.BlockSize) throw new InvalidDataException("La taille déclarée du système de fichiers COHERENT est invalide.");
        return fileSystemBlocks;
    }

    /// <summary>Lit le nombre de blocs déclaré sans appliquer les limites du dump.</summary>
    public static int ReadDeclaredFileSystemBlockCount(ReadOnlySpan<byte> bytes) => checked((int)CoherentCanonicalBinary.ReadUInt32(bytes.Slice(CoherentSuperblockLayout.FileSystemBlockCountOffset, CoherentCanonicalBinary.UInt32Length)));

    /// <summary>Indique si un champ contient son nom par défaut ou le marqueur de remplacement accepté.</summary>
    private static bool IsAcceptedName(string value, string defaultName, char placeholderPadding) => value == defaultName || value == CoherentSuperblockLayout.PlaceholderName + placeholderPadding;
}
