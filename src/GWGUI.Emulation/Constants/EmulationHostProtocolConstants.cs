namespace GWGUI.Emulation.Constants;

internal static class EmulationHostProtocolConstants
{
    internal const byte SoftResetCommand = 16;

    internal const byte HardResetCommand = 3;
    internal const byte StopCommand = 4;
    internal const byte InsertMediaCommand = 5;
    internal const byte EjectMediaCommand = 6;
    internal const byte SaveStateCommand = 7;
    internal const byte LoadStateCommand = 8;
    internal const byte SetOptionCommand = 9;
    internal const byte SelectDiskCommand = 10;
    internal const byte GetDiskStatusCommand = 13;
    internal const byte SetControllerPortDeviceCommand = 15;

    internal const int EmptyPointerDelta = 0;
    internal const string InvalidVideoFrameLengthFormat = "The {0} video frame requires {1} bytes; the shared slot supports {2}.";
    internal const string InvalidSharedVideoMetadata = "The host sent invalid shared video metadata.";
    internal const string SharedVideoEndedEarly = "The shared video frame ended early.";
    internal const int MaximumBlobLength = 512 * 1024 * 1024;
    internal const int VideoSlotCapacity = 128 * 1024 * 1024;
    internal const int VideoSlotCount = 2;
    internal const long VideoMapCapacity = (long)VideoSlotCapacity * VideoSlotCount;
    internal const int MaximumInputKeyCount = 512;
    internal const int MaximumInputControllerCount = 10;
    internal const int MaximumControllerFeedbackCount = MaximumInputControllerCount * 2;
    internal const string InvalidControllerFeedbackCount = "Invalid controller feedback count.";
    internal const string InvalidControllerFeedbackState = "Invalid controller feedback state.";
    internal const int MaximumControllerControlCount = 4096;
    internal const int MaximumAudioChunkCount = 1024;
    internal const int MaximumLedStateCount = 256;
    internal const int BytesPerPcmSample = sizeof(short);
    internal const string InvalidBinaryPayloadLengthFormat = "The {0} host sent invalid binary payload length {1}.";
    internal const string BinaryPayloadEndedEarlyFormat = "The {0} host binary payload ended early.";
    internal const string InvalidKeyCountFormat = "The {0} host input contains an invalid key count.";
    internal const string InvalidControllerCountFormat = "The {0} host input contains an invalid controller count.";
    internal const string InvalidControllerControlCountFormat = "The {0} host input contains an invalid controller control count.";
    internal const string InvalidVideoFrameFormat = "The {0} host sent an invalid video frame.";
    internal const string InvalidSharedVideoMetadataFormat = "The {0} host sent invalid shared video metadata.";
    internal const string SharedVideoEndedEarlyFormat = "The {0} shared video frame ended early.";
    internal const string InvalidAudioChunkCountFormat = "The {0} host sent an invalid audio chunk count.";
    internal const string InvalidPcmPayloadFormat = "The {0} host sent an invalid PCM payload.";
    internal const string InvalidLedStateCountFormat = "Invalid {0} LED state count {1}.";
}
