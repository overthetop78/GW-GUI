using System.IO;
using GWGUI.MediaFileSystems.Constants;
using GWGUI.MediaFileSystems.Interfaces;
using GWGUI.MediaFileSystems.Definitions;

namespace GWGUI.MediaFileSystems.FileSystems.Iso9660;

/// <summary>Reads Rock Ridge alternate names stored in ISO 9660 SUSP system-use entries.</summary>
public sealed class RockRidgeExtensionReader : Iso9660FileSystemReader
{
    private static readonly System.Text.UTF8Encoding StrictUtf8 = new(false, true);

    public override string Id => FileSystemIds.RockRidge;

    protected override bool AcceptFileSystem(IMediaOpticalTrack track, ReadOnlySpan<byte> descriptor)
    {
        var directory = ReadRootDirectoryData(track, descriptor);
        var offset = 0;
        while (offset < directory.Length)
        {
            var recordLength = directory[offset];
            if (recordLength == 0)
            {
                offset = checked((offset / Iso9660Constants.LogicalBlockSize + 1) * Iso9660Constants.LogicalBlockSize);
                continue;
            }
            if (recordLength < Iso9660Constants.DirectoryRecordMinimumLength || offset > directory.Length - recordLength)
                return false;
            if (ContainsRockRidgeExtensionReference(directory.AsSpan(offset, recordLength))) return true;
            offset += recordLength;
        }
        return false;
    }

    protected override string ResolveEntryName(ReadOnlySpan<byte> record, string decodedName)
    {
        var identifierLength = record[Iso9660Constants.DirectoryFileIdentifierLengthOffset];
        var systemUseOffset = Iso9660Constants.DirectoryFileIdentifierOffset + identifierLength;
        if ((identifierLength & 1) == 0) systemUseOffset++;
        if (systemUseOffset >= record.Length) return decodedName;

        using var name = new MemoryStream();
        var foundName = false;
        var offset = systemUseOffset;
        while (offset <= record.Length - 4)
        {
            var entryLength = record[offset + Iso9660Constants.SuspLengthOffset];
            if (entryLength < 4 || offset > record.Length - entryLength) break;
            var entry = record.Slice(offset, entryLength);
            if (entry[Iso9660Constants.SuspVersionOffset] == Iso9660Constants.SuspVersion
                && entry[0] == Iso9660Constants.RockRidgeAlternateNameSignatureFirstByte
                && entry[1] == Iso9660Constants.RockRidgeAlternateNameSignatureSecondByte)
            {
                if (entryLength < Iso9660Constants.RockRidgeAlternateNameMinimumLength) break;
                var flags = entry[Iso9660Constants.RockRidgeAlternateNameFlagsOffset];
                if ((flags & Iso9660Constants.RockRidgeAlternateNameCurrentFlag) != 0) return ".";
                if ((flags & Iso9660Constants.RockRidgeAlternateNameParentFlag) != 0) return "..";
                name.Write(entry[Iso9660Constants.RockRidgeAlternateNameContentOffset..]);
                foundName = true;
                if ((flags & Iso9660Constants.RockRidgeAlternateNameContinueFlag) == 0) break;
            }
            offset += entryLength;
        }

        return foundName ? DecodeRockRidgeName(name.ToArray()) : decodedName;
    }

    private static string DecodeRockRidgeName(byte[] value)
    {
        try
        {
            return StrictUtf8.GetString(value);
        }
        catch (System.Text.DecoderFallbackException)
        {
            return System.Text.Encoding.Latin1.GetString(value);
        }
    }

    private static bool ContainsRockRidgeExtensionReference(ReadOnlySpan<byte> record)
    {
        var identifierLength = record[Iso9660Constants.DirectoryFileIdentifierLengthOffset];
        var offset = Iso9660Constants.DirectoryFileIdentifierOffset + identifierLength;
        if ((identifierLength & 1) == 0) offset++;
        while (offset <= record.Length - 4)
        {
            var entryLength = record[offset + Iso9660Constants.SuspLengthOffset];
            if (entryLength < 4 || offset > record.Length - entryLength) return false;
            var entry = record.Slice(offset, entryLength);
            if (entryLength >= Iso9660Constants.SuspExtensionReferenceMinimumLength
                && entry[Iso9660Constants.SuspVersionOffset] == Iso9660Constants.SuspVersion
                && entry[0] == Iso9660Constants.SuspExtensionReferenceSignatureFirstByte
                && entry[1] == Iso9660Constants.SuspExtensionReferenceSignatureSecondByte)
            {
                var identifierLengthValue = entry[Iso9660Constants.SuspExtensionIdentifierLengthOffset];
                if (identifierLengthValue <= entryLength - Iso9660Constants.SuspExtensionIdentifierOffset)
                {
                    var identifier = System.Text.Encoding.ASCII.GetString(
                        entry.Slice(Iso9660Constants.SuspExtensionIdentifierOffset, identifierLengthValue));
                    if (identifier is Iso9660Constants.RockRidgeExtensionIdentifier
                        or Iso9660Constants.RockRidgeIeeeExtensionIdentifier)
                        return true;
                }
            }
            offset += entryLength;
        }
        return false;
    }
}
