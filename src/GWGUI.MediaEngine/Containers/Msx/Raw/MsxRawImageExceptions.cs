namespace GWGUI.MediaEngine.Containers.Msx.Raw;

/// <summary>Construit les erreurs propres aux images brutes MSX-DOS.</summary>
internal static class MsxRawImageExceptions
{
    /// <summary>Crée l'erreur signalant un secteur d'amorçage non MSX-DOS.</summary>
    /// <param name="length">Taille du contenu contrôlé, en octets.</param>
    /// <returns>L'exception décrivant le secteur d'amorçage invalide.</returns>
    public static InvalidDataException InvalidBootSector(int length) => new($"L'image de {length} octet(s) ne contient pas de secteur d'amorçage MSX-DOS valide.");
    /// <summary>Crée l'erreur signalant une capacité ou un descripteur non pris en charge.</summary>
    /// <param name="length">Capacité totale de l'image, en octets.</param>
    /// <param name="mediaDescriptor">Descripteur de média lu dans le BPB FAT.</param>
    /// <returns>L'exception décrivant la géométrie non prise en charge.</returns>
    public static InvalidDataException UnsupportedGeometry(int length, byte mediaDescriptor) => new($"Aucune géométrie MSX-DOS ne correspond aux {length} octets et au descripteur média 0x{mediaDescriptor:X2}.");
}
