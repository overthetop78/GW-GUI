namespace GWGUI.MediaEngine.FileSystems.Amiga;

/// <summary>Contient le contenu reconstruit d'un fichier et son état de validité.</summary>
public sealed record AmigaDosFileData(IReadOnlyList<byte> Content, bool IsValid);
