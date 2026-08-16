using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;

namespace GWGUI.Emulation.Atari.Cores;

internal static class AtariCoreDiagnosticFunctions
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
                AtariCoreReleaseConstants.WindowsX64Machine => AtariCoreCatalogConstants.WindowsX64Architecture,
                AtariCoreReleaseConstants.WindowsX86Machine => AtariCoreReleaseConstants.WindowsX86Architecture,
                AtariCoreReleaseConstants.WindowsArm64Machine => AtariCoreReleaseConstants.WindowsArm64Architecture,
                _ => AtariCoreReleaseConstants.UnknownDiagnosticValue
            };
        }
        catch (BadImageFormatException)
        {
            return AtariCoreReleaseConstants.UnknownDiagnosticValue;
        }
        catch (IOException)
        {
            return AtariCoreReleaseConstants.UnknownDiagnosticValue;
        }
    }

    internal static string ReadDeclaredVersion(string path)
    {
        try
        {
            return FileVersionInfo.GetVersionInfo(path).FileVersion
                ?? AtariCoreReleaseConstants.UnknownDiagnosticValue;
        }
        catch (FileNotFoundException)
        {
            return AtariCoreReleaseConstants.UnknownDiagnosticValue;
        }
    }

    internal static IReadOnlyList<string> ReadExports(string path)
    {
        try
        {
            using var stream = File.OpenRead(path);
            using var pe = new PEReader(stream, PEStreamOptions.LeaveOpen);
            var directory = pe.PEHeaders.PEHeader?.ExportTableDirectory ?? default;
            if (directory.RelativeVirtualAddress == AtariConstants.FirstBufferIndex
                || directory.Size < AtariCoreReleaseConstants.PeExportDirectoryMinimumSize) return [];
            var directoryOffset = RvaToFileOffset(pe.PEHeaders, directory.RelativeVirtualAddress);
            using var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, leaveOpen: true);
            stream.Position = directoryOffset + AtariCoreReleaseConstants.PeExportNumberOfNamesOffset;
            var count = reader.ReadUInt32();
            stream.Position = directoryOffset + AtariCoreReleaseConstants.PeExportAddressOfNamesOffset;
            var namesRva = reader.ReadUInt32();
            var namesOffset = RvaToFileOffset(pe.PEHeaders, checked((int)namesRva));
            var exports = new List<string>(checked((int)count));
            for (var index = AtariConstants.FirstBufferIndex; index < count; index++)
            {
                stream.Position = namesOffset + index * AtariCoreReleaseConstants.ExportNameRvaSize;
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
        throw new InvalidDataException(AtariCoreReleaseErrors.InvalidExportDirectory);
    }

    private static string ReadNullTerminatedString(BinaryReader reader)
    {
        var bytes = new List<byte>();
        while (bytes.Count < AtariCoreReleaseConstants.MaximumExportNameLength)
        {
            var value = reader.ReadByte();
            if (value == AtariConstants.FirstBufferIndex) break;
            bytes.Add(value);
        }
        return System.Text.Encoding.ASCII.GetString(bytes.ToArray());
    }
}
