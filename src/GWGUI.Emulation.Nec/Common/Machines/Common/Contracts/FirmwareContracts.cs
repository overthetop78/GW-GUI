namespace GWGUI.Emulation.Nec.Common.Machines.Common.Contracts;

/// <summary>Observations et identité d’un fichier candidat au catalogue de firmwares.</summary>
/// <param name="Path">Chemin absolu du fichier inspecté, conservé sous la responsabilité de son appelant.</param>
/// <param name="Size">Taille du candidat en octets.</param>
/// <param name="Md5">Empreinte MD5 calculée sur le contenu.</param>
/// <param name="Sha256">Empreinte SHA-256 calculée sur le même contenu.</param>
/// <param name="LastWriteTimeUtc">Date UTC de dernière modification observée lors du scan.</param>
/// <param name="Type">Catégorie reconnue, ou Unknown en l’absence de correspondance.</param>
/// <param name="IsKnown">Indique si une règle du catalogue a reconnu le candidat.</param>
/// <param name="IsOfficial">Indique si le catalogue classe le contenu comme officiel.</param>
/// <param name="Name">Nom reconnu, ou null pour un candidat inconnu.</param>
/// <param name="Version">Version reconnue, lorsqu’elle est connue.</param>
/// <param name="CompatibleModels">Identifiants des modèles compatibles annoncés par la règle de reconnaissance.</param>
/// <param name="IsVerified">Indique si les données de reconnaissance sont vérifiées.</param>
/// <remarks>Ce résultat ne possède aucun flux. L’adaptateur peut compléter son identité avec une copie de record
/// tout en conservant les observations déjà calculées par Common.</remarks>
public sealed record ScannedFirmware(string Path, long Size, string Md5, string Sha256, DateTime LastWriteTimeUtc, FirmwareType Type, bool IsKnown, bool IsOfficial, string? Name, string? Version, IReadOnlyList<string> CompatibleModels, bool IsVerified = true);
