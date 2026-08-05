using System.IO;
using GWGUI.Domain.Commands;
using GWGUI.Domain.Profiles;
using GWGUI.Scp;
using GWGUI.Domain.Formats;
using GWGUI.Domain.Naming;
using GWGUI.Domain.Hardware;
using GWGUI.Domain.Conversion;
using GWGUI.Domain.Read;
using GWGUI.Domain.Write;
using GWGUI.Domain.Maintenance;
using GWGUI.Scp.Decoding;

namespace GWGUI.Tests;

public sealed class CoreTests
{
    [Fact]
    public void GwHelpCapabilitiesAreParsedBySection()
    {
        const string help = """
            options:
              --format FORMAT

            FORMAT options:
              acorn.adfs.800  amiga.amigados  amiga.amigadoshd
              atarist.720     ibm.720         ibm.scan

            Supported file suffixes:
              .adf  .hfe  .ima  .img  .scp
            """;

        var capabilities = GwFormatCapabilitiesParser.ParseReadHelp(help);

        Assert.Contains("amiga.amigados", capabilities.FormatIds);
        Assert.Contains("ibm.scan", capabilities.FormatIds);
        Assert.DoesNotContain("--format", capabilities.FormatIds);
        Assert.Equal(6, capabilities.FormatIds.Count);
        Assert.Contains(".scp", capabilities.ImageExtensions);
        Assert.Equal(5, capabilities.ImageExtensions.Count);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("unrelated output")]
    public void MissingHelpSectionsReturnUnknownCapabilities(string? help)
    {
        Assert.False(GwFormatCapabilitiesParser.ParseReadHelp(help).IsKnown);
    }

    [Fact]
    public void RuntimeCapabilitiesFilterCuratedFormatsAndExtensions()
    {
        var capabilities = new GwFormatCapabilities(
            new HashSet<string>(["ibm.720"], StringComparer.OrdinalIgnoreCase),
            new HashSet<string>([".scp", ".img"], StringComparer.OrdinalIgnoreCase));

        var catalog = new CapabilityAwareImageFormatCatalog(new BuiltInImageFormatCatalog(), capabilities);

        Assert.Contains(catalog.Formats, format => format.Id == "raw.scp");
        var ibm = Assert.Single(catalog.Formats, format => format.Id == "ibm.720");
        Assert.Equal(".img", Assert.Single(ibm.Extensions).Extension);
        Assert.True(ibm.Extensions[0].IsDefault);
        Assert.DoesNotContain(catalog.Formats, format => format.Id == "atarist.720");
    }

    [Fact]
    public void CuratedCatalogContainsOfficialIbmAndAtariProfiles()
    {
        var catalog = new BuiltInImageFormatCatalog();
        string[] ibm = ["ibm.160", "ibm.180", "ibm.320", "ibm.360", "ibm.720", "ibm.800", "ibm.1200", "ibm.1440", "ibm.1680", "ibm.dmf", "ibm.2880", "ibm.scan"];
        string[] atari = ["atarist.360", "atarist.400", "atarist.440", "atarist.720", "atarist.800", "atarist.880"];

        Assert.All(ibm.Concat(atari), id => Assert.Contains(catalog.Formats, format => format.Id == id));
        Assert.Contains(catalog.Formats, format => format.Id == "amiga.amigados_hd");
        Assert.DoesNotContain(catalog.Formats, format => format.Id == "amiga.amigadoshd");
        Assert.All(catalog.Formats.Where(format => format.Family == "IBM PC"), format =>
            Assert.Equal(".ima", Assert.Single(format.Extensions, extension => extension.IsDefault).Extension));
    }

    [Fact]
    public void DisplayCommandQuotesPathsWithSpaces()
    {
        var command = new GwCommand("C:\\GW Tools\\gw.exe", "read", ["F:\\Disk Images\\My disk.scp"]);
        Assert.Equal("\"C:\\GW Tools\\gw.exe\" read \"F:\\Disk Images\\My disk.scp\"", command.ToDisplayString());
    }

    [Fact]
    public void DefaultProfileHasNoOptionalArguments()
    {
        var profile = OperationProfile.Default(OperationKind.Read);
        Assert.True(profile.IsSystem);
        Assert.Empty(profile.EnabledOptions);
        Assert.Empty(profile.Values);
    }

    [Fact]
    public void ScpHeaderReaderReadsCoreMetadata()
    {
        byte[] header = [(byte)'S', (byte)'C', (byte)'P', 0x24, 0, 5, 0, 83, 0, 0, 0, 0, 0, 0, 0, 0];
        var result = ScpHeaderReader.Read(header);
        Assert.Equal(84, result.TrackCount);
        Assert.Equal(5, result.Revolutions);
        Assert.Equal(0, result.Heads);
    }

    [Fact]
    public void ScpReaderReadsTrackRevolutionAndBigEndianFluxOverflow()
    {
        var data = new byte[0x2b0 + 16 + 6];
        data[0] = (byte)'S'; data[1] = (byte)'C'; data[2] = (byte)'P'; data[3] = 0x25; data[5] = 1; data[6] = 0; data[7] = 0;
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x10, 4), 0x2b0);
        data[0x2b0] = (byte)'T'; data[0x2b1] = (byte)'R'; data[0x2b2] = (byte)'K'; data[0x2b3] = 0;
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x2b4, 4), 8_000_000);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x2b8, 4), 3);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x2bc, 4), 16);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(0x2c0, 2), 100);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(0x2c2, 2), 0);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(0x2c4, 2), 50);
        uint checksum = 0; foreach (var value in data.AsSpan(0x10)) checksum = unchecked(checksum + value);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x0c, 4), checksum);
        var image = new ScpReader().Read(data);
        Assert.True(image.ChecksumValid);
        Assert.Equal([100u, 65_586u], image.Tracks[0].Revolutions[0].FluxIntervals);
        Assert.Equal(300d, image.Tracks[0].Revolutions[0].Rpm(image.Header.ResolutionNanoseconds), 3);
    }

    [Theory]
    [InlineData(0, "A")]
    [InlineData(25, "Z")]
    [InlineData(26, "AA")]
    [InlineData(27, "AB")]
    public void AlphabeticSequenceContinuesAfterZ(long value, string expected) =>
        Assert.Equal(expected, SequenceFormatter.Format(value, SequenceKind.Alphabetic, 1));

    [Fact]
    public void ExplicitExtensionCanReplaceImplicitImaDefault()
    {
        var format = new BuiltInImageFormatCatalog().Formats.Single(x => x.Id == "ibm.720");
        Assert.Equal(".ima", format.Extensions.Single(x => x.IsDefault).Extension);
        Assert.Contains(format.Extensions, x => x.Extension == ".img");
    }

    [Fact]
    public void ScpCanBeDecodedIntoAllKnownOutputFamilies()
    {
        var outputs = new BuiltInImageFormatCatalog().GetCompatibleOutputs(".scp");
        Assert.Contains(outputs, x => x.Id == "amiga.amigados");
        Assert.Contains(outputs, x => x.Id == "atarist.720");
        Assert.Contains(outputs, x => x.Id == "ibm.720");
    }

    [Fact]
    public void DeviceInfoSurvivesAnUnrelatedNetworkFailure()
    {
        const string output = "Host Tools: 1.23\nCOM3\nModel: Greaseweazle V4.1\nMCU: AT32F403A\nFirmware: 1.6\nSerial: GW0CF19C9E7592000007E0941B\nUSB: Full Speed (12 Mbit/s)\nError contacting github";
        var info = GwInfoParser.Parse(output);
        Assert.Equal("COM3", info.Port);
        Assert.Equal("Greaseweazle V4.1", info.Model);
        Assert.Equal("GW0CF19C9E7592000007E0941B", info.SerialNumber);
        Assert.True(info.HasNetworkWarning);
    }

    [Fact]
    public void DeviceInfoReadsCurrentIndentedPortLine()
    {
        var info = GwInfoParser.Parse("Host Tools: 1.23\nDevice:\n  Port:      COM12\n  Model:     Greaseweazle V4.1\n  Serial:    GW123");
        Assert.Equal("COM12", info.Port);
        Assert.Equal("Greaseweazle V4.1", info.Model);
        Assert.Equal("GW123", info.SerialNumber);
    }

    [Fact]
    public void WindowPlacementRejectsAWindowOutsideAllScreens()
    {
        var settings = new GWGUI.Domain.Settings.WindowPlacementSettings { Width = 1400, Height = 800, Left = 9000, Top = 9000 };
        var result = GWGUI.Domain.Settings.WindowPlacementPolicy.Normalize(settings, 1280, 720, 0, 0, 3840, 2160);
        Assert.Null(result.Left);
        Assert.Null(result.Top);
    }

    [Fact]
    public void WindowPlacementKeepsAVisibleSecondaryScreenPosition()
    {
        var settings = new GWGUI.Domain.Settings.WindowPlacementSettings { Width = 1400, Height = 800, Left = -1500, Top = 120 };
        var result = GWGUI.Domain.Settings.WindowPlacementPolicy.Normalize(settings, 1280, 720, -1920, 0, 5760, 2160);
        Assert.Equal(-1500, result.Left);
        Assert.Equal(120, result.Top);
    }

    [Fact]
    public void GwProgressCountsUniqueTracksAndIgnoresRetries()
    {
        var tracker = new GwProgressTracker();
        Assert.Null(tracker.Accept("Reading c=0-79:h=0-1 revs=3"));
        var first = tracker.Accept("T0.0: Raw Flux");
        var retry = tracker.Accept("T0.0: Retry #1.1");
        var second = tracker.Accept("T0.1: Raw Flux");
        Assert.Equal(160, first!.TotalTracks);
        Assert.Equal(1, retry!.CompletedTracks);
        Assert.Equal(2, second!.CompletedTracks);
    }

    [Fact]
    public void GwProgressUnderstandsSteppedAndCommaSeparatedTrackSets()
    {
        var tracker = new GwProgressTracker();
        tracker.Accept("Writing c=0-39/2,41:h=0");
        var progress = tracker.Accept("T0.0: Writing Track");
        Assert.Equal(21, progress!.TotalTracks);
    }

    [Fact]
    public void DefaultProfileCannotBeRenamedOrDeleted()
    {
        var store = new InMemoryProfileStore();
        var profile = store.Get(OperationKind.Read).Single();
        Assert.Throws<InvalidOperationException>(() => store.Rename(OperationKind.Read, profile.Id, "Autre"));
        Assert.Throws<InvalidOperationException>(() => store.Delete(OperationKind.Read, profile.Id));
    }

    [Fact]
    public void SavingUnderAnotherNameCreatesTheExpectedCopy()
    {
        var store = new InMemoryProfileStore();
        store.Save(new OperationProfile("p1", OperationKind.Read, "Disquettes récalcitrantes", new Dictionary<string, string>(), new HashSet<string> { "retries" }));
        store.Save(new OperationProfile("p2", OperationKind.Read, "Disquettes Acorn", new Dictionary<string, string>(), new HashSet<string> { "retries" }));
        Assert.Equal(3, store.Get(OperationKind.Read).Count);
    }

    [Fact]
    public void NoExplicitExtensionUsesImaWithoutCheckingIt()
    {
        var planner = new ConversionPlanner(new BuiltInImageFormatCatalog());
        var outputs = planner.Plan("disk.scp", "out", "disk", [new ConversionSelection("ibm.720", new HashSet<string>())], false);
        Assert.Single(outputs);
        Assert.Equal(".ima", outputs[0].Extension);
        Assert.True(outputs[0].UsesImplicitExtension);
    }

    [Fact]
    public void ExplicitImgReplacesImplicitImaAndBothCanBeRequested()
    {
        var planner = new ConversionPlanner(new BuiltInImageFormatCatalog());
        var imgOnly = planner.Plan("disk.scp", "out", "disk", [new ConversionSelection("ibm.720", new HashSet<string> { ".img" })], false);
        Assert.Equal([".img"], imgOnly.Select(x => x.Extension));
        var both = planner.Plan("disk.scp", "out", "disk", [new ConversionSelection("ibm.720", new HashSet<string> { ".ima", ".img" })], false);
        Assert.Equal(2, both.Count);
    }

    [Fact]
    public void DefaultReadAddsNoOptionalGwArguments()
    {
        var command = ReadCommandBuilder.Build(new ReadRequest("gw.exe", "disk.scp", ReadResultKind.RawScp, null, []));
        Assert.Equal(["disk.scp"], command.Arguments);
    }

    [Fact]
    public void OnlyEnabledReadOptionsAreEmitted()
    {
        var command = ReadCommandBuilder.Build(new ReadRequest("gw.exe", "disk.scp", ReadResultKind.RawScp, null,
            [new EnabledOption("--revs", "5"), new EnabledOption("--tracks", "0-79:c=0-79:h=0-1")], "COM3", null));
        Assert.Equal(["--device", "COM3", "--revs", "5", "--tracks", "0-79:c=0-79:h=0-1", "disk.scp"], command.Arguments);
    }

    [Fact]
    public void ExpertArgumentsPreserveQuotedValues()
    {
        var command = ReadCommandBuilder.Build(new ReadRequest("gw.exe", "disk.scp", ReadResultKind.RawScp, null, [], ExpertArguments: "--fake-index 300 --tracks \"c=0-79:h=0-1\""));
        Assert.Equal(["--fake-index", "300", "--tracks", "c=0-79:h=0-1", "disk.scp"], command.Arguments);
    }

    [Fact]
    public void NextNameSkipsExistingSequences()
    {
        var occupied = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { Path.Combine("out", "Disk 01.scp"), Path.Combine("out", "Disk 02.scp") };
        var result = OutputConflictResolver.FindNextAvailable("out", "Disk", ".scp", SequenceKind.Numeric, 2, 1, occupied.Contains);
        Assert.Equal(Path.Combine("out", "Disk 03.scp"), result);
    }

    [Theory]
    [InlineData("disk.adf", 901120, "amiga.amigados")]
    [InlineData("disk.adf", 819200, "acorn.adfs.800")]
    [InlineData("disk.ima", 1474560, "ibm.1440")]
    public void WriteDetectorUsesContainerSizeToResolveAmbiguity(string name, long length, string formatId)
    {
        var result = new ImageFormatDetector(new BuiltInImageFormatCatalog()).Detect(name, length);
        Assert.Equal(formatId, result.Format?.Id);
        Assert.False(result.RequiresUserChoice);
    }

    [Fact]
    public void UnknownImgGeometryRequiresExplicitChoice()
    {
        var result = new ImageFormatDetector(new BuiltInImageFormatCatalog()).Detect("disk.img", 12345);
        Assert.True(result.RequiresUserChoice);
    }

    [Fact]
    public void WriteVerificationIsEnabledUnlessNoVerifyWasExplicitlySelected()
    {
        var normal = WriteCommandBuilder.Build(new WriteRequest("gw.exe", "disk.adf", "amiga.amigados", []));
        Assert.DoesNotContain("--no-verify", normal.Arguments);
        var unsafeCommand = WriteCommandBuilder.Build(new WriteRequest("gw.exe", "disk.adf", "amiga.amigados", [], DisableVerify: true));
        Assert.Contains("--no-verify", unsafeCommand.Arguments);
    }

    [Fact]
    public void ConversionCommandUsesSelectedFormatAndSeparatePaths()
    {
        var output = new ConversionOutput("atarist.720", ".st", "out/disk.st", true);
        var command = ConversionCommandBuilder.Build("gw.exe", "source.scp", output);
        Assert.Equal(["--format", "atarist.720", "source.scp", "out/disk.st"], command.Arguments);
    }

    [Fact]
    public void ConversionTagsPreventSameExtensionOutputsFromColliding()
    {
        var planner = new ConversionPlanner(new BuiltInImageFormatCatalog());
        var outputs = planner.Plan("disk.scp", "out", "disk", [new ConversionSelection("amiga.amigados", new HashSet<string>()), new ConversionSelection("acorn.adfs.800", new HashSet<string>())], true);
        Assert.Equal(2, outputs.Select(x => x.OutputPath).Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Fact]
    public void MaintenanceDefaultsDoNotEmitOptionalArguments()
    {
        Assert.Empty(MaintenanceCommandBuilder.Erase(new EraseRequest("gw.exe", [])).Arguments);
        Assert.Empty(MaintenanceCommandBuilder.Clean(new CleanRequest("gw.exe")).Arguments);
    }

    [Fact]
    public void CleaningOptionsAreMappedExplicitly()
    {
        var command = MaintenanceCommandBuilder.Clean(new CleanRequest("gw.exe", 80, 3, 100));
        Assert.Equal(["--cylinders", "80", "--passes", "3", "--linger", "100"], command.Arguments);
    }

    [Fact]
    public void AmigaDecoderFindsTheDouble4489SyncWord()
    {
        var bits = Convert.ToString(0x4489, 2).PadLeft(16, '0') + Convert.ToString(0x4489, 2).PadLeft(16, '0');
        var intervals = new List<uint>(); var sinceTransition = 0;
        foreach (var bit in bits) { sinceTransition++; if (bit == '1') { intervals.Add((uint)(sinceTransition * 40)); sinceTransition = 0; } }
        var result = new AmigaMfmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));
        Assert.Contains(result.Structures, x => x.Kind == FluxStructureKind.AmigaSync);
        Assert.True(result.Confidence > 0);
    }

    [Fact]
    public void IsoMfmDecoderExtractsSectorIdentityAndHeaderCrc()
    {
        byte[] header = [0xa1, 0xa1, 0xa1, 0xfe, 0, 1, 2, 2]; var crc = TestCrc16(header);
        var raw = Convert.ToString(0x4489, 2).PadLeft(16, '0') + Convert.ToString(0x4489, 2).PadLeft(16, '0') + Convert.ToString(0x4489, 2).PadLeft(16, '0') +
                  EncodeMfmBytes(0xfe, 0, 1, 2, 2, (byte)(crc >> 8), (byte)crc) + "001";
        var intervals = BitsToIntervals(raw, 40);
        var result = new IsoMfmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));
        var sector = Assert.Single(result.Sectors!);
        Assert.Equal(2, sector.Number); Assert.Equal(512, sector.SizeBytes); Assert.True(sector.HeaderCrcValid);
    }

    [Fact]
    public void IsoFmDecoderExtractsSingleDensitySectorHeader()
    {
        byte[] header = [0xfe, 3, 0, 7, 1]; var crc = TestCrc16(header);
        var raw = Convert.ToString(0xf57e, 2).PadLeft(16, '0') + EncodeFmBytes(3, 0, 7, 1, (byte)(crc >> 8), (byte)crc) + "001";
        var intervals = BitsToIntervals(raw, 40); var result = new IsoFmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));
        var sector = Assert.Single(result.Sectors!); Assert.Equal(7, sector.Number); Assert.Equal(256, sector.SizeBytes); Assert.True(sector.HeaderCrcValid);
    }

    private static string EncodeMfmBytes(params byte[] values) { var result = new System.Text.StringBuilder(); var previous = 1; foreach (var value in values) for (var bit = 7; bit >= 0; bit--) { var data = (value >> bit) & 1; var clock = previous == 0 && data == 0 ? 1 : 0; result.Append(clock).Append(data); previous = data; } return result.ToString(); }
    private static string EncodeFmBytes(params byte[] values) => string.Concat(values.SelectMany(value => Enumerable.Range(0, 8).Select(bit => "1" + (((value >> (7 - bit)) & 1) != 0 ? "1" : "0"))));
    private static List<uint> BitsToIntervals(string bits, uint cellTicks) { var result = new List<uint>(); var cells = 0; foreach (var bit in bits) { cells++; if (bit == '1') { result.Add((uint)cells * cellTicks); cells = 0; } } return result; }
    private static ushort TestCrc16(IEnumerable<byte> values) { ushort crc = 0xffff; foreach (var value in values) { crc ^= (ushort)(value << 8); for (var bit = 0; bit < 8; bit++) crc = (ushort)((crc & 0x8000) != 0 ? (crc << 1) ^ 0x1021 : crc << 1); } return crc; }
}
