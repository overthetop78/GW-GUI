using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Visualization;

namespace GWGUI.MediaEngine.Interfaces.Visualization;

/// <summary>Describes the visualization capabilities of one or more media representations.</summary>
public interface IMediaVisualizationProvider
{
    IReadOnlySet<MediaRepresentationKind> RepresentationKinds { get; }

    bool CanProvide(MediaImageDocument document);

    MediaVisualizationDescriptor CreateDescriptor(MediaImageDocument document);
}
