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

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "GWGUI.sln")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException("GWGUI repository root not found.");
    }
}
