using MediaVolumeDescriptor = global::GWGUI.MediaFileSystems.Contracts.MediaVolumeDescriptor;
using System.Buffers.Binary;
using System.Collections.Frozen;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.FileSystems.Definitions;
using GWGUI.MediaEngine.Interfaces.Exploration;
using GWGUI.MediaEngine.Images.Reading.Optical;
using GWGUI.MediaEngine.Images.Models.Optical;

namespace GWGUI.MediaEngine.FileSystems.Udf;

/// <summary>Reads directly allocated UDF volumes carried by an optical data track.</summary>
public sealed class UdfFileSystemReader : IMediaFileSystemReader
{
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentations =
        new[] { MediaRepresentationKind.OpticalTracks }.ToFrozenSet();
    private readonly OpticalSectorReader sectors = new();

    public string Id => FileSystemIds.Udf;

    public IReadOnlySet<MediaRepresentationKind> RepresentationKinds => SupportedRepresentations;

    public bool CanRead(MediaImageDocument document, MediaVolumeDescriptor volume)
    {
        try
        {
            _ = ReadVolumeContext(document, volume);
            return true;
        }
        catch (Exception exception) when (exception is InvalidDataException or NotSupportedException
               or ArgumentOutOfRangeException or OverflowException)
        {
            return false;
        }
    }

    public FileSystemVolume Read(MediaImageDocument document, MediaVolumeDescriptor volume)
    {
        var context = ReadVolumeContext(document, volume);
        var visited = new HashSet<uint>();
        var root = ReadIcb(context, context.RootDirectoryBlock);
        if (root.FileType != UdfConstants.DirectoryFileType)
            throw new InvalidDataException("The UDF root ICB is not a directory.");
        var entries = ReadDirectory(context, root, visited);
        return new FileSystemVolume(
            context.VolumeName,
            Id,
            checked((long)context.PartitionLength * UdfConstants.LogicalBlockSize),
            0,
            null,
            null,
            entries,
            [],
            freeSpaceKnown: false,
            attributes: [$"UDF {FormatRevision(context.Revision)}"]);
    }

    private VolumeContext ReadVolumeContext(MediaImageDocument document, MediaVolumeDescriptor volume)
    {
        var track = ResolveTrack(document, volume);
        ValidateVolumeRecognitionSequence(track);
        var anchorLocation = FindAnchor(track);
        var anchor = ReadPhysicalBlock(track, anchorLocation);
        UdfDescriptorValidator.Validate(anchor, UdfConstants.AnchorDescriptorTagId, anchorLocation);
        var sequenceLength = ReadExtentLength(anchor, UdfConstants.AnchorMainVolumeDescriptorSequenceExtentOffset);
        var sequenceLocation = BinaryPrimitives.ReadUInt32LittleEndian(
            anchor.AsSpan(UdfConstants.AnchorMainVolumeDescriptorSequenceExtentOffset + UdfConstants.ExtentLocationOffset));
        if (sequenceLength == 0 || sequenceLength % UdfConstants.LogicalBlockSize != 0)
            throw new InvalidDataException("The UDF main volume descriptor sequence extent is invalid.");

        byte[]? logicalVolume = null;
        var partitions = new Dictionary<ushort, (uint Start, uint Length)>();
        var descriptorCount = sequenceLength / UdfConstants.LogicalBlockSize;
        for (uint index = 0; index < descriptorCount; index++)
        {
            var location = checked(sequenceLocation + index);
            var descriptor = ReadPhysicalBlock(track, location);
            var tagId = BinaryPrimitives.ReadUInt16LittleEndian(descriptor);
            if (tagId == UdfConstants.TerminatingDescriptorTagId)
            {
                UdfDescriptorValidator.Validate(descriptor, tagId, location);
                break;
            }
            if (tagId == UdfConstants.PartitionDescriptorTagId)
            {
                UdfDescriptorValidator.Validate(descriptor, tagId, location);
                var number = BinaryPrimitives.ReadUInt16LittleEndian(descriptor.AsSpan(UdfConstants.PartitionNumberOffset));
                var start = BinaryPrimitives.ReadUInt32LittleEndian(descriptor.AsSpan(UdfConstants.PartitionStartingLocationOffset));
                var length = BinaryPrimitives.ReadUInt32LittleEndian(descriptor.AsSpan(UdfConstants.PartitionLengthOffset));
                partitions[number] = (start, length);
            }
            else if (tagId == UdfConstants.LogicalVolumeDescriptorTagId)
            {
                UdfDescriptorValidator.Validate(descriptor, tagId, location);
                logicalVolume = descriptor;
            }
        }

        if (logicalVolume is null) throw new InvalidDataException("The UDF logical volume descriptor is missing.");
        var blockSize = BinaryPrimitives.ReadUInt32LittleEndian(logicalVolume.AsSpan(UdfConstants.LogicalVolumeBlockSizeOffset));
        if (blockSize != UdfConstants.LogicalBlockSize)
            throw new NotSupportedException($"Unsupported UDF logical block size {blockSize}.");
        var revision = BinaryPrimitives.ReadUInt16LittleEndian(logicalVolume.AsSpan(
            UdfConstants.LogicalVolumeDomainIdentifierOffset + UdfConstants.EntityIdentifierSuffixOffset));
        if (!UdfConstants.SupportedRevisions.Contains(revision))
            throw new NotSupportedException($"Unsupported UDF revision {FormatRevision(revision)}.");

        var mapTableLength = BinaryPrimitives.ReadUInt32LittleEndian(logicalVolume.AsSpan(UdfConstants.LogicalVolumeMapTableLengthOffset));
        var mapCount = BinaryPrimitives.ReadUInt32LittleEndian(logicalVolume.AsSpan(UdfConstants.LogicalVolumePartitionMapCountOffset));
        if (mapCount != 1 || mapTableLength != UdfConstants.TypeOnePartitionMapLength)
            throw new NotSupportedException("Only one directly mapped UDF physical partition is supported.");
        var map = logicalVolume.AsSpan(UdfConstants.LogicalVolumePartitionMapsOffset, UdfConstants.TypeOnePartitionMapLength);
        if (map[0] != UdfConstants.TypeOnePartitionMap || map[1] != UdfConstants.TypeOnePartitionMapLength)
            throw new NotSupportedException("Virtual, sparable, and metadata UDF partition maps are not supported.");
        var partitionNumber = BinaryPrimitives.ReadUInt16LittleEndian(map[UdfConstants.TypeOnePartitionMapPartitionNumberOffset..]);
        if (!partitions.TryGetValue(partitionNumber, out var partition) || partition.Length == 0)
            throw new InvalidDataException("The UDF physical partition descriptor is missing.");

        var fileSetIcb = ReadLongAllocationDescriptor(logicalVolume, UdfConstants.LogicalVolumeContentsUseOffset);
        EnsurePrimaryPartitionReference(fileSetIcb.PartitionReference);
        var fileSet = ReadPartitionBlock(track, partition.Start, partition.Length, fileSetIcb.Block);
        UdfDescriptorValidator.Validate(fileSet, UdfConstants.FileSetDescriptorTagId, fileSetIcb.Block);
        var rootIcb = ReadLongAllocationDescriptor(fileSet, UdfConstants.FileSetRootDirectoryIcbOffset);
        EnsurePrimaryPartitionReference(rootIcb.PartitionReference);
        var volumeName = UdfNameDecoder.DecodeDString(logicalVolume.AsSpan(
            UdfConstants.LogicalVolumeIdentifierOffset,
            UdfConstants.LogicalVolumeIdentifierLength));
        return new VolumeContext(track, partition.Start, partition.Length, revision, volumeName, rootIcb.Block);
    }

    private IReadOnlyList<FileSystemEntry> ReadDirectory(VolumeContext context, IcbData directory, HashSet<uint> visited)
    {
        if (!visited.Add(directory.Block))
            throw new InvalidDataException($"The UDF directory ICB {directory.Block} is referenced recursively.");
        var data = ReadIcbData(context, directory);
        var entries = new List<FileSystemEntry>();
        var offset = 0;
        while (offset <= data.Length - UdfConstants.DescriptorTagSize)
        {
            var tagId = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(offset));
            if (tagId == 0)
            {
                offset = checked((offset / UdfConstants.LogicalBlockSize + 1) * UdfConstants.LogicalBlockSize);
                continue;
            }
            if (tagId != UdfConstants.FileIdentifierDescriptorTagId)
                throw new InvalidDataException($"Unexpected UDF directory descriptor tag {tagId}.");
            var remaining = data.AsSpan(offset);
            UdfDescriptorValidator.Validate(remaining, UdfConstants.FileIdentifierDescriptorTagId, null);
            var characteristics = remaining[UdfConstants.FileIdentifierCharacteristicsOffset];
            var nameLength = remaining[UdfConstants.FileIdentifierLengthOffset];
            var implementationLength = BinaryPrimitives.ReadUInt16LittleEndian(
                remaining[UdfConstants.FileIdentifierImplementationUseLengthOffset..]);
            var recordLength = AlignFour(checked(UdfConstants.FileIdentifierImplementationUseOffset + implementationLength + nameLength));
            if (recordLength > remaining.Length)
                throw new InvalidDataException("A UDF file identifier exceeds its directory data.");
            if ((characteristics & (UdfConstants.FileIdentifierDeletedFlag | UdfConstants.FileIdentifierParentFlag)) == 0)
            {
                var nameOffset = UdfConstants.FileIdentifierImplementationUseOffset + implementationLength;
                var name = UdfNameDecoder.DecodeCompressedUnicode(remaining.Slice(nameOffset, nameLength));
                var childAddress = ReadLongAllocationDescriptor(remaining, UdfConstants.FileIdentifierIcbOffset);
                EnsurePrimaryPartitionReference(childAddress.PartitionReference);
                var child = ReadIcb(context, childAddress.Block);
                var isDirectory = child.FileType == UdfConstants.DirectoryFileType
                    || (characteristics & UdfConstants.FileIdentifierDirectoryFlag) != 0;
                var children = isDirectory ? ReadDirectory(context, child, visited) : [];
                IReadOnlyList<byte>? content = null;
                if (!isDirectory && child.InformationLength <= int.MaxValue)
                    content = ReadIcbData(context, child);
                entries.Add(new FileSystemEntry(
                    name,
                    isDirectory ? FileSystemEntryKind.Directory : FileSystemEntryKind.File,
                    child.InformationLength,
                    child.Modified,
                    string.Empty,
                    characteristics,
                    checked((int)Math.Min(child.Block, int.MaxValue)),
                    true,
                    children,
                    content,
                    nativeTypeId: child.FileType.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    occupiedSize: child.OccupiedLength,
                    created: child.Created,
                    accessed: child.Accessed,
                    dataValid: true));
            }
            offset += recordLength;
        }
        visited.Remove(directory.Block);
        return entries;
    }

    private IcbData ReadIcb(VolumeContext context, uint block)
    {
        var descriptor = ReadPartitionBlock(context.Track, context.PartitionStart, context.PartitionLength, block);
        var tagId = BinaryPrimitives.ReadUInt16LittleEndian(descriptor);
        if (tagId is not (UdfConstants.FileEntryDescriptorTagId or UdfConstants.ExtendedFileEntryDescriptorTagId))
            throw new InvalidDataException($"UDF ICB {block} is not a file entry.");
        UdfDescriptorValidator.Validate(descriptor, tagId, block);
        var extended = tagId == UdfConstants.ExtendedFileEntryDescriptorTagId;
        var informationLengthOffset = extended
            ? UdfConstants.ExtendedFileEntryInformationLengthOffset
            : UdfConstants.FileEntryInformationLengthOffset;
        var extendedAttributesLengthOffset = extended
            ? UdfConstants.ExtendedFileEntryExtendedAttributesLengthOffset
            : UdfConstants.FileEntryExtendedAttributesLengthOffset;
        var allocationDescriptorsLengthOffset = extended
            ? UdfConstants.ExtendedFileEntryAllocationDescriptorsLengthOffset
            : UdfConstants.FileEntryAllocationDescriptorsLengthOffset;
        var variableDataOffset = extended
            ? UdfConstants.ExtendedFileEntryVariableDataOffset
            : UdfConstants.FileEntryVariableDataOffset;
        var extendedAttributesLength = BinaryPrimitives.ReadUInt32LittleEndian(descriptor.AsSpan(extendedAttributesLengthOffset));
        var allocationDescriptorsLength = BinaryPrimitives.ReadUInt32LittleEndian(descriptor.AsSpan(allocationDescriptorsLengthOffset));
        var descriptorsOffset = checked(variableDataOffset + (int)extendedAttributesLength);
        if (allocationDescriptorsLength > int.MaxValue || descriptorsOffset > descriptor.Length - (int)allocationDescriptorsLength)
            throw new InvalidDataException("The UDF allocation descriptors exceed their file entry.");
        var flags = BinaryPrimitives.ReadUInt16LittleEndian(
            descriptor.AsSpan(UdfConstants.IcbTagOffset + UdfConstants.IcbFlagsOffset));
        var informationLength = BinaryPrimitives.ReadUInt64LittleEndian(descriptor.AsSpan(informationLengthOffset));
        if (informationLength > long.MaxValue)
            throw new NotSupportedException("The UDF entry length exceeds the supported signed range.");
        return new IcbData(
            block,
            descriptor[UdfConstants.IcbTagOffset + UdfConstants.IcbFileTypeOffset],
            (long)informationLength,
            (ushort)(flags & UdfConstants.IcbAllocationDescriptorTypeMask),
            descriptor.AsSpan(descriptorsOffset, (int)allocationDescriptorsLength).ToArray(),
            DecodeTimestamp(descriptor, extended ? UdfConstants.ExtendedFileEntryAccessTimeOffset : UdfConstants.FileEntryAccessTimeOffset),
            DecodeTimestamp(descriptor, extended ? UdfConstants.ExtendedFileEntryModificationTimeOffset : UdfConstants.FileEntryModificationTimeOffset),
            extended ? DecodeTimestamp(descriptor, UdfConstants.ExtendedFileEntryCreationTimeOffset) : null);
    }

    private byte[] ReadIcbData(VolumeContext context, IcbData icb)
    {
        if (icb.InformationLength > int.MaxValue)
            throw new NotSupportedException("The UDF entry is too large to expose as one in-memory item.");
        var result = new byte[(int)icb.InformationLength];
        if (icb.AllocationDescriptorType == UdfConstants.EmbeddedAllocationDescriptors)
        {
            if (result.Length > icb.AllocationDescriptors.Length)
                throw new InvalidDataException("The embedded UDF data exceeds its file entry.");
            icb.AllocationDescriptors.AsSpan(0, result.Length).CopyTo(result);
            return result;
        }
        if (icb.AllocationDescriptorType == UdfConstants.ExtendedAllocationDescriptors)
            throw new NotSupportedException("Extended UDF allocation descriptors are not supported.");

        var descriptorSize = icb.AllocationDescriptorType == UdfConstants.ShortAllocationDescriptors
            ? UdfConstants.ShortAllocationDescriptorSize
            : UdfConstants.LongAllocationDescriptorSize;
        if (icb.AllocationDescriptorType is not (UdfConstants.ShortAllocationDescriptors or UdfConstants.LongAllocationDescriptors)
            || icb.AllocationDescriptors.Length % descriptorSize != 0)
            throw new InvalidDataException("The UDF allocation descriptor list is invalid.");
        var completed = 0;
        for (var offset = 0; offset < icb.AllocationDescriptors.Length && completed < result.Length; offset += descriptorSize)
        {
            var descriptor = icb.AllocationDescriptors.AsSpan(offset, descriptorSize);
            var rawLength = BinaryPrimitives.ReadUInt32LittleEndian(descriptor);
            var extentType = rawLength >> UdfConstants.AllocationDescriptorTypeShift;
            var extentLength = (int)(rawLength & UdfConstants.AllocationDescriptorLengthMask);
            var block = BinaryPrimitives.ReadUInt32LittleEndian(descriptor[UdfConstants.ShortAllocationDescriptorBlockOffset..]);
            if (descriptorSize == UdfConstants.LongAllocationDescriptorSize)
            {
                var partitionReference = BinaryPrimitives.ReadUInt16LittleEndian(
                    descriptor[UdfConstants.LongAllocationDescriptorPartitionOffset..]);
                EnsurePrimaryPartitionReference(partitionReference);
            }
            var count = Math.Min(extentLength, result.Length - completed);
            if (extentType == 0)
                ReadPartitionBytes(context, block, count).CopyTo(result, completed);
            else if (extentType == 3)
                throw new NotSupportedException("UDF continuation allocation extents are not supported.");
            completed += count;
        }
        if (completed != result.Length)
            throw new InvalidDataException("The UDF allocation descriptors do not cover the entry length.");
        return result;
    }

    private byte[] ReadPartitionBytes(VolumeContext context, uint firstBlock, int length)
    {
        var result = new byte[length];
        var completed = 0;
        var block = firstBlock;
        while (completed < length)
        {
            var source = ReadPartitionBlock(context.Track, context.PartitionStart, context.PartitionLength, block++);
            var count = Math.Min(source.Length, length - completed);
            source.AsSpan(0, count).CopyTo(result.AsSpan(completed));
            completed += count;
        }
        return result;
    }

    private void ValidateVolumeRecognitionSequence(OpticalTrackDescriptor track)
    {
        var foundNsr = false;
        for (var index = 0; index < UdfConstants.MaximumVolumeRecognitionSectors; index++)
        {
            var block = checked((uint)(UdfConstants.FirstVolumeRecognitionSector + index));
            if (block >= track.SectorCount) break;
            var descriptor = ReadPhysicalBlock(track, block);
            var identifier = System.Text.Encoding.ASCII.GetString(
                descriptor,
                UdfConstants.VolumeStructureIdentifierOffset,
                UdfConstants.VolumeStructureIdentifierLength);
            if (identifier is UdfConstants.Nsr02Identifier or UdfConstants.Nsr03Identifier) foundNsr = true;
            if (identifier == UdfConstants.TerminatingExtendedAreaIdentifier) break;
        }
        if (!foundNsr) throw new InvalidDataException("The UDF NSR volume structure descriptor is missing.");
    }

    private uint FindAnchor(OpticalTrackDescriptor track)
    {
        var candidates = new List<long> { UdfConstants.AnchorSector };
        if (track.SectorCount > 0) candidates.Add(track.SectorCount - 1);
        if (track.SectorCount > UdfConstants.AnchorSector) candidates.Add(track.SectorCount - UdfConstants.AnchorSector - 1);
        foreach (var candidate in candidates.Distinct())
        {
            if (candidate < 0 || candidate >= track.SectorCount || candidate > uint.MaxValue) continue;
            try
            {
                var descriptor = ReadPhysicalBlock(track, (uint)candidate);
                UdfDescriptorValidator.Validate(descriptor, UdfConstants.AnchorDescriptorTagId, (uint)candidate);
                return (uint)candidate;
            }
            catch (InvalidDataException)
            {
            }
        }
        throw new InvalidDataException("No valid UDF anchor volume descriptor pointer was found.");
    }

    private byte[] ReadPhysicalBlock(OpticalTrackDescriptor track, uint block)
    {
        var data = sectors.ReadUserDataAsync(track, block).AsTask().GetAwaiter().GetResult();
        if (data.Length != UdfConstants.LogicalBlockSize)
            throw new NotSupportedException($"Unsupported UDF physical sector size {data.Length}.");
        return data;
    }

    private byte[] ReadPartitionBlock(OpticalTrackDescriptor track, uint partitionStart, uint partitionLength, uint block)
    {
        if (block >= partitionLength) throw new InvalidDataException("A UDF block lies outside its partition.");
        return ReadPhysicalBlock(track, checked(partitionStart + block));
    }

    private static OpticalTrackDescriptor ResolveTrack(MediaImageDocument document, MediaVolumeDescriptor volume)
    {
        if (document.Representation is not OpticalMediaImageRepresentation optical || optical.Tracks is null)
            throw new NotSupportedException("UDF requires optical track data.");
        return optical.Tracks.FirstOrDefault(track =>
                !track.IsAudio
                && (volume.SessionNumber is null || track.SessionNumber == volume.SessionNumber)
                && (volume.TrackNumber is null || track.TrackNumber == volume.TrackNumber))
            ?? throw new InvalidDataException("No data track is available for the requested UDF volume.");
    }

    private static uint ReadExtentLength(ReadOnlySpan<byte> source, int offset) =>
        BinaryPrimitives.ReadUInt32LittleEndian(source[(offset + UdfConstants.ExtentLengthOffset)..])
        & UdfConstants.AllocationDescriptorLengthMask;

    private static (uint Block, ushort PartitionReference) ReadLongAllocationDescriptor(ReadOnlySpan<byte> source, int offset) =>
        (BinaryPrimitives.ReadUInt32LittleEndian(source[(offset + UdfConstants.LongAllocationDescriptorBlockOffset)..]),
         BinaryPrimitives.ReadUInt16LittleEndian(source[(offset + UdfConstants.LongAllocationDescriptorPartitionOffset)..]));

    private static void EnsurePrimaryPartitionReference(ushort partitionReference)
    {
        if (partitionReference != 0)
            throw new NotSupportedException("Only the first directly mapped UDF partition is supported.");
    }

    private static int AlignFour(int value) => checked((value + 3) & ~3);

    private static DateTimeOffset? DecodeTimestamp(ReadOnlySpan<byte> source, int offset)
    {
        if (offset > source.Length - UdfConstants.TimestampSize) return null;
        var value = source.Slice(offset, UdfConstants.TimestampSize);
        var year = BinaryPrimitives.ReadUInt16LittleEndian(value[UdfConstants.TimestampYearOffset..]);
        if (year == 0 || value[UdfConstants.TimestampMonthOffset] == 0 || value[UdfConstants.TimestampDayOffset] == 0)
            return null;
        var typeAndTimezone = BinaryPrimitives.ReadUInt16LittleEndian(value);
        var encodedTimezone = (ushort)(typeAndTimezone & UdfConstants.TimestampTimezoneMask);
        var timezoneMinutes = encodedTimezone == UdfConstants.TimestampUnspecifiedTimezone
            ? 0
            : encodedTimezone >= 0x0800 ? encodedTimezone - 0x1000 : encodedTimezone;
        try
        {
            var offsetValue = TimeSpan.FromMinutes(timezoneMinutes);
            var timestamp = new DateTimeOffset(
                year,
                value[UdfConstants.TimestampMonthOffset],
                value[UdfConstants.TimestampDayOffset],
                value[UdfConstants.TimestampHourOffset],
                value[UdfConstants.TimestampMinuteOffset],
                value[UdfConstants.TimestampSecondOffset],
                offsetValue);
            var ticks = value[UdfConstants.TimestampCentisecondsOffset] * TimeSpan.TicksPerMillisecond * 10L
                + value[UdfConstants.TimestampHundredsOfMicrosecondsOffset] * TimeSpan.TicksPerMillisecond / 10L
                + value[UdfConstants.TimestampMicrosecondsOffset] * 10L;
            return timestamp.AddTicks(ticks);
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }

    private static string FormatRevision(ushort revision) =>
        $"{(revision >> 8):X}.{(revision & 0xFF):X2}";

    private sealed record VolumeContext(
        OpticalTrackDescriptor Track,
        uint PartitionStart,
        uint PartitionLength,
        ushort Revision,
        string VolumeName,
        uint RootDirectoryBlock);

    private sealed record IcbData(
        uint Block,
        byte FileType,
        long InformationLength,
        ushort AllocationDescriptorType,
        byte[] AllocationDescriptors,
        DateTimeOffset? Accessed,
        DateTimeOffset? Modified,
        DateTimeOffset? Created)
    {
        public long OccupiedLength => AllocationDescriptorType == UdfConstants.EmbeddedAllocationDescriptors
            ? AllocationDescriptors.Length
            : checked((long)InformationLength);
    }
}
