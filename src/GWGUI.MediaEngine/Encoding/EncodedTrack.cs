using GWGUI.MediaEngine.Flux;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Contient les cellules binaires et la révolution de flux produites par un encodeur.</summary>
/// <param name="EncoderId">Identifiant technique de l'encodeur utilisé.</param>
/// <param name="Bits">Cellules binaires produites, dans leur ordre d'émission.</param>
/// <param name="Revolution">Révolution de flux construite à partir des cellules.</param>
public sealed record EncodedTrack(string EncoderId, IReadOnlyList<bool> Bits, FluxRevolution Revolution);
