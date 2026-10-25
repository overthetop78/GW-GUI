using CoreAudioBufferStatusCallback = GWGUI.Emulation.Interop.ExternalCoreApi.AudioBufferStatusCallback;
using CommonCoreHostFunctions = GWGUI.Emulation.Snk.Common.Functions.CoreHostFunctions;

namespace GWGUI.Emulation.Snk.Emulators.Common.Interop.Services;

internal sealed partial class ExternalHostCallbacks
{
    /// <summary>Callback natif emprunté, conservé uniquement pendant la session du cœur.</summary>
    private CoreAudioBufferStatusCallback? _audioBufferStatusCallback;
    private int _minimumAudioLatencyMilliseconds;
    /// <summary>Instantané de la sortie physique transmis par Common avant le calcul de la frame.</summary>
    internal AudioOutputBufferStatus? OutputAudioStatus { get; set; }
    /// <summary>Latence minimale négociée avec le cœur, en millisecondes.</summary>
    internal int MinimumAudioLatencyMilliseconds => _minimumAudioLatencyMilliseconds;
    /// <summary>Notifie le délégué du cœur avec le calcul commun de remplissage et de sous-alimentation.</summary>
    internal void NotifyAudioBufferStatus() => CommonCoreHostFunctions.EnvironmentProcessing.PublishAudioBufferStatus(_audioBufferStatusCallback, OutputAudioStatus, FramesPerSecond);
    /// <summary>Retire les références empruntées avant le déchargement du code natif.</summary>
    private void ClearAudioBufferFeedback()
    {
        _audioBufferStatusCallback = null;
        OutputAudioStatus = null;
        _minimumAudioLatencyMilliseconds = 0;
    }
}
