namespace GWGUI.MediaEngine.Containers.TeleDisk;

/// <summary>Conserve les champs de l'en-tête d'un conteneur TeleDisk non compressé.</summary>
public sealed record Td0Header(byte Sequence, byte CheckSignature, byte Version, byte DataRate, byte DriveType, byte TrackDensity, byte DosMode, byte Surfaces);
