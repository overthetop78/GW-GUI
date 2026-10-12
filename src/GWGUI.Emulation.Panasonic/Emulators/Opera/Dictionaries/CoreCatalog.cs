using System.IO;
using GWGUI.Emulation.Panasonic.Emulators.Opera.Constants;
using GWGUI.Emulation.Panasonic.Emulators.Opera.Contracts;

namespace GWGUI.Emulation.Panasonic.Emulators.Opera.Dictionaries;

internal static class CoreCatalog
{
    internal static readonly EmulatorCatalogEntry Entry = new(
        Emulator.Opera,
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
    internal static EmulatorCatalogEntry Get(Emulator emulator) => emulator == Emulator.Opera ? Entry : throw new ArgumentOutOfRangeException(nameof(emulator));
    internal static CoreInstallationPaths GetInstallationPaths(Emulator emulator, string installationRoot, string version)
    {
        Get(emulator);
        ArgumentException.ThrowIfNullOrWhiteSpace(installationRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(version);
        if (version != Path.GetFileName(version) || version is "." or ".." || version.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) throw new ArgumentException(nameof(version));
        var directory = Path.Combine(Path.GetFullPath(installationRoot), Entry.Id, version);
        return new CoreInstallationPaths(directory, Path.Combine(directory, Entry.DllName), Path.Combine(directory, EmulatorCatalogConstants.ManifestFileName));
    }
    internal static string GetActiveManifestPath(Emulator emulator, string installationRoot)
    {
        Get(emulator);
        ArgumentException.ThrowIfNullOrWhiteSpace(installationRoot);
        return Path.Combine(Path.GetFullPath(installationRoot), Entry.Id, EmulatorCatalogConstants.ActiveManifestFileName);
    }
}
