namespace GWGUI.Emulation.Atari;

public static class AtariFirmwareCatalog
{
    private static readonly IReadOnlyList<AtariMachineModel> StandardStModels = AtariFirmwareFunctions.Values(
        AtariMachineModel.St, AtariMachineModel.Stf, AtariMachineModel.Stfm);
    private static readonly IReadOnlyList<AtariMachineModel> StandardAndMegaStModels = AtariFirmwareFunctions.Values(
        AtariMachineModel.St, AtariMachineModel.Stf, AtariMachineModel.Stfm, AtariMachineModel.MegaSt);
    private static readonly IReadOnlyList<AtariMachineModel> SteModels =
        AtariFirmwareFunctions.Values(AtariMachineModel.Ste);
    private static readonly IReadOnlyList<AtariMachineModel> SteAndMegaSteModels =
        AtariFirmwareFunctions.Values(AtariMachineModel.Ste, AtariMachineModel.MegaSte);
    private static readonly IReadOnlyList<AtariMachineModel> BasicFirmwareModels = AtariFirmwareFunctions.Values(
        AtariMachineModel.Atari800, AtariMachineModel.Atari800Xl,
        AtariMachineModel.Atari130Xe, AtariMachineModel.Xegs, AtariMachineModel.XlXe);
    private static readonly IReadOnlyList<AtariMachineModel> XlXeModels = AtariFirmwareFunctions.Values(
        AtariMachineModel.Atari800Xl, AtariMachineModel.Atari130Xe, AtariMachineModel.Xegs,
        AtariMachineModel.XlXe);
    private static readonly IReadOnlyList<AtariMachineModel> Atari400Model =
        AtariFirmwareFunctions.Values(AtariMachineModel.Atari400);
    private static readonly IReadOnlyList<AtariMachineModel> Atari800Model =
        AtariFirmwareFunctions.Values(AtariMachineModel.Atari800);
    private static readonly IReadOnlyList<AtariStRegion> AllTosRegions =
        AtariFirmwareFunctions.Values(Enum.GetValues<AtariStRegion>());
    private static readonly IReadOnlyList<AtariStRegion> NoRegions =
        AtariFirmwareFunctions.Values<AtariStRegion>();
    private static readonly IReadOnlyList<AtariFirmwareFingerprint> NoFingerprints =
        AtariFirmwareFunctions.Values<AtariFirmwareFingerprint>();

    private static readonly IReadOnlyList<AtariFirmwareDefinition> Definitions = AtariFirmwareFunctions.Values(
        AtariFirmwareFunctions.Tos(AtariStModelConstants.Tos100, StandardStModels, AllTosRegions),
        AtariFirmwareFunctions.Tos(AtariStModelConstants.Tos102, StandardAndMegaStModels, AllTosRegions,
            new AtariFirmwareFingerprint(AtariFirmwareHashAlgorithm.Md5,
            AtariFirmwareConstants.Tos102UnitedStatesMd5, AtariStRegion.UnitedStates)),
        AtariFirmwareFunctions.Tos(AtariStModelConstants.Tos104, StandardAndMegaStModels, AllTosRegions),
        AtariFirmwareFunctions.Tos(AtariStModelConstants.Tos106, SteModels, AllTosRegions),
        AtariFirmwareFunctions.Tos(AtariStModelConstants.Tos162, SteModels, AllTosRegions),
        AtariFirmwareFunctions.Tos(AtariStModelConstants.Tos205, SteAndMegaSteModels, AllTosRegions),
        AtariFirmwareFunctions.Tos(AtariStModelConstants.Tos206, AtariFirmwareFunctions.Values(
            AtariMachineModel.St, AtariMachineModel.Stf, AtariMachineModel.Stfm, AtariMachineModel.MegaSt,
            AtariMachineModel.Ste, AtariMachineModel.MegaSte), AllTosRegions),
        AtariFirmwareFunctions.Tos(AtariStModelConstants.Tos301, AtariFirmwareFunctions.Values(AtariMachineModel.Tt), AllTosRegions),
        AtariFirmwareFunctions.Tos(AtariStModelConstants.Tos305, AtariFirmwareFunctions.Values(AtariMachineModel.Tt), AllTosRegions),
        AtariFirmwareFunctions.Tos(AtariStModelConstants.Tos306, AtariFirmwareFunctions.Values(AtariMachineModel.Tt), AllTosRegions),
        AtariFirmwareFunctions.Tos(AtariStModelConstants.Tos400, AtariFirmwareFunctions.Values(AtariMachineModel.Falcon), AllTosRegions),
        AtariFirmwareFunctions.Tos(AtariStModelConstants.Tos401, AtariFirmwareFunctions.Values(AtariMachineModel.Falcon), AllTosRegions),
        AtariFirmwareFunctions.Tos(AtariStModelConstants.Tos402, AtariFirmwareFunctions.Values(AtariMachineModel.Falcon), AllTosRegions),
        AtariFirmwareFunctions.Tos(AtariStModelConstants.Tos404, AtariFirmwareFunctions.Values(AtariMachineModel.Falcon), AllTosRegions),
        AtariFirmwareFunctions.ReplaceableRevision("atari-400-osa-pal",
            AtariFirmwareKind.AtariSystemOs, "Rev. A PAL", AtariFirmwareConstants.AtariOsAFileName,
            AtariFirmwareConstants.AtariOsAMd5, Atari400Model, NoRegions),
        AtariFirmwareFunctions.ReplaceableRevision("atari-400-osa-ntsc",
            AtariFirmwareKind.AtariSystemOs, "Rev. A NTSC", AtariFirmwareConstants.AtariOsAFileName,
            AtariFirmwareConstants.AtariOsANtscMd5, Atari400Model, NoRegions),
        AtariFirmwareFunctions.ReplaceableRevision("atari-400-osb",
            AtariFirmwareKind.AtariSystemOs, "Rev. B NTSC", AtariFirmwareConstants.AtariOsBFileName,
            AtariFirmwareConstants.AtariOsBMd5, Atari400Model, NoRegions),
        AtariFirmwareFunctions.ReplaceableRevision("atari-400-osb-pc-xformer-patched",
            AtariFirmwareKind.AtariSystemOs, "Rev. B NTSC — PC Xformer patched",
            AtariFirmwareConstants.AtariOsBFileName, AtariFirmwareConstants.AtariOsBPatchedMd5,
            Atari400Model, NoRegions),
        AtariFirmwareFunctions.ReplaceableRevision(AtariFirmwareConstants.AtariOsAId,
            AtariFirmwareKind.AtariOsA, "Rev. A PAL", AtariFirmwareConstants.AtariOsAFileName,
            AtariFirmwareConstants.AtariOsAMd5,
            Atari800Model, NoRegions),
        AtariFirmwareFunctions.ReplaceableRevision(AtariFirmwareConstants.AtariOsANtscId,
            AtariFirmwareKind.AtariOsA, "Rev. A NTSC", AtariFirmwareConstants.AtariOsAFileName,
            AtariFirmwareConstants.AtariOsANtscMd5,
            Atari800Model, NoRegions),
        AtariFirmwareFunctions.ReplaceableRevision(AtariFirmwareConstants.AtariOsBId,
            AtariFirmwareKind.AtariOsB, "Rev. B NTSC", AtariFirmwareConstants.AtariOsBFileName,
            AtariFirmwareConstants.AtariOsBMd5,
            Atari800Model, NoRegions),
        AtariFirmwareFunctions.ReplaceableRevision(AtariFirmwareConstants.AtariOsBPatchedId,
            AtariFirmwareKind.AtariOsB, "Rev. B NTSC — PC Xformer patched", AtariFirmwareConstants.AtariOsBFileName,
            AtariFirmwareConstants.AtariOsBPatchedMd5,
            Atari800Model, NoRegions),
        AtariFirmwareFunctions.ReplaceableRevision(AtariFirmwareConstants.AtariXlOsId, AtariFirmwareKind.AtariXlOs,
            "BB01R2", AtariFirmwareConstants.AtariXlOsFileName, AtariFirmwareConstants.AtariXlOsMd5, XlXeModels, NoRegions),
        AtariFirmwareFunctions.ReplaceableRevision("atari-xl-xe-os-v3", AtariFirmwareKind.AtariXlOs,
            "BB01R3", AtariFirmwareConstants.AtariXlOsFileName, AtariFirmwareConstants.AtariXlXeOsV3Md5, XlXeModels, NoRegions),
        AtariFirmwareFunctions.ReplaceableRevision(AtariFirmwareConstants.AtariXlXeOsV4Id, AtariFirmwareKind.AtariXlOs,
            "BB01R4", AtariFirmwareConstants.AtariXlXeOsV4FileName, AtariFirmwareConstants.AtariXlXeOsV4Md5, XlXeModels, NoRegions),
        AtariFirmwareFunctions.ReplaceableRevision("atari-xl-xe-os-r59", AtariFirmwareKind.AtariXlOs,
            "BB01R59", AtariFirmwareConstants.AtariXlOsFileName, AtariFirmwareConstants.AtariXlXeOsR59Md5, XlXeModels, NoRegions),
        AtariFirmwareFunctions.ReplaceableRevision("atari-xl-xe-os-r59a", AtariFirmwareKind.AtariXlOs,
            "BB01R59A", AtariFirmwareConstants.AtariXlOsFileName, AtariFirmwareConstants.AtariXlXeOsR59AMd5, XlXeModels, NoRegions),
        AtariFirmwareFunctions.Replaceable(AtariFirmwareConstants.AtariBasicId, AtariFirmwareKind.AtariBasic,
            AtariFirmwareConstants.AtariBasicFileName, AtariFirmwareConstants.AtariBasicMd5,
            BasicFirmwareModels, NoRegions),
        AtariFirmwareFunctions.Replaceable(AtariFirmwareConstants.Atari5200Id, AtariFirmwareKind.Atari5200Bios,
            AtariFirmwareConstants.Atari5200FileName, AtariFirmwareConstants.Atari5200Md5,
            AtariFirmwareFunctions.Values(AtariMachineModel.Atari5200), NoRegions),
        AtariFirmwareFunctions.ReplaceableRevision("atari-5200-revision-a", AtariFirmwareKind.Atari5200Bios,
            "Revision A", AtariFirmwareConstants.Atari5200FileName, AtariFirmwareConstants.Atari5200RevisionAMd5,
            AtariFirmwareFunctions.Values(AtariMachineModel.Atari5200), NoRegions),
        AtariFirmwareFunctions.Replaceable(AtariFirmwareConstants.AtariXegsId, AtariFirmwareKind.AtariXegsBios,
            AtariFirmwareConstants.AtariXegsFileName, AtariFirmwareConstants.AtariXegsMd5,
            AtariFirmwareFunctions.Values(AtariMachineModel.Xegs), NoRegions),
        AtariFirmwareFunctions.None(AtariFirmwareConstants.Atari2600NoBiosId, AtariMachineModel.Atari2600,
            AtariFirmwareEvidence.StellaCoreInformation, NoRegions, NoFingerprints),
        AtariFirmwareFunctions.External(AtariFirmwareConstants.Atari7800Id, AtariFirmwareKind.Atari7800Bios,
            AtariFirmwareConstants.Atari7800FileName, AtariFirmwareConstants.Atari7800Md5,
            AtariFirmwareProvision.OptionalExternal, AtariMachineModel.Atari7800,
            AtariFirmwareEvidence.ProSystemCoreInformation, NoRegions),
        AtariFirmwareFunctions.ExternalRevision("atari-7800-europe", AtariFirmwareKind.Atari7800Bios,
            "Europe", AtariFirmwareConstants.Atari7800FileName, AtariFirmwareConstants.Atari7800EuropeMd5,
            AtariFirmwareProvision.OptionalExternal, AtariMachineModel.Atari7800,
            AtariFirmwareEvidence.ProSystemCoreInformation, NoRegions),
        AtariFirmwareFunctions.External(AtariFirmwareConstants.LynxBootId, AtariFirmwareKind.LynxBootRom,
            AtariFirmwareConstants.LynxBootFileName, AtariFirmwareConstants.LynxBootMd5,
            AtariFirmwareProvision.RequiredExternal, AtariMachineModel.Lynx,
            AtariFirmwareEvidence.BeetleLynxCoreInformation, NoRegions),
        new AtariFirmwareDefinition(AtariFirmwareConstants.JaguarBootId, AtariFirmwareKind.JaguarBootRom,
            "World", AtariFirmwareConstants.JaguarBootFileName, 131072, AtariFirmwareProvision.Embedded,
            AtariFirmwareDistribution.UserSuppliedCopyrighted, AtariFirmwareEvidence.VirtualJaguarCoreInformation,
            AtariFirmwareFunctions.Values(AtariMachineModel.Jaguar, AtariMachineModel.JaguarCd), NoRegions,
            AtariFirmwareFunctions.Values(new AtariFirmwareFingerprint(AtariFirmwareHashAlgorithm.Md5,
                AtariFirmwareConstants.JaguarBootMd5))),
        AtariFirmwareFunctions.JaguarCd(AtariFirmwareConstants.JaguarCdRetailId, AtariFirmwareConstants.JaguarCdRetailFileName, NoRegions, NoFingerprints),
        AtariFirmwareFunctions.JaguarCd(AtariFirmwareConstants.JaguarCdDeveloperId, AtariFirmwareConstants.JaguarCdDeveloperFileName, NoRegions, NoFingerprints),
        AtariFirmwareFunctions.None(AtariFirmwareConstants.JaguarCdDriveFirmwareId, AtariMachineModel.JaguarCd,
            AtariFirmwareEvidence.VirtualJaguarCoreInformation, NoRegions, NoFingerprints));

    private static readonly IReadOnlyDictionary<string, AtariFirmwareDefinition> ById =
        AtariFirmwareFunctions.Index(Definitions);

    public static IReadOnlyList<AtariFirmwareDefinition> All => Definitions;
    public static AtariFirmwareDefinition Get(string id) => ById.TryGetValue(id, out var definition)
        ? definition
        : throw new ArgumentOutOfRangeException(nameof(id), id, AtariErrorMessages.UnknownFirmware);
    public static IReadOnlyList<AtariFirmwareDefinition> ForModel(AtariMachineModel model) =>
        Definitions.Where(definition => definition.Models.Contains(model)).ToArray();

}
