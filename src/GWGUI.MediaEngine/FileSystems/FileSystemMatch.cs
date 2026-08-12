namespace GWGUI.MediaEngine.FileSystems;

/// <summary>Associe l'identifiant du lecteur ayant réussi au volume qu'il a décodé.</summary>
public sealed record FileSystemMatch(string ReaderId, FileSystemVolume Volume);
