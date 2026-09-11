using System.Buffers.Binary;
using System.Collections.Frozen;
using GWGUI.MediaEngine.Constants;

namespace GWGUI.MediaEngine.Formats.HardDisk.Qcow2;

/// <summary>Declares the autonomous QCOW2 profiles supported by MediaEngine.</summary>
internal static class Qcow2Format
{
    public static readonly ReadOnlyMemory<byte> Signature = CreateSignature();
    public static readonly IReadOnlySet<uint> Versions = new[]
    {
        HardDiskFormatConstants.Qcow2Version2,
        HardDiskFormatConstants.Qcow2Version3
    }.ToFrozenSet();
    public static readonly IReadOnlySet<string> Extensions = new[]
    {
        DiskImageFileExtensions.Qcow2,
        DiskImageFileExtensions.Qcow
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public static string FormatId => HardDiskImageFormatIds.Qcow2;
    public static bool CanRead => true;
    public static bool CanWrite => true;

    private static byte[] CreateSignature()
    {
        var bytes = new byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(bytes, HardDiskFormatConstants.Qcow2Magic);
        return bytes;
    }
}
