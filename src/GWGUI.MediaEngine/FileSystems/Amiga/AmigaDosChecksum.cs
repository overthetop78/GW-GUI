using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.FileSystems.Amiga;

/// <summary>Valide le checksum additif des blocs AmigaDOS.</summary>
public static class AmigaDosChecksum
{
    /// <summary>Indique si le bloc possède la taille attendue et une somme non vérifiée nulle.</summary>
    public static bool IsValid(ReadOnlySpan<byte> block)
    {
        if (block.Length != AmigaDosLayout.BlockSize) return false;
        uint sum = 0;
        for (var offset = 0; offset < block.Length; offset += AmigaDosLayout.WordSize) sum = unchecked(sum + BigEndianInt32.ReadUnsigned(block, offset));
        return sum == AmigaDosLayout.ValidChecksumSum;
    }
}
