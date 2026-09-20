using System.Collections.ObjectModel;

namespace GWGUI.MediaFileSystems.Exploration.Sequential;

/// <summary>Un bloc de bande déjà décodé, sans hypothèse sur l'existence d'un fichier.</summary>
public sealed class MediaSequentialDecodedBlock
{
    public MediaSequentialDecodedBlock(
        long position,
        ReadOnlyMemory<byte> data,
        bool? integrityValid,
        IReadOnlyDictionary<string, string> metadata)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(position);
        ArgumentNullException.ThrowIfNull(metadata);
        Position = position;
        Data = data;
        IntegrityValid = integrityValid;
        Metadata = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(metadata, StringComparer.Ordinal));
    }

    public long Position { get; }
    public ReadOnlyMemory<byte> Data { get; }
    public bool? IntegrityValid { get; }
    public IReadOnlyDictionary<string, string> Metadata { get; }
}
