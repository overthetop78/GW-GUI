using System.Collections.ObjectModel;

namespace GWGUI.MediaEngine.Contracts;

/// <summary>Returns bytes decoded from FSK UART frames and the evidence used to score the result.</summary>
public sealed class FskSerialDecodeResult
{
    public FskSerialDecodeResult(IReadOnlyList<byte> bytes, int validFrames, int testedFrames)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        ArgumentOutOfRangeException.ThrowIfNegative(validFrames);
        ArgumentOutOfRangeException.ThrowIfNegative(testedFrames);
        if (validFrames > testedFrames) throw new ArgumentOutOfRangeException(nameof(validFrames));
        Bytes = new ReadOnlyCollection<byte>(bytes.ToArray());
        ValidFrames = validFrames;
        TestedFrames = testedFrames;
    }

    public IReadOnlyList<byte> Bytes { get; }
    public int ValidFrames { get; }
    public int TestedFrames { get; }
}
