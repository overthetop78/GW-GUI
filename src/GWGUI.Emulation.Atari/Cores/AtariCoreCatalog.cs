namespace GWGUI.Emulation.Atari.Cores;

public static class AtariCoreCatalog
{
    private static readonly IReadOnlyList<AtariCoreCatalogEntry> Entries =
    [
        AtariCoreCatalogFunctions.Create(AtariCoreKind.Hatari, AtariCoreCatalogConstants.HatariId,
            AtariCoreIdentityConstants.Hatari, AtariCoreCatalogConstants.HatariDllName,
            AtariCoreCatalogConstants.HatariSource, AtariCoreCatalogConstants.HatariRevision,
            AtariMachineModel.St, AtariMachineModel.Stf, AtariMachineModel.Stfm, AtariMachineModel.MegaSt,
            AtariMachineModel.Ste, AtariMachineModel.MegaSte, AtariMachineModel.Tt, AtariMachineModel.Falcon),
        AtariCoreCatalogFunctions.Create(AtariCoreKind.Atari800, AtariCoreCatalogConstants.Atari800Id,
            AtariCoreIdentityConstants.Atari800, AtariCoreCatalogConstants.Atari800DllName,
            AtariCoreCatalogConstants.Atari800Source, AtariCoreCatalogConstants.Atari800Revision,
            AtariMachineModel.Atari400, AtariMachineModel.Atari800, AtariMachineModel.Atari800Xl,
            AtariMachineModel.Atari130Xe, AtariMachineModel.ModernXlXe320K, AtariMachineModel.ModernXlXe576K,
            AtariMachineModel.ModernXlXe1088K, AtariMachineModel.Xegs, AtariMachineModel.Atari5200),
        AtariCoreCatalogFunctions.Create(AtariCoreKind.Stella, AtariCoreCatalogConstants.StellaId,
            AtariCoreIdentityConstants.Stella, AtariCoreCatalogConstants.StellaDllName,
            AtariCoreCatalogConstants.StellaSource, AtariCoreCatalogConstants.StellaRevision,
            AtariMachineModel.Atari2600),
        AtariCoreCatalogFunctions.Create(AtariCoreKind.ProSystem, AtariCoreCatalogConstants.ProSystemId,
            AtariCoreIdentityConstants.ProSystem, AtariCoreCatalogConstants.ProSystemDllName,
            AtariCoreCatalogConstants.ProSystemSource, AtariCoreCatalogConstants.ProSystemRevision,
            AtariMachineModel.Atari7800),
        AtariCoreCatalogFunctions.Create(AtariCoreKind.BeetleLynx, AtariCoreCatalogConstants.BeetleLynxId,
            AtariCoreIdentityConstants.BeetleLynx, AtariCoreCatalogConstants.BeetleLynxDllName,
            AtariCoreCatalogConstants.BeetleLynxSource, AtariCoreCatalogConstants.BeetleLynxRevision,
            AtariMachineModel.Lynx),
        AtariCoreCatalogFunctions.Create(AtariCoreKind.VirtualJaguar, AtariCoreCatalogConstants.VirtualJaguarId,
            AtariCoreIdentityConstants.VirtualJaguar, AtariCoreCatalogConstants.VirtualJaguarDllName,
            AtariCoreCatalogConstants.VirtualJaguarSource, AtariCoreCatalogConstants.VirtualJaguarRevision,
            AtariMachineModel.Jaguar, AtariMachineModel.JaguarCd)
    ];

    private static readonly IReadOnlyDictionary<AtariCoreKind, AtariCoreCatalogEntry> ByKind =
        Entries.ToDictionary(entry => entry.Kind);
    private static readonly IReadOnlyDictionary<AtariMachineModel, AtariCoreKind> ByModel =
        AtariCoreCatalogFunctions.CreateModelAssociations(Entries);

    public static IReadOnlyList<AtariCoreCatalogEntry> All => Entries;

    public static AtariCoreCatalogEntry Get(AtariCoreKind kind) => ByKind.TryGetValue(kind, out var entry)
        ? entry
        : throw new ArgumentOutOfRangeException(nameof(kind), kind, null);

    public static AtariCoreCatalogEntry Get(AtariMachineModel model) => ByModel.TryGetValue(model, out var kind)
        ? Get(kind)
        : throw new ArgumentOutOfRangeException(nameof(model), model, AtariCoreCatalogErrors.MissingModel);

    public static AtariCoreInstallationPaths GetInstallationPaths(AtariCoreKind kind,
        string installationRoot, string version) =>
        AtariCoreCatalogFunctions.GetInstallationPaths(Get(kind), installationRoot, version);
}
