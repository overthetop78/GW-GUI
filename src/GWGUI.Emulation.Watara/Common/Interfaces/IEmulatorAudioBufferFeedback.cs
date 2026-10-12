namespace GWGUI.Emulation.Watara.Common.Interfaces;

internal interface IEmulatorAudioBufferFeedback
{
    int MinimumAudioLatencyMilliseconds { get; }
    void SetAudioBufferStatus(AudioOutputBufferStatus? status);
}