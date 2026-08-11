namespace GWGUI.MediaEngine.Decoding;

/// <summary>Associe l'index d'une révolution au résultat retenu pour celle-ci.</summary>
/// <param name="RevolutionIndex">Index dans la collection d'origine.</param><param name="Result">Résultat du décodage.</param>
public sealed record FluxDecodeSelection(int RevolutionIndex, FluxDecodeResult Result);
