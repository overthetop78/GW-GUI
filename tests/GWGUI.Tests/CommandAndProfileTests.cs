using System.IO;
using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.Domain.Commands;
using GWGUI.Domain.Profiles;
using GWGUI.MediaEngine;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.Domain.Formats;
using GWGUI.Domain.Naming;
using GWGUI.Domain.Hardware;
using GWGUI.Domain.Conversion;
using GWGUI.Domain.Read;
using GWGUI.Domain.Write;
using GWGUI.Domain.Maintenance;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.Exploration;
using GWGUI.Infrastructure.Processes;
using GWGUI.Infrastructure.Settings;
using GWGUI.Infrastructure.Hardware;
using GWGUI.Domain.Settings;
using GWGUI.App;
using GWGUI.App.Controls;
using GWGUI.App.ViewModels;
using GWGUI.App.Services;
using GWGUI.App.Rendering;
using GWGUI.App.Localization;
using SkiaSharp;
using System.Windows;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Threading;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace GWGUI.Tests;

public sealed class CommandAndProfileTests : CoreTestBase
{
    [Fact]
    public void DefaultProfileCannotBeRenamedOrDeleted()
    {
        IProfileStore<OperationProfile> store = new InMemoryProfileStore(OperationKind.Read);
        var profile = store.GetAll().Single();
        Assert.Throws<InvalidOperationException>(() => store.Rename(profile.Id, "Autre"));
        Assert.Throws<InvalidOperationException>(() => store.Delete(profile.Id));
    }

    [Fact]
    public void SavingUnderAnotherNameCreatesTheExpectedCopy()
    {
        IProfileStore<OperationProfile> store = new InMemoryProfileStore(OperationKind.Read);
        store.Save(new OperationProfile("p1", OperationKind.Read, "Disquettes récalcitrantes", new Dictionary<string, string>(), new HashSet<string> { "retries" }));
        store.Save(new OperationProfile("p2", OperationKind.Read, "Disquettes Acorn", new Dictionary<string, string>(), new HashSet<string> { "retries" }));
        Assert.Equal(3, store.GetAll().Count);
    }

    [Fact]
    public void ProfileStoreRejectsProfilesFromAnotherTab()
    {
        IProfileStore<OperationProfile> readProfiles = new InMemoryProfileStore(OperationKind.Read);
        var writeProfile = new OperationProfile("write-1", OperationKind.Write, "Même nom autorisé ailleurs", new Dictionary<string, string>(), new HashSet<string>());

        Assert.Throws<ArgumentException>(() => readProfiles.Save(writeProfile));
        Assert.Throws<ArgumentException>(() => new InMemoryProfileStore(OperationKind.Read, [writeProfile]));
        Assert.Single(readProfiles.GetAll());
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
            [new EnabledOption("--revs", "5"), new EnabledOption("--tracks", "c=0-79:h=0-1")], "COM3", null));
        Assert.Equal(["--device", "COM3", "--revs", "5", "--tracks", "c=0-79:h=0-1", "disk.scp"], command.Arguments);
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
    [InlineData("disk.adf", 820224, "acorn.adfs.800")]
    [InlineData("disk.adf", 1802240, "amiga.amigados_hd")]
    [InlineData("disk.st", 368640, "atarist.360")]
    [InlineData("disk.st", 901120, "atarist.880")]
    [InlineData("disk.ima", 163840, "ibm.160")]
    [InlineData("disk.ima", 1228800, "ibm.1200")]
    [InlineData("disk.ima", 1474560, "ibm.1440")]
    [InlineData("disk.img", 1720320, "ibm.1680")]
    [InlineData("disk.img", 2949120, "ibm.2880")]
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
    public void AdvancedReadOptionsRemainSeparateCommandArguments()
    {
        EnabledOption[] options = [new("--seek-retries", "2"), new("--fake-index", "300rpm"), new("--adjust-speed", "360rpm"), new("--pll", "period=5:phase=60"), new("--reverse"), new("--densel", "L")];
        var command = ReadCommandBuilder.Build(new ReadRequest("gw.exe", "disk.scp", ReadResultKind.RawScp, null, options));
        Assert.Equal(["--seek-retries", "2", "--fake-index", "300rpm", "--adjust-speed", "360rpm", "--pll", "period=5:phase=60", "--reverse", "--densel", "L", "disk.scp"], command.Arguments);
    }

    [Fact]
    public void AdvancedWriteOptionsRemainSeparateCommandArguments()
    {
        EnabledOption[] options = [new("--tracks", "c=0-79:h=0-1"), new("--pre-erase"), new("--precomp", "type=mfm:40=125"), new("--hard-sectors"), new("--gen-tg43")];
        var command = WriteCommandBuilder.Build(new WriteRequest("gw.exe", "disk.adf", "amiga.amigados", options));
        Assert.Equal(["--format", "amiga.amigados", "--tracks", "c=0-79:h=0-1", "--pre-erase", "--precomp", "type=mfm:40=125", "--hard-sectors", "--gen-tg43", "disk.adf"], command.Arguments);
    }

    [Fact]
    public void AdvancedConversionOptionsRemainSeparateCommandArguments()
    {
        var output = new ConversionOutput("ibm.720", ".ima", "out/disk.ima", true);
        EnabledOption[] options = [new("--tracks", "c=0-79:h=0-1"), new("--out-tracks", "c=0-39:h=0"), new("--adjust-speed", "300rpm"), new("--pll", "period=5:phase=60"), new("--reverse")];
        var command = ConversionCommandBuilder.Build("gw.exe", "source.scp", output, options);
        Assert.Equal(["--format", "ibm.720", "--tracks", "c=0-79:h=0-1", "--out-tracks", "c=0-39:h=0", "--adjust-speed", "300rpm", "--pll", "period=5:phase=60", "--reverse", "source.scp", "out/disk.ima"], command.Arguments);
    }

    [Theory]
    [InlineData("atarist.810", "disk.st")]
    [InlineData("amstrad.cpc", "disk.dsk")]
    [InlineData("amstrad.pcw", "disk.dsk")]
    public void CommandsUseTheBundledDiskDefinition(string formatId, string outputPath)
    {
        var read = ReadCommandBuilder.Build(new ReadRequest("gw.exe", outputPath, ReadResultKind.KnownFormat, formatId, []));
        var write = WriteCommandBuilder.Build(new WriteRequest("gw.exe", outputPath, formatId, []));
        var convert = ConversionCommandBuilder.Build("gw.exe", "source.scp", new(formatId, Path.GetExtension(outputPath), outputPath, true));

        foreach (var command in new[] { read, write, convert })
        {
            Assert.Contains("--diskdefs", command.Arguments);
            Assert.Contains(BuiltInDiskDefinitions.FilePath, command.Arguments);
            Assert.Contains(formatId, command.Arguments);
        }
    }

    [Fact]
    public void CustomDiskDefinitionOverridesTheBundledDefinition()
    {
        EnabledOption[] options = [new("--diskdefs", "custom.cfg")];
        var command = WriteCommandBuilder.Build(new WriteRequest("gw.exe", "disk.st", "atarist.810", options));

        Assert.Equal(1, command.Arguments.Count(argument => argument == "--diskdefs"));
        Assert.Contains("custom.cfg", command.Arguments);
        Assert.DoesNotContain(BuiltInDiskDefinitions.FilePath, command.Arguments);
    }

    [Theory]
    [InlineData("--revs", "0")]
    [InlineData("--retries", "-1")]
    [InlineData("--tracks", "")]
    [InlineData("--densel", "X")]
    public void InvalidStructuredOptionValuesAreRejected(string argument, string value)
    {
        Assert.Throws<ArgumentException>(() => ReadCommandBuilder.Build(new ReadRequest("gw.exe", "disk.scp", ReadResultKind.RawScp, null, [new EnabledOption(argument, value)])));
    }

    [Fact]
    public void MutuallyExclusiveStructuredOptionsAreRejected()
    {
        Assert.Throws<ArgumentException>(() => ReadCommandBuilder.Build(new ReadRequest("gw.exe", "disk.scp", ReadResultKind.RawScp, null, [new EnabledOption("--fake-index", "300rpm"), new EnabledOption("--hard-sectors")])));
        Assert.Throws<ArgumentException>(() => WriteCommandBuilder.Build(new WriteRequest("gw.exe", "disk.adf", null, [new EnabledOption("--densel", "H"), new EnabledOption("--gen-tg43")])));
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
    public void ConversionTagsAreStableAndIndependentFromTranslatedLabels()
    {
        var catalog = new BuiltInImageFormatCatalog(key => "translated:" + key);
        var output = Assert.Single(new ConversionPlanner(catalog).Plan("disk.scp", "out", "disk", [new ConversionSelection("ibm.720", new HashSet<string>())], true));
        Assert.Equal(Path.Combine("out", "[PC-720] disk.ima"), output.OutputPath);
    }

    [Fact]
    public void MaintenanceDefaultsDoNotEmitOptionalArguments()
    {
        Assert.Empty(MaintenanceCommandBuilder.Erase(new EraseRequest("gw.exe", [])).Arguments);
        Assert.Empty(MaintenanceCommandBuilder.Clean(new CleanRequest("gw.exe")).Arguments);
    }

    [Fact]
    public void CentralCommandBuilderCoversEveryApplicationOperation()
    {
        IGwCommandBuilder builder = new GwCommandBuilder();

        Assert.Equal("read", builder.BuildRead(new("gw.exe", "disk.scp", ReadResultKind.RawScp, null, [])).Verb);
        Assert.Equal("write", builder.BuildWrite(new("gw.exe", "disk.adf", "amiga.amigados", [])).Verb);
        Assert.Equal("convert", builder.BuildConversion("gw.exe", "disk.scp", new("ibm.720", ".ima", "disk.ima", true)).Verb);
        Assert.Equal("erase", builder.BuildErase(new("gw.exe", [])).Verb);
        Assert.Equal("clean", builder.BuildClean(new("gw.exe")).Verb);
        Assert.Equal("rpm", builder.BuildTool(new("gw.exe", "rpm", new Dictionary<string, string> { ["nr"] = "1" }, new HashSet<string>())).Verb);
        Assert.Equal(["--device", "COM9", "--bootloader"], builder.BuildInfo(new("gw.exe", "COM9", true)).Arguments);
    }

    [Fact]
    public void CleaningOptionsAreMappedExplicitly()
    {
        var command = MaintenanceCommandBuilder.Clean(new CleanRequest("gw.exe", 80, 3, 100));
        Assert.Equal(["--cylinders", "80", "--passes", "3", "--linger", "100"], command.Arguments);
    }

    [Fact]
    public void DiagnosticToolCommandsAreValidatedAndRouted()
    {
        var rpm = ToolCommandBuilder.Build(new("gw.exe", "rpm", new Dictionary<string, string> { ["nr"] = "3" }, new HashSet<string>(), "COM7", "B"));
        Assert.Equal(["--nr", "3", "--device", "COM7", "--drive", "B"], rpm.Arguments);
        var pin = ToolCommandBuilder.Build(new("gw.exe", "pin", new Dictionary<string, string> { ["pin"] = "26" }, new HashSet<string> { "set", "high" }, "COM7"));
        Assert.Equal(["set", "26", "H", "--device", "COM7"], pin.Arguments);
        Assert.Throws<ArgumentOutOfRangeException>(() => ToolCommandBuilder.Build(new("gw.exe", "pin", new Dictionary<string, string> { ["pin"] = "12" }, new HashSet<string>())));
        Assert.Throws<ArgumentOutOfRangeException>(() => ToolCommandBuilder.Build(new("gw.exe", "rpm", new Dictionary<string, string> { ["nr"] = "0" }, new HashSet<string>())));
    }

    [Fact]
    public void DelayToolCommandIncludesOnlyEnabledNonNegativeValues()
    {
        var values = new Dictionary<string, string> { ["select"] = "10", ["step"] = "3000" };
        var command = ToolCommandBuilder.Build(new("gw.exe", "delays", values, new HashSet<string> { "step" }));
        Assert.Equal(["--step", "3000"], command.Arguments);
        Assert.Throws<ArgumentOutOfRangeException>(() => ToolCommandBuilder.Build(new("gw.exe", "delays", new Dictionary<string, string> { ["step"] = "-1" }, new HashSet<string> { "step" })));
    }

    [Fact]
    public void AlignCommandCoversRequiredAndAdvancedOptions()
    {
        var values = new Dictionary<string, string>
        {
            ["tracks"] = "c=40:h=0-1", ["revs"] = "3", ["reads"] = "10",
            ["format"] = "ibm.720", ["adjust-speed"] = "300rpm", ["densel"] = "H"
        };
        var enabled = new HashSet<string> { "format", "adjust-speed", "densel", "reverse" };
        var command = ToolCommandBuilder.Build(new("gw.exe", "align", values, enabled, "COM7", "B"));

        Assert.Equal("align", command.Verb);
        Assert.Equal(["--tracks", "c=40:h=0-1", "--revs", "3", "--reads", "10", "--format", "ibm.720", "--adjust-speed", "300rpm", "--densel", "H", "--reverse", "--device", "COM7", "--drive", "B"], command.Arguments);
    }

    [Fact]
    public void AlignCommandRejectsInvalidOrExclusiveOptions()
    {
        var values = new Dictionary<string, string> { ["tracks"] = "c=40:h=0", ["revs"] = "3", ["reads"] = "10", ["fake-index"] = "300rpm" };
        Assert.Throws<ArgumentException>(() => ToolCommandBuilder.Build(new("gw.exe", "align", values, new HashSet<string> { "fake-index", "hard-sectors" })));
        Assert.Throws<ArgumentException>(() => ToolCommandBuilder.Build(new("gw.exe", "align", new Dictionary<string, string> { ["tracks"] = "", ["revs"] = "3", ["reads"] = "10" }, new HashSet<string>())));
        Assert.Throws<ArgumentOutOfRangeException>(() => ToolCommandBuilder.Build(new("gw.exe", "align", new Dictionary<string, string> { ["tracks"] = "c=40:h=0", ["revs"] = "0", ["reads"] = "10" }, new HashSet<string>())));
    }

    [Theory]
    [InlineData("c=0-79:h=0-1")]
    [InlineData("c=0-39/2,41:h=0:step=2:hswap:h0.off=+1")]
    [InlineData("c=0-79:h=0-1:step=1/2:h1.off=-2")]
    public void TrackSpecificationsFollowGreaseweazleGrammar(string value) => GwOptionValidator.ValidateTrackSpec(value);

    [Theory]
    [InlineData("c=79-0:h=0-1")]
    [InlineData("c=0-79:h=2")]
    [InlineData("c=0-79:h=0-1:step=0")]
    [InlineData("c=0-79")]
    [InlineData("c=0-79:h=0-1:unknown=1")]
    public void InvalidTrackSpecificationsAreRejected(string value) => Assert.Throws<ArgumentException>(() => GwOptionValidator.ValidateTrackSpec(value));

    [Fact]
    public void PllPrecompensationAndSpeedSpecificationsAreValidated()
    {
        GwOptionValidator.ValidatePllSpec("period=5:phase=60:lowpass=1.5");
        GwOptionValidator.ValidatePrecompSpec("type=mfm:40=125:60=150");
        foreach (var speed in new[] { "300rpm", "200ms", "40000000scp", ".5ms", "300" }) GwOptionValidator.ValidateSpeed(speed);
        Assert.Throws<ArgumentException>(() => GwOptionValidator.ValidatePllSpec("period=five:phase=60"));
        Assert.Throws<ArgumentException>(() => GwOptionValidator.ValidatePllSpec("period=5:jitter=2"));
        Assert.Throws<ArgumentException>(() => GwOptionValidator.ValidatePrecompSpec("type=wrong:40=125"));
        Assert.Throws<ArgumentException>(() => GwOptionValidator.ValidatePrecompSpec("type=mfm"));
        Assert.Throws<ArgumentException>(() => GwOptionValidator.ValidateSpeed("300xyz"));
    }
}
