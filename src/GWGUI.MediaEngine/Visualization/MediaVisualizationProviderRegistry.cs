using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Interfaces.Visualization;

namespace GWGUI.MediaEngine.Visualization;

/// <summary>Selects visualization data providers by representation and document capabilities.</summary>
public sealed class MediaVisualizationProviderRegistry
{
    private readonly IReadOnlyList<IMediaVisualizationProvider> providers;

    public MediaVisualizationProviderRegistry(IReadOnlyList<IMediaVisualizationProvider> providers)
    {
        ArgumentNullException.ThrowIfNull(providers);
        if (providers.Any(provider => provider is null)) throw new ArgumentException("A visualization provider cannot be null.", nameof(providers));
        this.providers = providers.ToArray();
    }

    public IMediaVisualizationProvider? Resolve(MediaImageDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        return providers.FirstOrDefault(provider =>
            provider.RepresentationKinds.Contains(document.Representation.RepresentationKind) &&
            provider.CanProvide(document));
    }

    public MediaVisualizationDescriptor CreateDescriptor(MediaImageDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        var provider = Resolve(document) ?? throw new NotSupportedException(
            $"No visualization provider supports representation '{document.Representation.RepresentationKind}' for format '{document.FormatId}'.");
        return provider.CreateDescriptor(document);
    }
}
