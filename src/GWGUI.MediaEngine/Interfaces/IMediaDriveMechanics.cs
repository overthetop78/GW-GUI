using GWGUI.MediaEngine.Contracts;

namespace GWGUI.MediaEngine.Interfaces;

/// <summary>Mécanique commune d’un lecteur ; l’appelant possède son horloge et sa sortie audio.</summary>
public interface IMediaDriveMechanics
{
    /// <summary>État physique courant, y compris la position des têtes.</summary>
    MediaDriveState State { get; }

    /// <summary>Indique si une rotation ou un événement mécanique produit encore du son.</summary>
    bool HasPendingAudio { get; }

    /// <summary>Indique qu’une opération de lecture ou d’écriture possède encore le lecteur.</summary>
    bool IsInUse { get; }

    /// <summary>Débute une opération ; les accès imbriqués partagent la même rotation.</summary>
    void BeginOperation();

    /// <summary>Termine une opération et arrête le moteur après le dernier accès possédé.</summary>
    void EndOperation();

    /// <summary>Applique le profil, le volume en pourcentage et l’activation des bruitages.</summary>
    void Configure(string? profile, double volumePercent, bool soundEnabled);

    /// <summary>Signale l’insertion du support sans déplacer les têtes.</summary>
    void Insert();

    /// <summary>Arrête le moteur et signale l’éjection du support.</summary>
    void Eject();

    /// <summary>Commande le moteur ; ses transitions déclenchent leurs enregistrements.</summary>
    void SetMotor(bool running);

    /// <summary>Actualise les transferts en cours déclarés par le contrôleur matériel.</summary>
    void SetActivity(bool reading, bool writing);

    /// <summary>Déplace les têtes et sélectionne la face sans confondre les deux opérations.</summary>
    void Seek(MediaDrivePosition position);

    /// <summary>Actualise la position atteinte par un transfert continu, sans nouveau déplacement commandé.</summary>
    void Advance(MediaDrivePosition position);

    /// <summary>Signale une lecture à la position physique demandée.</summary>
    void BeginRead(MediaDrivePosition position);

    /// <summary>Signale une écriture à la position physique demandée.</summary>
    void BeginWrite(MediaDrivePosition position);

    /// <summary>Termine l’accès aux données sans arrêter automatiquement la rotation.</summary>
    void EndAccess();

    /// <summary>Produit le prochain fragment mono PCM signé 16 bits à la fréquence demandée.</summary>
    short[] RenderAudio(int sampleRate, int sampleCount);

    /// <summary>Réinitialise l’état et retire les événements lorsque le propriétaire libère le lecteur.</summary>
    void Reset();
}
