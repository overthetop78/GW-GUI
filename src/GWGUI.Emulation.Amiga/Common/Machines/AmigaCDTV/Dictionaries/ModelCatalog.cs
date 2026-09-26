namespace GWGUI.Emulation.Amiga.Common.Machines.AmigaCDTV.Dictionaries;

using CommonModelConstants = GWGUI.Emulation.Amiga.Common.Machines.Common.Constants.ModelConstants;
using FamilyModelConstants = GWGUI.Emulation.Amiga.Common.Machines.AmigaCDTV.Constants.ModelConstants;

internal static class ModelCatalog
{
    internal static IReadOnlyList<Model> All { get; } =
    [
        new(FamilyModelConstants.CDTV, FamilyModelConstants.DisplayName, FamilyModelConstants.CDTV,
            [CommonModelConstants.Value68000], CommonModelConstants.OCS, 1024, 0, 0, true,
            FamilyModelConstants.FirmwareVersion, 1, true, 2, ControllerPortCount: 2,
            HasBuiltInFloppyDrive: false)
    ];
}
