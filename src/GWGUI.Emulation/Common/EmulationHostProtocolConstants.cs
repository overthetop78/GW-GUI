namespace GWGUI.Emulation.Common;

internal static class EmulationHostProtocolConstants
{
    internal const int EmptyPointerDelta = 0;
    internal const string InvalidVideoFrameLengthFormat = "The {0} video frame requires {1} bytes; the shared slot supports {2}.";
    internal const string InvalidSharedVideoMetadata = "The host sent invalid shared video metadata.";
    internal const string SharedVideoEndedEarly = "The shared video frame ended early.";
    internal const int MaximumBlobLength = 512 * 1024 * 1024;
    internal const int VideoSlotCapacity = 32 * 1024 * 1024;
    internal const int VideoSlotCount = 2;
    internal const long VideoMapCapacity = (long)VideoSlotCapacity * VideoSlotCount;
    internal const int MaximumInputKeyCount = 512;
    internal const int MaximumInputControllerCount = 8;
    internal const int MaximumAudioChunkCount = 1024;
    internal const int MaximumLedStateCount = 256;
    internal const int BytesPerPcmSample = sizeof(short);
}
