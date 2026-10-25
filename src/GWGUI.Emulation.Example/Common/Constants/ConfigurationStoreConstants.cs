namespace GWGUI.Emulation.Example.Common.Constants;


/// <summary>Conventions du magasin de configurations : noms, format du document et règles d’écriture.</summary>
/// <remarks>ConfigurationStore compose les chemins sous sa racine possédée et sérialise ses mutations.
/// Les noms de fichiers et les clés techniques sont invariants ; la version de schéma concerne le document
/// de configuration, indépendamment de la version de l’application ou du SDK.</remarks>
internal static class ConfigurationStoreConstants
{
    /// <summary>Nom du document enregistré dans le sous-dossier de chaque configuration de machine.</summary>
    internal const string MachineFileName = "machine.json";
    /// <summary>Format .NET d’un GUID sans séparateurs, utilisé pour nommer le dossier d’une configuration.</summary>
    internal const string MachineIdentifierFormat = "N";
    /// <summary>Séparateur canonique employé pour enregistrer les chemins relatifs.</summary>
    internal const char StoredDirectorySeparator = '/';
    /// <summary>Version du schéma des documents de configuration produits et reconnus par ce magasin.</summary>
    /// <remarks>Ce numéro correspond à la structure sérialisée et à ses contrôles de lecture. Un numéro plus élevé
    /// dans un autre magasin n’ajoute aucune capacité : le numéro doit rester cohérent avec le contrat utilisé ici.</remarks>
    internal const int CurrentSchemaVersion = 1;
    /// <summary>Nombre initial d’autorisations du verrou d’écriture : une sauvegarde peut commencer.</summary>
    internal const int InitialWriterCount = 1;
    /// <summary>Nombre maximal d’écritures concurrentes dans un même magasin.</summary>
    internal const int MaximumWriterCount = 1;
    /// <summary>Demande une indentation du JSON pour rendre le document enregistré lisible.</summary>
    internal const bool WriteIndentedJson = true;
    /// <summary>Autorise la suppression des sous-dossiers possédés lors du retrait d’une configuration.</summary>
    /// <remarks>Cette règle porte sur le dossier de configuration ; elle n’autorise pas à supprimer les médias externes référencés.</remarks>
    internal const bool RecursiveDirectoryDelete = true;
}

