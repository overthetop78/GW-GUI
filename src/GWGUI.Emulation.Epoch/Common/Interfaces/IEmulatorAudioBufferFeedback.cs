namespace GWGUI.Emulation.Epoch.Common.Interfaces;

/// <summary>Échange entre la sortie audio possédée par la machine et le cœur qui ajuste sa production.</summary>
/// <remarks>Un cœur implémente cette capacité lorsqu’il utilise le remplissage du tampon ou demande une latence minimale.
/// La machine transmet le statut avant le calcul de l’image et applique la latence annoncée après ce calcul.
/// Le statut est une description de valeurs ; il ne transfère ni la propriété ni la gestion de la sortie audio.</remarks>
internal interface IEmulatorAudioBufferFeedback
{
    /// <summary>Latence minimale demandée par le cœur, en millisecondes ; zéro indique aucune demande supplémentaire.</summary>
    int MinimumAudioLatencyMilliseconds { get; }
    /// <summary>Transmet l’état courant du tampon de sortie au cœur.</summary>
    /// <param name="status">Fréquence en Hz, remplissage et capacité en frames audio, activité ; null si le statut est indisponible.</param>
    /// <remarks>Le cœur conserve ou transmet cet instantané à son callback. L’hôte reste propriétaire de la sortie physique.</remarks>
    void SetAudioBufferStatus(AudioOutputBufferStatus? status);
}
