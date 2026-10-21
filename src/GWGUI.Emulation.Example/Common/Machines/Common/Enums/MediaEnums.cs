namespace GWGUI.Emulation.Example.Common.Machines.Common.Enums;

/// <summary>Disponibilité déclarée d’un média dans les capacités de la machine.</summary>
public enum MediaAvailability
{
    /// <summary>La capacité considérée permet l’utilisation du média.</summary>
    Available,
    /// <summary>La capacité considérée ne permet pas l’utilisation du média.</summary>
    Unavailable
}

/// <summary>Catégories de contenu utilisées pour vérifier et projeter les montages de la configuration.</summary>
public enum MediaCategory
{
    /// <summary>Image de disquette destinée à un lecteur de disquettes.</summary>
    Floppy,
    /// <summary>Image ou contenu de cassette destiné au transport correspondant.</summary>
    Cassette,
    /// <summary>Contenu de cartouche présenté au connecteur de cartouche.</summary>
    Cartridge
}
