using System.IO;
using GWGUI.Emulation.Amiga;
using GWGUI.Emulation.Amiga.Cores;

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
            for (var frame = 0; frame < 200; frame++) core.RunFrame();

            var video = Assert.IsType<GWGUI.Emulation.VideoFrame>(core.LatestVideoFrame);
            Assert.InRange(video.Width, 320, 1920);
            Assert.InRange(video.Height, 200, 1080);
            Assert.True(video.Pixels.Length >= video.Pitch * video.Height);
            Assert.True(video.Sequence >= 100);
            Assert.True(video.Pixels.Span.IndexOfAnyExcept((byte)0) >= 0);
            Assert.InRange(core.FramesPerSecond, 49, 61);
            Assert.InRange(core.SampleRate, 22050, 96000);
            Assert.NotNull(core.LatestAudioChunk);
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
        await first.ResumeAsync();
        await first.StopAsync();
        await second.StopAsync();
        Assert.Equal(GWGUI.Emulation.EmulationMachineState.Stopped, first.State);
        Assert.Equal(GWGUI.Emulation.EmulationMachineState.Stopped, second.State);
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
}
