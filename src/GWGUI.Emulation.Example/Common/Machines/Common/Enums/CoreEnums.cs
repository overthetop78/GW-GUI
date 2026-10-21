namespace GWGUI.Emulation.Example.Common.Machines.Common.Enums;

/// <summary>Identités de backends utilisées par les configurations et les catalogues du module.</summary>
/// <remarks>Ces valeurs désignent les adaptateurs, pas les modèles matériels.
/// Le catalogue associe chaque identité prise en charge à sa description et à sa fabrique.</remarks>
public enum Emulator
{
    /// <summary>Aucun backend sélectionné ; la valeur négative reste distincte des backends déclarés.</summary>
    None = -1,
    /// <summary>Backend portant l’identité B2 dans le catalogue d’adaptateurs.</summary>
    B2 = 0
}
