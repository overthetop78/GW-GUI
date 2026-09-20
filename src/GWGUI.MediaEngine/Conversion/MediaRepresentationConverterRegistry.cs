using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Interfaces.Conversion;

namespace GWGUI.MediaEngine.Conversion;

/// <summary>Selects in-memory representation converters by their declared capabilities.</summary>
public sealed class MediaRepresentationConverterRegistry
{
    private readonly IReadOnlyList<IMediaRepresentationConverter> converters;

    public MediaRepresentationConverterRegistry(IReadOnlyList<IMediaRepresentationConverter> converters)
    {
        ArgumentNullException.ThrowIfNull(converters);
        if (converters.Any(converter => converter is null)) throw new ArgumentException("A media representation converter cannot be null.", nameof(converters));
        var duplicate = converters.GroupBy(converter => converter.Id, StringComparer.OrdinalIgnoreCase).FirstOrDefault(group => group.Skip(1).Any());
        if (duplicate is not null) throw new ArgumentException($"Media representation converter identifier '{duplicate.Key}' is registered more than once.", nameof(converters));
        this.converters = converters.ToArray();
    }

    public IReadOnlyList<IMediaRepresentationConverter> Converters => converters;

    public IMediaRepresentationConverter? Resolve(
        MediaImageDocument source,
        string targetFormatId,
        MediaRepresentationKind targetRepresentationKind)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetFormatId);
        return converters.FirstOrDefault(converter =>
            converter.SourceRepresentationKinds.Contains(source.Representation.RepresentationKind) &&
            converter.TargetRepresentationKinds.Contains(targetRepresentationKind) &&
            converter.CanConvert(source, targetFormatId, targetRepresentationKind));
    }
}
