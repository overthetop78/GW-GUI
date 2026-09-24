using System.IO;
using GWGUI.MediaFileSystems.Constants;
using GWGUI.MediaFileSystems.Definitions;
using GWGUI.MediaFileSystems.FileSystems.Atari.ClkGraphicsLibrary;
using GWGUI.MediaFileSystems.FileSystems.Atari.Dos;
using GWGUI.MediaFileSystems.FileSystems.Atari.KFile;
using GWGUI.MediaFileSystems.Functions;
using GWGUI.MediaFileSystems.Interfaces;

namespace GWGUI.MediaFileSystems.FileSystems.Atari.BootDisk;

/// <summary>Identifie une disquette Atari contenant directement un programme de démarrage sans catalogue.</summary>
public sealed class AtariBootDiskFileSystemReader : IFileSystemReader
{
    private static readonly IReadOnlyList<IFileSystemReader> CataloguedReaders =
    [
        new AtariDosFileSystemReader(),
        new AtariKFileFileSystemReader(),
        new AtariClkGraphicsLibraryFileSystemReader()
    ];

    public string Id => FileSystemIds.AtariBootDisk;

    public IReadOnlySet<string> CatalogFormatIds => AtariBootDiskFormatCatalog.FormatIds;

    public bool CanRead(IMediaSectorImage image) =>
        CatalogFormatIds.Contains(image.FormatId)
        && AtariBootDiskFunctions.IsCompleteBootProgram(image)
        && CataloguedReaders.All(reader => !reader.CanRead(image));

    public FileSystemVolume Read(IMediaSectorImage image)
    {
        if (!CanRead(image))
            throw new InvalidDataException(AtariBootDiskConstants.InvalidImageMessage);

        return new FileSystemVolume(
            string.Empty,
            FileSystemIds.AtariBootDisk,
            image.Capacity,
            0,
            null,
            null,
            [],
            [],
            freeSpaceKnown: false,
            attributes:
            [
                AtariBootDiskConstants.BootableAttribute,
                AtariBootDiskConstants.DirectBootProgramAttribute
            ],
            bootable: true,
            fileSystemDisplayName: FileSystemDisplayNames.AtariBootDisk);
    }
}
