namespace GWGUI.Emulation.Example.Common.Constants;

/// <summary>Codes de sélection régionale des cartouches utilisés dans les options du module.</summary>
/// <remarks>La région d’une cartouche reste distincte du standard vidéo de la machine. Common inscrit
/// le choix sous la clé du module ; l’adaptateur le traduit vers les options reconnues par son cœur.
/// Les valeurs générales d’activation et de désactivation sont fournies par les contrats communs de réglages.</remarks>
internal static class CartridgeConstants
{
    /// <summary>Clé d’option propre au module pour transmettre le choix régional de la cartouche.</summary>
    internal const string RegionOptionKey = "gwgui_example_cartridge_region";
    /// <summary>Code laissant le backend déterminer la région depuis le contenu ou ses réglages.</summary>
    internal const string AutomaticRegionValue = "auto";
    /// <summary>Code de sélection régionale NTSC ; sa traduction vers les options natives appartient à l’adaptateur.</summary>
    internal const string NtscRegionValue = "ntsc";
    /// <summary>Code de sélection régionale PAL.</summary>
    internal const string PalRegionValue = "pal";
    /// <summary>Code de sélection régionale SECAM, accepté seulement si le backend le déclare.</summary>
    internal const string SecamRegionValue = "secam";
    /// <summary>Capacité régionale de base : la sélection reste indisponible sans déclaration du backend.</summary>
    /// <remarks>Le backend annonce explicitement sa prise en charge lorsqu’il peut appliquer ce choix.
    /// Cette valeur de base ne remplace pas la détection de région automatique effectuée par le cœur.</remarks>
    internal const bool SupportsRegionByDefault = false;
}
