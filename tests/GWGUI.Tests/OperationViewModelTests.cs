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

public sealed class OperationViewModelTests : CoreTestBase
{
    [Fact]
    public void ConversionTagPatternIsAppliedWithoutForcingBrackets()
    {
        var planner = new ConversionPlanner(new BuiltInImageFormatCatalog());
        var output = Assert.Single(planner.Plan("disk.scp", "out", "disk", [new ConversionSelection("ibm.720", new HashSet<string>())], true, "TAG-{tag} "));
        Assert.Equal("TAG-PC-720 disk.ima", Path.GetFileName(output.OutputPath));
        Assert.Throws<ArgumentException>(() => planner.Plan("disk.scp", "out", "disk", [new ConversionSelection("ibm.720", new HashSet<string>())], true, "_format"));
    }

    [Fact]
    public void ConversionTagVariablesProduceDeterministicFilenameSafeNames()
    {
        var planner = new ConversionPlanner(new BuiltInImageFormatCatalog());
        var familyFormat = Assert.Single(planner.Plan("disk.scp", "out", "disk", [new ConversionSelection("ibm.720", new HashSet<string>())], true, "[{FAMILY}-{FORMAT}-{EXTENSION}] "));
        Assert.Equal("[PC-720-IMA] disk.ima", Path.GetFileName(familyFormat.OutputPath));

        var format = new DiskFormat("ibm.720", "IBM PC", "IBM PC 720", [new ImageExtension(".ima", "IMA", true)], Tag: "PC-720");
        var rendered = ConversionPlanner.FormatTag("{NAME}_{DATE:YYYY-MM-DD}_{TIME:HH-MM-SS}_{TAG}", format, ".ima", "disk", new DateTime(2026, 8, 6, 14, 35, 42));
        Assert.Equal("disk_2026-08-06_14-35-42_PC-720", rendered);
    }

    [Fact]
    public void ReadViewModelBuildsNumericAndAlphabeticTargetsAndAdvancesOnlyWhenRequested()
    {
        var model = new GWGUI.App.ViewModels.ReadOperationViewModel
        {
            Folder = Path.Combine("images", "magazines"),
            FileName = "Tilt",
            AutoNumber = true,
            SequenceWidthIndex = 1,
            SequenceValue = "9"
        };

        Assert.Equal(Path.Combine("images", "magazines", "Tilt 09.scp"), model.BuildTarget(".scp", "Exemple"));
        Assert.True(model.TryAdvanceSequence());
        Assert.Equal("10", model.SequenceValue);

        model.SequenceKindIndex = 1;
        model.SequenceValue = "Z";
        Assert.Equal(Path.Combine("images", "magazines", "Tilt AZ.scp"), model.BuildTarget(".scp", "Exemple"));
        Assert.True(model.TryAdvanceSequence());
        Assert.Equal("AA", model.SequenceValue);
    }

    [Fact]
    public void ReadViewModelUsesExampleWithoutMutatingAnEmptyName()
    {
        var model = new GWGUI.App.ViewModels.ReadOperationViewModel { Folder = "images", FileName = "   " };
        Assert.Equal(Path.Combine("images", "Exemple.scp"), model.BuildTarget(".scp", "Exemple"));
        Assert.False(model.TryAdvanceSequence());
        Assert.Equal("   ", model.FileName);
    }

    [Fact]
    public void ReadViewModelDefaultProfileRemovesEveryOptionalGwArgument()
    {
        var model = new ReadOperationViewModel();
        model.ApplyOptions(new HashSet<string> { "revs", "tracks", "reverse", "diskdefs" }, new Dictionary<string, string>
        {
            ["revs"] = "3", ["tracks"] = "c=0-39:h=0", ["diskdefs"] = "custom.cfg", ["expert"] = "--raw"
        });
        Assert.Equal(4, model.BuildOptions().Count);

        model.ApplyOptions(new HashSet<string>(), new Dictionary<string, string>());

        Assert.Empty(model.BuildOptions());
        Assert.Empty(model.CaptureEnabledOptions());
        Assert.Equal("", model.ExpertArguments);
    }

    [Fact]
    public void ReadViewModelMapsProfileValuesAndEnforcesExclusiveOptions()
    {
        var model = new ReadOperationViewModel();
        model.ApplyOptions(new HashSet<string> { "retries", "densel" }, new Dictionary<string, string> { ["retries"] = "7", ["densel"] = "L" });
        Assert.Equal([new EnabledOption("--retries", "7"), new EnabledOption("--densel", "L")], model.BuildOptions());

        model.EnableTg43();
        model.EnableHardSectors();
        model.EnableFakeIndex();

        Assert.False(model.Densel.Enabled);
        Assert.True(model.Tg43.Enabled);
        Assert.False(model.HardSectors.Enabled);
        Assert.True(model.FakeIndex.Enabled);
        Assert.Contains("gen-tg43", model.CaptureEnabledOptions());
    }

    [Fact]
    public void WriteViewModelDefaultProfileRestoresVerificationAndClearsOptionalArguments()
    {
        var model = new WriteOperationViewModel();
        model.ApplyOptions(new HashSet<string> { "no-verify", "retries", "pre-erase" }, new Dictionary<string, string> { ["retries"] = "4", ["expert"] = "--raw" });
        Assert.True(model.DisableVerification);
        Assert.Equal([new EnabledOption("--retries", "4"), new EnabledOption("--pre-erase")], model.BuildOptions());

        model.ApplyOptions(new HashSet<string>(), new Dictionary<string, string>());

        Assert.False(model.DisableVerification);
        Assert.Empty(model.BuildOptions());
        Assert.Empty(model.CaptureEnabledOptions());
        Assert.Equal("", model.ExpertArguments);
    }

    [Fact]
    public void WriteViewModelRoundTripsProfilesAndEnforcesHardwareExclusions()
    {
        var model = new WriteOperationViewModel();
        model.ApplyOptions(new HashSet<string> { "tracks", "densel", "diskdefs" }, new Dictionary<string, string>
        {
            ["tracks"] = "c=0-39:h=0", ["densel"] = "L", ["diskdefs"] = "formats.cfg", ["expert"] = "--foo bar"
        });
        model.EnableTg43();
        model.EnableHardSectors();
        model.EnableFakeIndex();

        Assert.Equal("L", model.Densel.Value);
        Assert.False(model.Densel.Enabled);
        Assert.True(model.Tg43.Enabled);
        Assert.False(model.HardSectors.Enabled);
        Assert.True(model.FakeIndex.Enabled);
        Assert.Equal("--foo bar", model.CaptureValues()["expert"]);
        Assert.Contains("diskdefs", model.CaptureEnabledOptions());
    }

    [Fact]
    public void ConversionViewModelDefaultProfileClearsFormatsTagsAndOptionalArguments()
    {
        var model = new ConversionOperationViewModel();
        model.ApplyProfile(new HashSet<string> { "tags", "tracks", "format:ibm.720" }, new Dictionary<string, string>
        {
            ["tracks"] = "c=0-39:h=0", ["extensions:ibm.720"] = ".ima,.img", ["expert"] = "--raw"
        });
        Assert.True(model.AddTags);
        Assert.Contains("ibm.720", model.SelectedFormats);

        model.ApplyProfile(new HashSet<string>(), new Dictionary<string, string>());

        Assert.False(model.AddTags);
        Assert.Empty(model.SelectedFormats);
        Assert.Empty(model.ExplicitExtensions);
        Assert.Empty(model.BuildOptions());
        Assert.Equal("", model.ExpertArguments);
    }

    [Fact]
    public void ConversionViewModelRoundTripsMultipleFormatsAndExplicitExtensions()
    {
        var catalog = new BuiltInImageFormatCatalog();
        var model = new ConversionOperationViewModel { AddTags = true };
        model.Tracks.Enabled = true;
        model.Tracks.Value = "c=0-79:h=0-1";
        model.SetFormat("ibm.720", true, [".ima", ".img"]);
        model.SetFormat("atarist.720", true, []);

        var enabled = model.CaptureProfileEnabled();
        var values = model.CaptureProfileValues();
        var restored = new ConversionOperationViewModel();
        restored.ApplyProfile(enabled, values);
        var selections = restored.BuildSelections(catalog.Formats).ToArray();

        Assert.True(restored.AddTags);
        Assert.Equal(2, selections.Length);
        Assert.True(selections.Single(x => x.FormatId == "ibm.720").ExplicitExtensions.SetEquals([".ima", ".img"]));
        Assert.Empty(selections.Single(x => x.FormatId == "atarist.720").ExplicitExtensions);
        Assert.Equal([new EnabledOption("--tracks", "c=0-79:h=0-1")], restored.BuildOptions());
    }
}
