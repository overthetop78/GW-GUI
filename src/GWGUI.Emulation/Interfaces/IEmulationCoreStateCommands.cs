namespace GWGUI.Emulation.Interfaces;

/// <summary>Commandes de cœur exécutées par la file de commandes de la machine.</summary>
/// <remarks>Le module décrit les capacités réelles de son cœur ; ce contrat ne rend pas une opération prise en charge lorsqu’elle ne l’est pas.</remarks>
public interface IEmulationCoreStateCommands
{
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
}

