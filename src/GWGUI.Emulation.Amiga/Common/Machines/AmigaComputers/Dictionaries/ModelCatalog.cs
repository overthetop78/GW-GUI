namespace GWGUI.Emulation.Amiga.Common.Machines.AmigaComputers.Dictionaries;

using CommonModelConstants = GWGUI.Emulation.Amiga.Common.Constants.ModelConstants;
using FamilyModelConstants = GWGUI.Emulation.Amiga.Common.Machines.AmigaComputers.Constants.ModelConstants;

internal static class ModelCatalog
{
    internal static IReadOnlyList<Model> All { get; } =
    [
        new(FamilyModelConstants.A500, FamilyModelConstants.Amiga500, FamilyModelConstants.A500, [CommonModelConstants.Value68000], CommonModelConstants.OCS, 512, 512, 0, false, CommonModelConstants.Value13, 4, true, 4, ControllerPortCount: 2),
        new(FamilyModelConstants.A500PLUS, FamilyModelConstants.Amiga500Plus, FamilyModelConstants.A500PLUS, [CommonModelConstants.Value68000], CommonModelConstants.ECS, 1024, 0, 0, false, CommonModelConstants.Value204, 4, true, 4, ControllerPortCount: 2),
        new(FamilyModelConstants.A600, FamilyModelConstants.Amiga600, FamilyModelConstants.A600, [CommonModelConstants.Value68000], CommonModelConstants.ECS, 1024, 0, 0, false, CommonModelConstants.Value31, 4, true, 4, ControllerPortCount: 2),
        new(FamilyModelConstants.A1000, FamilyModelConstants.Amiga1000, FamilyModelConstants.A500OG, [CommonModelConstants.Value68000], CommonModelConstants.OCS, 512, 0, 0, false, CommonModelConstants.Value12, 4, true, 2, ControllerPortCount: 2),
        new(FamilyModelConstants.A1200, FamilyModelConstants.Amiga1200, FamilyModelConstants.A1200, [CommonModelConstants.Value68020], CommonModelConstants.AGA, 2048, 0, 0, false, CommonModelConstants.Value31, 4, true, 4, ControllerPortCount: 2),
        new(FamilyModelConstants.A2000, FamilyModelConstants.Amiga2000, FamilyModelConstants.A2000, [CommonModelConstants.Value68000], CommonModelConstants.ECS, 1024, 0, 0, false, CommonModelConstants.Value31, 4, true, 8, ControllerPortCount: 2),
        new(FamilyModelConstants.A3000, FamilyModelConstants.Amiga3000, FamilyModelConstants.A2000, [CommonModelConstants.Value68030], CommonModelConstants.ECS, 2048, 0, 8, false, CommonModelConstants.Value31, 4, true, 8, ControllerPortCount: 2),
        new(FamilyModelConstants.A4000, FamilyModelConstants.Amiga4000, FamilyModelConstants.A4040, [CommonModelConstants.Value68040, CommonModelConstants.Value68030], CommonModelConstants.AGA, 2048, 0, 8, false, CommonModelConstants.Value31, 4, true, 8, ControllerPortCount: 2)
    ];
}
