namespace GWGUI.MediaEngine.Images;

internal static class CommodoreGeometry
{
    public static int SectorsFor1541Track(int track) => track switch
    {
        >= 1 and <= 17 => 21,
        <= 24 => 19,
        <= 30 => 18,
        <= 40 => 17,
        _ => throw new ArgumentOutOfRangeException(nameof(track))
    };

    public static int BlocksPer1541Side(int tracks) => Enumerable.Range(1, tracks).Sum(SectorsFor1541Track);

    public static int To1541LogicalBlock(int track, int sector, int tracksPerSide, int side = 0)
    {
        if (track < 1 || track > tracksPerSide) throw new ArgumentOutOfRangeException(nameof(track));
        var sectors = SectorsFor1541Track(track);
        if (sector < 0 || sector >= sectors) throw new ArgumentOutOfRangeException(nameof(sector));
        return side * BlocksPer1541Side(tracksPerSide)
            + Enumerable.Range(1, track - 1).Sum(SectorsFor1541Track)
            + sector;
    }

    public static (int Track, int Sector, int Side) From1541LogicalBlock(int block, int tracksPerSide, int sides)
    {
        var blocksPerSide = BlocksPer1541Side(tracksPerSide);
        if (block < 0 || block >= blocksPerSide * sides) throw new ArgumentOutOfRangeException(nameof(block));
        var side = block / blocksPerSide;
        var remaining = block % blocksPerSide;
        for (var track = 1; track <= tracksPerSide; track++)
        {
            var sectors = SectorsFor1541Track(track);
            if (remaining < sectors) return (track, remaining, side);
            remaining -= sectors;
        }
        throw new InvalidOperationException();
    }

    public static int To1581LogicalBlock(int track, int sector)
    {
        if (track is < 1 or > 80 || sector is < 0 or >= 40) throw new ArgumentOutOfRangeException();
        return (track - 1) * 40 + sector;
    }
}
