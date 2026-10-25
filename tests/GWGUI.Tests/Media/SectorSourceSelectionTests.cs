using GWGUI.MediaEngine.Images.Conversion;
using GWGUI.MediaEngine.Images.Models.Sectors;

namespace GWGUI.Tests.Media;

public sealed class SectorSourceSelectionTests
{
    [Theory]
    [InlineData("source.scp", true)]
    [InlineData("source.SCP", true)]
    [InlineData("source.img", false)]
    [InlineData("source.scp.bak", false)]
    [InlineData("source", false)]
    public async Task UsesOnlyTheSelectedReaderAndKeepsItsTask(string path, bool isScp)
    {
        var scp = Task.FromResult(Image(1));
        var image = Task.FromResult(Image(2));
        var calls = new List<string>();

        var selected = MediaConversionService.ReadSectorSourceAsync(path,
            () => { calls.Add("scp"); return scp; },
            () => { calls.Add("image"); return image; });

        var expected = isScp ? scp : image;
        Assert.Same(expected, selected);
        Assert.Equal(isScp ? "scp" : "image", Assert.Single(calls));
        Assert.Same(await expected, await selected);
    }

    [Fact]
    public async Task KeepsAnUnfinishedReaderTaskUntilItsResultIsAvailable()
    {
        var image = Image(3);
        var gate = new TaskCompletionSource<SectorImage>(TaskCreationOptions.RunContinuationsAsynchronously);
        Task<SectorImage>? selected = null;
        try
        {
            selected = MediaConversionService.ReadSectorSourceAsync("source.scp",
                () => gate.Task, () => throw new InvalidOperationException("Wrong reader."));
            Assert.Same(gate.Task, selected);
            Assert.False(selected.IsCompleted);
            gate.SetResult(image);
            Assert.Same(image, await selected);
        }
        finally
        {
            gate.TrySetResult(image);
            if (selected is not null) await selected;
        }
    }

    [Theory]
    [InlineData("source.scp")]
    [InlineData("source.img")]
    public async Task KeepsTheReaderFailureWithoutTryingTheOtherReader(string path)
    {
        var expected = new InvalidOperationException("Reader failed.");
        Task<SectorImage> Fail() => Task.FromException<SectorImage>(expected);
        Task<SectorImage> WrongReader() => throw new InvalidOperationException("Wrong reader.");
        var selected = path.EndsWith(".scp", StringComparison.Ordinal)
            ? MediaConversionService.ReadSectorSourceAsync(path, Fail, WrongReader)
            : MediaConversionService.ReadSectorSourceAsync(path, WrongReader, Fail);

        var observed = await Assert.ThrowsAsync<InvalidOperationException>(() => selected);
        Assert.Same(expected, observed);
    }

    [Theory]
    [InlineData("source.scp")]
    [InlineData("source.img")]
    public void KeepsSynchronousReaderFailureWithoutFallback(string path)
    {
        var expected = new InvalidOperationException("Reader failed before returning a task.");
        Task<SectorImage> Fail() => throw expected;
        Task<SectorImage> WrongReader() => throw new InvalidOperationException("Wrong reader.");
        var observed = Assert.Throws<InvalidOperationException>(() =>
        {
            _ = path.EndsWith(".scp", StringComparison.Ordinal)
                ? MediaConversionService.ReadSectorSourceAsync(path, Fail, WrongReader)
                : MediaConversionService.ReadSectorSourceAsync(path, WrongReader, Fail);
        });
        Assert.Same(expected, observed);
    }

    [Theory]
    [InlineData("source.scp")]
    [InlineData("source.img")]
    public async Task KeepsTheCanceledReaderTaskAndItsToken(string path)
    {
        var token = new CancellationToken(canceled: true);
        var canceled = Task.FromCanceled<SectorImage>(token);
        Task<SectorImage> ReadCanceled() => canceled;
        Task<SectorImage> WrongReader() => throw new InvalidOperationException("Wrong reader.");
        var selected = path.EndsWith(".scp", StringComparison.Ordinal)
            ? MediaConversionService.ReadSectorSourceAsync(path, ReadCanceled, WrongReader)
            : MediaConversionService.ReadSectorSourceAsync(path, WrongReader, ReadCanceled);

        Assert.Same(canceled, selected);
        var observed = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => selected);
        Assert.Equal(token, observed.CancellationToken);
    }

    private static SectorImage Image(byte value) => new("test.source", 1, 1, 1, 1,
        [new SectorBlock(0, new SectorAddress(0, 0, 1), [value])]);
}

