using GWGUI.Emulation.Contracts;
namespace GWGUI.Emulation.Interfaces;

/// <summary>Commandes de cœur exécutées par la file de commandes de la machine.</summary>
/// <remarks>Le module décrit les capacités réelles de son cœur ; ce contrat ne rend pas une opération prise en charge lorsqu’elle ne l’est pas.</remarks>
public interface IEmulationCoreStateCommands
{
    /// <summary>Lit le statut détaillé du jeu de disques courant sans modifier son montage.</summary>
    /// <returns>Instantané géré des images, de leur sélection et de leur éjection ; aucun pointeur natif n'est conservé.</returns>
    DiskStatus GetDiskStatus();

    /// <summary>Demande la réinitialisation logicielle du cœur selon son entrée native de reset.</summary>
    /// <remarks>Common conserve les combinaisons de touches des profils et les règles imposant une recréation.
    /// Le backend possédant une seule entrée de reset réutilise son traitement natif existant.</remarks>
    void SoftReset();

    /// <summary>Réinitialise le cœur selon son comportement matériel.</summary>
    void HardReset();
    /// <summary>Arrête le contenu avant la libération du cœur par son propriétaire.</summary>
    void Stop();
    /// <summary>Produit un état binaire possédé par l’appelant ; l’enveloppe de fichier appartient au SDK.</summary>
    byte[] SaveState();
    /// <summary>Restaure un état emprunté pendant cet appel, après validation de son enveloppe par la machine.</summary>
    void LoadState(ReadOnlySpan<byte> state);
    /// <summary>Applique une valeur d’option suivant les règles du cœur.</summary>
    void SetOption(string key, string value);
    /// <summary>Sélectionne une image dans le jeu de disques chargé, avec un index commençant à zéro.</summary>
    void SelectDisk(int index);
    /// <summary>Publie les changements du média suivant la politique existante du backend.</summary>
    /// <param name="slot">Lecteur logique concerné, avec sa catégorie et son indice complets.</param>
    /// <remarks>Les copies de session sont publiées explicitement. Les contenus en lecture seule et les écritures
    /// déjà effectuées directement conservent leur politique ; cette commande ne réinitialise pas le cœur.</remarks>
    void SaveMediaChanges(EmulationMediaSlot slot);
    /// <summary>Indique si le backend conserve des changements possibles nécessitant une publication explicite.</summary>
    /// <param name="slot">Lecteur logique dont la politique de publication doit être consultée.</param>
    /// <returns>True pour un média modifiable dont le backend conserve une copie à publier.</returns>
    bool HasUnsavedMediaChanges(EmulationMediaSlot slot);
}

