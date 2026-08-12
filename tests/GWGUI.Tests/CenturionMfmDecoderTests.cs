using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.Tests;

/// <summary>Vérifie la validation des en-têtes et blocs de données Centurion MFM.</summary>
public sealed class CenturionMfmDecoderTests
{
    /// <summary>Vérifie un secteur complet avec ses deux CRC valides.</summary>
    [Fact]
    public void CompleteSectorExposesPayloadAndMetadata()
    {
        var payload = Enumerable.Range(0, 256).Select(index => (byte)(index * 29 + 3)).ToArray();
        var bits = TrackBitEncoding.Bits();
        AddHeader(bits, 4, 7, true);
        AddData(bits, CenturionMfmFormat.SupportedDataKey, payload, true);

        var result = Decode(bits);

        var sector = Assert.Single(result.Sectors);
        Assert.Equal(payload, sector.Data);
        Assert.Equal((4, 0, 7, 256, 1), (sector.Cylinder, sector.Head, sector.Number, sector.SizeBytes, sector.SizeCode));
        Assert.Equal(SectorIntegrityKind.Crc, sector.IntegrityKind);
        Assert.True(sector.IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatHeader);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatData);
        Assert.True(result.Confidence > 0);
    }

    /// <summary>Vérifie qu'un CRC d'en-tête invalide rend l'intégrité globale invalide.</summary>
    [Fact]
    public void InvalidHeaderCrcIsReported()
    {
        var bits = TrackBitEncoding.Bits();
        AddHeader(bits, 4, 7, false);
        AddData(bits, CenturionMfmFormat.SupportedDataKey, new byte[256], true);

        var sector = Assert.Single(Decode(bits).Sectors);

        Assert.False(sector.IntegrityValid);
    }

    /// <summary>Vérifie qu'un CRC de données invalide conserve la charge utile tout en signalant l'erreur.</summary>
    [Fact]
    public void InvalidDataCrcIsReported()
    {
        var payload = Enumerable.Repeat((byte)0x5a, 256).ToArray();
        var bits = TrackBitEncoding.Bits();
        AddHeader(bits, 4, 7, true);
        AddData(bits, CenturionMfmFormat.SupportedDataKey, payload, false);

        var sector = Assert.Single(Decode(bits).Sectors);

        Assert.Equal(payload, sector.Data);
        Assert.False(sector.IntegrityValid);
    }

    /// <summary>Vérifie qu'une marque de données isolée est décrite sans créer de secteur.</summary>
    [Fact]
    public void UnpairedDataMarkIsReported()
    {
        var bits = TrackBitEncoding.Bits();
        AddData(bits, CenturionMfmFormat.SupportedDataKey, new byte[256], true);

        var result = Decode(bits);

        Assert.Empty(result.Sectors);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatData && structure.Description.StartsWith("Unpaired", StringComparison.Ordinal));
    }

    /// <summary>Vérifie qu'un nouvel en-tête interrompt la recherche des données du précédent.</summary>
    [Fact]
    public void NewSectorMarkStopsDataSearch()
    {
        var bits = TrackBitEncoding.Bits();
        AddHeader(bits, 1, 2, true);
        AddHeader(bits, 3, 4, true);

        var result = Decode(bits);

        Assert.Contains(result.Sectors, sector => sector.Cylinder == 1 && sector.Number == 2 && sector.Data is null);
    }

    /// <summary>Vérifie qu'une clé inconnue n'est pas traitée comme une charge utile valide.</summary>
    [Fact]
    public void UnsupportedKeyIsRejected()
    {
        var bits = TrackBitEncoding.Bits();
        AddHeader(bits, 1, 2, true);
        AddData(bits, 1, new byte[256], true);

        var result = Decode(bits);

        Assert.Null(Assert.Single(result.Sectors).Data);
        Assert.Contains(result.Structures, structure => structure.Description.Contains("unsupported key 1", StringComparison.Ordinal));
    }

    /// <summary>Vérifie qu'une taille nulle ne produit pas de charge utile.</summary>
    [Fact]
    public void ZeroSizeIsRejected()
    {
        var bits = TrackBitEncoding.Bits();
        AddHeader(bits, 1, 2, true);
        AddData(bits, CenturionMfmFormat.SupportedDataKey, [], true);

        Assert.Null(Assert.Single(Decode(bits).Sectors).Data);
    }

    /// <summary>Vérifie qu'une charge utile tronquée reste indisponible.</summary>
    [Fact]
    public void TruncatedPayloadIsRejected()
    {
        var bits = TrackBitEncoding.Bits();
        AddHeader(bits, 1, 2, true);
        bits.Raw(CenturionMfmFormat.DataMark.ToArray());
        bits.Mfm([CenturionMfmFormat.SupportedDataKey, 1, 0, 0x55]);

        Assert.Null(Assert.Single(Decode(bits).Sectors).Data);
    }

    /// <summary>Vérifie qu'une taille non standard conserve les données et reçoit le code nul documenté.</summary>
    [Fact]
    public void NonStandardSizeUsesFallbackSizeCode()
    {
        byte[] payload = [0x12, 0x34, 0x56];
        var bits = TrackBitEncoding.Bits();
        AddHeader(bits, 1, 2, true);
        AddData(bits, CenturionMfmFormat.SupportedDataKey, payload, true);

        var sector = Assert.Single(Decode(bits).Sectors);

        Assert.Equal(payload, sector.Data);
        Assert.Equal(3, sector.SizeBytes);
        Assert.Equal(0, sector.SizeCode);
        Assert.True(sector.IntegrityValid);
    }

    /// <summary>Ajoute un en-tête Centurion encodé en MFM et son intervalle de recherche.</summary>
    private static void AddHeader(List<bool> bits, byte cylinder, byte sector, bool validCrc)
    {
        byte[] identity = [cylinder, sector];
        var crc = Crc16Calculator.Compute(identity, CenturionMfmFormat.CrcPolynomial, CenturionMfmFormat.CrcInitialValue);
        if (!validCrc) crc ^= ushort.MaxValue;
        bits.Raw(CenturionMfmFormat.SectorMark.ToArray());
        bits.Mfm(identity.Concat([(byte)(crc >> BitPrimitives.BitsPerByte), (byte)crc]));
        bits.Gap(CenturionMfmFormat.DataSearchDistanceBitCount);
    }

    /// <summary>Ajoute un bloc de données Centurion complet.</summary>
    private static void AddData(List<bool> bits, byte key, IReadOnlyList<byte> payload, bool validCrc)
    {
        var sizeHigh = (byte)(payload.Count >> BitPrimitives.BitsPerByte);
        var sizeLow = (byte)payload.Count;
        var crcInput = new byte[] { sizeHigh, sizeLow }.Concat(payload).ToArray();
        var crc = Crc16Calculator.Compute(crcInput, CenturionMfmFormat.CrcPolynomial, CenturionMfmFormat.CrcInitialValue);
        if (!validCrc) crc ^= ushort.MaxValue;
        bits.Raw(CenturionMfmFormat.DataMark.ToArray());
        bits.Mfm(new byte[] { key, sizeHigh, sizeLow }.Concat(payload).Concat([(byte)(crc >> BitPrimitives.BitsPerByte), (byte)crc]));
    }

    /// <summary>Décode les bits fournis avec la chaîne publique du codec.</summary>
    private static FluxDecodeResult Decode(IReadOnlyList<bool> bits) => new CenturionMfmDecoder().Decode(GWGUI.MediaEngine.Flux.FluxRevolutionFactory.Create(bits, 40, 8_000_000));
}
