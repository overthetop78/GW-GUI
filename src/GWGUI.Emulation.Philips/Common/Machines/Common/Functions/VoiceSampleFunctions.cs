using System.IO;
using System.IO.Compression;
using System.Buffers.Binary;
using System.Text.RegularExpressions;

namespace GWGUI.Emulation.Philips.Common.Machines.Common.Functions;

internal static class VoiceSampleFunctions
{
    private static readonly Regex FileNames = new(VoiceSampleConstants.FileNamePattern,
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    internal static IReadOnlyList<ZipArchiveEntry> Entries(ZipArchive archive)
    {
        var entries = archive.Entries.Where(entry => FileNames.IsMatch(entry.Name)).ToArray();
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (entries.Length == 0) throw new InvalidDataException();
        foreach (var entry in entries)
        {
            if (!names.Add(entry.Name) || !ValidWave(entry)) throw new InvalidDataException();
        }
        return entries;
    }

    private static bool ValidWave(ZipArchiveEntry entry)
    {
        if (entry.Length < VoiceSampleConstants.HeaderBytes) return false;
        Span<byte> header = stackalloc byte[VoiceSampleConstants.HeaderBytes];
        using var stream = entry.Open();
        stream.ReadExactly(header);
        var format = BinaryPrimitives.ReadUInt16LittleEndian(header[VoiceSampleConstants.FormatTagOffset..]);
        var bits = BinaryPrimitives.ReadUInt16LittleEndian(header[VoiceSampleConstants.BitsOffset..]);
        var channels = BinaryPrimitives.ReadUInt16LittleEndian(header[VoiceSampleConstants.ChannelsOffset..]);
        var dataSize = BinaryPrimitives.ReadUInt32LittleEndian(header[VoiceSampleConstants.DataSizeOffset..]);
        return header.StartsWith(VoiceSampleConstants.RiffMagic)
            && header[VoiceSampleConstants.WaveOffset..].StartsWith(VoiceSampleConstants.WaveMagic)
            && header[VoiceSampleConstants.FormatOffset..].StartsWith(VoiceSampleConstants.FormatMagic)
            && BinaryPrimitives.ReadUInt32LittleEndian(header[VoiceSampleConstants.FormatSizeOffset..])
                == VoiceSampleConstants.PlainFormatBytes
            && header[VoiceSampleConstants.DataOffset..].StartsWith(VoiceSampleConstants.DataMagic)
            && channels is 1 or 2
            && BinaryPrimitives.ReadUInt32LittleEndian(header[VoiceSampleConstants.SampleRateOffset..]) > 0
            && (format == VoiceSampleConstants.IntegerPcm
                ? bits is VoiceSampleConstants.BytePcmBits or VoiceSampleConstants.WordPcmBits
                : format == VoiceSampleConstants.FloatPcm && bits == VoiceSampleConstants.FloatPcmBits)
            && dataSize > 0 && dataSize <= entry.Length - VoiceSampleConstants.HeaderBytes
            && dataSize % (channels * (bits / VoiceSampleConstants.BytePcmBits)) == 0;
    }
}
