namespace GWGUI.Emulation.Functions;

internal static class EmulationFrameOutputFunctions
{
    internal static void PublishOutputs<TMachine>(TMachine machine, ref long videoSequence,
        Func<TMachine, VideoFrame?> latestVideo, Func<TMachine, AudioChunk?> dequeueAudio,
        Action<TMachine, VideoFrame> publishVideo, Action<TMachine, AudioChunk> publishAudio)
    {
        if (latestVideo(machine) is { } video && video.Sequence != videoSequence)
        {
            videoSequence = video.Sequence;
            publishVideo(machine, video);
        }
        while (dequeueAudio(machine) is { } audio)
        {
            publishAudio(machine, audio);
        }
    }
}
