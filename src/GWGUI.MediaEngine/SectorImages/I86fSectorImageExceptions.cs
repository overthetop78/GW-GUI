namespace GWGUI.MediaEngine.SectorImages;

internal static class I86fSectorImageExceptions
{
    public static InvalidDataException NoDecodableSectors(int trackCount) => new($"No FM or MFM sector could be decoded from the {trackCount} present 86F tracks.");
}
