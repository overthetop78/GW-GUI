using GWGUI.App.Contracts.Progress;
using GWGUI.App.Services.Hardware;
using GWGUI.App.Services.Operations;
using GWGUI.App.ViewModels.Main;
using GWGUI.App.Views.Controls.Visualization;
using GWGUI.Domain.Commands;
using GWGUI.Domain.Commands.Execution;
using GWGUI.Domain.Commands.Progress;
using GWGUI.Domain.Formats;
using GWGUI.Domain.Hardware;
using GWGUI.Domain.Hardware.Parsing;
using GWGUI.Domain.Naming;
using GWGUI.Domain.Profiles;
using GWGUI.Domain.Settings;
using GWGUI.Domain.Settings.Hardware;
using GWGUI.Domain.Settings.Window;
using GWGUI.Infrastructure.Hardware;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Exploration;
using GWGUI.MediaEngine.Exploration.Contracts;
using GWGUI.MediaEngine.Exploration.Scp;
using System.IO;
using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.MediaEngine;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Flux;
using GWGUI.Infrastructure.Processes;
using GWGUI.Infrastructure.Settings;
using SkiaSharp;
using System.Windows;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Threading;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace GWGUI.Tests;

public sealed class HardwareAndProgressTests : CoreTestBase
{
    [Fact]
    public void DisplayCommandQuotesPathsWithSpaces()
    {
        var command = new GwCommand("C:\\GW Tools\\gw.exe", "read", ["F:\\Disquettes été\\Tilt n°117 漢字.scp"]);
        Assert.Equal("\"C:\\GW Tools\\gw.exe\" read \"F:\\Disquettes été\\Tilt n°117 漢字.scp\"", command.ToDisplayString());
        Assert.Equal("F:\\Disquettes été\\Tilt n°117 漢字.scp", command.Arguments[0]);
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
    public void ScpReaderReadsCoreHeaderMetadata()
    {
        byte[] header = [(byte)'S', (byte)'C', (byte)'P', 0x24, 0, 5, 0, 83, 0, 0, 0, 0, 0, 0, 0, 0];
        var result = ScpReader.ReadHeader(header);
        Assert.Equal(84, result.TrackCount);
        Assert.Equal(5, result.Revolutions);
        Assert.Equal(ScpHeadSelection.Both, result.Heads);
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
        var checksum = ScpFormatAlgorithms.ComputeChecksum(data.AsSpan(ScpFormatConstants.TrackTableOffset));
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(ScpFormatConstants.ChecksumOffset, ScpFormatConstants.ChecksumLength), checksum);
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
    public async Task HardwareRegistryKeepsDisconnectedControllersAndMergesScannedUsbIdentity()
    {
        var output = new[]
        {
            new GwOutputLine(DateTimeOffset.UtcNow, GwOutputStream.Standard, "Model: Greaseweazle V4.1"),
            new GwOutputLine(DateTimeOffset.UtcNow, GwOutputStream.Standard, "Serial: GW-NEW-123")
        };
        var runner = new ScriptedRunner(new GwExecutionResult(0, false, TimeSpan.FromMilliseconds(10), output));
        IHardwareRegistry registry = new GreaseweazleHardwareRegistry(
            new StaticSerialDeviceDiscovery([new SerialDevice("COM9", "USB\\VID_1209&PID_4D69", "Greaseweazle serial device", 0x1209, 0x4d69)]),
            runner);
        var configured = new[]
        {
            new ControllerSettings { UsbId = "GW-OLD-001", LastPort = "COM3", Model = "Greaseweazle F7", IsAvailable = true }
        };

        var scanned = await registry.ScanAsync("gw.exe", configured);

        var disconnected = Assert.Single(scanned.ConfiguredControllers, controller => controller.UsbId == "GW-OLD-001");
        Assert.False(disconnected.IsAvailable);
        Assert.Equal("COM3", disconnected.LastPort);
        var discovered = Assert.Single(scanned.UnconfiguredControllers, controller => controller.UsbId == "GW-NEW-123");
        Assert.True(discovered.IsAvailable);
        Assert.Equal("COM9", discovered.LastPort);
        Assert.Equal("Greaseweazle V4.1", discovered.Model);
        var command = Assert.Single(runner.Commands);
        Assert.Equal("info", command.Verb);
        Assert.Equal(["--device", "COM9"], command.Arguments);
    }

    [Fact]
    public async Task HardwareRegistryTracksMultipleControllersAcrossDisconnectPortChangeAndReconnect()
    {
        var discovery = new MutableSerialDeviceDiscovery(
        [
            new("COM3", "PNP-A", "Greaseweazle A", 0x1209, 0x4d69),
            new("COM4", "PNP-B", "Greaseweazle B", 0x1209, 0x4d69)
        ]);
        var runner = new DeviceInfoRunner(new Dictionary<string, (string Serial, string Model)>
        {
            ["COM3"] = ("GW-A", "Greaseweazle V4.1"),
            ["COM4"] = ("GW-B", "Greaseweazle F7"),
            ["COM7"] = ("GW-B", "Greaseweazle F7"),
            ["COM9"] = ("GW-A", "Greaseweazle V4.1")
        });
        IHardwareRegistry registry = new GreaseweazleHardwareRegistry(discovery, runner);

        var initialScan = await registry.ScanAsync("gw.exe", []);
        var initial = initialScan.UnconfiguredControllers;
        Assert.Equal(2, initial.Count);
        Assert.All(initial, controller => Assert.True(controller.IsAvailable));

        discovery.Devices = [new("COM7", "PNP-B", "Greaseweazle B", 0x1209, 0x4d69)];
        var disconnected = (await registry.ScanAsync("gw.exe", initial)).ConfiguredControllers;
        var controllerA = Assert.Single(disconnected, controller => controller.UsbId == "GW-A");
        Assert.False(controllerA.IsAvailable);
        Assert.Equal("COM3", controllerA.LastPort);
        var controllerB = Assert.Single(disconnected, controller => controller.UsbId == "GW-B");
        Assert.True(controllerB.IsAvailable);
        Assert.Equal("COM7", controllerB.LastPort);

        discovery.Devices =
        [
            new("COM9", "PNP-A", "Greaseweazle A", 0x1209, 0x4d69),
            new("COM7", "PNP-B", "Greaseweazle B", 0x1209, 0x4d69)
        ];
        var reconnected = (await registry.ScanAsync("gw.exe", disconnected)).ConfiguredControllers;
        Assert.Equal(2, reconnected.Count);
        Assert.All(reconnected, controller => Assert.True(controller.IsAvailable));
        Assert.Equal("COM9", reconnected.Single(controller => controller.UsbId == "GW-A").LastPort);
        Assert.Equal("COM7", reconnected.Single(controller => controller.UsbId == "GW-B").LastPort);
    }

    [Fact]
    public async Task HardwareRegistryDoesNotProbeUnrelatedSerialPorts()
    {
        var runner = new ScriptedRunner(new GwExecutionResult(0, false, TimeSpan.Zero, []));
        IHardwareRegistry registry = new GreaseweazleHardwareRegistry(
            new StaticSerialDeviceDiscovery([new SerialDevice("COM6", "USB\\VID_2341&PID_0043\\ARDUINO", "Arduino", 0x2341, 0x0043)]),
            runner);

        var scanned = await registry.ScanAsync("gw.exe", []);

        Assert.Empty(scanned.ConfiguredControllers);
        Assert.Empty(scanned.UnconfiguredControllers);
        Assert.Empty(runner.Commands);
    }

    [Fact]
    public async Task StartupHardwareMonitorPersistsAvailabilityWithoutChangingConfiguration()
    {
        var controller = new ControllerSettings { UsbId = "GW-ONE", LastPort = "COM3", Model = "Greaseweazle V4.1", IsAvailable = true };
        var drive = new DriveSettings { ControllerUsbId = "GW-ONE", Selection = "A", Size = "3.5", Density = "HD" };
        var settings = new AppSettings { GwExecutablePath = WindowsPowerShell, Controllers = [controller], Drives = [drive] };
        var scanned = new ControllerSettings { UsbId = "GW-ONE", LastPort = "COM3", Model = controller.Model, IsAvailable = false };
        var store = new RecordingSettingsStore();
        var monitor = new StartupHardwareMonitor(new StaticHardwareRegistry([scanned]), store);

        var result = await monitor.CheckAsync(settings);

        Assert.True(result.Performed);
        Assert.Same(scanned, Assert.Single(result.MissingControllers));
        Assert.Same(drive, Assert.Single(settings.Drives));
        Assert.Equal("GW-ONE", settings.Drives[0].ControllerUsbId);
        Assert.Equal(1, store.SaveCount);
    }

    [Fact]
    public async Task StartupHardwareMonitorUpdatesPortSilentlyWhenControllerIsFound()
    {
        var settings = new AppSettings
        {
            GwExecutablePath = WindowsPowerShell,
            Controllers = [new() { UsbId = "GW-ONE", LastPort = "COM3", IsAvailable = true }]
        };
        var found = new ControllerSettings { UsbId = "GW-ONE", LastPort = "COM5", IsAvailable = true };
        var store = new RecordingSettingsStore();
        var monitor = new StartupHardwareMonitor(new StaticHardwareRegistry([found]), store);

        var result = await monitor.CheckAsync(settings);

        Assert.True(result.Performed);
        Assert.Empty(result.MissingControllers);
        Assert.Equal("COM5", Assert.Single(settings.Controllers).LastPort);
        Assert.Equal(1, store.SaveCount);
    }

    [Fact]
    public async Task StartupHardwareMonitorMarksConfiguredControllersUnavailableWithoutHostTools()
    {
        var settings = new AppSettings
        {
            GwExecutablePath = @"Z:\missing\gw.exe",
            Controllers = [new() { UsbId = "GW-ONE", LastPort = "COM3", IsAvailable = true }]
        };
        var store = new RecordingSettingsStore();
        var monitor = new StartupHardwareMonitor(new StaticHardwareRegistry([]), store);

        var result = await monitor.CheckAsync(settings);

        Assert.True(result.Performed);
        Assert.False(Assert.Single(settings.Controllers).IsAvailable);
        Assert.Single(result.MissingControllers);
        Assert.Equal(1, store.SaveCount);
    }

    [Fact]
    public async Task StartupHardwareMonitorReportsNewControllerWithoutConfiguringIt()
    {
        var settings = new AppSettings { GwExecutablePath = WindowsPowerShell };
        var detected = new ControllerSettings { UsbId = "GW-NEW", UsbSerialNumber = "GW-NEW", LastPort = "COM7", IsAvailable = true };
        var store = new RecordingSettingsStore();
        var monitor = new StartupHardwareMonitor(new StaticHardwareRegistry([], [detected]), store);

        var result = await monitor.CheckAsync(settings);

        Assert.Same(detected, Assert.Single(result.NewControllers));
        Assert.Empty(settings.Controllers);
        Assert.Empty(settings.UnconfiguredControllers);
        Assert.Equal(1, store.SaveCount);
    }

    [Fact]
    public async Task StartupHardwareMonitorRemembersDeclinedControllerAndDoesNotAskAgain()
    {
        var remembered = new ControllerSettings { UsbId = "GW-IGNORED", UsbSerialNumber = "GW-IGNORED", LastPort = "COM4", IsAvailable = false };
        var detected = new ControllerSettings { UsbId = "GW-IGNORED", UsbSerialNumber = "GW-IGNORED", LastPort = "COM9", IsAvailable = true };
        var settings = new AppSettings { GwExecutablePath = WindowsPowerShell, UnconfiguredControllers = [remembered] };
        var monitor = new StartupHardwareMonitor(new StaticHardwareRegistry([], [detected]), new RecordingSettingsStore());

        var result = await monitor.CheckAsync(settings);

        Assert.Empty(result.NewControllers);
        var retained = Assert.Single(settings.UnconfiguredControllers);
        Assert.Equal("COM9", retained.LastPort);
        Assert.True(retained.IsAvailable);
        Assert.Empty(settings.Controllers);
    }

    [Fact]
    public void PhysicalGreaseweazleDiscoveryFindsConnectedControllerWhenEnabled()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("GWGUI_TEST_PHYSICAL_DISCOVERY"), "1", StringComparison.Ordinal))
            return;

        var devices = new WindowsSerialDeviceDiscovery().FindSerialDevices();
        var controller = Assert.Single(devices, GreaseweazleDeviceMatcher.IsCandidate);
        Assert.Matches("^COM[0-9]+$", controller.Port);
        Assert.False(string.IsNullOrWhiteSpace(controller.StableId));
        Assert.True(controller.VendorId == 0x1209 || controller.UsbSerialNumber?.StartsWith("GW", StringComparison.OrdinalIgnoreCase) == true);
    }

    [Fact]
    public void WindowPlacementRejectsAWindowOutsideAllScreens()
    {
        var settings = new WindowPlacementSettings { Width = 1400, Height = 800, Left = 9000, Top = 9000 };
        var result = WindowPlacementPolicy.Normalize(settings, 1280, 720, 0, 0, 3840, 2160);
        Assert.Null(result.Left);
        Assert.Null(result.Top);
    }

    [Fact]
    public void WindowPlacementKeepsAVisibleSecondaryScreenPosition()
    {
        var settings = new WindowPlacementSettings { Width = 1400, Height = 800, Left = -1500, Top = 120 };
        var result = WindowPlacementPolicy.Normalize(settings, 1280, 720, -1920, 0, 5760, 2160);
        Assert.Equal(-1500, result.Left);
        Assert.Equal(120, result.Top);
    }

    [Fact]
    public void WindowPlacementClampsTheWholeWindowInsideTheVirtualDesktop()
    {
        var settings = new WindowPlacementSettings { Width = 1360, Height = 820, Left = 1200, Top = 700 };
        var result = WindowPlacementPolicy.Normalize(settings, 1280, 720, 0, 0, 2048, 1152);
        Assert.Equal(688, result.Left);
        Assert.Equal(332, result.Top);
    }

    [Theory]
    [InlineData(-2560, 0, 2560, 1440, 1.25, -1900, 100, -1900, 100, 1360, 820)]
    [InlineData(1920, 0, 2560, 1440, 1.25, 1600, 80, 1600, 80, 1360, 820)]
    [InlineData(0, -2160, 3840, 2160, 1.5, 100, -1300, 100, -1300, 1360, 820)]
    [InlineData(1920, 0, 1920, 1080, 1.0, 3400, 700, 2480, 260, 1360, 820)]
    [InlineData(0, 0, 1920, 1080, 1.5, 300, 200, 0, 0, 1280, 720)]
    public void WindowPlacementUsesTheActualMonitorWorkAreaAtDifferentDpi(
        double leftPixels, double topPixels, double widthPixels, double heightPixels, double scale,
        double savedLeft, double savedTop, double expectedLeft, double expectedTop, double expectedWidth, double expectedHeight)
    {
        var workLeft = leftPixels / scale;
        var workTop = topPixels / scale;
        var workWidth = widthPixels / scale;
        var workHeight = heightPixels / scale;
        var result = WindowPlacementPolicy.ConstrainToWorkArea(new(1360, 820, savedLeft, savedTop), workLeft, workTop, workWidth, workHeight);

        Assert.Equal(expectedLeft, result.Left);
        Assert.Equal(expectedTop, result.Top);
        Assert.Equal(expectedWidth, result.Width);
        Assert.Equal(expectedHeight, result.Height);
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
        Assert.Equal(GwTrackState.Success, first.State);
        Assert.Equal(1, retry!.CompletedTracks);
        Assert.Equal(GwTrackState.Retry, retry.State);
        Assert.Equal(2, second!.CompletedTracks);
        Assert.Equal(80, second.TotalOnHead);
        Assert.Equal(1, second.CompletedOnHead);
        Assert.True(second.Head0Expected);
        Assert.True(second.Head1Expected);
        Assert.Equal(Enumerable.Range(0, 80), second.Cylinders);
        Assert.Equal(1, second.NextCylinder);
        Assert.Equal(0, second.NextHead);
    }

    [Fact]
    public void ExternalReadProgressPublishesCommonStateAndBothFaceStrips()
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                var viewModel = new MainWindowViewModel("No hardware", "Ready");
                var face0 = new TrackProgressStrip();
                var face1 = new TrackProgressStrip();
                var controller = new OperationProgressController(
                    viewModel,
                    face0,
                    face1,
                    (key, values) => $"{key}:{string.Join(',', values)}");
                var published = new List<GWGUI.MediaEngine.Exploration.Contracts.IEtatLectureDisquette>();
                controller.ReadStateChanged += (_, state) => published.Add(state);

                controller.Begin();
                controller.Accept("Reading c=0-79:h=0-1 revs=3");
                controller.Accept("T0.0: Raw Flux");

                var state = Assert.IsAssignableFrom<GWGUI.MediaEngine.Exploration.Contracts.IEtatLectureDisquette>(
                    controller.CurrentReadState);
                Assert.Equal("Acquiring", state.Etape);
                Assert.Equal(1, state.NombrePistesTerminees);
                Assert.Equal(160, state.NombrePistesTotal);
                Assert.Equal(0, state.Cylindre);
                Assert.Equal(0, state.Face);
                Assert.Equal(160, state.EtatsPistes.Count);
                Assert.Equal("T0.0: Raw Flux", state.MessageExterne);
                Assert.Equal(80, face0.Segments.Count);
                Assert.Equal(80, face1.Segments.Count);
                Assert.Equal(TrackSegmentState.Success, face0.Segments[0].State);
                Assert.NotEmpty(published);
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(15)));
        Assert.Null(failure);
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
    public void GwProgressUsesConvertGeometryForPerSideSegments()
    {
        var tracker = new GwProgressTracker();
        Assert.Null(tracker.Accept("Converting c=0-79:h=0-1 -> c=0-79:h=0-1"));

        var progress = tracker.Accept("T24.0: Raw Flux (120817 flux in 599.49ms)");

        Assert.NotNull(progress);
        Assert.Equal(80, progress.TotalOnHead);
        Assert.Equal(160, progress.TotalTracks);
        Assert.True(progress.Head0Expected);
        Assert.True(progress.Head1Expected);
    }

    [Fact]
    public void GwProgressUsesEraseGeometryForPerSideSegments()
    {
        var tracker = new GwProgressTracker();
        Assert.Null(tracker.Accept("Erasing c=0-79:h=0-1, revs=3"));

        var progress = tracker.Accept("T0.1: Erasing Track");

        Assert.NotNull(progress);
        Assert.Equal(80, progress.TotalOnHead);
        Assert.Equal(160, progress.TotalTracks);
        Assert.True(progress.Head0Expected);
        Assert.True(progress.Head1Expected);
    }

    [Fact]
    public async Task ScpCaptureInfoReadsFinalMetadataWithoutDecodingFlux()
    {
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-scp-summary-{Guid.NewGuid():N}.scp");
        try
        {
            var data = BuildSingleTrackScp([100, 120, 140]);
            await File.WriteAllBytesAsync(path, data);
            var info = await ScpCaptureInfoReader.ReadAsync(path);

            Assert.Equal(1, info.CapturedTracks);
            Assert.Equal(0, info.MissingTracks);
            Assert.Equal(1, info.Cylinders);
            Assert.Equal(1, info.Sides);
            Assert.Equal(1, info.Header.Revolutions);
            Assert.True(info.ChecksumValid);
            Assert.Equal(data.Length, info.FileSize);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
