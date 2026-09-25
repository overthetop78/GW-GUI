using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;

namespace GWGUI.Emulation.Atari.Common.Functions;

internal static class CoreDiagnosticFunctions
{
    internal static string CalculateSha256(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream));
    }

    internal static string ReadArchitecture(string path)
    {
        try
        {
            using var stream = File.OpenRead(path);
            using var reader = new PEReader(stream);
            return (ushort)reader.PEHeaders.CoffHeader.Machine switch
            {
                CoreReleaseConstants.WindowsX64Machine => EmulatorCatalogConstants.WindowsX64Architecture,
                CoreReleaseConstants.WindowsX86Machine => CoreReleaseConstants.WindowsX86Architecture,
                CoreReleaseConstants.WindowsArm64Machine => CoreReleaseConstants.WindowsArm64Architecture,
                _ => CoreReleaseConstants.UnknownDiagnosticValue
            };
        }
        catch (BadImageFormatException)
        {
            return CoreReleaseConstants.UnknownDiagnosticValue;
        }
        catch (IOException)
        {
            return CoreReleaseConstants.UnknownDiagnosticValue;
        }
    }

    internal static string ReadDeclaredVersion(string path)
    {
        try
        {
            return FileVersionInfo.GetVersionInfo(path).FileVersion
                ?? CoreReleaseConstants.UnknownDiagnosticValue;
        }
        catch (FileNotFoundException)
        {
            return CoreReleaseConstants.UnknownDiagnosticValue;
        }
    }

    internal static IReadOnlyList<string> ReadExports(string path)
    {
        try
        {
            using var stream = File.OpenRead(path);
            using var pe = new PEReader(stream, PEStreamOptions.LeaveOpen);
            var directory = pe.PEHeaders.PEHeader?.ExportTableDirectory ?? default;
            if (directory.RelativeVirtualAddress == CommonConstants.FirstBufferIndex
                || directory.Size < CoreReleaseConstants.PeExportDirectoryMinimumSize) return [];
            var directoryOffset = RvaToFileOffset(pe.PEHeaders, directory.RelativeVirtualAddress);
            using var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, leaveOpen: true);
            stream.Position = directoryOffset + CoreReleaseConstants.PeExportNumberOfNamesOffset;
            var count = reader.ReadUInt32();
            stream.Position = directoryOffset + CoreReleaseConstants.PeExportAddressOfNamesOffset;
            var namesRva = reader.ReadUInt32();
            var namesOffset = RvaToFileOffset(pe.PEHeaders, checked((int)namesRva));
            var exports = new List<string>(checked((int)count));
            for (var index = CommonConstants.FirstBufferIndex; index < count; index++)
            {
                stream.Position = namesOffset + index * CoreReleaseConstants.ExportNameRvaSize;
                var nameOffset = RvaToFileOffset(pe.PEHeaders, checked((int)reader.ReadUInt32()));
                stream.Position = nameOffset;
                exports.Add(ReadNullTerminatedString(reader));
            }
            return exports.Order(StringComparer.Ordinal).ToArray();
        }
        catch (BadImageFormatException)
        {
            return [];
        }
        catch (IOException)
        {
            return [];
        }
        catch (InvalidDataException)
        {
            return [];
        }
        catch (OverflowException)
        {
            return [];
        }
    }

    private static long RvaToFileOffset(PEHeaders headers, int rva)
    {
        foreach (var section in headers.SectionHeaders)
        {
            var size = Math.Max(section.VirtualSize, section.SizeOfRawData);
            if (rva >= section.VirtualAddress && rva < section.VirtualAddress + size)
                return section.PointerToRawData + rva - section.VirtualAddress;
        }
        throw new InvalidDataException(CoreReleaseErrors.InvalidExportDirectory);
    }

    private static string ReadNullTerminatedString(BinaryReader reader)
    {
        var bytes = new List<byte>();
        while (bytes.Count < CoreReleaseConstants.MaximumExportNameLength)
        {
            var value = reader.ReadByte();
            if (value == CommonConstants.FirstBufferIndex) break;
            bytes.Add(value);
        }
        return System.Text.Encoding.ASCII.GetString(bytes.ToArray());
    }
}
