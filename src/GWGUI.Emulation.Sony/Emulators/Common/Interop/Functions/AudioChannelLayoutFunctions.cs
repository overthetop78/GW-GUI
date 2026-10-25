using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;

namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;

internal static class AudioChannelLayoutFunctions
{
    private static readonly (uint Native, EmulationAudioChannelLayout Common)[] Speakers =
    [
        (MultichannelAudioConstants.FrontLeft, EmulationAudioChannelLayout.FrontLeft),
        (MultichannelAudioConstants.FrontRight, EmulationAudioChannelLayout.FrontRight),
        (MultichannelAudioConstants.FrontCenter, EmulationAudioChannelLayout.FrontCenter),
        (MultichannelAudioConstants.LowFrequency, EmulationAudioChannelLayout.LowFrequency),
        (MultichannelAudioConstants.BackLeft, EmulationAudioChannelLayout.BackLeft),
        (MultichannelAudioConstants.BackRight, EmulationAudioChannelLayout.BackRight),
        (MultichannelAudioConstants.SideLeft, EmulationAudioChannelLayout.SideLeft),
        (MultichannelAudioConstants.SideRight, EmulationAudioChannelLayout.SideRight)
    ];

    internal static bool TryFromNative(uint mask, uint channels, out EmulationAudioChannelLayout layout)
    {
        layout = EmulationAudioChannelLayout.None;
        var remaining = mask;
        foreach (var speaker in Speakers)
            if ((mask & speaker.Native) != 0)
            {
                layout |= speaker.Common;
                remaining &= ~speaker.Native;
            }
        return remaining == 0 && channels is >= 1 and <= 8 && System.Numerics.BitOperations.PopCount((uint)layout) == channels;
    }
}
