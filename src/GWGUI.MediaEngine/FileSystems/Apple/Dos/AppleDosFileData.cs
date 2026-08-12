namespace GWGUI.MediaEngine.FileSystems.Apple.Dos;

/// <summary>Contient le contenu, la validité, la première référence et le nombre de secteurs traversés d'un fichier Apple DOS.</summary>
public sealed record AppleDosFileData(IReadOnlyList<byte> Content, bool IsValid, int StorageReference, int TraversedSectorCount);
