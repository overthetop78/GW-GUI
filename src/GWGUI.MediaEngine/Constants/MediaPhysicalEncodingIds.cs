namespace GWGUI.MediaEngine.Constants;

/// <summary>Defines versioned encodings used inside neutral physical media data units.</summary>
public static class MediaPhysicalEncodingIds
{
    public const string FluxTrackV1 = "flux-track-le32-v1";
    public const uint FluxTrackV1Magic = 0x31465747;
    public const int FluxTrackV1HeaderSize = 8;
    public const int FluxTrackV1RevolutionHeaderSize = 8;
    public const int UInt32Size = 4;
    public const int MaximumFluxRevolutions = byte.MaxValue;
    public const int MaximumFluxIntervalsPerRevolution = 16_777_216;
}
