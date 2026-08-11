namespace GWGUI.MediaEngine.SectorImages;

/// <summary>Construit les erreurs produites pendant la reconstruction sectorielle d'un conteneur 86F.</summary>
internal static class I86fSectorImageExceptions
{
    /// <summary>Signale qu'aucune piste présente n'a fourni de secteur FM ou MFM décodable.</summary>
    public static InvalidDataException NoDecodableSectors(int trackCount) => new($"No FM or MFM sector could be decoded from the {trackCount} present 86F tracks.");
}
