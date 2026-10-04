namespace GWGUI.Emulation.Nec.Common.Machines.Common.Constants;

internal static partial class SettingsDescriptionFunctionsConstants
{
    internal const int TwoColumns = 2;
    internal const int OneColumn = 1;
    internal const int KibPerMib = 1024;
    internal const int FirstChoice = 0;
    internal const int FirstAudioLatency = 20;
    internal const int SecondAudioLatency = 35;
    internal const int ThirdAudioLatency = 50;
    internal const int FourthAudioLatency = 75;
    internal const int FifthAudioLatency = 100;
    internal const int SixthAudioLatency = 150;
    internal const int SeventhAudioLatency = 250;
    internal const int MouseSensitivityDivisor = 4;
    internal static readonly int[] AudioLatencies =
    [
        FirstAudioLatency, SecondAudioLatency, ThirdAudioLatency,
        FourthAudioLatency, FifthAudioLatency, SixthAudioLatency,
        SeventhAudioLatency
    ];
}
