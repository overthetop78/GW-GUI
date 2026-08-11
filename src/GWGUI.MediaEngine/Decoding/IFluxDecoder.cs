using GWGUI.MediaEngine.Flux;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Définit un décodeur capable d'interpréter une révolution complète de flux rotationnel.</summary>
/// <remarks>L'identifiant est une chaîne technique extensible qui doit être unique dans un registre. Le nom affiché est purement descriptif et ne doit jamais servir à identifier un codec.</remarks>
public interface IFluxDecoder
{
    /// <summary>Obtient l'identifiant technique extensible et unique du codec.</summary>
    string Id { get; }
    /// <summary>Obtient le nom descriptif destiné à l'affichage.</summary>
    string DisplayName { get; }
    /// <summary>Analyse une révolution complète sans modifier ses intervalles et retourne un résultat immuable.</summary>
    /// <param name="revolution">Révolution dont la durée d'index et les intervalles sont exprimés en ticks de la source.</param>
    /// <returns>Résultat immuable ; son estimation de cellule de bit est exprimée dans les mêmes ticks que la révolution.</returns>
    /// <remarks>Une révolution vide, un flux non reconnu ou un flux tronqué produit un résultat sans secteurs plutôt qu'une erreur de contrat. Un flux reconnu mais corrompu conserve les secteurs détectés avec une intégrité invalide ou indéterminée. L'absence de secteurs est toujours représentée par une collection vide.</remarks>
    FluxDecodeResult Decode(FluxRevolution revolution);
}
