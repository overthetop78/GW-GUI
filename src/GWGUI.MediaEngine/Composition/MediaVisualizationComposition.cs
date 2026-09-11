using GWGUI.MediaEngine.Interfaces.Visualization;
using GWGUI.MediaEngine.Visualization;
using GWGUI.MediaEngine.Visualization.Providers;

namespace GWGUI.MediaEngine.Composition;

/// <summary>Provides the registered media visualization data providers.</summary>
public sealed class MediaVisualizationComposition
{
    private MediaVisualizationComposition(IReadOnlyList<IMediaVisualizationProvider> providers)
    {
        Providers = providers;
        Registry = new MediaVisualizationProviderRegistry(providers);
    }

    public IReadOnlyList<IMediaVisualizationProvider> Providers { get; }

    public MediaVisualizationProviderRegistry Registry { get; }

    public static MediaVisualizationComposition CreateDefault()
        => new([
            new FluxMediaVisualizationProvider(),
            new SectorMediaVisualizationProvider()
        ]);
}
