using System.Collections.ObjectModel;

namespace GWGUI.MediaEngine.Contracts;

/// <summary>Returns bytes decoded from FSK UART frames and the evidence used to score the result.</summary>
public sealed class FskSerialDecodeResult
{
    public FskSerialDecodeResult(IReadOnlyList<byte> bytes, int validFrames, int testedFrames, IReadOnlyList<int>? positions = null)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        ArgumentOutOfRangeException.ThrowIfNegative(validFrames);
        ArgumentOutOfRangeException.ThrowIfNegative(testedFrames);
        if (validFrames > testedFrames) throw new ArgumentOutOfRangeException(nameof(validFrames));
        Bytes = new ReadOnlyCollection<byte>(bytes.ToArray());
        ValidFrames = validFrames;
        TestedFrames = testedFrames;
        Positions = positions ?? [];
    }

    public IReadOnlyList<byte> Bytes { get; }
    public int ValidFrames { get; }
    public int TestedFrames { get; }
    /// <summary>Position du start de chaque octet : en bits pour DecodeFrames, en échantillons pour Decode.</summary>
    public IReadOnlyList<int> Positions { get; }
}
