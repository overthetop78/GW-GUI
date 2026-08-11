namespace GWGUI.MediaEngine.Encoding;

public sealed class EmuFmTrackEncoder : TrackEncoderBase
{
    public override string Id => "emu.fm";
    public override string DisplayName => "E-mu Emulator FM";
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits=TrackEncoding.Bits();
        foreach(var sector in request.Sectors)
        {
            if(sector.Data.Count!=0xe00) throw new ArgumentException("E-mu sectors contain 3584 bytes.");
            var rawTrack=Primitives.BitPrimitives.ReverseBits((byte)(request.Cylinder<<1|request.Head));
            var headerCrc=Primitives.Crc16Calculator.Compute([rawTrack],Primitives.Crc16Calculator.IbmPolynomial,Primitives.Crc16Calculator.ZeroInitialValue);
            bits.Raw(0x45,0x45,0x55,0x55,0x45,0x54,0x54,0x45);
            bits.DoubleFm([rawTrack,(byte)(headerCrc>>8),(byte)headerCrc]); bits.Gap(64,true);
            var dataCrc=Primitives.Crc16Calculator.Compute(sector.Data,Primitives.Crc16Calculator.IbmPolynomial,Primitives.Crc16Calculator.ZeroInitialValue);
            bits.Raw(0x45,0x45,0x55,0x55,0x45,0x54,0x54,0x45);
            bits.DoubleFm(sector.Data.Concat([(byte)(dataCrc>>8),(byte)dataCrc])); bits.Gap(64,true);
        }
        return bits;
    }
}
