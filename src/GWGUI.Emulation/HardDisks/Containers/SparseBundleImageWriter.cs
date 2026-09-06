using System.Globalization;
using System.Text;

namespace GWGUI.Emulation.HardDisks.Containers;

/// <summary>Autonomous unencrypted sparsebundle, emitted as metadata and relative band files.</summary>
public static class SparseBundleImageWriter
{
    public const int DefaultBandBytes = 8 << 20;
    public const long MaximumCapacity = 1L << 40;

    public static void Validate(long capacity, int bandBytes = DefaultBandBytes)
    {
        if (capacity < 512 || capacity > MaximumCapacity || capacity % 512 != 0)
            throw new ArgumentOutOfRangeException(nameof(capacity));
        if (bandBytes < 1 << 20 || bandBytes > 128 << 20 || (bandBytes & (bandBytes - 1)) != 0)
            throw new ArgumentOutOfRangeException(nameof(bandBytes));
    }

    /// <summary>The callback must consume each stream synchronously; its ownership stays here.</summary>
    public static void Write(long capacity, Action<string, Stream> emit, Action<Stream>? initialize = null,
        int bandBytes = DefaultBandBytes)
    {
        ArgumentNullException.ThrowIfNull(emit);
        Validate(capacity, bandBytes);
        using var content = SparseImageContent.Create(capacity, initialize);
        var metadata = Encoding.UTF8.GetBytes(FormattableString.Invariant($"""
            <?xml version="1.0" encoding="UTF-8"?>
            <!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
            <plist version="1.0"><dict>
            <key>CFBundleInfoDictionaryVersion</key><string>6.0</string>
            <key>band-size</key><integer>{bandBytes}</integer>
            <key>bundle-backingstore-version</key><integer>1</integer>
            <key>diskimage-bundle-type</key><string>com.apple.diskimage.sparsebundle</string>
            <key>size</key><integer>{capacity}</integer>
            </dict></plist>
            """));
        var bands = SparseImageContent.AllocatedUnits(content, bandBytes);
        // An empty band also materializes the mandatory bands directory for an entirely empty image.
        bands.Add(0);
        var buffer = new byte[Math.Min(bandBytes, capacity)];
        foreach (var band in bands)
        {
            var offset = band * bandBytes;
            var count = (int)Math.Min(bandBytes, capacity - offset);
            content.Position = offset;
            content.ReadExactly(buffer.AsSpan(0, count));
            while (count > 0 && buffer[count - 1] == 0) count--;
            if (count == 0 && band != 0) continue;
            using var source = new MemoryStream(buffer, 0, count, writable: false);
            emit("bands/" + band.ToString("x", CultureInfo.InvariantCulture), source);
        }
        using (var token = new MemoryStream()) emit("token", token);
        using (var backup = new MemoryStream(metadata, writable: false)) emit("Info.bckup", backup);
        using (var info = new MemoryStream(metadata, writable: false)) emit("Info.plist", info);
    }
}
