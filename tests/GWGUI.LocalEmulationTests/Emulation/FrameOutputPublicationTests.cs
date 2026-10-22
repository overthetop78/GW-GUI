using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Functions;

namespace GWGUI.Tests.Emulation;

public sealed class FrameOutputPublicationTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingOrRepeatedVideoStillDrainsAudio(bool repeated)
    {
        var state = new State { Video = repeated ? Frame(4) : null };
        var chunk = Chunk(1);
        state.Audio.Enqueue(chunk);
        long sequence = 4;
        Publish(state, ref sequence);
        Assert.Equal(4, sequence);
        Assert.Empty(state.Videos);
        Assert.Same(chunk, Assert.Single(state.Chunks));
    }

    [Fact]
    public void SequenceChangesBeforeVideoAndAudioFollowsVideo()
    {
        var state = new State { Video = Frame(5) };
        state.Audio.Enqueue(Chunk(1));
        state.Audio.Enqueue(Chunk(2));
        long sequence = 4;
        state.OnVideo = _ => Assert.Equal(5, sequence);
        Publish(state, ref sequence);
        Assert.Equal(["video:5", "audio:1", "audio:2"], state.Events);
        Assert.Equal(5, sequence);
    }

    [Fact]
    public void NullAudioStopsDrainWithoutConsumingLaterChunk()
    {
        var state = new State();
        state.Audio.Enqueue(null);
        var remaining = Chunk(2);
        state.Audio.Enqueue(remaining);
        long sequence = 0;
        Publish(state, ref sequence);
        Assert.Empty(state.Chunks);
        Assert.Same(remaining, Assert.Single(state.Audio));
    }

    [Fact]
    public void NotificationsObserveCurrentOwnerAndCurrentSubscribers()
    {
        var state = new State { Video = Frame(1) };
        var oldQueue = state.Audio;
        oldQueue.Enqueue(Chunk(99));
        state.OnVideo = current =>
        {
            current.Audio = new Queue<AudioChunk?>([Chunk(2), Chunk(3)]);
            current.OnAudio = owner =>
            {
                owner.Events.Add("first");
                owner.OnAudio = later => later.Events.Add("second");
            };
        };
        long sequence = 0;
        Publish(state, ref sequence);
        Assert.Equal(["video:1", "audio:2", "first", "audio:3", "second"], state.Events);
        Assert.Equal(99, Assert.Single(oldQueue)!.Sequence);
    }

    [Fact]
    public void VideoFailureKeepsSequenceAndDoesNotReadAudio()
    {
        var failure = new InvalidOperationException();
        var state = new State { Video = Frame(2), OnVideo = _ => throw failure };
        state.Audio.Enqueue(Chunk(1));
        long sequence = 0;
        Assert.Same(failure, Assert.Throws<InvalidOperationException>(() => Publish(state, ref sequence)));
        Assert.Equal(2, sequence);
        Assert.Single(state.Audio);
        Assert.Empty(state.Chunks);
    }

    [Fact]
    public void AudioFailureStopsBeforeNextDequeue()
    {
        var failure = new InvalidOperationException();
        var state = new State { OnAudio = _ => throw failure };
        state.Audio.Enqueue(Chunk(1));
        state.Audio.Enqueue(Chunk(2));
        long sequence = 0;
        Assert.Same(failure, Assert.Throws<InvalidOperationException>(() => Publish(state, ref sequence)));
        Assert.Equal(1, Assert.Single(state.Chunks).Sequence);
        Assert.Equal(2, Assert.Single(state.Audio)!.Sequence);
    }

    private static void Publish(State state, ref long sequence)
        => EmulationFrameOutputFunctions.PublishOutputs(state, ref sequence,
            static owner => owner.Video,
            static owner => owner.Audio.TryDequeue(out var audio) ? audio : null,
            static (owner, video) =>
            {
                owner.Videos.Add(video);
                owner.Events.Add($"video:{video.Sequence}");
                owner.OnVideo?.Invoke(owner);
            },
            static (owner, audio) =>
            {
                owner.Chunks.Add(audio);
                owner.Events.Add($"audio:{audio.Sequence}");
                owner.OnAudio?.Invoke(owner);
            });

    private static VideoFrame Frame(long sequence)
        => new(ReadOnlyMemory<byte>.Empty, 0, 0, 0, EmulationPixelFormat.Xrgb8888, 1, sequence, TimeSpan.Zero);

    private static AudioChunk Chunk(long sequence)
        => new(ReadOnlyMemory<short>.Empty, 48000, 0, sequence, TimeSpan.Zero);

    private sealed class State
    {
        public VideoFrame? Video;
        public Queue<AudioChunk?> Audio = new();
        public readonly List<VideoFrame> Videos = new();
        public readonly List<AudioChunk> Chunks = new();
        public readonly List<string> Events = new();
        public Action<State>? OnVideo;
        public Action<State>? OnAudio;
    }
}
