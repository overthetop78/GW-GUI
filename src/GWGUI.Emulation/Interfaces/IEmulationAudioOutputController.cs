using GWGUI.Emulation.Enums;

namespace GWGUI.Emulation.Interfaces;

/// <summary>Contrat du contrôleur audio commun existant, utilisable par une famille indépendante.</summary>
/// <remarks>Une sortie reçue est empruntée ; une sortie créée par la fabrique appartient au contrôleur.
/// L’appelant possède le contrôleur et le détruit après la fin des écritures de sa machine.</remarks>
public interface IEmulationAudioOutputController : IDisposable
{
    /// <summary>État de coupure de la sortie, distinct du volume mémorisé.</summary>
    bool IsMuted { get; }
    /// <summary>Gain normalisé actuellement appliqué aux échantillons.</summary>
    float Volume { get; }
    /// <summary>Instantané du tampon physique, ou null s’il est absent ou inaccessible.</summary>
    /// <remarks>IsActive tient compte du démarrage, de l’arrêt, de la pause, de la coupure et de la disponibilité du contrôleur.</remarks>
    AudioOutputBufferStatus? BufferStatus { get; }
    /// <summary>Demande une latence minimale en millisecondes, mémorisée pour les prochains démarrages.</summary>
    /// <returns>False pour une valeur négative, un refus de la sortie ou une erreur ; true lorsque la demande est acceptée.</returns>
    bool TrySetMinimumLatency(int latencyMilliseconds);
    /// <summary>Démarre ou redémarre la sortie à la fréquence fournie en Hz.</summary>
    void Start(int sampleRate);
    /// <summary>Démarre la sortie avec la fréquence en Hz et la disposition de canaux annoncées par le cœur.</summary>
    /// <remarks>Une sortie multicanale reçoit la disposition complète ; une sortie simple accepte la stéréo.
    /// Le contrôleur applique sa récupération habituelle si la sortie ne prend pas en charge cette disposition.</remarks>
    void Start(int sampleRate, EmulationAudioChannelLayout channelLayout);
    /// <summary>Écrit le bloc emprunté avec la politique commune de volume et de reprise sur erreur.</summary>
    void Write(AudioChunk chunk);
    /// <summary>Coupe ou rétablit la sortie ; une coupure vide ses échantillons en attente.</summary>
    void SetMuted(bool muted);
    /// <summary>Borne et mémorise le gain de sortie.</summary>
    void SetVolume(float volume);
    /// <summary>Suspend et vide la sortie en conservant le contrôleur.</summary>
    void Pause();
    /// <summary>Réactive la sortie après une pause.</summary>
    void Resume();
    /// <summary>Vide les sons précédant une réinitialisation.</summary>
    void Reset();
    /// <summary>Remplace la fabrique empruntée et libère la sortie précédente suivant sa propriété.</summary>
    void ReplaceFactory(Func<IAudioOutput?>? factory);
    /// <summary>Arrête la sortie jusqu’au prochain Start ; Dispose libère ses ressources possédées.</summary>
    void Stop();
}
