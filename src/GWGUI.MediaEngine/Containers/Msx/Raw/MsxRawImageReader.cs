using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Fat12;
using GWGUI.MediaEngine.Geometries.Msx;
using GWGUI.MediaEngine.Recognition.Msx;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.SectorImages.Builders;
using GWGUI.MediaEngine.SectorImages.Reading;

namespace GWGUI.MediaEngine.Containers.Msx.Raw;

/// <summary>Lit et valide une image sectorielle brute MSX-DOS.</summary>
public sealed class MsxRawImageReader : ISectorImageReader
{
    /// <summary>Indique si le chemin porte l'extension DSK utilisée comme indice de reconnaissance.</summary>
    /// <param name="path">Chemin du fichier à examiner.</param>
    /// <returns><see langword="true"/> lorsque le chemin se termine par l'extension DSK ; sinon <see langword="false"/>.</returns>
    public bool CanRead(string path) => Path.GetExtension(path).Equals(DiskImageFileExtensions.Dsk, StringComparison.OrdinalIgnoreCase);

    /// <summary>Lit l'image, valide son BPB MSX-DOS et reconstruit ses secteurs dans l'ordre CHS linéaire décrit par sa géométrie.</summary>
    /// <param name="path">Chemin de l'image brute à lire.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la lecture et la construction sectorielle.</param>
    /// <returns>L'image sectorielle MSX-DOS validée et associée à sa géométrie.</returns>
    /// <exception cref="InvalidDataException">Le secteur d'amorçage n'est pas reconnu comme MSX-DOS, ou la capacité et le descripteur de média ne correspondent à aucune géométrie prise en charge.</exception>
    /// <remarks>Les capacités et tailles sectorielles manipulées sont exprimées en octets. Les adresses sectorielles utilisent une numérotation commençant à un.</remarks>
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
