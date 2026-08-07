namespace GWGUI.Scp.Encoding;

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
            var rawTrack=TrackEncoding.ReverseBits((byte)(request.Cylinder<<1|request.Head));
            var headerCrc=TrackEncoding.Crc16([rawTrack],0x8005,0);
            bits.Raw(0x45,0x45,0x55,0x55,0x45,0x54,0x54,0x45);
            bits.DoubleFm([rawTrack,(byte)(headerCrc>>8),(byte)headerCrc]); bits.Gap(64,true);
            var dataCrc=TrackEncoding.Crc16(sector.Data,0x8005,0);
            bits.Raw(0x45,0x45,0x55,0x55,0x45,0x54,0x54,0x45);
            bits.DoubleFm(sector.Data.Concat([(byte)(dataCrc>>8),(byte)dataCrc])); bits.Gap(64,true);
        }
        return bits;
    }
}
