using GWGUI.Emulation;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Interfaces;

namespace GWGUI.Emulation.Thomson.Common.Services;

/// <summary>Relie la machine du module au contrôleur audio commun par son interface publique.</summary>
/// <param name="output">Sortie physique empruntée, ou null pour utiliser la fabrique.</param>
/// <param name="factory">Fabrique facultative ; les sorties qu’elle retourne appartiennent au contrôleur commun.</param>
/// <remarks>La machine possède ce relais et le libère dans son finally après l’arrêt de sa boucle.
/// Le traitement PCM, les verrous, la reprise après erreur et la propriété des sorties restent centralisés
/// dans Emulation. Ce relais n’ajoute ni tampon d’échantillons, ni thread, ni seconde sortie physique.</remarks>
internal sealed class AudioOutputController(IAudioOutput? output = null, Func<IAudioOutput?>? factory = null) : IDisposable
{
    /// <summary>Contrôleur possédé par ce relais, obtenu par l’API publique et conservé sous son interface.</summary>
    private readonly IEmulationAudioOutputController _controller = EmulationSdk.CreateAudioOutputController(output, factory);

    /// <summary>Indique la coupure du son ; le volume mémorisé reste indépendant de cette coupure.</summary>
    internal bool IsMuted => _controller.IsMuted;

    /// <summary>Gain normalisé entre zéro et un appliqué aux échantillons de sortie.</summary>
    internal float Volume => _controller.Volume;

    /// <summary>Instantané du tampon physique, ou null en l’absence de sortie accessible.</summary>
    /// <remarks>IsActive indique si le tampon participe actuellement à la lecture ; les compteurs restent
    /// ceux du contrat audio commun et ne sont pas déduits des compteurs de la machine.</remarks>
    internal AudioOutputBufferStatus? BufferStatus => _controller.BufferStatus;

    /// <summary>Applique une demande de latence minimale au contrôleur commun.</summary>
    /// <param name="latencyMilliseconds">Minimum en millisecondes, mémorisé également avant le démarrage ; zéro retire la demande.</param>
    /// <returns>True si la demande est acceptée ; false si la valeur est négative, refusée ou si la sortie échoue.</returns>
    internal bool TrySetMinimumLatency(int latencyMilliseconds) => _controller.TrySetMinimumLatency(latencyMilliseconds);

    /// <summary>Démarre la sortie ou adapte sa fréquence de lecture.</summary>
    /// <param name="sampleRate">Fréquence annoncée par le cœur, en hertz.</param>
    internal void Start(int sampleRate) => _controller.Start(sampleRate);

    /// <summary>Démarre une sortie dont le cœur annonce explicitement la disposition des canaux.</summary>
    /// <param name="sampleRate">Fréquence des frames audio en hertz.</param>
    /// <param name="channelLayout">Disposition décrivant les canaux de chaque frame entrelacée.</param>
    /// <remarks>Le contrôleur commun utilise IMultichannelAudioOutput pour une disposition multicanale.
    /// Une sortie simple reste utilisable en stéréo. Les blocs AudioChunk suivants portent eux aussi leur
    /// disposition ; un changement de format déclenche le redémarrage de la sortie par le même contrôleur.</remarks>
    internal void Start(int sampleRate, EmulationAudioChannelLayout channelLayout) => _controller.Start(sampleRate, channelLayout);

    /// <summary>Transmet un bloc PCM au traitement commun du volume et de la sortie.</summary>
    /// <param name="chunk">Bloc emprunté pendant l’appel, avec fréquence et échantillons entrelacés.</param>
    /// <remarks>Le contrôleur ne prend pas possession du bloc. Ses règles communes gèrent la pause,
    /// la coupure et une éventuelle reprise de la sortie en cas d’erreur d’écriture.</remarks>
    internal void Write(AudioChunk chunk) => _controller.Write(chunk);

    /// <summary>Coupe ou rétablit le son sans remplacer le volume configuré.</summary>
    /// <param name="muted">True coupe le son et vide les échantillons déjà en attente.</param>
    internal void SetMuted(bool muted) => _controller.SetMuted(muted);

    /// <summary>Mémorise le gain de sortie après son bornage par le contrôleur commun.</summary>
    /// <param name="volume">Gain normalisé ; les valeurs admises vont de zéro à un.</param>
    internal void SetVolume(float volume) => _controller.SetVolume(volume);

    /// <summary>Suspend et vide la sortie, tout en conservant le contrôleur pour une reprise.</summary>
    internal void Pause() => _controller.Pause();

    /// <summary>Réactive la sortie après une pause et permet sa récupération si elle était indisponible.</summary>
    internal void Resume() => _controller.Resume();

    /// <summary>Vide les échantillons précédant une réinitialisation du cœur.</summary>
    internal void Reset() => _controller.Reset();

    /// <summary>Remplace la fabrique et détache la sortie courante suivant sa propriété.</summary>
    /// <param name="factory">Fabrique de remplacement empruntée ; null retire la possibilité de créer une sortie.</param>
    /// <remarks>La sortie précédente est libérée si le contrôleur l’a créée. Une sortie reçue de l’appelant
    /// est seulement arrêtée et détachée ; sa destruction appartient toujours à cet appelant.</remarks>
    internal void ReplaceFactory(Func<IAudioOutput?>? factory) => _controller.ReplaceFactory(factory);

    /// <summary>Arrête la sortie et bloque les écritures jusqu’au prochain Start.</summary>
    internal void Stop() => _controller.Stop();

    /// <summary>Arrête puis libère le contrôleur possédé et les seules sorties dont il est propriétaire.</summary>
    /// <remarks>Le propriétaire appelle Dispose dans son finally, après la fin des écritures audio.
    /// Les chemins d’échec du contrôleur commun libèrent ses sorties possédées selon les mêmes règles.</remarks>
    public void Dispose() => _controller.Dispose();
}
