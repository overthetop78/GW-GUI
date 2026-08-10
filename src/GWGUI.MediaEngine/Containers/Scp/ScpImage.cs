namespace GWGUI.MediaEngine;

/// <summary>
/// Représente le contenu interprété d'un conteneur SCP complet.
/// </summary>
/// <param name="Header">En-tête SCP validé.</param>
/// <param name="Tracks">Pistes présentes et interprétées depuis la table du conteneur.</param>
/// <param name="ChecksumValid"><see langword="true"/> lorsque la somme de contrôle respecte les règles du format SCP.</param>
/// <param name="FileSize">Taille totale du conteneur source, en octets.</param>
public sealed record ScpImage(ScpHeader Header, IReadOnlyList<ScpTrack> Tracks, bool ChecksumValid, long FileSize);
