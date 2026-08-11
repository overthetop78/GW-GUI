using GWGUI.MediaEngine.Encoding.Definitions;
using GWGUI.MediaEngine.Geometries.Dec;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Encoding;

public sealed class DecRx02TrackEncoder : TrackEncoderBase
{
    public override string Id => FluxCodecIds.DecRx02;
    public override string DisplayName => FluxCodecDisplayNames.DecRx02;
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits=TrackEncoding.Bits();
        foreach(var sector in request.Sectors)
        {
            var m2fm=sector.Data.Count==DecRx02Geometry.PhysicalSectorSize; if(!m2fm&&sector.Data.Count!=DecRx02EncodingFormat.FmSectorByteCount) throw DecRx02EncodingFormat.InvalidSectorSize(sector.Data.Count);
            var sizeCode=sector.SizeCode??(m2fm?DecRx02EncodingFormat.M2FmSectorSizeCode:DecRx02EncodingFormat.FmSectorSizeCode);
            var headerCrc=Crc16Calculator.Compute([DecRx02EncodingFormat.HeaderAddressMark,(byte)request.Cylinder,(byte)request.Head,(byte)sector.Number,sizeCode],DecRx02EncodingFormat.CrcPolynomial,DecRx02EncodingFormat.CrcInitialValue);
            bits.Raw(DecRx02EncodingFormat.HeaderMark.ToArray()); bits.DoubleFm([(byte)request.Cylinder,(byte)request.Head,(byte)sector.Number,sizeCode,(byte)(headerCrc>>BitPrimitives.BitsPerByte),(byte)headerCrc]); bits.Gap(DecRx02EncodingFormat.GapBitCount,true);
            var mark=m2fm?(sector.Deleted?DecRx02EncodingFormat.M2FmDeletedDataMark:DecRx02EncodingFormat.M2FmDataMark):(sector.Deleted?DecRx02EncodingFormat.FmDeletedDataMark:DecRx02EncodingFormat.FmDataMark);
            bits.Raw(DecRx02EncodingFormat.DataMarks.Single(item=>item.Mark==mark).Pattern.ToArray());
            var crc=Crc16Calculator.Compute(new[]{mark}.Concat(sector.Data),DecRx02EncodingFormat.CrcPolynomial,DecRx02EncodingFormat.CrcInitialValue); var payload=sector.Data.Concat([(byte)(crc>>BitPrimitives.BitsPerByte),(byte)crc]).ToArray();
            if(m2fm) { bits.Add(false); var encoded=TrackEncoding.Bits(); encoded.Mfm(payload); ReplaceM2Fm(encoded); bits.AddRange(encoded); }
            else bits.DoubleFm(payload);
            bits.Gap(DecRx02EncodingFormat.GapBitCount,true);
        }
        return bits;
    }
    private static void ReplaceM2Fm(List<bool> bits)
    {
        var normal=DecRx02EncodingFormat.NormalM2FmRule;
        var encoded=DecRx02EncodingFormat.EncodedM2FmRule;
        for(var offset=1;offset+normal.Count<=bits.Count;offset+=2)
        {
            var match=true; for(var i=0;i<normal.Count;i++) if(bits[offset+i]!=normal[i]) { match=false; break; }
            if(!match) continue; for(var i=0;i<normal.Count;i++) bits[offset+i]=encoded[i]; offset+=normal.Count-3;
        }
    }
}
