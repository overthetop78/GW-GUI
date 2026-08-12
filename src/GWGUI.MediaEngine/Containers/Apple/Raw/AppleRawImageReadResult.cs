using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Containers.Apple.Raw;

/// <summary>Indique la preuve qui a conduit au choix de l'interprétation brute.</summary>
internal enum AppleRawImageMatchKind
{
    /// <summary>Une structure interne a validé l'interprétation.</summary>
    ValidatedStructure,
    /// <summary>L'extension a indiqué l'ordre sectoriel sans valider un système de fichiers.</summary>
    ExtensionHint,
    /// <summary>La géométrie a fourni le dernier choix en l'absence de structure reconnue.</summary>
    GeometryFallback
}

/// <summary>Associe l'image construite au type de preuve ayant déterminé son interprétation.</summary>
internal sealed record AppleRawImageReadResult(SectorImage Image, AppleRawImageMatchKind MatchKind);
