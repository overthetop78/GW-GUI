using System.Collections.ObjectModel;
using System.Globalization;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Images.Formats.Floppy.Adf;
using GWGUI.MediaEngine.Images.Formats.Floppy.BbcDfs;
using C = GWGUI.MediaEngine.Constants.FloppyDriveModelIds;
using E = GWGUI.MediaEngine.Constants.DiskImageFileExtensions;

namespace GWGUI.MediaEngine.Dictionaries;

/// <summary>Profils physiques réutilisant les géométries DFS/ADFS et les extensions centrales existantes.</summary>
internal static class FloppyDriveCatalog
{
    private static readonly IReadOnlyDictionary<string, (BbcDfsGeometry Geometry, bool DoubleDensity)> Definitions = new ReadOnlyDictionary<string, (BbcDfsGeometry, bool)>(new Dictionary<string, (BbcDfsGeometry, bool)>
    {
        [C.Double80] = (BbcDfsGeometry.Dsd80, false), [C.Single80] = (BbcDfsGeometry.Ssd80, false),
        [C.Double40] = (BbcDfsGeometry.Dsd40, false), [C.Single40] = (BbcDfsGeometry.Ssd40, false),
        [C.Double80Dd] = (BbcDfsGeometry.Dsd80, true), [C.Single80Dd] = (BbcDfsGeometry.Ssd80, true),
        [C.Double40Dd] = (BbcDfsGeometry.Dsd40, true), [C.Single40Dd] = (BbcDfsGeometry.Ssd40, true)
    });

    internal static IReadOnlyList<MediaFloppyDriveModel> Models(string family, bool doubleDensity)
    {
        if (family != C.AcornBbc) throw new ArgumentOutOfRangeException(nameof(family));
        return Array.AsReadOnly(Definitions.Where(entry => doubleDensity || !entry.Value.DoubleDensity).Select(entry => Describe(entry.Key)).ToArray());
    }

    internal static MediaFloppyDriveModel Describe(string id)
    {
        var (geometry, doubleDensity) = Definitions.TryGetValue(id, out var definition) ? definition : throw new ArgumentOutOfRangeException(nameof(id));
        var formats = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (doubleDensity)
            foreach (var adfs in AcornAdfGeometry.BbcGeometries.Where(adfs => adfs.Cylinders <= geometry.Cylinders && adfs.Heads <= geometry.Heads))
                formats[adfs.FormatId switch { DiskImageFormatIds.AcornAdfs160 => E.Ads, DiskImageFormatIds.AcornAdfs320 => E.Adm, _ => E.Adl }] = adfs.FormatId;
        formats[geometry.Heads == DiskGeometryConstants.SingleSidedHeadCount ? E.Ssd : E.Dsd] = geometry.FormatId;
        var extensions = formats.Keys.ToList();
        if (geometry.Heads == DiskGeometryConstants.DoubleSidedHeadCount) extensions.Add(E.Ssd);
        if (doubleDensity) extensions.Add(E.Adf);
        var sectors = doubleDensity ? AcornAdfGeometry.BbcGeometries[0].SectorsPerTrack : BbcDfsGeometry.SectorsPerTrack;
        var capacity = (long)geometry.Cylinders * geometry.Heads * sectors * BbcDfsGeometry.SectorSize;
        var display = string.Format(CultureInfo.InvariantCulture, C.DisplayFormat, geometry.Heads == DiskGeometryConstants.SingleSidedHeadCount ? C.SingleSided : C.DoubleSided, doubleDensity ? C.DoubleDensity : C.SingleDensity, capacity / DataSizeConstants.BytesPerKibibyte);
        var creationFormats = BbcDfsGeometry.Supported
            .Where(candidate => candidate.Cylinders <= geometry.Cylinders && candidate.Heads <= geometry.Heads)
            .Select(candidate => CreationFormat(candidate.FormatId,
                candidate.Heads == DiskGeometryConstants.SingleSidedHeadCount ? E.Ssd : E.Dsd,
                candidate.Heads, false, candidate.Capacity)).ToList();
        if (doubleDensity)
            creationFormats.AddRange(AcornAdfGeometry.BbcGeometries
                .Where(candidate => candidate.Cylinders <= geometry.Cylinders && candidate.Heads <= geometry.Heads)
                .Select(candidate => CreationFormat(candidate.FormatId,
                    candidate.FormatId switch { DiskImageFormatIds.AcornAdfs160 => E.Ads, DiskImageFormatIds.AcornAdfs320 => E.Adm, _ => E.Adl },
                    candidate.Heads, true, candidate.Capacity)));
        return new(id, display, capacity, extensions.AsReadOnly(), new ReadOnlyDictionary<string, string>(formats))
        {
            CreationFormats = creationFormats.AsReadOnly()
        };
    }

    private static MediaFloppyCreationFormat CreationFormat(string id, string extension, int heads, bool doubleDensity, long capacity) =>
        new(id, extension, string.Format(CultureInfo.InvariantCulture, C.CreationDisplayFormat,
            extension.TrimStart('.').ToUpperInvariant(),
            heads == DiskGeometryConstants.SingleSidedHeadCount ? C.SingleSided : C.DoubleSided,
            doubleDensity ? C.DoubleDensity : C.SingleDensity, capacity / DataSizeConstants.BytesPerKibibyte), capacity);

    internal static bool Supports(string model, long cylinders, int heads, int sectorSize, int sectorsPerTrack, long sectorCount)
    {
        if (!Definitions.TryGetValue(model, out var definition)) return false;
        var geometry = definition.Geometry;
        var validDensity = sectorsPerTrack == BbcDfsGeometry.SectorsPerTrack || definition.DoubleDensity && sectorsPerTrack == AcornAdfGeometry.BbcGeometries[0].SectorsPerTrack;
        return sectorSize == BbcDfsGeometry.SectorSize && validDensity && cylinders > 0 && cylinders <= geometry.Cylinders && heads > 0 && heads <= geometry.Heads && sectorCount == cylinders * heads * sectorsPerTrack;
    }
}
