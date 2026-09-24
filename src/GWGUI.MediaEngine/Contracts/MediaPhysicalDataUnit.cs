using System.Collections.ObjectModel;

namespace GWGUI.MediaEngine.Contracts;

/// <summary>Contains one ordered neutral data unit acquired from or prepared for a physical medium.</summary>
public sealed record MediaPhysicalDataUnit
{
    public MediaPhysicalDataUnit(
        long position,
        ReadOnlyMemory<byte> data,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(position);
        Position = position;
        Data = data;
        Metadata = new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>(metadata ?? new Dictionary<string, string>(), StringComparer.Ordinal));
    }

    public long Position { get; }

    public ReadOnlyMemory<byte> Data { get; }

    public IReadOnlyDictionary<string, string> Metadata { get; }
}
