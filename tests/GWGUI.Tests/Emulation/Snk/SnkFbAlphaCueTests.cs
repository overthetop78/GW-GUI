using GWGUI.Emulation.Snk.Emulators.Common.Exceptions;
using GWGUI.Emulation.Snk.Emulators.FbAlpha.Functions;
using GWGUI.MediaEngine.Images.Formats.Optical.BinCue;

namespace GWGUI.Tests.Emulation.Snk;

public sealed class SnkFbAlphaCueTests
{
    [Theory]
    [InlineData(99, true)]
    [InlineData(100, false)]
    public void NativeTableKeepsOneEntryForLeadout(int trackCount, bool supported)
    {
        var lines = new List<string> { "FILE \"disc.bin\" BINARY" };
        for (var track = 1; track <= trackCount; track++) lines.AddRange([$"  TRACK {track:00} AUDIO", $"    INDEX 01 {track:00}:00:00"]);
        var sheet = new CueSheetReader().Parse(lines);
        if (supported) CueRuntimeFunctions.Validate(sheet);
        else Assert.Throws<EmulationException>(() => CueRuntimeFunctions.Validate(sheet));
    }

    [Fact]
    public void RawDataAndAudioTracksInOneBinaryRemainSupported()
    {
        var sheet = new CueSheetReader().Parse(["FILE \"disc.bin\" BINARY", "  TRACK 01 MODE1/2352", "    INDEX 01 00:00:00", "  TRACK 02 AUDIO", "    INDEX 01 00:10:00"]);
        CueRuntimeFunctions.Validate(sheet);
    }

    [Theory]
    [InlineData("MODE1/2048")]
    [InlineData("MODE2/2336")]
    [InlineData("MODE2/2352")]
    public void NativeUnsupportedSectorModesAreRejectedBeforeLoading(string mode)
    {
        var sheet = new CueSheetReader().Parse(["FILE \"disc.bin\" BINARY", $"  TRACK 01 {mode}", "    INDEX 01 00:00:00"]);
        Assert.Throws<EmulationException>(() => CueRuntimeFunctions.Validate(sheet));
    }

    [Fact]
    public void SeparateTrackFilesAreRejectedBeforeLoading()
    {
        var sheet = new CueSheetReader().Parse(["FILE \"data.bin\" BINARY", "  TRACK 01 MODE1/2352", "    INDEX 01 00:00:00", "FILE \"audio.bin\" BINARY", "  TRACK 02 AUDIO", "    INDEX 01 00:00:00"]);
        Assert.Throws<EmulationException>(() => CueRuntimeFunctions.Validate(sheet));
    }

    [Fact]
    public void WaveContainerIsRejectedBeforeLoading()
    {
        var sheet = new CueSheetReader().Parse(["FILE \"audio.wav\" WAVE", "  TRACK 01 AUDIO", "    INDEX 01 00:00:00"]);
        Assert.Throws<EmulationException>(() => CueRuntimeFunctions.Validate(sheet));
    }
}
