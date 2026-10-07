namespace GWGUI.Emulation.Commodore.Common.Machines.CommodoreCDTV.Dictionaries;

using CommonModelConstants = GWGUI.Emulation.Commodore.Common.Machines.Common.Constants.ModelConstants;
using FamilyModelConstants = GWGUI.Emulation.Commodore.Common.Machines.CommodoreCDTV.Constants.ModelConstants;

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
