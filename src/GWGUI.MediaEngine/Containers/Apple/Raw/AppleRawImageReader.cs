using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Containers.Apple.Raw;

/// <summary>Dirige une charge utile Apple sans en-tête vers le Reader correspondant à sa capacité cataloguée.</summary>
internal static class AppleRawImageReader
{
    /// <summary>Recherche la capacité, exécute les sondes applicables et retourne l'image avec la preuve du choix.</summary>
    public static AppleRawImageReadResult Read(ReadOnlyMemory<byte> data, string extension)
    {
        var layout = AppleRawImageLayoutCatalog.Find(data.Length);
        if (layout is null) throw AppleRawImageExceptions.UnsupportedLayout(data.Length, extension, ["capacity catalog"]);
        if (layout == AppleRawImageLayoutCatalog.D13) return AppleII525RawImageReader.ReadDos32(data);
        if (layout == AppleRawImageLayoutCatalog.AppleII140K) return AppleII525RawImageReader.Read(data, extension);
        return Apple35RawImageReader.Read(data, extension, layout);
    }
}
