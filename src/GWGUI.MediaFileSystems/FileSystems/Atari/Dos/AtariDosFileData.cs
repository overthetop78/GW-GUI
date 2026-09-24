namespace GWGUI.MediaFileSystems.FileSystems.Atari.Dos;

/// <summary>Contient le contenu, la chaîne sectorielle et l'analyse d'un fichier Atari DOS.</summary>
public sealed record AtariDosFileData(
    IReadOnlyList<byte> Content,
    AtariDosSectorChain Chain,
    bool IsValid,
    IReadOnlyList<string> Attributes,
    IReadOnlyList<string> Diagnostics);
