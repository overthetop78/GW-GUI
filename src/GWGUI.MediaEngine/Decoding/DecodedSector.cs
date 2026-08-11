using System.Collections.ObjectModel;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Décrit un secteur reconstruit à partir d'un flux.</summary>
public sealed record DecodedSector
{
    /// <summary>Initialise un secteur décodé en copiant ses données et son étiquette lorsqu'elles sont fournies.</summary>
    public DecodedSector(byte Cylinder, byte Head, int Number, byte SizeCode, int SizeBytes, bool? IntegrityValid, int BitOffset, SectorIntegrityKind IntegrityKind = SectorIntegrityKind.Crc, IReadOnlyList<byte>? Data = null, IReadOnlyList<byte>? Tag = null)
    {
        this.Cylinder = Cylinder;
        this.Head = Head;
        this.Number = Number;
        this.SizeCode = SizeCode;
        this.SizeBytes = SizeBytes;
        this.IntegrityValid = IntegrityValid;
        this.BitOffset = BitOffset;
        this.IntegrityKind = IntegrityKind;
        this.Data = Data is null ? null : new ReadOnlyCollection<byte>(Data.ToArray());
        this.Tag = Tag is null ? null : new ReadOnlyCollection<byte>(Tag.ToArray());
    }

    public byte Cylinder { get; init; }
    public byte Head { get; init; }
    public int Number { get; init; }
    public byte SizeCode { get; init; }
    public int SizeBytes { get; init; }
    public bool? IntegrityValid { get; init; }
    public int BitOffset { get; init; }
    public SectorIntegrityKind IntegrityKind { get; init; }
    public IReadOnlyList<byte>? Data { get; }
    public IReadOnlyList<byte>? Tag { get; }
}
