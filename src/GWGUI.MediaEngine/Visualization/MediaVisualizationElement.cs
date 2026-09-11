namespace GWGUI.MediaEngine.Visualization;

/// <summary>Identifies one element in the order used to prepare a media visualization.</summary>
public sealed record MediaVisualizationElement
{
    public MediaVisualizationElement(long position, int? surface = null, long? length = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(position);
        if (surface is < 0) throw new ArgumentOutOfRangeException(nameof(surface));
        if (length is <= 0) throw new ArgumentOutOfRangeException(nameof(length));

        Position = position;
        Surface = surface;
        Length = length;
    }

    public long Position { get; }

    public int? Surface { get; }

    public long? Length { get; }
}
