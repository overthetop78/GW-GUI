namespace GWGUI.MediaEngine.Exploration;

/// <summary>
/// Décrit les métadonnées finales d'une capture SCP sans contenir les données de flux des pistes.
/// </summary>
/// <param name="Header">En-tête SCP validé à partir du début du fichier.</param>
/// <param name="CapturedTracks">Nombre d'entrées de piste présentes dans la table SCP.</param>
/// <param name="MissingTracks">Nombre d'entrées de piste attendues mais absentes ; cette valeur est toujours positive ou nulle.</param>
/// <param name="Cylinders">Nombre de cylindres distincts représentés par les entrées présentes.</param>
/// <param name="Sides">Nombre de faces distinctes représentées par les entrées présentes.</param>
/// <param name="ChecksumValid"><see langword="true"/> lorsque la somme de contrôle de la capture est valide selon les règles du format SCP.</param>
/// <param name="FileSize">Taille totale du fichier, en octets.</param>
public sealed record ScpCaptureInfo(
    ScpHeader Header,
    int CapturedTracks,
    int MissingTracks,
    int Cylinders,
    int Sides,
    bool ChecksumValid,
    long FileSize);
