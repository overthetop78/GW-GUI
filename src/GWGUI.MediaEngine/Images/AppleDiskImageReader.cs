using GWGUI.MediaEngine.Containers.Apple.DiskCopy;
using GWGUI.MediaEngine.Containers.Apple.TwoImg;
using GWGUI.MediaEngine.Containers.Apple.Woz;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Recognition.Apple;
using GWGUI.MediaEngine.Recognition.Definitions;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images;

/// <summary>Route les conteneurs et représentations Apple II, Apple III, Macintosh et Lisa vers leurs lecteurs spécialisés.</summary>
public sealed class AppleDiskImageReader : ISectorImageReader
{
    /// <summary>Extensions pouvant désigner un conteneur ou une représentation brute Apple pris en charge.</summary>
    private static readonly HashSet<string> Extensions = new(StringComparer.OrdinalIgnoreCase)
        { DiskImageFileExtensions.D13, DiskImageFileExtensions.Do, DiskImageFileExtensions.Po,
            DiskImageFileExtensions.TwoMg, DiskImageFileExtensions.Image, DiskImageFileExtensions.Dc42,
            DiskImageFileExtensions.Nib, DiskImageFileExtensions.Woz, DiskImageFileExtensions.Dsk,
            DiskImageFileExtensions.Img };

    /// <summary>Indique si l'extension du chemin constitue un indice Apple connu.</summary>
    /// <param name="path">Chemin du fichier à examiner.</param>
    /// <returns><see langword="true"/> lorsque l'extension est prise en charge ; sinon <see langword="false"/>.</returns>
    public bool CanRead(string path) => Extensions.Contains(Path.GetExtension(path));

    /// <summary>Détecte le conteneur par son contenu, puis valide et reconstruit l'image sectorielle Apple.</summary>
    /// <param name="path">Chemin du fichier Apple à lire.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la lecture.</param>
    /// <returns>Image sectorielle reconstruite par le lecteur 2IMG, DiskCopy, WOZ, NIB ou brut approprié.</returns>
    /// <exception cref="InvalidDataException">Le contenu ne respecte pas le format Apple détecté ou indicé.</exception>
    /// <exception cref="NotSupportedException">Le conteneur utilise une variante Apple non prise en charge.</exception>
    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var bytes = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        var extension = Path.GetExtension(path);
        if (bytes.AsSpan().StartsWith(TwoImgFormat.SignatureBytes)) return TwoImgReader.Read(bytes);
        if (DiskCopyReader.HasPrivateWord(bytes)) return DiskCopyReader.Read(bytes);
        if (bytes.AsSpan().StartsWith(WozFormat.Version1Signature) ||
            bytes.AsSpan().StartsWith(WozFormat.Version2Signature))
            return WozReader.Read(bytes);
        if (extension.Equals(DiskImageFileExtensions.Nib, StringComparison.OrdinalIgnoreCase))
            return NibTrackImageReader.Read(bytes);
        return AppleRawImageReader.Read(bytes, extension);
    }

    /// <summary>Vérifie les indices d'extension, de taille et de structure utilisés pour présélectionner une image Apple brute.</summary>
    /// <param name="path">Chemin du fichier à examiner.</param>
    /// <returns><see langword="true"/> lorsque les indices Apple attendus sont présents ; sinon <see langword="false"/>.</returns>
    public static bool LooksLikeAppleImage(string path)
    {
        try
        {
            var extension = Path.GetExtension(path);
            if (extension.Equals(DiskImageFileExtensions.D13, StringComparison.OrdinalIgnoreCase) ||
                extension.Equals(DiskImageFileExtensions.Do, StringComparison.OrdinalIgnoreCase) ||
                extension.Equals(DiskImageFileExtensions.Po, StringComparison.OrdinalIgnoreCase) ||
                extension.Equals(DiskImageFileExtensions.TwoMg, StringComparison.OrdinalIgnoreCase) ||
                extension.Equals(DiskImageFileExtensions.Image, StringComparison.OrdinalIgnoreCase) ||
                extension.Equals(DiskImageFileExtensions.Dc42, StringComparison.OrdinalIgnoreCase) ||
                extension.Equals(DiskImageFileExtensions.Nib, StringComparison.OrdinalIgnoreCase) ||
                extension.Equals(DiskImageFileExtensions.Woz, StringComparison.OrdinalIgnoreCase)) return true;
            if (extension.Equals(DiskImageFileExtensions.Img, StringComparison.OrdinalIgnoreCase))
            {
                var raw = File.ReadAllBytes(path);
                return AppleDiskImageSignatures.LooksLikeLisaOfficePayload(raw) ||
                       raw.Length is 409_600 or 819_200 or 1_474_560 &&
                       AppleDiskImageSignatures.LooksLikeMac(raw);
            }
            if (!extension.Equals(DiskImageFileExtensions.Dsk, StringComparison.OrdinalIgnoreCase)) return false;
            var bytes = File.ReadAllBytes(path);
            return bytes.Length == 143_360 ||
                   bytes.Length is 409_600 or 819_200 or 1_474_560 &&
                   AppleDiskImageSignatures.LooksLikeMac(bytes);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>Retourne le nombre de secteurs FileWare Lisa pour un cylindre.</summary>
    /// <param name="cylinder">Index du cylindre.</param>
    /// <returns>Nombre de secteurs présents sur le cylindre.</returns>
    internal static int LisaFileWareSectors(int cylinder) => AppleDiskGeometry.LisaFileWareSectors(cylinder);

    /// <summary>Retourne le nombre de secteurs Macintosh à vitesse zonée pour un cylindre.</summary>
    /// <param name="cylinder">Index du cylindre.</param>
    /// <returns>Nombre de secteurs présents sur le cylindre.</returns>
    internal static int AppleMacSectors(int cylinder) => AppleDiskGeometry.AppleMacSectors(cylinder);

    /// <summary>Construit une image Apple II depuis des pistes déjà décodées.</summary>
    /// <param name="decodedTracks">Pistes et secteurs décodés dans leur ordre physique.</param>
    /// <returns>Image sectorielle Apple II reconstruite.</returns>
    internal static SectorImage CreateAppleIIFromDecodedTracks(IEnumerable<(int Track, IReadOnlyList<DecodedSector> Sectors)> decodedTracks) =>
        AppleSectorImageFactory.CreateAppleIIFromDecodedTracks(decodedTracks);

    /// <summary>Construit une image Apple II RWTS18 depuis des pistes déjà décodées.</summary>
    /// <param name="decodedTracks">Pistes et secteurs RWTS18 décodés.</param>
    /// <returns>Image sectorielle RWTS18 reconstruite.</returns>
    internal static SectorImage CreateRwts18FromDecodedTracks(IEnumerable<(int Track, IReadOnlyList<DecodedSector> Sectors)> decodedTracks) =>
        AppleSectorImageFactory.CreateRwts18FromDecodedTracks(decodedTracks);

    /// <summary>Indique si une charge utile possède les structures attendues de Lisa Office System.</summary>
    /// <param name="data">Données sectorielles à examiner.</param>
    /// <returns><see langword="true"/> lorsque la structure Lisa Office est reconnue.</returns>
    internal static bool LooksLikeLisaOfficePayload(ReadOnlySpan<byte> data) =>
        AppleDiskImageSignatures.LooksLikeLisaOfficePayload(data);
}
