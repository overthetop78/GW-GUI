namespace GWGUI.MediaEngine.FileSystems.Atari.Dos;

/// <summary>Contient le contenu partiel, le nombre de secteurs parcourus et la validité d'un fichier Atari DOS.</summary>
public sealed record AtariDosFileData(IReadOnlyList<byte> Content, int TraversedSectors, bool IsValid);
