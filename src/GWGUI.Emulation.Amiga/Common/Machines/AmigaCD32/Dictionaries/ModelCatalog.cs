namespace GWGUI.Emulation.Amiga.Common.Machines.AmigaCD32.Dictionaries;

using CommonModelConstants = GWGUI.Emulation.Amiga.Common.Constants.ModelConstants;
using FamilyModelConstants = GWGUI.Emulation.Amiga.Common.Machines.AmigaCD32.Constants.ModelConstants;

internal static class ModelCatalog
{
    internal static IReadOnlyList<Model> All { get; } =
    [
        new(FamilyModelConstants.CD32, FamilyModelConstants.DisplayName, FamilyModelConstants.CD32,
            [CommonModelConstants.Value68020], CommonModelConstants.AGA, 2048, 0, 0, true,
            FamilyModelConstants.FirmwareVersion, 0, false, 0, 2, true, ControllerPortCount: 2,
            HasBuiltInFloppyDrive: false)
    ];
}
