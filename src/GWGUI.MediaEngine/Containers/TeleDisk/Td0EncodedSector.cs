namespace GWGUI.MediaEngine.Containers.TeleDisk;

/// <summary>Associe un mode d'encodage TeleDisk à sa charge utile.</summary>
internal sealed record Td0EncodedSector(Td0SectorEncoding Encoding, IReadOnlyList<byte> Payload);
