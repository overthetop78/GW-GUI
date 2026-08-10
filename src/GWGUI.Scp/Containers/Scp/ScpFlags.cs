namespace GWGUI.Scp;

/// <summary>
/// Indique les caractéristiques de capture déclarées dans l'en-tête d'un conteneur SCP.
/// </summary>
[Flags]
public enum ScpFlags : byte
{
    /// <summary>La capture est alignée sur l'index de rotation.</summary>
    IndexAligned = 1,

    /// <summary>La capture utilise une densité de 96 pistes par pouce.</summary>
    Tpi96 = 2,

    /// <summary>Le support capturé tourne nominalement à 360 tours par minute.</summary>
    Rpm360 = 4,

    /// <summary>Les données de flux ont été normalisées.</summary>
    Normalized = 8,

    /// <summary>La capture est déclarée réinscriptible.</summary>
    Writable = 16,

    /// <summary>Le conteneur possède un pied de fichier SCP.</summary>
    Footer = 32,

    /// <summary>Le conteneur représente un support SCP étendu et non une disquette standard.</summary>
    Extended = 64,

    /// <summary>Le conteneur a été produit par un logiciel tiers.</summary>
    ThirdPartyCreator = 128
}
