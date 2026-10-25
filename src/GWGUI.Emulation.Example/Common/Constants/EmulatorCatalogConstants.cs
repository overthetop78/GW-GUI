namespace GWGUI.Emulation.Example.Common.Constants;

/// <summary>Conventions d'installation choisies pour le catalogue du module.</summary>
/// <remarks>Le rôle et les noms suivent ceux des catalogues téléchargeables. Les valeurs décrivent le profil
/// utilisé ici : ses cœurs sont déjà livrés dans l'assembly dont le build fournit la version.</remarks>
internal static class EmulatorCatalogConstants
{
    /// <summary>Nom de manifeste représenté par une chaîne vide pour l'installation intégrée.</summary>
    /// <remarks>Le résultat Common transmet cette valeur comme ManifestPath. Le service de ce profil résout
    /// directement l'assembly et sa version ; un catalogue téléchargeable définit ici le nom de son manifeste.</remarks>
    internal const string ManifestFileName = "";
}
