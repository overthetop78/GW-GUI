using System.Buffers.Binary;
using System.Security.Cryptography;
using GWGUI.MediaEngine.Images.Writing.Encoding;

namespace GWGUI.Tests.Media;

public sealed class IsoTrackEncoderTests
{
    [Theory]
    [InlineData(false, 7296, 5239, "DC307D0ED0F1CD6C90D44427BF50AA6881727B7FD6BE745247E49C241D4192A2", "6703E5A17F84978457062C9178A63772A822ABA70AD93C7A3A341D5FCF56532D")]
    [InlineData(true, 7488, 2909, "1BF9E4D58DB1A11041FAF42EA99ED3432B5F53033B548A935BC0A94F4F66D8FF", "966C88652533BCABBE8014C79BBB77F8C1F6B1A875F4C2B1AF966086755A4BF8")]
    public void BitsAndFluxMatchTheReferenceBeforeExtraction(bool mfm, int bitCount, int intervalCount,
        string bitsSha256, string fluxSha256)
    {
        var first = Enumerable.Range(0, 128).Select(value => (byte)((value * 17 + 3) % 256)).ToArray();
        var second = Enumerable.Range(0, 256).Select(value => (byte)((value * 29 + 7) % 256)).ToArray();
        var track = Encoder(mfm).Encode(new(37, 1,
            [new TrackSector(3, first), new TrackSector(7, second, Deleted: true, SizeCode: 1)]));
        Assert.Equal(mfm ? "iso.mfm" : "iso.fm", track.EncoderId);
        Assert.Equal(bitCount, track.Bits.Count);
        Assert.Equal(bitsSha256, Convert.ToHexString(SHA256.HashData(
            track.Bits.Select(bit => bit ? (byte)1 : (byte)0).ToArray())));
        Assert.Equal(8_000_000u, track.Revolution.IndexTimeTicks);
        Assert.Equal(intervalCount, track.Revolution.FluxIntervals.Count);
        var bytes = new byte[intervalCount * sizeof(uint)];
        for (var index = 0; index < intervalCount; index++)
            BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(index * sizeof(uint), sizeof(uint)),
                track.Revolution.FluxIntervals[index]);
        Assert.Equal(fluxSha256, Convert.ToHexString(SHA256.HashData(bytes)));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SectorAddressStillUsesItsFormatValidation(bool mfm)
    {
        var error = Assert.Throws<ArgumentOutOfRangeException>(() =>
            Encoder(mfm).Encode(new(0, 0, [new TrackSector(256, new byte[128])])));
        Assert.Equal("Number", error.ParamName);
        Assert.Equal(256, error.ActualValue);
        Assert.Contains(mfm ? "ISO MFM" : "ISO FM", error.Message);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MismatchedSectorSizeStillUsesItsFormatValidation(bool mfm)
    {
        var error = Assert.Throws<ArgumentException>(() =>
            Encoder(mfm).Encode(new(0, 0, [new TrackSector(1, new byte[128], SizeCode: 1)])));
        Assert.Contains(mfm ? "ISO MFM" : "ISO FM", error.Message);
    }

    private static TrackEncoderBase Encoder(bool mfm) => mfm ? new IsoMfmTrackEncoder() : new IsoFmTrackEncoder();
}
