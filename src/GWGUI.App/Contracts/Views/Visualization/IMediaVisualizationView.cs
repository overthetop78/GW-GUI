namespace GWGUI.App.Contracts.Views.Visualization;

/// <summary>Coordinates element selection between a media view and the common visualization overview.</summary>
public interface IMediaVisualizationView
{
    event Action<int, long>? ElementSelected;

    void SelectElement(int surface, long position);
}
