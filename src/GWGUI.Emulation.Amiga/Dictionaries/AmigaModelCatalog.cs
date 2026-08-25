namespace GWGUI.Emulation.Amiga.Dictionaries;

public static class AmigaModelCatalog
{
    private static readonly HashSet<string> LegacyBackendIds =
        [AmigaModelCatalogConstants.A500OG, AmigaModelCatalogConstants.A1200OG, AmigaModelCatalogConstants.A2000OG, AmigaModelCatalogConstants.A4030, AmigaModelCatalogConstants.A4040, AmigaModelCatalogConstants.CD32FR];
    public static IReadOnlyList<AmigaModel> All { get; } =
    [
        new(AmigaModelCatalogConstants.A500, AmigaModelCatalogConstants.Amiga500, AmigaModelCatalogConstants.A500, [AmigaModelCatalogConstants.Value68000], AmigaModelCatalogConstants.OCS, 512, 512, 0, false, AmigaModelCatalogConstants.Value13, 4, true, 4, ControllerPortCount: 2),
        new(AmigaModelCatalogConstants.A500PLUS, AmigaModelCatalogConstants.Amiga500Plus, AmigaModelCatalogConstants.A500PLUS, [AmigaModelCatalogConstants.Value68000], AmigaModelCatalogConstants.ECS, 1024, 0, 0, false, AmigaModelCatalogConstants.Value204, 4, true, 4, ControllerPortCount: 2),
        new(AmigaModelCatalogConstants.A600, AmigaModelCatalogConstants.Amiga600, AmigaModelCatalogConstants.A600, [AmigaModelCatalogConstants.Value68000], AmigaModelCatalogConstants.ECS, 1024, 0, 0, false, AmigaModelCatalogConstants.Value31, 4, true, 4, ControllerPortCount: 2),
        new(AmigaModelCatalogConstants.A1000, AmigaModelCatalogConstants.Amiga1000, AmigaModelCatalogConstants.A500OG, [AmigaModelCatalogConstants.Value68000], AmigaModelCatalogConstants.OCS, 512, 0, 0, false, AmigaModelCatalogConstants.Value12, 4, true, 2, ControllerPortCount: 2),
        new(AmigaModelCatalogConstants.A1200, AmigaModelCatalogConstants.Amiga1200, AmigaModelCatalogConstants.A1200, [AmigaModelCatalogConstants.Value68020], AmigaModelCatalogConstants.AGA, 2048, 0, 0, false, AmigaModelCatalogConstants.Value31, 4, true, 4, ControllerPortCount: 2),
        new(AmigaModelCatalogConstants.A2000, AmigaModelCatalogConstants.Amiga2000, AmigaModelCatalogConstants.A2000, [AmigaModelCatalogConstants.Value68000], AmigaModelCatalogConstants.ECS, 1024, 0, 0, false, AmigaModelCatalogConstants.Value31, 4, true, 8, ControllerPortCount: 2),
        new(AmigaModelCatalogConstants.A3000, AmigaModelCatalogConstants.Amiga3000, AmigaModelCatalogConstants.A2000, [AmigaModelCatalogConstants.Value68030], AmigaModelCatalogConstants.ECS, 2048, 0, 8, false, AmigaModelCatalogConstants.Value31, 4, true, 8, ControllerPortCount: 2),
        new(AmigaModelCatalogConstants.A4000, AmigaModelCatalogConstants.Amiga4000, AmigaModelCatalogConstants.A4040, [AmigaModelCatalogConstants.Value68040, AmigaModelCatalogConstants.Value68030], AmigaModelCatalogConstants.AGA, 2048, 0, 8, false, AmigaModelCatalogConstants.Value31, 4, true, 8, ControllerPortCount: 2),
        new(AmigaModelCatalogConstants.CDTV, AmigaModelCatalogConstants.CommodoreCDTV, AmigaModelCatalogConstants.CDTV, [AmigaModelCatalogConstants.Value68000], AmigaModelCatalogConstants.OCS, 1024, 0, 0, true, AmigaModelCatalogConstants.Value13CDTV, 1, true, 2,
            ControllerPortCount: 2, HasBuiltInFloppyDrive: false),
        new(AmigaModelCatalogConstants.CD32, AmigaModelCatalogConstants.AmigaCD32, AmigaModelCatalogConstants.CD32, [AmigaModelCatalogConstants.Value68020], AmigaModelCatalogConstants.AGA, 2048, 0, 0, true, AmigaModelCatalogConstants.Value31CD32, 0, false, 0, 2, true,
            ControllerPortCount: 2, HasBuiltInFloppyDrive: false)
    ];

    public static AmigaModel Get(string id) => All.FirstOrDefault(model => model.Id.Equals(id, StringComparison.Ordinal))
        ?? FromLegacyId(id)
        ?? throw new ArgumentOutOfRangeException(nameof(id), id, AmigaModelCatalogConstants.UnsupportedAmigaModel);

    public static AmigaModel? FromLegacyId(string id) => id switch
    {
        AmigaModelCatalogConstants.A500OG => All.First(model => model.Id == AmigaModelCatalogConstants.A500),
        AmigaModelCatalogConstants.A1200OG => All.First(model => model.Id == AmigaModelCatalogConstants.A1200),
        AmigaModelCatalogConstants.A2000OG => All.First(model => model.Id == AmigaModelCatalogConstants.A2000),
        AmigaModelCatalogConstants.A4030 or AmigaModelCatalogConstants.A4040 => All.First(model => model.Id == AmigaModelCatalogConstants.A4000),
        AmigaModelCatalogConstants.CD32FR => All.First(model => model.Id == AmigaModelCatalogConstants.CD32),
        _ => null
    };

    public static string BackendModelFor(string id) => LegacyBackendIds.Contains(id) ? id : Get(id).BackendModel;
}
