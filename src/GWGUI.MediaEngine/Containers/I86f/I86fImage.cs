namespace GWGUI.MediaEngine.Containers.I86f;

/// <summary>Représente les drapeaux d'un conteneur 86F et ses pistes présentes.</summary>
/// <param name="Flags">Drapeaux lus dans l'en-tête du fichier.</param>
/// <param name="Tracks">Pistes présentes, dans l'ordre de leur index logique.</param>
public sealed record I86fImage(I86fFileFlags Flags, IReadOnlyList<I86fTrack> Tracks);
