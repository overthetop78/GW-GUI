namespace GWGUI.MediaEngine.FileSystems.Amiga.FlatArchive;

/// <summary>Regroupe les octets lus et les blocs absents ou invalides rencontrés.</summary>
internal sealed record AmigaFlatResourceReadResult(byte[] Bytes, IReadOnlyList<int> MissingBlocks, IReadOnlyList<int> InvalidBlocks);
