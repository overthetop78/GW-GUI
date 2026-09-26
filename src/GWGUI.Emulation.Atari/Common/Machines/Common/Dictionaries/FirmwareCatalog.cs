namespace GWGUI.Emulation.Atari.Common.Machines.Common.Dictionaries;

public static class FirmwareCatalog
{
    private static readonly IReadOnlyList<MachineModel> StandardStModels = FirmwareFunctions.Values(
        MachineModel.St, MachineModel.Stf, MachineModel.Stfm);
    private static readonly IReadOnlyList<MachineModel> StandardAndMegaStModels = FirmwareFunctions.Values(
        MachineModel.St, MachineModel.Stf, MachineModel.Stfm, MachineModel.MegaSt);
    private static readonly IReadOnlyList<MachineModel> SteModels =
        FirmwareFunctions.Values(MachineModel.Ste);
    private static readonly IReadOnlyList<MachineModel> SteAndMegaSteModels =
        FirmwareFunctions.Values(MachineModel.Ste, MachineModel.MegaSte);
    private static readonly IReadOnlyList<MachineModel> BasicFirmwareModels = FirmwareFunctions.Values(
        MachineModel.Atari800, MachineModel.Atari800Xl,
        MachineModel.Atari130Xe, MachineModel.Xegs, MachineModel.XlXe);
    private static readonly IReadOnlyList<MachineModel> XlXeModels = FirmwareFunctions.Values(
        MachineModel.Atari800Xl, MachineModel.Atari130Xe, MachineModel.Xegs,
        MachineModel.XlXe);
    private static readonly IReadOnlyList<MachineModel> Atari400Model =
        FirmwareFunctions.Values(MachineModel.Atari400);
    private static readonly IReadOnlyList<MachineModel> Atari800Model =
        FirmwareFunctions.Values(MachineModel.Atari800);
    private static readonly IReadOnlyList<StRegion> AllTosRegions =
        FirmwareFunctions.Values(Enum.GetValues<StRegion>());
    private static readonly IReadOnlyList<StRegion> NoRegions =
        FirmwareFunctions.Values<StRegion>();
    private static readonly IReadOnlyList<FirmwareFingerprint> NoFingerprints =
        FirmwareFunctions.Values<FirmwareFingerprint>();

    private static readonly IReadOnlyList<FirmwareDefinition> Definitions = FirmwareFunctions.Values(
        FirmwareFunctions.Tos(StModelConstants.Tos100, StandardStModels, AllTosRegions),
        FirmwareFunctions.Tos(StModelConstants.Tos102, StandardAndMegaStModels, AllTosRegions,
            new FirmwareFingerprint(FirmwareHashAlgorithm.Md5,
            FirmwareConstants.Tos102UnitedStatesMd5, StRegion.UnitedStates)),
        FirmwareFunctions.Tos(StModelConstants.Tos104, StandardAndMegaStModels, AllTosRegions),
        FirmwareFunctions.Tos(StModelConstants.Tos106, SteModels, AllTosRegions),
        FirmwareFunctions.Tos(StModelConstants.Tos162, SteModels, AllTosRegions),
        FirmwareFunctions.Tos(StModelConstants.Tos205, SteAndMegaSteModels, AllTosRegions),
        FirmwareFunctions.Tos(StModelConstants.Tos206, FirmwareFunctions.Values(
            MachineModel.St, MachineModel.Stf, MachineModel.Stfm, MachineModel.MegaSt,
            MachineModel.Ste, MachineModel.MegaSte), AllTosRegions),
        FirmwareFunctions.Tos(StModelConstants.Tos301, FirmwareFunctions.Values(MachineModel.Tt), AllTosRegions),
        FirmwareFunctions.Tos(StModelConstants.Tos305, FirmwareFunctions.Values(MachineModel.Tt), AllTosRegions),
        FirmwareFunctions.Tos(StModelConstants.Tos306, FirmwareFunctions.Values(MachineModel.Tt), AllTosRegions),
        FirmwareFunctions.Tos(StModelConstants.Tos400, FirmwareFunctions.Values(MachineModel.Falcon), AllTosRegions),
        FirmwareFunctions.Tos(StModelConstants.Tos401, FirmwareFunctions.Values(MachineModel.Falcon), AllTosRegions),
        FirmwareFunctions.Tos(StModelConstants.Tos402, FirmwareFunctions.Values(MachineModel.Falcon), AllTosRegions),
        FirmwareFunctions.Tos(StModelConstants.Tos404, FirmwareFunctions.Values(MachineModel.Falcon), AllTosRegions),
        FirmwareFunctions.ReplaceableRevision(FirmwareConstants.Atari400OsAId,
            FirmwareCategory.AtariSystemOs, FirmwareCatalogConstants.RevAPAL, FirmwareConstants.AtariOsAFileName,
            FirmwareConstants.AtariOsAMd5, Atari400Model, NoRegions),
        FirmwareFunctions.ReplaceableRevision(FirmwareConstants.Atari400OsANtscId,
            FirmwareCategory.AtariSystemOs, FirmwareCatalogConstants.RevANTSC, FirmwareConstants.AtariOsAFileName,
            FirmwareConstants.AtariOsANtscMd5, Atari400Model, NoRegions),
        FirmwareFunctions.ReplaceableRevision(FirmwareConstants.Atari400OsBId,
            FirmwareCategory.AtariSystemOs, FirmwareCatalogConstants.RevBNTSC, FirmwareConstants.AtariOsBFileName,
            FirmwareConstants.AtariOsBMd5, Atari400Model, NoRegions),
        FirmwareFunctions.ReplaceableRevision(FirmwareCatalogConstants.Atari400OsbPcXformerPatched,
            FirmwareCategory.AtariSystemOs, FirmwareCatalogConstants.RevBNTSCPCXformerPatched,
            FirmwareConstants.AtariOsBFileName, FirmwareConstants.AtariOsBPatchedMd5,
            Atari400Model, NoRegions),
        FirmwareFunctions.ReplaceableRevision(FirmwareConstants.AtariOsAId,
            FirmwareCategory.AtariOsA, FirmwareCatalogConstants.RevAPAL, FirmwareConstants.AtariOsAFileName,
            FirmwareConstants.AtariOsAMd5,
            Atari800Model, NoRegions),
        FirmwareFunctions.ReplaceableRevision(FirmwareConstants.AtariOsANtscId,
            FirmwareCategory.AtariOsA, FirmwareCatalogConstants.RevANTSC, FirmwareConstants.AtariOsAFileName,
            FirmwareConstants.AtariOsANtscMd5,
            Atari800Model, NoRegions),
        FirmwareFunctions.ReplaceableRevision(FirmwareConstants.AtariOsBId,
            FirmwareCategory.AtariOsB, FirmwareCatalogConstants.RevBNTSC, FirmwareConstants.AtariOsBFileName,
            FirmwareConstants.AtariOsBMd5,
            Atari800Model, NoRegions),
        FirmwareFunctions.ReplaceableRevision(FirmwareConstants.AtariOsBPatchedId,
            FirmwareCategory.AtariOsB, FirmwareCatalogConstants.RevBNTSCPCXformerPatched, FirmwareConstants.AtariOsBFileName,
            FirmwareConstants.AtariOsBPatchedMd5,
            Atari800Model, NoRegions),
        FirmwareFunctions.ReplaceableRevision(FirmwareConstants.AtariXlOsId, FirmwareCategory.AtariXlOs,
            FirmwareCatalogConstants.BB01R2, FirmwareConstants.AtariXlOsFileName, FirmwareConstants.AtariXlOsMd5, XlXeModels, NoRegions),
        FirmwareFunctions.ReplaceableRevision(FirmwareCatalogConstants.AtariXlXeOsV3, FirmwareCategory.AtariXlOs,
            FirmwareCatalogConstants.BB01R3, FirmwareConstants.AtariXlOsFileName, FirmwareConstants.AtariXlXeOsV3Md5, XlXeModels, NoRegions),
        FirmwareFunctions.ReplaceableRevision(FirmwareConstants.AtariXlXeOsV4Id, FirmwareCategory.AtariXlOs,
            FirmwareCatalogConstants.BB01R4, FirmwareConstants.AtariXlXeOsV4FileName, FirmwareConstants.AtariXlXeOsV4Md5, XlXeModels, NoRegions),
        FirmwareFunctions.ReplaceableRevision(FirmwareCatalogConstants.AtariXlXeOsR59, FirmwareCategory.AtariXlOs,
            FirmwareCatalogConstants.BB01R59, FirmwareConstants.AtariXlOsFileName, FirmwareConstants.AtariXlXeOsR59Md5, XlXeModels, NoRegions),
        FirmwareFunctions.ReplaceableRevision(FirmwareCatalogConstants.AtariXlXeOsR59a, FirmwareCategory.AtariXlOs,
            FirmwareCatalogConstants.BB01R59A, FirmwareConstants.AtariXlOsFileName, FirmwareConstants.AtariXlXeOsR59AMd5, XlXeModels, NoRegions),
        FirmwareFunctions.Replaceable(FirmwareConstants.AtariBasicId, FirmwareCategory.AtariBasic,
            FirmwareConstants.AtariBasicFileName, FirmwareConstants.AtariBasicMd5,
            BasicFirmwareModels, NoRegions),
        FirmwareFunctions.Replaceable(FirmwareConstants.Atari5200Id, FirmwareCategory.Atari5200Bios,
            FirmwareConstants.Atari5200FileName, FirmwareConstants.Atari5200Md5,
            FirmwareFunctions.Values(MachineModel.Atari5200), NoRegions),
        FirmwareFunctions.ReplaceableRevision(FirmwareCatalogConstants.Atari5200RevisionA, FirmwareCategory.Atari5200Bios,
            FirmwareCatalogConstants.RevisionA, FirmwareConstants.Atari5200FileName, FirmwareConstants.Atari5200RevisionAMd5,
            FirmwareFunctions.Values(MachineModel.Atari5200), NoRegions),
        FirmwareFunctions.Replaceable(FirmwareConstants.AtariXegsId, FirmwareCategory.AtariXegsBios,
            FirmwareConstants.AtariXegsFileName, FirmwareConstants.AtariXegsMd5,
            FirmwareFunctions.Values(MachineModel.Xegs), NoRegions),
        FirmwareFunctions.None(FirmwareConstants.Atari2600NoBiosId, MachineModel.Atari2600,
            FirmwareEvidence.StellaCoreInformation, NoRegions, NoFingerprints),
        FirmwareFunctions.External(FirmwareConstants.Atari7800Id, FirmwareCategory.Atari7800Bios,
            FirmwareConstants.Atari7800FileName, FirmwareConstants.Atari7800Md5,
            FirmwareProvision.OptionalExternal, MachineModel.Atari7800,
            FirmwareEvidence.ProSystemCoreInformation, NoRegions),
        FirmwareFunctions.ExternalRevision(FirmwareCatalogConstants.Atari7800Europe, FirmwareCategory.Atari7800Bios,
            FirmwareCatalogConstants.Europe, FirmwareConstants.Atari7800FileName, FirmwareConstants.Atari7800EuropeMd5,
            FirmwareProvision.OptionalExternal, MachineModel.Atari7800,
            FirmwareEvidence.ProSystemCoreInformation, NoRegions),
        FirmwareFunctions.External(FirmwareConstants.LynxBootId, FirmwareCategory.LynxBootRom,
            FirmwareConstants.LynxBootFileName, FirmwareConstants.LynxBootMd5,
            FirmwareProvision.RequiredExternal, MachineModel.Lynx,
            FirmwareEvidence.BeetleLynxCoreInformation, NoRegions),
        new FirmwareDefinition(FirmwareConstants.JaguarBootId, FirmwareCategory.JaguarBootRom,
            FirmwareCatalogConstants.World, FirmwareConstants.JaguarBootFileName, 131072, FirmwareProvision.Embedded,
            FirmwareDistribution.UserSuppliedCopyrighted, FirmwareEvidence.VirtualJaguarCoreInformation,
            FirmwareFunctions.Values(MachineModel.Jaguar, MachineModel.JaguarCd), NoRegions,
            FirmwareFunctions.Values(new FirmwareFingerprint(FirmwareHashAlgorithm.Md5,
                FirmwareConstants.JaguarBootMd5))),
        FirmwareFunctions.JaguarCd(FirmwareConstants.JaguarCdRetailId, FirmwareConstants.JaguarCdRetailFileName, NoRegions, NoFingerprints),
        FirmwareFunctions.JaguarCd(FirmwareConstants.JaguarCdDeveloperId, FirmwareConstants.JaguarCdDeveloperFileName, NoRegions, NoFingerprints),
        FirmwareFunctions.None(FirmwareConstants.JaguarCdDriveFirmwareId, MachineModel.JaguarCd,
            FirmwareEvidence.VirtualJaguarCoreInformation, NoRegions, NoFingerprints));

    private static readonly IReadOnlyDictionary<string, FirmwareDefinition> ById =
        FirmwareFunctions.Index(Definitions);

    public static IReadOnlyList<FirmwareDefinition> All => Definitions;
    public static FirmwareDefinition Get(string id) => ById.TryGetValue(id, out var definition)
        ? definition
        : throw new ArgumentOutOfRangeException(nameof(id), id, ErrorMessages.UnknownFirmware);
    public static IReadOnlyList<FirmwareDefinition> ForModel(MachineModel model) =>
        Definitions.Where(definition => definition.Models.Contains(model)).ToArray();

}
