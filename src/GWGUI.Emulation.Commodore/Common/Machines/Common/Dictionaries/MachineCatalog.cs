using GWGUI.Emulation;
using GWGUI.Emulation.Commodore.Common.Machines.Common.Constants;
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

public static class MachineCatalog
{
    public static IReadOnlyList<EmulationMachineDefinition> All { get; } =
    [
        new(AmigaComputersModels.A500.Id, MachineCatalogConstants.A500ResourceKey, MachineCatalogConstants.A500ImageResource),
        new(AmigaComputersModels.A500PLUS.Id, MachineCatalogConstants.A500PLUSResourceKey, MachineCatalogConstants.A500PLUSImageResource),
        new(AmigaComputersModels.A600.Id, MachineCatalogConstants.A600ResourceKey, MachineCatalogConstants.A600ImageResource),
        new(AmigaComputersModels.A1000.Id, MachineCatalogConstants.A1000ResourceKey, MachineCatalogConstants.A1000ImageResource),
        new(AmigaComputersModels.A1200.Id, MachineCatalogConstants.A1200ResourceKey, MachineCatalogConstants.A1200ImageResource),
        new(AmigaComputersModels.A2000.Id, MachineCatalogConstants.A2000ResourceKey, MachineCatalogConstants.A2000ImageResource),
        new(AmigaComputersModels.A3000.Id, MachineCatalogConstants.A3000ResourceKey, MachineCatalogConstants.A3000ImageResource),
        new(AmigaComputersModels.A4000.Id, MachineCatalogConstants.A4000ResourceKey, MachineCatalogConstants.A4000ImageResource),
        new(CommodoreCDTVModels.CDTV.Id, MachineCatalogConstants.CDTVResourceKey, MachineCatalogConstants.CDTVImageResource),
        new(AmigaCD32Models.CD32.Id, MachineCatalogConstants.CD32ResourceKey, MachineCatalogConstants.CD32ImageResource),
        new(Plus4Models.C16.Id, MachineCatalogConstants.C16ResourceKey),
        new(C64Models.C64.Id, MachineCatalogConstants.C64ResourceKey),
        new(C64Models.C64Dtv.Id, MachineCatalogConstants.C64DtvResourceKey),
        new(C64Models.C64SuperCpu.Id, MachineCatalogConstants.C64SuperCpuResourceKey),
        new(C128Models.C128.Id, MachineCatalogConstants.C128ResourceKey),
        new(CbmIIModels.CbmII510.Id, MachineCatalogConstants.CbmII510ResourceKey),
        new(CbmIIModels.CbmII610.Id, MachineCatalogConstants.CbmII610ResourceKey),
        new(CbmIIModels.CbmII620.Id, MachineCatalogConstants.CbmII620ResourceKey),
        new(CbmIIModels.CbmII620Plus.Id, MachineCatalogConstants.CbmII620PlusResourceKey),
        new(CbmIIModels.CbmII710.Id, MachineCatalogConstants.CbmII710ResourceKey),
        new(CbmIIModels.CbmII720.Id, MachineCatalogConstants.CbmII720ResourceKey),
        new(CbmIIModels.CbmII720Plus.Id, MachineCatalogConstants.CbmII720PlusResourceKey),
        new(PetModels.Pet2001.Id, MachineCatalogConstants.Pet2001ResourceKey),
        new(PetModels.Pet3008.Id, MachineCatalogConstants.Pet3008ResourceKey),
        new(PetModels.Pet3016.Id, MachineCatalogConstants.Pet3016ResourceKey),
        new(PetModels.Pet3032.Id, MachineCatalogConstants.Pet3032ResourceKey),
        new(PetModels.Pet3032B.Id, MachineCatalogConstants.Pet3032BResourceKey),
        new(PetModels.Pet4016.Id, MachineCatalogConstants.Pet4016ResourceKey),
        new(PetModels.Pet4032.Id, MachineCatalogConstants.Pet4032ResourceKey),
        new(PetModels.Pet4032B.Id, MachineCatalogConstants.Pet4032BResourceKey),
        new(PetModels.Pet8032.Id, MachineCatalogConstants.Pet8032ResourceKey),
        new(PetModels.Pet8096.Id, MachineCatalogConstants.Pet8096ResourceKey),
        new(PetModels.Pet8296.Id, MachineCatalogConstants.Pet8296ResourceKey),
        new(Plus4Models.Plus4.Id, MachineCatalogConstants.Plus4ResourceKey),
        new(PetModels.SuperPet.Id, MachineCatalogConstants.SuperPetResourceKey),
        new(Vic20Models.Vic20.Id, MachineCatalogConstants.Vic20ResourceKey),
        new(Vic20Models.Vic21.Id, MachineCatalogConstants.Vic21ResourceKey),
        new(Plus4Models.V364.Id, MachineCatalogConstants.V364ResourceKey),
        new(Plus4Models.C232.Id, MachineCatalogConstants.C232ResourceKey)
    ];
}
