using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.AppleDos;
using GWGUI.MediaEngine.FileSystems.Lisa;
using GWGUI.MediaEngine.FileSystems.Macintosh;
using GWGUI.MediaEngine.FileSystems.ProDos;
using GWGUI.MediaEngine.FileSystems.Sos;
using GWGUI.MediaEngine.Geometries.Apple;

namespace GWGUI.MediaEngine.Recognition.Apple;

/// <summary>Examine les indices d'extension, de capacité et de structure des images Apple brutes sans lire de fichier.</summary>
internal static class AppleRawImageProbe
{
    /// <summary>Indique si le contenu correspond à une représentation brute Apple prise en charge.</summary>
    /// <param name="extension">Extension utilisée uniquement comme indice.</param>
    /// <param name="bytes">Contenu déjà chargé de l'image candidate.</param>
    /// <param name="requestedFormatId">Identifiant éventuellement demandé par le consommateur.</param>
    /// <returns><see langword="true"/> lorsque les indices propres à l'extension et au contenu sont cohérents.</returns>
    public static bool LooksLikeAppleImage(string extension, ReadOnlyMemory<byte> bytes, string? requestedFormatId)
    {
        _ = requestedFormatId;
        if (extension.Equals(DiskImageFileExtensions.Img, StringComparison.OrdinalIgnoreCase))
            return LooksLikeLisaOffice(bytes.Span) || MacintoshGcrGeometry.IsSupportedCapacity(bytes.Length) && LooksLikeMac(bytes.Span);
        if (!extension.Equals(DiskImageFileExtensions.Dsk, StringComparison.OrdinalIgnoreCase)) return false;
        return bytes.Length == AppleIIGeometry.Capacity || MacintoshGcrGeometry.IsSupportedCapacity(bytes.Length) && LooksLikeMac(bytes.Span);
    }

    /// <summary>Sonde un VTOC Apple DOS 3.3 à son emplacement géométrique défini.</summary>
    public static bool LooksLikeDos33(ReadOnlySpan<byte> data)
    {
        if (data.Length != AppleIIGeometry.Capacity) return false;
        var offset = AppleDosVtoc.Track * AppleIIGeometry.SectorsPerTrack * AppleIIGeometry.SectorSize;
        return AppleDosVtoc.IsValid(data.Slice(offset, AppleIIGeometry.SectorSize), AppleIIGeometry.TrackCount, AppleIIGeometry.SectorsPerTrack, AppleIIGeometry.SectorSize);
    }

    /// <summary>Sonde l'en-tête de volume ProDOS sans lever d'exception sur un contenu trop court.</summary>
    public static bool LooksLikeProDos(ReadOnlySpan<byte> data)
    {
        var offset = ProDosVolumeHeader.BlockNumber * ProDosVolumeHeader.BlockSize;
        return data.Length >= offset + ProDosVolumeHeader.BlockSize && ProDosVolumeHeader.IsValid(data.Slice(offset, ProDosVolumeHeader.BlockSize));
    }

    /// <summary>Sonde une signature MFS ou HFS dans le bloc maître Macintosh.</summary>
    public static bool LooksLikeMac(ReadOnlySpan<byte> data) => ProbeMac(data) is not null;

    /// <summary>Retourne la variante MFS ou HFS nommée par le bloc maître, ou aucune si le contrôle échoue.</summary>
    public static MacintoshFileSystemKind? ProbeMac(ReadOnlySpan<byte> data) => MacintoshVolumeSignatures.Identify(data);

    /// <summary>Sonde chaque page Lisa jusqu'à trouver une version et un nom de volume valides.</summary>
    public static bool LooksLikeLisaOffice(ReadOnlySpan<byte> data)
    {
        if (data.Length != LisaVolumeHeader.Capacity) return false;
        for (var offset = 0; offset + LisaVolumeHeader.MinimumLength <= data.Length; offset += LisaVolumeHeader.PageSize)
            if (LisaVolumeHeader.IsValid(data[offset..])) return true;
        return false;
    }

    /// <summary>Sonde le marqueur SOS dans la fenêtre d'amorçage bornée d'une image de capacité attendue.</summary>
    public static bool LooksLikeSos(ReadOnlySpan<byte> data) => data.Length == SosBootFormat.ImageCapacity && SosBootFormat.ContainsMarker(data);
}
