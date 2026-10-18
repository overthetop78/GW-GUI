using System.IO;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Images.Reading.Recognition;

namespace GWGUI.Tests.Media;

public sealed class MediaReaderProbeTests
{
    private static readonly IReadOnlySet<string> Formats = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "format" };

    [Theory]
    [InlineData(null, true)]
    [InlineData("format", true)]
    [InlineData("FORMAT", true)]
    [InlineData("other", false)]
    [InlineData("", false)]
    public async Task ChecksTheRequestedFormatBeforeCallingTheValidator(string? requested, bool accepted)
    {
        var context = Context(requested);
        var token = new CancellationToken(canceled: true);
        var calls = 0;
        var result = await MediaRecognitionRegistry.CanReadAsync(context, Formats, (source, cancellation) =>
        {
            Assert.Same(context, source);
            Assert.Equal(token, cancellation);
            calls++;
            return new ValueTask<int>(1);
        }, token);
        Assert.Equal(accepted, result);
        Assert.Equal(accepted ? 1 : 0, calls);
    }

    [Fact]
    public async Task WaitsForTheValidatorBeforeAcceptingTheSource()
    {
        var gate = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        Task<bool>? pending = null;
        try
        {
            pending = MediaRecognitionRegistry.CanReadAsync(Context(), Formats,
                (_, _) => new ValueTask<int>(gate.Task), CancellationToken.None).AsTask();
            Assert.False(pending.IsCompleted);
            gate.SetResult(1);
            Assert.True(await pending);
        }
        finally
        {
            gate.TrySetResult(1);
            if (pending is not null) await pending;
        }
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task RejectsInvalidOrUnsupportedDataFromEitherFailurePath(bool unsupported, bool asynchronous)
    {
        Exception expected = unsupported ? new NotSupportedException("unsupported") : new InvalidDataException("invalid");
        var calls = 0;
        ValueTask<int> Validate(MediaRecognitionContext _, CancellationToken cancellation)
        {
            calls++;
            if (asynchronous) return new ValueTask<int>(Task.FromException<int>(expected));
            throw expected;
        }
        Assert.False(await MediaRecognitionRegistry.CanReadAsync(Context(), Formats, Validate, CancellationToken.None));
        Assert.Equal(1, calls);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PropagatesOtherReadErrors(bool asynchronous)
    {
        var expected = new IOException("read failed");
        ValueTask<int> Validate(MediaRecognitionContext _, CancellationToken cancellation)
        {
            if (asynchronous) return new ValueTask<int>(Task.FromException<int>(expected));
            throw expected;
        }
        var result = MediaRecognitionRegistry.CanReadAsync(Context(), Formats, Validate, CancellationToken.None);
        Assert.Same(expected, await Assert.ThrowsAsync<IOException>(() => result.AsTask()));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PropagatesCancellationAndItsToken(bool asynchronous)
    {
        var token = new CancellationToken(canceled: true);
        ValueTask<int> Validate(MediaRecognitionContext _, CancellationToken cancellation)
        {
            Assert.Equal(token, cancellation);
            if (asynchronous) return new ValueTask<int>(Task.FromCanceled<int>(cancellation));
            throw new OperationCanceledException(cancellation);
        }
        var result = MediaRecognitionRegistry.CanReadAsync(Context(), Formats, Validate, token);
        var observed = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => result.AsTask());
        Assert.Equal(token, observed.CancellationToken);
    }

    [Fact]
    public async Task KeepsContextFailureInTheReturnedAsynchronousResult()
    {
        var result = MediaRecognitionRegistry.CanReadAsync(null!, Formats,
            static (_, _) => new ValueTask<int>(1), CancellationToken.None);
        await Assert.ThrowsAsync<NullReferenceException>(() => result.AsTask());
    }

    private static MediaRecognitionContext Context(string? requested = null) =>
        new(new MediaSourceDescriptor("memory.disk", [], KnownLength: 0, RequestedFormatId: requested));
}
