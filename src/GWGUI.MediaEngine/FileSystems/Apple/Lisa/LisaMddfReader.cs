using System.Buffers.Binary;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Apple.Lisa;

/// <summary>Sélectionne et valide la page MDDF d'un volume Lisa.</summary>
internal static class LisaMddfReader
{
    /// <summary>Lit la dernière page MDDF dans l'ordre validé de ses numéros de page.</summary>
    public static LisaMddf Read(SectorImage image)
    {
        var pages = image.AvailableBlocks.Select(block => (Block: block, HasTag: LisaPageTagReader.TryRead(block, out var tag), Tag: tag)).Where(item => item.HasTag && item.Tag.FileId == LisaFileSystemLayout.MddfFileId).OrderBy(item => item.Tag.PageNumber).ToArray();
        if (pages.Length == 0) throw LisaFileSystemExceptions.MissingTaggedFileSystem(image.AvailableBlocks.Count);
        var data = pages[^1].Block.Data.ToArray();
        if (data.Length < LisaVolumeHeader.MinimumLength) throw LisaFileSystemExceptions.TruncatedMddf(data.Length, LisaVolumeHeader.MinimumLength);
        var version = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(LisaVolumeHeader.VersionOffset, sizeof(ushort)));
        var nameLength = Math.Min(data[LisaVolumeHeader.NameLengthOffset], (byte)LisaVolumeHeader.MaximumNameLength);
        var name = nameLength > 0 && data.Length >= LisaVolumeHeader.NameOffset + nameLength ? LisaVolumeHeader.DecodeName(data.AsSpan(LisaVolumeHeader.NameOffset, nameLength)) : string.Empty;
        return new(version, string.IsNullOrWhiteSpace(name) ? LisaFileSystemLayout.DefaultVolumeName : name);
    }
}
