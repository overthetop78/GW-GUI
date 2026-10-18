using System.IO;
using GWGUI.MediaEngine.Images.Conversion.Apple;

namespace GWGUI.Tests;

public sealed class AppleSectorOrderReuseTests
{
    private const int SectorSize = 256;
    private const int SectorsPerTrack = 16;
    private const int TrackSize = SectorSize * SectorsPerTrack;
    private static readonly int[] TargetToSource = [0, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 15];

    [Theory]
    [InlineData(false, 0)]
    [InlineData(false, 1)]
    [InlineData(false, 3)]
    [InlineData(false, 35)]
    [InlineData(true, 0)]
    [InlineData(true, 1)]
    [InlineData(true, 3)]
    [InlineData(true, 35)]
    public void BothDirectionsPreserveTheFixedSectorPermutation(bool reverse, int tracks)
    {
        var input = new byte[tracks * TrackSize];
        for (var track = 0; track < tracks; track++)
            for (var sector = 0; sector < SectorsPerTrack; sector++)
                for (var offset = 0; offset < SectorSize; offset++)
                    input[track * TrackSize + sector * SectorSize + offset] = (byte)(track * 37 + sector * 11 + offset);
        var before = (byte[])input.Clone();
        var actual = reverse ? AppleIISectorOrderConverter.ProDosToDos(input) : AppleIISectorOrderConverter.DosToProDos(input);
        Assert.NotSame(input, actual);
        Assert.Equal(input.Length, actual.Length);
        Assert.Equal(before, input);
        for (var track = 0; track < tracks; track++)
            for (var sector = 0; sector < SectorsPerTrack; sector++)
                Assert.True(input.AsSpan(track * TrackSize + TargetToSource[sector] * SectorSize, SectorSize)
                    .SequenceEqual(actual.AsSpan(track * TrackSize + sector * SectorSize, SectorSize)),
                    $"Track {track}, sector {sector}, reverse={reverse}");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(255)]
    [InlineData(4095)]
    [InlineData(4097)]
    public void BothDirectionsRejectIncompleteTracksWithTheExistingError(int length)
    {
        var input = new byte[length];
        var forward = Assert.Throws<InvalidDataException>(() => AppleIISectorOrderConverter.DosToProDos(input));
        var reverse = Assert.Throws<InvalidDataException>(() => AppleIISectorOrderConverter.ProDosToDos(input));
        var expected = $"Apple II image contains {length} bytes; its length must be a multiple of {TrackSize} bytes.";
        Assert.Equal(expected, forward.Message);
        Assert.Equal(expected, reverse.Message);
    }
}
