namespace GWGUI.Emulation.Functions;

/// <summary>Publication commune des sorties produites par un cœur d’émulation.</summary>
internal static class EmulationFrameOutputFunctions
{
    /// <summary>Publie une nouvelle image, puis vide la file audio dans son ordre de production.</summary>
    /// <remarks>La séquence vidéo évite de publier deux fois la même image. La machine et les données sont empruntées.</remarks>
    public static void PublishOutputs<TMachine>(TMachine machine, ref long videoSequence,
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
