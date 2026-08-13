namespace GWGUI.MediaEngine.Encoding;

/// <summary>Defines the time base shared by tracks produced by the internal encoders.</summary>
public static class EncodedTrackTiming
{
    /// <summary>Duration of one encoder tick in nanoseconds.</summary>
    public const uint TickNanoseconds = 25;
}
