namespace GWGUI.Emulation.Atari.Dictionaries;

public static class AtariCoreCatalog
{
    private static readonly IReadOnlyList<AtariCoreCatalogEntry> Entries =
    [
        AtariCoreCatalogFunctions.Create(AtariEmulator.Hatari, AtariCoreCatalogConstants.HatariId,
            AtariCoreIdentityConstants.Hatari, AtariCoreCatalogConstants.HatariDllName,
            AtariCoreCatalogConstants.HatariSource, AtariCoreCatalogConstants.HatariRevision,
            AtariMachineModel.St, AtariMachineModel.Stf, AtariMachineModel.Stfm, AtariMachineModel.MegaSt,
            AtariMachineModel.Ste, AtariMachineModel.MegaSte, AtariMachineModel.Tt, AtariMachineModel.Falcon),
        AtariCoreCatalogFunctions.Create(AtariEmulator.Atari800, AtariCoreCatalogConstants.Atari800Id,
            AtariCoreIdentityConstants.Atari800, AtariCoreCatalogConstants.Atari800DllName,
            AtariCoreCatalogConstants.Atari800Source, AtariCoreCatalogConstants.Atari800Revision,
            AtariMachineModel.Atari400, AtariMachineModel.Atari800, AtariMachineModel.Atari800Xl,
            AtariMachineModel.Atari130Xe, AtariMachineModel.Xegs, AtariMachineModel.XlXe,
            AtariMachineModel.Atari5200),
        AtariCoreCatalogFunctions.Create(AtariEmulator.Stella, AtariCoreCatalogConstants.StellaId,
            AtariCoreIdentityConstants.Stella, AtariCoreCatalogConstants.StellaDllName,
            AtariCoreCatalogConstants.StellaSource, AtariCoreCatalogConstants.StellaRevision,
            AtariMachineModel.Atari2600),
        AtariCoreCatalogFunctions.Create(AtariEmulator.ProSystem, AtariCoreCatalogConstants.ProSystemId,
            AtariCoreIdentityConstants.ProSystem, AtariCoreCatalogConstants.ProSystemDllName,
            AtariCoreCatalogConstants.ProSystemSource, AtariCoreCatalogConstants.ProSystemRevision,
            AtariMachineModel.Atari7800),
        AtariCoreCatalogFunctions.Create(AtariEmulator.BeetleLynx, AtariCoreCatalogConstants.BeetleLynxId,
            AtariCoreIdentityConstants.BeetleLynx, AtariCoreCatalogConstants.BeetleLynxDllName,
            AtariCoreCatalogConstants.BeetleLynxSource, AtariCoreCatalogConstants.BeetleLynxRevision,
            AtariMachineModel.Lynx),
        AtariCoreCatalogFunctions.Create(AtariEmulator.VirtualJaguar, AtariCoreCatalogConstants.VirtualJaguarId,
            AtariCoreIdentityConstants.VirtualJaguar, AtariCoreCatalogConstants.VirtualJaguarDllName,
            AtariCoreCatalogConstants.VirtualJaguarSource, AtariCoreCatalogConstants.VirtualJaguarRevision,
            AtariMachineModel.Jaguar, AtariMachineModel.JaguarCd)
    ];

    private static readonly IReadOnlyDictionary<AtariEmulator, AtariCoreCatalogEntry> ByEmulator =
        Entries.ToDictionary(entry => entry.Emulator);
    private static readonly IReadOnlyDictionary<AtariMachineModel, AtariEmulator> ByModel =
        AtariCoreCatalogFunctions.CreateModelAssociations(Entries);

    public static IReadOnlyList<AtariCoreCatalogEntry> All => Entries;

    public static AtariCoreCatalogEntry Get(AtariEmulator emulator) => ByEmulator.TryGetValue(emulator, out var entry)
        ? entry
        : throw new ArgumentOutOfRangeException(nameof(emulator), emulator, null);

    public static AtariCoreCatalogEntry Get(AtariMachineModel model) => ByModel.TryGetValue(model, out var emulator)
        ? Get(emulator)
        : throw new ArgumentOutOfRangeException(nameof(model), model, AtariCoreCatalogErrors.MissingModel);

    public static AtariCoreInstallationPaths GetInstallationPaths(AtariEmulator emulator,
        string installationRoot, string version) =>
        AtariCoreCatalogFunctions.GetInstallationPaths(Get(emulator), installationRoot, version);

    public static string GetActiveManifestPath(AtariEmulator emulator, string installationRoot) =>
        AtariCoreCatalogFunctions.GetActiveManifestPath(Get(emulator), installationRoot);
}
