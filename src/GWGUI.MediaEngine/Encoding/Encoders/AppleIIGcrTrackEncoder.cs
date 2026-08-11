using GWGUI.MediaEngine.Encoding.Definitions;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encode les pistes utilisant le format Apple IIGCR.</summary>
public sealed class AppleIIGcrTrackEncoder : TrackEncoderBase
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => FluxCodecIds.AppleIIGcr;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => FluxCodecDisplayNames.AppleIIGcr;
    /// <summary>Encode les secteurs demandés sous forme de cellules binaires.</summary>
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackEncoding.Bits(); var volume = (byte)Attribute(request, AppleIIGcrFormat.VolumeAttributeName, AppleIIGcrFormat.DefaultVolume);
        var useFiveAndThree = Attribute(request, AppleIIGcrFormat.SectorsPerTrackAttributeName, request.Sectors.Count) == AppleIIGcrFormat.FiveAndThreeSectorsPerTrack;
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != AppleIIGcrFormat.SectorSize) throw AppleIIGcrFormat.InvalidSectorSize(sector.Data.Count);
            bits.Gap(AppleIIGcrFormat.LeadingGapBitCount, true); bits.Raw(AppleIIGcrFormat.PrologueFirstByte, AppleIIGcrFormat.PrologueSecondByte, useFiveAndThree ? AppleIIGcrFormat.FiveAndThreeAddressPrologueLastByte : AppleIIGcrFormat.SixAndTwoAddressPrologueLastByte);
            foreach (var value in new[] { volume,(byte)request.Cylinder,(byte)sector.Number,(byte)(volume ^ request.Cylinder ^ sector.Number) }) bits.Raw((byte)((value >> 1) | AppleIIGcrFormat.FourAndFourMask),(byte)(value | AppleIIGcrFormat.FourAndFourMask));
            bits.Raw(AppleIIGcrFormat.EpilogueFirstByte, AppleIIGcrFormat.EpilogueSecondByte, AppleIIGcrFormat.EpilogueLastByte, AppleIIGcrFormat.SyncByte, AppleIIGcrFormat.SyncByte, AppleIIGcrFormat.SyncByte, AppleIIGcrFormat.PrologueFirstByte, AppleIIGcrFormat.PrologueSecondByte, AppleIIGcrFormat.DataPrologueLastByte);
            bits.Raw(useFiveAndThree ? EncodeFiveAndThree(sector.Data) : EncodeSixAndTwo(sector.Data)); bits.Raw(AppleIIGcrFormat.EpilogueFirstByte, AppleIIGcrFormat.EpilogueSecondByte, AppleIIGcrFormat.EpilogueLastByte); bits.Gap(AppleIIGcrFormat.TrailingGapBitCount);
        }
        return bits;
    }
    /// <summary>Exécute le traitement « Encode Six And Two » propre à ce format.</summary>
    private static byte[] EncodeSixAndTwo(IReadOnlyList<byte> source)
    {
        var buffer = new byte[AppleIIGcrFormat.SixAndTwoWorkBufferByteCount]; for (var i=0;i<source.Count;i++) buffer[i]=source[i];
        var encoded = new List<byte>(AppleIIGcrFormat.SixAndTwoEncodedByteCount); byte checksum = 0;
        for (var index = 0; index < AppleIIGcrFormat.SixAndTwoAuxiliaryByteCount; index++)
        {
            var value = (byte)(((buffer[index]&1)<<1)|((buffer[index]&2)>>1)|((buffer[index+AppleIIGcrFormat.SixAndTwoAuxiliaryByteCount]&1)<<3)|((buffer[index+AppleIIGcrFormat.SixAndTwoAuxiliaryByteCount]&2)<<1)|((buffer[index+AppleIIGcrFormat.SixAndTwoAuxiliaryByteCount*2]&1)<<5)|((buffer[index+AppleIIGcrFormat.SixAndTwoAuxiliaryByteCount*2]&2)<<3));
            encoded.Add(AppleIIGcrFormat.SixAndTwoTable[value ^ checksum]); checksum = value;
        }
        for (var index=0;index<AppleIIGcrFormat.SectorSize;index++) { var value=(byte)(source[index]>>2); encoded.Add(AppleIIGcrFormat.SixAndTwoTable[value^checksum]); checksum=value; }
        encoded.Add(AppleIIGcrFormat.SixAndTwoTable[checksum]); return encoded.ToArray();
    }

    /// <summary>Exécute le traitement « Encode Five And Three » propre à ce format.</summary>
    private static byte[] EncodeFiveAndThree(IReadOnlyList<byte> source)
    {
        const int chunkSize = AppleIIGcrFormat.FiveAndThreeChunkByteCount; const int threeSize = AppleIIGcrFormat.FiveAndThreeAuxiliaryByteCount;
        var top = new byte[AppleIIGcrFormat.SectorSize]; var threes = new byte[threeSize]; var chunk = chunkSize - 1; var sourceOffset = 0;
        for (var index = 0; index < chunkSize * 5; index += 5)
        {
            var zero = source[sourceOffset++]; var one = source[sourceOffset++]; var two = source[sourceOffset++];
            var three = source[sourceOffset++]; var four = source[sourceOffset++];
            top[chunk] = (byte)(zero >> 3); top[chunk + chunkSize] = (byte)(one >> 3);
            top[chunk + chunkSize * 2] = (byte)(two >> 3); top[chunk + chunkSize * 3] = (byte)(three >> 3);
            top[chunk + chunkSize * 4] = (byte)(four >> 3);
            threes[chunk] = (byte)(((zero & 7) << 2) | ((three & 4) >> 1) | ((four & 4) >> 2));
            threes[chunk + chunkSize] = (byte)(((one & 7) << 2) | (three & 2) | ((four & 2) >> 1));
            threes[chunk + chunkSize * 2] = (byte)(((two & 7) << 2) | ((three & 1) << 1) | (four & 1));
            chunk--;
        }
        var last = source[sourceOffset]; top[AppleIIGcrFormat.SectorSize - 1] = (byte)(last >> 3); threes[^1] = (byte)(last & 7);
        var encoded = new List<byte>(AppleIIGcrFormat.FiveAndThreeEncodedByteCount); byte checksum = 0;
        for (var index = threeSize - 1; index >= 0; index--) { encoded.Add(AppleIIGcrFormat.FiveAndThreeTable[threes[index] ^ checksum]); checksum = threes[index]; }
        for (var index = 0; index < AppleIIGcrFormat.SectorSize; index++) { encoded.Add(AppleIIGcrFormat.FiveAndThreeTable[top[index] ^ checksum]); checksum = top[index]; }
        encoded.Add(AppleIIGcrFormat.FiveAndThreeTable[checksum]);
        return encoded.ToArray();
    }
}
