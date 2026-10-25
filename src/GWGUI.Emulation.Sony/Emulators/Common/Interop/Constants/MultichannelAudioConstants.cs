namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;

internal static class MultichannelAudioConstants
{
    internal const uint GetBatchCallback = 94 | ExternalCoreApiConstants.ExperimentalCommandFlag;
    internal const int MaximumFramesPerBatch = 262144;
    internal const uint FrontLeft = 0x001;
    internal const uint FrontRight = 0x002;
    internal const uint FrontCenter = 0x004;
    internal const uint LowFrequency = 0x008;
    internal const uint BackLeft = 0x010;
    internal const uint BackRight = 0x020;
    internal const uint SideLeft = 0x200;
    internal const uint SideRight = 0x400;
}
