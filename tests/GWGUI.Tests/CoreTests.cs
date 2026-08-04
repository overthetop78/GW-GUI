using GWGUI.Domain.Commands;
using GWGUI.Domain.Profiles;
using GWGUI.Scp;
using GWGUI.Domain.Formats;
using GWGUI.Domain.Naming;
using GWGUI.Domain.Hardware;
using GWGUI.Domain.Conversion;

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
        byte[] header = [(byte)'S', (byte)'C', (byte)'P', 0x24, 0, 0, 83, 5, 0, 0, 2, 0, 0, 0, 0, 0];
        var result = ScpHeaderReader.Read(header);
        Assert.Equal(84, result.TrackCount);
        Assert.Equal(5, result.Revolutions);
        Assert.Equal(2, result.Heads);
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
}
