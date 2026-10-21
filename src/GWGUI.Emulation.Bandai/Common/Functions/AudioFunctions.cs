using GWGUI.Emulation;

namespace GWGUI.Emulation.Bandai.Common.Functions;

internal static class AudioFunctions
{

    internal static short[] SingleFrame(short left, short right) => EmulationSdk.AudioProcessing.SingleFrame(left, right);
}

internal static class AudioOutputFunctions
{
}
