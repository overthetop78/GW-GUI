namespace GWGUI.MediaEngine.Exploration.Metadata;

/// <summary>Identifiants stables des caractéristiques de contenu détectables.</summary>
public static class DiskContentIds
{
    /// <summary>Crack attribué explicitement à The Company dans l'image.</summary>
    public const string CrackTheCompany = "crack-the-company";
    /// <summary>Données compactées portant la signature FIRE.</summary>
    public const string CompressionFire = "compression-fire";
    /// <summary>Organisation sectorielle composée de blocs ATN!/File Imploder.</summary>
    public const string OrganizationAtnArchive = "organization-atn-archive";
    /// <summary>Image amorçable complète chargée directement par secteurs et dépourvue de catalogue reconnu.</summary>
    public const string OrganizationCataloglessBootImage = "organization-catalogless-boot-image";
    /// <summary>Données compactées au format ATN!, compatible avec File Imploder.</summary>
    public const string CompressionAtnImploder = "compression-atn-imploder";
}
