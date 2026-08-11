using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Images.Interpretations;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.Recognition.Definitions;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images;

public sealed class DiskImageExplorer(DiskImageRecognitionRegistry containers, FileSystemRegistry fileSystems, ScpImageExplorationService scpExploration)
{
    private readonly DiskImageInterpretationService interpretations = new(fileSystems);

    public IReadOnlySet<string> SupportedFormatIds => fileSystems.SupportedFormatIds;

    public static DiskImageExplorer CreateDefault() => DiskImageExplorerFactory.CreateDefault();

    public async Task<ExploredDiskImage> ExploreAsync(string path, string? formatId = null, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("The disk image does not exist.", path);
        if (Path.GetExtension(path).Equals(DiskImageFileExtensions.Scp, StringComparison.OrdinalIgnoreCase) && formatId is null)
            return await scpExploration.ExploreAutomaticallyAsync(path, cancellationToken).ConfigureAwait(false);

        SectorImage image;
        try
        {
            image = await containers.ReadAsync(path, formatId, cancellationToken).ConfigureAwait(false);
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
                ? image : SectorImageInterpretation.Retag(image, formatId);
            if (fileSystems.TryRead(selectedImage, formatId, out var selectedVolume) ||
                fileSystems.TryRead(selectedImage, null, out selectedVolume))
                detected.Add(new(formatId, formatId, selectedVolume));
        }
        var unique = detected
            .GroupBy(match => $"{match.FormatId}\0{match.ReaderId}\0{match.Volume.Name}", StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First()).ToArray();
        return interpretations.CreateDocument(path, image, unique, [image.FormatId]);
    }

}
