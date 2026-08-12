using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;

namespace GWGUI.Tests;

public sealed class TrackEncoderTests
{
    public static TheoryData<string, int> RoundTrips => new()
    {
        { "iso.mfm", 512 }, { "iso.fm", 256 }, { "amiga.mfm", 512 },
        { "apple2.gcr", 256 }, { "applemac.gcr", 512 }, { "applelisa.fileware.gcr", 512 }, { "commodore.gcr", 256 },
        { "hp.mmfm", 256 }, { "datageneral.fm", 512 }, { "micropolis.mfm", 256 },
        { "membrain.mfm", 512 }, { "aed6200p.mfm", 512 }, { "qdmo5.mfm", 128 },
        { "centurion.mfm", 256 }, { "northstar.mfm", 512 }, { "heathkit.fm", 256 },
        { "micraln.fm", 128 }, { "emu.fm", 0xe00 }, { "tycom.fm", 128 },
        { "dec.rx02", 128 }, { "commodore900.gcr", 512 }, { "victor9k.gcr", 512 }
    };

    [Fact]
    public void FluxCodecIdentifiersAreUnique()
    {
        var identifiers = typeof(FluxCodecIds).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).Select(field => Assert.IsType<string>(field.GetRawConstantValue())).ToArray();

        Assert.Equal(identifiers.Length, identifiers.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void RegisteredCodecIdentifiersHaveOneDisplayName()
    {
        var codecs = new FluxDecoderRegistry().Decoders.Select(codec => (codec.Id, codec.DisplayName)).Concat(new FluxEncoderRegistry().Encoders.Select(codec => (codec.Id, codec.DisplayName)));
        var contradictions = codecs.GroupBy(codec => codec.Id, StringComparer.Ordinal).Where(group => group.Select(codec => codec.DisplayName).Distinct(StringComparer.Ordinal).Count() > 1).Select(group => group.Key).ToArray();

        Assert.Empty(contradictions);
    }

    [Fact]
    public void RegisteredDecodersExposeTheirCommonDisplayName()
    {
        var displayNames = CodecDisplayNamesById();

        foreach (var decoder in new FluxDecoderRegistry().Decoders) Assert.Equal(displayNames[decoder.Id], decoder.DisplayName);
    }

    [Fact]
    public void RegisteredEncodersExposeTheirCommonDisplayName()
    {
        var displayNames = CodecDisplayNamesById();

        foreach (var encoder in new FluxEncoderRegistry().Encoders) Assert.Equal(displayNames[encoder.Id], encoder.DisplayName);
    }

    [Fact]
    public void RegisteredCodecsHaveANonEmptyDisplayName()
    {
        var displayNames = CodecDisplayNamesById();
        var codecs = new FluxDecoderRegistry().Decoders.Select(codec => (codec.Id, codec.DisplayName)).Concat(new FluxEncoderRegistry().Encoders.Select(codec => (codec.Id, codec.DisplayName)));

        foreach (var codec in codecs) { Assert.True(displayNames.ContainsKey(codec.Id)); Assert.False(string.IsNullOrWhiteSpace(codec.DisplayName)); }
    }

    [Fact]
    public void RegistryContainsEncoderForEverySemanticDecoder()
    {
        var decoderIds = new FluxDecoderRegistry().Decoders.Where(item => item.Id != "raw").Select(item => item.Id).Order().ToArray();
        var encoderIds = new FluxEncoderRegistry().Encoders.Select(item => item.Id).Order().ToArray();
        Assert.Equal(decoderIds, encoderIds);
    }

    [Fact]
    public void DefaultEncoderCatalogPreservesItsPublicOrderAndUniqueness()
    {
        string[] expected = ["iso.mfm", "iso.fm", "amiga.mfm", "apple2.gcr", "apple2.rwts18", "applemac.gcr", "applelisa.fileware.gcr", "commodore.gcr", "hp.mmfm", "datageneral.fm", "micropolis.mfm", "membrain.mfm", "aed6200p.mfm", "qdmo5.mfm", "centurion.mfm", "northstar.mfm", "heathkit.fm", "micraln.fm", "emu.fm", "tycom.fm", "dec.rx02", "arburg", "victor9k.gcr", "commodore900.gcr"];
        var actual = new FluxEncoderRegistry().Encoders.Select(encoder => encoder.Id).ToArray();

        Assert.Equal(expected, actual);
        Assert.Equal(actual.Length, actual.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void RegistryCopiesTheProvidedCollection()
    {
        var first = new StubTrackEncoder("first");
        ITrackEncoder[] source = [first];
        var registry = new FluxEncoderRegistry(source);

        source[0] = new StubTrackEncoder("replacement");

        Assert.Same(first, Assert.Single(registry.Encoders));
        Assert.Same(first, registry.Get("first"));
    }

    [Fact]
    public void RegistryRejectsNullEncoderEmptyIdentifierAndDuplicateIdentifier()
    {
        Assert.Contains("index 0", Assert.Throws<ArgumentException>(() => new FluxEncoderRegistry(new ITrackEncoder[] { null! })).Message, StringComparison.Ordinal);
        Assert.Contains("index 0", Assert.Throws<ArgumentException>(() => new FluxEncoderRegistry([new StubTrackEncoder(" ")])).Message, StringComparison.Ordinal);
        Assert.Contains("duplicate", Assert.Throws<ArgumentException>(() => new FluxEncoderRegistry([new StubTrackEncoder("duplicate"), new StubTrackEncoder("duplicate")])).Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RegistryReportsMissingIdentifierAndExecutesInjectedEncoder()
    {
        var encoder = new StubTrackEncoder("injected");
        var registry = new FluxEncoderRegistry([encoder]);
        var request = new TrackEncodeRequest(0, 0, [new TrackSector(1, [0])]);

        Assert.Contains("missing", Assert.Throws<KeyNotFoundException>(() => registry.Get("missing")).Message, StringComparison.Ordinal);
        Assert.Same(encoder.Result, registry.Encode("injected", request));
        Assert.Same(request, encoder.LastRequest);
    }

    [Fact]
    public void TrackEncodingModelsCopyTheirInputCollectionsAndExposeNamedDefaults()
    {
        var data = new byte[] { 1, 2 };
        var sectorAttributes = new Dictionary<string, int> { ["sector"] = 3 };
        var sector = new TrackSector(1, data, Attributes: sectorAttributes);
        var sectors = new[] { sector };
        var requestAttributes = new Dictionary<string, int> { ["track"] = 4 };
        var request = new TrackEncodeRequest(2, 1, sectors, requestAttributes);
        var bits = new[] { true, false };
        var encoded = new EncodedTrack("test", bits, GWGUI.MediaEngine.Flux.FluxRevolutionFactory.Create(bits, 1, 2));

        data[0] = 9;
        sectorAttributes["sector"] = 9;
        sectors[0] = new TrackSector(2, [9]);
        requestAttributes["track"] = 9;
        bits[0] = false;

        Assert.Equal((byte)1, sector.Data[0]);
        Assert.Equal(3, sector.Attributes!["sector"]);
        Assert.Same(sector, request.Sectors[0]);
        Assert.Equal(4, request.Attributes!["track"]);
        Assert.True(encoded.Bits[0]);
        Assert.Equal(TrackEncodingDefaults.BitCellTicks, request.BitCellTicks);
        Assert.Equal(TrackEncodingDefaults.IndexTimeTicks, request.IndexTimeTicks);
    }

    [Fact]
    public void EncodedTrackAndEncoderContractDependOnGenericFluxRevolution()
    {
        Assert.Equal(typeof(GWGUI.MediaEngine.Flux.FluxRevolution), typeof(EncodedTrack).GetProperty(nameof(EncodedTrack.Revolution))!.PropertyType);
        Assert.Equal(typeof(EncodedTrack), typeof(ITrackEncoder).GetMethod(nameof(ITrackEncoder.Encode))!.ReturnType);
    }

    [Fact]
    public void TrackEncoderBaseRejectsNullAndOutOfRangeCoordinates()
    {
        var encoder = new TestTrackEncoder([true]);
        var sector = new TrackSector(1, [0]);

        Assert.Throws<ArgumentNullException>(() => encoder.Encode(null!));
        encoder.Encode(new TrackEncodeRequest(TrackEncodingLimits.MinimumCylinder, TrackEncodingLimits.MinimumHead, [sector]));
        encoder.Encode(new TrackEncodeRequest(TrackEncodingLimits.MaximumCylinder, TrackEncodingLimits.MaximumHead, [sector]));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new TrackEncodeRequest(TrackEncodingLimits.MinimumCylinder - 1, 0, [sector])));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new TrackEncodeRequest(TrackEncodingLimits.MaximumCylinder + 1, 0, [sector])));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new TrackEncodeRequest(0, TrackEncodingLimits.MinimumHead - 1, [sector])));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new TrackEncodeRequest(0, TrackEncodingLimits.MaximumHead + 1, [sector])));
    }

    [Fact]
    public void TrackEncoderBaseRejectsMissingSectorsZeroDurationsAndEmptyOutput()
    {
        var sector = new TrackSector(1, [0]);
        var encoder = new TestTrackEncoder([true]);

        Assert.Throws<ArgumentException>(() => encoder.Encode(new TrackEncodeRequest(0, 0, [])));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new TrackEncodeRequest(0, 0, [sector], BitCellTicks: 0)));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new TrackEncodeRequest(0, 0, [sector], IndexTimeTicks: 0)));
        Assert.Contains("test.base", Assert.Throws<InvalidOperationException>(() => new TestTrackEncoder([]).Encode(new TrackEncodeRequest(0, 0, [sector]))).Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TrackEncoderBaseReadsAttributesAndProducesRequestedFluxDurations()
    {
        var sector = new TrackSector(1, [0], Attributes: new Dictionary<string, int> { ["sector"] = 7 });
        var request = new TrackEncodeRequest(0, 0, [sector], new Dictionary<string, int> { ["track"] = 6 }, BitCellTicks: 5, IndexTimeTicks: 123);
        var encoder = new TestTrackEncoder([false, true]);

        Assert.Equal(6, encoder.RequestAttribute(request, "track", 1));
        Assert.Equal(1, encoder.RequestAttribute(request, "missing", 1));
        Assert.Equal(7, encoder.SectorAttribute(sector, "sector", 2));
        Assert.Equal(2, encoder.SectorAttribute(sector, "missing", 2));
        var encoded = encoder.Encode(request);
        Assert.Equal([false, true], encoded.Bits);
        Assert.Equal((uint)123, encoded.Revolution.IndexTimeTicks);
        Assert.Equal([(uint)10], encoded.Revolution.FluxIntervals);
    }

    private static IReadOnlyDictionary<string, string> CodecDisplayNamesById()
    {
        var identifierFields = typeof(FluxCodecIds).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        var displayNameFields = typeof(FluxCodecDisplayNames).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).ToDictionary(field => field.Name, StringComparer.Ordinal);
        return identifierFields.ToDictionary(field => Assert.IsType<string>(field.GetRawConstantValue()), field => Assert.IsType<string>(displayNameFields[field.Name].GetRawConstantValue()), StringComparer.Ordinal);
    }

    [Theory]
    [MemberData(nameof(RoundTrips))]
    public void EncodedTrackIsRecognizedByMatchingDecoder(string id, int size)
    {
        var data = Enumerable.Range(0, size).Select(index => (byte)(index * 37 + 11)).ToArray();
        var sectorNumber = id == "qdmo5.mfm" ? 0x123 : 3;
        var request = new TrackEncodeRequest(2, 0, [new TrackSector(sectorNumber, data)]);
        var encoded = new FluxEncoderRegistry().Encode(id, request);

        Assert.Equal(id, encoded.EncoderId);
        Assert.NotEmpty(encoded.Bits);
        Assert.IsType<GWGUI.MediaEngine.Flux.FluxRevolution>(encoded.Revolution);
        var decoded = new FluxDecoderRegistry().Decode(id, encoded.Revolution);

        var sector = Assert.Single(decoded.Sectors!);
        Assert.True(sector.IntegrityValid, string.Join(" | ", decoded.Structures.Select(item => item.Description)));
        Assert.Equal(id == "emu.fm" ? 1 : sectorNumber, sector.Number);
        Assert.Equal(id == "qdmo5.mfm" ? 0 : id == "commodore.gcr" ? 3 : 2, sector.Cylinder);
        Assert.Equal(0, sector.Head);
        var payload = sector.Data ?? (id == "commodore.gcr" ? decoded.DecodedBytes.Skip(7).Take(size).ToArray() : decoded.DecodedBytes.TakeLast(size).ToArray());
        Assert.Equal(data, payload);
    }

    [Fact]
    public void AppleDos32FiveAndThreeTrackRoundTrips()
    {
        var sectors = Enumerable.Range(0, 13)
            .Select(number => new TrackSector(number, Enumerable.Range(0, 256)
                .Select(index => (byte)(number * 19 + index * 31)).ToArray()))
            .ToArray();
        var attributes = new Dictionary<string, int> { ["sectorsPerTrack"] = 13 };
        var encoded = new FluxEncoderRegistry().Encode("apple2.gcr", new TrackEncodeRequest(12, 0, sectors, attributes));
        var decoded = new FluxDecoderRegistry().Decode("apple2.gcr", encoded.Revolution);

        Assert.Equal(13, decoded.Sectors!.Count);
        foreach (var expected in sectors)
        {
            var actual = Assert.Single(decoded.Sectors, sector => sector.Number == expected.Number);
            Assert.True(actual.IntegrityValid);
            Assert.Equal(expected.Data, actual.Data);
        }
    }

    [Fact]
    public void AppleRwts18TrackRoundTrips()
    {
        var sectors = Enumerable.Range(0, 6)
            .Select(number => new TrackSector(number, Enumerable.Range(0, 768)
                .Select(index => (byte)(number * 23 + index * 41)).ToArray()))
            .ToArray();
        var encoded = new FluxEncoderRegistry().Encode("apple2.rwts18", new TrackEncodeRequest(18, 0, sectors));
        var decoded = new FluxDecoderRegistry().Decode("apple2.rwts18", encoded.Revolution);

        Assert.Equal(FluxCodecIds.AppleRwts18, decoded.DecoderId);
        Assert.Equal(FluxCodecDisplayNames.AppleRwts18, decoded.DisplayName);
        Assert.True(decoded.Confidence > 0);
        Assert.Equal(6, decoded.Sectors!.Count);
        foreach (var expected in sectors)
        {
            var actual = Assert.Single(decoded.Sectors, sector => sector.Number == expected.Number);
            Assert.True(actual.IntegrityValid, string.Join(" | ", decoded.Structures.Select(item => item.Description)));
            Assert.Equal(expected.Data, actual.Data);
            Assert.Equal(18, actual.Cylinder);
            Assert.Equal(0, actual.Head);
            Assert.Equal(768, actual.SizeBytes);
            Assert.Equal(3, actual.SizeCode);
            Assert.Equal(SectorIntegrityKind.Checksum, actual.IntegrityKind);
        }
    }

    [Theory]
    [InlineData(11)]
    [InlineData(22)]
    public void CompleteAmigaDdAndHdTracksRoundTrip(int sectorCount)
    {
        var sectors = Enumerable.Range(0, sectorCount)
            .Select(number => new TrackSector(number, Enumerable.Range(0, 512).Select(index => (byte)(number * 29 + index * 17)).ToArray()))
            .ToArray();
        var cellTicks = sectorCount == 22 ? 20u : 40u;
        var encoded = new FluxEncoderRegistry().Encode("amiga.mfm", new TrackEncodeRequest(37, 1, sectors, BitCellTicks: cellTicks));
        var decoded = new FluxDecoderRegistry().Decode("amiga.mfm", encoded.Revolution);

        Assert.Equal(sectorCount, decoded.Sectors!.Count);
        foreach (var expected in sectors)
        {
            var actual = Assert.Single(decoded.Sectors, sector => sector.Number == expected.Number);
            Assert.True(actual.IntegrityValid);
            Assert.Equal(expected.Data, actual.Data);
            Assert.Equal(37, actual.Cylinder);
            Assert.Equal(1, actual.Head);
        }
    }

    [Fact]
    public void ArburgDataTrackRoundTrips()
    {
        var data = Enumerable.Range(0, 0x9fe).Select(index => (byte)(index * 7)).ToArray();
        var encoded = new FluxEncoderRegistry().Encode("arburg", new TrackEncodeRequest(0, 0, [new TrackSector(1, data)]));
        var decoded = new FluxDecoderRegistry().Decode("arburg", encoded.Revolution);
        var sector = Assert.Single(decoded.Sectors!);
        Assert.True(sector.IntegrityValid);
        Assert.Equal(0, sector.Cylinder);
        Assert.Equal(0, sector.Head);
        Assert.Equal(1, sector.Number);
        Assert.Equal(data, sector.Data);
        Assert.Equal(data, decoded.DecodedBytes.TakeLast(data.Length));
        Assert.Equal(FluxStructureKind.FormatData, Assert.Single(decoded.Structures).Kind);
        Assert.True(decoded.Confidence > 0);
    }

    [Fact]
    public void ArburgSystemTrackRoundTrips()
    {
        var data = Enumerable.Range(0, 0xefe).Select(index => (byte)(index * 5)).ToArray();
        var attributes = new Dictionary<string, int> { ["system"] = 1 };
        var encoded = new FluxEncoderRegistry().Encode("arburg", new TrackEncodeRequest(0, 0, [new TrackSector(1, data, Attributes: attributes)]));
        var decoded = new FluxDecoderRegistry().Decode("arburg", encoded.Revolution);
        var sector = Assert.Single(decoded.Sectors!);
        Assert.True(sector.IntegrityValid);
        Assert.Equal(0, sector.Cylinder);
        Assert.Equal(0, sector.Head);
        Assert.Equal(1, sector.Number);
        Assert.Equal(data, sector.Data);
        Assert.Equal(data, decoded.DecodedBytes.TakeLast(data.Length));
        Assert.Equal(FluxStructureKind.FormatHeader, Assert.Single(decoded.Structures).Kind);
        Assert.True(decoded.Confidence > 0);
    }

    [Fact]
    public void DecRx02M2FmTrackRoundTrips()
    {
        var data = Enumerable.Range(0, 256).Select(index => (byte)(index * 13)).ToArray();
        var encoded = new FluxEncoderRegistry().Encode("dec.rx02", new TrackEncodeRequest(4, 1, [new TrackSector(6, data)]));
        var decoded = new FluxDecoderRegistry().Decode("dec.rx02", encoded.Revolution);
        var sector = Assert.Single(decoded.Sectors!);
        Assert.True(sector.IntegrityValid, string.Join(" | ", decoded.Structures.Select(item => item.Description)));
        Assert.Equal(data, decoded.DecodedBytes.TakeLast(data.Length));
        Assert.Equal(data, sector.Data);
    }

    [Theory]
    [InlineData("iso.mfm")]
    [InlineData("iso.fm")]
    public void IsoDeletedSectorRoundTrips(string id)
    {
        var data = Enumerable.Range(0, 128).Select(index => (byte)(index + 1)).ToArray();
        var encoded = new FluxEncoderRegistry().Encode(id, new TrackEncodeRequest(1, 0, [new TrackSector(2, data, Deleted: true)]));
        var decoded = new FluxDecoderRegistry().Decode(id, encoded.Revolution);
        Assert.True(Assert.Single(decoded.Sectors!).IntegrityValid);
        Assert.Contains(decoded.Structures, item => item.Kind == FluxStructureKind.DeletedDataAddressMark);
    }

    private sealed class StubTrackEncoder(string id) : ITrackEncoder
    {
        public string Id { get; } = id;
        public string DisplayName => Id;
        public TrackEncodeRequest? LastRequest { get; private set; }
        public EncodedTrack Result { get; } = new(id, [true], GWGUI.MediaEngine.Flux.FluxRevolutionFactory.Create([true], 1, 1));

        public EncodedTrack Encode(TrackEncodeRequest request)
        {
            LastRequest = request;
            return Result;
        }
    }

    private sealed class TestTrackEncoder(IReadOnlyList<bool> bits) : TrackEncoderBase
    {
        public override string Id => "test.base";
        public override string DisplayName => Id;
        protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request) => bits;
        public int RequestAttribute(TrackEncodeRequest request, string key, int fallback) => Attribute(request, key, fallback);
        public int SectorAttribute(TrackSector sector, string key, int fallback) => Attribute(sector, key, fallback);
    }
}
