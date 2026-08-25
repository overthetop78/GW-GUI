using GWGUI.App;
using GWGUI.Emulation;
using GWGUI.Emulation.Amiga;
using GWGUI.Emulation.Amiga.Cores;
using System.IO;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GWGUI.Tests;

[Trait("Category", "LocalAssets")]
public sealed class AmigaExternalCoreTests
{
    [Fact]
    public void RelativeCorePath_IsRejectedBeforeNativeLoading()
    {
        using var core = new AmigaExternalCore("puae_libretro.dll");
        var error = Assert.Throws<ArgumentException>(() => core.Initialize(
            AmigaMachineConfiguration.A500(FindKickstart()), CreateSession()));
        Assert.Contains("absolute", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void MissingCorePath_ReportsAmigaCoreNotFound()
    {
        var missing = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "missing.dll");
        using var core = new AmigaExternalCore(missing);
        var error = Assert.Throws<FileNotFoundException>(() => core.Initialize(
            AmigaMachineConfiguration.A500(FindKickstart()), CreateSession()));
        Assert.Contains("AmigaCoreNotFound", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void NativeLibraryWithoutRequiredExports_IsRejectedAndCanBeDisposedTwice()
    {
        var session = CreateSession();
        using var core = new AmigaExternalCore(Path.Combine(Environment.SystemDirectory, "version.dll"));
        try
        {
            Assert.Throws<EntryPointNotFoundException>(() => core.Initialize(
                AmigaMachineConfiguration.A500(FindKickstart()), session));
            core.Dispose();
            core.Dispose();
        }
        finally
        {
            if (Directory.Exists(session)) Directory.Delete(session, true);
        }
    }

    [Fact]
    public void InvalidNativeLibraryFile_IsRejected()
    {
        var root = CreateSession();
        Directory.CreateDirectory(root);
        var invalidLibrary = Path.Combine(root, "invalid.dll");
        File.WriteAllText(invalidLibrary, "not a native library");
        using var core = new AmigaExternalCore(invalidLibrary);
        try
        {
            Assert.Throws<BadImageFormatException>(() => core.Initialize(
                AmigaMachineConfiguration.A500(FindKickstart()), Path.Combine(root, "session")));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task RepeatedStartStop_ReleasesEveryNativeSession()
    {
        var repository = FindRepositoryRoot();
        var kickstart = Path.Combine(repository, "image_test", "Roms", "Bios", "Kickstart 1.3.rom");
        var corePath = Path.Combine(repository, "artifacts", "ppua", "puae_libretro.dll");
        var sessions = Path.Combine(Path.GetTempPath(), "GWGUI-Amiga-Stress", Guid.NewGuid().ToString("N"));
        var count = int.TryParse(Environment.GetEnvironmentVariable("GWGUI_AMIGA_STRESS_COUNT"), out var requested)
            ? Math.Clamp(requested, 1, 500) : 5;
        var process = Process.GetCurrentProcess();
        var initialHandles = process.HandleCount;
        var configuration = AmigaMachineConfiguration.A500(kickstart);
        var hostExecutable = Path.Combine(AppContext.BaseDirectory, "gwgui.app.exe");
        var engine = new AmigaEngine();
        var context = CreateContext(sessions, corePath, hostExecutable);
        try
        {
            for (var iteration = 0; iteration < count; iteration++)
            {
                await using (var machine = engine.CreateMachine(configuration, context))
                {
                    await machine.Lifecycle.StartAsync();
                    await WaitForFrame(machine, TimeSpan.FromSeconds(10));
                }
                Assert.Empty(Directory.Exists(sessions) ? Directory.EnumerateDirectories(sessions) : []);
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            process.Refresh();
            Assert.InRange(process.HandleCount, 0, initialHandles + 25);
        }
        finally
        {
            if (Directory.Exists(sessions)) Directory.Delete(sessions, true);
        }
    }

    public static TheoryData<string, string> BootableComputerModels => new()
    {
        { "A500OG", "Kickstart 1.2.rom" },
        { "A500", "Kickstart 1.3.rom" },
        { "A500PLUS", "Kickstart 2.0.rom" },
        { "A600", "Kickstart 3.1.rom" },
        { "A1200OG", "Kickstart 3.1.rom" },
        { "A1200", "Kickstart 3.1.rom" },
        { "A2000OG", "Kickstart 1.3.rom" },
        { "A2000", "Kickstart 3.1.rom" },
        { "A4030", "Kickstart 3.1.rom" },
        { "A4040", "Kickstart 3.1.rom" }
    };

    [Theory]
    [MemberData(nameof(BootableComputerModels))]
    public void ComputerModel_WithMatchingLocalKickstart_ProducesVideoAndAudio(string model, string romName)
    {
        var repository = FindRepositoryRoot();
        var kickstart = Path.Combine(repository, "image_test", "Roms", "Bios", romName);
        var session = Path.Combine(Path.GetTempPath(), "GWGUI-Amiga-Models", model, Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(session);
        try
        {
            using var core = new AmigaExternalCore(Path.Combine(repository, "artifacts", "ppua", "puae_libretro.dll"));
            core.Initialize(new AmigaMachineConfiguration(model, kickstart), session);
            for (var frame = 0; frame < 300; frame++) core.RunFrame();
            Assert.NotNull(core.LatestVideoFrame);
            Assert.NotNull(core.LatestAudioChunk);
            Assert.True(core.LatestVideoFrame!.Sequence > 0);
            Assert.InRange(core.LatestVideoFrame.Width, 320, 1920);
            Assert.InRange(core.LatestVideoFrame.Height, 200, 1080);
            Assert.True(core.LatestAudioChunk!.FrameCount > 0);
        }
        finally
        {
            if (Directory.Exists(session)) Directory.Delete(session, true);
        }
    }

    [Fact]
    public void A500_MultidiskPlaylist_ExposesAndSelectsEveryDisk()
    {
        var repository = FindRepositoryRoot();
        var kickstart = Path.Combine(repository, "image_test", "Roms", "Bios", "Kickstart 1.3.rom");
        var first = @"F:\Disquettes\Amiga Workbench\Amiga_Workbench_1.3.3.adf";
        var second = Path.Combine(repository, "image_test", "validated_images", "Commodore", "Amiga",
            "3.5 pouces DD - AmigaDOS OFS", "Boot-DD-OFS.adf");
        var session = Path.Combine(Path.GetTempPath(), "GWGUI-Amiga-Multidisk", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(session);
        try
        {
            using var core = new AmigaExternalCore(Path.Combine(repository, "artifacts", "ppua", "puae_libretro.dll"));
            var configuration = AmigaMachineConfiguration.A500(kickstart) with
            {
                Floppies = [new AmigaFloppyConfiguration(first, "Workbench"), new AmigaFloppyConfiguration(second, "Boot test")]
            };
            core.Initialize(configuration, session);
            for (var frame = 0; frame < 30; frame++) core.RunFrame();
            Assert.Equal(2, core.DiskCount);
            core.SelectDisk(1);
            Assert.Equal(1, core.CurrentDiskIndex);
            core.SelectDisk(0);
            Assert.Equal(0, core.CurrentDiskIndex);
        }
        finally
        {
            if (Directory.Exists(session)) Directory.Delete(session, true);
        }
    }

    [Fact]
    public void A500_WithKickstartAndWorkbenchAdf_ProducesVideoAndAudio()
    {
        var repository = FindRepositoryRoot();
        var kickstart = Path.Combine(repository, "image_test", "Roms", "Bios", "Kickstart 1.3.rom");
        var workbench = @"F:\Disquettes\Amiga Workbench\Amiga_Workbench_1.3.3.adf";
        Assert.True(File.Exists(kickstart), $"Missing Kickstart: {kickstart}");
        Assert.True(File.Exists(workbench), $"Missing Workbench ADF: {workbench}");
        Assert.Equal(524288, new FileInfo(kickstart).Length);
        using (var stream = File.OpenRead(kickstart))
            Assert.Equal("1D68BA18412501D2A4B307A0A632B94A50B839C2C7C5FF2DF6DE2C38B99A921F",
                Convert.ToHexString(SHA256.HashData(stream)));

        var session = Path.Combine(Path.GetTempPath(), "GWGUI-Amiga-Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(session);
        try
        {
            using var core = new AmigaExternalCore(Path.Combine(repository, "artifacts", "ppua", "puae_libretro.dll"));
            core.Initialize(AmigaMachineConfiguration.A500(kickstart, workbench), session);
            var sessionKickstart = Path.Combine(session, "System", "kick34005.A500");
            Assert.True(File.Exists(sessionKickstart));
            using (var stream = File.OpenRead(sessionKickstart))
                Assert.Equal("1D68BA18412501D2A4B307A0A632B94A50B839C2C7C5FF2DF6DE2C38B99A921F",
                    Convert.ToHexString(SHA256.HashData(stream)));
            for (var frame = 0; frame < 1500; frame++) core.RunFrame();

            var video = Assert.IsType<GWGUI.Emulation.Contracts.VideoFrame>(core.LatestVideoFrame);
            Assert.InRange(video.Width, 320, 1920);
            Assert.InRange(video.Height, 200, 1080);
            Assert.True(video.Pixels.Length >= video.Pitch * video.Height);
            Assert.True(video.Sequence >= 100);
            Assert.True(video.Pixels.Span.IndexOfAnyExcept((byte)0) >= 0);
            Assert.True(video.Pixels.ToArray().Distinct().Take(5).Count() >= 5);
            if (Environment.GetEnvironmentVariable("GWGUI_AMIGA_FRAME_PATH") is { Length: > 0 } framePath)
                SaveFrame(video, framePath);
            Assert.InRange(core.FramesPerSecond, 49, 61);
            Assert.InRange(core.SampleRate, 22050, 96000);
            Assert.InRange(core.Region, 0u, 1u);
            Assert.Equal(0u, core.GetMemorySize(0));
            Assert.Equal(0, core.GetMemoryData(0));
            Assert.NotNull(core.LatestAudioChunk);
            Assert.True(core.Options.Count > 100);
            var modelOption = Assert.Single(core.Options, option => option.Key == "puae_model");
            Assert.Contains(modelOption.Values, value => value.Value == "A500");
            Assert.Contains(core.Options, option => option.Key == "puae_kickstart");
            core.SetOption("puae_floppy_write_protection", "enabled");
            core.RunFrame();
            Assert.Throws<ArgumentOutOfRangeException>(() => core.SetOption("puae_floppy_write_protection", "invalid"));
            core.SetInput(new EmulationInputSnapshot(new HashSet<EmulationKey> { EmulationKey.LeftAmiga, EmulationKey.M },
                new EmulationPointerState(12, -4, 0, true, false, false),
                [new EmulationControllerState(1, 0, 0, 0, 0, 0, 0)]));
            core.RunFrame();
            var state = core.SaveState();
            Assert.True(state.Length > 1024);
            core.RunFrame();
            core.LoadState(state);
            core.RunFrame();
        }
        finally
        {
            if (Directory.Exists(session)) Directory.Delete(session, true);
        }
    }

    [Fact]
    public void InvalidConfiguredCoreOption_IsRejectedDuringInitialization()
    {
        var repository = FindRepositoryRoot();
        var session = Path.Combine(Path.GetTempPath(), "GWGUI-Amiga-InvalidOption", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(session);
        try
        {
            using var core = new AmigaExternalCore(Path.Combine(repository, "artifacts", "ppua", "puae_libretro.dll"));
            var configuration = AmigaMachineConfiguration.A500(
                Path.Combine(repository, "image_test", "Roms", "Bios", "Kickstart 1.3.rom")) with
            {
                Options = new Dictionary<string, string> { ["puae_floppy_write_protection"] = "definitely-invalid" }
            };

            var error = Assert.Throws<InvalidDataException>(() => core.Initialize(configuration, session));
            Assert.Contains("puae_floppy_write_protection", error.Message, StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(session)) Directory.Delete(session, true);
        }
    }

    [Fact]
    public async Task Engine_RunsTwoIndependentA500Machines()
    {
        var repository = FindRepositoryRoot();
        var kickstart = Path.Combine(repository, "image_test", "Roms", "Bios", "Kickstart 1.3.rom");
        var adf = @"F:\Disquettes\Amiga Workbench\Amiga_Workbench_1.3.3.adf";
        var corePath = Path.Combine(repository, "artifacts", "ppua", "puae_libretro.dll");
        var sessions = Path.Combine(Path.GetTempPath(), "GWGUI-Amiga-Tests", Guid.NewGuid().ToString("N"));
        var engine = new AmigaEngine();
        var context = CreateContext(sessions, corePath, Path.Combine(AppContext.BaseDirectory, "gwgui.app.exe"));
        await using var first = engine.CreateMachine(AmigaMachineConfiguration.A500(kickstart, adf), context);
        await using var second = engine.CreateMachine(AmigaMachineConfiguration.A500(kickstart), context);

        await first.Lifecycle.StartAsync();
        await second.Lifecycle.StartAsync();
        await WaitForFrame(first, TimeSpan.FromSeconds(15));
        await WaitForFrame(second, TimeSpan.FromSeconds(15));

        Assert.Equal("PUAE", first.Runtime.EmulatorName);
        Assert.False(string.IsNullOrWhiteSpace(first.Runtime.EmulatorVersion));
        Assert.Contains("adf", first.Runtime.SupportedContentExtensions);
        Assert.Contains("hdf", first.Runtime.SupportedContentExtensions);
        Assert.Contains("iso", first.Runtime.SupportedContentExtensions);
        Assert.Contains("m3u", first.Runtime.SupportedContentExtensions);
        Assert.NotEqual(first.Id, second.Id);
        Assert.Equal(GWGUI.Emulation.Enums.EmulationMachineState.Running, first.State);
        Assert.Equal(GWGUI.Emulation.Enums.EmulationMachineState.Running, second.State);
        await first.Lifecycle.PauseAsync();
        Assert.Equal(GWGUI.Emulation.Enums.EmulationMachineState.Paused, first.State);
        Assert.Equal(GWGUI.Emulation.Enums.EmulationMachineState.Running, second.State);
        await first.Media.EjectAsync(EmulationMediaSlot.Floppy0);
        var replacement = Path.Combine(repository, "image_test", "validated_images", "Commodore", "Amiga",
            "3.5 pouces DD - AmigaDOS OFS", "Boot-DD-OFS.adf");
        await first.Media.InsertAsync(new EmulationMedia(
            replacement, EmulationMediaSlot.Floppy0, EmulationMediaType.Floppy, false, true));
        var statePath = Path.Combine(sessions, "first.state");
        await first.SavedStates.SaveAsync(statePath);
        Assert.True(new FileInfo(statePath).Length > 1024);
        await first.SavedStates.LoadAsync(statePath);
        await first.Lifecycle.ResumeAsync();
        await first.Lifecycle.StopAsync();
        await second.Lifecycle.StopAsync();
        Assert.Equal(GWGUI.Emulation.Enums.EmulationMachineState.Stopped, first.State);
        Assert.Equal(GWGUI.Emulation.Enums.EmulationMachineState.Stopped, second.State);
    }

    [Fact]
    public async Task Machine_ForwardsNativeAudioToConfiguredOutput()
    {
        var repository = FindRepositoryRoot();
        var output = new RecordingAudioOutput();
        var engine = new AmigaEngine();
        var context = CreateContext(
            Path.Combine(Path.GetTempPath(), "GWGUI-Amiga-Tests", Guid.NewGuid().ToString("N")),
            Path.Combine(repository, "artifacts", "ppua", "puae_libretro.dll"),
            Path.Combine(AppContext.BaseDirectory, "gwgui.app.exe"), () => output);
        await using var machine = engine.CreateMachine(AmigaMachineConfiguration.A500(
            Path.Combine(repository, "image_test", "Roms", "Bios", "Kickstart 1.3.rom")), context);
        await machine.Lifecycle.StartAsync();
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        while (output.SamplesWritten == 0) await Task.Delay(20, cancellation.Token);
        await machine.Lifecycle.StopAsync();
        Assert.InRange(output.SampleRate, 22050, 96000);
        Assert.True(output.SamplesWritten > 0);
        Assert.True(output.WasStopped);
    }

    [Fact]
    public async Task ProcessCore_RejectsContentExtensionNotAdvertisedByTheNativeCore()
    {
        var repository = FindRepositoryRoot();
        var root = Path.Combine(Path.GetTempPath(), "GWGUI-Amiga-Unsupported", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var unsupported = Path.Combine(root, "not-amiga.txt");
        await File.WriteAllTextAsync(unsupported, "not an Amiga image");
        var engine = new AmigaEngine();
        var context = CreateContext(Path.Combine(root, "Sessions"),
            Path.Combine(repository, "artifacts", "ppua", "puae_libretro.dll"),
            Path.Combine(AppContext.BaseDirectory, "gwgui.app.exe"));
        await using var machine = engine.CreateMachine(AmigaMachineConfiguration.A500(
            Path.Combine(repository, "image_test", "Roms", "Bios", "Kickstart 1.3.rom"), unsupported), context);
        try
        {
            var error = await Assert.ThrowsAsync<InvalidOperationException>(() => machine.Lifecycle.StartAsync().AsTask());
            Assert.Contains("does not support '.txt'", error.Message, StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    private static async Task WaitForFrame(IEmulatedMachine machine, TimeSpan timeout)
    {
        using var cancellation = new CancellationTokenSource(timeout);
        while (machine.Video.LatestFrame is null)
        {
            if (machine.State == EmulationMachineState.Faulted)
                throw new InvalidOperationException("The Amiga machine faulted before producing video.");
            await Task.Delay(20, cancellation.Token);
        }
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "GWGUI.sln")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException("GWGUI repository root not found.");
    }

    private static string FindKickstart() => Path.Combine(FindRepositoryRoot(), "image_test", "Roms", "Bios", "Kickstart 1.3.rom");

    private static string CreateSession() => Path.Combine(Path.GetTempPath(), "GWGUI-Amiga-Tests", Guid.NewGuid().ToString("N"));

    private static AmigaMachineCreationContext CreateContext(string sessions, string corePath,
        string hostExecutable, Func<IAudioOutput?>? audioOutputFactory = null) =>
        new(sessions, corePath, hostExecutable, audioOutputFactory, null);

    private static void SaveFrame(GWGUI.Emulation.Contracts.VideoFrame frame, string path)
    {
        var format = frame.PixelFormat == GWGUI.Emulation.Enums.EmulationPixelFormat.Rgb565 ? PixelFormats.Bgr565 : PixelFormats.Bgr32;
        var bitmap = BitmapSource.Create(frame.Width, frame.Height, 96, 96, format, null,
            frame.Pixels.ToArray(), frame.Pitch);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        using var stream = File.Create(path);
        encoder.Save(stream);
    }

    private sealed class RecordingAudioOutput : IAudioOutput
    {
        public int SampleRate { get; private set; }
        public long SamplesWritten { get; private set; }
        public bool WasStopped { get; private set; }
        public void Start(int sampleRate) => SampleRate = sampleRate;
        public void Write(ReadOnlySpan<short> interleavedStereo) => SamplesWritten += interleavedStereo.Length;
        public void Flush() { }
        public void Stop() => WasStopped = true;
        public void Dispose() { }
    }
}
