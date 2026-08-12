using System.IO;
using GWGUI.MediaEngine.FileSystems.Definitions;
using GWGUI.MediaEngine.FileSystems.Readers;
using GWGUI.MediaEngine.Containers.TeleDisk;
using GWGUI.MediaEngine.Images;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.Tests;

/// <summary>Vérifie les avertissements communs des lecteurs de systèmes de fichiers.</summary>
public sealed class FileSystemWarningMessagesTests
{
    /// <summary>Vérifie que le nom de l'entrée et le diagnostic d'origine sont conservés.</summary>
    [Fact]
    public void PreservesEntryNameAndOriginalDiagnostic()
    {
        var warning = FileSystemWarningMessages.EntryReadFailure("README", new InvalidDataException("invalid chain"));
        Assert.Contains("README", warning, StringComparison.Ordinal);
        Assert.Contains("invalid chain", warning, StringComparison.Ordinal);
    }

    /// <summary>Vérifie que l'avertissement de blocs manquants contient le nom de l'entrée.</summary>
    [Fact]
    public void IncludesEntryNameInMissingBlocksWarning() => Assert.Contains("SYSTEM.PASCAL", FileSystemWarningMessages.MissingDataBlocks("SYSTEM.PASCAL"), StringComparison.Ordinal);

    /// <summary>Vérifie l'avertissement RT-11 produit par le lecteur public lorsqu'un bloc de fichier manque.</summary>
    [Fact]
    public async Task Rt11PublicReaderReportsMissingFileBlock()
    {
        var path = Directory.EnumerateFiles(ImageTestRoot(), "BA-J837B-BC_MINC_MA_DEMO_23_V2.0_BIN_RX2.img", SearchOption.AllDirectories).First();
        var source = await DiskImageExplorer.CreateDefault().ExploreAsync(path);
        var entry = source.Volume.Entries.First();
        var incomplete = WithoutBlock(source.Image, entry.StorageReference);
        var volume = new Rt11FileSystemReader().Read(incomplete);
        Assert.Contains(volume.Warnings, warning => warning == FileSystemWarningMessages.MissingDataBlocks(entry.Name));
    }

    /// <summary>Vérifie l'avertissement UCSD produit par le lecteur public lorsqu'un bloc de fichier manque.</summary>
    [Fact]
    public async Task UcsdPublicReaderReportsMissingFileBlock()
    {
        var path = Directory.EnumerateFiles(ImageTestRoot(), "ucsdpasc.td0", SearchOption.AllDirectories).First();
        var source = await new Td0Reader().ReadAsync(path);
        var reader = new UcsdFileSystemReader();
        var entry = reader.Read(source).Entries.First();
        var volume = reader.Read(WithoutBlock(source, entry.StorageReference));
        Assert.Contains(volume.Warnings, warning => warning == FileSystemWarningMessages.MissingDataBlocks(entry.Name));
    }

    /// <summary>Crée une image identique à la source en omettant un bloc logique.</summary>
    private static SectorImage WithoutBlock(SectorImage source, int logicalBlock) => new(source.FormatId, source.BlockSize, source.Cylinders, source.Heads, source.SectorsPerTrack, source.AvailableBlocks.Where(block => block.LogicalBlock != logicalBlock), capacity: source.Capacity, logicalBlockCount: source.BlockCount);

    /// <summary>Retourne le dossier des images de test non versionnées.</summary>
    private static string ImageTestRoot() => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "image_test"));
}
