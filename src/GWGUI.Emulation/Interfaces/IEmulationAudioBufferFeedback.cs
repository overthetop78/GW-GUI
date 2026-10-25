using GWGUI.Emulation.Contracts;

namespace GWGUI.Emulation.Interfaces;

/// <summary>Échange le remplissage de la sortie audio et la latence minimale avec le cœur.</summary>
/// <remarks>Le propriétaire de la machine transmet un instantané avant chaque frame puis applique la demande
/// du cœur à sa sortie audio. Le cœur ne possède pas cette sortie ; il transmet les valeurs au callback natif
/// ou à son hôte séparé. Une sortie absente reste représentée par null.</remarks>
public interface IEmulationAudioBufferFeedback
{
    /// <summary>Latence minimale demandée par le cœur, en millisecondes ; zéro signifie aucune demande.</summary>
    int MinimumAudioLatencyMilliseconds { get; }
    /// <summary>Transmet le remplissage, la capacité, la fréquence et l’activité de la sortie empruntée.</summary>
    /// <param name="status">Instantané du tampon ; null lorsque la sortie ne fournit aucun statut.</param>
    void SetAudioBufferStatus(AudioOutputBufferStatus? status);
}