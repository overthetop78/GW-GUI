using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;

namespace GWGUI.Tests;

/// <summary>Vérifie l'identité et l'encodage de la spécialisation Lisa FileWare.</summary>
public sealed class AppleLisaFileWareGcrTrackEncoderTests
{
    /// <summary>Vérifie que l'encodeur expose l'identité centrale Lisa FileWare.</summary>
    [Fact]
    public void EncoderExposesCentralLisaIdentity()
    {
        var encoder = new AppleLisaFileWareGcrTrackEncoder();
        Assert.Equal(FluxCodecIds.AppleLisaFileWareGcr, encoder.Id);
        Assert.Equal(FluxCodecDisplayNames.AppleLisaFileWareGcr, encoder.DisplayName);
    }

    /// <summary>Vérifie l'octet de format et l'aller-retour par les composants publics Lisa FileWare.</summary>
    [Fact]
    public void PublicEncoderRoundTripsTheLisaFormat()
    {
        var data = Enumerable.Range(0, AppleIwmGcrFormat.SectorByteCount).Select(index => (byte)(index * 37 + 11)).ToArray();
        var encoded = new FluxEncoderRegistry().Encode(FluxCodecIds.AppleLisaFileWareGcr, new TrackEncodeRequest(2, 0, [new TrackSector(3, data)]));
        var decoded = new FluxDecoderRegistry().Decode(FluxCodecIds.AppleLisaFileWareGcr, encoded.Revolution);
        var sector = Assert.Single(decoded.Sectors!);
        Assert.Equal(AppleIwmGcrFormat.DefaultFormat, sector.FormatCode);
        Assert.Equal(data, sector.Data);
        Assert.True(sector.IntegrityValid);
    }
}
