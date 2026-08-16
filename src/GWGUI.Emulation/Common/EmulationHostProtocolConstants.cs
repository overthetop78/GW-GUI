namespace GWGUI.Emulation.Common;

internal static class EmulationHostProtocolConstants
{
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
