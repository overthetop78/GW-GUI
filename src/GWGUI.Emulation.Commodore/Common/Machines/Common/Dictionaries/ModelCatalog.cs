using AmigaComputersModels = GWGUI.Emulation.Commodore.Common.Machines.AmigaComputers.Dictionaries.ModelCatalog;
using CommodoreCDTVModels = GWGUI.Emulation.Commodore.Common.Machines.CommodoreCDTV.Dictionaries.ModelCatalog;
using AmigaCD32Models = GWGUI.Emulation.Commodore.Common.Machines.AmigaCD32.Dictionaries.ModelCatalog;
using Plus4Models = GWGUI.Emulation.Commodore.Common.Machines.Plus4.Dictionaries.ModelCatalog;
using C64Models = GWGUI.Emulation.Commodore.Common.Machines.C64.Dictionaries.ModelCatalog;
using C128Models = GWGUI.Emulation.Commodore.Common.Machines.C128.Dictionaries.ModelCatalog;
using CbmIIModels = GWGUI.Emulation.Commodore.Common.Machines.CbmII.Dictionaries.ModelCatalog;
using PetModels = GWGUI.Emulation.Commodore.Common.Machines.Pet.Dictionaries.ModelCatalog;
using Vic20Models = GWGUI.Emulation.Commodore.Common.Machines.Vic20.Dictionaries.ModelCatalog;

namespace GWGUI.Emulation.Commodore.Common.Machines.Common.Dictionaries;

public static class ModelCatalog
{
    public static IReadOnlyList<Model> All { get; } =
    [
        AmigaComputersModels.A500,
        AmigaComputersModels.A500PLUS,
        AmigaComputersModels.A600,
        AmigaComputersModels.A1000,
        AmigaComputersModels.A1200,
        AmigaComputersModels.A2000,
        AmigaComputersModels.A3000,
        AmigaComputersModels.A4000,
        CommodoreCDTVModels.CDTV,
        AmigaCD32Models.CD32,
        Plus4Models.C16,
        C64Models.C64,
        C64Models.C64Dtv,
        C64Models.C64SuperCpu,
        C128Models.C128,
        C128Models.C128D,
        CbmIIModels.CbmII510,
        CbmIIModels.CbmII610,
        CbmIIModels.CbmII620,
        CbmIIModels.CbmII620Plus,
        CbmIIModels.CbmII710,
        CbmIIModels.CbmII720,
        CbmIIModels.CbmII720Plus,
        PetModels.Pet2001,
        PetModels.Pet3008,
        PetModels.Pet3016,
        PetModels.Pet3032,
        PetModels.Pet3032B,
        PetModels.Pet4016,
        PetModels.Pet4032,
        PetModels.Pet4032B,
        PetModels.Pet8032,
        PetModels.Pet8096,
        PetModels.Pet8296,
        Plus4Models.Plus4,
        PetModels.SuperPet,
        Vic20Models.Vic20,
        Vic20Models.Vic21,
        Plus4Models.V364,
        Plus4Models.C232
    ];

    public static Model Get(string id) => All.FirstOrDefault(model => model.Id.Equals(id, StringComparison.Ordinal))
        ?? throw new ArgumentOutOfRangeException(nameof(id), id, null);
}
