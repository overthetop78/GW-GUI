using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Fat12;
using GWGUI.MediaEngine.Geometries.Msx;
using GWGUI.MediaEngine.Recognition.Msx;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.SectorImages.Builders;

namespace GWGUI.MediaEngine.Containers.Msx.Raw;

/// <summary>Lit et valide une image sectorielle brute MSX-DOS.</summary>
public sealed class MsxRawImageReader
{
    /// <summary>Lit l'image, valide son BPB MSX-DOS et reconstruit ses secteurs dans l'ordre CHS linÃ©aire dÃ©crit par sa gÃ©omÃ©trie.</summary>
    /// <param name="path">Chemin de l'image brute Ã  lire.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la lecture et la construction sectorielle.</param>
    /// <returns>L'image sectorielle MSX-DOS validÃ©e et associÃ©e Ã  sa gÃ©omÃ©trie.</returns>
    /// <exception cref="InvalidDataException">Le secteur d'amorÃ§age n'est pas reconnu comme MSX-DOS, ou la capacitÃ© et le descripteur de mÃ©dia ne correspondent Ã  aucune gÃ©omÃ©trie prise en charge.</exception>
    /// <remarks>Les capacitÃ©s et tailles sectorielles manipulÃ©es sont exprimÃ©es en octets. Les adresses sectorielles utilisent une numÃ©rotation commenÃ§ant Ã  un.</remarks>
    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var data = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        if (!MsxBootSectorProbe.LooksLikeMsx(data)) throw MsxRawImageExceptions.InvalidBootSector(data.Length);
        var mediaDescriptor = data[FatBootSectorLayout.MediaDescriptorOffset];
        var geometry = MsxDiskGeometryCatalog.Find(data.Length, mediaDescriptor) ?? throw MsxRawImageExceptions.UnsupportedGeometry(data.Length, mediaDescriptor);
        var linear = new LinearSectorImageGeometry(FatBootSectorLayout.SectorSize, geometry.Cylinders, geometry.Heads, geometry.SectorsPerTrack, SectorNumbering.OneBased);
        return LinearSectorImageBuilder.Create(data, geometry.FormatId, linear, cancellationToken);
    }
}
