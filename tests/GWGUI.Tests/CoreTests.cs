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
}
