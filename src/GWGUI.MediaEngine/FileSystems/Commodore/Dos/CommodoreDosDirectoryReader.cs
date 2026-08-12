using System.Buffers.Binary;
using GWGUI.MediaEngine.Geometries.Commodore;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Commodore.Dos;

/// <summary>Valide et lit les chaînes de répertoire Commodore DOS.</summary>
internal static class CommodoreDosDirectoryReader
{
    /// <summary>Vérifie qu'une chaîne contient uniquement des entrées actives plausibles.</summary>
    public static bool IsPlausible(SectorImage image, int firstTrack, int firstSector)
    {
        var track = firstTrack;
        var sector = firstSector;
        var visited = new HashSet<(int Track, int Sector)>();
        var valid = 0;
        var invalid = 0;
        while (track != 0 && visited.Count < CommodoreDosLayout.MaximumDirectoryChainLength && visited.Add((track, sector)))
        {
            if (!CommodoreDosSectorReader.TryRead(image, track, sector, out var data)) return false;
            for (var slot = 0; slot < CommodoreDosLayout.DirectoryEntryCount; slot++)
            {
                var offset = slot * CommodoreDosLayout.DirectoryEntrySize;
                if (offset < 0 || offset > data.Count - CommodoreDosLayout.DirectoryEntrySize) return false;
                var rawType = (CommodoreDosFileType)data[offset + CommodoreDosLayout.FileTypeOffset];
                var type = rawType & CommodoreDosFileType.BaseTypeMask;
                if (type == CommodoreDosFileType.Del) continue;
                var name = PetsciiCodec.Decode(Copy(data, offset + CommodoreDosLayout.FileNameOffset, CommodoreDosLayout.NameLength));
                var dataTrack = data[offset + CommodoreDosLayout.FirstDataTrackOffset];
                var dataSector = data[offset + CommodoreDosLayout.FirstDataSectorOffset];
                var plausible = type is >= CommodoreDosFileType.Seq and <= CommodoreDosFileType.Cbm && name.Length > 0 && !name.Contains('\ufffd') && (dataTrack == 0 || CommodoreDosGeometry.TryToLogicalBlock(image, dataTrack, dataSector, out _));
                if (plausible) valid++; else invalid++;
            }
            track = data[CommodoreDosLayout.NextTrackOffset];
            sector = data[CommodoreDosLayout.NextSectorOffset];
        }
        return invalid == 0 && (valid > 0 || visited.Count == 1);
    }

    /// <summary>Lit les entrées et propage à chacune la validité de sa chaîne de données.</summary>
    public static IReadOnlyList<FileSystemEntry> Read(SectorImage image, int firstTrack, int firstSector, List<string> warnings)
    {
        var entries = new List<FileSystemEntry>();
        var visited = new HashSet<(int Track, int Sector)>();
        var track = firstTrack;
        var sector = firstSector;
        while (track != 0)
        {
            if (!visited.Add((track, sector)))
            {
                warnings.Add(CommodoreDosWarnings.DirectoryCycle(track, sector));
                break;
            }
            var sectorStatus = CommodoreDosSectorReader.Read(image, track, sector, out var data);
            if (sectorStatus != CommodoreDosSectorReadStatus.Success)
            {
                warnings.Add(sectorStatus switch { CommodoreDosSectorReadStatus.Truncated => CommodoreDosWarnings.DirectorySectorTruncated(track, sector, data.Count), CommodoreDosSectorReadStatus.InvalidCoordinate => CommodoreDosWarnings.DirectoryCoordinateInvalid(track, sector), _ => CommodoreDosWarnings.DirectorySectorMissing(track, sector) });
                break;
            }
            ReadEntries(image, data, entries, warnings);
            track = data[CommodoreDosLayout.NextTrackOffset];
            sector = data[CommodoreDosLayout.NextSectorOffset];
        }
        return entries.OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    /// <summary>Lit les slots valides d'un secteur de répertoire complet.</summary>
    private static void ReadEntries(SectorImage image, IReadOnlyList<byte> data, List<FileSystemEntry> entries, List<string> warnings)
    {
        for (var slot = 0; slot < CommodoreDosLayout.DirectoryEntryCount; slot++)
        {
            var offset = slot * CommodoreDosLayout.DirectoryEntrySize;
            if (offset < 0 || offset > data.Count - CommodoreDosLayout.DirectoryEntrySize) continue;
            var rawType = (CommodoreDosFileType)data[offset + CommodoreDosLayout.FileTypeOffset];
            if ((rawType & CommodoreDosFileType.BaseTypeMask) == CommodoreDosFileType.Del) continue;
            var name = PetsciiCodec.Decode(Copy(data, offset + CommodoreDosLayout.FileNameOffset, CommodoreDosLayout.NameLength));
            if (name.Length == 0) continue;
            var firstDataTrack = data[offset + CommodoreDosLayout.FirstDataTrackOffset];
            var firstDataSector = data[offset + CommodoreDosLayout.FirstDataSectorOffset];
            var countBytes = Copy(data, offset + CommodoreDosLayout.DeclaredBlockCountOffset, sizeof(ushort));
            var declaredBlocks = BinaryPrimitives.ReadUInt16LittleEndian(countBytes);
            var file = CommodoreDosFileReader.Read(image, firstDataTrack, firstDataSector, warnings, name);
            var size = file.IsValid ? file.Content.Count : declaredBlocks * CommodoreDosLayout.DataBytesPerSector;
            entries.Add(new(name, FileSystemEntryKind.File, size, null, CommodoreDosFileTypeNames.GetComment(rawType), (uint)(byte)rawType, file.FirstLogicalBlock.GetValueOrDefault(), file.IsValid, [], file.Content));
        }
    }

    /// <summary>Copie une petite plage bornée destinée à un codec ou une primitive binaire.</summary>
    private static byte[] Copy(IReadOnlyList<byte> source, int offset, int length)
    {
        var result = new byte[length];
        for (var index = 0; index < length; index++) result[index] = source[offset + index];
        return result;
    }
}
