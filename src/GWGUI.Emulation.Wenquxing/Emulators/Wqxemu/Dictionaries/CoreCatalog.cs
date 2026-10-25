using EmulatorCatalogFunctions = GWGUI.Emulation.Wenquxing.Common.Functions.EmulatorCatalogFunctions;
using GWGUI.Emulation;
using GWGUI.Emulation.Wenquxing.Emulators.Wqxemu.Constants;

namespace GWGUI.Emulation.Wenquxing.Emulators.Wqxemu.Dictionaries;
internal static class CoreCatalog
{
    internal static readonly EmulatorCatalogEntry Entry = new(
        Emulator.Wqxemu,
        EmulatorConstants.Id,
        EmulatorConstants.LibraryName,
        EmulatorConstants.DllName,
        EmulatorConstants.DllName + EmulatorCatalogConstants.ArchiveExtension,
        new Uri(EmulatorConstants.ArchiveUri),
        new Uri(EmulatorConstants.SourceUri),
        EmulatorConstants.InspectedRevision,
        Enum.GetValues<MachineModel>().ToHashSet(),
        EmulatorConstants.DescriptionResourceKey
    );
    internal static EmulatorCatalogEntry Get(Emulator emulator) => emulator == Emulator.Wqxemu ? Entry : throw new ArgumentOutOfRangeException(nameof(emulator));
    internal static CoreInstallationPaths GetInstallationPaths(Emulator emulator, string installationRoot, string version) =>
        EmulatorCatalogFunctions.GetInstallationPaths(Get(emulator), installationRoot, version);

    internal static string GetActiveManifestPath(Emulator emulator, string installationRoot) =>
        EmulatorCatalogFunctions.GetActiveManifestPath(Get(emulator), installationRoot);
}