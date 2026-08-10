namespace GWGUI.MediaEngine;

/// <summary>
/// Représente une entrée de piste SCP et les révolutions qu'elle contient.
/// </summary>
/// <param name="TrackNumber">Numéro brut de l'entrée dans la table des pistes SCP.</param>
/// <param name="Cylinder">Numéro de cylindre calculé à partir de l'entrée SCP.</param>
/// <param name="Head">Numéro de face calculé à partir de l'entrée SCP.</param>
/// <param name="Revolutions">Révolutions capturées pour cette piste.</param>
public sealed record ScpTrack(byte TrackNumber, int Cylinder, int Head, IReadOnlyList<ScpRevolution> Revolutions);
