using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Apple.Lisa;

/// <summary>Décode les tags de pages Lisa sans inventer de valeur lorsqu'ils sont absents ou tronqués.</summary>
internal static class LisaPageTagReader
{
    /// <summary>Tente de lire un tag complet.</summary>
    public static bool TryRead(SectorBlock block, out LisaPageTag tag)
    {
        tag = default;
        if (block.Tag is null || block.Tag.Count < LisaFileSystemLayout.TagLength) return false;
        var fileId = (ushort)(block.Tag[LisaFileSystemLayout.TagFileIdHighOffset] << BitPrimitives.BitsPerByte | block.Tag[LisaFileSystemLayout.TagFileIdLowOffset]);
        var pageNumber = (block.Tag[LisaFileSystemLayout.TagPageHighOffset] << BitPrimitives.BitsPerByte | block.Tag[LisaFileSystemLayout.TagPageLowOffset]) & LisaFileSystemLayout.PageNumberMask;
        tag = new(fileId, pageNumber);
        return true;
    }

    /// <summary>Indique si l'identifiant désigne un fichier utilisateur et non un fichier système réservé.</summary>
    public static bool IsUserFile(ushort fileId) => fileId is >= LisaFileSystemLayout.FirstUserFileId and <= LisaFileSystemLayout.LastUserFileId && !LisaFileSystemLayout.ReservedFileIds.Contains(fileId);
}
