namespace GWGUI.MediaEngine.Enums;

/// <summary>Indique si les adresses d'une image sectorielle décrivent une géométrie physique ou seulement un espace logique.</summary>
public enum SectorImageAddressingKind
{
    /// <summary>Les cylindres, faces et secteurs correspondent à une géométrie physique connue.</summary>
    Physical,
    /// <summary>Les blocs sont ordonnés logiquement sans géométrie physique déterminée.</summary>
    Logical
}
