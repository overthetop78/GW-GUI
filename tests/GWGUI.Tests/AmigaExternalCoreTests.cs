using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GWGUI.Emulation.Amiga;
using GWGUI.Emulation.Amiga.Cores;
using GWGUI.Emulation;

namespace GWGUI.Tests;

public sealed class AmigaExternalCoreTests
{
    [Fact]
    public void A500_WithKickstartAndWorkbenchAdf_ProducesVideoAndAudio()
    {
        var repository = FindRepositoryRoot();
        var kickstart = Path.Combine(repository, "image_test", "Roms", "Bios", "Kickstart 1.3.rom");
        var workbench = @"F:\Disquettes\Amiga Workbench\Amiga_Workbench_1.3.3.adf";
        Assert.True(File.Exists(kickstart), $"Missing Kickstart: {kickstart}");
        Assert.True(File.Exists(workbench), $"Missing Workbench ADF: {workbench}");

        var session = Path.Combine(Path.GetTempPath(), "GWGUI-Amiga-Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(session);
        try
        {
            using var core = new AmigaExternalCore(Path.Combine(repository, "artifacts", "ppua", "puae_libretro.dll"));
            core.Initialize(AmigaMachineConfiguration.A500(kickstart, workbench), session);
            for (var frame = 0; frame < 1500; frame++) core.RunFrame();

            var video = Assert.IsType<GWGUI.Emulation.VideoFrame>(core.LatestVideoFrame);
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
    public async Task Engine_RunsTwoIndependentA500Machines()
    {
        var repository = FindRepositoryRoot();
        var kickstart = Path.Combine(repository, "image_test", "Roms", "Bios", "Kickstart 1.3.rom");
        var adf = @"F:\Disquettes\Amiga Workbench\Amiga_Workbench_1.3.3.adf";
        var corePath = Path.Combine(repository, "artifacts", "ppua", "puae_libretro.dll");
        var sessions = Path.Combine(Path.GetTempPath(), "GWGUI-Amiga-Tests", Guid.NewGuid().ToString("N"));
        var engine = new AmigaEngine(sessions, corePath);
        await using var first = engine.CreateAmigaMachine(AmigaMachineConfiguration.A500(kickstart, adf));
        await using var second = engine.CreateAmigaMachine(AmigaMachineConfiguration.A500(kickstart));

        await first.StartAsync();
        await second.StartAsync();
        await WaitForFrame(first, TimeSpan.FromSeconds(15));
        await WaitForFrame(second, TimeSpan.FromSeconds(15));

        Assert.NotEqual(first.Id, second.Id);
        Assert.Equal(GWGUI.Emulation.EmulationMachineState.Running, first.State);
        Assert.Equal(GWGUI.Emulation.EmulationMachineState.Running, second.State);
        await first.PauseAsync();
        Assert.Equal(GWGUI.Emulation.EmulationMachineState.Paused, first.State);
        Assert.Equal(GWGUI.Emulation.EmulationMachineState.Running, second.State);
        await first.EjectFloppyAsync();
        var replacement = Path.Combine(repository, "image_test", "validated_images", "Commodore", "Amiga",
            "3.5 pouces DD - AmigaDOS OFS", "Boot-DD-OFS.adf");
        await first.InsertFloppyAsync(replacement);
        var statePath = Path.Combine(sessions, "first.state");
        await first.SaveStateAsync(statePath);
        Assert.True(new FileInfo(statePath).Length > 1024);
        await first.LoadStateAsync(statePath);
        await first.ResumeAsync();
        await first.StopAsync();
        await second.StopAsync();
        Assert.Equal(GWGUI.Emulation.EmulationMachineState.Stopped, first.State);
        Assert.Equal(GWGUI.Emulation.EmulationMachineState.Stopped, second.State);
    }

    [Fact]
    public async Task Machine_ForwardsNativeAudioToConfiguredOutput()
    {
        var repository = FindRepositoryRoot();
        var output = new RecordingAudioOutput();
        var engine = new AmigaEngine(Path.Combine(Path.GetTempPath(), "GWGUI-Amiga-Tests", Guid.NewGuid().ToString("N")),
            Path.Combine(repository, "artifacts", "ppua", "puae_libretro.dll"), () => output);
        await using var machine = engine.CreateAmigaMachine(AmigaMachineConfiguration.A500(
            Path.Combine(repository, "image_test", "Roms", "Bios", "Kickstart 1.3.rom")));
        await machine.StartAsync();
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        while (output.SamplesWritten == 0) await Task.Delay(20, cancellation.Token);
        await machine.StopAsync();
        Assert.InRange(output.SampleRate, 22050, 96000);
        Assert.True(output.SamplesWritten > 0);
        Assert.True(output.WasStopped);
    }

    private static async Task WaitForFrame(IAmigaMachine machine, TimeSpan timeout)
    {
        using var cancellation = new CancellationTokenSource(timeout);
        while (machine.LatestVideoFrame is null)
            await Task.Delay(20, cancellation.Token);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "GWGUI.sln")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException("GWGUI repository root not found.");
    }

    private static void SaveFrame(GWGUI.Emulation.VideoFrame frame, string path)
    {
        var format = frame.PixelFormat == GWGUI.Emulation.EmulationPixelFormat.Rgb565 ? PixelFormats.Bgr565 : PixelFormats.Bgr32;
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
