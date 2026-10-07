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
        new(Plus4Models.C16.Id, MachineCatalogConstants.C16ResourceKey, MachineCatalogConstants.C16ImageResource),
        new(C64Models.C64.Id, MachineCatalogConstants.C64ResourceKey, MachineCatalogConstants.C64ImageResource),
        new(C64Models.C64Dtv.Id, MachineCatalogConstants.C64DtvResourceKey, MachineCatalogConstants.C64DtvImageResource),
        new(C64Models.C64SuperCpu.Id, MachineCatalogConstants.C64SuperCpuResourceKey, MachineCatalogConstants.C64SuperCpuImageResource),
        new(C128Models.C128.Id, MachineCatalogConstants.C128ResourceKey, MachineCatalogConstants.C128ImageResource),
        new(CbmIIModels.CbmII510.Id, MachineCatalogConstants.CbmII510ResourceKey, MachineCatalogConstants.CbmII510ImageResource),
        new(CbmIIModels.CbmII610.Id, MachineCatalogConstants.CbmII610ResourceKey, MachineCatalogConstants.CbmII610ImageResource),
        new(CbmIIModels.CbmII620.Id, MachineCatalogConstants.CbmII620ResourceKey, MachineCatalogConstants.CbmII610ImageResource),
        new(CbmIIModels.CbmII620Plus.Id, MachineCatalogConstants.CbmII620PlusResourceKey, MachineCatalogConstants.CbmII610ImageResource),
        new(CbmIIModels.CbmII710.Id, MachineCatalogConstants.CbmII710ResourceKey, MachineCatalogConstants.CbmII710ImageResource),
        new(CbmIIModels.CbmII720.Id, MachineCatalogConstants.CbmII720ResourceKey, MachineCatalogConstants.CbmII710ImageResource),
        new(CbmIIModels.CbmII720Plus.Id, MachineCatalogConstants.CbmII720PlusResourceKey, MachineCatalogConstants.CbmII710ImageResource),
        new(PetModels.Pet2001.Id, MachineCatalogConstants.Pet2001ResourceKey, MachineCatalogConstants.Pet2001ImageResource),
        new(PetModels.Pet3008.Id, MachineCatalogConstants.Pet3008ResourceKey, MachineCatalogConstants.Pet3032ImageResource),
        new(PetModels.Pet3016.Id, MachineCatalogConstants.Pet3016ResourceKey, MachineCatalogConstants.Pet3032ImageResource),
        new(PetModels.Pet3032.Id, MachineCatalogConstants.Pet3032ResourceKey, MachineCatalogConstants.Pet3032ImageResource),
        new(PetModels.Pet3032B.Id, MachineCatalogConstants.Pet3032BResourceKey),
        new(PetModels.Pet4016.Id, MachineCatalogConstants.Pet4016ResourceKey, MachineCatalogConstants.Pet4032ImageResource),
        new(PetModels.Pet4032.Id, MachineCatalogConstants.Pet4032ResourceKey, MachineCatalogConstants.Pet4032ImageResource),
        new(PetModels.Pet4032B.Id, MachineCatalogConstants.Pet4032BResourceKey, MachineCatalogConstants.Pet4032ImageResource),
        new(PetModels.Pet8032.Id, MachineCatalogConstants.Pet8032ResourceKey, MachineCatalogConstants.Pet8032ImageResource),
        new(PetModels.Pet8096.Id, MachineCatalogConstants.Pet8096ResourceKey, MachineCatalogConstants.Pet8032ImageResource),
        new(PetModels.Pet8296.Id, MachineCatalogConstants.Pet8296ResourceKey, MachineCatalogConstants.Pet8296ImageResource),
        new(Plus4Models.Plus4.Id, MachineCatalogConstants.Plus4ResourceKey, MachineCatalogConstants.Plus4ImageResource),
        new(PetModels.SuperPet.Id, MachineCatalogConstants.SuperPetResourceKey, MachineCatalogConstants.SuperPetImageResource),
        new(Vic20Models.Vic20.Id, MachineCatalogConstants.Vic20ResourceKey, MachineCatalogConstants.Vic20ImageResource),
        new(Vic20Models.Vic21.Id, MachineCatalogConstants.Vic21ResourceKey, MachineCatalogConstants.Vic20ImageResource),
        new(Plus4Models.V364.Id, MachineCatalogConstants.V364ResourceKey, MachineCatalogConstants.V364ImageResource),
        new(Plus4Models.C232.Id, MachineCatalogConstants.C232ResourceKey, MachineCatalogConstants.C232ImageResource)
    ];
}
