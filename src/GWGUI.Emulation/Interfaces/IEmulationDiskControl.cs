using GWGUI.Emulation.Contracts;

namespace GWGUI.Emulation.Interfaces;

/// <summary>Contrôle des images de disque d’un cœur sous le contrat commun, sans accès à son implémentation SDK.</summary>
/// <remarks>Les callbacks capturés et leur code restent empruntés au cœur. La session cesse les appels
/// avant Clear et avant le déchargement du cœur. Les tables reçues sont empruntées pendant la capture.
/// Le contenu préparé pour une insertion est possédé par cette opération et libéré dans son nettoyage ;
/// la fabrique, sa politique et ses validations restent empruntées au module.</remarks>
public interface IEmulationDiskControl
{
    /// <summary>Disponibilité du callback obligatoire d’éjection, selon la règle de capture existante.</summary>
    bool IsAvailable { get; }
    /// <summary>Nombre d’images annoncé par le cœur, avec la conversion contrôlée existante ; zéro si indisponible.</summary>
    int ImageCount { get; }
    /// <summary>Index courant, ou moins un pour une sélection absente ou un contrôle indisponible.</summary>
    int CurrentIndex { get; }
    /// <summary>État d’éjection annoncé par le cœur ; true lorsque le contrôle est indisponible.</summary>
    bool IsEjected { get; }
    /// <summary>Capture la table native de base empruntée et valide ses callbacks dans leur ordre existant.</summary>
    void Capture(nint data);
    /// <summary>Capture la table native étendue et ses callbacks facultatifs de chemin et de libellé.</summary>
    void CaptureExtended(nint data);
    /// <summary>Détache les seuls délégués empruntés, sans détruire le cœur ni sa bibliothèque.</summary>
    void Clear();
    /// <summary>Sélectionne une image et restaure l’état précédent si une commande échoue.</summary>
    void Select(int index);
    /// <summary>Copie le chemin annoncé pour une image, ou null si son callback est absent ou refuse la requête.</summary>
    string? GetPath(int index);
    /// <summary>Copie le libellé annoncé pour une image, ou null si son callback est absent ou refuse la requête.</summary>
    string? GetLabel(int index);
    /// <summary>Construit le statut commun avec les mêmes lectures de nombre, d’index et d’éjection.</summary>
    DiskStatus GetStatus();
    /// <summary>Insère un contenu avec le chargement et les règles de restauration choisis à la construction.</summary>
    void Insert(string path);
    /// <summary>Insère à un emplacement précis en créant les emplacements manquants avant de charger le contenu.</summary>
    /// <param name="index">Emplacement demandé par le module.</param>
    /// <param name="path">Chemin validé selon la politique du contrôle.</param>
    /// <param name="firstImageIndex">Index minimal conservé depuis les règles du module.</param>
    /// <param name="createContent">Fabrique empruntée ; le contenu créé est libéré après remplacement ou échec.</param>
    /// <remarks>Le traitement reprend l’insertion à emplacement existante : sélection avant chargement,
    /// remplacement, fermeture du plateau puis nettoyage. Un échec réinsère le plateau si son état initial le demandait.</remarks>
    void InsertAt(int index, string path, int firstImageIndex, Func<string, IEmulationLoadedContent> createContent);
    /// <summary>Demande l’éjection avec le diagnostic du module si le cœur refuse.</summary>
    void Eject();
}
