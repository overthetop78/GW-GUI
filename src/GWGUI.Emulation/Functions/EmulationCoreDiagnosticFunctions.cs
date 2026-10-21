using System.IO;
using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;

namespace GWGUI.Emulation.Functions;

/// <summary>Inspection et identité des bibliothèques de cœur pour tout module indépendant.</summary>
/// <remarks>Ces opérations inspectent les fichiers et métadonnées sans charger un cœur. Les flux créés sont libérés ; les fournisseurs de messages ne sont évalués que sur leurs branches d’erreur.</remarks>
internal sealed class EmulationCoreDiagnosticFunctions : IEmulationCoreDiagnostics
{
    /// <summary>Vérifie que l’architecture annoncée correspond à celle attendue.</summary>
    public void RequireArchitecture(string? actual, string expected, Func<string> error)
    {
        if (!string.Equals(actual, expected, StringComparison.Ordinal))
            throw new InvalidDataException(error());
    }

    /// <summary>Construit le manifeste diagnostique avec date, tailles, empreinte et métadonnées de la bibliothèque.</summary>
    public EmulationCoreDiagnosticManifest CreateManifest(string releaseId, string releaseVersion, string downloadUrl,
        long archiveSize, string libraryPath)
        => new EmulationCoreDiagnosticManifest(releaseId, releaseVersion, downloadUrl, DateTimeOffset.UtcNow, archiveSize,
            new FileInfo(libraryPath).Length, CalculateSha256(libraryPath), ReadArchitecture(libraryPath),
            ReadDeclaredVersion(libraryPath), ReadExports(libraryPath));

    /// <summary>Renvoie le chemin d’une installation complète et vérifiée, ou null lorsqu’elle manque ou reste invalide.</summary>
    public string? FindInstalledLibraryPath(string path, Action<string>? verifyInstallation,
        Func<bool>? isInstallationComplete = null)
    {
        if (!File.Exists(path) || isInstallationComplete?.Invoke() == false) return null;
        try
        {
            verifyInstallation?.Invoke(path);
            return path;
        }
        catch (IOException) { return null; }
        catch (InvalidDataException) { return null; }
    }

    /// <summary>Vérifie les signatures PE et la machine x64 avec les erreurs différées du module.</summary>
    public void VerifyWindowsX64Library(string path, Func<string> notPe,
        Func<string> invalidPe, Func<string> wrongArchitecture)
    {
        using var stream = File.OpenRead(path);
        using var reader = new BinaryReader(stream);
        if (stream.Length < 0x40 || reader.ReadUInt16() != 0x5A4D)
            throw new InvalidDataException(notPe());
        stream.Position = 0x3c;
        var peOffset = reader.ReadInt32();
        if (peOffset < 0x40 || peOffset > stream.Length - 6)
            throw new InvalidDataException(invalidPe());
        stream.Position = peOffset;
        if (reader.ReadUInt32() != 0x00004550 || reader.ReadUInt16() != 0x8664)
            throw new InvalidDataException(wrongArchitecture());
    }

    /// <summary>Calcule l’empreinte SHA-256 hexadécimale du fichier, avec libération du flux.</summary>
    public string CalculateSha256(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream));
    }

    /// <summary>Lit l’architecture PE ou renvoie unknown si le fichier ne peut pas être interprété.</summary>
    internal static string ReadArchitecture(string path)
    {
        try
        {
            using var stream = File.OpenRead(path);
            using var reader = new PEReader(stream);
            return (ushort)reader.PEHeaders.CoffHeader.Machine switch
            {
                0x8664 => "x64",
                0x014c => "x86",
                0xaa64 => "arm64",
                _ => "unknown"
            };
        }
        catch (BadImageFormatException)
        {
            return "unknown";
        }
        catch (IOException)
        {
            return "unknown";
        }
    }

    /// <summary>Lit la version déclarée dans le fichier ou la valeur unknown prévue.</summary>
    internal static string ReadDeclaredVersion(string path)
    {
        try
        {
            return FileVersionInfo.GetVersionInfo(path).FileVersion
                ?? "unknown";
        }
        catch (FileNotFoundException)
        {
            return "unknown";
        }
    }

    /// <summary>Lit et trie les noms exportés par une bibliothèque PE sans charger cette bibliothèque.</summary>
    internal static IReadOnlyList<string> ReadExports(string path)
    {
        try
        {
            using var stream = File.OpenRead(path);
            using var pe = new PEReader(stream, PEStreamOptions.LeaveOpen);
            var directory = pe.PEHeaders.PEHeader?.ExportTableDirectory ?? default;
            if (directory.RelativeVirtualAddress == 0
                || directory.Size < 40) return [];
            var directoryOffset = RvaToFileOffset(pe.PEHeaders, directory.RelativeVirtualAddress);
            using var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, leaveOpen: true);
            stream.Position = directoryOffset + 24;
            var count = reader.ReadUInt32();
            stream.Position = directoryOffset + 32;
            var namesRva = reader.ReadUInt32();
            var namesOffset = RvaToFileOffset(pe.PEHeaders, checked((int)namesRva));
            var exports = new List<string>(checked((int)count));
            for (var index = 0; index < count; index++)
            {
                stream.Position = namesOffset + index * sizeof(uint);
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
        throw new InvalidDataException("Invalid PE export directory.");
    }

    private static string ReadNullTerminatedString(BinaryReader reader)
    {
        var bytes = new List<byte>();
        while (bytes.Count < 4_096)
        {
            var value = reader.ReadByte();
            if (value == 0) break;
            bytes.Add(value);
        }
        return System.Text.Encoding.ASCII.GetString(bytes.ToArray());
    }
}
