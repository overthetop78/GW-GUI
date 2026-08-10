namespace GWGUI.MediaEngine.Containers.Scp;

/// <summary>
/// Indique les caractéristiques de capture déclarées dans l'en-tête d'un conteneur SCP.
/// </summary>
/// <remarks>
/// Chaque valeur occupe un bit distinct de l'octet de drapeaux SCP. Plusieurs valeurs peuvent donc être
/// combinées au moyen d'une opération binaire afin de représenter toutes les caractéristiques d'une capture.
/// </remarks>
[Flags]
public enum ScpFlags : byte
{
    /// <summary>Aucune caractéristique optionnelle n'est déclarée ; valeur binaire <c>0x00</c>.</summary>
    None = 0x00,

    /// <summary>La capture est alignée sur l'index de rotation ; masque binaire <c>0x01</c>.</summary>
    IndexAligned = 0x01,

    /// <summary>La capture utilise une densité de 96 pistes par pouce ; masque binaire <c>0x02</c>.</summary>
    Tpi96 = 0x02,

    /// <summary>Le support capturé tourne nominalement à 360 tours par minute ; masque binaire <c>0x04</c>.</summary>
    Rpm360 = 0x04,

    /// <summary>Les données de flux ont été normalisées ; masque binaire <c>0x08</c>.</summary>
    Normalized = 0x08,

    /// <summary>La capture est déclarée réinscriptible ; masque binaire <c>0x10</c>.</summary>
    Writable = 0x10,

    /// <summary>Le conteneur possède un pied de fichier SCP ; masque binaire <c>0x20</c>.</summary>
    Footer = 0x20,

    /// <summary>Le conteneur représente un support SCP étendu et non une disquette standard ; masque binaire <c>0x40</c>.</summary>
    Extended = 0x40,

    /// <summary>Le conteneur a été produit par un logiciel tiers ; masque binaire <c>0x80</c>.</summary>
    ThirdPartyCreator = 0x80
}
