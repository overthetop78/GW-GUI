using GWGUI.MediaAnalysis.Contracts;
using GWGUI.MediaAnalysis.Enums;
using GWGUI.MediaAnalysis.Functions;

namespace GWGUI.MediaAnalysis.Dictionaries.FileTypes;

public static class MediaContentTypeCatalog
{
    public static IReadOnlyList<MediaContentTypeDefinition> Rows { get; } =
    [
        .. CommonFileTypeTable.Rows,
        .. AmigaFileTypeTable.Rows,
        .. IbmPcFileTypeTable.Rows,
        .. AtariFileTypeTable.Rows,
        .. AppleFileTypeTable.Rows,
        .. CommodoreFileTypeTable.Rows,
        .. CpmFileTypeTable.Rows,
        .. BbcMicroFileTypeTable.Rows,
        .. DecFileTypeTable.Rows,
        .. MsxFileTypeTable.Rows,
        .. UcsdFileTypeTable.Rows
    ];

    private static readonly IReadOnlyDictionary<(MediaFileSystemFamily? Family, string Extension), MediaContentTypeDefinition> Index =
        Rows.ToDictionary(row => (row.Family, row.Extension));

    public static MediaContentTypeDefinition? Find(MediaFileSystemFamily family, string? extension)
    {
        var normalized = MediaContentTypeRuleFactory.Normalize(extension ?? string.Empty);
        if (normalized.Length == 0) return null;
        if (Index.TryGetValue((family, normalized), out var exact)) return exact;
        var parent = family switch
        {
            MediaFileSystemFamily.Atari8Bit or MediaFileSystemFamily.AtariTos => MediaFileSystemFamily.Atari,
            _ => (MediaFileSystemFamily?)null
        };
        if (parent is not null && Index.TryGetValue((parent, normalized), out var inherited)) return inherited;
        return Index.TryGetValue((null, normalized), out var common) ? common : null;
    }
}
