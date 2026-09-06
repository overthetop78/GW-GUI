using System.Text;

namespace GWGUI.Emulation.HardDisks.FileSystems;

/// <summary>D90 DOS volume with 153 cylinders, 4 or 6 heads and 32 sectors of 256 bytes.</summary>
public static class D90VolumeFormatter
{
    public static void Validate(long capacity, string label, string diskId = "ID")
    {
        if (capacity is not (5013504 or 7520256)) throw new ArgumentOutOfRangeException(nameof(capacity));
        if (label.Length is < 1 or > 16 || label.Any(c => !Allowed(c)) ||
            diskId.Length != 2 || diskId.Any(c => !Allowed(c)))
            throw new ArgumentException("D90 requires a 1–16 character uppercase label and a two-character disk ID.");
    }

    public static void Format(Stream volume, string label, string diskId = "ID")
    {
        Validate(volume.Length, label, diskId);
        var heads = (int)(volume.Length / (153 * 32 * 256));
        var trackSectors = heads * 32;
        var tracksPerBam = 250 / (heads * 5);
        var bamStart = 256 - tracksPerBam * heads * 5;
        var bamCount = (153 + tracksPerBam - 1) / tracksPerBam;
        var bamTracks = Enumerable.Range(0, bamCount).Select(i => Math.Min(152, 1 + i * tracksPerBam)).ToArray();
        var allocated = new HashSet<int> { 0, 1, 76 * trackSectors + 10, 76 * trackSectors + 20 };
        foreach (var track in bamTracks) allocated.Add(track * trackSectors);
        void Put(int track, int sector, byte[] bytes)
        { volume.Position = (track * (long)trackSectors + sector) * 256; volume.Write(bytes); }

        var config = new byte[256]; config[1] = 1; config[3] = 0xff;
        config[4] = config[6] = 76; config[5] = 10; config[7] = 20; config[8] = 1;
        Encoding.ASCII.GetBytes(diskId).CopyTo(config, 10); Put(0, 0, config);
        Put(0, 1, Enumerable.Repeat((byte)0xff, 256).ToArray());
        for (var index = 0; index < bamCount; index++)
        {
            var bam = new byte[256];
            bam[0] = index + 1 < bamCount ? (byte)bamTracks[index + 1] : (byte)0xff;
            bam[1] = index + 1 < bamCount ? (byte)0 : (byte)0xff;
            bam[2] = index > 0 ? (byte)bamTracks[index - 1] : (byte)0xff;
            bam[3] = index > 0 ? (byte)0 : (byte)0xff;
            var first = index * tracksPerBam; var end = Math.Min(153, first + tracksPerBam);
            bam[4] = (byte)first; bam[5] = (byte)end;
            for (var track = first; track < end; track++)
            for (var head = 0; head < heads; head++)
            {
                var at = bamStart + ((track - first) * heads + head) * 5;
                for (var sector = 0; sector < 32; sector++)
                    if (!allocated.Contains(track * trackSectors + head * 32 + sector))
                    { bam[at]++; bam[at + 1 + sector / 8] |= (byte)(1 << (sector % 8)); }
            }
            Put(bamTracks[index], 0, bam);
        }
        var header = new byte[256]; header.AsSpan(6, 27).Fill(0xa0);
        header[0] = 76; header[1] = 10; Encoding.ASCII.GetBytes(label).CopyTo(header, 6);
        Encoding.ASCII.GetBytes(diskId).CopyTo(header, 24); header[27] = (byte)'3'; header[28] = (byte)'A';
        Put(76, 20, header);
        var directory = new byte[256]; directory[1] = 0xff; Put(76, 10, directory);
        volume.Flush();
    }

    private static bool Allowed(char value) => value is >= 'A' and <= 'Z' or >= '0' and <= '9' or ' ' or '-' or '_' or '.';
}
