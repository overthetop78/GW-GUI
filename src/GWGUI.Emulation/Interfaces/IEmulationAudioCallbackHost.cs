using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Interop;

namespace GWGUI.Emulation.Interfaces;

/// <summary>Propriétaire des callbacks audio stéréo et de leur file de blocs copiés.</summary>
/// <remarks>Le cœur emprunte les délégués, conservés vivants par ce propriétaire. La session arrête le cœur
/// avant Dispose dans son finally. Les buffers natifs des callbacks sont empruntés pendant leur copie.</remarks>
public interface IEmulationAudioCallbackHost : IDisposable
{
    /// <summary>Callback recevant une frame stéréo de deux échantillons signés sur seize bits.</summary>
    ExternalCoreApi.AudioSampleCallback SampleCallback { get; }
    /// <summary>Callback copiant un bloc stéréo emprunté et renvoyant le nombre de frames consommées.</summary>
    ExternalCoreApi.AudioBatchCallback BatchCallback { get; }
    /// <summary>Fréquence des prochains blocs publiés, en Hz, annoncée par le cœur.</summary>
    int SampleRate { get; set; }
    /// <summary>Dernier bloc publié, ou null avant réception et après Dispose.</summary>
    AudioChunk? LatestChunk { get; }
    /// <summary>Première erreur interceptée par les callbacks, sans propagation à travers la frontière native.</summary>
    Exception? Failure { get; }
    /// <summary>Nombre de frames stéréo encore présentes dans la file.</summary>
    int BufferedFrames { get; }
    /// <summary>Nombre de blocs anciens retirés pour respecter la limite de mise en tampon.</summary>
    long OverrunCount { get; }
    /// <summary>Compteur de sous-alimentation conservé par l’implémentation ; zéro pour cette file productrice.</summary>
    long UnderrunCount { get; }
    /// <summary>Retire le plus ancien bloc et actualise le nombre de frames ; false si la file est vide.</summary>
    bool TryDequeue(out AudioChunk? chunk);
}
