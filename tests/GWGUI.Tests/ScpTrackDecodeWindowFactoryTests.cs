using System.IO;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Reconstruction.Scp;

namespace GWGUI.Tests;

public sealed class ScpTrackDecodeWindowFactoryTests
{
    [Fact]
    public void SingleRevolutionRemainsThePrimaryUnmodifiedView()
    {
        var revolution = new ScpRevolution(10, 2, [3, 7]);
        var track = new ScpTrack(0, 0, 0, [revolution]);

        var windows = ScpTrackDecodeWindowFactory.Create(track);

        var window = Assert.Single(windows);
        Assert.Same(revolution.Flux, window.Flux);
        Assert.Equal(1, window.Revolution);
        Assert.False(window.IsContinuous);
        Assert.Same(revolution.Flux, ScpTrackDecodeWindowFactory.Primary(track).Flux);
        Assert.Same(revolution, Assert.Single(track.Revolutions));
    }

    [Fact]
    public void MultipleRevolutionsExposeOneContinuousDecodeViewAndPreserveOriginals()
    {
        var first = new ScpRevolution(10, 2, [3, 7]);
        var second = new ScpRevolution(20, 2, [11, 9]);
        var third = new ScpRevolution(30, 1, [30]);
        var track = new ScpTrack(0, 0, 0, [first, second, third]);

        var windows = ScpTrackDecodeWindowFactory.Create(track);

        var window = Assert.Single(windows);
        Assert.Equal(0, window.Revolution);
        Assert.True(window.IsContinuous);
        Assert.Equal(60u, window.Flux.IndexTimeTicks);
        Assert.Equal([3u, 7u, 11u, 9u, 30u], window.Flux.FluxIntervals);
        Assert.True(ScpTrackDecodeWindowFactory.Primary(track).IsContinuous);
        Assert.Equal([first, second, third], track.Revolutions);
    }

    [Fact]
    public void EmptyTrackHasNoDecodeWindow()
    {
        var track = new ScpTrack(0, 0, 0, []);

        Assert.Empty(ScpTrackDecodeWindowFactory.Create(track));
        Assert.Throws<InvalidDataException>(() => ScpTrackDecodeWindowFactory.Primary(track));
    }
}
