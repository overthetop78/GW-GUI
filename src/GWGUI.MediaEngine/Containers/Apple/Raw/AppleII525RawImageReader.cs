using GWGUI.MediaEngine.Conversion.Apple;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Geometries.Apple;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.Recognition.Apple;
using GWGUI.MediaEngine.SectorImages.Builders;

namespace GWGUI.MediaEngine.Containers.Apple.Raw;

/// <summary>Lit les représentations sectorielles Apple II 5,25 pouces en ordre DOS ou ProDOS.</summary>
internal static class AppleII525RawImageReader
{
    /// <summary>Géométrie DOS 3.2 de 35 pistes, une face, treize secteurs de 256 octets à base zéro.</summary>
    private static readonly LinearSectorImageGeometry Dos32Geometry = new(AppleIIGeometry.SectorSize, AppleIIGeometry.TrackCount, DiskGeometryConstants.SingleSidedHeadCount, AppleIIGeometry.Dos32SectorsPerTrack);
    /// <summary>Géométrie DOS 3.3 de 35 pistes, une face, seize secteurs de 256 octets à base zéro.</summary>
    private static readonly LinearSectorImageGeometry Dos33Geometry = new(AppleIIGeometry.SectorSize, AppleIIGeometry.TrackCount, DiskGeometryConstants.SingleSidedHeadCount, AppleIIGeometry.SectorsPerTrack);
    /// <summary>Géométrie ProDOS de 35 pistes, une face, huit blocs de 512 octets à base zéro.</summary>
    private static readonly LinearSectorImageGeometry ProDosGeometry = new(MacintoshGcrGeometry.BlockSize, AppleIIGeometry.TrackCount, DiskGeometryConstants.SingleSidedHeadCount, AppleIIGeometry.ProDosBlocksPerTrack);

    /// <summary>Construit l'unique interprétation géométrique D13.</summary>
    public static AppleRawImageReadResult ReadDos32(ReadOnlyMemory<byte> data) => new(LinearSectorImageBuilder.Create(data, DiskImageFormatIds.AppleIIDos32, Dos32Geometry), AppleRawImageMatchKind.GeometryFallback);

    /// <summary>Applique dans l'ordre l'indice PO, ProDOS direct, DOS direct, SOS converti, ProDOS converti et le repli DOS 3.3.</summary>
    public static AppleRawImageReadResult Read(ReadOnlyMemory<byte> data, string extension)
    {
        if (extension.Equals(DiskImageFileExtensions.Po, StringComparison.OrdinalIgnoreCase) && AppleRawImageProbe.LooksLikeSos(data.Span)) return new(LinearSectorImageBuilder.Create(data, DiskImageFormatIds.AppleIIISos, ProDosGeometry), AppleRawImageMatchKind.ValidatedStructure);
        if (extension.Equals(DiskImageFileExtensions.Po, StringComparison.OrdinalIgnoreCase)) return CreateProDos(data, AppleRawImageMatchKind.ExtensionHint);
        if (AppleRawImageProbe.LooksLikeProDos(data.Span)) return CreateProDos(data, AppleRawImageMatchKind.ValidatedStructure);
        if (AppleRawImageProbe.LooksLikeDos33(data.Span)) return CreateDos33(data, AppleRawImageMatchKind.ValidatedStructure);
        var converted = AppleIISectorOrderConverter.DosToProDos(data.Span);
        if (AppleRawImageProbe.LooksLikeSos(converted)) return new(LinearSectorImageBuilder.Create(converted, DiskImageFormatIds.AppleIIISos, ProDosGeometry), AppleRawImageMatchKind.ValidatedStructure);
        if (AppleRawImageProbe.LooksLikeProDos(converted)) return CreateProDos(converted, AppleRawImageMatchKind.ValidatedStructure);
        return CreateDos33(data, AppleRawImageMatchKind.GeometryFallback);
    }

    /// <summary>Construit une image ProDOS dans l'ordre direct de blocs de 512 octets.</summary>
    private static AppleRawImageReadResult CreateProDos(ReadOnlyMemory<byte> data, AppleRawImageMatchKind matchKind) => new(LinearSectorImageBuilder.Create(data, DiskImageFormatIds.AppleIIProDos, ProDosGeometry), matchKind);

    /// <summary>Construit une image DOS 3.3 dans l'ordre direct de secteurs de 256 octets.</summary>
    private static AppleRawImageReadResult CreateDos33(ReadOnlyMemory<byte> data, AppleRawImageMatchKind matchKind) => new(LinearSectorImageBuilder.Create(data, DiskImageFormatIds.AppleIIDos33, Dos33Geometry), matchKind);
}
