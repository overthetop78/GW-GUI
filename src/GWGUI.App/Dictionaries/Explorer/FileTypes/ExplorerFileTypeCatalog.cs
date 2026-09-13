using GWGUI.App.Contracts.Explorer;
using GWGUI.App.Enums.Explorer;

namespace GWGUI.App.Dictionaries.Explorer.FileTypes;

public static class ExplorerFileTypeCatalog
{
    public static IReadOnlyList<ExplorerFileTypeDefinition> Rows { get; } =
    [
        .. CommonFileTypeTable.Rows,
        .. AmigaFileTypeTable.Rows,
        .. IbmPcFileTypeTable.Rows,
        .. AtariFileTypeTable.Rows,
        .. AppleFileTypeTable.Rows,
        .. CommodoreFileTypeTable.Rows,
        .. CpmFileTypeTable.Rows,
        .. OtherMachineFileTypeTable.Rows
    ];

    private static readonly IReadOnlyDictionary<(ExplorerFileSystemFamily? Family, string Extension), ExplorerFileTypeDefinition> Index =
        Rows.ToDictionary(row => (row.Family, row.Extension));

    public static ExplorerFileTypeDefinition? Find(ExplorerFileSystemFamily family, string? extension)
    {
        var normalized = ExplorerFileTypeRuleFactory.Normalize(extension ?? string.Empty);
        if (normalized.Length == 0) return null;
        if (Index.TryGetValue((family, normalized), out var exact)) return exact;
        return Index.TryGetValue((null, normalized), out var common) ? common : null;
    }
}
