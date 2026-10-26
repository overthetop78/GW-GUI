namespace GWGUI.Emulation.Interfaces;

/// <summary>Mécanique commune d’un lecteur, sans sortie audio ni ressources natives possédées.</summary>
public interface IEmulationDriveMechanics
{
    /// <summary>Indique si le lecteur a encore une rotation ou un événement sonore à rendre.</summary>
    bool HasPendingAudio { get; }
    /// <summary>Applique le profil de lecteur, le volume en pourcentage et l’activation.</summary>
    void Configure(string? profile, double volumePercent, bool enabled);
    /// <summary>Signale une insertion dans le lecteur possédé par la machine.</summary>
    void Insert();
    /// <summary>Signale une éjection et arrête la rotation du support.</summary>
    void Eject();
    /// <summary>Commande la rotation depuis le contrôleur matériel.</summary>
    void SetMotor(bool running);
    /// <summary>Signale les transferts en cours du contrôleur.</summary>
    void SetActivity(bool reading, bool writing);
    /// <summary>Transmet une position physique ou, si elle est inconnue, une adresse logique.</summary>
    void Seek(int cylinder, int head, int sector = -1, long logicalBlock = -1);
    /// <summary>Produit le fragment mono PCM signé 16 bits demandé par l’horloge de l’émulateur.</summary>
    short[] RenderAudio(int sampleRate, int sampleCount);
    /// <summary>Libère les états et sons en attente lorsque le propriétaire détruit son lecteur.</summary>
    void Reset();
}
