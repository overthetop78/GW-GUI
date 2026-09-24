using System.Buffers.Binary;
using System.Collections.Frozen;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Enums;

namespace GWGUI.MediaEngine.Images.Formats.HardDisk.Vdi;

/// <summary>Declares the autonomous VDI layouts supported by MediaEngine.</summary>
internal static class VdiFormat
{
    public static readonly ReadOnlyMemory<byte> Signature = CreateSignature();
    public static readonly IReadOnlySet<uint> Versions = new[] { HardDiskFormatConstants.VdiVersion1_1 }.ToFrozenSet();
    public static readonly IReadOnlySet<string> Extensions =
        new[] { DiskImageFileExtensions.Vdi }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    public static readonly IReadOnlySet<HardDiskImageVariant> Variants = new[]
    {
        HardDiskImageVariant.Fixed,
        HardDiskImageVariant.Dynamic
    }.ToFrozenSet();

    public static string FormatId => HardDiskImageFormatIds.Vdi;
    public static bool CanRead => true;
    public static bool CanWrite => true;

    private static byte[] CreateSignature()
    {
        var signature = new byte[4];
        BinaryPrimitives.WriteUInt32LittleEndian(signature, HardDiskFormatConstants.VdiSignature);
        return signature;
    }
}
