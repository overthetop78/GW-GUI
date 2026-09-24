namespace GWGUI.MediaEngine.Contracts;

/// <summary>Describes one track index and its signed sector position relative to the track program area.</summary>
public sealed record OpticalTrackIndex
{
    public OpticalTrackIndex(int number, long relativeSector)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(number);
        if (number > 99) throw new ArgumentOutOfRangeException(nameof(number));
        Number = number;
        RelativeSector = relativeSector;
    }

    public int Number { get; }

    public long RelativeSector { get; }
}
