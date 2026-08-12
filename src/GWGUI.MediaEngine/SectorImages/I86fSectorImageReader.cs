using GWGUI.MediaEngine.Containers.I86f;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Flux.Conversion;
using GWGUI.MediaEngine.Images;
using GWGUI.MediaEngine.Geometries.Ibm;
using GWGUI.MediaEngine.Reconstruction.Iso;

namespace GWGUI.MediaEngine.SectorImages;

/// <summary>Décode les pistes d'un conteneur 86F et construit leur image sectorielle ISO FM ou MFM.</summary>
/// <param name="reader">Lecteur du conteneur et de ses cellules de bits.</param>
/// <param name="decoders">Registre des décodeurs de flux ISO.</param>
public sealed class I86fSectorImageReader(I86fReader reader, FluxDecoderRegistry decoders)
{
    /// <summary>Lit un conteneur 86F, choisit le décodeur de chaque piste et construit l'image sectorielle.</summary>
    /// <param name="path">Chemin du fichier 86F.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la lecture ou le parcours des pistes.</param>
    /// <returns>L'image sectorielle reconstruite.</returns>
    /// <exception cref="IOException">Une erreur d'entrée-sortie survient pendant la lecture.</exception>
    /// <exception cref="InvalidDataException">Le conteneur est invalide ou aucun secteur n'est décodable.</exception>
    /// <exception cref="OverflowException">Un calcul de taille ou de position dépasse la capacité d'un entier.</exception>
    /// <exception cref="OperationCanceledException">L'opération est annulée.</exception>
    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var container = await reader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        var candidates = new Dictionary<SectorAddress, List<IsoSectorCandidate>>();
        foreach (var track in container.Tracks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var revolution = I86fBitCellFluxConverter.Convert(track.Bits);
            if (revolution is null) continue;
            var decoderId = DecoderIdFor(track.Flags);
            var decoded = decoders.Decode(decoderId, revolution.Flux);
            foreach (var sector in decoded.Sectors)
            {
                if (sector.Data is null || sector.Number < 0) continue;
                var address = new SectorAddress(sector.Cylinder, sector.Head, sector.Number);
                if (!candidates.TryGetValue(address, out var values)) candidates[address] = values = [];
                values.Add(new(sector, Revolution: 0, SourceTrack: track.LogicalIndex));
            }
        }
        if (candidates.Count == 0) throw I86fSectorImageExceptions.NoDecodableSectors(container.Tracks.Count);
        var measured = IsoSectorImageBuilder.Measure(candidates);
        var formatId = measured.SectorSize == 512 ? IbmPcGeometryCatalog.FormatIdForGeometry(measured.Cylinders, measured.Heads, measured.SectorsPerTrack, measured.SectorSize) : DiskImageFormatIds.I86fFromGeometry(measured.SectorSize, measured.Cylinders, measured.Heads, measured.SectorsPerTrack);
        return IsoSectorImageBuilder.CreateUniform(formatId, candidates, measured.SectorSize, measured.Cylinders, measured.Heads, measured.SectorsPerTrack, address => measured.ZeroBased ? Array.IndexOf(measured.SectorOrder, address.Number) : address.Number - 1, capacity: (long)measured.Cylinders * measured.Heads * measured.SectorsPerTrack * measured.SectorSize);
    }

    /// <summary>Sélectionne le décodeur ISO correspondant aux drapeaux d'une piste 86F.</summary>
    /// <param name="flags">Drapeaux de la piste.</param>
    /// <returns>L'identifiant du décodeur ISO MFM ou ISO FM.</returns>
    internal static string DecoderIdFor(I86fTrackFlags flags) => (flags & I86fTrackFlags.EncodingMask) == I86fTrackFlags.MfmEncoding ? FluxCodecIds.IsoMfm : FluxCodecIds.IsoFm;
}
