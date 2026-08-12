namespace GWGUI.MediaEngine.FileSystems;

/// <summary>Conserve l'échec d'un lecteur qui avait reconnu l'image.</summary>
public sealed record FileSystemReadFailure(string ReaderId, InvalidDataException Exception);
