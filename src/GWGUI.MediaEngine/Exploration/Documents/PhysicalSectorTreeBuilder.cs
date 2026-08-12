using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Exploration.Documents;

/// <summary>Construit l'arborescence physique affichée lorsqu'aucun système de fichiers n'est reconnu.</summary>
internal static class PhysicalSectorTreeBuilder
{
    /// <summary>Regroupe les blocs par piste et les ordonne par adresse physique.</summary>
    /// <param name="image">Image dont les blocs disponibles doivent être présentés.</param>
    /// <returns>Répertoires de pistes contenant leurs fichiers de secteurs.</returns>
    public static IReadOnlyList<FileSystemEntry> Build(SectorImage image) => image.AvailableBlocks.GroupBy(block => (block.Address.Cylinder, block.Address.Head)).OrderBy(group => group.Key.Cylinder).ThenBy(group => group.Key.Head).Select(group => new FileSystemEntry(PhysicalSectorEntryNames.Track(group.Key.Cylinder, group.Key.Head), FileSystemEntryKind.Directory, group.Sum(block => (long)block.Data.Count), null, string.Empty, 0, 0, group.All(block => block.IntegrityValid != false), group.OrderBy(block => block.Address.Number).Select(block => new FileSystemEntry(PhysicalSectorEntryNames.Sector(block.Address.Number), FileSystemEntryKind.File, block.Data.Count, null, string.Empty, 0, block.LogicalBlock, block.IntegrityValid != false, [], block.Data)))).ToArray();
}
