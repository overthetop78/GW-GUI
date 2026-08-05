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
using GWGUI.Infrastructure.Processes;
using GWGUI.Infrastructure.Settings;
using GWGUI.Domain.Settings;
using GWGUI.App;
using GWGUI.App.ViewModels;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace GWGUI.Tests;

public sealed class CoreTests
{
    private static string WindowsPowerShell => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "WindowsPowerShell", "v1.0", "powershell.exe");

    [Fact]
    public void MainWindowStateViewModelPublishesSharedStatusChanges()
    {
        var model = new MainWindowViewModel("No hardware", "Ready");
        var changed = new List<string?>();
        model.PropertyChanged += (_, args) => changed.Add(args.PropertyName);

        model.HardwareText = "Drive 1";
        model.ProfileText = "Profile: Default";
        model.ProfileVisibility = Visibility.Visible;
        model.ProgressVisibility = Visibility.Visible;
        model.ProgressValue = 50;

        Assert.Equal("Drive 1", model.HardwareText);
        Assert.Equal(Visibility.Visible, model.ProfileVisibility);
        Assert.Equal(50, model.ProgressValue);
        Assert.Contains(nameof(model.HardwareText), changed);
        Assert.Contains(nameof(model.ProgressValue), changed);
    }

    [Fact]
    public void MainWindowXamlLoadsWithStatusBindingsAndAlignmentMenu()
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                var app = Application.Current as GWGUI.App.App ?? new GWGUI.App.App();
                app.InitializeComponent();
                var window = new MainWindow();

                Assert.IsType<MainWindowViewModel>(window.DataContext);
                Assert.Equal("align", Assert.IsType<System.Windows.Controls.MenuItem>(window.FindName("AlignMenuItem")).Tag);
                var hardwareText = Assert.IsType<System.Windows.Controls.TextBlock>(window.FindName("HardwareStatusText"));
                var progress = Assert.IsType<System.Windows.Controls.ProgressBar>(window.FindName("OperationProgress"));
                var readFileName = Assert.IsType<System.Windows.Controls.TextBox>(window.FindName("ReadFileName"));
                var readRevs = Assert.IsType<System.Windows.Controls.CheckBox>(window.FindName("ReadRevsEnabled"));
                var writeNoVerify = Assert.IsType<System.Windows.Controls.CheckBox>(window.FindName("WriteNoVerify"));
                var convertTags = Assert.IsType<System.Windows.Controls.CheckBox>(window.FindName("ConvertTags"));
                Assert.NotNull(BindingOperations.GetBindingExpression(hardwareText, System.Windows.Controls.TextBlock.TextProperty));
                Assert.NotNull(BindingOperations.GetBindingExpression(progress, System.Windows.Controls.Primitives.RangeBase.ValueProperty));
                Assert.Equal("Read.FileName", BindingOperations.GetBindingExpression(readFileName, System.Windows.Controls.TextBox.TextProperty)?.ParentBinding.Path.Path);
                Assert.Equal("Read.Revs.Enabled", BindingOperations.GetBindingExpression(readRevs, System.Windows.Controls.Primitives.ToggleButton.IsCheckedProperty)?.ParentBinding.Path.Path);
                Assert.Equal("Write.NoVerify.Enabled", BindingOperations.GetBindingExpression(writeNoVerify, System.Windows.Controls.Primitives.ToggleButton.IsCheckedProperty)?.ParentBinding.Path.Path);
                Assert.Equal("Conversion.AddTags", BindingOperations.GetBindingExpression(convertTags, System.Windows.Controls.Primitives.ToggleButton.IsCheckedProperty)?.ParentBinding.Path.Path);
                var model = Assert.IsType<MainWindowViewModel>(window.DataContext);
                static System.Windows.Controls.CheckBox Probe(MainWindowViewModel dataContext, string path)
                {
                    var probe = new System.Windows.Controls.CheckBox { DataContext = dataContext };
                    BindingOperations.SetBinding(probe, System.Windows.Controls.Primitives.ToggleButton.IsCheckedProperty, new Binding(path) { Mode = BindingMode.TwoWay });
                    return probe;
                }
                var readProbe = Probe(model, "Read.Revs.Enabled"); var writeProbe = Probe(model, "Write.NoVerify.Enabled"); var convertProbe = Probe(model, "Conversion.AddTags");
                readProbe.IsChecked = true; writeProbe.IsChecked = true; convertProbe.IsChecked = true;
                Assert.True(model.Read.Revs.Enabled, "Read checkbox did not update its source");
                Assert.True(model.Write.NoVerify.Enabled, "Write checkbox did not update its source");
                Assert.True(model.Conversion.AddTags, "Conversion checkbox did not update its source");
                model.Read.Revs.Enabled = false; model.Write.NoVerify.Enabled = false; model.Conversion.AddTags = false;
                Assert.False(readProbe.IsChecked); Assert.False(writeProbe.IsChecked); Assert.False(convertProbe.IsChecked);
                window.Close();
            }
            catch (Exception exception) { failure = exception; }
            finally { Dispatcher.CurrentDispatcher.InvokeShutdown(); }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(30)), "The WPF smoke test timed out.");
        if (failure is not null) throw failure;
    }

    [Fact]
    public async Task RunnerCapturesUnicodeStandardErrorAndExitCode()
    {
        var runner = new GreaseweazleRunner();
        var command = new GwCommand(WindowsPowerShell, "-NoProfile", ["-Command", "[Console]::OutputEncoding=[Text.Encoding]::UTF8; Write-Output 'café 漢字'; [Console]::Error.WriteLine('échec Ω'); exit 7"]);
        var result = await runner.RunAsync(command);
        Assert.Equal(7, result.ExitCode);
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Output, line => line.Stream == GwOutputStream.Standard && line.Text.Contains("café 漢字"));
        Assert.Contains(result.Output, line => line.Stream == GwOutputStream.Error && line.Text.Contains("échec Ω"));
    }

    [Fact]
    public async Task BatchExecutorContinuesAfterFailuresAndKeepsAnExactSummary()
    {
        var runner = new ScriptedRunner(
            new GwExecutionResult(0, false, TimeSpan.Zero, []),
            new GwExecutionResult(2, false, TimeSpan.Zero, []),
            new GwExecutionResult(0, false, TimeSpan.Zero, []));
        var items = new[] { "one", "two", "three" }.Select(label => new GwBatchItem(label, new GwCommand("gw.exe", "convert", [label]))).ToArray();
        var started = new List<string>();

        var result = await new GwBatchExecutor(runner).RunAsync(items, itemStarting: item => started.Add(item.Label));

        Assert.False(result.WasCancelled);
        Assert.Equal(2, result.SuccessfulCount);
        Assert.Equal(["two"], result.FailedLabels);
        Assert.Equal(["one", "two", "three"], started);
        Assert.Equal(3, runner.Commands.Count);
    }

    [Fact]
    public async Task BatchExecutorStopsImmediatelyAfterACommandReportsCancellation()
    {
        var runner = new ScriptedRunner(
            new GwExecutionResult(0, false, TimeSpan.Zero, []),
            new GwExecutionResult(-1, true, TimeSpan.Zero, []),
            new GwExecutionResult(0, false, TimeSpan.Zero, []));
        var items = new[] { "one", "two", "three" }.Select(label => new GwBatchItem(label, new GwCommand("gw.exe", "convert", [label]))).ToArray();

        var result = await new GwBatchExecutor(runner).RunAsync(items);

        Assert.True(result.WasCancelled);
        Assert.Equal(1, result.SuccessfulCount);
        Assert.Empty(result.FailedLabels);
        Assert.Equal(2, runner.Commands.Count);
    }

    [Fact]
    public async Task RunnerRejectsASecondConcurrentCommand()
    {
        var runner = new GreaseweazleRunner();
        using var cancellation = new CancellationTokenSource();
        var first = runner.RunAsync(new GwCommand(WindowsPowerShell, "-NoProfile", ["-Command", "Start-Sleep -Seconds 20"]), cancellationToken: cancellation.Token);
        Assert.True(SpinWait.SpinUntil(() => runner.IsRunning, TimeSpan.FromSeconds(2)));
        await Assert.ThrowsAsync<InvalidOperationException>(() => runner.RunAsync(new GwCommand(WindowsPowerShell, "-NoProfile", ["-Command", "exit 0"])));
        cancellation.Cancel();
        Assert.True((await first).WasCancelled);
        Assert.False(runner.IsRunning);
    }

    [Fact]
    public async Task RunnerReassemblesAFragmentedUtf8Line()
    {
        var runner = new GreaseweazleRunner();
        var command = new GwCommand(WindowsPowerShell, "-NoProfile", ["-Command", "[Console]::OutputEncoding=[Text.Encoding]::UTF8; [Console]::Out.Write('frag'); Start-Sleep -Milliseconds 50; [Console]::Out.WriteLine('menté')"]);
        var result = await runner.RunAsync(command);
        Assert.True(result.IsSuccess);
        Assert.Contains(result.Output, line => line.Text == "fragmenté");
    }

    [Fact]
    public void ConversionCompatibilityUsesTheDetectedGeometryForSectorImages()
    {
        var catalog = new BuiltInImageFormatCatalog();
        var detection = new ImageFormatDetector(catalog).Detect("disk.ima", 737280);
        var outputs = ConversionSourceCompatibility.GetOutputs(catalog, ".ima", detection);
        Assert.Collection(outputs, output => Assert.Equal("ibm.720", output.Id));
    }

    [Fact]
    public void ConversionCompatibilityKeepsAllDecodableFormatsForRawFlux()
    {
        var catalog = new BuiltInImageFormatCatalog();
        var detection = new ImageFormatDetector(catalog).Detect("disk.scp", 1234);
        var outputs = ConversionSourceCompatibility.GetOutputs(catalog, ".scp", detection);
        Assert.Contains(outputs, output => output.Id == "amiga.amigados");
        Assert.Contains(outputs, output => output.Id == "atarist.720");
        Assert.Contains(outputs, output => output.Id == "ibm.720");
    }

    [Fact]
    public void RawScpReadNeverAddsAStaleKnownFormat()
    {
        var command = ReadCommandBuilder.Build(new ReadRequest("gw.exe", "disk.scp", ReadResultKind.RawScp, "acorn.adfs.800", []));
        Assert.DoesNotContain("--format", command.Arguments);
        Assert.Equal(["disk.scp"], command.Arguments);
    }

    [Fact]
    public void KnownFormatReadRequiresAndAddsItsFormat()
    {
        var command = ReadCommandBuilder.Build(new ReadRequest("gw.exe", "disk.adf", ReadResultKind.KnownFormat, "amiga.amigados", []));
        Assert.Equal(["--format", "amiga.amigados", "disk.adf"], command.Arguments);
        Assert.Throws<ArgumentException>(() => ReadCommandBuilder.Build(new ReadRequest("gw.exe", "disk.adf", ReadResultKind.KnownFormat, null, [])));
    }

    [Fact]
    public void ADriveArgumentIsOnlyUsedWhenSeveralDrivesAreConfigured()
    {
        var first = new DriveSettings { Selection = "A" };
        var second = new DriveSettings { Selection = "B" };
        Assert.Null(HardwareRoutingPolicy.DriveArgument([first], first));
        Assert.Equal("B", HardwareRoutingPolicy.DriveArgument([first, second], second));
    }

    [Theory]
    [InlineData("A", 0)]
    [InlineData("Z", 25)]
    [InlineData("AA", 26)]
    [InlineData("AB", 27)]
    public void AlphabeticSequenceInputParsesLikeItsDisplayedValue(string text, long expected)
    {
        Assert.True(SequenceFormatter.TryParse(text, SequenceKind.Alphabetic, out var value));
        Assert.Equal(expected, value);
        Assert.Equal(text, SequenceFormatter.Format(value, SequenceKind.Alphabetic, 1));
    }

    [Fact]
    public void RawContainerIdsAreNeverSentAsGwFormatArguments()
    {
        var write = WriteCommandBuilder.Build(new WriteRequest("gw.exe", "disk.scp", "raw.scp", []));
        Assert.Equal(["disk.scp"], write.Arguments);
        var convert = ConversionCommandBuilder.Build("gw.exe", "disk.scp", new ConversionOutput("raw.hfe", ".hfe", "disk.hfe", true));
        Assert.Equal(["disk.scp", "disk.hfe"], convert.Arguments);
        Assert.Equal("raw.gcr", GwFormatArgument.FromCatalogId("raw.gcr"));
    }

    [Fact]
    public void PortableMarkerMovesSettingsNextToTheApplication()
    {
        var directory = Path.Combine(Path.GetTempPath(), "gwgui-portable-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            Assert.Equal(Path.Combine("roaming", "GW GUI"), StoragePaths.ResolveDataDirectory(directory, "roaming"));
            File.WriteAllText(Path.Combine(directory, "portable.flag"), "");
            Assert.Equal(Path.Combine(directory, "Data"), StoragePaths.ResolveDataDirectory(directory, "roaming"));
        }
        finally { Directory.Delete(directory, true); }
    }

    [Fact]
    public async Task VersionOneSettingsMigrateFormatIdentifiersAndCollections()
    {
        var directory = Path.Combine(Path.GetTempPath(), "gwgui-settings-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "settings.json");
        try
        {
            await File.WriteAllTextAsync(path, """{"SchemaVersion":1,"Read":{"FormatId":"amiga.amigadoshd"},"Conversion":{"SelectedFormats":["amiga.amigadoshd"],"ExplicitExtensions":{"amiga.amigadoshd":[".adf"]}},"Profiles":[{"Operation":"Convert","Name":"HD","EnabledOptions":["format:amiga.amigadoshd"],"Values":{"extensions:amiga.amigadoshd":".adf"}}]}""");
            var settings = await new JsonSettingsStore(path).LoadAsync();

            Assert.Equal(SettingsMigrator.CurrentVersion, settings.SchemaVersion);
            Assert.Equal("amiga.amigados_hd", settings.Read.FormatId);
            Assert.Contains("amiga.amigados_hd", settings.Conversion.SelectedFormats);
            Assert.Contains("amiga.amigados_hd", settings.Conversion.ExplicitExtensions.Keys);
            Assert.Contains("format:amiga.amigados_hd", settings.Profiles[0].EnabledOptions);
            Assert.Contains("extensions:amiga.amigados_hd", settings.Profiles[0].Values.Keys);
            Assert.NotNull(settings.Write);
        }
        finally { Directory.Delete(directory, true); }
    }

    [Fact]
    public async Task InvalidSettingsRecoverFromLastBackupAndArePreserved()
    {
        var directory = Path.Combine(Path.GetTempPath(), "gwgui-settings-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "settings.json");
        try
        {
            var store = new JsonSettingsStore(path);
            await store.SaveAsync(new AppSettings { Language = "fr" });
            await store.SaveAsync(new AppSettings { Language = "en" });
            await File.WriteAllTextAsync(path, "{ invalid json");

            var recovered = await store.LoadAsync();

            Assert.Equal("fr", recovered.Language);
            Assert.Contains(Directory.GetFiles(directory), file => file.Contains(".invalid-", StringComparison.Ordinal));
            Assert.Contains("\"Language\": \"fr\"", await File.ReadAllTextAsync(path));
        }
        finally { Directory.Delete(directory, true); }
    }

    [Fact]
    public async Task OperationLogWriterRotatesAndKeepsCommandAndOutput()
    {
        var directory = Path.Combine(Path.GetTempPath(), "gwgui-log-" + Guid.NewGuid().ToString("N"));
        try
        {
            var writer = new RotatingOperationLogWriter(directory, maximumBytes: 220, maximumFiles: 3);
            var command = new GwCommand("gw.exe", "read", ["disk.scp"]);
            for (var index = 0; index < 5; index++)
            {
                var line = new GwOutputLine(DateTimeOffset.UtcNow, GwOutputStream.Standard, $"T{index}.0: " + new string('x', 90));
                await writer.WriteAsync(command, new GwExecutionResult(0, false, TimeSpan.FromSeconds(1), [line]));
            }

            var files = Directory.GetFiles(directory, "operations*.log");
            Assert.Equal(3, files.Length);
            var current = await File.ReadAllTextAsync(Path.Combine(directory, "operations.log"));
            Assert.Contains("gw.exe read disk.scp", current);
            Assert.Contains("T4.0", current);
        }
        finally { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
    }

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
    public void RuntimeCapabilitiesExposePreviouslyUnknownDiskDefinitionsAsRareFormats()
    {
        var capabilities = new GwFormatCapabilities(
            new HashSet<string>(["ibm.720", "dec.rx02", "ensoniq.mirage"], StringComparer.OrdinalIgnoreCase),
            new HashSet<string>([".scp", ".img"], StringComparer.OrdinalIgnoreCase));

        var catalog = new CapabilityAwareImageFormatCatalog(new BuiltInImageFormatCatalog(), capabilities);

        var dec = Assert.Single(catalog.Formats, format => format.Id == "dec.rx02");
        Assert.Equal("DEC", dec.Family);
        Assert.Equal("DEC — RX02", dec.DisplayName);
        Assert.False(dec.IsCommon);
        Assert.Equal(".img", Assert.Single(dec.Extensions).Extension);
        Assert.Equal("DEC-RX02", dec.Tag);
        Assert.Contains(".scp", dec.CompatibleSourceExtensions!);
        Assert.Contains(catalog.Formats, format => format.Id == "ensoniq.mirage");
    }

    [Fact]
    public void CustomDiskDefsReaderResolvesPrefixesAndImports()
    {
        var directory = Path.Combine(Path.GetTempPath(), "gwgui-diskdefs-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            File.WriteAllText(Path.Combine(directory, "child.cfg"), "disk format1\nend\n");
            File.WriteAllText(Path.Combine(directory, "root.cfg"), "disk local\nend\nimport vendor. \"child.cfg\"\n");

            var formats = DiskDefsFormatReader.Read(Path.Combine(directory, "root.cfg"));

            Assert.Equal(new HashSet<string>(["local", "vendor.format1"], StringComparer.OrdinalIgnoreCase), formats);
        }
        finally { Directory.Delete(directory, true); }
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
    public void CatalogDisplayNamesAreProvidedByTheActiveLocalizer()
    {
        var catalog = new BuiltInImageFormatCatalog(key => "localized:" + key);
        var format = Assert.Single(catalog.Formats, item => item.Id == "ibm.720");
        Assert.Equal("localized:Format.ibm.720", format.DisplayName);
        Assert.Equal("localized:Extension.ima", format.Extensions[0].DisplayName);
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
    public void WindowPlacementClampsTheWholeWindowInsideTheVirtualDesktop()
    {
        var settings = new GWGUI.Domain.Settings.WindowPlacementSettings { Width = 1360, Height = 820, Left = 1200, Top = 700 };
        var result = GWGUI.Domain.Settings.WindowPlacementPolicy.Normalize(settings, 1280, 720, 0, 0, 2048, 1152);
        Assert.Equal(688, result.Left);
        Assert.Equal(332, result.Top);
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
        Assert.Equal(Path.Combine("out", "disk [PC-720].ima"), output.OutputPath);
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
    public void IsoMfmDecoderExtractsSectorIdentityAndDataCrc()
    {
        byte[] header = [0xa1, 0xa1, 0xa1, 0xfe, 0, 1, 2, 2]; var crc = TestCrc16(header);
        var data = Enumerable.Range(0, 512).Select(index => (byte)(index * 13)).ToArray(); var dataCrc = TestCrc16(new byte[] { 0xa1,0xa1,0xa1,0xfb }.Concat(data));
        var raw = Convert.ToString(0x4489, 2).PadLeft(16, '0') + Convert.ToString(0x4489, 2).PadLeft(16, '0') + Convert.ToString(0x4489, 2).PadLeft(16, '0') +
                  EncodeMfmBytes(0xfe, 0, 1, 2, 2, (byte)(crc >> 8), (byte)crc) + string.Concat(Enumerable.Repeat("10", 20)) +
                  Convert.ToString(0x44894489, 2).PadLeft(32, '0') + Convert.ToString(0x4489, 2).PadLeft(16, '0') + EncodeMfmBytes(new byte[] { 0xfb }.Concat(data).Concat([(byte)(dataCrc >> 8), (byte)dataCrc]).ToArray()) + "001";
        var intervals = BitsToIntervals(raw, 40);
        var result = new IsoMfmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));
        var sector = Assert.Single(result.Sectors!);
        Assert.Equal(2, sector.Number); Assert.Equal(512, sector.SizeBytes); Assert.True(sector.IntegrityValid);
    }

    [Fact]
    public void IsoFmDecoderExtractsSingleDensitySectorData()
    {
        byte[] header = [0xfe, 3, 0, 7, 1]; var crc = TestCrc16(header);
        var data = Enumerable.Range(0, 256).Select(index => (byte)(index * 17)).ToArray(); var dataCrc = TestCrc16(new byte[] { 0xfb }.Concat(data));
        var raw = Convert.ToString(0xf57e, 2).PadLeft(16, '0') + EncodeFmBytes(3, 0, 7, 1, (byte)(crc >> 8), (byte)crc) + string.Concat(Enumerable.Repeat("10", 20)) + Convert.ToString(0xf56f, 2).PadLeft(16, '0') + EncodeFmBytes(data.Concat([(byte)(dataCrc >> 8), (byte)dataCrc]).ToArray()) + "001";
        var intervals = BitsToIntervals(raw, 40); var result = new IsoFmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));
        var sector = Assert.Single(result.Sectors!); Assert.Equal(7, sector.Number); Assert.Equal(256, sector.SizeBytes); Assert.True(sector.IntegrityValid);
    }

    [Theory]
    [InlineData((byte)0xfb, false)]
    [InlineData((byte)0xf8, true)]
    public void IsoMfmDecoderRecognizesDeletedDataAndCorruptedCrc(byte mark, bool corrupt)
    {
        byte[] header = [0xa1,0xa1,0xa1,0xfe,4,1,9,0]; var headerCrc = TestCrc16(header); var data = Enumerable.Range(0, 128).Select(index => (byte)(index * 19 + 1)).ToArray(); var dataCrc = TestCrc16(new byte[] { 0xa1,0xa1,0xa1,mark }.Concat(data)); if (corrupt) dataCrc++;
        var sync = string.Concat(Enumerable.Repeat(Convert.ToString(0x4489, 2).PadLeft(16, '0'), 3)); var raw = sync + EncodeMfmBytes(0xfe,4,1,9,0,(byte)(headerCrc >> 8),(byte)headerCrc) + string.Concat(Enumerable.Repeat("10", 20)) + sync + EncodeMfmBytes(new[] { mark }.Concat(data).Concat([(byte)(dataCrc >> 8),(byte)dataCrc]).ToArray()) + "001";
        var intervals = BitsToIntervals(raw, 40); var result = new IsoMfmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));
        Assert.Equal(!corrupt, Assert.Single(result.Sectors!).IntegrityValid); Assert.Contains(result.Structures, structure => structure.Kind == (mark == 0xf8 ? FluxStructureKind.DeletedDataAddressMark : FluxStructureKind.DataAddressMark));
    }

    [Theory]
    [InlineData((byte)0xfb, false)]
    [InlineData((byte)0xf8, true)]
    public void IsoFmDecoderRecognizesDeletedDataAndCorruptedCrc(byte mark, bool corrupt)
    {
        byte[] header = [0xfe,2,0,5,0]; var headerCrc = TestCrc16(header); var data = Enumerable.Range(0, 128).Select(index => (byte)(index * 23 + 2)).ToArray(); var dataCrc = TestCrc16(new[] { mark }.Concat(data)); if (corrupt) dataCrc++;
        var rawMark = mark == 0xfb ? 0xf56f : 0xf56a; var raw = Convert.ToString(0xf57e, 2).PadLeft(16, '0') + EncodeFmBytes(2,0,5,0,(byte)(headerCrc >> 8),(byte)headerCrc) + string.Concat(Enumerable.Repeat("10", 20)) + Convert.ToString(rawMark, 2).PadLeft(16, '0') + EncodeFmBytes(data.Concat([(byte)(dataCrc >> 8),(byte)dataCrc]).ToArray()) + "001";
        var intervals = BitsToIntervals(raw, 40); var result = new IsoFmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));
        Assert.Equal(!corrupt, Assert.Single(result.Sectors!).IntegrityValid); Assert.Contains(result.Structures, structure => structure.Kind == (mark == 0xf8 ? FluxStructureKind.DeletedDataAddressMark : FluxStructureKind.DataAddressMark));
    }

    [Fact]
    public void IsoDecodersReportUnavailableIntegrityWithoutDataField()
    {
        byte[] mfmHeader = [0xa1,0xa1,0xa1,0xfe,0,0,1,0]; var mfmCrc = TestCrc16(mfmHeader); var mfmRaw = string.Concat(Enumerable.Repeat(Convert.ToString(0x4489, 2).PadLeft(16, '0'), 3)) + EncodeMfmBytes(0xfe,0,0,1,0,(byte)(mfmCrc >> 8),(byte)mfmCrc) + "001";
        byte[] fmHeader = [0xfe,0,0,1,0]; var fmCrc = TestCrc16(fmHeader); var fmRaw = Convert.ToString(0xf57e, 2).PadLeft(16, '0') + EncodeFmBytes(0,0,1,0,(byte)(fmCrc >> 8),(byte)fmCrc) + "001";
        var mfmIntervals = BitsToIntervals(mfmRaw, 40); var fmIntervals = BitsToIntervals(fmRaw, 40);
        Assert.Null(Assert.Single(new IsoMfmDecoder().Decode(new ScpRevolution(8_000_000, (uint)mfmIntervals.Count, mfmIntervals)).Sectors!).IntegrityValid);
        Assert.Null(Assert.Single(new IsoFmDecoder().Decode(new ScpRevolution(8_000_000, (uint)fmIntervals.Count, fmIntervals)).Sectors!).IntegrityValid);
    }

    [Fact]
    public void AppleGcrDecoderFindsAddressAndDataProloguesDespiteShortNoise()
    {
        var bits = Convert.ToString(0xD5AA96, 2).PadLeft(24, '0') + "0001000" + Convert.ToString(0xD5AAAD, 2).PadLeft(24, '0') + "1";
        var intervals = BitsToIntervals(bits, 40); intervals.Insert(0, 2);
        var result = new AppleGcrDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.AppleAddress);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.AppleData);
        Assert.Equal(40, result.EstimatedBitCellTicks);
    }

    [Fact]
    public void AdaptiveFluxClockFollowsGradualSpeedDrift()
    {
        var prologue = Convert.ToString(0xD5AA96, 2).PadLeft(24, '0');
        var bits = string.Concat(Enumerable.Repeat(prologue + "000", 10)) + "1";
        var intervals = new List<uint>(); var cells = 0; var transition = 0;
        foreach (var bit in bits)
        {
            cells++;
            if (bit != '1') continue;
            var cellTicks = 36d + Math.Min(8, transition * .25);
            intervals.Add((uint)Math.Round(cells * cellTicks)); cells = 0; transition++;
        }
        var result = new AppleGcrDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));
        Assert.True(result.Structures.Count(structure => structure.Kind == FluxStructureKind.AppleAddress) >= 8);
        Assert.InRange(result.EstimatedBitCellTicks, 36, 44);
    }

    [Fact]
    public void RawFluxDecoderReportsShortNoiseAndLongDropout()
    {
        var intervals = Enumerable.Repeat(80u, 30).ToList(); intervals[8] = 5; intervals[20] = 900;
        var result = new RawFluxDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));
        Assert.Equal(2, result.Structures.Count(structure => structure.Kind == FluxStructureKind.TimingAnomaly));
    }

    [Fact]
    public void CommodoreGcrDecoderFindsSyncAndHeaderBlock()
    {
        const string headerByte08 = "01010" + "01001";
        var intervals = BitsToIntervals("111111111111" + headerByte08 + "1", 40);
        var result = new CommodoreGcrDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.CommodoreSync);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.CommodoreHeader);
        Assert.Contains((byte)0x08, result.DecodedBytes);
    }

    [Fact]
    public void DecoderRegistryExposesGcrFamilies()
    {
        var ids = new FluxDecoderRegistry().Decoders.Select(decoder => decoder.Id).ToHashSet();
        Assert.Contains("apple2.gcr", ids); Assert.Contains("commodore.gcr", ids); Assert.Contains("northstar.mfm", ids); Assert.Contains("heathkit.fm", ids);
    }

    [Fact]
    public void NorthstarDecoderRecognizesHardSectorBlockMark()
    {
        var raw = string.Concat(Enumerable.Repeat("10", 60)) + EncodeMfmBytesFromZero(0, 0, 0, 0, 0, 0, 0, 0xfb) + "001";
        var intervals = BitsToIntervals(raw, 40);
        var result = new NorthstarMfmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatHeader);
    }

    [Fact]
    public void NorthstarDecoderExtractsSectorIdentityAndRotatingChecksum()
    {
        var data = Enumerable.Range(0, 512).Select(index => (byte)(index * 17)).ToArray();
        byte checksum = 0;
        foreach (var value in data) { checksum ^= value; checksum = (byte)((checksum >> 7) | (checksum << 1)); }
        var block = Enumerable.Repeat((byte)0, 7).Concat([(byte)0xfb, (byte)0x37]).Concat(data).Append(checksum).ToArray();
        var raw = string.Concat(Enumerable.Repeat("10", 60)) + EncodeMfmBytesFromZero(block) + "001";
        var intervals = BitsToIntervals(raw, 40);

        var result = new NorthstarMfmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        var sector = Assert.Single(result.Sectors!);
        Assert.Equal(3, sector.Cylinder);
        Assert.Equal(7, sector.Number);
        Assert.Equal(512, sector.SizeBytes);
        Assert.True(sector.IntegrityValid);
        Assert.Equal(SectorIntegrityKind.Checksum, sector.IntegrityKind);
    }

    [Fact]
    public void HeathkitDecoderRecognizesBitReversedFdHeaderMark()
    {
        var raw = EncodeFmBytes(0, 0, 0, 0xbf) + "001"; var intervals = BitsToIntervals(raw, 40);
        var result = new HeathkitFmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatHeader);
    }

    [Fact]
    public void HeathkitDecoderExtractsBitReversedHeaderAndChecksum()
    {
        const byte volume = 2, cylinder = 12, sectorNumber = 5;
        byte checksum = 0;
        foreach (var value in new[] { volume, cylinder, sectorNumber }) { checksum ^= value; checksum = (byte)((checksum >> 7) | (checksum << 1)); }
        static byte Reverse(byte value) { byte result = 0; for (var bit = 0; bit < 8; bit++) result = (byte)((result << 1) | ((value >> bit) & 1)); return result; }
        var raw = EncodeFmBytes(0, 0, 0, 0xbf, Reverse(volume), Reverse(cylinder), Reverse(sectorNumber), Reverse(checksum)) + "001";
        var intervals = BitsToIntervals(raw, 40);

        var result = new HeathkitFmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        var sector = Assert.Single(result.Sectors!);
        Assert.Equal(cylinder, sector.Cylinder);
        Assert.Equal(sectorNumber, sector.Number);
        Assert.Equal(256, sector.SizeBytes);
        Assert.True(sector.IntegrityValid);
        Assert.Equal(SectorIntegrityKind.Checksum, sector.IntegrityKind);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MembrainDecoderExtractsPackedIdentityAndNativeCrc(bool corruptCrc)
    {
        byte[] prefix = [0xa1, 0xfe, 0x04, 0xb9];
        var crc = TestCrc16(prefix, 0x8005, 0x0000);
        if (corruptCrc) crc ^= 1;
        var data = Enumerable.Range(0, 512).Select(index => (byte)(index * 7)).ToArray();
        var dataCrc = TestCrc16(new byte[] { 0xa1, 0xf8 }.Concat(data), 0x8005, 0x0000);
        var raw = Convert.ToString(0x44895554, 2).PadLeft(32, '0') + EncodeMfmBytesFromZero(0x04, 0xb9, (byte)(crc >> 8), (byte)crc) + "00000000" +
                  Convert.ToString(0x4489554a, 2).PadLeft(32, '0') + EncodeMfmBytesFromZero(data.Concat([(byte)(dataCrc >> 8), (byte)dataCrc]).ToArray()) + "001";
        var intervals = BitsToIntervals(raw, 40);

        var result = new MembrainMfmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        var sector = Assert.Single(result.Sectors!);
        Assert.Equal(37, sector.Cylinder);
        Assert.Equal(1, sector.Head);
        Assert.Equal(9, sector.Number);
        Assert.Equal(512, sector.SizeBytes);
        Assert.Equal(!corruptCrc, sector.IntegrityValid);
        Assert.Equal(SectorIntegrityKind.Crc, sector.IntegrityKind);
        Assert.Equal(data, result.DecodedBytes.TakeLast(512));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MembrainDecoderValidatesDataCrc(bool corruptData)
    {
        byte[] header = [0xa1, 0xfe, 0x04, 0xb9]; var headerCrc = TestCrc16(header, 0x8005, 0x0000);
        var data = Enumerable.Range(0, 512).Select(index => (byte)(255 - index)).ToArray(); var dataCrc = TestCrc16(new byte[] { 0xa1, 0xf8 }.Concat(data), 0x8005, 0x0000);
        if (corruptData) dataCrc ^= 1;
        var raw = Convert.ToString(0x44895554, 2).PadLeft(32, '0') + EncodeMfmBytesFromZero(0x04, 0xb9, (byte)(headerCrc >> 8), (byte)headerCrc) + "00000000" +
                  Convert.ToString(0x4489554a, 2).PadLeft(32, '0') + EncodeMfmBytesFromZero(data.Concat([(byte)(dataCrc >> 8), (byte)dataCrc]).ToArray()) + "001";
        var intervals = BitsToIntervals(raw, 40);

        var result = new MembrainMfmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        Assert.Equal(!corruptData, Assert.Single(result.Sectors!).IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatData && structure.Description.Contains(corruptData ? "invalid" : "valid"));
    }

    [Fact]
    public void MembrainDecoderReportsUnavailableIntegrityWithoutDataBlock()
    {
        byte[] header = [0xa1, 0xfe, 0x04, 0xb9]; var crc = TestCrc16(header, 0x8005, 0x0000);
        var raw = Convert.ToString(0x44895554, 2).PadLeft(32, '0') + EncodeMfmBytesFromZero(0x04, 0xb9, (byte)(crc >> 8), (byte)crc) + "001"; var intervals = BitsToIntervals(raw, 40);

        var result = new MembrainMfmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        Assert.Null(Assert.Single(result.Sectors!).IntegrityValid);
    }

    [Theory]
    [InlineData(512, false)]
    [InlineData(1024, true)]
    public void Aed6200pDecoderExtractsVariableSectorSizeAndHeaderCrc(int sectorSize, bool corruptCrc)
    {
        byte[] prefix = [0xc6, 12, (byte)sectorSize, 3, (byte)(sectorSize >> 8)];
        var crc = TestCrc16(prefix);
        if (corruptCrc) crc ^= 1;
        var raw = Convert.ToString(0x5094, 2).PadLeft(16, '0') + EncodeMfmBytesFromZero(12, (byte)sectorSize, 3, (byte)(sectorSize >> 8), (byte)(crc >> 8), (byte)crc) + "001";
        var intervals = BitsToIntervals(raw, 40);

        var result = new Aed6200pMfmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        var sector = Assert.Single(result.Sectors!);
        Assert.Equal(12, sector.Cylinder);
        Assert.Equal(0, sector.Head);
        Assert.Equal(3, sector.Number);
        Assert.Equal(sectorSize, sector.SizeBytes);
        Assert.Equal(!corruptCrc, sector.IntegrityValid);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CenturionDecoderExtractsSectorIdentityAndXmodemHeaderCrc(bool corruptCrc)
    {
        byte[] identity = [17, 6];
        var crc = TestCrc16(identity, 0x1021, 0x0000);
        if (corruptCrc) crc ^= 1;
        var raw = Convert.ToString(0x91224489, 2).PadLeft(32, '0') + EncodeMfmBytesFromZero(17, 6, (byte)(crc >> 8), (byte)crc) + "001";
        var intervals = BitsToIntervals(raw, 40);

        var result = new CenturionMfmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        var sector = Assert.Single(result.Sectors!);
        Assert.Equal(17, sector.Cylinder);
        Assert.Equal(6, sector.Number);
        Assert.Equal(0, sector.SizeBytes);
        Assert.Equal(!corruptCrc, sector.IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Description.Contains(corruptCrc ? "invalid" : "valid", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void QdMo5DecoderExtractsWideSectorNumberAndDataChecksum(bool corruptChecksum)
    {
        var data = Enumerable.Range(0, 128).Select(index => (byte)(index * 11)).ToArray();
        var checksum = (byte)(0x5a + data.Sum(value => value));
        if (corruptChecksum) checksum++;
        static string RawMark(string hexadecimal) => string.Concat(Convert.FromHexString(hexadecimal).Select(value => Convert.ToString(value, 2).PadLeft(8, '0')));
        var headerMark = RawMark("A914A914A914A914A9144491");
        var dataMark = RawMark("A914A914A914A914A9149144");
        var headerTail = new byte[] { 0x12, 0x34 }.Concat(new byte[13]).ToArray();
        var raw = headerMark + EncodeMfmBytesFromZero(headerTail) + string.Concat(Enumerable.Repeat("10", 20)) + dataMark + EncodeMfmBytesFromZero(data.Append(checksum).ToArray()) + "001";
        var intervals = BitsToIntervals(raw, 40);

        var result = new QdMo5MfmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        var sector = Assert.Single(result.Sectors!);
        Assert.Equal(0x1234, sector.Number);
        Assert.Equal(128, sector.SizeBytes);
        Assert.Equal(!corruptChecksum, sector.IntegrityValid);
        Assert.Equal(SectorIntegrityKind.Checksum, sector.IntegrityKind);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatData && structure.Description.Contains(corruptChecksum ? "invalid" : "valid", StringComparison.Ordinal));
    }

    [Fact]
    public void QdMo5DecoderReportsUnavailableIntegrityWhenDataBlockIsMissing()
    {
        static string RawMark(string hexadecimal) => string.Concat(Convert.FromHexString(hexadecimal).Select(value => Convert.ToString(value, 2).PadLeft(8, '0')));
        var headerTail = new byte[] { 0x01, 0x02 }.Concat(new byte[13]).ToArray();
        var raw = RawMark("A914A914A914A914A9144491") + EncodeMfmBytesFromZero(headerTail) + "001";
        var intervals = BitsToIntervals(raw, 40);

        var result = new QdMo5MfmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        var sector = Assert.Single(result.Sectors!);
        Assert.Equal(0x0102, sector.Number);
        Assert.Null(sector.IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Description.Contains("unavailable", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EmuFmDecoderExtractsTrackIdentityAndValidatesLargeDataCrc(bool corruptDataCrc)
    {
        static byte Reverse(byte value) { byte result = 0; for (var bit = 0; bit < 8; bit++) result = (byte)((result << 1) | ((value >> bit) & 1)); return result; }
        static string EncodeEmuFm(IEnumerable<byte> values) => string.Concat(values.SelectMany(value => Enumerable.Range(0, 8).Select(bit => "01" + ((((value >> (7 - bit)) & 1) != 0) ? "01" : "00"))));
        byte track = 25, rawTrack = Reverse(track);
        var headerCrc = TestCrc16([rawTrack], 0x8005, 0x0000);
        var data = Enumerable.Range(0, 0xe00).Select(index => (byte)(index * 13)).ToArray();
        var dataCrc = TestCrc16(data, 0x8005, 0x0000);
        if (corruptDataCrc) dataCrc ^= 1;
        var marker = EncodeEmuFm([Reverse(0xfa), Reverse(0x96)]);
        var raw = marker + EncodeEmuFm([rawTrack, (byte)(headerCrc >> 8), (byte)headerCrc]) + new string('1', 64)
            + marker + EncodeEmuFm(data.Concat([(byte)(dataCrc >> 8), (byte)dataCrc])) + "1";
        var intervals = BitsToIntervals(raw, 40);

        var result = new EmuFmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        var sector = Assert.Single(result.Sectors!);
        Assert.Equal(12, sector.Cylinder);
        Assert.Equal(1, sector.Head);
        Assert.Equal(1, sector.Number);
        Assert.Equal(0xe00, sector.SizeBytes);
        Assert.Equal(!corruptDataCrc, sector.IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatData && structure.Description.Contains(corruptDataCrc ? "invalid" : "valid", StringComparison.Ordinal));
    }

    [Fact]
    public void EmuFmDecoderReportsUnavailableDataIntegrityWhenOnlyHeaderExists()
    {
        static byte Reverse(byte value) { byte result = 0; for (var bit = 0; bit < 8; bit++) result = (byte)((result << 1) | ((value >> bit) & 1)); return result; }
        static string EncodeEmuFm(IEnumerable<byte> values) => string.Concat(values.SelectMany(value => Enumerable.Range(0, 8).Select(bit => "01" + ((((value >> (7 - bit)) & 1) != 0) ? "01" : "00"))));
        var rawTrack = Reverse(8); var headerCrc = TestCrc16([rawTrack], 0x8005, 0x0000);
        var marker = EncodeEmuFm([Reverse(0xfa), Reverse(0x96)]);
        var raw = marker + EncodeEmuFm([rawTrack, (byte)(headerCrc >> 8), (byte)headerCrc]) + "1";
        var intervals = BitsToIntervals(raw, 40);

        var result = new EmuFmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        var sector = Assert.Single(result.Sectors!);
        Assert.Equal(4, sector.Cylinder);
        Assert.Equal(0, sector.Head);
        Assert.Null(sector.IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Description.Contains("unavailable", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(0xf8, false)]
    [InlineData(0xf9, false)]
    [InlineData(0xfa, false)]
    [InlineData(0xfb, true)]
    public void TycomFmDecoderExtractsIdentityDataMarkAndCrc(byte dataMark, bool corruptDataCrc)
    {
        static string EncodeTycomFm(IEnumerable<byte> values) => string.Concat(values.SelectMany(value => Enumerable.Range(0, 8).Select(bit => "01" + ((((value >> (7 - bit)) & 1) != 0) ? "01" : "00"))));
        static string RawMark(string hexadecimal) => string.Concat(Convert.FromHexString(hexadecimal).Select(value => Convert.ToString(value, 2).PadLeft(8, '0')));
        const byte cylinder = 31, sectorNumber = 7;
        var headerCrc = TestCrc16([0xfe, cylinder, sectorNumber], 0x1021, 0xffff);
        var data = Enumerable.Range(0, 128).Select(index => (byte)(index * 19)).ToArray();
        var dataCrc = TestCrc16(new byte[] { dataMark }.Concat(data), 0x1021, 0xffff);
        if (corruptDataCrc) dataCrc ^= 1;
        var dataPattern = dataMark switch { 0xf8 => "55111444", 0xf9 => "55111445", 0xfa => "55111454", _ => "55111455" };
        var raw = RawMark("55111554") + EncodeTycomFm([cylinder, sectorNumber, (byte)(headerCrc >> 8), (byte)headerCrc]) + new string('1', 64)
            + RawMark(dataPattern) + EncodeTycomFm(data.Concat([(byte)(dataCrc >> 8), (byte)dataCrc])) + "1";
        var intervals = BitsToIntervals(raw, 40);

        var result = new TycomFmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        var sector = Assert.Single(result.Sectors!);
        Assert.Equal(cylinder, sector.Cylinder);
        Assert.Equal(sectorNumber, sector.Number);
        Assert.Equal(128, sector.SizeBytes);
        Assert.Equal(!corruptDataCrc, sector.IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatData && structure.Description.Contains(dataMark.ToString("X2"), StringComparison.Ordinal));
    }

    [Fact]
    public void TycomFmDecoderReportsUnavailableDataIntegrityWhenOnlyHeaderExists()
    {
        static string EncodeTycomFm(IEnumerable<byte> values) => string.Concat(values.SelectMany(value => Enumerable.Range(0, 8).Select(bit => "01" + ((((value >> (7 - bit)) & 1) != 0) ? "01" : "00"))));
        static string RawMark(string hexadecimal) => string.Concat(Convert.FromHexString(hexadecimal).Select(value => Convert.ToString(value, 2).PadLeft(8, '0')));
        var headerCrc = TestCrc16([0xfe, 4, 2], 0x1021, 0xffff);
        var raw = RawMark("55111554") + EncodeTycomFm([4, 2, (byte)(headerCrc >> 8), (byte)headerCrc]) + "1";
        var intervals = BitsToIntervals(raw, 40);

        var result = new TycomFmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        var sector = Assert.Single(result.Sectors!);
        Assert.Equal(4, sector.Cylinder);
        Assert.Equal(2, sector.Number);
        Assert.Null(sector.IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Description.Contains("unavailable", StringComparison.Ordinal));
    }

    [Fact]
    public void TycomFmDecoderRejectsCorruptedHeaderCrc()
    {
        static string EncodeTycomFm(IEnumerable<byte> values) => string.Concat(values.SelectMany(value => Enumerable.Range(0, 8).Select(bit => "01" + ((((value >> (7 - bit)) & 1) != 0) ? "01" : "00"))));
        static string RawMark(string hexadecimal) => string.Concat(Convert.FromHexString(hexadecimal).Select(value => Convert.ToString(value, 2).PadLeft(8, '0')));
        var headerCrc = (ushort)(TestCrc16([0xfe, 9, 3], 0x1021, 0xffff) ^ 1);
        var raw = RawMark("55111554") + EncodeTycomFm([9, 3, (byte)(headerCrc >> 8), (byte)headerCrc]) + "1";
        var intervals = BitsToIntervals(raw, 40);

        var result = new TycomFmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        Assert.Empty(result.Sectors!);
        Assert.Contains(result.Structures, structure => structure.Description.Contains("header CRC invalid", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(0xf8, false)]
    [InlineData(0xf9, false)]
    [InlineData(0xfa, false)]
    [InlineData(0xfb, false)]
    [InlineData(0xfc, false)]
    [InlineData(0xfd, true)]
    public void DecRx02DecoderExtractsAllDataMarksAndFmOrM2FmCrc(byte dataMark, bool corruptDataCrc)
    {
        static string EncodeRxFm(IEnumerable<byte> values) => string.Concat(values.SelectMany(value => Enumerable.Range(0, 8).Select(bit => "01" + ((((value >> (7 - bit)) & 1) != 0) ? "01" : "00"))));
        static string RawMark(string hexadecimal) => string.Concat(Convert.FromHexString(hexadecimal).Select(value => Convert.ToString(value, 2).PadLeft(8, '0')));
        static string EncodeM2Fm(byte[] values)
        {
            var bits = EncodeMfmBytesFromZero(values).ToCharArray(); const string normal = "00101010100", encoded = "01000100010"; var replacements = 0;
            for (var offset = 1; offset + normal.Length <= bits.Length; offset += 2)
            {
                var matches = true; for (var index = 0; index < normal.Length; index++) if (bits[offset + index] != normal[index]) { matches = false; break; }
                if (!matches) continue; for (var index = 0; index < encoded.Length; index++) bits[offset + index] = encoded[index]; replacements++; offset += normal.Length - 3;
            }
            Assert.True(replacements > 0, "The M²FM vector must exercise the DEC 11-bit substitution rule."); return new string(bits);
        }
        const byte cylinder = 22, head = 1, sectorNumber = 9, sizeCode = 0;
        var headerCrc = TestCrc16([0xfe, cylinder, head, sectorNumber, sizeCode], 0x1021, 0xffff);
        var m2fm = dataMark is 0xf9 or 0xfd; var size = m2fm ? 256 : 128;
        var data = Enumerable.Range(0, size).Select(index => (byte)(index * 23)).ToArray();
        var dataCrc = TestCrc16(new byte[] { dataMark }.Concat(data), 0x1021, 0xffff); if (corruptDataCrc) dataCrc ^= 1;
        var markPattern = dataMark switch { 0xf8 => "55111444", 0xf9 => "55111445", 0xfa => "55111454", 0xfb => "55111455", 0xfc => "55111544", _ => "55111545" };
        var payload = data.Concat([(byte)(dataCrc >> 8), (byte)dataCrc]).ToArray();
        var encodedPayload = m2fm ? "0" + EncodeM2Fm(payload) : EncodeRxFm(payload);
        var raw = RawMark("55111554") + EncodeRxFm([cylinder, head, sectorNumber, sizeCode, (byte)(headerCrc >> 8), (byte)headerCrc]) + new string('1', 64)
            + RawMark(markPattern) + encodedPayload + "1";
        var intervals = BitsToIntervals(raw, 40);

        var result = new DecRx02Decoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        var sector = Assert.Single(result.Sectors!);
        Assert.Equal(cylinder, sector.Cylinder); Assert.Equal(head, sector.Head); Assert.Equal(sectorNumber, sector.Number);
        Assert.Equal(size, sector.SizeBytes); Assert.Equal(!corruptDataCrc, sector.IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatData && structure.Description.Contains(dataMark.ToString("X2"), StringComparison.Ordinal) && structure.Description.Contains(m2fm ? "M²FM" : "FM", StringComparison.Ordinal));
    }

    [Fact]
    public void DecRx02DecoderReportsUnavailableDataAndRejectsBadHeaderCrc()
    {
        static string EncodeRxFm(IEnumerable<byte> values) => string.Concat(values.SelectMany(value => Enumerable.Range(0, 8).Select(bit => "01" + ((((value >> (7 - bit)) & 1) != 0) ? "01" : "00"))));
        static string RawMark(string hexadecimal) => string.Concat(Convert.FromHexString(hexadecimal).Select(value => Convert.ToString(value, 2).PadLeft(8, '0')));
        var validCrc = TestCrc16([0xfe, 5, 0, 2, 0], 0x1021, 0xffff);
        var validBits = RawMark("55111554") + EncodeRxFm([5, 0, 2, 0, (byte)(validCrc >> 8), (byte)validCrc]) + "1";
        var invalidCrc = (ushort)(validCrc ^ 1);
        var invalidBits = RawMark("55111554") + EncodeRxFm([5, 0, 2, 0, (byte)(invalidCrc >> 8), (byte)invalidCrc]) + "1";

        var validIntervals = BitsToIntervals(validBits, 40); var invalidIntervals = BitsToIntervals(invalidBits, 40);
        var missing = new DecRx02Decoder().Decode(new ScpRevolution(8_000_000, (uint)validIntervals.Count, validIntervals));
        var corrupt = new DecRx02Decoder().Decode(new ScpRevolution(8_000_000, (uint)invalidIntervals.Count, invalidIntervals));

        Assert.Null(Assert.Single(missing.Sectors!).IntegrityValid);
        Assert.Contains(missing.Structures, structure => structure.Description.Contains("unavailable", StringComparison.Ordinal));
        Assert.Empty(corrupt.Sectors!);
        Assert.Contains(corrupt.Structures, structure => structure.Description.Contains("header CRC invalid", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ArburgDecoderValidatesFullFmDataTrackChecksum(bool corruptChecksum)
    {
        static byte Reverse(byte value) { byte result = 0; for (var bit = 0; bit < 8; bit++) result = (byte)((result << 1) | ((value >> bit) & 1)); return result; }
        static string EncodeArburgFm(IEnumerable<byte> values) => string.Concat(values.SelectMany(value => Enumerable.Range(0, 8).Select(bit => { var reversed = Reverse(value); return "01" + ((((reversed >> (7 - bit)) & 1) != 0) ? "01" : "00"); })));
        static string RawMark(string hexadecimal) => string.Concat(Convert.FromHexString(hexadecimal).Select(value => Convert.ToString(value, 2).PadLeft(8, '0')));
        var data = Enumerable.Range(0, 0x9fe).Select(index => (byte)(index * 29)).ToArray(); ushort checksum = 0; foreach (var value in data) checksum += value;
        if (corruptChecksum) checksum++;
        var block = data.Concat([(byte)checksum, (byte)(checksum >> 8)]).ToArray();
        var raw = RawMark("4444444455555555") + EncodeArburgFm(block) + "1"; var intervals = BitsToIntervals(raw, 40);

        var result = new ArburgDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        var sector = Assert.Single(result.Sectors!); Assert.Equal(0xa00, sector.SizeBytes); Assert.Equal(!corruptChecksum, sector.IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatData && structure.Description.Contains(corruptChecksum ? "invalid" : "valid", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ArburgDecoderValidatesFullVariableLengthSystemTrackChecksum(bool corruptChecksum)
    {
        static string EncodeSystem(IEnumerable<byte> values) => string.Concat(values.SelectMany(value => Enumerable.Range(0, 8).Select(bit => ((value >> bit) & 1) != 0 ? "001" : "01")));
        static string RawMark(string hexadecimal) => string.Concat(Convert.FromHexString(hexadecimal).Select(value => Convert.ToString(value, 2).PadLeft(8, '0')));
        var data = Enumerable.Range(0, 0xefe).Select(index => (byte)(index * 31)).ToArray(); ushort checksum = 0; foreach (var value in data) checksum += value;
        if (corruptChecksum) checksum++;
        var block = data.Concat([(byte)checksum, (byte)(checksum >> 8)]).ToArray();
        var raw = RawMark("5555555555249249") + EncodeSystem(block) + "1"; var intervals = BitsToIntervals(raw, 40);

        var result = new ArburgDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        var sector = Assert.Single(result.Sectors!); Assert.Equal(0xf00, sector.SizeBytes); Assert.Equal(!corruptChecksum, sector.IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatHeader && structure.Description.Contains(corruptChecksum ? "invalid" : "valid", StringComparison.Ordinal));
    }

    [Fact]
    public void ArburgDecoderReportsUnavailableIntegrityForTruncatedTrackBlocks()
    {
        static string RawMark(string hexadecimal) => string.Concat(Convert.FromHexString(hexadecimal).Select(value => Convert.ToString(value, 2).PadLeft(8, '0')));
        static FluxDecodeResult Decode(string marker)
        {
            var intervals = BitsToIntervals(RawMark(marker) + "1", 40); return new ArburgDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));
        }
        var data = Decode("4444444455555555"); var system = Decode("5555555555249249");
        Assert.Null(Assert.Single(data.Sectors!).IntegrityValid); Assert.Null(Assert.Single(system.Sectors!).IntegrityValid);
        Assert.All(data.Structures.Concat(system.Structures), structure => Assert.Contains("unavailable", structure.Description, StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void Victor9kDecoderExtractsIdentityAndValidatesHeaderAndDataChecksums(bool corruptHeader, bool corruptData)
    {
        static string EncodeGcr(IEnumerable<byte> values)
        {
            int[] table = [0x0a,0x0b,0x12,0x13,0x0e,0x0f,0x16,0x17,0x09,0x19,0x1a,0x1b,0x0d,0x1d,0x1e,0x15];
            return string.Concat(values.SelectMany(value => new[] { value >> 4, value & 15 }).Select(nibble => Convert.ToString(table[nibble], 2).PadLeft(5, '0')));
        }
        static string Block(string markerHex, IReadOnlyList<byte> values)
        {
            var marker = string.Concat(Convert.FromHexString(markerHex).Select(value => Convert.ToString(value, 2).PadLeft(8, '0'))); var bits = marker.ToList(); var encoded = EncodeGcr(values);
            while (bits.Count < 49 + encoded.Length * 2) bits.Add('0');
            for (var index = 0; index < encoded.Length; index++)
            {
                var position = 49 + index * 2;
                if (position < marker.Length) Assert.Equal(marker[position], encoded[index]);
                bits[position] = encoded[index];
            }
            return new(bits.ToArray());
        }
        const byte cylinder = 17; const byte sector = 6;
        var headerChecksum = (byte)(cylinder + sector + (corruptHeader ? 1 : 0));
        byte[] header = [0x06, cylinder, sector, headerChecksum, 0xa1, 0x1a];
        var data = Enumerable.Range(0, 512).Select(index => (byte)(index * 29 + 7)).ToArray(); ushort checksum = 0; foreach (var value in data) checksum += value;
        if (corruptData) checksum++;
        var dataBlock = new byte[] { 0x00 }.Concat(data).Concat([(byte)checksum, (byte)(checksum >> 8)]).ToArray();
        var raw = Block("5555555555551111", header) + new string('0', 20) + Block("5555555555551104", dataBlock) + "1";
        var intervals = BitsToIntervals(raw, 40);

        var result = new Victor9kGcrDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        var decoded = Assert.Single(result.Sectors!); Assert.Equal(cylinder, decoded.Cylinder); Assert.Equal(sector, decoded.Number); Assert.Equal(512, decoded.SizeBytes);
        Assert.Equal(!corruptHeader && !corruptData, decoded.IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatData && structure.Description.Contains(corruptData ? "invalid" : "valid", StringComparison.Ordinal));
    }

    [Fact]
    public void Victor9kDecoderReportsUnavailableIntegrityForTruncatedSector()
    {
        var marker = string.Concat(Convert.FromHexString("5555555555551111").Select(value => Convert.ToString(value, 2).PadLeft(8, '0'))); var intervals = BitsToIntervals(marker + "1", 40);
        var result = new Victor9kGcrDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));
        Assert.Null(Assert.Single(result.Sectors!).IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatHeader && structure.Description.Contains("unavailable", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void AppleGcrDecoderExtractsAddressAndDecodesSixAndTwoData(bool corruptAddress, bool corruptData)
    {
        byte[] table = [0x96,0x97,0x9a,0x9b,0x9d,0x9e,0x9f,0xa6,0xa7,0xab,0xac,0xad,0xae,0xaf,0xb2,0xb3,0xb4,0xb5,0xb6,0xb7,0xb9,0xba,0xbb,0xbc,0xbd,0xbe,0xbf,0xcb,0xcd,0xce,0xcf,0xd3,0xd6,0xd7,0xd9,0xda,0xdb,0xdc,0xdd,0xde,0xdf,0xe5,0xe6,0xe7,0xe9,0xea,0xeb,0xec,0xed,0xee,0xef,0xf2,0xf3,0xf4,0xf5,0xf6,0xf7,0xf9,0xfa,0xfb,0xfc,0xfd,0xfe,0xff];
        static IEnumerable<byte> FourAndFour(byte value) => [(byte)((value >> 1) | 0xaa), (byte)(value | 0xaa)];
        static string Bits(IEnumerable<byte> values) => string.Concat(values.Select(value => Convert.ToString(value, 2).PadLeft(8, '0')));
        static byte[] EncodeData(byte[] source, IReadOnlyList<byte> translation, bool corrupt)
        {
            var buffer = new byte[300]; source.CopyTo(buffer, 0); var encoded = new List<byte>(343); byte checksum = 0;
            for (var index = 0; index < 86; index++)
            {
                var value = (byte)(((buffer[index] & 1) << 1) | ((buffer[index] & 2) >> 1) | ((buffer[index + 86] & 1) << 3) | ((buffer[index + 86] & 2) << 1) | ((buffer[index + 172] & 1) << 5) | ((buffer[index + 172] & 2) << 3));
                encoded.Add(translation[value ^ checksum]); checksum = value;
            }
            for (var index = 0; index < 256; index++) { var value = (byte)(source[index] >> 2); encoded.Add(translation[value ^ checksum]); checksum = value; }
            encoded.Add(translation[(checksum + (corrupt ? 1 : 0)) & 0x3f]); return encoded.ToArray();
        }
        const byte volume = 254; const byte track = 19; const byte sector = 11;
        var addressChecksum = (byte)(volume ^ track ^ sector ^ (corruptAddress ? 1 : 0));
        var address = FourAndFour(volume).Concat(FourAndFour(track)).Concat(FourAndFour(sector)).Concat(FourAndFour(addressChecksum));
        var data = Enumerable.Range(0, 256).Select(index => (byte)(index * 37 + 9)).ToArray();
        var calibration = new string('1', 100);
        var raw = calibration + Bits([0xd5,0xaa,0x96]) + Bits(address) + Bits([0xde,0xaa,0xeb,0xff,0xff,0xff]) + Bits([0xd5,0xaa,0xad]) + Bits(EncodeData(data, table, corruptData)) + Bits([0xde,0xaa,0xeb]) + "1";
        var intervals = BitsToIntervals(raw, 40);

        var result = new AppleGcrDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        var decoded = Assert.Single(result.Sectors!); Assert.Equal(track, decoded.Cylinder); Assert.Equal(sector, decoded.Number); Assert.Equal(256, decoded.SizeBytes);
        Assert.Equal(!corruptAddress && !corruptData, decoded.IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.AppleData && structure.Description.Contains(corruptData ? "invalid" : "valid", StringComparison.Ordinal));
        if (!corruptData) Assert.Equal(data, result.DecodedBytes.Skip(4).Take(256));
    }

    [Fact]
    public void AppleGcrDecoderReportsUnavailableIntegrityWhenDataBlockIsMissing()
    {
        var calibration = new string('1', 100); var mark = string.Concat(Convert.FromHexString("D5AA96").Select(value => Convert.ToString(value, 2).PadLeft(8, '0')));
        var address = string.Concat(Enumerable.Repeat("10101010", 8)); var epilogue = string.Concat(Convert.FromHexString("DEAAEB").Select(value => Convert.ToString(value, 2).PadLeft(8, '0')));
        var intervals = BitsToIntervals(calibration + mark + address + epilogue + "0001", 40); var result = new AppleGcrDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));
        Assert.Null(Assert.Single(result.Sectors!).IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.AppleAddress && structure.Description.Contains("unavailable", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void CommodoreGcrDecoderExtractsTrackSectorAndValidatesData(bool corruptHeader, bool corruptData)
    {
        int[] table = [0x0a,0x0b,0x12,0x13,0x0e,0x0f,0x16,0x17,0x09,0x19,0x1a,0x1b,0x0d,0x1d,0x1e,0x15];
        string Encode(IEnumerable<byte> values) => string.Concat(values.SelectMany(value => new[] { value >> 4, value & 15 }).Select(nibble => Convert.ToString(table[nibble], 2).PadLeft(5, '0')));
        const byte track = 23; const byte sector = 8; const byte id2 = 0xa1; const byte id1 = 0x1a;
        var headerChecksum = (byte)(sector ^ track ^ id2 ^ id1 ^ (corruptHeader ? 1 : 0));
        byte[] header = [0x08, headerChecksum, sector, track, id2, id1];
        var data = Enumerable.Range(0, 256).Select(index => (byte)(index * 43 + 5)).ToArray(); byte checksum = 0; foreach (var value in data) checksum ^= value;
        if (corruptData) checksum ^= 1;
        var dataBlock = new byte[] { 0x07 }.Concat(data).Append(checksum).ToArray();
        var raw = new string('1', 100) + "000" + new string('1', 20) + Encode(header) + "000000" + new string('1', 20) + Encode(dataBlock) + "0001";
        var intervals = BitsToIntervals(raw, 40);

        var result = new CommodoreGcrDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        var decoded = Assert.Single(result.Sectors!); Assert.Equal(track, decoded.Cylinder); Assert.Equal(sector, decoded.Number); Assert.Equal(256, decoded.SizeBytes);
        Assert.Equal(!corruptHeader && !corruptData, decoded.IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatData && structure.Description.Contains(corruptData ? "invalid" : "valid", StringComparison.Ordinal));
        Assert.Equal(data, result.DecodedBytes.Skip(7).Take(256));
    }

    [Fact]
    public void CommodoreGcrDecoderReportsUnavailableIntegrityWhenDataIsMissing()
    {
        int[] table = [0x0a,0x0b,0x12,0x13,0x0e,0x0f,0x16,0x17,0x09,0x19,0x1a,0x1b,0x0d,0x1d,0x1e,0x15];
        string Encode(IEnumerable<byte> values) => string.Concat(values.SelectMany(value => new[] { value >> 4, value & 15 }).Select(nibble => Convert.ToString(table[nibble], 2).PadLeft(5, '0')));
        byte[] header = [0x08, 0x03, 0x02, 0x01, 0xa1, 0xa1]; var raw = new string('1', 100) + "000" + new string('1', 20) + Encode(header) + "0001";
        var intervals = BitsToIntervals(raw, 40); var result = new CommodoreGcrDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));
        Assert.Null(Assert.Single(result.Sectors!).IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.CommodoreHeader && structure.Description.Contains("unavailable", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void AmigaMfmDecoderExtractsIdentityAndDecodesOddEvenData(bool corruptHeader, bool corruptData)
    {
        static byte Nibble(byte value, bool odd)
        {
            byte result = 0; var firstBit = odd ? 7 : 6; for (var index = 0; index < 4; index++) result |= (byte)(((value >> (firstBit - index * 2)) & 1) << (3 - index)); return result;
        }
        static byte[] EncodeOddEven(IReadOnlyList<byte> values)
        {
            var odd = new List<byte>(); var even = new List<byte>();
            for (var index = 0; index < values.Count; index += 2) { odd.Add((byte)((Nibble(values[index], true) << 4) | Nibble(values[index + 1], true))); even.Add((byte)((Nibble(values[index], false) << 4) | Nibble(values[index + 1], false))); }
            return odd.Concat(even).ToArray();
        }
        static (byte High, byte Low) Parity(IReadOnlyList<byte> encoded, bool split)
        {
            byte high = 0, low = 0;
            if (split) { var half = encoded.Count / 2; for (var index = 0; index < half; index += 2) { high ^= (byte)(encoded[index] ^ encoded[half + index]); low ^= (byte)(encoded[index + 1] ^ encoded[half + index + 1]); } }
            else for (var index = 0; index < encoded.Count; index += 4) { high ^= (byte)(encoded[index] ^ encoded[index + 2]); low ^= (byte)(encoded[index + 1] ^ encoded[index + 3]); }
            return (high, low);
        }
        const byte cylinder = 34; const byte head = 1; const byte sector = 7;
        byte[] info = [0xff, (byte)(cylinder << 1 | head), sector, 4]; var headerAndLabel = EncodeOddEven(info).Concat(new byte[16]).ToArray(); var headerParity = Parity(headerAndLabel, false);
        var data = Enumerable.Range(0, 512).Select(index => (byte)(index * 47 + 3)).ToArray(); var encodedData = EncodeOddEven(data); var dataParity = Parity(encodedData, true);
        if (corruptHeader) headerParity.High ^= 1; if (corruptData) dataParity.Low ^= 1;
        var encoded = headerAndLabel.Concat(new byte[] { 0,0,headerParity.High,headerParity.Low,0,0,dataParity.High,dataParity.Low }).Concat(encodedData).ToArray();
        var raw = string.Concat(Enumerable.Repeat("10", 50)) + Convert.ToString(0x44894489, 2).PadLeft(32, '0') + EncodeMfmBytesFromZero(encoded) + "001";
        var intervals = BitsToIntervals(raw, 40);

        var result = new AmigaMfmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));

        var decoded = Assert.Single(result.Sectors!); Assert.Equal(cylinder, decoded.Cylinder); Assert.Equal(head, decoded.Head); Assert.Equal(sector, decoded.Number); Assert.Equal(512, decoded.SizeBytes);
        Assert.Equal(!corruptHeader && !corruptData, decoded.IntegrityValid);
        Assert.Equal(data, result.DecodedBytes.Skip(4).Take(512));
    }

    [Fact]
    public void AmigaMfmDecoderReportsUnavailableIntegrityWhenDataIsTruncated()
    {
        var encodedHeader = new byte[28]; encodedHeader[0] = 0xf0; encodedHeader[2] = 0xf0;
        var raw = string.Concat(Enumerable.Repeat("10", 50)) + Convert.ToString(0x44894489, 2).PadLeft(32, '0') + EncodeMfmBytesFromZero(encodedHeader) + "001";
        var intervals = BitsToIntervals(raw, 40); var result = new AmigaMfmDecoder().Decode(new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));
        Assert.Null(Assert.Single(result.Sectors!).IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.AmigaSync && structure.Description.Contains("unavailable", StringComparison.Ordinal));
    }

    [Fact]
    public void NativeChecksumDecodersReportCorruptedBlocks()
    {
        var northstarData = new byte[512];
        var northstarBlock = Enumerable.Repeat((byte)0, 7)
            .Concat([(byte)0xfb, (byte)0x21])
            .Concat(northstarData)
            .Append((byte)0x01)
            .ToArray();
        var northstarIntervals = BitsToIntervals(EncodeMfmBytesFromZero(northstarBlock) + "001", 40);
        var northstar = new NorthstarMfmDecoder().Decode(new ScpRevolution(8_000_000, (uint)northstarIntervals.Count, northstarIntervals));

        static byte Reverse(byte value) { byte result = 0; for (var bit = 0; bit < 8; bit++) result = (byte)((result << 1) | ((value >> bit) & 1)); return result; }
        var heathkitBits = EncodeFmBytes(0, 0, 0, 0xbf, Reverse(1), Reverse(2), Reverse(3), Reverse(0xff)) + "001";
        var heathkitIntervals = BitsToIntervals(heathkitBits, 40);
        var heathkit = new HeathkitFmDecoder().Decode(new ScpRevolution(8_000_000, (uint)heathkitIntervals.Count, heathkitIntervals));

        Assert.False(Assert.Single(northstar.Sectors!).IntegrityValid);
        Assert.False(Assert.Single(heathkit.Sectors!).IntegrityValid);
    }

    [Theory]
    [InlineData("membrain.mfm", "44895554", FluxStructureKind.FormatHeader)]
    [InlineData("aed6200p.mfm", "5094", FluxStructureKind.FormatHeader)]
    [InlineData("qdmo5.mfm", "A914A914A914A914A9144491", FluxStructureKind.FormatHeader)]
    [InlineData("centurion.mfm", "91224489", FluxStructureKind.FormatHeader)]
    [InlineData("emu.fm", "4545555545545445", FluxStructureKind.FormatHeader)]
    [InlineData("arburg", "5555555555249249", FluxStructureKind.FormatHeader)]
    [InlineData("victor9k.gcr", "5555555555551111", FluxStructureKind.FormatHeader)]
    [InlineData("tycom.fm", "55111444", FluxStructureKind.FormatData)]
    [InlineData("dec.rx02", "55111545", FluxStructureKind.FormatData)]
    public void SignatureMfmDecodersRecognizeTheirNativeMarks(string decoderId, string hexadecimal, FluxStructureKind expectedKind)
    {
        var mark = string.Concat(Convert.FromHexString(hexadecimal).Select(value => Convert.ToString(value, 2).PadLeft(8, '0')));
        var calibration = decoderId is "emu.fm" or "tycom.fm" or "dec.rx02" or "arburg" or "victor9k.gcr" ? "" : string.Concat(Enumerable.Repeat("10", 50));
        var bits = calibration + string.Concat(Enumerable.Repeat(mark + "000", 4)) + "001";
        var intervals = BitsToIntervals(bits, 40);
        var result = new FluxDecoderRegistry().Decode(decoderId, new ScpRevolution(8_000_000, (uint)intervals.Count, intervals));
        Assert.Contains(result.Structures, structure => structure.Kind == expectedKind);
    }

    [Fact]
    public void DecoderRegistrySelectsMostConvincingRevolution()
    {
        var weak = new ScpRevolution(8_000_000, 2, [40u, 40u]);
        var prologues = string.Concat(Enumerable.Repeat(Convert.ToString(0xD5AA96, 2).PadLeft(24, '0') + "1", 8));
        var strongIntervals = BitsToIntervals(prologues, 40);
        var strong = new ScpRevolution(8_000_000, (uint)strongIntervals.Count, strongIntervals);
        var best = new FluxDecoderRegistry().DecodeBest([weak, strong], "apple2.gcr");
        Assert.NotNull(best); Assert.Equal(1, best.Value.RevolutionIndex); Assert.Equal("apple2.gcr", best.Value.Result.DecoderId);
    }

    [Fact]
    public void ConversionTagPatternIsAppliedWithoutForcingBrackets()
    {
        var planner = new ConversionPlanner(new BuiltInImageFormatCatalog());
        var output = Assert.Single(planner.Plan("disk.scp", "out", "disk", [new ConversionSelection("ibm.720", new HashSet<string>())], true, "_{tag}"));
        Assert.Equal("disk_PC-720.ima", Path.GetFileName(output.OutputPath));
        Assert.Throws<ArgumentException>(() => planner.Plan("disk.scp", "out", "disk", [new ConversionSelection("ibm.720", new HashSet<string>())], true, "_format"));
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

    private sealed class ScriptedRunner(params GwExecutionResult[] results) : IGreaseweazleRunner
    {
        private readonly Queue<GwExecutionResult> _results = new(results);
        public List<GwCommand> Commands { get; } = [];
        public bool IsRunning { get; private set; }
        public Task<GwExecutionResult> RunAsync(GwCommand command, IProgress<GwOutputLine>? output = null, CancellationToken cancellationToken = default)
        {
            Commands.Add(command); IsRunning = true;
            try { return Task.FromResult(_results.Dequeue()); }
            finally { IsRunning = false; }
        }
    }

    private static string EncodeMfmBytes(params byte[] values) { var result = new System.Text.StringBuilder(); var previous = 1; foreach (var value in values) for (var bit = 7; bit >= 0; bit--) { var data = (value >> bit) & 1; var clock = previous == 0 && data == 0 ? 1 : 0; result.Append(clock).Append(data); previous = data; } return result.ToString(); }
    private static string EncodeMfmBytesFromZero(params byte[] values) { var result = new System.Text.StringBuilder(); var previous = 0; foreach (var value in values) for (var bit = 7; bit >= 0; bit--) { var data = (value >> bit) & 1; var clock = previous == 0 && data == 0 ? 1 : 0; result.Append(clock).Append(data); previous = data; } return result.ToString(); }
    private static string EncodeFmBytes(params byte[] values) => string.Concat(values.SelectMany(value => Enumerable.Range(0, 8).Select(bit => "1" + (((value >> (7 - bit)) & 1) != 0 ? "1" : "0"))));
    private static List<uint> BitsToIntervals(string bits, uint cellTicks) { var result = new List<uint>(); var cells = 0; foreach (var bit in bits) { cells++; if (bit == '1') { result.Add((uint)cells * cellTicks); cells = 0; } } return result; }
    private static ushort TestCrc16(IEnumerable<byte> values) => TestCrc16(values, 0x1021, 0xffff);
    private static ushort TestCrc16(IEnumerable<byte> values, ushort polynomial, ushort initial) { var crc = initial; foreach (var value in values) { crc ^= (ushort)(value << 8); for (var bit = 0; bit < 8; bit++) crc = (ushort)((crc & 0x8000) != 0 ? (crc << 1) ^ polynomial : crc << 1); } return crc; }
}
