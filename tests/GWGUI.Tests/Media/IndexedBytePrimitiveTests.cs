using System.Collections;
using GWGUI.MediaFileSystems.Primitives;

namespace GWGUI.Tests.Media;

public sealed class IndexedBytePrimitiveTests
{
    [Fact]
    public void CopyUsesOnlyTheRequestedIndexesAndReturnsIndependentData()
    {
        var source = new IndexedSource(index => (byte)(index + 10));
        var first = ByteListCopy.Copy(source, 5, 3);
        Assert.Equal(new byte[] { 15, 16, 17 }, first);
        Assert.Equal(new[] { 5, 6, 7 }, source.Reads);
        first[0] = 0;
        Assert.Equal(new byte[] { 15, 16, 17 }, ByteListCopy.Copy(source, 5, 3));
    }

    [Fact]
    public void EmptyCopyIgnoresSourceAndOffsetAndStillReturnsANewArray()
    {
        var first = ByteListCopy.Copy(null!, int.MinValue, 0);
        var second = ByteListCopy.Copy(null!, int.MaxValue, 0);
        Assert.Empty(first);
        Assert.Empty(second);
        Assert.NotSame(first, second);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NegativeLengthFailsDuringAllocationBeforeReading(bool nullSource)
    {
        var observed = new IndexedSource(_ => throw new InvalidOperationException("must not read"));
        IReadOnlyList<byte> source = nullSource ? null! : observed;
        Assert.Throws<OverflowException>(() => ByteListCopy.Copy(source, 0, -1));
        Assert.Empty(observed.Reads);
    }

    [Fact]
    public void CopyPropagatesTheOriginalIndexerFailure()
    {
        var expected = new InvalidOperationException("indexer");
        var source = new IndexedSource(_ => throw expected);
        var actual = Assert.Throws<InvalidOperationException>(() => ByteListCopy.Copy(source, -4, 2));
        Assert.Same(expected, actual);
        Assert.Equal(new[] { -4 }, source.Reads);
    }

    [Fact]
    public void CopyPreservesUncheckedOffsetAddition()
    {
        var source = new IndexedSource(index => (byte)(index & 0xff));
        Assert.Equal(new byte[] { 255, 0 }, ByteListCopy.Copy(source, int.MaxValue, 2));
        Assert.Equal(new[] { int.MaxValue, int.MinValue }, source.Reads);
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(1, 2, 513)]
    [InlineData(255, 0, 255)]
    [InlineData(0, 1, 256)]
    [InlineData(255, 255, 65535)]
    public void UInt16ReadsTwoIndexesInLittleEndianOrder(byte low, byte high, int expected)
    {
        var bytes = new byte[] { 0, low, high };
        var source = new IndexedSource(index => bytes[index]);
        Assert.Equal(expected, LittleEndianUInt16.ReadIndexed(source, 1));
        Assert.Equal(new[] { 1, 2 }, source.Reads);
    }

    [Theory]
    [InlineData(0, 0, 0, 0)]
    [InlineData(1, 2, 3, 197121)]
    [InlineData(0, 0, 1, 65536)]
    [InlineData(255, 255, 255, 16777215)]
    public void UInt24KeepsIndexedAndSpanValues(byte low, byte middle, byte high, int expected)
    {
        var bytes = new byte[] { 0, low, middle, high };
        var source = new IndexedSource(index => bytes[index]);
        Assert.Equal(expected, LittleEndianUInt24.ReadIndexed(source, 1));
        Assert.Equal(new[] { 1, 2, 3 }, source.Reads);
        Assert.Equal(expected, LittleEndianUInt24.Read(bytes, 1));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void IndexedReadsStopAtTheFirstFailure(bool threeBytes)
    {
        var expected = new InvalidOperationException("second index");
        var source = new IndexedSource(index => index == 1 ? throw expected : (byte)7);
        var actual = Assert.Throws<InvalidOperationException>(() =>
        {
            if (threeBytes) LittleEndianUInt24.ReadIndexed(source, 0);
            else LittleEndianUInt16.ReadIndexed(source, 0);
        });
        Assert.Same(expected, actual);
        Assert.Equal(new[] { 0, 1 }, source.Reads);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void IndexedReadsPreserveUncheckedOffsets(bool threeBytes)
    {
        var source = new IndexedSource(index => (byte)(index & 0xff));
        var actual = threeBytes ? LittleEndianUInt24.ReadIndexed(source, int.MaxValue) : LittleEndianUInt16.ReadIndexed(source, int.MaxValue);
        Assert.Equal(threeBytes ? 0x0100ff : 0xff, actual);
        Assert.Equal(threeBytes ? new[] { int.MaxValue, int.MinValue, int.MinValue + 1 } : new[] { int.MaxValue, int.MinValue }, source.Reads);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void IndexedReadsKeepTheirRawNullFailure(bool threeBytes)
        => Assert.Throws<NullReferenceException>(() =>
        {
            if (threeBytes) LittleEndianUInt24.ReadIndexed(null!, 0);
            else LittleEndianUInt16.ReadIndexed(null!, 0);
        });

    [Theory]
    [InlineData(-1)]
    [InlineData(1)]
    [InlineData(int.MaxValue)]
    public void SpanReadKeepsItsRangeValidation(int offset)
    {
        var error = Assert.Throws<ArgumentOutOfRangeException>(() => LittleEndianUInt24.Read(new byte[3], offset));
        Assert.Equal("offset", error.ParamName);
    }

    [Fact]
    public void SpanReadRejectsAShortBufferBeforeIndexing()
        => Assert.Throws<ArgumentOutOfRangeException>(() => LittleEndianUInt24.Read(new byte[2], 0));

    private sealed class IndexedSource(Func<int, byte> get) : IReadOnlyList<byte>
    {
        public List<int> Reads { get; } = [];
        public int Count => throw new InvalidOperationException("Count must not be read.");
        public byte this[int index]
        {
            get { Reads.Add(index); return get(index); }
        }
        public IEnumerator<byte> GetEnumerator() => throw new InvalidOperationException("Must not enumerate.");
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
