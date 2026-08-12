using System.Buffers.Binary;

namespace GWGUI.MediaEngine.FileSystems.Apple.InformXzip;

/// <summary>Représente les champs validés d'une histoire Z-machine version 5.</summary>
public sealed record ZMachineV5Header(byte Version, ushort HighMemory, ushort InitialProgramCounter, ushort Dictionary, ushort Objects, ushort Globals, ushort StaticMemory, int Length, ushort Checksum)
{
    /// <summary>Tente de lire et valider l'en-tête d'une histoire.</summary>
    public static bool TryParse(ReadOnlySpan<byte> story, out ZMachineV5Header? header)
    {
        if (!TryParseHeader(story, out header) || header is null || header.Length > story.Length) return false;
        return true;
    }

    /// <summary>Parse l'en-tête présent dans le premier secteur sans exiger le reste de l'histoire.</summary>
    public static bool TryParseHeader(ReadOnlySpan<byte> story, out ZMachineV5Header? header)
    {
        header = null;
        if (story.Length < AppleInformXzipLayout.MinimumHeaderLength || story[AppleInformXzipLayout.VersionOffset] != AppleInformXzipLayout.ZMachineVersion) return false;
        var length = BinaryPrimitives.ReadUInt16BigEndian(story.Slice(AppleInformXzipLayout.LengthOffset, sizeof(ushort))) * AppleInformXzipLayout.LengthUnit;
        if (length is < AppleInformXzipLayout.MinimumHeaderLength or > AppleInformXzipLayout.MaximumStorySectorCount * AppleInformXzipLayout.SectorSize) return false;
        var highMemory = Read(story, AppleInformXzipLayout.HighMemoryOffset);
        var initialPc = Read(story, AppleInformXzipLayout.InitialProgramCounterOffset);
        var dictionary = Read(story, AppleInformXzipLayout.DictionaryOffset);
        var objects = Read(story, AppleInformXzipLayout.ObjectsOffset);
        var globals = Read(story, AppleInformXzipLayout.GlobalsOffset);
        var staticMemory = Read(story, AppleInformXzipLayout.StaticMemoryOffset);
        if (highMemory < AppleInformXzipLayout.MinimumHeaderLength || initialPc < highMemory || initialPc >= length || dictionary < AppleInformXzipLayout.MinimumHeaderLength || dictionary >= length || objects < AppleInformXzipLayout.MinimumHeaderLength || objects >= length || globals < AppleInformXzipLayout.MinimumHeaderLength || globals >= length || staticMemory < AppleInformXzipLayout.MinimumHeaderLength || staticMemory >= length) return false;
        header = new(story[AppleInformXzipLayout.VersionOffset], highMemory, initialPc, dictionary, objects, globals, staticMemory, length, Read(story, AppleInformXzipLayout.ChecksumOffset));
        return true;
    }

    /// <summary>Vérifie le checksum de l'histoire validée.</summary>
    public bool ChecksumMatches(ReadOnlySpan<byte> story)
    {
        var checksum = 0;
        for (var index = AppleInformXzipLayout.ChecksumDataOffset; index < Length; index++) checksum = (checksum + story[index]) & ushort.MaxValue;
        return checksum == Checksum;
    }

    /// <summary>Lit un entier 16 bits en ordre big-endian.</summary>
    private static ushort Read(ReadOnlySpan<byte> story, int offset) => BinaryPrimitives.ReadUInt16BigEndian(story.Slice(offset, sizeof(ushort)));
}
