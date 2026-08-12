namespace GWGUI.MediaEngine.FileSystems.Ucsd;

/// <summary>Contient un fichier UCSD positionné et ses blocs invalides.</summary>
/// <param name="Content">Contenu reconstruit du fichier.</param>
/// <param name="IsValid">Indique si tous les blocs requis ont été lus.</param>
/// <param name="MissingBlocks">Blocs absents ou tronqués.</param>
/// <param name="Size">Taille logique annoncée du fichier.</param>
internal sealed record UcsdFileContent(IReadOnlyList<byte> Content, bool IsValid, IReadOnlyList<int> MissingBlocks, long Size);
