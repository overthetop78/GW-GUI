using System.Buffers.Binary;
using System.Text;

namespace GWGUI.Emulation.HardDisks.FileSystems;

/// <summary>SWAPSPACE2 version 1, explicit page size and integer byte order. No hibernation state.</summary>
public static class LinuxSwapVolumeFormatter
{
    public static void Validate(long capacity, string label, int pageBytes = 4096, IReadOnlyList<uint>? badPages = null)
    {
        if (pageBytes is not (4096 or 8192 or 16384 or 32768 or 65536) || capacity < 10L * pageBytes ||
            capacity % pageBytes != 0 || capacity / pageBytes > uint.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(capacity));
        if (label.Any(c => c < 32 || c > 126) || label.Length > 15)
            throw new ArgumentException("This swap label profile accepts up to 15 printable ASCII characters.", nameof(label));
        if (badPages is not null && (badPages.Count > (pageBytes - 10 - 1536) / 4 || badPages.Count > capacity / pageBytes - 10 ||
            badPages.Any(page => page == 0 || page >= capacity / pageBytes) || badPages.Distinct().Count() != badPages.Count))
            throw new ArgumentException("Invalid or duplicate swap bad-page index.", nameof(badPages));
    }

    public static void Format(Stream volume, string label, int pageBytes = 4096, bool bigEndian = false,
        Guid? uuid = null, IReadOnlyList<uint>? badPages = null)
    {
        var pages = badPages?.ToArray() ?? [];
        Validate(volume.Length, label, pageBytes, pages);
        if (!volume.CanSeek || !volume.CanWrite) throw new ArgumentException("A writable seekable volume is required.");
        var header = new byte[pageBytes];
        Put(1024, 1); Put(1028, checked((uint)(volume.Length / pageBytes - 1))); Put(1032, (uint)pages.Length);
        (uuid ?? Guid.NewGuid()).ToByteArray(bigEndian: true).CopyTo(header, 1036);
        Encoding.ASCII.GetBytes(label).CopyTo(header, 1052);
        for (var i = 0; i < pages.Length; i++) Put(1536 + i * 4, pages[i]);
        "SWAPSPACE2"u8.CopyTo(header.AsSpan(pageBytes - 10));
        volume.Position = 0; volume.Write(header); volume.Flush();
        void Put(int offset, uint value)
        {
            if (bigEndian) BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(offset), value);
            else BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(offset), value);
        }
    }

    internal static bool IsProfile(string id) => id is "linux-swap-4k" or "linux-swap-8k" or "linux-swap-16k" or
        "linux-swap-32k" or "linux-swap-64k" or "linux-swap-4k-be" or "linux-swap-8k-be" or
        "linux-swap-16k-be" or "linux-swap-32k-be" or "linux-swap-64k-be";
}
