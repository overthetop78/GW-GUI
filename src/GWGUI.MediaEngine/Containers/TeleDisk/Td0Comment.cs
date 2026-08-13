namespace GWGUI.MediaEngine.Containers.TeleDisk;

/// <summary>Conserve le commentaire binaire TeleDisk et sa date d'origine.</summary>
public sealed record Td0Comment(byte Year, byte Month, byte Day, byte Hour, byte Minute, byte Second, IReadOnlyList<byte> Data);
