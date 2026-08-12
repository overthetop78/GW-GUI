namespace GWGUI.MediaEngine.FileSystems.Dec.Rt11;

/// <summary>Contient un contenu RT-11 positionné et ses blocs invalides.</summary>
/// <param name="Content">Contenu dont les lacunes conservent leur position.</param>
/// <param name="IsValid">Validité de tous les blocs demandés.</param>
/// <param name="MissingBlocks">Numéros des blocs absents ou tronqués.</param>
public sealed record Rt11FileContent(IReadOnlyList<byte> Content, bool IsValid, IReadOnlyList<int> MissingBlocks);
