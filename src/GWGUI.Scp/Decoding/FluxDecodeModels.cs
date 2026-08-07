namespace GWGUI.Scp.Decoding;

public enum FluxStructureKind { Sync, IdAddressMark, DataAddressMark, DeletedDataAddressMark, AmigaSync, AppleAddress, AppleData, CommodoreSync, CommodoreHeader, FormatHeader, FormatData, TimingAnomaly }

public enum SectorIntegrityKind { Crc, Checksum }

public sealed record FluxStructure(FluxStructureKind Kind, int BitOffset, int BitLength, string Description);

public sealed record DecodedSector(byte Cylinder, byte Head, int Number, byte SizeCode, int SizeBytes, bool? IntegrityValid, int BitOffset, SectorIntegrityKind IntegrityKind = SectorIntegrityKind.Crc, IReadOnlyList<byte>? Data = null);

public sealed record FluxDecodeResult(string DecoderId, string DisplayName, double Confidence, double EstimatedBitCellTicks, IReadOnlyList<FluxStructure> Structures, IReadOnlyList<byte> DecodedBytes, IReadOnlyList<DecodedSector>? Sectors = null);
