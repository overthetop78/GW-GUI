namespace GWGUI.MediaEngine.Decoding;

public sealed record DecodedSector(byte Cylinder, byte Head, int Number, byte SizeCode, int SizeBytes, bool? IntegrityValid, int BitOffset, SectorIntegrityKind IntegrityKind = SectorIntegrityKind.Crc, IReadOnlyList<byte>? Data = null, IReadOnlyList<byte>? Tag = null);
