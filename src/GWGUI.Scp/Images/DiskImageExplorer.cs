using GWGUI.Scp.FileSystems;
using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images;

public sealed class DiskImageExplorer(
    AdfImageReader adfReader,
    AtariStImageReader stReader,
    MsaImageReader msaReader,
    AtrImageReader atrReader,
    CommodoreD64ImageReader d64Reader,
    CommodoreD71ImageReader d71Reader,
    CommodoreD81ImageReader d81Reader,
    AmstradDskImageReader amstradDskReader,
    MsxImageReader msxReader,
    IbmPcImageReader ibmPcReader,
    AppleDiskImageReader appleReader,
    BbcDfsImageReader bbcReader,
    CoherentImageReader coherentReader,
    DecRx02ImageReader decRx02Reader,
    Td0ImageReader td0Reader,
    I86fImageReader i86fReader,
    Cp2ImageReader cp2Reader,
    ImdImageReader imdReader,
    FileSystemRegistry fileSystems,
    ScpImageExplorationService scpExploration)
{
    private readonly DiskImageInterpretationService interpretations = new(fileSystems);

    public IReadOnlySet<string> SupportedFormatIds => fileSystems.SupportedFormatIds;

    public static DiskImageExplorer CreateDefault() => DiskImageExplorerFactory.CreateDefault();

    public async Task<ExploredDiskImage> ExploreAsync(
        string path,
        string? formatId = null,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("The disk image does not exist.", path);
        var extension = Path.GetExtension(path).ToLowerInvariant();
        if (extension.Equals(".scp", StringComparison.OrdinalIgnoreCase) && formatId is null)
            return await scpExploration.ExploreAutomaticallyAsync(path, cancellationToken).ConfigureAwait(false);

        SectorImage image;
        try
        {
            image = await ReadContainerAsync(path, extension, formatId, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is InvalidDataException or NotSupportedException)
        {
            return interpretations.Unknown(path);
        }

        var detected = new List<ExploredFileSystem>();
        if (formatId is null)
        {
            foreach (var match in fileSystems.ReadAll(image))
                detected.Add(new(image.FormatId, match.ReaderId, match.Volume));
            if (detected.Count == 0)
            {
                foreach (var interpretation in interpretations.AdditionalFileSystemInterpretations(image))
                {
                    if (!fileSystems.TryRead(interpretation, interpretation.FormatId, out var volume)) continue;
                    image = interpretation;
                    detected.Add(new(interpretation.FormatId, interpretation.FormatId, volume));
                    break;
                }
            }
        }
        else
        {
            var selectedImage = image.FormatId.Equals(formatId, StringComparison.OrdinalIgnoreCase)
                ? image : DiskImageInterpretationService.Retag(image, formatId);
            if (fileSystems.TryRead(selectedImage, formatId, out var selectedVolume) ||
                fileSystems.TryRead(selectedImage, null, out selectedVolume))
                detected.Add(new(formatId, formatId, selectedVolume));
        }
        var unique = detected
            .GroupBy(match => $"{match.FormatId}\0{match.ReaderId}\0{match.Volume.Name}", StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First()).ToArray();
        return interpretations.CreateDocument(path, image, unique, [image.FormatId]);
    }

    private async Task<SectorImage> ReadContainerAsync(
        string path,
        string extension,
        string? formatId,
        CancellationToken cancellationToken)
    {
        if (extension.Equals(".adf", StringComparison.OrdinalIgnoreCase))
            return await adfReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        if (extension.Equals(".ssd", StringComparison.OrdinalIgnoreCase) || extension.Equals(".dsd", StringComparison.OrdinalIgnoreCase))
            return await bbcReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        if (extension.Equals(".bin", StringComparison.OrdinalIgnoreCase) &&
            CoherentImageReader.LooksLikeCoherent(await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false)))
            return await coherentReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        if (extension.Equals(".img", StringComparison.OrdinalIgnoreCase) &&
            (formatId?.Equals("dec.rx02", StringComparison.OrdinalIgnoreCase) == true ||
             DecRx02ImageReader.LooksLikeRt11(await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false))))
            return await decRx02Reader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        if (extension.Equals(".st", StringComparison.OrdinalIgnoreCase))
            return await stReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        if (extension.Equals(".msa", StringComparison.OrdinalIgnoreCase))
            return await msaReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        if (extension.Equals(".atr", StringComparison.OrdinalIgnoreCase))
            return await atrReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        if (extension.Equals(".d64", StringComparison.OrdinalIgnoreCase))
            return await d64Reader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        if (extension.Equals(".d71", StringComparison.OrdinalIgnoreCase))
            return await d71Reader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        if (extension.Equals(".d81", StringComparison.OrdinalIgnoreCase))
            return await d81Reader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        if (extension.Equals(".dsk", StringComparison.OrdinalIgnoreCase) &&
            (formatId?.StartsWith("apple", StringComparison.OrdinalIgnoreCase) == true ||
             AppleDiskImageReader.LooksLikeAppleImage(path)))
            return await appleReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        if (extension is ".do" or ".po" or ".2mg" or ".image" or ".d13" or ".dc42" or ".nib" or ".woz")
            return await appleReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        if (extension.Equals(".dsk", StringComparison.OrdinalIgnoreCase) &&
            (formatId?.StartsWith("msx.", StringComparison.OrdinalIgnoreCase) == true ||
             MsxImageReader.LooksLikeMsx(await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false))))
            return await msxReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        if (extension.Equals(".dsk", StringComparison.OrdinalIgnoreCase) || extension.Equals(".edsk", StringComparison.OrdinalIgnoreCase))
            return await amstradDskReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        if (extension.Equals(".img", StringComparison.OrdinalIgnoreCase) &&
            (formatId?.StartsWith("mac.", StringComparison.OrdinalIgnoreCase) == true || AppleDiskImageReader.LooksLikeAppleImage(path)))
            return await appleReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        if (extension.Equals(".img", StringComparison.OrdinalIgnoreCase))
            return await ReadRawImgAsync(path, cancellationToken).ConfigureAwait(false);
        if (extension.Equals(".ima", StringComparison.OrdinalIgnoreCase))
            return await ibmPcReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        if (extension.Equals(".td0", StringComparison.OrdinalIgnoreCase))
            return await td0Reader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        if (extension.Equals(".86f", StringComparison.OrdinalIgnoreCase))
            return await i86fReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        if (extension.Equals(".cp2", StringComparison.OrdinalIgnoreCase))
            return await cp2Reader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        if (extension.Equals(".imd", StringComparison.OrdinalIgnoreCase))
            return await imdReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        if (extension.Equals(".scp", StringComparison.OrdinalIgnoreCase))
        {
            if (formatId is not null && !SupportedFormatIds.Contains(formatId))
                throw new NotSupportedException($"The selected format '{formatId}' is not supported by the explorer yet.");
            return await scpExploration.ReadAsync(path, formatId, cancellationToken).ConfigureAwait(false);
        }
        throw new NotSupportedException($"The image extension '{extension}' is not supported by the explorer yet.");
    }

    private static async Task<SectorImage> ReadRawImgAsync(string path, CancellationToken cancellationToken)
    {
        var bytes = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        var hasFatBpb = IbmPcImageReader.HasValidBpbGeometry(bytes);
        if (!hasFatBpb && FileSystems.Readers.AmstradCpmFileSystemReader.LooksLikeCpcRawImage(bytes))
            return DiskImageInterpretationService.Retag(IbmPcImageReader.Create(bytes, cancellationToken), "amstrad.cpc");
        if (!hasFatBpb && FileSystems.Readers.AmstradCpmFileSystemReader.LooksLikePcwDiskSpecification(bytes))
            return DiskImageInterpretationService.Retag(IbmPcImageReader.Create(bytes, cancellationToken), "amstrad.pcw");
        return IbmPcImageReader.Create(bytes, cancellationToken);
    }
}
